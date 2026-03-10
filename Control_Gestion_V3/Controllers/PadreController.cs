using CapaModelo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Control_Gestion_V3.Controllers
{
    public class PadreController : Controller
    {
        // GET: Padre
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["Usuario"] is Usuario usuario)
            {
                ViewBag.RolUsuario = usuario.Nom_Rol;
                ViewBag.NombreUsuario = usuario.Nom_User;
                ViewBag.Nombre = usuario.Nom_Completo;
                ViewBag.Cedula_Usuario = usuario.Cedula_Usuario;
            }
            else
            {
                ViewBag.NombreUsuario = "Usuario no identificado";
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
