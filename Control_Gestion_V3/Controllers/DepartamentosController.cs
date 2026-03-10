using CapaDatos;
using CapaModelo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Control_Gestion_V3.Controllers
{
    public class DepartamentosController : Controller
    {
        // GET: Departamentos
        public ActionResult departamentos()
        {
            return View();
        }
        public JsonResult Obtener()
        {
            // Obtiene la lista de departamentos activos
            List<Departamentos> olista = CD_Departamentos.Instancia.ObtenerDepartamentos();
            return Json(new { data = olista }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult Guardar(Departamentos objeto)
        {
            bool respuesta = false;
            try
            {
                if (objeto.Id_Departamento == 0)  // Verifica si es un nuevo registro
                {
                    // Llamada al método para registrar el nuevo departamento
                    respuesta = CD_Departamentos.Instancia.RegistrarDepartamento(objeto);
                }
                else
                {
                    // Llamada al método para modificar un departamento existente
                    respuesta = CD_Departamentos.Instancia.ModificarDepartamento(objeto);
                }
            }
            catch (Exception ex)
            {
                // Manejar cualquier excepción
                Console.WriteLine($"Error: {ex.Message}");
                respuesta = false;
            }
            // Retornar la respuesta como JSON
            return Json(new { resultado = respuesta }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult Eliminar(int id = 0)
        {
            bool respuesta = CD_Departamentos.Instancia.EliminarDepartamento(id);
            return Json(new { resultado = respuesta }, JsonRequestBehavior.AllowGet);
        }
    }
}