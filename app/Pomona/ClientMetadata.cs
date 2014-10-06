#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2014 Karsten Nikolai Strand
// 
// Permission is hereby granted, free of charge, to any person obtaining a 
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
// ----------------------------------------------------------------------------

#endregion

using System;
using System.Linq;

namespace Pomona
{
    /// <summary>
    /// Metadata about the generated REST client.
    /// </summary>
    public class ClientMetadata
    {
        private readonly string assemblyName;
        private readonly string informationalVersion;
        private readonly string interfaceName;
        private readonly string name;
        private readonly string @namespace;


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

            this.assemblyName = assemblyName;
            this.name = name;
            this.interfaceName = interfaceName;
            this.@namespace = @namespace;
            this.informationalVersion = informationalVersion;
        }


        /// <summary>
        /// Gets or sets the name of the generated assembly. Default set to "Client".
        /// </summary>
        /// <value>
        /// The name of the generated assembly. Default set to "Client".
        /// </value>
        public string AssemblyName
        {
            get { return this.assemblyName; }
        }

        /// <summary>
        /// Gets or sets the informational version of the generated assembly.
        /// </summary>
        /// <value>
        /// The informational version of the generated assembly.
        /// </value>
        /// <exception cref="System.ArgumentNullException">value</exception>
        public string InformationalVersion
        {
            get { return this.informationalVersion; }
        }

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
        public string InterfaceName
        {
            get { return this.interfaceName; }
        }

        /// <summary>
        /// Gets or sets the name of the REST client <c>class</c>.
        /// Default set to "Client".
        /// </summary>
        /// <value>
        /// The name of the REST client <c>class</c>. Default set to "Client".
        /// </value>
        public string Name
        {
            get { return this.name; }
        }

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
        public string Namespace
        {
            get { return this.@namespace; }
        }


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
                assemblyName = this.assemblyName;

            if (String.IsNullOrWhiteSpace(name))
                name = this.name;

            if (String.IsNullOrWhiteSpace(interfaceName))
                interfaceName = this.interfaceName;

            if (String.IsNullOrWhiteSpace(@namespace))
                @namespace = this.@namespace;

            if (String.IsNullOrWhiteSpace(informationalVersion))
                informationalVersion = this.informationalVersion;

            return new ClientMetadata(assemblyName, name, interfaceName, @namespace, informationalVersion);
        }
    }
}