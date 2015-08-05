#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2015 Karsten Nikolai Strand
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using NUnit.Framework;

using Pomona.Common.Internals;

namespace Pomona.TestHelpers
{
    /// <summary>
    /// Abstract base class for Visual Studio solution sanity checks.
    /// </summary>
    public static class SolutionTestsHelper
    {
        public static IEnumerable<string> FindCSharpSourceFiles(string path)
        {
            return FindSourceFiles("*.cs", path);
        }


        /// <summary>
        /// Finds the physical path of the Visual Studio Project File the assembly is compiled from.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>The physical path of the  Visual Studio Project File the assembly is compiled from.</returns>
        public static string FindProjectPathOf(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            if (String.IsNullOrEmpty(assembly.CodeBase))
                throw new ArgumentException(String.Format("The assembly '{0}' has no code base.", assembly), "assembly");

            UriBuilder uri = new UriBuilder(assembly.CodeBase);
            string unescapeDataString = Uri.UnescapeDataString(uri.Path);
            string assemblyPath = Path.GetFileName(unescapeDataString);

            if (String.IsNullOrEmpty(assemblyPath))
            {
                throw new FileNotFoundException(
                    String.Format("Could not find a physical path for '{0}'.", assembly.CodeBase));
            }

            FileInfo assemblyFile = new FileInfo(assemblyPath);
            DirectoryInfo parentDirectory = assemblyFile.Directory;
            string fileNotFoundMessage = String.Format("Couldn't find a Project file for '{0}'.", assembly);
            string projectFileName = Path.ChangeExtension(assemblyFile.Name, "csproj");

            try
            {
                while ((parentDirectory = parentDirectory.Parent) != null)
                {
                    FileInfo[] solutionFiles = parentDirectory.GetFiles(projectFileName, SearchOption.AllDirectories);

                    if (solutionFiles.Length > 0)
                        return solutionFiles[0].FullName;
                }
            }
            catch (Exception exception)
            {
                throw new FileNotFoundException(fileNotFoundMessage, exception);
            }

            throw new FileNotFoundException(fileNotFoundMessage);
        }


        /// <summary>
        /// Finds the physical path to the Visual Studio Project File <typeparamref name="T"/> is defined in.
        /// </summary>
        /// <typeparam name="T">The type whose  Visual Studio Project File we should find the physical path to.</typeparam>
        /// <returns>The physical path to the  Visual Studio Project File <typeparamref name="T"/> is defined in.</returns>
        public static string FindProjectPathOf<T>()
        {
            Assembly assembly = typeof(T).Assembly;
            return FindProjectPathOf(assembly);
        }


        /// <summary>
        /// Finds the physical path to the Visual Studio Solution File of the <see cref="Assembly"/> <typeparamref name="T"/> is defined in.
        /// </summary>
        /// <typeparam name="T">The type whose <see cref="Assembly"/> we should find the physical path to the Visual Studio Solution File of.</typeparam>
        /// <returns>
        /// The physical path to the Visual Studio Solution File of the <see cref="Assembly"/> <typeparamref name="T"/> is defined in.
        /// </returns>
        public static string FindSolutionPathOf<T>()
        {
            Assembly assembly = typeof(T).Assembly;
            return FindSolutionPathOf(assembly);
        }


        /// <summary>
        /// Finds the physical path to the Visual Studio Solution File of the <see cref="Assembly"/> <paramref name="object"/> is defined in.
        /// </summary>
        /// <param name="object">The type whose <see cref="Assembly"/> we should find the physical path to the Visual Studio Solution File of.</param>
        /// <returns>
        /// The physical path to the Visual Studio Solution File of the <see cref="Assembly"/> <paramref name="object"/> is defined in.
        /// </returns>
        public static string FindSolutionPathOf(object @object)
        {
            if (@object == null)
                throw new ArgumentNullException("object");

            return FindSolutionPathOf(@object.GetType());
        }


