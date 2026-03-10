using CapaDatos;
using CapaModelo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Control_Gestion_V3.NavBar
{
    public static class AccesoNavBar
    {
        public static MvcHtmlString ActionLinkAllow(this HtmlHelper helper)
        {
            StringBuilder sb = new StringBuilder();

            if (HttpContext.Current.Session["Usuario"] != null)
            {
                Usuario oUsuario = (Usuario)HttpContext.Current.Session["Usuario"];
                Usuario rptUsuario = CD_Usuario.Instancia.ObtenerDetalleUsuario(oUsuario.IdUsuario);

                foreach (Menu item in rptUsuario.oListaMenu)
                {
                    // Contenedor de cada sección del menú
                    sb.AppendLine("<a class='nav-link collapsed' href='#' data-bs-toggle='collapse' data-bs-target='#" + item.Nombre.Replace(" ", "") + "' aria-expanded='false' aria-controls='" + item.Nombre.Replace(" ", "") + "'>");
                    sb.AppendLine("<div class='sb-nav-link-icon'><i class='" + item.Icono + "'></i></div>");
                    sb.AppendLine(item.Nombre);
                    sb.AppendLine("<div class='sb-sidenav-collapse-arrow'><i class='fas fa-angle-down'></i></div>");
                    sb.AppendLine("</a>");

                    // Submenú desplegable
                    sb.AppendLine("<div class='collapse' id='" + item.Nombre.Replace(" ", "") + "' aria-labelledby='headingOne' data-bs-parent='#sidenavAccordion'>");
                    sb.AppendLine("<nav class='sb-sidenav-menu-nested nav'>");

                    foreach (SubMenu subitem in item.oSubMenu)
                    {
                        if (subitem.Activo == true)
                        {
                            sb.AppendLine("<a class='nav-link' href='/" + subitem.Controlador + "/" + subitem.Vista + "'>");
                            //  sb.AppendLine("<i class='" + subitem.Icono + "'></i> " + subitem.Nombre);
                            sb.AppendLine("<i class='" + subitem.Icono + "' style='margin-right: 5px;'></i>" + subitem.Nombre);
                            sb.AppendLine("</a>");
                        }
                    }
                    sb.AppendLine("</nav>");
                    sb.AppendLine("</div>");
                }
            }

            return new MvcHtmlString(sb.ToString());
        }
    }
}