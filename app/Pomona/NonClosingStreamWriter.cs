#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

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