        /// <summary>
        /// Finds the physical path to the Visual Studio Solution File of the <see cref="Assembly"/> <paramref name="type"/> is defined in.
        /// </summary>
        /// <param name="type">The type whose <see cref="Assembly"/> we should find the physical path to the Visual Studio Solution File of.</param>
        /// <returns>
        /// The physical path to the Visual Studio Solution File of the <see cref="Assembly"/> <paramref name="type"/> is defined in.
        /// </returns>
        public static string FindSolutionPathOf(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return FindSolutionPathOf(type.Assembly);
        }


        /// <summary>
        /// Finds the physical path to the Visual Studio Solution File of the <paramref name="assembly"/>.
        /// </summary>
        /// <param name="assembly">The assembly of which to find the physical Visual Studio Solution File path to.</param>
        /// <returns>
        /// The physical path to the Visual Studio Solution File of the <paramref name="assembly"/>.
        /// </returns>
        public static string FindSolutionPathOf(Assembly assembly)
        {
            string projectPath = FindProjectPathOf(assembly);
            FileInfo projectFile = new FileInfo(projectPath);
            DirectoryInfo parentDirectory = projectFile.Directory;

            string fileNotFoundMessage = String.Format("Couldn't find a Solution file for '{0}'.", assembly);

            try
            {
                while ((parentDirectory = parentDirectory.Parent) != null)
                {
                    FileInfo[] solutionFiles = parentDirectory.GetFiles("*.sln");

                    if (solutionFiles.Length > 0)
                        return solutionFiles[0].FullName;
                }
            }
            catch (Exception exception)
            {
                throw new FileNotFoundException(fileNotFoundMessage, exception);
            }

            throw new FileNotFoundException(fileNotFoundMessage);
        }


        public static IEnumerable<string> FindSourceFiles(string searchPattern, string path)
        {
            var sourceCodeFiles = Directory.EnumerateFiles(path,
                                                           searchPattern,
                                                           SearchOption.AllDirectories)
                                           .Where(x => !IsIgnoredPath(Path.GetDirectoryName(x)));
            return sourceCodeFiles;
        }


        /// <summary>
        /// Check all projects in solution at path for consistent nuget package references.
        /// </summary>
        /// <param name="solutionPath">Path to .sln file.</param>
        public static void VerifyNugetPackageReferences(string solutionPath, Func<NugetPackageElement, bool> filter)
        {
            filter = filter ?? (x => true);
            var solutionDir = Path.GetDirectoryName(solutionPath);
            //var solution = new ICSharpCode.NRefactory.ConsistencyCheck.Solution(solutionPath);
            var packages = NugetPackageElement.Load(solutionDir).Where(filter).GroupBy(x => x.Id).ToList();
            StringBuilder sb = new StringBuilder();
            int errorCount = 0;
            foreach (var package in packages)
            {
                var versions = package.GroupBy(x => x.Version).ToList();
                if (versions.Count > 1)
                {
                    sb.AppendFormat("Found multiple versions of package {0}:\r\n{1}",
                                    package.Key,
                                    String.Join("",
                                                versions.Select(
                                                    x =>
                                                        String.Format("    {0}\r\n{1}",
                                                                      x.Key,
                                                                      String.Join("",
                                                                                  x.Select(
                                                                                      y => String.Format("        {0}\r\n", y.ProjectName)))))));

                    errorCount++;

                    var suggestedVersion =
                        versions.Select(x => x.Key).OrderBy(x => x, new NugetPackageElement.VersionComparer()).Last();
                    var suggestedUpgrades =
                        versions.Where(x => x.Key != suggestedVersion).SelectMany(x => x);
                    sb.AppendFormat("    Suggested version is {0}, install using:\r\n{1}",
                                    suggestedVersion,
                                    String.Join("",
                                                suggestedUpgrades.Select(
                                                    x =>
                                                        String.Format("        Update-Package -Id {0} -ProjectName {1} -Version {2}\r\n",
                                                                      x.Id,
                                                                      x.ProjectName,
                                                                      suggestedVersion))));
                }
            }
            foreach (var item in packages.SelectMany(x => x))
                errorCount += item.ValidateHintPathReference(sb);
            Assert.That(errorCount, Is.EqualTo(0), "Found package reference inconsitencies:\r\n" + sb);
        }


