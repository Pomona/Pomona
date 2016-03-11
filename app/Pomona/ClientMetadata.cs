#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;

namespace Pomona
{
    /// <summary>
    /// Metadata about the generated REST client.
    /// </summary>
    public class ClientMetadata
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMetadata" /> class.
        /// </summary>
        /// <param name="assemblyName">The name of the generated assembly. Default set to "Client".</param>
        /// <param name="name">The name of the REST client <c>class</c>.</param>
        /// <param name="interfaceName">The name of the REST client <c>interface</c>. This should usually be
        /// identical to <paramref name="name" />, with an 'I' prefix. Default set to "IClient".</param>
        /// <param name="namespace">The that <see cref="Name" /> and all other generated classes will reside
        /// within. Usually identical to <see cref="AssemblyName" />, but not necessarily.
        /// Default set to "Client".</param>
        /// <param name="informationalVersion">The informational version of the generated assembly. Defaults to 1.0.0.0.</param>
        protected internal ClientMetadata(string assemblyName = "Client",
                                          string name = "Client",
                                          string interfaceName = "IClient",
                                          string @namespace = "Client",
                                          string informationalVersion = "1.0.0.0")
        {
            if (String.IsNullOrWhiteSpace(assemblyName))
                assemblyName = "Client";

            if (String.IsNullOrWhiteSpace(name))
                name = assemblyName.Split('.').Last();

            if (String.IsNullOrWhiteSpace(interfaceName))
                interfaceName = String.Concat('I', name);

            if (String.IsNullOrWhiteSpace(@namespace))
                @namespace = assemblyName;

            if (String.IsNullOrWhiteSpace(informationalVersion))
                informationalVersion = "1.0.0.0";

            if (!IsRunningOnMono())
            {
                // NOTE: Validation of identifiers not working on Mono, due to System.CodeDom not being fully compatible.
                ValidateIdentifiers(new Dictionary<string, CodeObject>
                {
                    { "name", new CodeTypeDeclaration(name) },
                    { "interfaceName", new CodeTypeDeclaration(interfaceName) },
                    { "namespace", new CodeNamespace(@namespace) },
                });
            }

            AssemblyName = assemblyName;
            Name = name;
            InterfaceName = interfaceName;
            Namespace = @namespace;
            InformationalVersion = informationalVersion;
        }


        /// <summary>
        /// Gets or sets the name of the generated assembly. Default set to "Client".
        /// </summary>
        /// <value>
        /// The name of the generated assembly. Default set to "Client".
        /// </value>
        public string AssemblyName { get; }

        /// <summary>
        /// Gets or sets the informational version of the generated assembly.
        /// </summary>
        /// <value>
        /// The informational version of the generated assembly.
        /// </value>
        /// <exception cref="System.ArgumentNullException">value</exception>
        public string InformationalVersion { get; }

        /// <summary>
        /// Gets or sets the name of the REST client <c>interface</c>.
        /// This should usually be identical to <see cref="Name"/>, with an 'I' prefix.
        /// Default set to "IClient".
        /// </summary>
        /// <value>
        /// The name of the REST client <c>class</c>.
        /// This should usually be identical to <see cref="Name"/>, with an 'I' prefix.
        /// Default set to "IClient".
        /// </value>
        public string InterfaceName { get; }

        /// <summary>
        /// Gets or sets the name of the REST client <c>class</c>.
        /// Default set to "Client".
        /// </summary>
        /// <value>
        /// The name of the REST client <c>class</c>. Default set to "Client".
        /// </value>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the namespace that <see cref="Name"/> and all other generated
        /// classes will reside within. Usually identical to <see cref="AssemblyName"/>,
        /// but not necessarily.
        /// Default set to "Client".
        /// </summary>
        /// <value>
        /// The namespace that <see cref="Name"/> and all other generated
        /// classes will reside within. Usually identical to <see cref="AssemblyName"/>,
        /// but not necessarily.
        /// Default set to "Client".
        /// </value>
        public string Namespace { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ClientMetadata" /> class, based on the current
        /// instance. Parameters with a given value will override the current value; the others will have
        /// their value from the current instance.
        /// </summary>
        /// <param name="assemblyName">The name of the generated assembly. Default set to "Client".</param>
        /// <param name="name">The name of the REST client <c>class</c>.</param>
        /// <param name="interfaceName">The name of the REST client <c>interface</c>. This should usually be
        /// identical to <paramref name="name" />, with an 'I' prefix. Default set to "IClient".</param>
        /// <param name="namespace">The that <see cref="Name" /> and all other generated classes will reside
        /// within. Usually identical to <see cref="AssemblyName" />, but not necessarily.
        /// Default set to "Client".</param>
        /// <param name="informationalVersion">The informational version of the generated assembly. Defaults to 1.0.0.0.</param>
        /// <returns>
        /// A new instance of the <see cref="ClientMetadata" /> class, based on the current
        /// instance. Parameters with a given value will override the current value; the others will have
        /// their value from the current instance.
        /// </returns>
        public ClientMetadata With(string assemblyName = null,
                                   string name = null,
                                   string interfaceName = null,
                                   string @namespace = null,
                                   string informationalVersion = null)
        {
            if (String.IsNullOrWhiteSpace(assemblyName))
                assemblyName = AssemblyName;

            if (String.IsNullOrWhiteSpace(name))
                name = Name;

            if (String.IsNullOrWhiteSpace(interfaceName))
                interfaceName = String.Concat('I', name);

            if (String.IsNullOrWhiteSpace(@namespace))
                @namespace = Namespace;

            if (String.IsNullOrWhiteSpace(informationalVersion))
                informationalVersion = InformationalVersion;

            return new ClientMetadata(assemblyName, name, interfaceName, @namespace, informationalVersion);
        }


        /// <summary>
        /// Check whether we are running on Mono runtime.
        /// </summary>
        /// <returns>True if running on Mono runtime.</returns>
        private static bool IsRunningOnMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }


        private static void ValidateIdentifiers(IEnumerable<KeyValuePair<string, CodeObject>> codeObjects)
        {
            foreach (var kvp in codeObjects)
            {
                var codeObject = kvp.Value;

                try
                {
                    CodeGenerator.ValidateIdentifiers(codeObject);
                }
                catch (Exception exception)
                {
                    var typeMember = codeObject as CodeTypeMember;
                    if (typeMember != null)
                        throw new ArgumentException(String.Format("'{0}' is not a valid type name.", typeMember.Name), kvp.Key);

                    var ns = codeObject as CodeNamespace;
                    if (ns != null)
                        throw new ArgumentException(String.Format("'{0}' is not a valid namespace.", ns.Name), kvp.Key);

                    throw new ArgumentException(exception.Message, kvp.Key);
                }
            }
        }
    }
}