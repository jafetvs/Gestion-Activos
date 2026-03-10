using CapaModelo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Control_Gestion_V3.Controllers
{
    public class HomeController : PadreController
    {
        private static Usuario SesionUsuario;
        public ActionResult Index()
        {

          /*  if (Session["Usuario"] != null)
                SesionUsuario = (Usuario)Session["Usuario"];
            else
            {
                SesionUsuario = new Usuario();
            }
            try
            {
                ViewBag.NombreUsuario = SesionUsuario.Nom_User;
                //   ViewBag.NombreUsuario = SesionUsuario.Nom_Rol + " " + SesionUsuario.Nom_User;
                ViewBag.RolUsuario = SesionUsuario.oRol.Descripcion;
                ViewBag.Cedula = SesionUsuario.Cedula_Usuario;
            }
            catch (Exception ex)
            {
                // Log el error
                System.Diagnostics.Debug.WriteLine(ex.Message);
                ViewBag.NombreUsuario = "Usuario desconocido";
                ViewBag.RolUsuario = "Sin rol asignado";
            }
            */

            return View();
        }

      

    }
}