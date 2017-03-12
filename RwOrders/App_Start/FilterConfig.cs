using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using RwOrders.Models;
using System;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;

namespace RwOrders
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AuthorizeAttribute());
            filters.Add(new RequireHttpsAttribute());
            filters.Add(new PageViewAttribute());
        }
    }

    public class PageViewAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAjaxRequest())
            {
                string userId = filterContext.HttpContext.User.Identity.GetId();
                string key = "UserVisit->" + userId;
                if (userId != null)
                {
                    UserVisit visit = HttpRuntime.Cache[key] as UserVisit;
                    if (visit == null)
                    {
                        visit = new UserVisit
                        {
                            UserID = userId,
                            VisitDate = DateTime.Now,
                            Url = filterContext.HttpContext.Request.Url.ToString(),
                            Browser = filterContext.HttpContext.Request.Browser.Browser,
                            IpAddress = filterContext.HttpContext.Request.UserHostAddress
                        };
                        HttpRuntime.Cache.Insert(key, visit, null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration, CacheItemPriority.Default, onRemove);
                    }
                    else
                    {
                        visit.VisitDate = DateTime.Now;
                        visit.Url = filterContext.HttpContext.Request.Url.ToString();
                        visit.Browser = filterContext.HttpContext.Request.Browser.Browser;
                        visit.IpAddress = filterContext.HttpContext.Request.UserHostAddress;
                        visit.Count++;
                    }
                }
            }

            base.OnActionExecuting(filterContext);
        }

        private static void onRemove(string key, object value, CacheItemRemovedReason reason)
        {
            if (!key.StartsWith("UserVisit->"))
                return;

            UserVisit visit = value as UserVisit;
            if (visit == null)
                return;

            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                UserStore<ApplicationUser> userStore = new UserStore<ApplicationUser>(db);
                ApplicationUserManager userManager = new ApplicationUserManager(userStore);

                ApplicationUser user = userManager.FindByIdAsync(visit.UserID).Result;
                if (user != null)
                {
                    user.LastVisit = visit.VisitDate;
                    user.LastPage = visit.Url;
                    user.LastBrowser = visit.Browser;
                    user.LastIP = visit.IpAddress;
                    user.NumberOfVisits += visit.Count;

                    IdentityResult result = userManager.UpdateAsync(user).Result;
                }
            }
        }
    }
}
