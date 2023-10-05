﻿#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

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

            Assert.That(programFilesX86Path, Is.Not.Null, "Program Files path is null");

            var peverifyPath = Path.Combine(programFilesX86Path,
                                            @"Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\PEVerify.exe");
            if (!File.Exists(peverifyPath))
                Assert.Inconclusive("Unable to run peverify test, need to have Microsoft sdk installed.");

            dllPath = dllPath.Replace("\"", "\\\"");
            var peVerifyArguments = $"\"{dllPath}\" /md /il";

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

