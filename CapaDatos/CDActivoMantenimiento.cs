using CapaModelo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class CDActivoMantenimiento
    {
        private static CDActivoMantenimiento _instancia;
        private CDActivoMantenimiento() { }

        public static CDActivoMantenimiento Instancia
        {
            get
            {
                if (_instancia == null)
                    _instancia = new CDActivoMantenimiento();
                return _instancia;
            }
        }

        private static object DbNull(object v) => v ?? DBNull.Value;

        // -------------------- Listar / Filtrar --------------------
        public List<ActivoMantenimiento> ListarPorActivo(
            int idActivo,
            DateTime? desde = null,
            DateTime? hasta = null,
            string texto = null,
            string tipo = null)
        {
            var lista = new List<ActivoMantenimiento>();

            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Mantenimiento_ListarPorActivo", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdActivo", idActivo);
                cmd.Parameters.Add(new SqlParameter("@Desde", SqlDbType.Date) { Value = (object)desde ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@Hasta", SqlDbType.Date) { Value = (object)hasta ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@Texto", SqlDbType.NVarChar, 150) { Value = (object)texto ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@Tipo", SqlDbType.NVarChar, 50) { Value = (object)tipo ?? DBNull.Value });

                try
                {
                    cn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ActivoMantenimiento
                            {
                                IdMantenimiento = SqlUtils.GetInt(dr, "IdMantenimiento"),
                                IdActivo = SqlUtils.GetInt(dr, "IdActivo"),
                                IdSolicitante = SqlUtils.GetInt(dr, "IdSolicitante"),
                                // OJO: en BD se llama 'Fecha' -> en el modelo es 'FechaMantenimiento'
                                FechaMantenimiento = SqlUtils.GetDate(dr, "FechaMantenimiento"),
                                Tipo = SqlUtils.GetString(dr, "Tipo"),
                                Detalle = SqlUtils.GetString(dr, "Detalle"),
                                IdUsuarioCrea = SqlUtils.GetInt(dr, "IdUsuarioCrea"),
                                FechaRegistro = SqlUtils.GetDate(dr, "FechaRegistro"),

                                // Enriquecidos por el SP (JOIN a SOLICITANTE)
                                ResponsableNombre = SqlUtils.GetString(dr, "ResponsableNombre"),
                                ResponsableCedula = SqlUtils.GetString(dr, "ResponsableCedula")
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    lista = new List<ActivoMantenimiento>();
                }
            }

            return lista;
        }

        // ------------------------ Registrar ------------------------
        // Firma del SP:
        //  @IdActivo, @IdSolicitante, @Tipo, @Detalle, @Fecha, @IdUsuarioCrea, @IdGenerado OUT, @Resultado OUT
        public (int idGenerado, bool ok) Registrar(ActivoMantenimiento m)
        {
            int id = 0; bool ok = false;

            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Mantenimiento_Registrar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@IdActivo", m.IdActivo);
                cmd.Parameters.AddWithValue("@IdSolicitante", m.IdSolicitante);
                cmd.Parameters.AddWithValue("@Tipo", m.Tipo); // string tal cual
                cmd.Parameters.AddWithValue("@Detalle", DbNull(m.Detalle));
                cmd.Parameters.Add(new SqlParameter("@FechaMantenimiento", SqlDbType.Date)
                {
                    // Si permites NULL en el SP, manda DBNull cuando venga default
                    Value = (m.FechaMantenimiento == default(DateTime)) ? (object)DBNull.Value : m.FechaMantenimiento
                });
                cmd.Parameters.AddWithValue("@IdUsuarioCrea", m.IdUsuarioCrea);

                var pId = new SqlParameter("@IdGenerado", SqlDbType.Int) { Direction = ParameterDirection.Output };
                var pOk = new SqlParameter("@Resultado", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(pId);
                cmd.Parameters.Add(pOk);

                try
                {
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    ok = pOk.Value != DBNull.Value && Convert.ToBoolean(pOk.Value);
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

        // ------------------------- Editar --------------------------
        // Firma del SP:
        //  @IdMantenimiento, @IdActivo, @IdSolicitante, @Tipo, @Detalle, @Fecha, @Resultado OUT
        public bool Editar(ActivoMantenimiento m)
        {
            bool ok = false;

            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Mantenimiento_Editar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@IdMantenimiento", m.IdMantenimiento);
                cmd.Parameters.AddWithValue("@IdActivo", m.IdActivo);
                cmd.Parameters.AddWithValue("@IdSolicitante", m.IdSolicitante);
                cmd.Parameters.AddWithValue("@Tipo", m.Tipo);
                cmd.Parameters.AddWithValue("@Detalle", DbNull(m.Detalle));
                cmd.Parameters.Add(new SqlParameter("@FechaMantenimiento", SqlDbType.Date)
                {
                    Value = (m.FechaMantenimiento == default(DateTime)) ? (object)DBNull.Value : m.FechaMantenimiento
                });

                var pOk = new SqlParameter("@Resultado", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(pOk);

                try
                {
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    ok = pOk.Value != DBNull.Value && Convert.ToBoolean(pOk.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    ok = false;
                }
            }

            return ok;
        }

        // ------------------------ Eliminar -------------------------
        // Firma del SP:
        //  @IdMantenimiento, @IdActivo, @Resultado OUT
        public bool Eliminar(int idMantenimiento, int idActivo)
        {
            bool ok = false;

            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Mantenimiento_Eliminar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@IdMantenimiento", idMantenimiento);
                cmd.Parameters.AddWithValue("@IdActivo", idActivo);

                var pOk = new SqlParameter("@Resultado", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                cmd.Parameters.Add(pOk);

                try
                {
                    cn.Open();
                    cmd.ExecuteNonQuery();
                    ok = pOk.Value != DBNull.Value && Convert.ToBoolean(pOk.Value);
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
