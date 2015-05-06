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

using System;

namespace Pomona.Example.Models
{
    public class Loner : EntityBase
    {
        private readonly string name;
        private readonly DateTime optionalDate;
        private readonly string optionalInfo;
        private readonly int strength;


        public Loner(string name, int strength, string optionalInfo, DateTime? optionalDate = null)
        {
            this.name = name;
            this.strength = strength;
            this.optionalInfo = optionalInfo;
            this.optionalDate = optionalDate ?? DateTime.UtcNow;
            Occupation = "default boring";
        }


        public string Name
        {
            get { return this.name; }
        }

        public string Occupation { get; set; }

        public DateTime OptionalDate
        {
            get { return this.optionalDate; }
        }

        public string OptionalInfo
        {
            get { return this.optionalInfo; }
        }

        public int Strength
        {
            get { return this.strength; }
        }
    }
}