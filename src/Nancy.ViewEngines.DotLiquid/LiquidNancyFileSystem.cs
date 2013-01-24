﻿namespace Nancy.ViewEngines.DotLiquid
{
    using System;
    using System.Text.RegularExpressions;
    using global::DotLiquid;
    using global::DotLiquid.FileSystems;
    using liquid = global::DotLiquid;    
    using System.Linq;

    /// <summary>
    /// <see cref="IFileSystem"/> implementation around the Nancy templates.
    /// </summary>
    public class LiquidNancyFileSystem : IFileSystem
    {
        private readonly ViewEngineStartupContext viewEngineStartupContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiquidNancyFileSystem"/> class,
        /// with the provided <paramref name="context"/>.
        /// </summary>
        /// <param name="context">The context that the engine can operate in.</param>
        public LiquidNancyFileSystem(ViewEngineStartupContext context)
        {
            viewEngineStartupContext = context;
        }

        /// <summary>
        /// Reads the content of the template specified by the <paramref name="templateName"/> parameter.
        /// </summary>
        /// <param name="context">The <see cref="Context"/> of the call.</param>
        /// <param name="templateName">The name of the template to read.</param>
        /// <exception cref="liquid.Exceptions.FileSystemException">The specified template could not be located.</exception>
        /// <returns>The content of the template.</returns>
        public string ReadTemplateFile(Context context, string templateName)
        {
            IRenderContext renderContext = context.Registers["nancy"] as IRenderContext;
            if (renderContext != null)
            {
                // Clean up the template name
                templateName = GetCleanTemplateName(templateName);

                // Try to find a matching template using established view conventions
                ViewLocationResult viewLocation = null;
                if (viewEngineStartupContext.Extensions.Any(
                    s => templateName.EndsWith(s, StringComparison.OrdinalIgnoreCase)))
                {
                    // The template name does end with a valid extension, just try to find it
                    viewLocation = renderContext.LocateView(templateName, null);
                }
                else
                {
                    // The template name does not end with a valid extension, try all the possibilities
                    foreach (string extension in viewEngineStartupContext.Extensions)
                    {
                        viewLocation = renderContext.LocateView(String.Concat(templateName, ".", extension), null);
                        if (viewLocation != null) break;
                    }
                }

                // If we found one, get the template and pass it back
                // Eventually, it would be better to pass back the actual template from the cache if it's already been parsed
                // Or to parse here and store it in the cache before passing it back in not
                if (viewLocation != null)
                {
                    return viewLocation.Contents.Invoke().ReadToEnd();
                }
            }
            throw new liquid.Exceptions.FileSystemException("Template file {0} not found", new[] { templateName });
        }

        private string GetCleanTemplateName(string templateName)
        {
            return templateName
                .Replace(@"""", "")
                .Replace("'", "")
                .Replace(@"\", "/");
        }
    }
}