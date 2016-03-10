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
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ICSharpCode.NRefactory.CSharp;

using NUnit.Framework;

using Pomona.Common.Internals;
using Pomona.TestHelpers;

namespace Pomona.UnitTests
{
    [TestFixture]
    public class SolutionTests
    {
        private static string SolutionDirectory
        {
            get { return Path.GetDirectoryName(SolutionTestsHelper.FindSolutionPathOf<SolutionTests>()); }
        }


        [Ignore("Does not support C# 6, must be reimplemented using roslyn")]
        [Test]
        public void AllClassesAreContainedInFilesWithCorrectName()
        {
            var p = new CSharpParser();
            var errorCount = 0;

            var csFiles = SolutionTestsHelper.FindCSharpSourceFiles(SolutionDirectory).Select(x => p.Parse(File.ReadAllText(x), x));
            foreach (var csFile in csFiles)
            {
                var topLevelTypes =
                    csFile.Children.Flatten(x => x is TypeDeclaration ? Enumerable.Empty<AstNode>() : x.Children)
                          .OfType<TypeDeclaration>()
                          .Where(x => x.Name != Path.GetFileNameWithoutExtension(csFile.FileName))
                          .Select(x => new { x.Name, csFile.FileName })
                          .ToList();

                foreach (var td in topLevelTypes)
                    Console.WriteLine("Type " + td.Name + " does not match filename " + td.FileName);

                errorCount += topLevelTypes.Count;
            }

            Assert.That(errorCount, Is.EqualTo(0));
        }


        [Test]
        public void AllCsFilesAreIncludedInProjects()
        {
            foreach (var csProjFile in SolutionTestsHelper.FindSourceFiles("*.csproj", SolutionDirectory))
                SolutionTestsHelper.VerifyProjectNoOrphanSourceCodeFiles(csProjFile);
        }


        [Test]
        public void AllPackagesInSolutionHaveSameVersionAndReferencesAreCorrect()
        {
            var excludedPackages = new string[] { "NHibernate", "Iesi.Collections" };
            SolutionTestsHelper.VerifyNugetPackageReferences(SolutionTestsHelper.FindSolutionPathOf(this),
                                                             x => !excludedPackages.Contains(x.Id));
        }


        [Test]
        public void NoSourceCodeContainsNoCommit()
        {
            const string commitBlockString = "NO" + "COMMIT";
            NoSourceCodeContains(commitBlockString);
        }


        private static void NoSourceCodeContains(string shouldNotCountainString)
        {
            var path = SolutionDirectory;

            var sourceCodeFiles = SolutionTestsHelper.FindCSharpSourceFiles(path);

            var foundFilesWithNoCommitMessage = false;

            foreach (var file in sourceCodeFiles)
            {
                var fileText = File.ReadAllText(file);
                var fileName = file.Replace(path + Path.DirectorySeparatorChar, String.Empty);
                var lines = fileText.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                IList<Tuple<int, int, string>> finds = new List<Tuple<int, int, string>>();

                for (var i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var lineNumber = i + 1;
                    var index = line.IndexOf(String.Concat(' ', shouldNotCountainString),
                                             StringComparison.InvariantCulture);
                    if (index == -1)
                        continue;

                    finds.Add(new Tuple<int, int, string>(lineNumber, index, line));
                }

                if (!finds.Any())
                    continue;

                foundFilesWithNoCommitMessage = true;

                Console.WriteLine("Found '{0}' in '{1}':", shouldNotCountainString, fileName);

                foreach (var find in finds)
                {
                    var lineNumber = find.Item1;
                    var index = find.Item2;
                    var line = find.Item3;
                    Console.Write("- ({0}, {1}): ", lineNumber, index);
                    Console.WriteLine(line.Trim());
                }
            }

            Assert.That(foundFilesWithNoCommitMessage, Is.False, "Found files with '{0}'.", shouldNotCountainString);
        }
    }
}