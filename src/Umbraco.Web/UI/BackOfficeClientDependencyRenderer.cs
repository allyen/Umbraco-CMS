using ClientDependency.Core;
using ClientDependency.Core.Config;
using ClientDependency.Core.FileRegistration.Providers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Umbraco.Core.Configuration;

namespace Umbraco.Web.UI
{
    public class BackOfficeClientDependencyRenderer : BaseRenderer
    {
        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            // Assign the provider a default name if it doesn't have one
            if (string.IsNullOrEmpty(name))
                name = nameof(BackOfficeClientDependencyRenderer);

            base.Initialize(name, config);
        }

        /// <summary>
        /// Override because we need to ensure the &amp; is replaced with &amp;amp; This is only required for this one w3c compliancy, the URL itself is a valid URL.
        /// </summary>
        /// <param name="allDependencies"></param>
        /// <param name="paths"></param>
        /// <param name="jsOutput"></param>
        /// <param name="cssOutput"></param>
        /// <param name="http"></param>
        public override void RegisterDependencies(List<IClientDependencyFile> allDependencies, HashSet<IClientDependencyPath> paths, out string jsOutput, out string cssOutput, HttpContextBase http)
        {
            base.RegisterDependencies(allDependencies, paths, out jsOutput, out cssOutput, http);

            jsOutput = jsOutput.Replace("&", "&amp;");
            cssOutput = cssOutput.Replace("&", "&amp;");
        }

        protected override string RenderJsDependencies(IEnumerable<IClientDependencyFile> jsDependencies, HttpContextBase http, IDictionary<string, string> htmlAttributes)
        {
            if (!jsDependencies.Any())
                return string.Empty;

            var sb = new StringBuilder();

            if (http.IsDebuggingEnabled || !EnableCompositeFiles)
            {
                foreach (var dependency in jsDependencies)
                {
                    sb.Append(RenderSingleJsFile(dependency.FilePath, htmlAttributes));
                }
            }
            else
            {
                var comp = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.ProcessCompositeList(jsDependencies, ClientDependencyType.Javascript, http, ClientDependencySettings.Instance.CompositeFileHandlerPath);
                foreach (var s in comp)
                {
                    sb.Append(RenderSingleJsFile(s, htmlAttributes));
                }
            }

            return sb.ToString();
        }

        protected override string RenderCssDependencies(IEnumerable<IClientDependencyFile> cssDependencies, HttpContextBase http, IDictionary<string, string> htmlAttributes)
        {
            if (!cssDependencies.Any())
                return string.Empty;

            var sb = new StringBuilder();

            if (http.IsDebuggingEnabled || !EnableCompositeFiles)
            {
                foreach (var dependency in cssDependencies)
                {
                    sb.Append(RenderSingleCssFile(dependency.FilePath, htmlAttributes));
                }
            }
            else
            {
                RenderCssComposites(http, htmlAttributes, sb, cssDependencies);
            }

            return sb.ToString();
        }

        private void RenderCssComposites(HttpContextBase http, IDictionary<string, string> htmlAttributes, StringBuilder sb, IEnumerable<IClientDependencyFile> cssDependencies)
        {
            var cdPath = GlobalSettings.ClientDependencyBackOfficePath ?? ClientDependencySettings.Instance.CompositeFileHandlerPath;
            var comp = ClientDependencySettings.Instance.DefaultCompositeFileProcessingProvider.ProcessCompositeList(
                cssDependencies,
                ClientDependencyType.Css,
                http,
                cdPath);

            foreach (var s in comp)
            {
                sb.Append(RenderSingleCssFile(s, htmlAttributes));
            }
        }

        protected override string RenderSingleJsFile(string js, IDictionary<string, string> htmlAttributes)
        {
            return string.Format(HtmlEmbedContants.ScriptEmbedWithSource, js, htmlAttributes.ToHtmlAttributes());
        }

        protected override string RenderSingleCssFile(string css, IDictionary<string, string> htmlAttributes)
        {
            return string.Format(HtmlEmbedContants.CssEmbedWithSource, css, htmlAttributes.ToHtmlAttributes());
        }
    }
}
