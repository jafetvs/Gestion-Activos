using CapaModelo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class CDActivoClase
    {
        private static CDActivoClase _instancia = null;
        private CDActivoClase() { }
        public static CDActivoClase Instancia
        {
            get
            {
                if (_instancia == null)
                    _instancia = new CDActivoClase();
                return _instancia;
            }
        }

        public List<ActivoClase> Listar()
        {
            var lista = new List<ActivoClase>();
            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Clase_Listar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                try
                {
                    cn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ActivoClase
                            {
                                IdClase = SqlUtils.GetInt(dr, "IdClase"),
                                Nombre = SqlUtils.GetString(dr, "Nombre"),
                                Activo = SqlUtils.GetBool(dr, "Activo"),
                                FechaRegistro = SqlUtils.GetDate(dr, "FechaRegistro")
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    lista = new List<ActivoClase>();
                }
            }
            return lista;
        }

        public (int idGenerado, bool ok) Registrar(ActivoClase obj)
        {
            int id = 0; bool ok = false;
            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Clase_Registrar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Nombre", obj.Nombre);
                cmd.Parameters.AddWithValue("@Activo", obj.Activo);

                var pId = new SqlParameter("@IdGenerado", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var pOk = new SqlParameter("@Resultado", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(pId);
                cmd.Parameters.Add(pOk);

                try
                {
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    ok = Convert.ToBoolean(pOk.Value);
                    if (ok) id = Convert.ToInt32(pId.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    id = 0; ok = false;
                }
            }
            return (id, ok);
        }

        public bool Modificar(ActivoClase obj)
        {
            bool ok = false;
            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Clase_Modificar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdClase", obj.IdClase);
                cmd.Parameters.AddWithValue("@Nombre", obj.Nombre);
                cmd.Parameters.AddWithValue("@Activo", obj.Activo);

                var pOk = new SqlParameter("@Resultado", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(pOk);

                try
                {
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    ok = Convert.ToBoolean(pOk.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    ok = false;
                }
            }
            return ok;
        }

        public bool Eliminar(int idClase)
        {
            bool ok = false;
            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Clase_Eliminar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdClase", idClase);

                var pOk = new SqlParameter("@Resultado", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(pOk);

                try
                {
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    ok = Convert.ToBoolean(pOk.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    ok = false;
                }
            }
            return ok;
        }
    }
}
