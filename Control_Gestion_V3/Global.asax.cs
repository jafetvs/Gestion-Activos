using System;
using System.Web;
using System.Web.Helpers;       // AntiForgery / AntiForgeryConfig
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;      // FormsAuthentication

namespace Control_Gestion_V3
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        /// <summary>
        /// Siembra la cookie AntiForgery al inicio del request si aún no existe.
        /// Evita: "El servidor no puede modificar cookies después de enviar los encabezados HTTP."
        /// cuando se renderiza un formulario con @Html.AntiForgeryToken().
        /// </summary>
        protected void Application_BeginRequest()
        {
            var cookieName = AntiForgeryConfig.CookieName; // por defecto: "__RequestVerificationToken"
            var existing = Request.Cookies[cookieName];

            // Si ya hay cookie válida, no hacemos nada.
            if (existing != null && !string.IsNullOrEmpty(existing.Value))
                return;

            // Genera tokens; nos interesa escribir la cookie si faltaba.
            string cookieToken, formToken;
            AntiForgery.GetTokens(null, out cookieToken, out formToken);

            var antiforgeryCookie = new HttpCookie(cookieName, cookieToken)
            {
                HttpOnly = true,
                Secure = Request.IsSecureConnection,            // usa Secure solo en HTTPS
                Path = FormsAuthentication.FormsCookiePath       // ruta estándar de cookies
            };

            Response.Cookies.Add(antiforgeryCookie);
        }
    }
}
