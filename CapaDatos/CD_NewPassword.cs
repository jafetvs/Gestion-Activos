using CapaModelo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_NewPassword
    {
        public static CD_NewPassword _instancia = null;

        private CD_NewPassword()
        {

        }
        public static CD_NewPassword Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new CD_NewPassword();
                }
                return _instancia;
            }
        }
        public string RecuperarPassword(string correo)
        {
            string nuevoCodigo = "";
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    // Generar código aleatorio de 6 dígitos
                    nuevoCodigo = new Random().Next(100000, 999999).ToString();

                    // Establecer fecha de expiración (por ejemplo, 15 minutos desde ahora)
                    DateTime fechaExpiracion = DateTime.Now.AddMinutes(15);

                    // Query en línea en lugar de procedimiento almacenado
                    string query = @"UPDATE INGRESO 
                             SET Codigo_Recuperacion = @Codigo, 
                                 Fecha_Expiracion_Codigo = @FechaExpiracion 
                             WHERE Correo = @Correo";

                    SqlCommand cmd = new SqlCommand(query, oConexion);
                    cmd.Parameters.AddWithValue("@Codigo", nuevoCodigo);
                    cmd.Parameters.AddWithValue("@FechaExpiracion", fechaExpiracion);
                    cmd.Parameters.AddWithValue("@Correo", correo);

                    oConexion.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }

                return nuevoCodigo;
            }
        }

        // Verificar si el código de recuperación es válido
        public bool ValidarCodigo(string correo, string codigo)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                // Consulta que valida el correo, el código y que el código no haya expirado
                string query = @"SELECT COUNT(1) 
                         FROM INGRESO 
                         WHERE Correo = @Correo 
                           AND Codigo_Recuperacion = @Codigo 
                           AND Fecha_Expiracion_Codigo IS NOT NULL 
                           AND Fecha_Expiracion_Codigo > GETDATE()";

                SqlCommand cmd = new SqlCommand(query, oConexion);
                cmd.Parameters.AddWithValue("@Correo", correo);
                cmd.Parameters.AddWithValue("@Codigo", codigo);

                oConexion.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());

                return count > 0;
            }
        }

        //INGRESO DE LOS NUEVOS DATOS 

        public bool CambiarContraseña(string correo, string nuevaContraseña)
        {
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                string query = @"UPDATE INGRESO 
                         SET Clave = @Clave, 
                             Codigo_Recuperacion = NULL, 
                             Fecha_Expiracion_Codigo = NULL 
                         WHERE Correo = @Correo";

                SqlCommand cmd = new SqlCommand(query, oConexion);
                cmd.Parameters.AddWithValue("@Correo", correo);
                cmd.Parameters.AddWithValue("@Clave", HashPassword(nuevaContraseña));

                oConexion.Open();
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }

        // Método para hash de contraseñas
        private string HashPassword(string str)
        {
            SHA256 sha256 = SHA256Managed.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] stream = null;
            StringBuilder sb = new StringBuilder();
            stream = sha256.ComputeHash(encoding.GetBytes(str));
            for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);
            return sb.ToString();
        }


    }
}
