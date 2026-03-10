using CapaModelo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class CDProveedor
    {
        private static CDProveedor _instancia;
        private CDProveedor() { }
        public static CDProveedor Instancia
        {
            get
            {
                if (_instancia == null) _instancia = new CDProveedor();
                return _instancia;
            }
        }

        private static object DbNull(object v) => v ?? DBNull.Value;
        private static string TrimOrNull(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

        /* ===========================================================
           LISTAR (SIN filtros) → usa la sobrecarga con todo NULL
           =========================================================== */
        public List<Proveedor> Listar()
            => Listar(null, null, null, null, null, null, null, null, null);

        /* ===========================================================
           LISTAR (CON filtros)
           Requiere que usp_Proveedor_Listar acepte (opcionales):
             @Texto NVARCHAR(100)=NULL,
             @Servicio VARCHAR(150)=NULL,
             @NoContrato VARCHAR(50)=NULL,
             @NoProcedimiento VARCHAR(50)=NULL,
             @RenovacionContrato BIT=NULL,
             @Activo BIT=NULL,
             @VenceDesde DATE=NULL,
             @VenceHasta DATE=NULL
           y filtre con (@Param IS NULL OR ...) + LIKE para @Texto.
           =========================================================== */
        public List<Proveedor> Listar(
            string texto,
            string servicio,
            string noContrato,
            string noProcedimiento,
            bool? renovacionContrato,
            bool? activo,
            DateTime? venceDesde,
            DateTime? venceHasta,
            int? idProveedor)
        {
            var lista = new List<Proveedor>();

            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Proveedor_Listar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Texto", (object)TrimOrNull(texto) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Servicio", (object)TrimOrNull(servicio) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NoContrato", (object)TrimOrNull(noContrato) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@NoProcedimiento", (object)TrimOrNull(noProcedimiento) ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@RenovacionContrato", (object)renovacionContrato ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Activo", (object)activo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@VenceDesde", (object)venceDesde ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@VenceHasta", (object)venceHasta ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IdProveedor", (object)idProveedor ?? DBNull.Value);

                try
                {
                    cn.Open();
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Proveedor
                            {
                                IdProveedor = SqlUtils.GetInt(dr, "IdProveedor"),
                                NombreEmpresa = SqlUtils.GetString(dr, "NombreEmpresa"),
                                Servicio = SqlUtils.GetString(dr, "Servicio"),
                                NoContrato = SqlUtils.GetString(dr, "NoContrato"),
                                NoProcedimiento = SqlUtils.GetString(dr, "NoProcedimiento"),
                                RenovacionContrato = SqlUtils.GetBool(dr, "RenovacionContrato"),
                                FechaVencimientoContrato = SqlUtils.GetNullableDate(dr, "FechaVencimientoContrato"),
                                Contacto1_Nombre = SqlUtils.GetString(dr, "Contacto1_Nombre"),
                                Contacto1_Telefono = SqlUtils.GetString(dr, "Contacto1_Telefono"),
                                Contacto1_Correo = SqlUtils.GetString(dr, "Contacto1_Correo"),
                                Contacto2_Nombre = SqlUtils.GetString(dr, "Contacto2_Nombre"),
                                Contacto2_Telefono = SqlUtils.GetString(dr, "Contacto2_Telefono"),
                                Contacto2_Correo = SqlUtils.GetString(dr, "Contacto2_Correo"),
                                PaginaWeb = SqlUtils.GetString(dr, "PaginaWeb"),
                                Activo = SqlUtils.GetBool(dr, "Activo"),
                                FechaRegistro = SqlUtils.GetDate(dr, "FechaRegistro")
                                // Si agregas IdUsuarioCrea en el SELECT, lo mapeas aquí:
                                // IdUsuarioCrea = SqlUtils.GetInt(dr, "IdUsuarioCrea"),
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    lista = new List<Proveedor>();
                }
            }

            return lista;
        }



        public bool TieneActivosAsociados(int idProveedor)
        {
            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("SELECT TOP 1 1 FROM dbo.ACTIVO WHERE IdProveedor = @Id", cn))
            {
                cmd.Parameters.AddWithValue("@Id", idProveedor);
                try
                {
                    cn.Open();
                    var r = cmd.ExecuteScalar();
                    return r != null && r != DBNull.Value;
                }
                catch
                {
                    // En caso de error de conexión, no bloqueamos aquí; que decida el controlador.
                    return false;
                }
            }
        }


        /* ===========================================================
           REGISTRAR (tabla exige IdUsuarioCrea)
           =========================================================== */
        public (int idGenerado, bool ok) Registrar(Proveedor p, int idUsuarioCrea)
        {
            int id = 0; bool ok = false;

            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Proveedor_Registrar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@NombreEmpresa", p.NombreEmpresa);
                cmd.Parameters.AddWithValue("@Servicio", DbNull(TrimOrNull(p.Servicio)));
                cmd.Parameters.AddWithValue("@NoContrato", DbNull(TrimOrNull(p.NoContrato)));
                cmd.Parameters.AddWithValue("@NoProcedimiento", DbNull(TrimOrNull(p.NoProcedimiento)));
                cmd.Parameters.AddWithValue("@RenovacionContrato", p.RenovacionContrato);
                cmd.Parameters.AddWithValue("@FechaVencimientoContrato", (object)p.FechaVencimientoContrato ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Contacto1_Nombre", DbNull(TrimOrNull(p.Contacto1_Nombre)));
                cmd.Parameters.AddWithValue("@Contacto1_Telefono", DbNull(TrimOrNull(p.Contacto1_Telefono)));
                cmd.Parameters.AddWithValue("@Contacto1_Correo", DbNull(TrimOrNull(p.Contacto1_Correo)));
                cmd.Parameters.AddWithValue("@Contacto2_Nombre", DbNull(TrimOrNull(p.Contacto2_Nombre)));
                cmd.Parameters.AddWithValue("@Contacto2_Telefono", DbNull(TrimOrNull(p.Contacto2_Telefono)));
                cmd.Parameters.AddWithValue("@Contacto2_Correo", DbNull(TrimOrNull(p.Contacto2_Correo)));
                cmd.Parameters.AddWithValue("@PaginaWeb", DbNull(TrimOrNull(p.PaginaWeb)));
                cmd.Parameters.AddWithValue("@Activo", p.Activo);

                // Auditoría
                cmd.Parameters.AddWithValue("@IdUsuarioCrea", idUsuarioCrea);

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
           MODIFICAR
           =========================================================== */
        public bool Modificar(Proveedor p)
        {
            bool ok = false;

            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Proveedor_Modificar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@IdProveedor", p.IdProveedor);
                cmd.Parameters.AddWithValue("@NombreEmpresa", p.NombreEmpresa);
                cmd.Parameters.AddWithValue("@Servicio", DbNull(TrimOrNull(p.Servicio)));
                cmd.Parameters.AddWithValue("@NoContrato", DbNull(TrimOrNull(p.NoContrato)));
                cmd.Parameters.AddWithValue("@NoProcedimiento", DbNull(TrimOrNull(p.NoProcedimiento)));
                cmd.Parameters.AddWithValue("@RenovacionContrato", p.RenovacionContrato);
                cmd.Parameters.AddWithValue("@FechaVencimientoContrato", (object)p.FechaVencimientoContrato ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Contacto1_Nombre", DbNull(TrimOrNull(p.Contacto1_Nombre)));
                cmd.Parameters.AddWithValue("@Contacto1_Telefono", DbNull(TrimOrNull(p.Contacto1_Telefono)));
                cmd.Parameters.AddWithValue("@Contacto1_Correo", DbNull(TrimOrNull(p.Contacto1_Correo)));
                cmd.Parameters.AddWithValue("@Contacto2_Nombre", DbNull(TrimOrNull(p.Contacto2_Nombre)));
                cmd.Parameters.AddWithValue("@Contacto2_Telefono", DbNull(TrimOrNull(p.Contacto2_Telefono)));
                cmd.Parameters.AddWithValue("@Contacto2_Correo", DbNull(TrimOrNull(p.Contacto2_Correo)));
                cmd.Parameters.AddWithValue("@PaginaWeb", DbNull(TrimOrNull(p.PaginaWeb)));
                cmd.Parameters.AddWithValue("@Activo", p.Activo);

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
           ELIMINAR
           =========================================================== */
        public bool Eliminar(int idProveedor)
        {
            bool ok = false;

            using (var cn = new SqlConnection(Conexion.CN))
            using (var cmd = new SqlCommand("usp_Proveedor_Eliminar", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdProveedor", idProveedor);

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
