# SendCATAASSurprise
Uses the CATAAS API (https://cataas.com) to send people of your choosing by email a random cat and text. 
You can choose what type of message you want to send by telling the application to send a birthday, christmas or new year wishes. 

I made this because a friend sent me the API and I'm a fan of cats. Cat is love, cat is live. 
Also gives people the idea that you never forget about them depite me automating the whole process and still forgot about untill they thank you for the kind gesture. 
The less they know.

## How does it work?
The application is made using C# as a console application. The usage is extremely simple and you can use it to automate the messages as well.
To send something, start the console application, it will ask for the e-mail address and what kind of message you want to send. As of now you can select one of three
- Birthday
- Christmas
- Newyear
(Do type these without any spaces)

If you want to automate it, create a new scheduled task in Windows. Set it to run once a month. Lets say for Christmas you would want to set it to every december the 25th at 00:00 and the first time it should run should also be on 25 december current or next year depending on when you created the task.
Next navigate to the path where you stored the console application, then set the paramaters like this: <emailaddress> <typeofmessage> 
example: "hello@there.com" "christmas"
    
Also don't forget to set the task to run whether you are logged on or not. 

## How do I change/add lines?
The current lines are in Dutch, you can easily change the lines to any language you like by editing the **newyeargen**, **birthdaygen** and **christmasgen** arrays in the **Program** class.
I should warn you that you shouldn't make the lines too long. The CATAAS API doesn't do new lines as far as I know so if you do make the lines too long, it will be cutoff on both the left and right side because all text is centered. 

## Does this work with any Exchange version?
I tested this with Exchange 2013, so I would assume it would work for any version above that. 
I'm also assuming it might work for 2007/2010 as well because all you really need is to allow the sending computer to send mail. So basically configure a relay in your Exchange server.
I've seen cases where SSL isn't really working properly on an Exchange server because the certificates seems wrong despite admins renewing it.
In the Sendmail class you might want to leave out **smtp.EnableSsl** or set it to false.

As for Exchange 365, this works as well. You will need to program modern authentication in it which I haven't. 
To save myself some time, I've opted to use a free SMTP service to send the emails (https://www.smtp2go.com/)

## How I change the mailserver settings?
Before you can even send, you will need to edit the **App.config** for that. It will contain settings that look like this:
```C#
<appSettings>
    <add key="Host" value="<smtp server"/>
    <add key="Port" value="<smtp port>"/>
    <add key="FromEmail" value="<From Emailaddress>"/>
    <add key="FromEmailName" value="<Displayname E-mail> "/>
    <add key="Username" value="<email username>"/>
    <add key="Password" value="<email password"/>
  </appSettings>
  ``` 

 ## How do I edit the body and subject?
You can find these settings in the **Program** class. 
Scroll all the way down to find the switch statement that sends the email. 
Change the second parameter in the following line to change the subject: 
```C#
sm.Send(mail, "VROLIJK KERSTFEEST!", churl);
```
The body is actually contained within the **Sendmail** class. Look for the **mm.body** line you will see it adds the imagecontent variable for the image and then a "With regards" line in dutch.
I've enabled HTML so you can edit the body with all the HTML tags you need to create a body or just leave it all out and just send the image. To do that, just delete the entire line but leave **imagecontent** there

## Any dependancies?
No, none at all. It uses the basic out-of-the box functionalities. 
- **System.net.mail**
- **System.IO**
- **System.Text**
Really not all that special as you can see. 
