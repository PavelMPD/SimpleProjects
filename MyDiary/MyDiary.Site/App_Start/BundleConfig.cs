using System.Web;
using System.Web.Optimization;

namespace MyDiary.Site
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            /*Исходный
             bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));
             */

            //DC

            // Осталвена старая верия из коробки. потому что с новой не работает группировка
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                 "~/Scripts/jquery.min.js"
                 ));

            bundles.Add(new ScriptBundle("~/bundles/jquery.unobtrusive").Include(
                "~/Scripts/jquery-ui-1.9.2.custom.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryUI").Include(
                "~/Scripts/jquery.unobtrusive-ajax.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/dc.jquery").Include(
                "~/Scripts/dc.jquery.ext.js",
                "~/Scripts/dc.consts.js",
                "~/Scripts/jquery.cookie.js",
                "~/Scripts/json2.js",
                "~/Scripts/jquery.form.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/dc").Include(
                "~/Scripts/dc.keys.js",
                "~/Scripts/dc.display.js",
                "~/Scripts/dc.card.print.js",
                "~/Scripts/dc.tabs.ext.js",
                "~/Scripts/dc.grid.ext.js",
                "~/Scripts/dc.common-selector.js",
                "~/Scripts/dc.batch-operations-control.js",
                "~/Scripts/dc.user-page-position.js",
                 "~/Scripts/dc.progressbar.js"
            ));


            //Kendo UI
            bundles.Add(new ScriptBundle("~/bundles/kendo").Include(
                    "~/Scripts/kendo.web.min.*",
                     "~/Scripts/kendo.all.min.*",
                    "~/Scripts/kendo.aspnetmvc.min.*",
                    "~/Scripts/kendo.draganddrop.min.js",
                    "~/Scripts/kendo.culture.ru-RU.min.js",
                    "~/Scripts/chosen.jquery.min.js"
            ));

            bundles.Add(new ScriptBundle("~/bundles/jstorage.js").Include(
                "~/Scripts/jstorage.js",
                "~/Scripts/dc.storage.js"
             ));

            bundles.Add(new StyleBundle("~/Content/kendo").Include(
                    "~/Content/kendo.common.*",
                    "~/Content/kendo.default.*"));

            bundles.Add(new StyleBundle("~/Content/jqueryUI").Include(
                "~/Content/jquery-ui-1.9.2.custom.css",
                "~/Content/chosen.css"));

            bundles.Add(new StyleBundle("~/Content/dc-grid").Include(
                "~/Content/dc-grid.css"));

            bundles.IgnoreList.Clear();
            bundles.IgnoreList.Ignore("*.intellisense.js");
            bundles.IgnoreList.Ignore("*-vsdoc.js");
            bundles.IgnoreList.Ignore("*.debug.js", OptimizationMode.WhenEnabled);
        }
    }
}