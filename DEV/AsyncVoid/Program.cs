using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AsyncVoid
{
    internal class Program
    {
        static void Main(string[] args)
        {

            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string date = DateTime.Now.ToString("yyyyMMdd");
                using StreamWriter file = new("~\\Logger_ " + date + ".txt", append: true);
                file.WriteLine("Hello world");
            }
            catch (Exception e)
            {
                //Operations.CheckStatusOrSend(e);
                Console.WriteLine("Init time: {0}", DateTime.Now.ToString());
                var task = Operations.SendEmail(e);
                Console.WriteLine("Sigo trabajando...");
                Console.WriteLine(task.Status);
                Operations.CheckStatusOrSend(task);
                Console.WriteLine("Mail Sent");

            }

        }
        
    }

    public class Operations
    {
        public static async void CheckStatusOrSend(Task t)
        {
            Debug.WriteLine(t.Status);
            while (!t.IsCompleted)
            {
                
            }

                await t;

        }

        public static async Task SendEmail(Exception e)
        {
            var task = CreateEmail(e);

            SmtpClient mySmtpClient = new SmtpClient("smtp.gmail.com");
            // add from,to mailaddresses
            MailAddress from = new MailAddress("enmarkados03@gmail.com", "enmarkados03@gmail.com");
            MailAddress to = new MailAddress("sistemas@enmarkados.com", "sistemas@enmarkados.com");
            MailMessage myMail = new System.Net.Mail.MailMessage(from, to);

            // set smtp-client with basicAuthentication
            ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            mySmtpClient.UseDefaultCredentials = false;
            NetworkCredential basicAuthenticationInfo = new NetworkCredential("enmarkados03@gmail.com", "ynynpvwkbopwozgx");
            mySmtpClient.Credentials = basicAuthenticationInfo;
            mySmtpClient.Port = 587;
            mySmtpClient.EnableSsl = true;
            mySmtpClient.UseDefaultCredentials = false;


            // add ReplyTo
            MailAddress replyTo = new MailAddress("soporte@enmarkados.com");
            myMail.ReplyToList.Add(replyTo);

            // set subject and encoding
            myMail.Subject = "Mensaje error API";
            myMail.SubjectEncoding = System.Text.Encoding.UTF8;
            myMail.IsBodyHtml = false;

            // set body-message and encoding

            myMail.Body = await task;
            //myMail.BodyEncoding = System.Text.Encoding.UTF8;
            // text or html
            //myMail.IsBodyHtml = true;

            try
            {
                mySmtpClient.Send(myMail);

            }
            catch (Exception ex)
            {
                string time = DateTime.Now.ToString("HH:mm:ss");
                Console.WriteLine(time + " ----- " + ex.ToString());
            }
        }

        public static async Task<string> CreateEmail(Exception e)
        {
            Console.WriteLine("Creating body");
            var task = Task.Delay(60000);

            string body = "";
            StringBuilder sb = new StringBuilder();
            var lastLine = e.StackTrace.Split('\n').Last();
            sb.AppendLine("Ha ocurrido un error:  " + lastLine);
            sb.AppendLine(e.Message);
            sb.AppendLine(DateTime.Now.ToLongDateString());
            body = sb.ToString();

            await task;
            return body;
        }
    }
}