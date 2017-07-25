 using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(JSW.Gomoku.Startup))]
namespace JSW.Gomoku
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
