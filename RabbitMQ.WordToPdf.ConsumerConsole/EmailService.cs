using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.WordToPdf.ConsumerConsole
{
    public class EmailService
    {
        public static bool EmailSend(string email, MemoryStream memoryStream, string fileName)
        {
            try
            {
                var senderMail = "xxxxxx@xxxxxx.com";
                memoryStream.Position = 0;
                System.Net.Mime.ContentType ct = new System.Net.Mime.ContentType(System.Net.Mime.MediaTypeNames.Application.Pdf);

                Attachment attachment = new Attachment(memoryStream, ct);
                attachment.ContentDisposition.FileName = $"{fileName}.pdf";

                MailMessage mailMessage = new MailMessage();

                SmtpClient smtpClient = new SmtpClient();

                mailMessage.From = new MailAddress(senderMail);
                mailMessage.To.Add(email);
                mailMessage.Subject = "PDF File Created!";
                mailMessage.Body = "PDF File has been converted from word file. ";
                mailMessage.IsBodyHtml = true;

                mailMessage.Attachments.Add(attachment);

                smtpClient.Host = "smtp.gmail.com";
                smtpClient.Port = 587;
                smtpClient.UseDefaultCredentials = false;
                NetworkCredential credentials = new NetworkCredential("xxxxxx@xxxxxx.com", "+password******");

                smtpClient.EnableSsl = true;                           
                smtpClient.Credentials = credentials;

                smtpClient.Send(mailMessage);

                Console.WriteLine($"Email sent to {email}");

                memoryStream.Close();
                memoryStream.Dispose();
                return true;
            }
            catch (Exception ex )
            {
                Console.WriteLine($"Error {ex.Message} occur while sending to {email}");
                return false;
            }
        }

    }
}
