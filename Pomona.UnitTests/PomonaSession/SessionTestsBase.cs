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

using System.Linq;
using NUnit.Framework;
using Pomona.Example;
using Pomona.Example.Models;

namespace Pomona.UnitTests.PomonaSession
{
    public abstract class SessionTestsBase
    {
        private CritterDataSource dataSource;
        private Critter firstCritter;
        private Pomona.PomonaSession session;
        private TypeMapper typeMapper;

        public Critter FirstCritter
        {
            get { return firstCritter; }
        }

        public int FirstCritterId
        {
            get { return firstCritter.Id; }
        }

        protected IPomonaDataSource DataSource
        {
            get { return dataSource; }
        }

        protected TypeMapper TypeMapper
        {
            get { return typeMapper; }
        }

        protected Pomona.PomonaSession Session
        {
            get { return session; }
        }

        [SetUp]
        public void SetUp()
        {
            dataSource = new CritterDataSource();
            typeMapper = new TypeMapper(CritterDataSource.GetEntityTypes());
            session = new Pomona.PomonaSession(dataSource, typeMapper, UriResolver);
            firstCritter = dataSource.List<Critter>().First();
        }

        private string UriResolver(object x)
        {
            var entity = x as EntityBase;
            if (entity == null)
                return null;

            return string.Format("http://localhost/{0}/{1}", x.GetType().Name.ToLower(), entity.Id);
        }
    }
}