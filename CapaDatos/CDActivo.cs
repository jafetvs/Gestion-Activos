using CapaModelo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class CDActivo
    {
        private static CDActivo _instancia = null;
        private CDActivo() { }
        public static CDActivo Instancia
        {
            get
            {
                if (_instancia == null)
                    _instancia = new CDActivo();
                return _instancia;
            }
        }

        private static ActivoClasificacion ParseClasificacion(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return ActivoClasificacion.SinDefinir;
            return (ActivoClasificacion)Enum.Parse(typeof(ActivoClasificacion), s, true);
        }

        // Listar con nuevos filtros: proveedor, tipo y clase
        public List<Activo> Listar(
            string texto = null,
            int? idCategoria = null,
            int? idDepto = null,
            ActivoClasificacion? clasif = null,
            //int? idProveedor = null,
            int? idTipo = null,
            int? idClase = null)
        {
            var lista = new List<Activo>();
            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Activo_Listar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Texto", (object)texto ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IdCategoria", (object)idCategoria ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IdDepto", (object)idDepto ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Clasificacion", (object)(clasif?.ToString()) ?? DBNull.Value);
               // cmd.Parameters.AddWithValue("@IdProveedor", (object)idProveedor ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IdTipo", (object)idTipo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IdClase", (object)idClase ?? DBNull.Value);

                try
                {
                    cn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var a = new Activo
                            {
                                IdActivo = SqlUtils.GetInt(dr, "IdActivo"),
                                Codigo = SqlUtils.GetString(dr, "Codigo"),
                                NombreTipo = SqlUtils.GetString(dr, "NombreTipo"),
                                Descripcion = SqlUtils.GetString(dr, "Descripcion"),
                                IdCategoria = SqlUtils.GetInt(dr, "IdCategoria"),
                                Clasificacion = ParseClasificacion(SqlUtils.GetString(dr, "Clasificacion")),
                                Serie = SqlUtils.GetString(dr, "Serie"),
                                Modelo = SqlUtils.GetString(dr, "Modelo"),
                                PlacaCodBarras = SqlUtils.GetString(dr, "PlacaCodBarras"),
                                Procesador = SqlUtils.GetString(dr, "Procesador"),
                                RAM = SqlUtils.GetString(dr, "RAM"),
                                SistemaOperativo = SqlUtils.GetString(dr, "SistemaOperativo"),
                                DiscoDuro = SqlUtils.GetString(dr, "DiscoDuro"),
                                IP = SqlUtils.GetString(dr, "IP"),
                                Id_Departamento = SqlUtils.GetNullableInt(dr, "Id_Departamento"),
                                IdSolicitante = SqlUtils.GetNullableInt(dr, "IdSolicitante"),
                                UsuarioLocal = SqlUtils.GetString(dr, "UsuarioLocal"),
                                OtraCaracteristica = SqlUtils.GetString(dr, "OtraCaracteristica"),
                                FechaEntrega = SqlUtils.GetNullableDate(dr, "FechaEntrega"),
                                Costo = SqlUtils.GetNullableDecimal(dr, "Costo"),
                                NoFactura = SqlUtils.GetString(dr, "NoFactura"),
                                FechaCompra = SqlUtils.GetNullableDate(dr, "FechaCompra"),
                                IdProveedor = SqlUtils.GetNullableInt(dr, "IdProveedor"),

                                // NUEVOS CAMPOS
                                IdTipo = SqlUtils.GetInt(dr, "IdTipo"),
                                IdClase = SqlUtils.GetInt(dr, "IdClase"),
                                UbicacionFisica = SqlUtils.GetString(dr, "UbicacionFisica"),
                                UsoPrincipal = SqlUtils.GetString(dr, "UsoPrincipal"),

                                ActivoFlag = SqlUtils.GetBool(dr, "Activo"),
                                IdUsuarioCrea = SqlUtils.GetInt(dr, "IdUsuarioCrea"),
                                FechaRegistro = SqlUtils.GetDate(dr, "FechaRegistro")
                            };

                            // Si quieres, podrías mapear nombres de catálogos desde el reader:
                            // var tipoNombre = SqlUtils.GetString(dr, "TipoNombre"); etc.

                            lista.Add(a);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    lista = new List<Activo>();
                }
            }
            return lista;
        }

        public (int idGenerado, bool ok) Registrar(Activo a)
        {
            int id = 0; bool ok = false;
            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Activo_Registrar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Codigo", a.Codigo);
                cmd.Parameters.AddWithValue("@NombreTipo", a.NombreTipo);
                cmd.Parameters.AddWithValue("@Descripcion", (object)a.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IdCategoria", a.IdCategoria);
                cmd.Parameters.AddWithValue("@Clasificacion", a.Clasificacion.ToString());
                cmd.Parameters.AddWithValue("@Serie", (object)a.Serie ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Modelo", (object)a.Modelo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PlacaCodBarras", (object)a.PlacaCodBarras ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Procesador", (object)a.Procesador ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RAM", (object)a.RAM ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SistemaOperativo", (object)a.SistemaOperativo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DiscoDuro", (object)a.DiscoDuro ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IP", (object)a.IP ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Id_Departamento", (object)a.Id_Departamento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IdSolicitante", (object)a.IdSolicitante ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UsuarioLocal", (object)a.UsuarioLocal ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OtraCaracteristica", (object)a.OtraCaracteristica ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaEntrega", (object)a.FechaEntrega ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Costo", (object)a.Costo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NoFactura", (object)a.NoFactura ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaCompra", (object)a.FechaCompra ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IdProveedor", (object)a.IdProveedor ?? DBNull.Value);

                // NUEVOS
                cmd.Parameters.AddWithValue("@IdTipo", a.IdTipo);
                cmd.Parameters.AddWithValue("@IdClase", a.IdClase);
                cmd.Parameters.AddWithValue("@UbicacionFisica", (object)a.UbicacionFisica ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UsoPrincipal", (object)a.UsoPrincipal ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@IdUsuarioCrea", a.IdUsuarioCrea);

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

        public bool Modificar(Activo a)
        {
            bool ok = false;
            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Activo_Modificar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@IdActivo", a.IdActivo);
                cmd.Parameters.AddWithValue("@Codigo", a.Codigo);
                cmd.Parameters.AddWithValue("@NombreTipo", a.NombreTipo);
                cmd.Parameters.AddWithValue("@Descripcion", (object)a.Descripcion ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IdCategoria", a.IdCategoria);
                cmd.Parameters.AddWithValue("@Clasificacion", a.Clasificacion.ToString());
                cmd.Parameters.AddWithValue("@Serie", (object)a.Serie ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Modelo", (object)a.Modelo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PlacaCodBarras", (object)a.PlacaCodBarras ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Procesador", (object)a.Procesador ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RAM", (object)a.RAM ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SistemaOperativo", (object)a.SistemaOperativo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DiscoDuro", (object)a.DiscoDuro ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IP", (object)a.IP ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Id_Departamento", (object)a.Id_Departamento ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IdSolicitante", (object)a.IdSolicitante ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UsuarioLocal", (object)a.UsuarioLocal ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OtraCaracteristica", (object)a.OtraCaracteristica ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaEntrega", (object)a.FechaEntrega ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Costo", (object)a.Costo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NoFactura", (object)a.NoFactura ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@FechaCompra", (object)a.FechaCompra ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IdProveedor", (object)a.IdProveedor ?? DBNull.Value);

                // NUEVOS
                cmd.Parameters.AddWithValue("@IdTipo", a.IdTipo);
                cmd.Parameters.AddWithValue("@IdClase", a.IdClase);
                cmd.Parameters.AddWithValue("@UbicacionFisica", (object)a.UbicacionFisica ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UsoPrincipal", (object)a.UsoPrincipal ?? DBNull.Value);

                cmd.Parameters.AddWithValue("@Activo", a.ActivoFlag);

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

        public bool Eliminar(int idActivo)
        {
            bool ok = false;
            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Activo_Eliminar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdActivo", idActivo);

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
                    Console.WriteLine("Error en Eliminar: " + ex.Message);
                    ok = false;
                }
            }
            return ok;
        }

        // Helper simple (si quieres uno específico por SP, crea otro método)
        public Activo Obtener(int idActivo)
        {
            foreach (var a in Listar())
                if (a.IdActivo == idActivo) return a;
            return null;
        }
    }
}
