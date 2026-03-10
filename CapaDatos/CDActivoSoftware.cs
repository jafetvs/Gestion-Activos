using CapaModelo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class CDActivoSoftware
    {
        private static CDActivoSoftware _instancia;
        private CDActivoSoftware() { }
        public static CDActivoSoftware Instancia
        {
            get
            {
                if (_instancia == null)
                    _instancia = new CDActivoSoftware();
                return _instancia;
            }
        }

        // -------------------------- Helpers --------------------------
        private static object DbNull(object v) => v ?? DBNull.Value;

        // ------------------- Listar / Filtrar por activo -------------
        // Filtros: @Desde/@Hasta (sobre FechaInstalacion) y @Texto (Nombre/Editor)
        public List<ActivoSoftware> ListarPorActivo(
            int idActivo,
            DateTime? desde = null,
            DateTime? hasta = null,
            string texto = null)
        {
            var lista = new List<ActivoSoftware>();

            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Software_ListarPorActivo", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdActivo", idActivo);
                cmd.Parameters.Add(new SqlParameter("@Desde", SqlDbType.Date) { Value = (object)desde ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@Hasta", SqlDbType.Date) { Value = (object)hasta ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@Texto", SqlDbType.NVarChar, 150) { Value = (object)texto ?? DBNull.Value });

                try
                {
                    cn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ActivoSoftware
                            {
                                IdSoftware = SqlUtils.GetInt(dr, "IdSoftware"),
                                IdActivo = SqlUtils.GetInt(dr, "IdActivo"),
                                Nombre = SqlUtils.GetString(dr, "Nombre"),
                                Editor = SqlUtils.GetString(dr, "Editor"),
                                FechaInstalacion = SqlUtils.GetNullableDate(dr, "FechaInstalacion"),
                                Tamano = SqlUtils.GetString(dr, "Tamano"),
                                Version = SqlUtils.GetString(dr, "Version"),
                                IdUsuarioCrea = SqlUtils.GetInt(dr, "IdUsuarioCrea"),
                                FechaRegistro = SqlUtils.GetDate(dr, "FechaRegistro")
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    lista = new List<ActivoSoftware>();
                }
            }

            return lista;
        }

        // --------------------------- Registrar ------------------------
        // Requiere: IdActivo, Nombre, IdUsuarioCrea
        // Opcionales: Editor, FechaInstalacion, Tamano, Version
        public (int idGenerado, bool ok) Registrar(ActivoSoftware s)
        {
            int id = 0; bool ok = false;

            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Software_Agregar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdActivo", s.IdActivo);
                cmd.Parameters.AddWithValue("@Nombre", s.Nombre);
                cmd.Parameters.AddWithValue("@Editor", DbNull(s.Editor));
                cmd.Parameters.AddWithValue("@FechaInstalacion", (object)s.FechaInstalacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Tamano", DbNull(s.Tamano));
                cmd.Parameters.AddWithValue("@Version", DbNull(s.Version));
                cmd.Parameters.AddWithValue("@IdUsuarioCrea", s.IdUsuarioCrea);

                var pId = new SqlParameter("@IdGenerado", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var pOk = new SqlParameter("@Resultado", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(pId);
                cmd.Parameters.Add(pOk);

                try
                {
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    ok = (pOk.Value != DBNull.Value) && Convert.ToBoolean(pOk.Value);
                    if (ok && pId.Value != DBNull.Value) id = Convert.ToInt32(pId.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    id = 0; ok = false;
                }
            }

            return (id, ok);
        }

        // ----------------------------- Editar -------------------------
        // No modifica IdUsuarioCrea ni FechaRegistro (auditoría)
        public bool Editar(ActivoSoftware s)
        {
            bool ok = false;

            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Software_Editar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdSoftware", s.IdSoftware);
                cmd.Parameters.AddWithValue("@IdActivo", s.IdActivo);
                cmd.Parameters.AddWithValue("@Nombre", s.Nombre);
                cmd.Parameters.AddWithValue("@Editor", DbNull(s.Editor));
                cmd.Parameters.AddWithValue("@FechaInstalacion", (object)s.FechaInstalacion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Tamano", DbNull(s.Tamano));
                cmd.Parameters.AddWithValue("@Version", DbNull(s.Version));

                var pOk = new SqlParameter("@Resultado", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(pOk);

                try
                {
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    ok = (pOk.Value != DBNull.Value) && Convert.ToBoolean(pOk.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    ok = false;
                }
            }

            return ok;
        }

        // ---------------------------- Eliminar ------------------------
        // Firma alineada al SP: requiere IdSoftware + IdActivo
        public bool Eliminar(int idSoftware, int idActivo)
        {
            bool ok = false;

            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Software_Eliminar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdSoftware", idSoftware);
                cmd.Parameters.AddWithValue("@IdActivo", idActivo);

                var pOk = new SqlParameter("@Resultado", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(pOk);

                try
                {
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    ok = (pOk.Value != DBNull.Value) && Convert.ToBoolean(pOk.Value);
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
