#region License

// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2013 Karsten Nikolai Strand
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
using System.Linq;
using System.Reflection;
using Pomona.Common;
using Pomona.Common.TypeSystem;

namespace Pomona.Handlers
{
    public class PomonaMethodScanner
    {
        private readonly TypeMapper typeMapper;

        public PomonaMethodScanner(TypeMapper typeMapper)
        {
            if (typeMapper == null) throw new ArgumentNullException("typeMapper");
            this.typeMapper = typeMapper;
        }

        private static IEnumerable<PomonaMethodInfo> GetMethods(Type type)
        {
            return
                type.GetMethods()
                    .Select(m => new PomonaMethodInfo(m, m.GetFirstOrDefaultAttribute<PomonaMethodAttribute>(true)))
                    .Where(x => x.Attribute != null);
        }

        public void ScanPostToResourceHandlers(Type handlerClass)
        {
            // TODO: MAKE THIS WORK AGAIN!
            return;

#if false
            foreach (var x in GetMethods(handlerClass)
                .Where(x => x.IsPost)
                .Where(x => x.Parameters.Length == 2))
            {
                var targetType = typeMapper.GetClassMapping(x.Parameters[0].ParameterType) as ResourceType;
                var formType = typeMapper.GetClassMapping(x.Parameters[1].ParameterType);

                if (targetType == null)
                    throw new InvalidOperationException(
                        "Target type of post-to-resource handler does not have a valid URL.");

                targetType.DeclaredPostHandlers.Add(new HandlerInfo(x.Attribute.HttpMethod, x.Attribute.UriName,
                                                                    x.Method, targetType, formType));
            }
#endif
        }

        private class PomonaMethodInfo
        {
            private readonly PomonaMethodAttribute attribute;
            private readonly MethodInfo method;
            private readonly ParameterInfo[] parameters;

            public PomonaMethodInfo(MethodInfo method, PomonaMethodAttribute attribute)
            {
                this.method = method;
                parameters = method.GetParameters();
                this.attribute = attribute;
            }

            public ParameterInfo[] Parameters
            {
                get { return parameters; }
            }

            public bool IsPost
            {
                get { return string.Equals("POST", attribute.HttpMethod); }
            }

            public MethodInfo Method
            {
                get { return method; }
            }

            public PomonaMethodAttribute Attribute
            {
                get { return attribute; }
            }
        }
    }
}