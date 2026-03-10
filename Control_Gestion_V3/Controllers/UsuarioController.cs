using CapaDatos;
using CapaModelo;
using Control_Gestion_V3.Encriptaciones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Control_Gestion_V3.Controllers
{
    public class UsuarioController : PadreController
    {
        // GET: Usuario
        public ActionResult usuario()
        {
            return View();
        }
        public ActionResult perfil()
        {
            return View();
        }
        //OBTENER LISTA DE USUARIOS 
        public JsonResult Obtener()
        {
            List<Usuario> oListaUsuario = CD_Usuario.Instancia.ObtenerUsuarios();
            return Json(new { data = oListaUsuario }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult EditarUsuario()
        {
            return View();
        }
        //OBTENER LOS DATOS DEL USUARIO PREDETERMINADO
        public JsonResult ObtenerUnUsuario(string Cedula_Usuario)
        {
            Usuario usuario = CD_Usuario.Instancia.ObtenerUnUsuario(Cedula_Usuario);
            return Json(new { data = usuario }, JsonRequestBehavior.AllowGet);
        }
        //MODIFICAR O GUARDAR UN NUEVO USUARIO 
        [HttpPost]
        public JsonResult Guardar(Usuario objeto)
        {
            bool respuesta = false;
            try
            {
                if (objeto.IdUsuario == 0)
                {
                    objeto.Clave = Encriptar.contrasena(objeto.Nom_User);
                    if (CD_Usuario.Instancia.RegistrarUsuario(objeto))
                    {// Mapear y registrar Solicitantes si RegistrarUsuario es exitoso
                        Solicitantes solicitantes = new Solicitantes
                        {
                            TipoDocumento = "N/A",
                            Cedula_Solicitante = objeto.Cedula_Usuario,
                            Nombre = objeto.Nom_Completo,
                            Direccion = objeto.Direccion,
                            Telefono = objeto.Telefono1,
                            Telefono2 = objeto.Telefono2,
                            Correo = objeto.Correo,
                            Activo = objeto.Activo,
                        };

                        respuesta = CD_Solicitantes.Instancia.RegistrarSolicitante(solicitantes);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(objeto.Clave))
                    {
                        objeto.Clave = Encriptar.GetSHA256(objeto.Clave);
                    }
                    respuesta = CD_Usuario.Instancia.ModificarUsuario(objeto);
                }
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción
                Console.WriteLine($"Error: {ex.Message}");
                respuesta = false;
            }
            return Json(new { resultado = respuesta }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Eliminar(string Cedula_Usuario)
        {
            bool respuesta = CD_Usuario.Instancia.EliminarUsuario(Cedula_Usuario);

            return Json(new { resultado = respuesta }, JsonRequestBehavior.AllowGet);
        }

    }
}