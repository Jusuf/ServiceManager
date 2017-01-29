using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WU15.Azure.ServiceManager.Web.Startup))]
namespace WU15.Azure.ServiceManager.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
