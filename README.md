# SendCATAASSurprise
Uses the CATAAS API (https://cataas.com) to send people of your choosing by email a random cat and text. 
You can choose what type of message you want to send by telling the application to send a birthday, christmas or new year wishes. 

I made this because a friend sent me the API and I'm a fan of cats. Cat is love, cat is live. 
Also gives people the idea that you never forget about them depite me automating the whole process and still forgot about untill they thank you for the kind gesture. 
The less they know.

## How does it work?
The application is made using C# as a console application on .NET 10. To build it you need the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0). The usage is extremely simple and you can use it to automate the messages as well.
To send something, start the console application, it will ask for the e-mail address and what kind of message you want to send. As of now you can select one of three
- Birthday
- Christmas
- Newyear
(Do type these without any spaces)

If you want to automate it, create a new scheduled task in Windows. Set it to run once a month. Lets say for Christmas you would want to set it to every december the 25th at 00:00 and the first time it should run should also be on 25 december current or next year depending on when you created the task.
Next navigate to the path where you stored the console application, then set the paramaters like this: <emailaddress> <typeofmessage> so for example: "hello@there.com" "christmas"
Also don't forget to set the task to run whether you are logged on or not. 

## How do I change/add lines?
The current lines are in Dutch, you can easily change the lines to any language you like by editing the **messages** list in the **Program** class. Each message type has a subject and a list of lines, one line is picked at random.
To add a new message type, add another entry to that list, e.g. `["easter"] = ("VROLIJK PASEN!", new[] { "..." })`.
I should warn you that you shouldn't make the lines too long. The CATAAS API doesn't do new lines as far as I know so if you do make the lines too long, it will be cutoff on both the left and right side because all text is centered. 

## How does it send email?
It sends through Microsoft 365 using the Microsoft Graph API and an app registration. No SMTP, no passwords and no need to disable MFA.
The app logs in as itself (client ID + client secret) and sends the email from the mailbox you set in **FromEmail**.

### 1. Create the app registration
- Go to https://entra.microsoft.com > **Applications** > **App registrations** > **New registration**
- Give it a name (e.g. *CATAAS Surprise*), leave the rest on default and click **Register**
- On the **Overview** page, copy the **Application (client) ID** and **Directory (tenant) ID**

### 2. Give it permission to send mail
- Go to **API permissions** > **Add a permission** > **Microsoft Graph** > **Application permissions**
- Search for **Mail.Send**, tick it and click **Add permissions**
- Click **Grant admin consent for <your tenant>**

> ⚠️ **Mail.Send** as an application permission lets the app send as **any** mailbox in your tenant.
> Limit it to just the sending mailbox with [RBAC for Applications in Exchange Online](https://learn.microsoft.com/exchange/permissions-exo/application-rbac) (or the older application access policies).

### 3. Create a client secret
- Go to **Certificates & secrets** > **Client secrets** > **New client secret**
- Copy the **Value** straight away, you can't see it again later. Note the expiry date, the app stops sending when it expires.

### 4. Configure the application
Fill in **App.config**:
```xml
<appSettings>
    <add key="TenantId" value="<Directory (tenant) ID>"/>
    <add key="ClientId" value="<Application (client) ID>"/>
    <add key="ClientSecret" value=""/>
    <add key="FromEmail" value="<mailbox to send from>"/>
    <add key="ErrorEmail" value="<where error reports go>"/>
</appSettings>
```
Put the client secret in an environment variable instead of **App.config**, so it never ends up in git:
```powershell
[Environment]::SetEnvironmentVariable("CATAAS_CLIENT_SECRET", "<secret value>", "User")
```
Set it for the same user account the scheduled task runs as. If you really want to, you can put it in **ClientSecret** in **App.config** instead, but don't commit that.

The **FromEmail** mailbox needs to be a licensed user mailbox or a shared mailbox (shared mailboxes don't need a license) in your tenant.
The display name recipients see is the display name of that mailbox.

If anything goes wrong (unknown message type, CATAAS down, Microsoft 365 refusing the email), the details are emailed to **ErrorEmail** with the subject "Error in CATAAS App" and the application exits with code 1, so a scheduled task shows it as failed.
If the Microsoft 365 login itself fails (e.g. an expired client secret), the error can't be emailed and is only shown in the console. 

 ## How do I edit the body and subject?
The subjects are in the **messages** list in the **Program** class, next to the lines for each message type:
```C#
["christmas"] = ("VROLIJK KERSTFEEST!", new[]
```
The body is contained within the **SendMail** class. Look for the **content** line in the **Send** method: the HTML string there contains the image (`<img src="cid:myPic">`) followed by a "With regards" line in Dutch.
You can edit that HTML however you like, or remove everything except the `<img src="cid:myPic">` tag to send only the image.

## Any dependencies?
Just one NuGet package, which is restored automatically when you build:
- **System.Configuration.ConfigurationManager** (reads the settings from **App.config**)

Everything else comes with .NET out of the box (**System.Net.Http** for Microsoft Graph, **System.Text.Json**, **System.IO**).
