using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Reciper
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Xóa một lần dữ liệu mẫu cũ theo yêu cầu người dùng để người dùng tự nhập mới
            try
            {
                string flagFile = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\cleared_seeded.flag";
                if (!System.IO.File.Exists(flagFile))
                {
                    using (var db = new Models.ReciperEntities())
                    {
                        db.Database.ExecuteSqlCommand("DELETE FROM recipe_ingredients; DELETE FROM steps;");
                    }
                    string dir = AppDomain.CurrentDomain.BaseDirectory + "App_Data";
                    if (!System.IO.Directory.Exists(dir))
                    {
                        System.IO.Directory.CreateDirectory(dir);
                    }
                    System.IO.File.WriteAllText(flagFile, "done " + DateTime.Now);
                }
            }
            catch { }
        }
    }
}

