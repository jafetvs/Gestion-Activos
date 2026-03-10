using CapaModelo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class CDActivoMovimiento
    {
        private static CDActivoMovimiento _instancia;
        private CDActivoMovimiento() { }
        public static CDActivoMovimiento Instancia
        {
            get
            {
                if (_instancia == null)
                    _instancia = new CDActivoMovimiento();
                return _instancia;
            }
        }

        // -------------------------- Helpers --------------------------

        private static MovimientoTipo ParseTipo(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return default;
            return Enum.TryParse(s, true, out MovimientoTipo val) ? val : default;
        }

        private static object DbNullIf<T>(T value) => (object)value ?? DBNull.Value;

        // ----------------------- Listar / Filtrar ---------------------
        // Lee ResponsableNombre / ResponsableCedula devueltos por el SP,
        // y mapea IdSolicitante (responsable del movimiento).
        public List<ActivoMovimiento> ListarPorActivo(
            int idActivo,
            DateTime? desde = null,
            DateTime? hasta = null,
            string texto = null)
        {
            var lista = new List<ActivoMovimiento>();

            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Movimiento_ListarPorActivo", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdActivo", idActivo);

                cmd.Parameters.Add(new SqlParameter("@Desde", SqlDbType.Date) { Value = (object)desde ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@Hasta", SqlDbType.Date) { Value = (object)hasta ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@Texto", SqlDbType.NVarChar, 100) { Value = (object)texto ?? DBNull.Value });

                try
                {
                    cn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var mov = new ActivoMovimiento
                            {
                                IdMovimiento = SqlUtils.GetInt(dr, "IdMovimiento"),
                                IdActivo = SqlUtils.GetInt(dr, "IdActivo"),
                                FechaMovimiento = SqlUtils.GetDate(dr, "FechaMovimiento"),
                                TipoMovimiento = ParseTipo(SqlUtils.GetString(dr, "TipoMovimiento")),
                                Detalle = SqlUtils.GetString(dr, "Detalle"),
                                IdUsuario = SqlUtils.GetInt(dr, "IdUsuario"),

                                // Responsable del movimiento
                                IdSolicitante = SqlUtils.GetInt(dr, "IdSolicitante"),
                                ResponsableNombre = SqlUtils.GetString(dr, "ResponsableNombre"),
                                ResponsableCedula = SqlUtils.GetString(dr, "ResponsableCedula")
                            };
                            lista.Add(mov);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    lista = new List<ActivoMovimiento>();
                }
            }

            return lista;
        }

        // --------------------------- Registrar ------------------------
        // Usa @IdSolicitante como responsable OBLIGATORIO y @FechaMovimiento opcional.
        public (int idGenerado, bool ok) Registrar(ActivoMovimiento m)
        {
            int id = 0; bool ok = false;

            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Movimiento_Registrar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdActivo", m.IdActivo);
                cmd.Parameters.AddWithValue("@IdSolicitante", m.IdSolicitante);
                cmd.Parameters.AddWithValue("@TipoMovimiento", m.TipoMovimiento.ToString());
                cmd.Parameters.AddWithValue("@Detalle", DbNullIf(m.Detalle));
                cmd.Parameters.AddWithValue("@IdUsuario", m.IdUsuario);
                cmd.Parameters.Add(new SqlParameter("@FechaMovimiento", SqlDbType.DateTime)
                {
                    Value = (m.FechaMovimiento == default(DateTime) ? (object)DBNull.Value : m.FechaMovimiento)
                });

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

        // ----------------------------- Editar -------------------------
        // Permite cambiar tipo, detalle, responsable y, opcionalmente, la fecha.
        public bool Editar(int idMovimiento, int idActivo,int idSolicitante, MovimientoTipo tipo, string detalle,  DateTime? fecha = null)
        {
            bool ok = false;

            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Movimiento_Editar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdMovimiento", idMovimiento);
                cmd.Parameters.AddWithValue("@IdActivo", idActivo);
                cmd.Parameters.AddWithValue("@IdSolicitante", idSolicitante);
                cmd.Parameters.AddWithValue("@TipoMovimiento", tipo.ToString());
                cmd.Parameters.AddWithValue("@Detalle", DbNullIf(detalle));
                //cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmd.Parameters.Add(new SqlParameter("@FechaMovimiento", SqlDbType.DateTime)
                {
                    Value = (object)fecha ?? DBNull.Value
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

        // ---------------------------- Eliminar ------------------------
        public bool Eliminar(int idMovimiento, int idActivo)
        {
            bool ok = false;

            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Movimiento_Eliminar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdMovimiento", idMovimiento);
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
