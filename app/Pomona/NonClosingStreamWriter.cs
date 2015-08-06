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

using System.IO;
using System.Text;

namespace Pomona
{
    /// <summary>
    /// A version of <see cref="StreamWriter"/> that doesn't close the underlying <see cref="Stream"/> when <see cref="Dispose"/>d.
    /// Writes to the underlying <see cref="Stream"/> with a BOM-less UTF-8 encoding.
    /// </summary>
    internal class NonClosingStreamWriter : StreamWriter
    {
        private static readonly Encoding bomLessUtf8Encoding;


        /// <summary>Initializes the <see cref="NonClosingStreamWriter"/> class.</summary>
        static NonClosingStreamWriter()
        {
            bomLessUtf8Encoding = new UTF8Encoding(false);
        }


        /// <summary>Initializes a new instance of the <see cref="NonClosingStreamWriter"/> class.</summary>
        /// <param name="stream">The stream to write to.</param>
        public NonClosingStreamWriter(Stream stream)
            : base(stream, bomLessUtf8Encoding)
        {
        }


        /// <summary>
        /// Flushes the content of the <see cref="StreamWriter"/> to the underlying <see cref="Stream"/>,
        /// but does not close it.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Flush();

            base.Dispose(false);
        }
    }
}