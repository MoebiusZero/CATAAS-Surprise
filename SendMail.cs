using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace SendCATAASsurprises
{
    class SendMail
    {
        public String ConvertImageURLToBase64(String url)
        {
            StringBuilder _sb = new StringBuilder();
            Byte[] _byte = this.GetImage(url);
            _sb.Append(Convert.ToBase64String(_byte, 0, _byte.Length));
            return _sb.ToString();
        }

        public String ConvertImageURLToBase64Type(string url)
        {
            StringBuilder _sb = new StringBuilder();
            Byte[] _byte = this.GetImage(url);
            _sb.Append(Convert.ToBase64String(_byte, 0, _byte.Length));
            var base64 = _sb.ToString();

            var data = base64.Substring(0, 5);

            switch (data.ToUpper())
            {
                case "IVBOR":
                    return "png";
                case "/9J/4":
                    return "jpg";               
                default:
                    return string.Empty;
            }        
        }

        private byte[] GetImage(string url)
        {
            Stream stream = null;
            byte[] buf;

            try
            {
                WebProxy myProxy = new WebProxy();
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                stream = response.GetResponseStream();

                using (BinaryReader br = new BinaryReader(stream))
                {
                    int len = (int)(response.ContentLength);
                    buf = br.ReadBytes(len);
                    br.Close();
                }

                stream.Close();
                response.Close();
            }
            catch (Exception exp)
            {
                buf = null;
            }

            return (buf);
        }

    public void Send(string email, string subject, string url)
        {           
            var base64 = ConvertImageURLToBase64(url);
            string savefile = "tempimage.jpg";
            File.WriteAllBytes(savefile, Convert.FromBase64String(base64));

            using MailMessage mm = new MailMessage();
            mm.To.Add(new MailAddress(email));
            mm.From = new MailAddress(ConfigurationManager.AppSettings["FromEmail"], ConfigurationManager.AppSettings["FromEmailName"]);
            mm.Subject = subject;
            LinkedResource LinkedImage = new LinkedResource(savefile);
            LinkedImage.ContentId = "myPic";

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(
                "<img src=cid:myPic> <br><br><h3>Met vriendelijke groet</h3><h4> El Tigre </h4><p style = font-size:8px> Zit een afbeelding bij, klik in Outlook de optie om afbeeldingen of geblokkeerde inhoud te weergeven</p>", null, "text/html"
                );

            htmlView.LinkedResources.Add(LinkedImage);
            mm.AlternateViews.Add(htmlView);
            
            SmtpClient smtp = new SmtpClient
            {
                Host = ConfigurationManager.AppSettings["Host"],
                EnableSsl = false
            };
            NetworkCredential NetworkCred = new NetworkCredential(ConfigurationManager.AppSettings["Username"], ConfigurationManager.AppSettings["Password"]);
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = NetworkCred;
            smtp.EnableSsl = false;
            
            smtp.Port = int.Parse(ConfigurationManager.AppSettings["Port"]);
            Console.WriteLine("Sending Email......");
            smtp.Send(mm);
            Console.WriteLine("Email Sent.");
            mm.AlternateViews.Dispose();
            File.Delete(savefile);
            System.Threading.Thread.Sleep(3000);
            Environment.Exit(0);
        }
    }
}
