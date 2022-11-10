using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace WriteSendSave
{
    internal class Program
    {
        static void Main(string[] args)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("En un atardecer triste y quejoso");
            sb.AppendLine("Meditaba yo debil y abrumado");
            sb.AppendLine("Sobre un volumen de ciencias muy curioso");
            sb.AppendLine("De temas que ya estaban olvidados");


            for (int i = 0; i < 4; i++)
            {
                string time = DateTime.Now.ToString("HH:mm:ss");
                MyStreamWriter.ExampleAsync(time + "_hello" + i.ToString()); 
            }

            //MailSender.SendEmail(sb.ToString());    
        }
    }


    class MyStreamWriter
    {
        public static async Task ExampleAsync(string msg)
        {
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string date = DateTime.Now.ToString("yyyyMMdd");
                using StreamWriter file = new("~\\Logger_ " + date + ".txt", append: true);
                await file.WriteLineAsync(msg);
            }
            catch (Exception e)
            {
                MailSender.SendEmail(e);
            }
        }
    }

    class MailSender
    {


        public static string CreateEmail(Exception e)
        {
            string body = "";
            StringBuilder sb = new StringBuilder();
            var lastLine = e.StackTrace.Split('\n').Last();
            sb.AppendLine("Ha ocurrido un error en el API:  " + lastLine);
            sb.AppendLine(e.Message);
            body = sb.ToString();



            return body;
        }

        public static void SendEmail(Exception e)
        {

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
            myMail.Body = CreateEmail(e);
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
                MyStreamWriter.ExampleAsync(time + " ----- " + e.ToString());
            }
        }
    }


    class DBSaver
    {

        static string ConnectionString = "Server=192.168.1.180;Database=MamaDelNorte;User Id=sa;Password=%gOn%hAy%vAs%iTz%lAu%;";
        public static void DBInsert(Exception e)
        {
            var st = new StackTrace(e, true);

            string query = "INSERT INTO [dbo].[ErrorLog] ([ErrorType],[ErrorMessage],[ErrorLocation],[CreationDate]) VALUES (@p1, @p2, @p3, @p4);";

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                // Creating SqlCommand objcet   
                SqlCommand cm = new SqlCommand(query, connection);
                cm.Parameters.AddWithValue("@p1", "DB");
                cm.Parameters.AddWithValue("@p2", e.Message);
                cm.Parameters.AddWithValue("@p3", e.StackTrace.ToString());
                cm.Parameters.AddWithValue("@p4", DateTime.UtcNow.ToLongTimeString);
                // Opening Connection  
                connection.Open();
                // Executing the SQL query  
                cm.ExecuteNonQuery();

                connection.Close();
            }
        }
    }
    
}


/// 1- DB
/// 2- 