using CapaModelo;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;

namespace CapaDatos
{
    public class CD_Usuario
    {
        public static CD_Usuario _instancia = null;

        private CD_Usuario()
        {

        }
        public static CD_Usuario Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new CD_Usuario();
                }
                return _instancia;
            }
        }
        public List<Usuario> ObtenerUsuarios()
        {
            List<Usuario> rptListaUsuario = new List<Usuario>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                SqlCommand cmd = new SqlCommand("usp_ObtenerUsuario", oConexion);
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        rptListaUsuario.Add(new Usuario()
                        {
                            IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                            Cedula_Usuario = dr["Cedula_Usuario"].ToString(),
                            TipoDocumento = dr["TipoDocumento"].ToString(),
                            Nom_Rol = dr["Nom_Rol"].ToString(),
                            Nom_Completo = dr["Nom_Completo"].ToString(),
                            Nom_User = dr["Nom_User"].ToString(),
                            Direccion = dr["Direccion"] != DBNull.Value ? dr["Direccion"].ToString() : null,
                            Telefono1 = dr["Telefono1"] != DBNull.Value ? (int?)Convert.ToInt32(dr["Telefono1"]) : null,
                            Telefono2 = dr["Telefono2"] != DBNull.Value ? (int?)Convert.ToInt32(dr["Telefono2"]) : null,
                            Fax = dr["Fax"] != DBNull.Value ? (int?)Convert.ToInt32(dr["Fax"]) : null,
                            Correo = dr["Correo"].ToString(),
                            Clave = dr["Clave"].ToString(),
                            IdRol = Convert.ToInt32(dr["IdRol"]),
                            Activo = Convert.ToBoolean(dr["Activo"]),
                            FechaRegistro = Convert.ToDateTime(dr["FechaRegistro"]),

                            oRol = new Rol()
                            {
                                Descripcion = dr["DescripcionRol"].ToString()
                            }
                        });
                    }

                    dr.Close();
                    return rptListaUsuario;
                }
                catch (Exception ex)
                {
                    rptListaUsuario = null;
                    return rptListaUsuario;
                }
            }
        }
        public Usuario ObtenerUnUsuario(string Cedula_Usuario)
        {
            Usuario rptUsuario = new Usuario();

            string query = @"SELECT IdUsuario, Cedula_Usuario, Nom_Rol, Nom_Completo, Nom_User, Direccion, 
                            Telefono1, Telefono2, Fax, Correo, Clave, IdRol, Activo, 
                            Codigo_Recuperacion, Fecha_Expiracion_Codigo, FechaRegistro 
                     FROM INGRESO WHERE Cedula_Usuario = @Cedula_Usuario";
    
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                SqlCommand cmd = new SqlCommand(query, oConexion);
                cmd.Parameters.AddWithValue("@Cedula_Usuario", Cedula_Usuario);

                try
                {
                    oConexion.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                rptUsuario.IdUsuario = reader.GetInt32(reader.GetOrdinal("IdUsuario"));
                                rptUsuario.Cedula_Usuario = reader["Cedula_Usuario"].ToString();
                                rptUsuario.Nom_Rol = reader["Nom_Rol"].ToString();
                                rptUsuario.Nom_Completo = reader["Nom_Completo"].ToString();
                                rptUsuario.Nom_User = reader["Nom_User"].ToString();
                                rptUsuario.Direccion = reader["Direccion"] != DBNull.Value ? reader["Direccion"].ToString() : null;
                                rptUsuario.Telefono1 = reader["Telefono1"] != DBNull.Value ? (int?)reader.GetInt32(reader.GetOrdinal("Telefono1")) : null;
                                rptUsuario.Telefono2 = reader["Telefono2"] != DBNull.Value ? (int?)reader.GetInt32(reader.GetOrdinal("Telefono2")) : null;
                                rptUsuario.Fax = reader["Fax"] != DBNull.Value ? (int?)reader.GetInt32(reader.GetOrdinal("Fax")) : null;
                                rptUsuario.Correo = reader["Correo"].ToString();
                                rptUsuario.Clave = reader["Clave"].ToString();
                                rptUsuario.IdRol = reader.GetInt32(reader.GetOrdinal("IdRol"));
                                rptUsuario.Activo = reader.GetBoolean(reader.GetOrdinal("Activo"));
                                rptUsuario.FechaRegistro = reader.GetDateTime(reader.GetOrdinal("FechaRegistro"));
                            }
                        }
                        else
                        {
                            rptUsuario = null;
                        }
                    }

                    return rptUsuario;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return null;
                }
            }
        }
      /*  public Usuario ObtenerUnUsuario(string Cedula_Usuario)
        {
            Usuario rptUsuario = new Usuario();

            // Definir la consulta SQL para obtener los datos de la tabla INGRESO
            string query = "SELECT Cedula_Usuario, Nom_User FROM INGRESO WHERE Cedula_Usuario = @Cedula_Usuario";

            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                // Usamos SqlCommand con una consulta SQL en lugar de un procedimiento almacenado
                SqlCommand cmd = new SqlCommand(query, oConexion);
                cmd.Parameters.AddWithValue("@Cedula_Usuario", Cedula_Usuario);

                try
                {
                    // Abrimos la conexión a la base de datos
                    oConexion.Open();

                    // Ejecutamos la consulta y obtenemos los datos
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            // Leemos los resultados y asignamos los valores a la variable rptUsuario
                            while (reader.Read())
                            {
                                if (!reader.IsDBNull(reader.GetOrdinal("Cedula_Usuario")))
                                {
                                    rptUsuario.Cedula_Usuario = reader.GetString(reader.GetOrdinal("Cedula_Usuario"));

                                }

                                if (!reader.IsDBNull(reader.GetOrdinal("Nom_User")))
                                {
                                    rptUsuario.Nom_User = reader.GetString(reader.GetOrdinal("Nom_User"));
                                }
                            }
                        }
                        else
                        {
                            // Si no hay filas, retornamos null o un objeto con valores predeterminados
                            rptUsuario = null;
                        }
                    }

                    return rptUsuario;
                }
                catch (Exception ex)
                {
                    // En caso de error, puedes registrar el error o simplemente devolver null
                    // Asegúrate de loguear o manejar el error según sea necesario
                    rptUsuario = null;
                    Console.WriteLine($"Error: {ex.Message}"); // Aquí podrías loguear el error
                    return rptUsuario;
                }
            }
        }*/


        //DETALLE DE USUARIOS 
        public Usuario ObtenerDetalleUsuario(int IdUsuario)
        {
            Usuario rptUsuario = new Usuario();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                SqlCommand cmd = new SqlCommand("usp_ObDetalleUsuario", oConexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdUsuario", IdUsuario);

                try
                {
                    oConexion.Open();
                    using (XmlReader dr = cmd.ExecuteXmlReader())
                    {
                        while (dr.Read())
                        {
                            XDocument doc = XDocument.Load(dr);
                            if (doc.Element("Usuario") != null)
                            {
                                rptUsuario = (from dato in doc.Elements("Usuario")
                                              select new Usuario()
                                              {
                                                  IdUsuario = int.Parse(dato.Element("IdUsuario").Value),
                                                  //TipoDocumento= dato.Element("TipoDocumento").Value,
                                                  Cedula_Usuario = dato.Element("Cedula_Usuario").Value,
                                                  Nom_Rol = dato.Element("Nom_Rol").Value,
                                                  Nom_User = dato.Element("Nom_User").Value,
                                                  Correo = dato.Element("Correo").Value,
                                                  Clave = dato.Element("Clave").Value,
                                                  Direccion = dato.Element("Direccion") != null ? dato.Element("Direccion").Value : null,
                                                  Telefono1 = dato.Element("Telefono1") != null && !string.IsNullOrEmpty(dato.Element("Telefono1").Value) ? (int?)int.Parse(dato.Element("Telefono1").Value) : null,
                                                  Telefono2 = dato.Element("Telefono2") != null && !string.IsNullOrEmpty(dato.Element("Telefono2").Value) ? (int?)int.Parse(dato.Element("Telefono2").Value) : null,
                                                  Fax = dato.Element("Fax") != null && !string.IsNullOrEmpty(dato.Element("Fax").Value) ? (int?)int.Parse(dato.Element("Fax").Value) : null

                                              }).FirstOrDefault();

                                rptUsuario.oRol = (from dato in doc.Element("Usuario").Elements("DetalleRol")
                                                   select new Rol()
                                                   {
                                                       Descripcion = dato.Element("Descripcion").Value
                                                   }).FirstOrDefault();

                                rptUsuario.oListaMenu = (from menu in doc.Element("Usuario").Element("DetalleMenu").Elements("Menu")
                                                         select new Menu()
                                                         {
                                                             Nombre = menu.Element("NombreMenu").Value,
                                                             Icono = menu.Element("Icono").Value,
                                                             oSubMenu = (from submenu in menu.Element("DetalleSubMenu").Elements("SubMenu")
                                                                         select new SubMenu()
                                                                         {
                                                                             Nombre = submenu.Element("NombreSubMenu").Value,
                                                                             Controlador = submenu.Element("Controlador").Value,
                                                                             Vista = submenu.Element("Vista").Value,
                                                                             Icono = submenu.Element("Icono").Value,
                                                                             Activo = submenu.Element("Activo").Value == "1"
                                                                         }).ToList()
                                                         }).ToList();
                            }
                            else
                            {
                                rptUsuario = null;
                            }
                        }
                        dr.Close();
                    }
                    return rptUsuario;
                }
                catch (Exception ex)
                {
                    rptUsuario = null;
                    return rptUsuario;
                }
            }
        }
        public bool RegistrarUsuario(Usuario oUsuario)
        {
            bool respuesta = true;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("usp_RegistrarUsuario", oConexion);
                    cmd.Parameters.AddWithValue("Cedula_Usuario", oUsuario.Cedula_Usuario);
                    cmd.Parameters.AddWithValue("Nom_Rol", oUsuario.Nom_Rol);
                    cmd.Parameters.AddWithValue("Nom_Completo", oUsuario.Nom_Completo);
                    cmd.Parameters.AddWithValue("Nom_User", oUsuario.Nom_User);
                    cmd.Parameters.AddWithValue("Direccion", oUsuario.Direccion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("Telefono1", oUsuario.Telefono1 == 0 ? (object)DBNull.Value : oUsuario.Telefono1);
                    cmd.Parameters.AddWithValue("Telefono2", oUsuario.Telefono2 == 0 ? (object)DBNull.Value : oUsuario.Telefono2);
                    cmd.Parameters.AddWithValue("Fax", oUsuario.Fax == 0 ? (object)DBNull.Value : oUsuario.Fax);
                    cmd.Parameters.AddWithValue("Correo", oUsuario.Correo);
                    cmd.Parameters.AddWithValue("Clave", oUsuario.Clave);
                    cmd.Parameters.AddWithValue("IdRol", oUsuario.IdRol);
                    cmd.Parameters.AddWithValue("Activo", oUsuario.Activo);
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

        public bool ModificarUsuario(Usuario oUsuario)
        {
            bool respuesta = true;

            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("usp_ModificarUsuario", oConexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Agregar parámetros con tipos explícitos
                    cmd.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = oUsuario.IdUsuario;
                    cmd.Parameters.Add("@Cedula_Usuario", SqlDbType.Int).Value = oUsuario.Cedula_Usuario;
                    cmd.Parameters.Add("@Nom_Rol", SqlDbType.VarChar, 60).Value = oUsuario.Nom_Rol;
                    cmd.Parameters.Add("@Nom_User", SqlDbType.VarChar, 50).Value = oUsuario.Nom_User;
                    cmd.Parameters.Add("@Nom_Completo", SqlDbType.VarChar, 100).Value = oUsuario.Nom_Completo;
                    cmd.Parameters.Add("@Direccion", SqlDbType.VarChar, 50).Value = string.IsNullOrEmpty(oUsuario.Direccion) ? (object)DBNull.Value : oUsuario.Direccion;
                    cmd.Parameters.Add("@Telefono1", SqlDbType.Int).Value = oUsuario.Telefono1 == 0 ? (object)DBNull.Value : oUsuario.Telefono1;
                    cmd.Parameters.Add("@Telefono2", SqlDbType.Int).Value = oUsuario.Telefono2 == 0 ? (object)DBNull.Value : oUsuario.Telefono2;
                    cmd.Parameters.Add("@Fax", SqlDbType.Int).Value = oUsuario.Fax == 0 ? (object)DBNull.Value : oUsuario.Fax;
                    cmd.Parameters.Add("@Correo", SqlDbType.VarChar, 60).Value = oUsuario.Correo;
                    cmd.Parameters.Add("@Clave", SqlDbType.VarChar, 250).Value = string.IsNullOrEmpty(oUsuario.Clave) ? (object)DBNull.Value : oUsuario.Clave;
                    cmd.Parameters.Add("@IdRol", SqlDbType.Int).Value = oUsuario.IdRol;
                    cmd.Parameters.Add("@Activo", SqlDbType.Bit).Value = oUsuario.Activo;
                    cmd.Parameters.Add("@Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;

                    oConexion.Open();
                    cmd.ExecuteNonQuery();

                    respuesta = Convert.ToBoolean(cmd.Parameters["@Resultado"].Value);
                }
                catch (Exception ex)
                {
                    // Aquí puedes registrar ex.Message en un log
                    respuesta = false;
                }
            }

            return respuesta;
        }

        public bool EliminarUsuario(string Cedula_Usuario)
        {
            bool respuesta = true;

            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("usp_EliminarUsuario", oConexion);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Parámetro de entrada: Cedula
                    cmd.Parameters.AddWithValue("@Cedula_Usuario", Cedula_Usuario);

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
