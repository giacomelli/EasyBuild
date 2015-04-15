#if !TRAVIS_CI
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Web;
using System.Xml;
using HelperSharp;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace EasyBuild
{
    /// <summary>
    /// Task to serialize resource files with localized labels to JS files.
    /// </summary>
    public class Resources2JSTask : Task
    {
        #region Fields
        private Assembly m_resourcesAssembly;
        private IList<Type> m_resourceTypes;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Resources2JSTask"/> class.
        /// </summary>
        public Resources2JSTask()
        {
            DefaultCultureCode = "en";
            IgnoreResourceNames = new string[0];
            TextNotFoundMarkup = "[TEXT NOT FOUND] ";
            HtmlEncodeEnabled = true;
            SerializationFilename = "Globalizations";
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the filename of the assembly with the resources files.
        /// </summary>
        [Required]
        public string AssemblyFileName { get; set; }

        /// <summary>
        /// Gets or sets the serialization folder where the generated JS files will be writen.
        /// </summary>
        [Required]
        public string SerializationFolder { get; set; }

        /// <summary>
        /// Gets or sets the serialization filename where the generated JS files will be writen.
        /// </summary>
        public string SerializationFilename { get; set; }

        /// <summary>
        /// Gets or sets the list of culture codes to be considered.
        /// </summary>
        [Required]
        public string[] CultureCodes { get; set; }

        /// <summary>
        /// Gets or sets the default culture code.
        /// </summary>        
        public string DefaultCultureCode { get; set; }

        /// <summary>
        /// Gets or sets the list of resource files that should be ignored.
        /// </summary>
        public string[] IgnoreResourceNames { get; set; }

        /// <summary>
        /// Gets or sets the text not found markup.
        /// </summary>
        /// <value>
        /// The text not found markup.
        /// </value>
        public string TextNotFoundMarkup { get; set; }

        /// <summary>
        /// Gets or sets a value indicating when [Html Encode Enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if html encode is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool HtmlEncodeEnabled { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// When overridden in a derived class, executes the task.
        /// </summary>
        /// <returns>
        /// true if the task successfully executed; otherwise, false.
        /// </returns>
        public override bool Execute()
        {
            // Loads the resource assembly.
            m_resourcesAssembly = Assembly.LoadFrom(AssemblyFileName);

            if (m_resourcesAssembly == null)
            {
                throw new InvalidOperationException("Assembly '{0}' not found.".With(AssemblyFileName));
            }

            Log.LogMessage("Resources assembly: {0} ", m_resourcesAssembly.FullName);

            // Find the resource files.
            var types = m_resourcesAssembly.GetTypes()
                            .Where(t => t.GetProperties(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Any(p => p.PropertyType == typeof(ResourceManager)));

            types = types.Where(t => !IgnoreResourceNames.Any(i => t.Name.ToUpperInvariant().Equals(i)));
            m_resourceTypes = types.ToList();
            Log.LogMessage("Resource files found: {0}", String.Join(", ", m_resourceTypes.Select(t => t.Name)));

            ForEachCulture(CreateJSFiles);

            return true;
        }

        /// <summary>
        /// Cria os arquivos JS utilizando a cultura e os resources especificados.
        /// </summary>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void CreateJSFiles(CultureInfo culture)
        {
            Log.LogMessage("Creating JS files for culture '{0}'...", culture.Name);

            var serializationFullFilename = "{0}.{1}.js".With(Path.Combine(SerializationFolder, SerializationFilename), culture.Name);

            using (var fileStream = new StreamWriter(File.Open(serializationFullFilename, FileMode.Create, FileAccess.Write), Encoding.UTF8))
            {
                ForEachResource((rt, rm) =>
                {
                    var normalizedResourceName = NormalizePropertyName(rt.Name);
                    fileStream.WriteLine("globalization.{0} = {{", normalizedResourceName);

                    var properties = GetSerializableProperties(rt).ToList();

                    for (int i = 0; i < properties.Count; i++)
                    {
                        var p = properties[i];
                        Log.LogMessage("Serializing property '{0}'...", p.Name);

                        var globalizedText = rm.GetString(p.Name, culture);

                        if (((object)globalizedText) == null)
                        {
                            globalizedText = "{0} {1}".With(TextNotFoundMarkup, p.Name);
                        }

                        fileStream.Write("  '{0}': '{1}'", NormalizePropertyName(p.Name), NormalizePropertyValue(globalizedText, HtmlEncodeEnabled));

                        if (i + 1 < properties.Count)
                        {
                            fileStream.WriteLine(",");
                        }
                    }

                    fileStream.WriteLine("}};", normalizedResourceName);
                });
            }
        }

        /// <summary>
        /// Normaliza o nome de uma propriedade.
        /// </summary>
        /// <returns>O nome da propriedade normalizado.</returns>
        private static string NormalizePropertyName(string memberName)
        {
            return Char.ToLowerInvariant(memberName[0]) + memberName.Substring(1);
        }

        private static string NormalizePropertyValue(string globalizedText, bool htmlEncodeEnabled)
        {
            var result = globalizedText
                                            .Replace(Environment.NewLine, "\\n")
                                            .Replace("\n", "\\n");

            if (htmlEncodeEnabled)
            {
                result = HttpUtility.HtmlEncode(result);
            }

            return result;
        }

        /// <summary>
        /// Executa a action para cada cultura informada pelo usuário da linha de comando.
        /// </summary>
        /// <param name="action">A ação a ser executada.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        protected void ForEachCulture(Action<CultureInfo> action)
        {
            ExceptionHelper.ThrowIfNull("action", action);

            var cultures = CultureCodes.Select(c => new CultureInfo(c));

            foreach (var culture in cultures)
            {
                action(culture);
            }
        }

        /// <summary>
        /// Executa a action para cada resource informado pelo usuário da linha de comando.
        /// </summary>
        /// <param name="action">A ação a ser executada.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        private void ForEachResource(Action<Type, ResourceManager> action)
        {
            ExceptionHelper.ThrowIfNull("action", action);

            foreach (var r in m_resourceTypes)
            {
                action(r, new ResourceManager(r));
            }
        }

        /// <summary>
        /// Obtém as propriedades do resource que serão serializadas.
        /// </summary>
        /// <returns>As propriedades que podem ser serializadas.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        private static IEnumerable<PropertyInfo> GetSerializableProperties(Type resourceType)
        {
            ExceptionHelper.ThrowIfNull("resourceType", resourceType);

            var properties = resourceType.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(p => p.PropertyType == typeof(string) && !p.Name.Equals("ResourceManager") && !p.Name.Equals("Culture"));

            return properties;
        }

        /// <summary>
        /// Carrega o arquivo xml do resource informado.
        /// </summary>
        /// <param name="resourceFilesFolder">A pasta dos arquivos .resx.</param>
        /// <param name="resourceType">O tipo do resource.</param>
        /// <param name="culture">A cultura.</param>
        /// <returns>O documento xml.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters"), SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode")]
        private XmlDocument LoadResourceXmlFile(string resourceFilesFolder, Type resourceType, CultureInfo culture)
        {
            ExceptionHelper.ThrowIfNull("resourceFilesFolder", resourceFilesFolder);
            ExceptionHelper.ThrowIfNull("resourceType", resourceType);
            ExceptionHelper.ThrowIfNull("culture", culture);

            var xmlDoc = new XmlDocument();
            var searchPattern = culture.Name.Equals(DefaultCultureCode, StringComparison.OrdinalIgnoreCase) ? "{0}.resx" : "{0}.{1}.resx";
            var files = Directory.GetFiles(resourceFilesFolder, searchPattern.With(resourceType.Name, culture.Name));

            if (files.Length > 0)
            {
                xmlDoc.Load(files[0]);
            }

            return xmlDoc;
        }
        #endregion
    }
}
#endif