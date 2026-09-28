using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace SendCATAASsurprises
{
    class Program
    {
        //Kept here so the error report can say who the email was for and what type it was
        private static string mail;
        private static string type;

        static async Task Main(string[] args)
        {
            bool interactive = args.Length == 0;

            try
            {
                await Run(args, interactive);
            }
            catch (Exception ex)
            {
                Environment.ExitCode = 1;
                Console.WriteLine("Something went wrong: " + ex.Message);
                await ReportError(ex, args);
            }

            //Give the user a moment to read the result before the console window closes
            if (interactive)
            {
                await Task.Delay(3000);
            }
        }

        //Emails the error, if that fails as well (e.g. Microsoft 365 login is broken) it can only be shown in the console
        private static async Task ReportError(Exception ex, string[] args)
        {
            try
            {
                await SendMail.SendError(ex, mail, type, args);
                Console.WriteLine("The error details have been emailed.");
            }
            catch (Exception reportEx)
            {
                Console.WriteLine("Could not email the error details either: " + reportEx.Message);
                Console.WriteLine();
                Console.WriteLine(ex);
            }
        }

        private static async Task Run(string[] args, bool interactive)
        {
            if (args.Length == 1)
            {
                throw new ArgumentException("Missing message type. Usage: SendCATAASsurprises <emailaddress> <Birthday|Christmas|Newyear>");
            }

            if (interactive)
            {
                Console.WriteLine("Who do you want to send an email to?:");
                mail = Console.ReadLine();
                Console.WriteLine("What type of mail do you want to send?:");
                Console.WriteLine("Select one of the following (Birthday, Christmas, Newyear):");
                type = Console.ReadLine();
            }
            else
            {
                mail = args[0];
                type = args[1];
            }

            if (string.IsNullOrWhiteSpace(mail))
            {
                throw new ArgumentException("No email address was given.");
            }

            //Get the current year for use in the New Year wishes generator
            string year = DateTime.Now.Year.ToString();

            //Subject and possible lines for each message type, one line is picked at random
            var messages = new Dictionary<string, (string Subject, string[] Lines)>
            {
                ["christmas"] = ("VROLIJK KERSTFEEST!", new[]
                {
                    "De kerstman heeft het goede idee, bezoek mensen alleen 1x per jaar",
                    "Kerst is een dag om te vieren en om je bankrekening leeg te plunderen",
                    "Geen geld deze kerst, hier mag je deze kat hebben voor gezelschap",
                    "Kerst is de Disneyficatie van Christendom",
                    "Kerst is geweldig, veel kado's die je krijgt om te ruilen",
                    "Verstuur je kerstkado's op tijd zodat PostNL die kwijt kan maken",
                    "Kerst is het seizoen waar je kado's dit jaar koopt met geld van volgende jaar",
                    "Mentaal ben je voorbereid voor kerst, financieel nooit",
                    "Kerstkado's komen vanuit het hart, maar geld en kadobonnen werken ook",
                    "Kerst is een dag van wonderen. Al mijn geld verdwijnt gewoon!",
                    "De kerstman is altijd vrolijk, hij weet waar alle stoute meiden wonen :)"
                }),

                ["birthday"] = ("GEFELICITEERD JARIGE JOB!", new[]
                {
                    "Een leuke verjaardag is 90 procent mentaal en 10 procent alcohol",
                    "Blij dat je geboren bent op deze dag, anders had ik geen vermaak",
                    "Je ziet niet ouder uit dan 16! Vanaf een afstand, met mijn ogen dicht...",
                    "Jarig zijn is een excuus om dronken te worden op een doordeweekse dag",
                    "Een echte vriend onthoudt je verjaardag, niet je leeftijd",
                    "Moge je verjaardagstaart lekker sappig zijn",
                    "Op naar nog een jaar van twijfelachtige levensbeslissingen!",
                    "Jarig zijn is net golf, het is leuker als je het niet bijhoudt",
                    "From hearte congratulations with your furyearday",
                    "Geweldig nieuws! …je leeft nog steeds!",
                    "Gefeliciteerd met 1 jaar dichterbij pensioen!",
                    "Wat gaat naar boven, maar nooit naar beneden? Je leeftijd! :3"
                }),

                ["newyear"] = ("GELUKKIG NIEUWJAAR!", new[]
                {
                    "Gelukkig nieuwjaar! Spoiler Alert! Het zal hetzelfde voelen",
                    "Op naar 365 nieuwe rondjes om de zon, kansen en teleurstellingen",
                    "Gelukkig nieuwjaar! Goed gedaan, je hebt het overleefd",
                    "Tijd om oude fouten te maken op verschillende manieren, YAY NIEUWJAAR!",
                    "Is het alweer " + year + "? Ik was zo gewend aan de vorige",
                    "Een sprankelend nieuw jaar om te beginnen met oude gewoonten",
                    "Ben sinds vorig jaar niet zo enthousiast geweest over een nieuw jaar",
                    "Zorgwekkend dat alcohol nodig is om nog een jaar het hoofd te bieden..."
                })
            };

            if (!messages.TryGetValue((type ?? string.Empty).Trim().ToLower(), out var message))
            {
                throw new ArgumentException($"Unknown message type '{type}'. Choose one of: Birthday, Christmas, Newyear (without spaces).");
            }

            //Create E-mail
            string line = message.Lines[Random.Shared.Next(message.Lines.Length)];
            string url = "https://cataas.com/cat/says/" + Uri.EscapeDataString(line) + "?fontColor=orange&fontSize=20&width=1000&height=1000";
            await SendMail.Send(mail, message.Subject, url);
        }
    }
}
