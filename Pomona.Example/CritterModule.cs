#region License

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

#endregion

using System;
using System.Collections.Generic;
using System.Linq;

using Pomona.Example.Models;

namespace Pomona.Example
{
    public class CritterModule : PomonaModule
    {
        private CritterRepository critterRepository;
        private IEnumerable<Type> entityTypes;


        public CritterModule()
        {
        }


        public CritterRepository CritterRepository
        {
            get { return this.critterRepository ?? (this.critterRepository = new CritterRepository()); }
        }


        protected override T GetById<T>(int id)
        {
            return (T)((object)CritterRepository.GetAll<T>().Cast<EntityBase>().FirstOrDefault(x => x.Id == id));
        }


        protected override Type GetEntityBaseType()
        {
            return typeof(EntityBase);
        }


        protected override IEnumerable<Type> GetEntityTypes()
        {
            if (this.entityTypes == null)
                this.entityTypes =
                    typeof(CritterModule).Assembly.GetTypes().Where(x => x.Namespace == "Pomona.Example.Models");
            return this.entityTypes;
        }


        protected override int GetIdFor(object entity)
        {
            return ((EntityBase)entity).Id;
        }


        protected override IList<T> ListAll<T>()
        {
            return CritterRepository.GetAll<T>();
        }
    }
}