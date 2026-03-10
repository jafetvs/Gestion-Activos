using Control_Gestion_V3.Filters;
using System.Web;
using System.Web.Mvc;

namespace Control_Gestion_V3
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new VerificarSession());
        }


    }
}
