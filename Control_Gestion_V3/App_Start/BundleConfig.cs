using System.Web;
using System.Web.Optimization;

namespace Control_Gestion_V3
{
    public class BundleConfig
    {
        // Para obtener más información sobre las uniones, visite https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Utilice la versión de desarrollo de Modernizr para desarrollar y obtener información sobre los formularios.  De esta manera estará
            // para la producción, use la herramienta de compilación disponible en https://modernizr.com para seleccionar solo las pruebas que necesite.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/Complementos").Include(
                    /* "~/Scripts/jquery.jquery-3.7.0.min.js",*/
                     "~/Scripts/jquery.validate.min.js",
                     "~/Scripts/fontawesome/all.min.js",
                     "~/Scripts/DataTables/jquery.dataTables.js",
                     "~/Scripts/DataTables/dataTables.buttons.min.js",
                     "~/Scripts/DataTables/buttons.html5.min.js",
                     "~/Scripts/jszip.js",
                     "~/Scripts/DataTables/jquery.dataTables.responsive.min.js",
                     "~/Content/sweetalert/sweetalert.js",
                    
                     "~/Scripts/scripts.js"
                     ));//BOTÓN OCULTAR MENU PRINCIPAL

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                   "~/Scripts/bootstrap.bundle.main.js",
                   "~/Scripts/bootstrap.bundle.js"));
            //FORMATOS CALENDARIOS 
           bundles.Add(new StyleBundle("~/JQUERY_UI/Calendarios").Include(
                  //      //JQUERY_UI
                  "~/Scripts/jquery-ui-1.12.1/jquery-ui.min.js",
                  "~/Scripts/jquery-ui-1.12.1/jquery-ui-timepicker-addon.js",
                  "~/Content/jquery-ui-1.12.1/jquery-ui.es.js",

                  //Bootstrap Duallistbox
                 /* "~/Content/Plugins/Bootstrap-Duallistbox/js/jquery.bootstrap-duallistbox.min.js",*/

                  //LOADING OVERLAY
                  "~/Content/jquery-loading-overlay/loadingoverlay.min.js"
                   ));
            bundles.Add(new StyleBundle("~/JQUERY_UI/CalendariosCSS").Include(
                      //JQUERY UI
                      "~/Content/jquery-ui-1.12.1/jquery-ui.min.css",
                      "~/Content/jquery-ui-1.12.1/jquery-ui-timepicker-addon.css"
                     ));

            /* bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                       "~/Scripts/bootstrap.js"));*/

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/jquery-ui.min.css",
                      "~/Content/DataTables/css/jquery.dataTables.css",
                      "~/Content/DataTables/css/buttons.dataTables.min.css",
                      "~/Content/DataTables/css/responsive.dataTables.css",
                      "~/Content/sweetalert/sweetalert.css",
                      "~/Content/css/Menu.css",
                      "~/Content/css/General.css",
                      "~/Content/site.css"));

            // Agregar el bundle para FullCalendar
            bundles.Add(new ScriptBundle("~/bundles/fullcalendar").Include(
                     "~/Scripts/moment.min.js",
                     "~/Scripts/fullcalendar.min.js"

                     )); // Ruta al archivo JS de FullCalendar

            bundles.Add(new StyleBundle("~/Content/fullcalendarCSS").Include(
                       "~/Content/fullcalendar.min.css")); // Ruta al archivo CSS de FullCalendar




        }
    }
}


/*  public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new StyleBundle("~/Content/css").Include(
                     
                       "~/Content/bootstrap.min.css",
                      "~/Content/Plugins/sweetalert2/css/sweetalert.css",
                       "~/Content/Plugins/Bootstrap-Duallistbox/css/bootstrap-duallistbox.min.css",
                     ));



            bundles.Add(new StyleBundle("~/Content/PluginsCSS").Include(

                      //JQUERY UI
                      "~/Content/Plugins/jquery-ui-1.12.1/jquery-ui.min.css",
                      "~/Content/Plugins/jquery-ui-1.12.1/jquery-ui-timepicker-addon.css"

                     ));

            bundles.Add(new StyleBundle("~/Content/PluginsJS").Include(

             //      //JQUERY UI
                  "~/Content/Plugins/jquery-ui-1.12.1/jquery-ui.min.js",
                  "~/Content/Plugins/jquery-ui-1.12.1/jquery-ui-timepicker-addon.js",
                  "~/Content/Plugins/jquery-ui-1.12.1/jquery-ui.es.js",

                   //Bootstrap Duallistbox
                  "~/Content/Plugins/Bootstrap-Duallistbox/js/jquery.bootstrap-duallistbox.min.js",

                 //LOADING OVERLAY
                  "~/Content/Plugins/jquery-loading-overlay/loadingoverlay.min.js",

                  "~/Scripts/jquery.validate.min.js"
                   ));

            // Agregar el bundle para FullCalendar
              bundles.Add(new ScriptBundle("~/bundles/fullcalendar").Include(
                       "~/Scripts/moment.min.js",
                       "~/Scripts/fullcalendar.min.js"
                     
                       )); // Ruta al archivo JS de FullCalendar

            bundles.Add(new StyleBundle("~/Content/fullcalendarCSS").Include(
                       "~/Content/fullcalendar.min.css")); // Ruta al archivo CSS de FullCalendar

        }*/