#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at https://pomona.rest/

#endregion

using System;

using Mono.Cecil;

namespace Pomona.UnitTests.GenerateClientDllApp
{
    internal static class Extensions
    {
        public static bool UnableToLoadAssemblyFromFileShare(this Exception exception, string assemblyFileName)
        {
            if (!(exception is AssemblyResolutionException))
                return false;

            return assemblyFileName.IsFileShare()
                   || assemblyFileName.IsProbablyFileShare();
        }


        private static bool IsFileShare(this string path)
        {
            return path != null && path.StartsWith("\\\\");
        }


        private static bool IsProbablyFileShare(this string path)
        {
            if (path == null)
                return false;

            const int characterE = (int)'E';
            const int haracterZ = (int)'Z';

            for (var i = characterE; i <= haracterZ; i++)
            {
                var character = (char)i;
                var stupidWindowsDriveLetterSequence = String.Concat(character, ":\\");

                if (path.StartsWith(stupidWindowsDriveLetterSequence))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

