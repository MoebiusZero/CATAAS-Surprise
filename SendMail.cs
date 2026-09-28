using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SendCATAASsurprises
{
    static class SendMail
    {
        private static readonly HttpClient http = new HttpClient();

        //Downloads the cat image, returns the image and its content type (e.g. image/jpeg)
        private static async Task<(byte[] Image, string ContentType)> GetImage(string url)
        {
            byte[] image;
            string contentType;

            try
            {
                using HttpResponseMessage response = await http.GetAsync(url);
                response.EnsureSuccessStatusCode();
                image = await response.Content.ReadAsByteArrayAsync();
                contentType = response.Content.Headers.ContentType?.MediaType;
            }
            catch (Exception exp)
            {
                throw new InvalidOperationException($"Could not download the cat image from CATAAS ({url}): {exp.Message}", exp);
            }

            if (image.Length == 0)
            {
                throw new InvalidOperationException($"CATAAS returned an empty image ({url}).");
            }

            if (contentType == null || !contentType.StartsWith("image/"))
            {
                contentType = MediaTypeNames.Image.Jpeg;
            }

            return (image, contentType);
        }

        //Gets an app-only access token for Microsoft Graph using the app registration's client secret
        private static async Task<string> GetAccessToken()
        {
            string tenantId = ConfigurationManager.AppSettings["TenantId"];
            string clientId = ConfigurationManager.AppSettings["ClientId"];

            //Prefer the environment variable so the secret doesn't have to live in App.config
            string clientSecret = Environment.GetEnvironmentVariable("CATAAS_CLIENT_SECRET");
            if (string.IsNullOrEmpty(clientSecret))
            {
                clientSecret = ConfigurationManager.AppSettings["ClientSecret"];
            }

            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                throw new InvalidOperationException("TenantId and ClientId must be set in App.config, and the client secret in the CATAAS_CLIENT_SECRET environment variable (or ClientSecret in App.config).");
            }

            FormUrlEncodedContent form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["scope"] = "https://graph.microsoft.com/.default",
                ["grant_type"] = "client_credentials"
            });

            using HttpResponseMessage response = await http.PostAsync($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token", form);
            string body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Could not get an access token from Microsoft 365 ({(int)response.StatusCode}): {body}");
            }

            using JsonDocument json = JsonDocument.Parse(body);
            return json.RootElement.GetProperty("access_token").GetString();
        }

        public static async Task Send(string email, string subject, string url)
        {
            var (image, contentType) = await GetImage(url);
            string fromEmail = ConfigurationManager.AppSettings["FromEmail"];

            var mail = new
            {
                message = new
                {
                    subject,
                    body = new
                    {
                        contentType = "HTML",
                        content = "<img src=\"cid:myPic\"> <br><br><h3>Met vriendelijke groet</h3><h4> El Tigre </h4><p style=\"font-size:8px\"> Zit een afbeelding bij, klik in Outlook de optie om afbeeldingen of geblokkeerde inhoud te weergeven</p>"
                    },
                    toRecipients = new[] { new { emailAddress = new { address = email } } },
                    //The image is sent as an inline attachment, the body refers to it with cid:myPic
                    attachments = new[]
                    {
                        new Dictionary<string, object>
                        {
                            ["@odata.type"] = "#microsoft.graph.fileAttachment",
                            ["name"] = "cat." + contentType.Substring("image/".Length),
                            ["contentType"] = contentType,
                            ["contentBytes"] = Convert.ToBase64String(image),
                            ["contentId"] = "myPic",
                            ["isInline"] = true
                        }
                    }
                },
                saveToSentItems = false
            };

            Console.WriteLine("Sending Email......");
            await PostSendMail(fromEmail, mail);
            Console.WriteLine("Email Sent.");
        }

        //Emails the details of an error to the address in the ErrorEmail setting
        public static async Task SendError(Exception error, string recipient, string type, string[] args)
        {
            string fromEmail = ConfigurationManager.AppSettings["FromEmail"];
            string errorEmail = ConfigurationManager.AppSettings["ErrorEmail"];

            if (string.IsNullOrEmpty(errorEmail))
            {
                throw new InvalidOperationException("ErrorEmail is not set in App.config.");
            }

            static string Encode(string value) => WebUtility.HtmlEncode(string.IsNullOrEmpty(value) ? "(empty)" : value);

            string content =
                "<h2>Error in CATAAS App</h2>" +
                "<table cellpadding=\"4\" style=\"border-collapse:collapse\">" +
                $"<tr><td><b>Time</b></td><td>{Encode(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss zzz"))}</td></tr>" +
                $"<tr><td><b>Computer</b></td><td>{Encode(Environment.MachineName)}</td></tr>" +
                $"<tr><td><b>Windows user</b></td><td>{Encode(Environment.UserDomainName + "\\" + Environment.UserName)}</td></tr>" +
                $"<tr><td><b>Recipient</b></td><td>{Encode(recipient)}</td></tr>" +
                $"<tr><td><b>Message type</b></td><td>{Encode(type)}</td></tr>" +
                $"<tr><td><b>Arguments</b></td><td>{Encode(args.Length == 0 ? "(none, started interactively)" : string.Join(" ", args))}</td></tr>" +
                $"<tr><td><b>Working directory</b></td><td>{Encode(Environment.CurrentDirectory)}</td></tr>" +
                "</table>" +
                "<h3>Error</h3>" +
                $"<p><b>{Encode(error.GetType().FullName)}</b>: {Encode(error.Message)}</p>" +
                "<h3>Details (including inner exceptions and stack trace)</h3>" +
                $"<pre style=\"font-size:12px\">{Encode(error.ToString())}</pre>";

            var mail = new
            {
                message = new
                {
                    subject = "Error in CATAAS App",
                    importance = "high",
                    body = new { contentType = "HTML", content },
                    toRecipients = new[] { new { emailAddress = new { address = errorEmail } } }
                },
                saveToSentItems = false
            };

            await PostSendMail(fromEmail, mail);
        }

        //Sends a message through Microsoft Graph as the given mailbox
        private static async Task PostSendMail(string fromEmail, object mail)
        {
            string token = await GetAccessToken();

            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://graph.microsoft.com/v1.0/users/{Uri.EscapeDataString(fromEmail)}/sendMail");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(JsonSerializer.Serialize(mail), Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Microsoft Graph could not send the email ({(int)response.StatusCode}): {error}");
            }
        }
    }
}
