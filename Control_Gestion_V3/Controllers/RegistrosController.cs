using CapaDatos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Control_Gestion_V3.Controllers
{
    public class RegistrosController : Controller
    {
        // GET: Registros
        public ActionResult RecuperarPassword()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult NewContrasena()
        {
            return View();
        }
        public ActionResult CrearCuenta()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult EnvioCorreo(string correo)
        {
            bool respuesta = false;

            if (string.IsNullOrEmpty(correo))
            {
                ViewBag.Mensaje = "Ingrese su correo electrónico.";
                return View();
            }
            string nuevoCodigo = CD_NewPassword.Instancia.RecuperarPassword(correo);
            if (nuevoCodigo.Length > 0)
            {
                Correos envio = new Correos();
                respuesta = envio.EnviarCorreo(correo, nuevoCodigo);
            }
            return Json(new { resultado = respuesta }, JsonRequestBehavior.AllowGet);
        }

        // Procesar el cambio de contraseña
        [HttpPost]
        public ActionResult NewContrasena(string correoHidden, string codigo, string inputPassword)
        {

            // Validar código de recuperación
            bool esCodigoValido = CD_NewPassword.Instancia.ValidarCodigo(correoHidden, codigo);
            if (!esCodigoValido)
            {
                ViewBag.Error = "Código inválido o expirado.";
                return View();
            }

            // Intentar cambiar la contraseña
            bool cambioExitoso = CD_NewPassword.Instancia.CambiarContraseña(correoHidden, inputPassword);

            if (cambioExitoso)
            {
                // Redirigir al login si todo fue exitoso
                return RedirectToAction("index", "Login");
            }
            else
            {
                ViewBag.Error = "Ocurrió un error al cambiar la contraseña.";
                return View();
            }
        }





    }
}