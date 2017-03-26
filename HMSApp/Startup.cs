using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HMSApp.Startup))]
namespace HMSApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
 
