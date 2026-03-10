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
    public class CD_Solicitantes
    {
        public static CD_Solicitantes _instancia = null;
        private CD_Solicitantes()
        {
        }
        public static CD_Solicitantes Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new CD_Solicitantes();
                }
                return _instancia;
            }
        }
        public List<Solicitantes> ObtenerSolicitantes()
        {
            List<Solicitantes> rptListaPartes = new List<Solicitantes>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                SqlCommand cmd = new SqlCommand("usp_ObtenerSolicitante", oConexion);
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        rptListaPartes.Add(new Solicitantes()
                        {
                            ID_Usuario = dr["ID_usuario"] != DBNull.Value ? Convert.ToInt32(dr["ID_usuario"]) : 0,
                            TipoDocumento = dr["TipoDocumento"]?.ToString() ?? string.Empty,
                            Cedula_Solicitante = dr["Cedula_Solicitante"]?.ToString() ?? string.Empty,
                            Nombre = dr["Nombre"]?.ToString() ?? string.Empty,
                            Direccion = dr["Direccion"] != DBNull.Value ? dr["Direccion"].ToString() : null,
                            Telefono = dr["Telefono"] != DBNull.Value ? (int?)Convert.ToInt32(dr["Telefono"]) : null,
                            Telefono2 = dr["Telefono2"] != DBNull.Value ? (int?)Convert.ToInt32(dr["Telefono2"]) : null,
                            Fax = dr["Fax"] != DBNull.Value ? (int?)Convert.ToInt32(dr["Fax"]) : null,
                            Correo = dr["Correo"]?.ToString() ?? "No Especificado",
                            Activo = dr["Activo"] != DBNull.Value && Convert.ToBoolean(dr["Activo"]),
                            FechaRegistro = dr["FechaRegistro"] != DBNull.Value ? Convert.ToDateTime(dr["FechaRegistro"]) : DateTime.MinValue
                        });
                    }

                    dr.Close();
                    return rptListaPartes;
                }
                catch (Exception ex)
                {
                    string err = "llll" + ex;
                    rptListaPartes = null;
                    return rptListaPartes;
                }
            }
        }
        public Solicitantes ObtenerUnSolicitante(string Cedula_Solicitante)
        {
            Solicitantes solicitante = null;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                SqlCommand cmd = new SqlCommand("usp_ObtenerUnSolicitante", oConexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Cedula_Solicitante", Cedula_Solicitante);
                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        solicitante = new Solicitantes()
                        {
                            ID_Usuario = dr["ID_usuario"] != DBNull.Value ? Convert.ToInt32(dr["ID_usuario"]) : 0,
                            TipoDocumento = dr["TipoDocumento"]?.ToString() ?? string.Empty,
                            Cedula_Solicitante = dr["Cedula_Solicitante"]?.ToString() ?? string.Empty,
                            Nombre = dr["Nombre"]?.ToString() ?? string.Empty,
                            Direccion = dr["Direccion"] != DBNull.Value ? dr["Direccion"].ToString() : null,
                            Telefono = dr["Telefono"] != DBNull.Value ? (int?)Convert.ToInt32(dr["Telefono"]) : null,
                            Telefono2 = dr["Telefono2"] != DBNull.Value ? (int?)Convert.ToInt32(dr["Telefono2"]) : null,
                            Fax = dr["Fax"] != DBNull.Value ? (int?)Convert.ToInt32(dr["Fax"]) : null,
                            Correo = dr["Correo"]?.ToString() ?? "No Especificado",
                            Activo = dr["Activo"] != DBNull.Value && Convert.ToBoolean(dr["Activo"]),
                            FechaRegistro = dr["FechaRegistro"] != DBNull.Value ? Convert.ToDateTime(dr["FechaRegistro"]) : DateTime.MinValue
                        };
                    }
                    dr.Close();
                    return solicitante;
                }
                catch (Exception ex)
                {
                    string err = "Error: " + ex.Message;
                    return null;
                }
            }
        }
        public bool RegistrarSolicitante(Solicitantes oSolicitantes)
        {
            bool respuesta = true;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("usp_RegistrarSolicitante", oConexion);
                    cmd.Parameters.AddWithValue("TipoDocumento", oSolicitantes.TipoDocumento ?? (object)DBNull.Value);
                    // cmd.Parameters.AddWithValue("TipoDocumento", oSolicitantes.TipoDocumento);
                    cmd.Parameters.AddWithValue("Cedula_Solicitante", oSolicitantes.Cedula_Solicitante);
                    cmd.Parameters.AddWithValue("Nombre", oSolicitantes.Nombre);
                    cmd.Parameters.AddWithValue("Direccion", oSolicitantes.Direccion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("Telefono", oSolicitantes.Telefono == 0 ? (object)DBNull.Value : oSolicitantes.Telefono);
                    cmd.Parameters.AddWithValue("Telefono2", oSolicitantes.Telefono2 == 0 ? (object)DBNull.Value : oSolicitantes.Telefono2);
                    cmd.Parameters.AddWithValue("Fax", oSolicitantes.Fax == 0 ? (object)DBNull.Value : oSolicitantes.Fax);
                    cmd.Parameters.AddWithValue("Correo", oSolicitantes.Correo);
                    cmd.Parameters.AddWithValue("Activo", oSolicitantes.Activo);
                    cmd.Parameters.Add("Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;
                    oConexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = Convert.ToBoolean(cmd.Parameters["Resultado"].Value);
                }
                catch (Exception ex)
                {
                    string Queerror = "fggggggggggh" + ex;
                    respuesta = false;
                }
            }
            return respuesta;
        }
        public bool ModificarSolicitante(Solicitantes oSolicitantes)
        {
            bool respuesta = true;

            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("usp_ModificarSolicitante", oConexion);
                    cmd.Parameters.AddWithValue("TipoDocumento", oSolicitantes.TipoDocumento ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("Cedula_Solicitante", oSolicitantes.Cedula_Solicitante);
                    cmd.Parameters.AddWithValue("Nombre", oSolicitantes.Nombre);
                    cmd.Parameters.AddWithValue("Direccion", oSolicitantes.Direccion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("Telefono", oSolicitantes.Telefono == 0 ? (object)DBNull.Value : oSolicitantes.Telefono);
                    cmd.Parameters.AddWithValue("Telefono2", oSolicitantes.Telefono2 == 0 ? (object)DBNull.Value : oSolicitantes.Telefono2);
                    cmd.Parameters.AddWithValue("Fax", oSolicitantes.Fax == 0 ? (object)DBNull.Value : oSolicitantes.Fax);
                    cmd.Parameters.AddWithValue("Correo", oSolicitantes.Correo);
                    cmd.Parameters.AddWithValue("Activo", oSolicitantes.Activo);
                    cmd.Parameters.Add("Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;
                    oConexion.Open();
                    cmd.ExecuteNonQuery();
                    respuesta = Convert.ToBoolean(cmd.Parameters["Resultado"].Value);
                }
                catch (Exception ex)
                {
                    string Queerror = "fggggggggggh" + ex;
                    respuesta = false;
                }
            }

            return respuesta;
        }
        public bool EliminarSolicitante(string Cedula_Solicitante)
        {
            bool respuesta = true;

            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("usp_EliminarSolicitante", oConexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Parámetro de entrada: Cedula
                    cmd.Parameters.AddWithValue("@Cedula_Solicitante", Cedula_Solicitante);
                    // Parámetro de salida: Resultado
                    SqlParameter resultadoParam = new SqlParameter("@Resultado", SqlDbType.Bit);
                    resultadoParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(resultadoParam);
                    oConexion.Open();
                    cmd.ExecuteNonQuery();
                    // Obtener el resultado
                    respuesta = Convert.ToBoolean(resultadoParam.Value);
                }
                catch (Exception ex)
                {
                    string repuest = "kkkkkkkkkkkkkk" + ex;
                    respuesta = false;
                }
            }
            return respuesta;
        }
    }
}