        public static void VerifyProjectNoOrphanSourceCodeFiles(string projectPath)
        {
            XDocument project;
            using (var f = File.OpenRead(projectPath))
            {
                project = XDocument.Load(f);
            }
            var projectDir = Path.GetDirectoryName(projectPath);
            var xmlNamespaceManager = new XmlNamespaceManager(new NameTable());
            xmlNamespaceManager.AddNamespace("pj", project.Root.Name.NamespaceName);
            var filesInProject =
                ((IEnumerable)project.XPathEvaluate("/pj:Project/pj:ItemGroup/pj:Compile/@Include", xmlNamespaceManager))
                    .Cast<XAttribute>()
                    .Select(x => x.Value)
                    .Where(x => Path.GetExtension(x) == ".cs")
                    .Select(x => MakeRelative(Path.GetFullPath(Path.Combine(projectDir, x)), projectDir)).ToList();
            var filesInDirectory =
                FindCSharpSourceFiles(projectDir)
                    .Select(x => MakeRelative(x, projectDir)).ToList();

            var errorLog = new StringBuilder();
            var filesNotIncludedInProject = filesInDirectory.Except(filesInProject, StringComparer.OrdinalIgnoreCase);
            foreach (var orphanFile in filesNotIncludedInProject)
                errorLog.AppendFormat("File \"{0}\" in project folder of {1} is not included.", orphanFile, projectPath);

            if (errorLog.Length > 0)
                Assert.Fail(errorLog.ToString());
        }


        private static bool IsIgnoredPath(string directoryName)
        {
            return directoryName.Contains("\\obj\\") || directoryName.Contains("\\bin\\");
        }


        private static string MakeRelative(string filePath, string referencePath)
        {
            var fileUri = new Uri(filePath);
            var referenceUri = new Uri(referencePath);
            return referenceUri.MakeRelativeUri(fileUri).ToString().Replace("/", Path.DirectorySeparatorChar.ToString());
        }

        #region Nested type: NugetPackageElement

        public class NugetPackageElement
        {
            private static readonly string[] knownFrameworkVersions = { "net451", "net45", "net40", "net35", "net20" };
            private static readonly string[] preferredFrameworkVersions = { "net45", "net40", "net35", "net20" };
            private readonly string id;
            private readonly string projectFile;
            private readonly string solutionDirectoy;
            private readonly string version;
            private string[] availableFrameworkVersions;
            private string fullPackagePath;
            private XDocument projectXmlDocument;


            public NugetPackageElement(string project,
                                       XElement packagesConfigElement,
                                       string solutionDirectoy,
                                       XDocument projectXmlDocument = null)
            {
                this.projectFile = project;
                this.solutionDirectoy = solutionDirectoy;
                this.projectXmlDocument = projectXmlDocument;
                this.id = packagesConfigElement.Attribute("id").Value;
                this.version = packagesConfigElement.Attribute("version").Value;
            }


            public string AssumedPackagePath
            {
                get { return AssumedPackagePathStart + this.version; }
            }

            public string AssumedPackagePathStart
            {
                get { return Path.Combine(RelativePackagesPath, Id + "."); }
            }

            public string Id
            {
                get { return this.id; }
            }

            public string PreferredFrameworkVersion
            {
                get { return preferredFrameworkVersions.FirstOrDefault(x => AvailableFrameworkVersions.Contains(x)); }
            }

            public string ProjectDirectory
            {
                get { return Path.GetDirectoryName(this.projectFile); }
            }

            public string ProjectName
            {
                get { return Path.GetFileNameWithoutExtension(this.projectFile); }
            }

            public XDocument ProjectXmlDocument
            {
                get { return this.projectXmlDocument ?? (this.projectXmlDocument = XDocument.Load(this.projectFile)); }
            }

            public string RelativePackagesPath
            {
                get
                {
                    return GetRelativePath(Path.Combine(this.solutionDirectoy, "packages"),
                                           Path.GetDirectoryName(this.projectFile));
                }
            }

            public string Version
            {
                get { return this.version; }
            }

            private string[] AvailableFrameworkVersions
            {
                get
                {
                    return this.availableFrameworkVersions
                           ?? (this.availableFrameworkVersions =
                               Directory.GetDirectories(Path.Combine(FullPackagePath, "lib"))
                                        .Select(x => Path.GetFileName(x).ToLower())
                                        .Where(x => knownFrameworkVersions.Contains(x))
                                        .ToArray());
                }
            }

