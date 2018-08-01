using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ExtPartMappingTool.Startup))]
namespace ExtPartMappingTool
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
