using CapaModelo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class CDActivoAdjunto
    {
        private static CDActivoAdjunto _instancia;
        private CDActivoAdjunto() { }
        public static CDActivoAdjunto Instancia
        {
            get
            {
                if (_instancia == null) _instancia = new CDActivoAdjunto();
                return _instancia;
            }
        }

        private static object DbNull(object v) => v ?? DBNull.Value;

        /* ===========================================================
           LISTAR (solo metadatos, SIN Contenido) con filtro opcional
           =========================================================== */
        public List<ActivoAdjunto> ListarPorActivo(int idActivo, string texto = null)
        {
            var lista = new List<ActivoAdjunto>();

            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Adjunto_ListarPorActivo", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdActivo", idActivo);
                cmd.Parameters.Add(new SqlParameter("@Texto", SqlDbType.NVarChar, 200)
                { Value = (object)texto ?? DBNull.Value });

                try
                {
                    cn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new ActivoAdjunto
                            {
                                IdAdjunto = SqlUtils.GetInt(dr, "IdAdjunto"),
                                IdActivo = SqlUtils.GetInt(dr, "IdActivo"),
                                NombreArchivo = SqlUtils.GetString(dr, "NombreArchivo"),
                                ContentType = SqlUtils.GetString(dr, "ContentType"),
                                TamanoBytes = SqlUtils.GetNullableLong(dr, "TamanoBytes"),
                                IdUsuarioCrea = SqlUtils.GetInt(dr, "IdUsuarioCrea"),
                                FechaRegistro = SqlUtils.GetDate(dr, "FechaRegistro")
                                // Contenido NO se lee aquí (para no traer binarios pesados)
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    lista = new List<ActivoAdjunto>();
                }
            }

            return lista;
        }

        /* ===========================================================
           SUBIR / AGREGAR (incluye binario)
           =========================================================== */
        public (int idGenerado, bool ok) Subir(ActivoAdjunto a)
        {
            int id = 0; bool ok = false;

            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Adjunto_Subir", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@IdActivo", a.IdActivo);
                cmd.Parameters.AddWithValue("@NombreArchivo", a.NombreArchivo ?? string.Empty);
                cmd.Parameters.Add("@Contenido", SqlDbType.VarBinary, -1).Value = a.Contenido ?? (object)DBNull.Value;
                cmd.Parameters.AddWithValue("@ContentType", DbNull(a.ContentType));
                cmd.Parameters.AddWithValue("@TamanoBytes", DbNull(a.TamanoBytes));
                cmd.Parameters.AddWithValue("@IdUsuarioCrea", a.IdUsuarioCrea);

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

        /* ===========================================================
           DESCARGAR (trae binario + metadatos)
           =========================================================== */
        public ActivoAdjunto Descargar(int idAdjunto)
        {
            ActivoAdjunto adj = null;

            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Adjunto_Descargar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdAdjunto", idAdjunto);

                try
                {
                    cn.Open();
                    // SingleRow → optimiza y evita lecturas innecesarias
                    using (var dr = cmd.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (dr.Read())
                        {
                            adj = new ActivoAdjunto
                            {
                                IdAdjunto = SqlUtils.GetInt(dr, "IdAdjunto"),
                                IdActivo = SqlUtils.GetInt(dr, "IdActivo"),
                                NombreArchivo = SqlUtils.GetString(dr, "NombreArchivo"),
                                ContentType = SqlUtils.GetString(dr, "ContentType"),
                                TamanoBytes = SqlUtils.GetNullableLong(dr, "TamanoBytes"),
                                IdUsuarioCrea = SqlUtils.GetInt(dr, "IdUsuarioCrea"),
                                FechaRegistro = SqlUtils.GetDate(dr, "FechaRegistro"),
                            };

                            // Leer VARBINARY(MAX) en un solo paso (más robusto que el bucle por chunks)
                            int ordinal = dr.GetOrdinal("Contenido");
                            if (!dr.IsDBNull(ordinal))
                            {
                                adj.Contenido = (byte[])dr.GetValue(ordinal);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    adj = null;
                }
            }

            return adj;
        }

        /* ===========================================================
           ELIMINAR
           =========================================================== */
        public bool Eliminar(int idAdjunto, int idActivo)
        {
            bool ok = false;

            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Adjunto_Eliminar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdAdjunto", idAdjunto);
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

        /* ===========================================================
           (OPCIONAL) RENOMBRAR
           =========================================================== */
        public bool Renombrar(int idAdjunto, int idActivo, string nuevoNombre)
        {
            bool ok = false;

            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Adjunto_Renombrar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdAdjunto", idAdjunto);
                cmd.Parameters.AddWithValue("@IdActivo", idActivo);
                cmd.Parameters.AddWithValue("@NombreArchivo", nuevoNombre ?? string.Empty);

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