            private string FullPackagePath
            {
                get
                {
                    return this.fullPackagePath
                           ?? (this.fullPackagePath = Path.Combine(this.solutionDirectoy, "packages", Id + "." + Version));
                }
            }


            public static IEnumerable<NugetPackageElement> Load(string solutionDirectory)
            {
                return Load(Directory.EnumerateFiles(solutionDirectory, "*.csproj", SearchOption.AllDirectories),
                            solutionDirectory);
            }


            public static IEnumerable<NugetPackageElement> Load(IEnumerable<string> projects, string solutionDirectory)
            {
                foreach (var projectFile in projects)
                {
                    var projectLocal = projectFile;
                    var packagesXml = Path.Combine(Path.GetDirectoryName(projectFile), "packages.config");
                    if (!File.Exists(packagesXml))
                    {
                        Console.WriteLine("Project {0} has no packages.config; skipping.", projectFile);
                        continue;
                    }

                    var projectXDoc = XDocument.Load(projectFile);
                    foreach (
                        var item in
                            XDocument.Load(packagesXml).Descendants("package").Select(
                                x => new NugetPackageElement(projectLocal, x, solutionDirectory, projectXDoc)))
                        yield return item;
                }
            }


            public int ValidateHintPathReference(StringBuilder errorLog)
            {
                int errorCount = 0;
                if (!Directory.Exists(FullPackagePath))
                    Console.WriteLine("Warning: Could not find package directory " + FullPackagePath);

                XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
                var nsManager = new XmlNamespaceManager(new NameTable());
                nsManager.AddNamespace("x", ns.NamespaceName);
                var assumedPackagePathStart = AssumedPackagePathStart;
                var xpathPredicate =
                    String.Format(
                        "//x:Project/x:ItemGroup/x:Reference[starts-with(x:HintPath,'{0}') and string-length(x:HintPath) > ({1} + 1) and string(number(substring(x:HintPath,{1},1))) != 'NaN']",
                        assumedPackagePathStart,
                        assumedPackagePathStart.Length + 1);
                var referencesGroupedByVersion =
                    ProjectXmlDocument.XPathSelectElements(xpathPredicate, nsManager).Select(
                        x => new LibReference(this, assumedPackagePathStart, x, ns)).ToList().GroupBy(
                            x => x.PathVersionPart).ToList();
                if (referencesGroupedByVersion.Count == 0)
                {
                    if (Directory.Exists(this.fullPackagePath)
                        && Directory.EnumerateFiles(this.fullPackagePath, "*.dll", SearchOption.AllDirectories).Any())
                    {
                        errorLog.AppendFormat(
                            "Warning: Could not find any reference to {0} in {1}.\r\n    Suggestion: Update-Package -reinstall {0} -ProjectName {1}\r\n",
                            Id,
                            ProjectName);
                    }
                }

                if (referencesGroupedByVersion.Count > 1)
                {
                    errorLog.AppendFormat("There are dll-references from {0} to multiple versions of {1} ({2})\r\n",
                                          ProjectName,
                                          Id,
                                          String.Join(", ", referencesGroupedByVersion.Select(x => x.Key)));
                    errorCount++;
                }

                foreach (var refVerGroup in referencesGroupedByVersion)
                {
                    if (!DirVersionMatches(Version, refVerGroup.Key))
                    {
                        errorLog.AppendFormat(
                            "Reference made from {0} to {1} got hintpath {2}, expected {3}\\???.\r\n    Suggestion: Update-Package –reinstall {1} -ProjectName {0}\r\n",
                            ProjectName,
                            Id,
                            refVerGroup.First().HintPath,
                            AssumedPackagePath);
                        errorCount++;
                    }
                    foreach (var item in refVerGroup.Where(x => x.TargetFrameworkVersion != PreferredFrameworkVersion))
                    {
                        errorLog.AppendFormat(
                            "Reference from {0} to {1} got hintpath {2} targeting wrong framework {3}, should be {4}.\r\n    Suggestion: Update-Package –reinstall {1} -ProjectName {0}\r\n",
                            ProjectName, Id, item.HintPath, item.TargetFrameworkVersion, PreferredFrameworkVersion);
                        errorCount++;
                    }
                }
                return errorCount;
            }


