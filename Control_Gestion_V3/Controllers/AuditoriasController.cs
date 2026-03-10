using CapaDatos;
using CapaModelo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Control_Gestion_V3.Controllers
{
    public class AuditoriasController : Controller
    {
        // GET: Auditorias
        public ActionResult auditorias()
        {
            return View();
        }
        public JsonResult Obtener(string fechainicio, string fechafin, string tabla, string Cedula_Solicitante)
        {
            // Manejo de valores nulos o vacíos para fechas
            DateTime? fechaInicioParsed = !string.IsNullOrEmpty(fechainicio) ? Convert.ToDateTime(fechainicio) : (DateTime?)null;
            DateTime? fechaFinParsed = !string.IsNullOrEmpty(fechafin) ? Convert.ToDateTime(fechafin) : (DateTime?)null;


            // Llamada al método de datos con los filtros, incluyendo la cédula
            List<Auditorias> lista = CD_Auditorias.Instancia.ObtenerAuditoria(fechaInicioParsed, fechaFinParsed, tabla, Cedula_Solicitante);

            // Manejo de lista nula
            if (lista == null)
                lista = new List<Auditorias>();
            // Retornar el resultado en formato JSON
            return Json(new { data = lista }, JsonRequestBehavior.AllowGet);
        }

    }
}