namespace Nancy.Localization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Resources;

    /// <summary>
    /// Resource based implementation of <see cref="ITextResource"/>
    /// </summary>
    public class ResourceBasedTextResource  : ITextResource
    {
        private readonly IResourceAssemblyProvider resourceAssemblyProvider;
        private readonly IDictionary<string, ResourceManager> resourceManagers;
        
        /// <summary>
        /// Initializes a new instance of <see cref="ResourceBasedTextResource"/> to read strings from *.resx files
        /// </summary>
        /// <param name="resourceAssemblyProvider">The <see cref="IResourceAssemblyProvider"/> that should be used when scanning.</param>
        public ResourceBasedTextResource(IResourceAssemblyProvider resourceAssemblyProvider)
        {
            this.resourceAssemblyProvider = resourceAssemblyProvider;

            var resources = 
                from assembly in this.resourceAssemblyProvider.GetAssembliesToScan()
                from resourceName in assembly.GetManifestResourceNames()
                where resourceName.EndsWith(".resources")
                let parts = resourceName.Split(new[] { '.' })
                let name = parts[parts.Length - 2]
                let baseName = resourceName.Replace(".resources", string.Empty)
                select new
                    {
                        Name = name,
                        Manager = new ResourceManager(baseName, assembly)
                    };

            this.resourceManagers = 
                resources.ToDictionary(x => x.Name, x => x.Manager, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Used to return a string value from *.resx files
        /// </summary>
        /// <param name="key">The key to look for in the resource file</param>
        /// <param name="context">The <see cref="NancyContext"/> used to determine the culture for returning culture specific values.</param>
        /// <returns>Returns a string value from culture specific or default file or null if key does not exist as determined by <see cref="ResourceManager"/>.</returns>
        public string this[string key, NancyContext context]
        {
            get
            {
                var components =
                    GetKeyComponents(key);

                var manager = this.resourceManagers.ContainsKey(components.Item1) ?
                    this.resourceManagers[components.Item1] :
                    null;

                return (manager == null) ? null : manager.GetString(components.Item2, context.Culture);
            }
        }

        private static Tuple<string, string> GetKeyComponents(string key)
        {
            var index =
                key.IndexOf(".", StringComparison.InvariantCulture);

            if (index == -1)
            {
                throw new InvalidOperationException("The text key needs to be specified in the format resourcename.resourcekey.");
            }

            return new Tuple<string, string>(
                key.Substring(0, index),
                key.Substring(index + 1));
        }
    }
}