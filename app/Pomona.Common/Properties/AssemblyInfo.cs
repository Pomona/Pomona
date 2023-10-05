#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly : AssemblyTitle("Pomona.Common")]
[assembly : AssemblyDescription("Pomona shared library between server and client.")]

// The following GUID is for the ID of the typelib if this project is exposed to COM

[assembly : Guid("74ed614e-29e9-42c5-87b3-8f6268a1ec74")]
[assembly : InternalsVisibleTo("Pomona.SystemTests")]
[assembly : InternalsVisibleTo("Pomona.UnitTests")]

