# SendCATAASSurprise
Uses the CATAAS API (https://cataas.com) to send people of your choosing by email a random cat and text. 
You can choose what type of message you want to send by telling the application to send a birthday, christmas or new year wishes. 

## How does it work?
The application is made using C# as a console application. The use is extremely simple and you can use it to automate the messages as well.
To send something, start the console application, it will ask for e-mail address and what kind of message you want to send. 

If you want to automate it, create a new scheduled task in Windows. Set it to run once a month. For Christmas you would want to set it to every december the 25th and starting on 25 december current or next year depending on when you created the task
Next navigate to the path where you stored the console application, then set the paramaters like this: <emailaddress> <typeofmessage> so for example: "hello@there.com" "christmas"

## How do I change/add lines?
The current lines are in Dutch, you can easily change the lines to any language you like by editing the newyeargen, birthdaygen and christmasgen arrays.
I should warn you that you shouldn't make the lines too long. The CATAAS API doesn't do new lines as far as I know so if you do make the lines too long, it will be cutoff on both the left and right side. 

## Does this work with any Exchange version?
I tested this with Exchange 2013, so I would assume it would work for any version above that. 
I'm also assuming it might work for 2007/2010 as well because all you really need to to allow the sending computer to the relay.
I've seen cases where SSL isn't really working properly on an Exchange server because the certificates seem wrong dispite admins renewing it.
In the Sendmail class you might want to leave out **smtp.EnableSsl** or set it to false.

As for Exchange 365, this works as well. The caveat here is that the account you are using to send the e-mails must have MFA disabled. 
You have two ways to send e-mails. Direct-send or Microsoft's SMTP server.
Direct send allows you to send only mails to anyone within your tenant, this does not require a license!
If you want to send mail externally, then you need to assign at least a Exchange license to the user and configure it with the least amount of rights because you will be using it as a "service account"

### Disable MFA
By default Azure enables MFA for ALL users because of the Security Defaults. To make sending possible you need to disable the Azure Security Defaults and replace it with Conditional Access policies to cover MFA policies you might have or need. 
I will not be covering how to create a conditional access policy, I will only tell you how to disable the Security Defaults for "testing" purposes
- Login to https://portal.azure.com and go to **Azure Active Directory**
- Go to **Properties**
- All the way down you'll find a link named **Manage security defaults**
- Set it to **No** and when asked about why, just select anything and save.

To test if the account has MFA disabled, try logging into any Microsoft 365 service with it and see if it asks for anything. 

## How I change the mailserver settings?
You will need to edit the **App.config** for that. It will contain settings that look like this:
 <sup><appSettings>
    <add key="Host" value="<smtp server"/>
    <add key="Port" value="<smtp port>"/>
    <add key="FromEmail" value="<From Emailaddress>"/>
    <add key="FromEmailName" value="<Displayname E-mail> "/>
    <add key="Username" value="<email username>"/>
    <add key="Password" value="<email password"/>
  </appSettings></sup>
  
 ## How do I edit the body and subject?
You can find these settings in the **Program** class. 
Scroll all the way down to find the switch statement that sends the email. 
Change the second parameter in the following line to change the subject: <sup>sm.Send(mail, "VROLIJK KERSTFEEST!", churl);</sup>

The body is actually contained within the **Sendmail** class. Look for the **mm.body** line you will see it adds the imagecontent variable for the image and then a "With regards" line in dutch.
I've enabled HTML so you can edit the body with all the HTML tags you need to create a body or just leave it all out and just send the image. 

## Any dependancies?
No, none at all. It uses the basic out-of-the box functionalities. 
- **System.net.mail**
- **System.IO**
- **System.Text**
Really not all that special as you can see. 