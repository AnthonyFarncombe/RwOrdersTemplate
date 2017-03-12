using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RwOrders.Startup))]
namespace RwOrders
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
