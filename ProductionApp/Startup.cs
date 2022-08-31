using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ProductionApp.Startup))]
namespace ProductionApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}
