#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;

using Pomona.Common.Internals;

namespace Pomona.CodeGen
{
    /// <summary>
    /// Contains extension methods for <see cref="string"/>.
    /// </summary>
    public static class VersionStringExtensions
    {
        /// <summary>
        /// Pads the <paramref name="versionNumber" /> to four dot separated segments.
        /// E.g. '1.2' will be padded to '1.2.0.0'.
        /// </summary>
        /// <param name="versionNumber">The version number.</param>
        /// <param name="numberOfVersionSegments">The number of dot separated segments to have in the returned version number.</param>
        /// <returns>
        /// Pads the <paramref name="versionNumber" /> to four dot separated segments.
        /// E.g. '1.2' will be padded to '1.2.0.0'.
        /// </returns>
        public static string PadTo(this string versionNumber, int numberOfVersionSegments)
        {
            versionNumber = (versionNumber ?? String.Empty).Trim();
            var segments = versionNumber.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            var padded = segments.Pad(numberOfVersionSegments, "0");
            var taken = padded.Take(numberOfVersionSegments);
            return String.Join(".", taken);
        }
    }
}