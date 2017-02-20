using System.Collections.Generic;

namespace Umbraco.Web.UI.JavaScript
{
    /// <summary>
    /// A custom renderer that only outputs a dependency path instead of script tags - for use with the js loader with yepnope
    /// </summary>
    public class DependencyPathRenderer : BackOfficeClientDependencyRenderer
    {
        public override string Name
        {
            get { return "Umbraco.DependencyPathRenderer"; }
        }

        /// <summary>
        /// Used to delimit each dependency so we can split later
        /// </summary>
        public const string Delimiter = "||||";

        protected override string RenderSingleJsFile(string js, IDictionary<string, string> htmlAttributes)
        {
            return js + Delimiter;
        }

        protected override string RenderSingleCssFile(string css, IDictionary<string, string> htmlAttributes)
        {
            return css + Delimiter;
        }

    }
}