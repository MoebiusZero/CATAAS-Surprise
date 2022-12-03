using System;
using System.Net.Mail;
using System.Configuration;


namespace SendCATAASsurprises
{
    class Program
    {
        public static string mail;
        public static string type;

        static void Main(string[] args)
        {
            if (args.Length == 0)
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

            string[] christmasgen =
            {
                    "De kerstman heeft het goede idee, bezoek mensen alleen 1x per jaar",
                    "Kerst is een dag om te vieren en om je bankrekening leeg te plunderen",
                    "Geen geld deze kerst, hier mag je deze kat hebben voor gezelschap",
                    "Kerst is de Disneyficatie van Christendom",
                    "Kerst is geweldig, veel kado's die je krijgt om om te ruilen",
                    "Verstuur je kerstkado's op tijd zodat PostNL die kwijt kan maken",
                    "Kerst is het seizoen waar je kado's dit jaar koopt met geld van volgende jaar",
                    "Mentaal ben je voorbereid voor kerst, financieel nooit",
                    "Kerstkado's komen vanuit het hard, maar geld en kadobonnen werken ook",
                    "Kerst is een dag van wonderen. Al mijn geld verdwijnt gewoon!",
                    "De kerstman is altijd vrolijk, hij weet waar alle stoute meiden wonen :)"
                };

            string[] birthdaygen =
            {
                    "Een leuke verjaardag is 90 procent mentaal en 10 procent alcohol",
                    "Blij dat je geboren bent op deze dag, anders had ik geen vermaak",
                    "Je ziet niet ouder uit dan 16! Vanaf een afstand, met mijn ogen dicht...",
                    "Jarig zijn is een excuus om dronken te worden op een doordeweekse dag",
                    "Een echte vriend onthoudt je verjaardag, niet je leeftijd",
                    "Moge je verjaardagstaart lekker sappig zijn",
                    "Op naar nog een jaar van twijfelachtige levensbeslissingen!",
                    "Jarig zijn is net golf, het is leuker als je het niet bijhoudt"
                };

            //Get the current year for use in the New Year wishes generator
            string year = DateTime.Now.Year.ToString();

            string[] newyeargen =
            {
                    "Gelukkig nieuwjaar! Spoiler Alert! Het zal hetzelfde voelen",
                    "Op naar 365 nieuwe rondjes om de zon, kansen en teleurstellingen",
                    "Gelukkig nieuwjaar! Goed gedaan, je hebt het overleeft",
                    "Tijd om oude foute te maken op verschillende manieren, YAY NIEUWJAAR!",
                    "Is het alweer " + year + "? Ik was zo gewend aan de vorige",
                    "Een sprankelend nieuw jaar om te beginnen met oude gewoonten",
                    "Ben sinds vorig jaar niet zo enthousiast geweest over een nieuw jaar",
                    "Zorgwekkend dat alcohol nodig is om nog een jaar het hoofd te bieden..."
                };

            //Create E-mail
            switch (type.ToLower())
            {
                case "christmas":
                    Random chrnd = new Random();
                    int chindex = chrnd.Next(christmasgen.Length);
                    string christmasline = Uri.EscapeDataString(christmasgen[chindex]);
                    string churl = "https://cataas.com/c/s/" + christmasline + "?wi=800&he=800";
                    SendMail sm = new SendMail();
                    sm.Send(mail, "VROLIJK KERSTFEEST!", churl);
                    break;

                case "birthday":
                    Random bdrnd = new Random();
                    int bdindex = bdrnd.Next(birthdaygen.Length);
                    string birthdayline = Uri.EscapeDataString(birthdaygen[bdindex]);
                    string bdurl = "https://cataas.com/c/s/" + birthdayline + "?wi=800&he=800";
                    SendMail bdsm = new SendMail();
                    bdsm.Send(mail, "GEFELICITEERD JARIGE JOB!", bdurl);
                    break;

                case "newyear":
                    Random nyrnd = new Random();
                    int nyindex = nyrnd.Next(newyeargen.Length);
                    string newyearline = Uri.EscapeDataString(newyeargen[nyindex]);
                    string nyurl = "https://cataas.com/c/s/" + newyearline + "?wi=800&he=800";
                    SendMail nysm = new SendMail();
                    nysm.Send(mail, "GELUKKIG NIEUWJAAR!", nyurl);
                    break;
            }
        }
    }
}

