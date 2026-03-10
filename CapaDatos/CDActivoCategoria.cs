using CapaModelo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDatos
{
    public class CDActivoCategoria
    {
        private static CDActivoCategoria _instancia = null;
        private CDActivoCategoria() { }
        public static CDActivoCategoria Instancia
        {
            get
            {
                if (_instancia == null)
                    _instancia = new CDActivoCategoria();
                return _instancia;
            }
        }

        public List<ActivoCategoria> Listar()
        {
            var lista = new List<ActivoCategoria>();
            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Categoria_Listar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                try
                {
                    cn.Open();
                    using (var dr = cmd.ExecuteReader())
                        while (dr.Read())
                            lista.Add(new ActivoCategoria
                            {
                                IdCategoria = SqlUtils.GetInt(dr, "IdCategoria"),
                                Nombre = SqlUtils.GetString(dr, "Nombre"),
                                Activo = SqlUtils.GetBool(dr, "Activo"),
                                FechaRegistro = SqlUtils.GetDate(dr, "FechaRegistro")
                            });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    lista = new List<ActivoCategoria>();
                }
            }
            return lista;
        }

        public (int idGenerado, bool ok) Registrar(ActivoCategoria obj)
        {
            int id = 0; bool ok = false;
            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Categoria_Registrar", cn))
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

        public bool Modificar(ActivoCategoria obj)
        {
            bool ok = false;
            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Categoria_Modificar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdCategoria", obj.IdCategoria);
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

        public bool Eliminar(int idCategoria)
        {
            bool ok = false;
            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Categoria_Eliminar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdCategoria", idCategoria);

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
