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
using System.Diagnostics;
using System.IO;

using NUnit.Framework;

namespace Pomona.UnitTests
{
    public static class PeVerifyHelper
    {
        public static void Verify(string dllPath)
        {
            var programFilesX86Path = Environment.GetEnvironmentVariable("ProgramFiles(x86)") ??
                                      Environment.GetEnvironmentVariable("ProgramFiles");
            Assert.NotNull(programFilesX86Path);

            var peverifyPath = Path.Combine(programFilesX86Path,
                @"Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\PEVerify.exe");
            if (!File.Exists(peverifyPath))
                Assert.Inconclusive("Unable to run peverify test, need to have Microsoft sdk installed.");

            var peVerifyArguments = string.Format("\"{0}\" /md /il", dllPath.Replace("\"", "\\\""));

            var proc = new Process
            {
                StartInfo =
                    new ProcessStartInfo(peverifyPath, peVerifyArguments)
                    {
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = false,
                        UseShellExecute = false
                    }
            };
            proc.Start();
            Console.Write(proc.StandardOutput.ReadToEnd());
            proc.WaitForExit();
            Assert.That(proc.ExitCode, Is.EqualTo(0), "PEVerify returned error code " + proc.ExitCode);
        }


 
    }
}