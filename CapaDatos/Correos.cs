using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class Correos
    {
        private readonly string _fromEmail;
        private readonly string _fromPassword;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        public Correos()
        {
            _fromEmail = ConfigurationManager.AppSettings["FromEmail"];
            _fromPassword = ConfigurationManager.AppSettings["FromPassword"];
            _smtpHost = ConfigurationManager.AppSettings["SMTPHost"];
            _smtpPort = int.Parse(ConfigurationManager.AppSettings["SMTPPort"]);
        }
        public bool EnviarCorreo(string correo, string nuevoCodigo)
        {
            String mensajeError = "";

            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(_fromEmail);
                    mail.To.Add(correo);
                    mail.Subject = "Código de Recuperación";
                    mail.Body =
                        $"Estimado/a {correo},<br><br>" +
                     $"Hemos recibido una solicitud para restablecer la contraseña. " +
                     $"Código: <strong>{nuevoCodigo}</strong><br><br>" +
                     $"Este código es válido por 15 minutos.<br><br>" +
                     $"Saludos cordiales,<br>Equipo de Soporte";
                    mail.IsBodyHtml = true;
                    using (SmtpClient smtpClient = new SmtpClient(_smtpHost, _smtpPort))
                    {
                        // Configuración del cliente SMTP
                        smtpClient.Credentials = new NetworkCredential(_fromEmail, _fromPassword);
                        smtpClient.EnableSsl = true;

                        // Enviar correo
                        smtpClient.Send(mail);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                mensajeError = $"Error al enviar el correo: {ex.Message}";
                return false;

            }
        }
        public bool EnviarCorreoCaso(string correo, string Nombre, String Num_Expediente)
        {
            String mensajeError = "";

            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(_fromEmail);
                    mail.To.Add(correo);
                    mail.Subject = "Apertura de caso";
                    mail.Body =
                        $"Estimado/a {Nombre},<br><br>" +
                     $"Hemos recibido una solicitud abrir un caso " +
                      $"El cual puede ser consultado con el siguiente número de expediente  {Num_Expediente},<br><br>" +
                     $"Saludos cordiales,<br>Equipo de gestión jurídica";
                    mail.IsBodyHtml = true;
                    using (SmtpClient smtpClient = new SmtpClient(_smtpHost, _smtpPort))
                    {
                        // Configuración del cliente SMTP
                        smtpClient.Credentials = new NetworkCredential(_fromEmail, _fromPassword);
                        smtpClient.EnableSsl = true;

                        // Enviar correo
                        smtpClient.Send(mail);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                mensajeError = $"Error al enviar el correo: {ex.Message}";
                return false;

            }
        }
        public bool EnviarCierreCaso(string correo, string Nombre, String Num_Expediente)
        {
            String mensajeError = "";

            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(_fromEmail);
                    mail.To.Add(correo);
                    mail.Subject = "Cierre de caso";
                    mail.Body =
                        $"Estimado/a {Nombre},<br><br>" +
                     $" caso  terminado" +
                      $"El cual puede ser consultado con el siguiente número de expediente  {Num_Expediente},<br><br>" +
                     $"Saludos cordiales,<br>Equipo de gestión jurídica";
                    mail.IsBodyHtml = true;
                    using (SmtpClient smtpClient = new SmtpClient(_smtpHost, _smtpPort))
                    {
                        // Configuración del cliente SMTP
                        smtpClient.Credentials = new NetworkCredential(_fromEmail, _fromPassword);
                        smtpClient.EnableSsl = true;

                        // Enviar correo
                        smtpClient.Send(mail);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                mensajeError = $"Error al enviar el correo: {ex.Message}";
                return false;

            }
        }

    }
}
