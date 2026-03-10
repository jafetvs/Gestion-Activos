using CapaDatos;
using CapaModelo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Control_Gestion_V3.Controllers
{
    public class SolicitantesController : Controller
    {
        // GET: Solicitantes
        public ActionResult solicitantes()
        {
            return View();
        }
        public JsonResult Obtener()
        {
            List<Solicitantes> oListaPartes = CD_Solicitantes.Instancia.ObtenerSolicitantes();
            return Json(new { data = oListaPartes }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult EditarSolicitante()
        {
            return View();
        }
        public JsonResult EditarUnSolicitante(string Cedula_Solicitante)
        {
            var datos = CD_Solicitantes.Instancia.ObtenerUnSolicitante(Cedula_Solicitante);
            return Json(new { data = datos }, JsonRequestBehavior.AllowGet);
        }
       /* public JsonResult UnSolicitante(string cedula)
        {
            String sinDatos = null;
            var datos = CD_Solicitantes.Instancia.ObtenerUnSolicitante(cedula);
            if (datos != null)
            {
                var solicitante = new Partes
                {
                    Cedula = datos.Cedula_Solicitante,
                    Nombre = datos.Nombre,
                    //  Telefono = datos.Telefono,

                };
                return Json(new { data = datos }, JsonRequestBehavior.AllowGet);
                //     return Json(new { data = solicitante }, JsonRequestBehavior.AllowGet);
            }

            // Devuelve los datos del solicitante como JSON
            return Json(new { data = sinDatos }, JsonRequestBehavior.AllowGet);
        }*/
        public JsonResult Guardar(Solicitantes objeto)
        {
            bool respuesta = false;

            if (objeto.ID_Usuario == 0)
            {
                respuesta = CD_Solicitantes.Instancia.RegistrarSolicitante(objeto);
            }
            else
            {
                respuesta = CD_Solicitantes.Instancia.ModificarSolicitante(objeto);
            }
            return Json(new { resultado = respuesta }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Eliminar(string Cedula_Solicitante)
        {
            bool respuesta = CD_Solicitantes.Instancia.EliminarSolicitante(Cedula_Solicitante);
            return Json(new { resultado = respuesta }, JsonRequestBehavior.AllowGet);
        }
    }
}