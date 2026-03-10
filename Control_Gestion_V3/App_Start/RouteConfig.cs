using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Control_Gestion_V3
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
          /*  routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
            */
            // Definir la ruta predeterminada como Login/Index
            routes.MapRoute(
              name: "Default",
              url: "{controller}/{action}/{id}",
              defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
          );

            routes.MapRoute(
                name: "Login",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Login", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
            name: "EliminarArchivo",
            url: "Intervenciones/EliminarArchivo/{id}",
            defaults: new { controller = "Intervenciones", action = "EliminarArchivo" },
            constraints: new { id = @"\d+" } // Solo restringimos el ID a números
            );

            // Asegúrate de que la ruta de Casos esté accesible
            routes.MapRoute(
                name: "ActualizarCasos",
                url: "Casos/Actualizar/{id}",
                defaults: new { controller = "Casos", action = "Actualizar", id = UrlParameter.Optional }
            );
            // Asegúrate de que la ruta de Casos esté accesible
            routes.MapRoute(
                name: "NotasInternas",
                url: "Casos/ActualizarNotasInternas/{id}",
                defaults: new { controller = "Casos", action = "ActualizarNotasInternas", id = UrlParameter.Optional }
            );

        }
    }
}
