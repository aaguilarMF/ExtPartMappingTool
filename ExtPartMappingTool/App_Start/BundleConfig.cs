using System.Web;
using System.Web.Optimization;

namespace ExtPartMappingTool
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/JQuery")
                .IncludeDirectory("~/Scripts/JQuery", "*.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                        "~/Content/bootstrap-theme.min.css",
                      "~/Content/bootstrap.min.css",
                      "~/Content/ui-bootstrap-csp.css",
                      "~/Content/ui-grid.min.css",
                      "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/bundles/Angular")
                .Include("~/Scripts/angular.min.js")
                .Include("~/Scripts/angular-route.min.js")
                .Include("~/Scripts/angular-ui-router.min.js") //replaced angular-route.js
                .Include("~/Scripts/angular-animate.min.js")
                .Include("~/Scripts/angular-file-upload.min.js")
                .Include("~/Scripts/ui-grid.min.js")
                );

            bundles.Add(new ScriptBundle("~/bundles/ui-bootstrap")
                .IncludeDirectory("~/Scripts/angular-ui", "*.js")
                );

            bundles.Add(new ScriptBundle("~/bundles/ExtPartMappingTool")
                .IncludeDirectory("~/Scripts/Controllers", "*.js")
                .IncludeDirectory("~/Scripts/Services", "*.js")
                .IncludeDirectory("~/Scripts/Directives", "*.js")
                .IncludeDirectory("~/Scripts/Factories", "*.js")
                /*.IncludeDirectory("~/Scripts/Services", "*.js")
                .IncludeDirectory("~/Scripts/Directives", "*.js")
                .IncludeDirectory("~/Scripts/Factories", "*.js")*/
                .Include("~/Scripts/_ExtPartMappingTool.js")
                );

            #region Process Aces and Pies Files
            bundles.Add(new ScriptBundle("~/processFilesContent/css").Include(
                "~/Content/bootstrap-theme.min.css",
                      "~/Content/bootstrap.min.css",
                      "~/Content/Animate.css",
                      "~/Content/site.css"));

            bundles.Add(new ScriptBundle("~/processFilesContent/knockoutjs")
                .Include("~/Scripts/knockout-{version}.js")
                );
            bundles.Add(new ScriptBundle("~/processFilesContent/ViewModels")
                .Include("~/Scripts/knockout-{version}.js",
                        "~/Scripts/ViewModels/ProcessAcesPiesViewModel.js")
                );
            bundles.Add(new ScriptBundle("~/processFilesContent/Services")
                .Include(
                        "~/Scripts/pipeline-status.js",
                        "~/Scripts/Services/StagingService.js",
                        "~/Scripts/Services/ProductionService.js")
                );
            #endregion
        }
    }
}
