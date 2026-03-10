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
    public class CD_Procedencias
    {
        public static CD_Procedencias _instancia = null;

        private CD_Procedencias()
        {
        }
        public static CD_Procedencias Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new CD_Procedencias();
                }
                return _instancia;
            }
        }
        public List<Procedencia> Procedencias(bool? activo = null)
        {
            List<Procedencia> listProcedenciaActiva = new List<Procedencia>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                SqlCommand cmd = new SqlCommand("usp_ObtenerProcedencias", oConexion);
                cmd.CommandType = CommandType.StoredProcedure;
                // Agregar el parámetro opcional @Activo si tiene un valor
                if (activo.HasValue)
                {
                    cmd.Parameters.AddWithValue("@Activo", activo.Value);
                }
                try
                {
                    oConexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        listProcedenciaActiva.Add(new Procedencia()
                        {
                            Id_Procedencia = Convert.ToInt32(dr["Id_Procedencia"]),
                            Cod_Procedencia = Convert.ToInt32(dr["Cod_Procedencia"]),
                            Descripcion = dr["Descripcion"].ToString(),
                            Activo = Convert.ToBoolean(dr["Activo"]),
                            FechaRegistro = Convert.ToDateTime(dr["FechaRegistro"])
                        });
                    }
                    dr.Close();
                    return listProcedenciaActiva;
                }
                catch (Exception ex)
                {
                    listProcedenciaActiva = null;
                    return listProcedenciaActiva;
                }
            }
        }
        public bool RegistrarProcedencia(Procedencia oProcedencia)
        {
            bool respuesta = true;

            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("usp_RegistrarProcedencia", oConexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Descripcion", oProcedencia.Descripcion);
                    cmd.Parameters.AddWithValue("@Activo", oProcedencia.Activo);
                    // Parámetro de salida para el resultado
                    cmd.Parameters.Add("@Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;

                    // Abrir conexión
                    oConexion.Open();

                    // Ejecutar el procedimiento almacenado
                    cmd.ExecuteNonQuery();

                    // Obtener el resultado del parámetro de salida
                    respuesta = Convert.ToBoolean(cmd.Parameters["@Resultado"].Value);
                }
                catch (Exception ex)
                {
                    // Manejar excepciones (puedes loguearlas si es necesario)
                    respuesta = false;
                }
            }
            return respuesta;
        }
        public bool ModificarProcedencia(Procedencia oProcedencia)
        {
            bool respuesta = true;

            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("usp_ModificarProcedencia", oConexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Procedencia", oProcedencia.Id_Procedencia);
                    cmd.Parameters.AddWithValue("@Descripcion", oProcedencia.Descripcion);
                    cmd.Parameters.AddWithValue("@Activo", oProcedencia.Activo);
                    // Parámetro de salida para el resultado
                    cmd.Parameters.Add("@Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;

                    // Abrir conexión
                    oConexion.Open();

                    // Ejecutar el procedimiento almacenado
                    cmd.ExecuteNonQuery();

                    // Obtener el resultado del parámetro de salida
                    respuesta = Convert.ToBoolean(cmd.Parameters["@Resultado"].Value);
                }
                catch (Exception ex)
                {
                    // Manejar excepciones (puedes loguearlas si es necesario)
                    respuesta = false;
                }
            }
            return respuesta;
        }

        public bool EliminarProcedencia(int codProcedencia)
        {
            bool respuesta = true;
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("usp_EliminarProcedencia", oConexion);
                    cmd.Parameters.AddWithValue("@Cod_Procedencia", codProcedencia);
                    cmd.Parameters.Add("@Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oConexion.Open();

                    cmd.ExecuteNonQuery();

                    // Obtener el valor de salida del procedimiento almacenado
                    respuesta = Convert.ToBoolean(cmd.Parameters["@Resultado"].Value);
                }
                catch (Exception ex)
                {
                    // Manejar el error (puedes registrar el error si es necesario)
                    respuesta = false;
                }
            }

            return respuesta;
        }

    }
}
