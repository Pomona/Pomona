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

namespace Pomona.Common
{
    public abstract class ResourceFetchContext : IResourceFetchContext
    {
        private static readonly IResourceFetchContext @default;
        private static readonly IResourceFetchContext lazy;


        static ResourceFetchContext()
        {
            lazy = new LazyResourceFetchContext();
            @default = new DefaultResourceFetcContext();
        }


        public static IResourceFetchContext Default
        {
            get { return @default; }
        }

        public static IResourceFetchContext Lazy
        {
            get { return lazy; }
        }

        public abstract bool LazyEnabled { get; }

        #region Nested type: DefaultResourceFetcContext

        private class DefaultResourceFetcContext : ResourceFetchContext
        {
            public override bool LazyEnabled
            {
                get
                {
                    // TODO: We need to get this from global settings somehow. @asbjornu
                    return false;
                }
            }
        }

        #endregion

        #region Nested type: LazyResourceFetchContext

        private class LazyResourceFetchContext : ResourceFetchContext
        {
            public override bool LazyEnabled
            {
                get { return true; }
            }
        }

        #endregion
    }
}