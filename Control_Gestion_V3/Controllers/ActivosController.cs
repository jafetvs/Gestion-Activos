using CapaDatos;
using CapaModelo;
using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Control_Gestion_V3.Controllers
{
    public class ActivosController : PadreController
    {
        // ================== Helpers ==================
        private void CargarCombos()
        {
            ViewBag.Categorias = CDActivoCategoria.Instancia.Listar()
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToList();

            ViewBag.Departamentos = CD_Departamentos.Instancia.ObtenerDepartamentos(true)
                .OrderBy(d => d.Descripcion)
                .ToList();

            // Se mantiene para formularios (aunque no filtremos por proveedor)
            ViewBag.Proveedores = CDProveedor.Instancia.Listar()
                .Where(p => p.Activo)
                .OrderBy(p => p.NombreEmpresa)
                .ToList();

            ViewBag.Solicitantes = CD_Solicitantes.Instancia.ObtenerSolicitantes()
                .OrderBy(s => s.Nombre)
                .ToList();

            ViewBag.Clasificaciones = Enum.GetValues(typeof(ActivoClasificacion))
                .Cast<ActivoClasificacion>()
                .Select(x => new SelectListItem { Text = x.ToString(), Value = x.ToString() })
                .ToList();

            // NUEVO: combos de Tipo y Clase
            ViewBag.Tipos = CDActivoTipo.Instancia.Listar()
                .Where(t => t.Activo)
                .OrderBy(t => t.Nombre)
                .ToList();

            ViewBag.Clases = CDActivoClase.Instancia.Listar()
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToList();
        }

        // ================== Rutas base ==================
        public ActionResult Index(
            string texto = null, int? idCategoria = null, int? idDepto = null,
            ActivoClasificacion? clasificacion = null, int? idTipo = null, int? idClase = null)
        {
            return RedirectToAction("Activos", new { texto, idCategoria, idDepto, clasificacion, idTipo, idClase });
        }

        // ================== Vista principal ==================
        public ActionResult Activos(
            string texto = null, int? idCategoria = null, int? idDepto = null,
            ActivoClasificacion? clasificacion = null, int? idTipo = null, int? idClase = null)
        {
            try
            {
                var data = CDActivo.Instancia.Listar(texto, idCategoria, idDepto, clasificacion, idTipo, idClase);

                bool incluirDesechados = clasificacion.HasValue && clasificacion.Value == ActivoClasificacion.Desechado;
                if (!incluirDesechados)
                    data = data.Where(a => a.Clasificacion != ActivoClasificacion.Desechado).ToList();

                CargarCombos();
                ViewBag.FiltroTexto = texto;
                ViewBag.FiltroIdCategoria = idCategoria;
                ViewBag.FiltroIdDepto = idDepto;
                ViewBag.FiltroClasif = clasificacion;
                ViewBag.FiltroIdTipo = idTipo;
                ViewBag.FiltroIdClase = idClase;

                return View("activos", data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Activos (GET): " + ex.Message);
                TempData["Error"] = "Ocurrió un error al cargar la lista de activos.";
                CargarCombos();
                return View("activos", new List<Activo>());
            }
        }

        // ================== Filtrar (AJAX/Partial) ==================
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult Filtrar(
            string texto, int? idCategoria, int? idDepto,
            ActivoClasificacion? clasificacion, int? idTipo, int? idClase)
        {
            try
            {
                var lista = CDActivo.Instancia.Listar(texto, idCategoria, idDepto, clasificacion, idTipo, idClase);

                bool incluirDesechados = clasificacion.HasValue && clasificacion.Value == ActivoClasificacion.Desechado;
                if (!incluirDesechados)
                    lista = lista.Where(a => a.Clasificacion != ActivoClasificacion.Desechado).ToList();

                CargarCombos();
                ViewBag.FiltroClasif = clasificacion;
                ViewBag.FiltroIdTipo = idTipo;
                ViewBag.FiltroIdClase = idClase;

                return PartialView("_ActivosFilas", lista);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Filtrar Activos: " + ex.Message);
                CargarCombos();
                ViewBag.FiltroClasif = clasificacion;
                ViewBag.FiltroIdTipo = idTipo;
                ViewBag.FiltroIdClase = idClase;
                return PartialView("_ActivosFilas", Enumerable.Empty<Activo>());
            }
        }

        // ================== Crear (POST) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(Activo model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Datos inválidos. Verifica la información ingresada.";
                return RedirectToAction("Activos");
            }

            try
            {
                var usuario = Session["Usuario"] as Usuario;
                model.IdUsuarioCrea = usuario != null ? usuario.IdUsuario : 0;

                var (id, ok) = CDActivo.Instancia.Registrar(model);
                if (!ok)
                {
                    TempData["Error"] = "No se pudo registrar el activo. Verifique que el código no exista.";
                    return RedirectToAction("Activos");
                }

                TempData["Ok"] = "Activo creado correctamente.";
                return RedirectToAction("Activos");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Crear Activo: " + ex.Message);
                TempData["Error"] = "Ocurrió un error al crear el activo.";
                return RedirectToAction("Activos");
            }
        }

        // =============== Editar: pestaña RESUMEN ===============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarResumen(
            int IdActivo,
            string Codigo,
            string NombreTipo,
            ActivoClasificacion Clasificacion,
            string Descripcion,
            int IdCategoria,
            int? Id_Departamento,
            int? IdSolicitante,
            string OtraCaracteristica,
            DateTime? FechaEntrega,

            // NUEVOS CAMPOS (Tipo/Clase/Ubicación/Uso)
            int IdTipo,
            int IdClase,
            string UbicacionFisica,
            string UsoPrincipal
        )
        {
            try
            {
                var actual = CDActivo.Instancia.Obtener(IdActivo);
                if (actual == null)
                {
                    TempData["Error"] = "Activo no encontrado.";
                    return RedirectToAction("Activos");
                }

                // --------- SOLO RESUMEN ----------
                actual.Codigo = (Codigo ?? "").Trim();
                actual.NombreTipo = (NombreTipo ?? "").Trim();
                actual.Clasificacion = Clasificacion;
                actual.Descripcion = string.IsNullOrWhiteSpace(Descripcion) ? null : Descripcion.Trim();
                actual.IdCategoria = IdCategoria;
                actual.Id_Departamento = Id_Departamento;
                actual.IdSolicitante = IdSolicitante;
                actual.OtraCaracteristica = string.IsNullOrWhiteSpace(OtraCaracteristica) ? null : OtraCaracteristica.Trim();
                actual.FechaEntrega = FechaEntrega;

                // --------- NUEVOS CAMPOS ----------
                actual.IdTipo = IdTipo;
                actual.IdClase = IdClase;
                actual.UbicacionFisica = string.IsNullOrWhiteSpace(UbicacionFisica) ? null : UbicacionFisica.Trim();
                actual.UsoPrincipal = string.IsNullOrWhiteSpace(UsoPrincipal) ? null : UsoPrincipal.Trim();

                // Guardar (SP valida Código único, etc.)
                var ok = CDActivo.Instancia.Modificar(actual);
                if (!ok)
                {
                    TempData["Error"] = "No se pudo guardar. Verifica que el CÓDIGO no esté usado por otro activo.";
                    return RedirectToAction("Detalle", new { id = IdActivo, tab = "resumen" });
                }

                TempData["Ok"] = "Datos de resumen actualizados.";
                return RedirectToAction("Detalle", new { id = IdActivo, tab = "resumen" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EditarResumen error: " + ex.Message);
                TempData["Error"] = "Ocurrió un error al guardar.";
                return RedirectToAction("Detalle", new { id = IdActivo, tab = "resumen" });
            }
        }



        // =============== Editar: pestaña COMPRA ===============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarCompra(
            int IdActivo,
            int? IdProveedor,
            string NoFactura,
            decimal? Costo,
            DateTime? FechaCompra)
        {
            try
            {
                var actual = CDActivo.Instancia.Obtener(IdActivo);
                if (actual == null)
                {
                    TempData["Error"] = "Activo no encontrado.";
                    return RedirectToAction("Activos");
                }

                // Solo campos de COMPRA
                actual.IdProveedor = IdProveedor;
                actual.NoFactura = string.IsNullOrWhiteSpace(NoFactura) ? null : NoFactura.Trim();
                actual.Costo = Costo;
                actual.FechaCompra = FechaCompra;

                var ok = CDActivo.Instancia.Modificar(actual);
                if (!ok)
                {
                    TempData["Error"] = "No se pudo guardar los datos de compra.";
                    return RedirectToAction("Detalle", new { id = IdActivo, tab = "compra" });
                }

                TempData["Ok"] = "Datos de compra actualizados.";
                return RedirectToAction("Detalle", new { id = IdActivo, tab = "compra" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EditarCompra error: " + ex.Message);
                TempData["Error"] = "Ocurrió un error al guardar.";
                return RedirectToAction("Detalle", new { id = IdActivo, tab = "compra" });
            }
        }


        // =============== Editar: pestaña HARDWARE ===============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarHardware(
            int IdActivo,
            string Serie,
            string Modelo,
            string PlacaCodBarras,
            string IP,
            string Procesador,
            string RAM,
            string SistemaOperativo,
            string DiscoDuro)
        {
            try
            {
                var actual = CDActivo.Instancia.Obtener(IdActivo);
                if (actual == null)
                {
                    TempData["Error"] = "Activo no encontrado.";
                    return RedirectToAction("Activos");
                }

                // Solo campos de HARDWARE
                actual.Serie = string.IsNullOrWhiteSpace(Serie) ? null : Serie.Trim();
                actual.Modelo = string.IsNullOrWhiteSpace(Modelo) ? null : Modelo.Trim();
                actual.PlacaCodBarras = string.IsNullOrWhiteSpace(PlacaCodBarras) ? null : PlacaCodBarras.Trim();
                actual.IP = string.IsNullOrWhiteSpace(IP) ? null : IP.Trim();
                actual.Procesador = string.IsNullOrWhiteSpace(Procesador) ? null : Procesador.Trim();
                actual.RAM = string.IsNullOrWhiteSpace(RAM) ? null : RAM.Trim();
                actual.SistemaOperativo = string.IsNullOrWhiteSpace(SistemaOperativo) ? null : SistemaOperativo.Trim();
                actual.DiscoDuro = string.IsNullOrWhiteSpace(DiscoDuro) ? null : DiscoDuro.Trim();

                var ok = CDActivo.Instancia.Modificar(actual);
                if (!ok)
                {
                    TempData["Error"] = "No se pudo guardar el hardware. Verifica restricciones (por ejemplo, serie si fuera única).";
                    return RedirectToAction("Detalle", new { id = IdActivo, tab = "hardware" });
                }

                TempData["Ok"] = "Datos de hardware actualizados.";
                return RedirectToAction("Detalle", new { id = IdActivo, tab = "hardware" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EditarHardware error: " + ex.Message);
                TempData["Error"] = "Ocurrió un error al guardar.";
                return RedirectToAction("Detalle", new { id = IdActivo, tab = "hardware" });
            }
        }



        // ================== Eliminar (POST/AJAX) ==================
        [HttpPost]
        public ActionResult Eliminar(int id)
        {
            try
            {
                var ok = CDActivo.Instancia.Eliminar(id);
                if (!ok)
                    return Json(new { ok = false, msg = "No se encontró el activo o no se pudo marcar como 'Desechado'." });

                return Json(new { ok = true, msg = "Activo marcado como 'Desechado'." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Eliminar Activo: " + ex.Message);
                return Json(new { ok = false, msg = "Ocurrió un error al marcar el activo como 'Desechado'." });
            }
        }

        // ================== Detalle (solo lectura) ==================
        [HttpGet]
        public ActionResult Detalle(int id, string returnUrl = null)
        {
            try
            {
                var activo = CDActivo.Instancia.Obtener(id);
                if (activo == null)
                {
                    TempData["Error"] = "Activo no encontrado.";
                    return RedirectToAction("Activos");
                }

                ViewBag.Software = CDActivoSoftware.Instancia.ListarPorActivo(id);
                ViewBag.Adjuntos = CDActivoAdjunto.Instancia.ListarPorActivo(id);
                ViewBag.Movimientos = CDActivoMovimiento.Instancia.ListarPorActivo(id);

                CargarCombos(); // <- aquí usualmente cargas ViewBag.Proveedores (activos), etc.

                // ====== Proveedor actual por ID, sin filtrar por estado (puede estar INACTIVO) ======
                if (activo.IdProveedor.HasValue && activo.IdProveedor.Value > 0)
                {
                    try
                    {
                        // Usamos el SP de LISTAR pasando @IdProveedor y @Activo = NULL
                        var provActual = CDProveedor.Instancia.Listar(
                            texto: null,
                            servicio: null,
                            noContrato: null,
                            noProcedimiento: null,
                            renovacionContrato: null,
                            activo: null, // IMPORTANTE: null para NO filtrar por estado
                            venceDesde: null,
                            venceHasta: null,
                            idProveedor: activo.IdProveedor.Value
                        ).FirstOrDefault();

                        ViewBag.ProveedorActual = provActual;                  // objeto completo (IdProveedor, NombreEmpresa, Activo, ...)
                        ViewBag.ProveedorActualNombre = provActual?.NombreEmpresa;   // acceso directo al nombre de empresa
                    }
                    catch
                    {
                        ViewBag.ProveedorActual = null;
                        ViewBag.ProveedorActualNombre = null;
                    }
                }

                if (string.IsNullOrWhiteSpace(returnUrl) && Request?.UrlReferrer != null)
                    returnUrl = Request.UrlReferrer.PathAndQuery;
                ViewBag.ReturnUrl = returnUrl;

                return View("Detalle", activo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Detalle Activo: " + ex.Message);
                TempData["Error"] = "Ocurrió un error al cargar el detalle del activo.";
                return RedirectToAction("Activos");
            }
        }


        // ================== Exportar Excel (mantengo columnas extendidas) ==================
        public ActionResult ExportarExcel(
            string texto = null, int? idCategoria = null, int? idDepto = null,
            ActivoClasificacion? clasificacion = null, int? idTipo = null, int? idClase = null)
        {
            try
            {
                var data = CDActivo.Instancia.Listar(texto, idCategoria, idDepto, clasificacion, idTipo, idClase)
                           .Where(a => a.Clasificacion != ActivoClasificacion.Desechado)
                           .ToList();

                // Diccionarios para nombres
                var dicCat = CDActivoCategoria.Instancia.Listar().ToDictionary(x => x.IdCategoria, x => x.Nombre);
                var dicDep = CD_Departamentos.Instancia.ObtenerDepartamentos(true).ToDictionary(x => x.Id_Departamento, x => x.Descripcion);
                var dicTipo = CDActivoTipo.Instancia.Listar().ToDictionary(x => x.IdTipo, x => x.Nombre);
                var dicClase = CDActivoClase.Instancia.Listar().ToDictionary(x => x.IdClase, x => x.Nombre);

                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Activos");

                    // Encabezados ampliados
                    ws.Cell(1, 1).Value = "Código";
                    ws.Cell(1, 2).Value = "Nombre";
                    ws.Cell(1, 3).Value = "Descripción";
                    ws.Cell(1, 4).Value = "Categoría";
                    ws.Cell(1, 5).Value = "Departamento";
                    ws.Cell(1, 6).Value = "Clasificación";
                    ws.Cell(1, 7).Value = "Tipo";
                    ws.Cell(1, 8).Value = "Clase";
                    ws.Cell(1, 9).Value = "Ubicación Física";
                    ws.Cell(1, 10).Value = "Uso Principal";
                    ws.Cell(1, 11).Value = "Placa / Código Barras";
                    ws.Cell(1, 12).Value = "F. Registro";

                    var header = ws.Range(1, 1, 1, 12);
                    header.Style.Font.Bold = true;
                    header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.SheetView.FreezeRows(1);

                    int row = 2;
                    foreach (var a in data)
                    {
                        ws.Cell(row, 1).Value = a.Codigo ?? "";
                        ws.Cell(row, 2).Value = a.NombreTipo ?? "";
                        ws.Cell(row, 3).Value = a.Descripcion ?? "";
                        ws.Cell(row, 4).Value = dicCat.TryGetValue(a.IdCategoria, out var cat) ? cat : "-";
                        ws.Cell(row, 5).Value = (a.Id_Departamento.HasValue && dicDep.TryGetValue(a.Id_Departamento.Value, out var dep)) ? dep : "-";
                        ws.Cell(row, 6).Value = a.Clasificacion.ToString();
                        ws.Cell(row, 7).Value = dicTipo.TryGetValue(a.IdTipo, out var tnom) ? tnom : "-";
                        ws.Cell(row, 8).Value = dicClase.TryGetValue(a.IdClase, out var cnom) ? cnom : "-";
                        ws.Cell(row, 9).Value = string.IsNullOrWhiteSpace(a.UbicacionFisica) ? "-" : a.UbicacionFisica;
                        ws.Cell(row, 10).Value = string.IsNullOrWhiteSpace(a.UsoPrincipal) ? "-" : a.UsoPrincipal;
                        ws.Cell(row, 11).Value = string.IsNullOrWhiteSpace(a.PlacaCodBarras) ? "-" : a.PlacaCodBarras;
                        ws.Cell(row, 12).Value = a.FechaRegistro;
                        ws.Cell(row, 12).Style.DateFormat.Format = "dd/MM/yyyy";
                        row++;
                    }

                    try { ws.Columns(1, 12).AdjustToContents(); }
                    catch
                    {
                        ws.Column(1).Width = 14;  // Código
                        ws.Column(2).Width = 24;  // Nombre
                        ws.Column(3).Width = 36;  // Descripción
                        ws.Column(4).Width = 18;  // Categoría
                        ws.Column(5).Width = 20;  // Depto
                        ws.Column(6).Width = 16;  // Clasificación
                        ws.Column(7).Width = 16;  // Tipo
                        ws.Column(8).Width = 18;  // Clase
                        ws.Column(9).Width = 24;  // Ubicación
                        ws.Column(10).Width = 24; // Uso
                        ws.Column(11).Width = 22; // Placa
                        ws.Column(12).Width = 14; // Fecha
                    }

                    using (var stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);
                        return File(stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "Activos.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error ExportarExcel Activos: " + ex);
                TempData["Error"] = "Ocurrió un error al exportar a Excel.";
                return RedirectToAction("Activos", new { texto, idCategoria, idDepto, clasificacion, idTipo, idClase });
            }
        }

        // ================== Exportar PDF (con todas las columnas) ==================
        public ActionResult ExportarPdf(
            string texto = null, int? idCategoria = null, int? idDepto = null,
            ActivoClasificacion? clasificacion = null, int? idTipo = null, int? idClase = null)
        {
            try
            {
                var data = CDActivo.Instancia.Listar(texto, idCategoria, idDepto, clasificacion, idTipo, idClase)
                           .Where(a => a.Clasificacion != ActivoClasificacion.Desechado)
                           .ToList();

                // Diccionarios para mostrar nombres
                var dicCat = CDActivoCategoria.Instancia.Listar().ToDictionary(x => x.IdCategoria, x => x.Nombre);
                var dicDep = CD_Departamentos.Instancia.ObtenerDepartamentos(true).ToDictionary(x => x.Id_Departamento, x => x.Descripcion);
                var dicTipo = CDActivoTipo.Instancia.Listar().ToDictionary(x => x.IdTipo, x => x.Nombre);
                var dicClase = CDActivoClase.Instancia.Listar().ToDictionary(x => x.IdClase, x => x.Nombre);
                var dicResp = CD_Solicitantes.Instancia.ObtenerSolicitantes()
                                .ToDictionary(x => x.ID_Usuario,
                                              x => string.IsNullOrWhiteSpace(x.Cedula_Solicitante)
                                                    ? x.Nombre
                                                    : $"{x.Nombre} ({x.Cedula_Solicitante})");

                using (var stream = new MemoryStream())
                {
                    var doc = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
                    PdfWriter.GetInstance(doc, stream);
                    doc.Open();

                    var title = new Paragraph("Listado de Activos")
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingAfter = 10f
                    };
                    doc.Add(title);

                    // Columnas: Código, Nombre, Categoría, Departamento, Tipo, Clase, Ubicación, Responsable, Placa/Cód. Barras, F. Registro
                    var table = new PdfPTable(10) { WidthPercentage = 100 };
                    table.SetWidths(new float[] { 10f, 18f, 14f, 16f, 12f, 12f, 18f, 20f, 14f, 12f });

                    table.AddCell("Código");
                    table.AddCell("Nombre");
                    table.AddCell("Categoría");
                    table.AddCell("Departamento");
                    table.AddCell("Tipo");
                    table.AddCell("Clase");
                    table.AddCell("Ubicación");
                    table.AddCell("Responsable");
                    table.AddCell("Placa/Cód. Barras");
                    table.AddCell("F. Registro");

                    foreach (var a in data)
                    {
                        table.AddCell(a.Codigo ?? "");
                        table.AddCell(a.NombreTipo ?? "");
                        table.AddCell(dicCat.TryGetValue(a.IdCategoria, out var cat) ? cat : "-");
                        table.AddCell((a.Id_Departamento.HasValue && dicDep.TryGetValue(a.Id_Departamento.Value, out var dep)) ? dep : "-");
                        table.AddCell(dicTipo.TryGetValue(a.IdTipo, out var tnom) ? tnom : "-");
                        table.AddCell(dicClase.TryGetValue(a.IdClase, out var cnom) ? cnom : "-");
                        table.AddCell(string.IsNullOrWhiteSpace(a.UbicacionFisica) ? "-" : a.UbicacionFisica);
                        var resp = (a.IdSolicitante.HasValue && dicResp.TryGetValue(a.IdSolicitante.Value, out var r)) ? r : "-";
                        table.AddCell(resp);
                        table.AddCell(string.IsNullOrWhiteSpace(a.PlacaCodBarras) ? "-" : a.PlacaCodBarras);
                        table.AddCell(a.FechaRegistro.ToString("dd/MM/yyyy"));
                    }

                    doc.Add(table);
                    doc.Close();

                    return File(stream.ToArray(), "application/pdf", "Activos.pdf");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error ExportarPdf Activos: " + ex.Message);
                TempData["Error"] = "Ocurrió un error al exportar a PDF.";
                return RedirectToAction("Activos", new { texto, idCategoria, idDepto, clasificacion, idTipo, idClase });
            }
        }


        /* ===============================================================
           MOVIMIENTOS DE ACTIVO (acciones MVC)
           =============================================================== */

        private int GetUsuarioActualId()
        {
            var u = Session["Usuario"] as Usuario;
            return (u != null ? u.IdUsuario : 0);
        }

        // ---------- Listar (Partial principal) ----------
        [HttpGet]
        public ActionResult Movimientos(
            int idActivo,
            DateTime? desde = null,
            DateTime? hasta = null,
            string texto = null,
            string responsable = null,
            int page = 1,
            int pageSize = 25)
        {
            try
            {
                var lista = CDActivoMovimiento.Instancia
                                .ListarPorActivo(idActivo, desde, hasta, texto)
                            ?? new List<ActivoMovimiento>();

                var ordered = lista
                    .OrderByDescending(m => m.FechaMovimiento)
                    .ThenByDescending(m => m.IdMovimiento);

                var total = ordered.Count();
                var pageCount = (int)Math.Ceiling(total / (double)pageSize);
                if (page < 1) page = 1;
                if (page > pageCount && pageCount > 0) page = pageCount;

                var pageData = ordered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.Page = page;
                ViewBag.TotalPages = pageCount;
                ViewBag.Responsable = responsable;

                return PartialView("_MovimientosTabla", pageData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Movimientos (GET): " + ex.Message);
                return PartialView("_MovimientosTabla", Enumerable.Empty<ActivoMovimiento>());
            }
        }

        // ---------- Alias para el JS (usa ?q= en lugar de ?texto=) ----------
        [HttpGet]
        public ActionResult MovimientosLista(
            int idActivo,
            DateTime? desde = null,
            DateTime? hasta = null,
            string q = null,
            string responsable = null,
            int page = 1,
            int pageSize = 25)
        {
            return Movimientos(idActivo, desde, hasta, q, responsable, page, pageSize);
        }

        // ---------- Formulario (modal) ----------
        [HttpGet]
        public ActionResult MovimientoForm(int idActivo, int idMovimiento = 0)
        {
            ActivoMovimiento model;

            if (idMovimiento > 0)
            {
                model = CDActivoMovimiento.Instancia
                         .ListarPorActivo(idActivo)
                         .FirstOrDefault(x => x.IdMovimiento == idMovimiento);

                if (model == null)
                    return Content("<div class='p-3 text-danger'>Movimiento no encontrado.</div>");
            }
            else
            {
                model = new ActivoMovimiento
                {
                    IdMovimiento = 0,
                    IdActivo = idActivo,
                    FechaMovimiento = DateTime.Now
                };
            }

            ViewBag.Solicitantes = CD_Solicitantes.Instancia.ObtenerSolicitantes()
                                        .OrderBy(s => s.Nombre)
                                        .ToList();

            return PartialView("_MovimientoForm", model);
        }

        // ---------- Registrar ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarMovimiento(int idActivo, int idSolicitante, MovimientoTipo tipo, string detalle, DateTime? fecha = null)
        {
            try
            {
                if (idActivo <= 0)
                    return Json(new { ok = false, msg = "Activo inválido." });

                if (idSolicitante <= 0)
                    return Json(new { ok = false, msg = "El responsable es obligatorio." });

                var m = new ActivoMovimiento
                {
                    IdActivo = idActivo,
                    IdSolicitante = idSolicitante,
                    TipoMovimiento = tipo,
                    Detalle = string.IsNullOrWhiteSpace(detalle) ? null : detalle.Trim(),
                    IdUsuario = GetUsuarioActualId(),
                    FechaMovimiento = fecha ?? DateTime.Now
                };

                var (id, ok) = CDActivoMovimiento.Instancia.Registrar(m);
                if (!ok) return Json(new { ok = false, msg = "No se pudo registrar el movimiento." });

                return Json(new { ok = true, msg = "Movimiento registrado.", idGenerado = id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error AgregarMovimiento: " + ex.Message);
                return Json(new { ok = false, msg = "Ocurrió un error al registrar el movimiento." });
            }
        }

        // ---------- Editar ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarMovimiento(int idMovimiento, int idActivo, int idSolicitante, MovimientoTipo tipo, string detalle, DateTime? fecha = null)
        {
            try
            {
                if (idSolicitante <= 0)
                    return Json(new { ok = false, msg = "El responsable es obligatorio." });

                var ok = CDActivoMovimiento.Instancia.Editar(idMovimiento, idActivo, idSolicitante, tipo, detalle, fecha);
                if (!ok) return Json(new { ok = false, msg = "No se pudo editar el movimiento." });

                return Json(new { ok = true, msg = "Movimiento actualizado." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error EditarMovimiento: " + ex.Message);
                return Json(new { ok = false, msg = "Ocurrió un error al editar el movimiento." });
            }
        }

        // ---------- Eliminar ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarMovimiento(int idMovimiento, int idActivo)
        {
            try
            {
                var ok = CDActivoMovimiento.Instancia.Eliminar(idMovimiento, idActivo);
                if (!ok) return Json(new { ok = false, msg = "No se pudo eliminar el movimiento." });

                return Json(new { ok = true, msg = "Movimiento eliminado." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error EliminarMovimiento: " + ex.Message);
                return Json(new { ok = false, msg = "Ocurrió un error al eliminar el movimiento." });
            }
        }

        /* ===============================================================
           SOFTWARE INSTALADO POR ACTIVO
           =============================================================== */
        #region Software

        [HttpGet]
        public ActionResult Software(
            int idActivo,
            DateTime? desde = null,
            DateTime? hasta = null,
            string texto = null,
            int page = 1,
            int pageSize = 25)
        {
            try
            {
                var lista = CDActivoSoftware.Instancia
                                .ListarPorActivo(idActivo, desde, hasta, texto)
                            ?? new List<ActivoSoftware>();

                var ordered = lista
                    .OrderByDescending(s => s.FechaInstalacion ?? DateTime.MinValue)
                    .ThenBy(s => s.Nombre ?? string.Empty)
                    .ThenByDescending(s => s.IdSoftware);

                var total = ordered.Count();
                var pageCount = (int)Math.Ceiling(total / (double)pageSize);
                if (page < 1) page = 1;
                if (page > pageCount && pageCount > 0) page = pageCount;

                var pageData = ordered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.Page = page;
                ViewBag.TotalPages = pageCount;

                return PartialView("_SoftwareTabla", pageData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Software (GET): " + ex.Message);
                return PartialView("_SoftwareTabla", Enumerable.Empty<ActivoSoftware>());
            }
        }

        [HttpGet]
        public ActionResult SoftwareLista(
            int idActivo,
            DateTime? desde = null,
            DateTime? hasta = null,
            string q = null,
            int page = 1,
            int pageSize = 25)
        {
            return Software(idActivo, desde, hasta, q, page, pageSize);
        }

        [HttpGet]
        public ActionResult SoftwareForm(int idActivo, int idSoftware = 0)
        {
            ActivoSoftware model;

            if (idSoftware > 0)
            {
                model = CDActivoSoftware.Instancia
                         .ListarPorActivo(idActivo)
                         .FirstOrDefault(x => x.IdSoftware == idSoftware);

                if (model == null)
                    return Content("<div class='p-3 text-danger'>Software no encontrado.</div>");
            }
            else
            {
                model = new ActivoSoftware
                {
                    IdSoftware = 0,
                    IdActivo = idActivo,
                    FechaInstalacion = null,
                    FechaRegistro = DateTime.Now
                };
            }

            return PartialView("_SoftwareForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarSoftware(
            int idActivo,
            string nombre,
            string editor,
            DateTime? fechaInstalacion,
            string tamano,
            string version)
        {
            try
            {
                if (idActivo <= 0)
                    return Json(new { ok = false, msg = "Activo inválido." });

                if (string.IsNullOrWhiteSpace(nombre))
                    return Json(new { ok = false, msg = "El nombre del software es obligatorio." });

                var s = new ActivoSoftware
                {
                    IdActivo = idActivo,
                    Nombre = nombre?.Trim(),
                    Editor = string.IsNullOrWhiteSpace(editor) ? null : editor.Trim(),
                    FechaInstalacion = fechaInstalacion,
                    Tamano = string.IsNullOrWhiteSpace(tamano) ? null : tamano.Trim(),
                    Version = string.IsNullOrWhiteSpace(version) ? null : version.Trim(),
                    IdUsuarioCrea = GetUsuarioActualId()
                };

                var (id, ok) = CDActivoSoftware.Instancia.Registrar(s);
                if (!ok) return Json(new { ok = false, msg = "No se pudo registrar el software." });

                return Json(new { ok = true, msg = "Software registrado.", idGenerado = id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error AgregarSoftware: " + ex.Message);
                return Json(new { ok = false, msg = "Ocurrió un error al registrar el software." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarSoftware(
            int idSoftware,
            int idActivo,
            string nombre,
            string editor,
            DateTime? fechaInstalacion,
            string tamano,
            string version)
        {
            try
            {
                if (idSoftware <= 0 || idActivo <= 0)
                    return Json(new { ok = false, msg = "Parámetros inválidos." });

                if (string.IsNullOrWhiteSpace(nombre))
                    return Json(new { ok = false, msg = "El nombre del software es obligatorio." });

                var s = new ActivoSoftware
                {
                    IdSoftware = idSoftware,
                    IdActivo = idActivo,
                    Nombre = nombre?.Trim(),
                    Editor = string.IsNullOrWhiteSpace(editor) ? null : editor.Trim(),
                    FechaInstalacion = fechaInstalacion,
                    Tamano = string.IsNullOrWhiteSpace(tamano) ? null : tamano.Trim(),
                    Version = string.IsNullOrWhiteSpace(version) ? null : version.Trim()
                };

                var ok = CDActivoSoftware.Instancia.Editar(s);
                if (!ok) return Json(new { ok = false, msg = "No se pudo editar el software." });

                return Json(new { ok = true, msg = "Software actualizado." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error EditarSoftware: " + ex.Message);
                return Json(new { ok = false, msg = "Ocurrió un error al editar el software." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarSoftware(int idSoftware, int idActivo)
        {
            try
            {
                var ok = CDActivoSoftware.Instancia.Eliminar(idSoftware, idActivo);
                if (!ok) return Json(new { ok = false, msg = "No se pudo eliminar el software." });

                return Json(new { ok = true, msg = "Software eliminado." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error EliminarSoftware: " + ex.Message);
                return Json(new { ok = false, msg = "Ocurrió un error al eliminar el software." });
            }
        }

        #endregion

        /* ===============================================================
           MANTENIMIENTOS POR ACTIVO
           =============================================================== */
        #region Mantenimientos

        [HttpGet]
        public ActionResult Mantenimientos(
            int idActivo,
            DateTime? desde = null,
            DateTime? hasta = null,
            string texto = null,
            string tipo = null,
            int page = 1,
            int pageSize = 25)
        {
            try
            {
                var lista = CDActivoMantenimiento.Instancia
                                .ListarPorActivo(idActivo, desde, hasta, texto, tipo)
                            ?? new List<ActivoMantenimiento>();

                var ordered = lista
                    .OrderByDescending(m => m.FechaMantenimiento)
                    .ThenByDescending(m => m.IdMantenimiento);

                var total = ordered.Count();
                var pageCount = (int)Math.Ceiling(total / (double)pageSize);
                if (page < 1) page = 1;
                if (page > pageCount && pageCount > 0) page = pageCount;

                var pageData = ordered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.Page = page;
                ViewBag.TotalPages = pageCount;

                return PartialView("_MantenimientosTabla", pageData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Mantenimientos (GET): " + ex.Message);
                return PartialView("_MantenimientosTabla", Enumerable.Empty<ActivoMantenimiento>());
            }
        }

        [HttpGet]
        public ActionResult MantenimientosLista(
            int idActivo,
            DateTime? desde = null,
            DateTime? hasta = null,
            string q = null,
            string tipo = null,
            int page = 1,
            int pageSize = 25)
        {
            return Mantenimientos(idActivo, desde, hasta, q, tipo, page, pageSize);
        }

        [HttpGet]
        public ActionResult MantenimientoForm(int idActivo, int idMantenimiento = 0)
        {
            ActivoMantenimiento model;

            if (idMantenimiento > 0)
            {
                model = CDActivoMantenimiento.Instancia
                            .ListarPorActivo(idActivo)
                            .FirstOrDefault(x => x.IdMantenimiento == idMantenimiento);

                if (model == null)
                    return Content("<div class='p-3 text-danger'>Mantenimiento no encontrado.</div>");
            }
            else
            {
                model = new ActivoMantenimiento
                {
                    IdMantenimiento = 0,
                    IdActivo = idActivo,
                    FechaMantenimiento = DateTime.Today
                };
            }

            ViewBag.Solicitantes = CD_Solicitantes.Instancia.ObtenerSolicitantes()
                                        .OrderBy(s => s.Nombre)
                                        .ToList();

            ViewBag.Tipos = new[] { "Preventivo", "Correctivo", "Predictivo", "Inspección", "Calibración", "Limpieza", "Actualización", "Reparación" };

            return PartialView("_MantenimientoForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarMantenimiento(int idActivo, int idSolicitante, string tipo, DateTime? fecha, string detalle)
        {
            try
            {
                if (idActivo <= 0)
                    return Json(new { ok = false, msg = "Activo inválido." });

                if (idSolicitante <= 0)
                    return Json(new { ok = false, msg = "El responsable es obligatorio." });

                if (string.IsNullOrWhiteSpace(tipo))
                    return Json(new { ok = false, msg = "El tipo es obligatorio." });

                if (!fecha.HasValue)
                    return Json(new { ok = false, msg = "La fecha es obligatoria." });

                var m = new ActivoMantenimiento
                {
                    IdActivo = idActivo,
                    IdSolicitante = idSolicitante,
                    Tipo = tipo.Trim(),
                    FechaMantenimiento = fecha.Value,
                    Detalle = string.IsNullOrWhiteSpace(detalle) ? null : detalle.Trim(),
                    IdUsuarioCrea = GetUsuarioActualId()
                };

                var (id, ok) = CDActivoMantenimiento.Instancia.Registrar(m);
                if (!ok) return Json(new { ok = false, msg = "No se pudo registrar el mantenimiento." });

                return Json(new { ok = true, msg = "Mantenimiento registrado.", idGenerado = id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error AgregarMantenimiento: " + ex.Message);
                return Json(new { ok = false, msg = "Ocurrió un error al registrar el mantenimiento." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarMantenimiento(int idMantenimiento, int idActivo, int idSolicitante, string tipo, DateTime? fecha, string detalle)
        {
            try
            {
                if (idMantenimiento <= 0 || idActivo <= 0)
                    return Json(new { ok = false, msg = "Parámetros inválidos." });

                if (idSolicitante <= 0)
                    return Json(new { ok = false, msg = "El responsable es obligatorio." });

                if (string.IsNullOrWhiteSpace(tipo))
                    return Json(new { ok = false, msg = "El tipo es obligatorio." });

                if (!fecha.HasValue)
                    return Json(new { ok = false, msg = "La fecha es obligatoria." });

                var ok = CDActivoMantenimiento.Instancia.Editar(
                    new ActivoMantenimiento
                    {
                        IdMantenimiento = idMantenimiento,
                        IdActivo = idActivo,
                        IdSolicitante = idSolicitante,
                        Tipo = tipo.Trim(),
                        FechaMantenimiento = fecha.Value,
                        Detalle = string.IsNullOrWhiteSpace(detalle) ? null : detalle.Trim()
                    });

                if (!ok) return Json(new { ok = false, msg = "No se pudo editar el mantenimiento." });

                return Json(new { ok = true, msg = "Mantenimiento actualizado." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error EditarMantenimiento: " + ex.Message);
                return Json(new { ok = false, msg = "Ocurrió un error al editar el mantenimiento." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarMantenimiento(int idMantenimiento, int idActivo)
        {
            try
            {
                var ok = CDActivoMantenimiento.Instancia.Eliminar(idMantenimiento, idActivo);
                if (!ok) return Json(new { ok = false, msg = "No se pudo eliminar el mantenimiento." });

                return Json(new { ok = true, msg = "Mantenimiento eliminado." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error EliminarMantenimiento: " + ex.Message);
                return Json(new { ok = false, msg = "Ocurrió un error al eliminar el mantenimiento." });
            }
        }
        #endregion

        /* ===============================================================
           ADJUNTOS POR ACTIVO (binario en DB)
           =============================================================== */
        #region Adjuntos

        private static string MapExtFromContentType(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType)) return null;
            var ct = contentType.ToLowerInvariant();

            if (ct == "application/pdf") return ".pdf";
            if (ct == "application/msword") return ".doc";
            if (ct == "application/vnd.ms-excel") return ".xls";
            if (ct == "application/vnd.openxmlformats-officedocument.wordprocessingml.document") return ".docx";
            if (ct == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") return ".xlsx";
            if (ct.StartsWith("image/")) return "." + ct.Substring("image/".Length);
            if (ct == "text/plain") return ".txt";
            if (ct == "application/zip") return ".zip";
            return null;
        }

        private static string EnsureNameHasExtension(string name, string contentType)
        {
            if (string.IsNullOrWhiteSpace(name)) name = "archivo";
            if (name.Contains(".")) return name;

            var ext = MapExtFromContentType(contentType);
            return string.IsNullOrWhiteSpace(ext) ? name : (name + ext);
        }

        [HttpGet]
        public ActionResult Adjuntos(
            int idActivo,
            string texto = null,
            int page = 1,
            int pageSize = 25)
        {
            try
            {
                var lista = CDActivoAdjunto.Instancia.ListarPorActivo(idActivo, texto) ?? new List<ActivoAdjunto>();

                var ordered = lista
                    .OrderByDescending(a => a.FechaRegistro)
                    .ThenByDescending(a => a.IdAdjunto);

                var total = ordered.Count();
                var pageCount = (int)Math.Ceiling(total / (double)pageSize);
                if (page < 1) page = 1;
                if (page > pageCount && pageCount > 0) page = pageCount;

                var pageData = ordered
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.Page = page;
                ViewBag.TotalPages = pageCount;

                return PartialView("_AdjuntosTabla", pageData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Adjuntos (GET): " + ex.Message);
                return PartialView("_AdjuntosTabla", Enumerable.Empty<ActivoAdjunto>());
            }
        }

        [HttpGet]
        public ActionResult AdjuntosLista(
            int idActivo,
            string q = null,
            int page = 1,
            int pageSize = 25)
        {
            return Adjuntos(idActivo, q, page, pageSize);
        }

        [HttpGet]
        public ActionResult AdjuntoForm(int idActivo, int idAdjunto = 0)
        {
            ActivoAdjunto model;

            if (idAdjunto > 0)
            {
                model = CDActivoAdjunto.Instancia
                         .ListarPorActivo(idActivo)
                         .FirstOrDefault(x => x.IdAdjunto == idAdjunto);

                if (model == null)
                    return Content("<div class='p-3 text-danger'>Adjunto no encontrado.</div>");
            }
            else
            {
                model = new ActivoAdjunto
                {
                    IdAdjunto = 0,
                    IdActivo = idActivo,
                    FechaRegistro = DateTime.Now
                };
            }

            return PartialView("_AdjuntoForm", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarAdjunto(int idActivo, string nombreArchivo, System.Web.HttpPostedFileBase archivo)
        {
            try
            {
                if (idActivo <= 0)
                    return Json(new { ok = false, msg = "Activo inválido." });

                if (archivo == null || archivo.ContentLength <= 0)
                    return Json(new { ok = false, msg = "Debe seleccionar un archivo." });

                var originalName = System.IO.Path.GetFileName(archivo.FileName);
                var ext = System.IO.Path.GetExtension(originalName);

                nombreArchivo = (nombreArchivo ?? "").Trim();
                string nombreGuardar;

                if (string.IsNullOrWhiteSpace(nombreArchivo))
                {
                    nombreGuardar = originalName;
                }
                else
                {
                    nombreGuardar = nombreArchivo.Contains(".") || string.IsNullOrEmpty(ext)
                        ? nombreArchivo
                        : nombreArchivo + ext;
                }

                byte[] contenido;
                using (var ms = new MemoryStream())
                {
                    archivo.InputStream.CopyTo(ms);
                    contenido = ms.ToArray();
                }

                var adj = new ActivoAdjunto
                {
                    IdActivo = idActivo,
                    NombreArchivo = nombreGuardar,
                    Contenido = contenido,
                    ContentType = string.IsNullOrWhiteSpace(archivo.ContentType) ? null : archivo.ContentType,
                    TamanoBytes = archivo.ContentLength,
                    IdUsuarioCrea = GetUsuarioActualId()
                };

                var (id, ok) = CDActivoAdjunto.Instancia.Subir(adj);
                if (!ok) return Json(new { ok = false, msg = "No se pudo guardar el adjunto." });

                return Json(new { ok = true, msg = "Adjunto guardado.", idGenerado = id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error AgregarAdjunto: " + ex.Message);
                return Json(new { ok = false, msg = "Ocurrió un error al guardar el adjunto." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarNombreAdjunto(int idAdjunto, int idActivo, string nombreArchivo)
        {
            try
            {
                if (idAdjunto <= 0 || idActivo <= 0)
                    return Json(new { ok = false, msg = "Parámetros inválidos." });

                nombreArchivo = (nombreArchivo ?? "").Trim();
                if (string.IsNullOrWhiteSpace(nombreArchivo))
                    return Json(new { ok = false, msg = "El nombre del archivo es obligatorio." });

                var ok = CDActivoAdjunto.Instancia.Renombrar(idAdjunto, idActivo, nombreArchivo);
                if (!ok) return Json(new { ok = false, msg = "No se pudo renombrar el adjunto." });

                return Json(new { ok = true, msg = "Adjunto renombrado." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error EditarNombreAdjunto: " + ex.Message);
                return Json(new { ok = false, msg = "Ocurrió un error al renombrar el adjunto." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarAdjunto(int idAdjunto, int idActivo)
        {
            try
            {
                var ok = CDActivoAdjunto.Instancia.Eliminar(idAdjunto, idActivo);
                if (!ok) return Json(new { ok = false, msg = "No se pudo eliminar el adjunto." });

                return Json(new { ok = true, msg = "Adjunto eliminado." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error EliminarAdjunto: " + ex.Message);
                return Json(new { ok = false, msg = "Ocurrió un error al eliminar el adjunto." });
            }
        }

        [HttpGet]
        public ActionResult DescargarAdjunto(int idAdjunto)
        {
            try
            {
                var a = CDActivoAdjunto.Instancia.Descargar(idAdjunto);
                if (a == null || a.Contenido == null || a.Contenido.Length == 0)
                    return Content("Archivo no encontrado o vacío.");

                var ct = string.IsNullOrWhiteSpace(a.ContentType) ? "application/octet-stream" : a.ContentType;
                var nombreDescarga = EnsureNameHasExtension(a.NombreArchivo, ct);

                return File(a.Contenido, ct, nombreDescarga);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error DescargarAdjunto: " + ex.Message);
                return Content("No se pudo descargar el archivo.");
            }
        }

        #endregion
    }
}
