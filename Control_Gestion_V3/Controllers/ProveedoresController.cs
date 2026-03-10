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
    [Authorize]
    public class ProveedoresController : PadreController
    {
        // ================== Helpers ==================
        private int GetUsuarioActualId()
        {
            var u = Session["Usuario"] as Usuario;
            return (u != null ? u.IdUsuario : 0);
        }

        // ================== Rutas base ==================
        public ActionResult Index(
            string texto = null,
            string servicio = null,
            string noContrato = null,
            string noProcedimiento = null,
            bool? renovacionContrato = null,
            bool? activo = null,
            DateTime? venceDesde = null,
            DateTime? venceHasta = null)
        {
            return RedirectToAction("Proveedores", new
            {
                texto,
                servicio,
                noContrato,
                noProcedimiento,
                renovacionContrato,
                activo,
                venceDesde,
                venceHasta
            });
        }

        // ================== Vista principal ==================
        [HttpGet]
        public ActionResult Proveedores(
            string texto = null,
            string servicio = null,
            string noContrato = null,
            string noProcedimiento = null,
            bool? renovacionContrato = null,
            bool? activo = null,
            DateTime? venceDesde = null,
            DateTime? venceHasta = null)
        {
            try
            {
                var data = CDProveedor.Instancia.Listar(
                    texto, servicio, noContrato, noProcedimiento,
                    renovacionContrato, activo, venceDesde, venceHasta,
                    idProveedor: null
                );

                // Mantén los filtros en ViewBag para la vista
                ViewBag.FTexto = texto;
                ViewBag.FServicio = servicio;
                ViewBag.FNoContrato = noContrato;
                ViewBag.FNoProcedimiento = noProcedimiento;
                ViewBag.FRenovacion = renovacionContrato;
                ViewBag.FActivo = activo;
                ViewBag.FVenceDesde = venceDesde;
                ViewBag.FVenceHasta = venceHasta;

                return View("proveedores", data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Proveedores (GET): " + ex.Message);
                TempData["Error"] = "Ocurrió un error al cargar la lista de proveedores.";
                return View("proveedores", new List<Proveedor>());
            }
        }

        // ================== Filtrar (AJAX/Partial) ==================
        [HttpGet]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult Filtrar(
            string texto = null,
            string servicio = null,
            string noContrato = null,
            string noProcedimiento = null,
            bool? renovacionContrato = null,
            bool? activo = null,
            DateTime? venceDesde = null,
            DateTime? venceHasta = null)
        {
            try
            {
                var lista = CDProveedor.Instancia.Listar(
                    texto, servicio, noContrato, noProcedimiento,
                    renovacionContrato, activo, venceDesde, venceHasta,
                    idProveedor: null
                );

                return PartialView("_ProveedoresFilas", lista);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Filtrar Proveedores: " + ex.Message);
                return PartialView("_ProveedoresFilas", Enumerable.Empty<Proveedor>());
            }
        }

        // ================== Detalle (solo lectura) ==================
        [HttpGet]
        public ActionResult Detalle(int id, string returnUrl = null)
        {
            try
            {
                // Traer por ID usando el SP de listar, sin filtrar por estado
                var prov = CDProveedor.Instancia.Listar(
                    texto: null,
                    servicio: null,
                    noContrato: null,
                    noProcedimiento: null,
                    renovacionContrato: null,
                    activo: null,
                    venceDesde: null,
                    venceHasta: null,
                    idProveedor: id
                ).FirstOrDefault();

                if (prov == null)
                {
                    TempData["Error"] = "Proveedor no encontrado.";
                    return RedirectToAction("Proveedores");
                }

                if (string.IsNullOrWhiteSpace(returnUrl) && Request?.UrlReferrer != null)
                    returnUrl = Request.UrlReferrer.PathAndQuery;
                ViewBag.ReturnUrl = returnUrl;

                return View("Detalle", prov);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Detalle Proveedor: " + ex.Message);
                TempData["Error"] = "Ocurrió un error al cargar el detalle del proveedor.";
                return RedirectToAction("Proveedores");
            }
        }

        // ================== Crear ==================
        [HttpGet]
        public ActionResult Crear()
        {
            return View(new Proveedor
            {
                Activo = true,
                RenovacionContrato = false
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Crear(Proveedor model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var (id, ok) = CDProveedor.Instancia.Registrar(model, GetUsuarioActualId());
                if (!ok || id <= 0)
                {
                    TempData["Error"] = "No se pudo registrar el proveedor.";
                    return View(model);
                }

                TempData["Ok"] = "Proveedor registrado correctamente.";
                return RedirectToAction("Proveedores");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al registrar: " + ex.Message;
                return View(model);
            }
        }

        // ================== Editar ==================
        [HttpGet]
        public ActionResult Editar(int id)
        {
            var prov = CDProveedor.Instancia.Listar(
                texto: null,
                servicio: null,
                noContrato: null,
                noProcedimiento: null,
                renovacionContrato: null,
                activo: null,
                venceDesde: null,
                venceHasta: null,
                idProveedor: id
            ).FirstOrDefault();

            if (prov == null)
            {
                TempData["Error"] = "Proveedor no encontrado.";
                return RedirectToAction("Proveedores");
            }
            return View(prov);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Editar(Proveedor model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                bool ok = CDProveedor.Instancia.Modificar(model);
                if (!ok)
                {
                    TempData["Error"] = "No se pudo modificar el proveedor.";
                    return View(model);
                }

                TempData["Ok"] = "Proveedor actualizado correctamente.";
                return RedirectToAction("Proveedores");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al modificar: " + ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id)
        {
            try
            {
                // Mensaje específico si tiene activos
                if (CDProveedor.Instancia.TieneActivosAsociados(id))
                {
                    var msg = "No se pudo eliminar: hay activos asociados a este proveedor.";
                    if (Request.IsAjaxRequest())
                        return Json(new { ok = false, reason = "inuse", message = msg });
                    TempData["Error"] = msg;
                    return RedirectToAction("Proveedores");
                }

                bool ok = CDProveedor.Instancia.Eliminar(id);

                if (Request.IsAjaxRequest())
                    return Json(new
                    {
                        ok,
                        reason = ok ? null : "failed",
                        message = ok ? "Proveedor eliminado correctamente."
                                     : "No se pudo eliminar el proveedor."
                    });

                if (ok) TempData["Ok"] = "Proveedor eliminado correctamente.";
                else TempData["Error"] = "No se pudo eliminar el proveedor.";
            }
            catch (Exception ex)
            {
                var msg = "Error al eliminar: " + ex.Message;
                if (Request.IsAjaxRequest())
                    return Json(new { ok = false, reason = "error", message = msg });
                TempData["Error"] = msg;
            }

            return RedirectToAction("Proveedores");
        }


        // ================== Exportar Excel ==================
        [HttpGet]
        public ActionResult ExportarExcel(
            string texto = null,
            string servicio = null,
            string noContrato = null,
            string noProcedimiento = null,
            bool? renovacionContrato = null,
            bool? activo = null,
            DateTime? venceDesde = null,
            DateTime? venceHasta = null)
        {
            try
            {
                var data = CDProveedor.Instancia.Listar(
                    texto, servicio, noContrato, noProcedimiento,
                    renovacionContrato, activo, venceDesde, venceHasta,
                    idProveedor: null
                ).ToList();

                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("Proveedores");

                    // Encabezados
                    string[] headers = new[]
                    {
                        "Proveedor","Servicio","N° Contrato","N° Procedimiento",
                        "Renovación","Vence","Contacto 1","Teléfono 1","Correo 1",
                        "Contacto 2","Teléfono 2","Correo 2","Web","Activo","F. Registro"
                    };
                    for (int i = 0; i < headers.Length; i++)
                        ws.Cell(1, i + 1).Value = headers[i];

                    var header = ws.Range(1, 1, 1, headers.Length);
                    header.Style.Font.Bold = true;
                    header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.SheetView.FreezeRows(1);

                    int row = 2;
                    foreach (var p in data)
                    {
                        ws.Cell(row, 1).Value = p.NombreEmpresa ?? "";
                        ws.Cell(row, 2).Value = p.Servicio ?? "";
                        ws.Cell(row, 3).Value = p.NoContrato ?? "";
                        ws.Cell(row, 4).Value = p.NoProcedimiento ?? "";
                        ws.Cell(row, 5).Value = p.RenovacionContrato ? "Sí" : "No";
                        ws.Cell(row, 6).Value = p.FechaVencimientoContrato;
                        if (p.FechaVencimientoContrato.HasValue)
                            ws.Cell(row, 6).Style.DateFormat.Format = "dd/MM/yyyy";

                        ws.Cell(row, 7).Value = p.Contacto1_Nombre ?? "";
                        ws.Cell(row, 8).Value = p.Contacto1_Telefono ?? "";
                        ws.Cell(row, 9).Value = p.Contacto1_Correo ?? "";
                        ws.Cell(row, 10).Value = p.Contacto2_Nombre ?? "";
                        ws.Cell(row, 11).Value = p.Contacto2_Telefono ?? "";
                        ws.Cell(row, 12).Value = p.Contacto2_Correo ?? "";
                        ws.Cell(row, 13).Value = p.PaginaWeb ?? "";
                        ws.Cell(row, 14).Value = p.Activo ? "Sí" : "No";
                        ws.Cell(row, 15).Value = p.FechaRegistro;
                        ws.Cell(row, 15).Style.DateFormat.Format = "dd/MM/yyyy";
                        row++;
                    }

                    try { ws.Columns().AdjustToContents(); } catch { }

                    using (var ms = new MemoryStream())
                    {
                        wb.SaveAs(ms);
                        return File(ms.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "Proveedores.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ExportarExcel Proveedores: " + ex.Message);
                TempData["Error"] = "Ocurrió un error al exportar a Excel.";
                return RedirectToAction("Proveedores", new
                {
                    texto,
                    servicio,
                    noContrato,
                    noProcedimiento,
                    renovacionContrato,
                    activo,
                    venceDesde,
                    venceHasta
                });
            }
        }

        // ================== Exportar PDF ==================
        [HttpGet]
        public ActionResult ExportarPdf(
            string texto = null,
            string servicio = null,
            string noContrato = null,
            string noProcedimiento = null,
            bool? renovacionContrato = null,
            bool? activo = null,
            DateTime? venceDesde = null,
            DateTime? venceHasta = null)
        {
            try
            {
                var data = CDProveedor.Instancia.Listar(
                    texto, servicio, noContrato, noProcedimiento,
                    renovacionContrato, activo, venceDesde, venceHasta,
                    idProveedor: null
                ).ToList();

                using (var ms = new MemoryStream())
                {
                    var doc = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
                    PdfWriter.GetInstance(doc, ms);
                    doc.Open();

                    var title = new Paragraph("Listado de Proveedores")
                    {
                        Alignment = Element.ALIGN_CENTER,
                        SpacingAfter = 10f
                    };
                    doc.Add(title);

                    var table = new PdfPTable(10) { WidthPercentage = 100 };
                    table.SetWidths(new float[] { 20f, 15f, 12f, 12f, 10f, 10f, 16f, 14f, 14f, 10f });

                    // Encabezados (resumen útil)
                    string[] headers = new[]
                    {
                        "Proveedor","Servicio","N° Contrato","N° Proced.","Renov.","Vence",
                        "Contacto 1","Correo 1","Web","Activo"
                    };
                    foreach (var h in headers) table.AddCell(h);

                    foreach (var p in data)
                    {
                        table.AddCell(p.NombreEmpresa ?? "");
                        table.AddCell(p.Servicio ?? "");
                        table.AddCell(p.NoContrato ?? "");
                        table.AddCell(p.NoProcedimiento ?? "");
                        table.AddCell(p.RenovacionContrato ? "Sí" : "No");
                        table.AddCell(p.FechaVencimientoContrato.HasValue ? p.FechaVencimientoContrato.Value.ToString("dd/MM/yyyy") : "-");
                        table.AddCell(p.Contacto1_Nombre ?? "");
                        table.AddCell(p.Contacto1_Correo ?? "");
                        table.AddCell(p.PaginaWeb ?? "");
                        table.AddCell(p.Activo ? "Sí" : "No");
                    }

                    doc.Add(table);
                    doc.Close();

                    return File(ms.ToArray(), "application/pdf", "Proveedores.pdf");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ExportarPdf Proveedores: " + ex.Message);
                TempData["Error"] = "Ocurrió un error al exportar a PDF.";
                return RedirectToAction("Proveedores", new
                {
                    texto,
                    servicio,
                    noContrato,
                    noProcedimiento,
                    renovacionContrato,
                    activo,
                    venceDesde,
                    venceHasta
                });
            }
        }
    }
}
