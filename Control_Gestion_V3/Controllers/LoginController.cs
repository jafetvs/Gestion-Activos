using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Control_Gestion_V3.Encriptaciones;
using CapaDatos;
using CapaModelo;
using System.Web.Security;
using System.Web.Optimization;
using System.Web.UI.WebControls;

namespace Control_Gestion_V3.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Ya está logueado, redirigir al Home u otra página
                return RedirectToAction("Index", "Home");
            }

            // Evitar caché del navegador
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();

            return View();
        }
        public ActionResult vistaPrueba()
        {
           

            return View();
        }

        [HttpPost]
        public ActionResult Index(string Nom_User, string clave, bool? rememberMe)
        {
            try
            {
                Usuario ousuario = CD_Usuario.Instancia.ObtenerUsuarios()
                    .FirstOrDefault(u => u.Nom_User == Nom_User && u.Clave == Encriptar.GetSHA256(clave));

                if (ousuario == null)
                {
                    ViewBag.Error = "Usuario o contraseña no correcta";
                    return View();
                }

                // Guardar usuario en sesión
                Session["Usuario"] = ousuario;

                // Manejo del "Recordar contraseña"
                if (rememberMe == true)
                {
                    FormsAuthentication.SetAuthCookie(Nom_User, true); // Cookie persistente
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(Nom_User, false); // Solo para la sesión actual
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Ocurrió un error al intentar iniciar sesión. Por favor, intente nuevamente o comuníquese con soporte.";
                return View();
            }
        }
        public ActionResult Salir()
        {
            // Cierra la sesión del usuario
            Session.Clear();
            Session.Abandon();


            // (Opcional) Borra manualmente todas las cookies si deseas
            if (Request.Cookies[".ASPXAUTH"] != null)
            {
                var cookie = new HttpCookie(".ASPXAUTH")
                {
                    Expires = DateTime.Now.AddDays(-1),
                    HttpOnly = true
                };
                Response.Cookies.Add(cookie);
            }

            return RedirectToAction("Index", "Login");
        }
    }
}
