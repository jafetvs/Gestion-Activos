using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Control_Gestion_V3.Encriptaciones
{
    public class Encriptar
    {
        public static string GetSHA256(string str)
        {
            SHA256 sha256 = SHA256Managed.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] stream = null;
            StringBuilder sb = new StringBuilder();
            stream = sha256.ComputeHash(encoding.GetBytes(str));
            for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);
            return sb.ToString();
        }
        public static string contrasena(string palabra)
        {
            // Concatenar "123*" a la clave generada
            string claveFinal = palabra + "123*";
            // Encriptar la clave usando SHA256
            SHA256 sha256 = SHA256.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] stream = sha256.ComputeHash(encoding.GetBytes(claveFinal));

            // Convertir el hash a una cadena hexadecimal
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < stream.Length; i++)
            {
                sb.AppendFormat("{0:x2}", stream[i]);
            }

            return sb.ToString();
        }
    }
}