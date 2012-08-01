// ----------------------------------------------------------------------------
// Pomona source code
// 
// Copyright © 2012 Karsten Nikolai Strand
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Pomona
{
    public class PomonaQueryFilterParser
    {
        private readonly TypeMapper typeMapper;

        public PomonaQueryFilterParser(TypeMapper typeMapper)
        {
            if (typeMapper == null) throw new ArgumentNullException("typeMapper");
            this.typeMapper = typeMapper;
        }


        public IEnumerable<PomonaQuery.Condition> Parse(TransformedType rootType, string filter, out bool errorWhileParsing)
        {
            if (rootType == null) throw new ArgumentNullException("rootType");
            var parts = filter.Split(',').Select(x => ParseFilterPart(rootType, x)).ToArray();

            // For supporting graceful error handling
            errorWhileParsing = parts.Contains(null);
            return parts.Where(x => x != null);
        }

        public PomonaQuery.Condition ParseFilterPart(TransformedType rootType, string filterPart)
        {
            // A filter part is split by the character $
            var split = filterPart.Split('$');

            if (split.Length < 3)
                return null;

            var propnameRaw = split[0];
            var operatorRaw = split[1];
            var valueRaw = split[2];

            var translatedPropertyName = typeMapper.ConvertToInternalPropertyPath(rootType, propnameRaw);

            PomonaQuery.Operator op;
            if (!Enum.TryParse(operatorRaw, true, out op))
                return null;

            return new PomonaQuery.Condition() {Operator = op, PropertyName = translatedPropertyName, Value = valueRaw};
        }
    }
}