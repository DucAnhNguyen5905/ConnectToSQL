using System;
using System.Net;
using System.Net.Mail;

public class EmailService
{
    public static void SendOTP(string email, string otp)
    {
        string fromEmail = "ducanhnguyen5905@gmail.com";
        string fromPassword = "zmor riri qpfb cjdo";

        MailMessage mail = new MailMessage();
        mail.From = new MailAddress(fromEmail);
        mail.To.Add(email);
        mail.Subject = "Your OTP Code";
        mail.Body = "Your OTP is: " + otp;

        SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
        smtp.Credentials = new NetworkCredential(fromEmail, fromPassword);
        smtp.EnableSsl = true;

        smtp.Send(mail);
    }
}

