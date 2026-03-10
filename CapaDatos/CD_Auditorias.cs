using CapaModelo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CD_Auditorias
    {
        public static CD_Auditorias _instancia = null;
        private CD_Auditorias()  { }
        public static CD_Auditorias Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new CD_Auditorias();
                }
                return _instancia;
            }
        }
        public List<Auditorias> ObtenerAuditoria(DateTime? fechaInicio = null, DateTime? fechaFin = null, string tabla = "", string cedulaUsuario = "")
        {
            List<Auditorias> listaAuditorias = new List<Auditorias>();

            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                SqlCommand cmd = new SqlCommand("usp_ObtenerAuditoria", oConexion);
                cmd.CommandType = CommandType.StoredProcedure;

                // Parámetros del procedimiento almacenado (manejo de nulos)
                cmd.Parameters.AddWithValue("@Cedula_Usuario", string.IsNullOrEmpty(cedulaUsuario) ? (object)DBNull.Value : cedulaUsuario);
                cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio.HasValue ? (object)fechaInicio.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaFin", fechaFin.HasValue ? (object)fechaFin.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@Tabla", string.IsNullOrEmpty(tabla) ? (object)DBNull.Value : tabla);

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        listaAuditorias.Add(new Auditorias()
                        {
                            Cod_Auditoria = Convert.ToInt32(dr["Cod_Auditoria"]),
                            Cedula_Usuario = dr["Cedula_Usuario"].ToString(),
                            Nom_Rol = dr["Nom_Rol"].ToString(),
                            Tabla = dr["Tabla"].ToString(),
                            Procedimiento = dr["Procedimiento"].ToString(),
                            ID_ColumAfectada = dr["ID_ColumAfectada"] != DBNull.Value ? Convert.ToInt32(dr["ID_ColumAfectada"]) : (int?)null,
                            Modificacion_JSON = dr["Modificacion_JSON"] != DBNull.Value ? dr["Modificacion_JSON"].ToString() : null,
                            Fecha_Modificacion = Convert.ToDateTime(dr["Fecha_Modificacion"])
                        });
                    }
                    dr.Close();
                    return listaAuditorias;
                }
                catch (Exception ex)
                {
                    // Registrar el error o lanzarlo si es crítico
                    Console.WriteLine("Error: " + ex.Message);
                    return null;
                }
            }
        }
        public bool GuardarAuditoria(Auditorias datos)
        {
            bool respuesta = false;
            using (SqlConnection conexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    string query = @"INSERT INTO AUDITORIA (Cedula_Usuario, Nom_Rol, Tabla, Procedimiento,
    ID_ColumAfectada, Modificacion_JSON)
    VALUES (@Cedula_Usuario, @Nom_Rol, @Tabla, @Procedimiento, @ID_ColumAfectada, @Modificacion_JSON)";


                    using (SqlCommand cmd = new SqlCommand(query, conexion))
                    {
                        cmd.Parameters.AddWithValue("@Cedula_Usuario", datos.Cedula_Usuario ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Nom_Rol", datos.Nom_Rol ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Tabla", datos.Tabla ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Procedimiento", datos.Procedimiento ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ID_ColumAfectada", datos.ID_ColumAfectada.HasValue ? datos.ID_ColumAfectada.Value : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Modificacion_JSON", datos.Modificacion_JSON ?? (object)DBNull.Value);

                        conexion.Open();
                        int filasAfectadas = cmd.ExecuteNonQuery();
                        respuesta = filasAfectadas > 0;
                    }
                }
                catch (Exception ex)
                {
                    // Log o gestión de errores
                    Console.WriteLine("Error al guardar en la auditoría: " + ex.Message);
                    respuesta = false;
                }
            }
            return respuesta;
        }




    }

}
