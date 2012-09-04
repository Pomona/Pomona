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

namespace Pomona.Example.Models
{
    public class WebColor
    {
        public WebColor(string color)
        {
            color = color.Trim();
            // Parses #RRGGBB where each component is a hex value
            if (color.Length != 7 || color[0] != '#')
            {
                throw new InvalidOperationException(
                    "Unable to parse color, needs to be in format #RRGGBB (hex for each component)");
            }

            for (var i = 0; i < 3; i++)
            {
                var colorComponentValue = Convert.ToInt32(color.Substring(1 + (i*2), 2), 16);
                if (i == 0)
                    Red = colorComponentValue;
                if (i == 1)
                    Green = colorComponentValue;
                if (i == 2)
                    Blue = colorComponentValue;
            }
        }


        public int Blue { get; set; }
        public int Green { get; set; }
        public int Red { get; set; }


        public override string ToString()
        {
            return "The color is " + ToStringConverted();
        }


        public string ToStringConverted()
        {
            return string.Format("#{0:x2}{1:x2}{2:x2}", Red, Green, Blue);
        }
    }
}