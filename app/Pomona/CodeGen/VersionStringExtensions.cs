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