            private static bool DirVersionMatches(string version, string dirVersion)
            {
                if (version == dirVersion)
                    return true;

                // Directory 1.8 matches version 1.8.0, so if last part of version is .0 we can remove it.
                if (ParseVersionParts(version).SequenceEqual(ParseVersionParts(dirVersion)))
                    return true;
                return false;
            }


            private string GetRelativePath(string filespec, string folder)
            {
                Uri pathUri = new Uri(filespec);
                // Folders must end in a slash
                if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    folder += Path.DirectorySeparatorChar;
                Uri folderUri = new Uri(folder);
                return
                    Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/',
                                                                                                 Path.DirectorySeparatorChar));
            }


            private static int[] ParseVersionParts(string version)
            {
                string _;
                return ParseVersionParts(version, out _);
            }


            private static int[] ParseVersionParts(string version, out string preReleasePart)
            {
                var mainAndPreleaseParts = version.Split('-');
                if (mainAndPreleaseParts.Length > 1)
                    preReleasePart = mainAndPreleaseParts[1];
                else
                    preReleasePart = null;
                return mainAndPreleaseParts[0].Split('.').Select(Int32.Parse).Pad(3, 0).ToArray();
            }

            #region Nested type: LibReference

            private class LibReference
            {
                private readonly string hintPath;
                private readonly NugetPackageElement parent;
                private readonly string pathVersionPart;
                private readonly string targetFrameworkVersion;


                public LibReference(NugetPackageElement parent,
                                    string hintPathBeforePrefix,
                                    XElement element,
                                    XNamespace ns)
                {
                    this.parent = parent;
                    var hintPathElement = element.Descendants(ns + "HintPath").First();
                    this.hintPath = hintPathElement.Value;
                    this.pathVersionPart = this.hintPath.Substring(hintPathBeforePrefix.Length,
                                                                   this.hintPath.IndexOfAny(new char[] { '\\', '/' },
                                                                                            hintPathBeforePrefix.Length)
                                                                   - hintPathBeforePrefix.Length);
                    var packageRelativePath = this.hintPath.Substring(hintPathBeforePrefix.Length + this.pathVersionPart.Length + 1);

                    var pathSegments = packageRelativePath.Split(Path.DirectorySeparatorChar);
                    if (pathSegments[0] == "lib")
                    {
                        this.targetFrameworkVersion =
                            pathSegments
                                .Skip(1)
                                .Take(1)
                                .Where(x => knownFrameworkVersions.Contains(x, StringComparer.OrdinalIgnoreCase))
                                .Select(x => x.ToLower())
                                .FirstOrDefault();
                    }
                }


                public string HintPath
                {
                    get { return this.hintPath; }
                }

                public string PathVersionPart
                {
                    get { return this.pathVersionPart; }
                }

                public string TargetFrameworkVersion
                {
                    get { return this.targetFrameworkVersion; }
                }
            }

            #endregion

            #region Nested type: VersionComparer

            internal class VersionComparer : StringComparer
            {
                public override int Compare(string x, string y)
                {
                    string xPreRelease;
                    var xParts = ParseVersionParts(x, out xPreRelease).Pad(4, 0).ToArray();
                    string yPreRelease;
                    var yParts = ParseVersionParts(y, out yPreRelease).Pad(4, 0).ToArray();
                    if (xParts.SequenceEqual(yParts))
                    {
                        if (xPreRelease == null && yPreRelease == null)
                            return 0;
                        if (xPreRelease == null)
                            return 1;
                        if (yPreRelease == null)
                            return -1;
                        return InvariantCultureIgnoreCase.Compare(xPreRelease, yPreRelease);
                    }
                    return xParts.Zip(yParts, (xp, yp) => xp - yp).First(q => q != 0);
                }


                public override bool Equals(string x, string y)
                {
                    return Compare(x, y) == 0;
                }


                public override int GetHashCode(string obj)
                {
                    return obj.GetHashCode();
                }
            }

            #endregion
        }

        #endregion
    }
}