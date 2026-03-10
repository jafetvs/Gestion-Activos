using CapaModelo;
using Control_Gestion_V3.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Control_Gestion_V3.Filters
{
    public class VerificarSession : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            Usuario oUsuario = (Usuario)HttpContext.Current.Session["Usuario"];
            if (oUsuario == null)
            {


                // Verificar si la solicitud no es AJAX
                if (!filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    // Verificar si la acción no es Login/Index o Registros/CrearCuenta
                    if (!(filterContext.Controller is LoginController) || filterContext.ActionDescriptor.ActionName != "Index")
                    {
                        if (!(filterContext.Controller is RegistrosController &&
                           (filterContext.ActionDescriptor.ActionName == "CrearCuenta" ||
                            filterContext.ActionDescriptor.ActionName == "RecuperarPassword" ||
                            filterContext.ActionDescriptor.ActionName == "NewContrasena")))
                        {
                            filterContext.HttpContext.Response.Redirect("~/Login/Index");
                        }
                    }
                }
            }
            else
            {// Si el usuario ya está autenticado y está en LoginController (Página de login), redirigir a la página principal
                if (filterContext.Controller is LoginController && filterContext.ActionDescriptor.ActionName == "Index")
                {
                    filterContext.HttpContext.Response.Redirect("~/Home/Index");
                }
            }
            base.OnActionExecuting(filterContext);
        }
    }
}