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
    public class CD_Departamentos
    {
        public static CD_Departamentos _instancia = null;

        private CD_Departamentos()
        {

        }
        public static CD_Departamentos Instancia
        {
            get
            {
                if (_instancia == null)
                {
                    _instancia = new CD_Departamentos();
                }
                return _instancia;
            }
        }
        public List<Departamentos> ObtenerDepartamentos(bool? activo = null)
        {
            List<Departamentos> listaDepartamentos = new List<Departamentos>();
            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                // Utilizamos el procedimiento almacenado correcto
                SqlCommand cmd = new SqlCommand("usp_ObtenerDepartamentos", oConexion);
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
                        listaDepartamentos.Add(new Departamentos()
                        {
                            Id_Departamento = Convert.ToInt32(dr["Id_Departamento"]),
                            Cod_Departamento = Convert.ToInt32(dr["Cod_Departamento"]),
                            Descripcion = dr["Descripcion"].ToString(),
                            Activo = Convert.ToBoolean(dr["Activo"]),
                            FechaRegistro = Convert.ToDateTime(dr["FechaRegistro"])
                        });
                    }
                    dr.Close();

                    return listaDepartamentos;
                }
                catch (Exception ex)
                {
                    // Manejo de excepciones: opcionalmente puedes loguear el error
                    listaDepartamentos = null;
                    return listaDepartamentos;
                }
            }
        }
        public bool RegistrarDepartamento(Departamentos oDepartamento)
        {
            bool respuesta = true;

            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("usp_RegistrarDepartamento", oConexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Descripcion", oDepartamento.Descripcion);
                    cmd.Parameters.AddWithValue("@Activo", oDepartamento.Activo);
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


        public bool ModificarDepartamento(Departamentos oDepartamento)
        {
            bool respuesta = true;

            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("usp_ModificarDepartamento", oConexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id_Departamento", oDepartamento.Id_Departamento);
                    cmd.Parameters.AddWithValue("@Descripcion", oDepartamento.Descripcion);
                    cmd.Parameters.AddWithValue("@Activo", oDepartamento.Activo);
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
        public bool EliminarDepartamento(int codDepartamento)
        {
            bool respuesta = true;

            using (SqlConnection oConexion = new SqlConnection(Conexion.CN))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("usp_EliminarDepartamentos", oConexion);
                    cmd.Parameters.AddWithValue("@Cod_Departamento", codDepartamento);
                    cmd.Parameters.Add("@Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Abrir conexión
                    oConexion.Open();

                    // Ejecutar el procedimiento almacenado
                    cmd.ExecuteNonQuery();

                    // Obtener el resultado del parámetro de salida
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
