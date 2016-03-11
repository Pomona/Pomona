#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;

namespace Pomona.UnitTests.CodeGen
{
    public class IndependentClass
    {
        public static readonly IndependentClass StaticField = new IndependentClass("klsjlkdj");
        private List<IndependentClass> listOfMySelf;


        private IndependentClass(string klsjlkdj)
        {
            this.listOfMySelf = new List<IndependentClass>();
        }


        public bool AutoProp { get; set; }
        public IndependentClass SelfRef { get; set; }


        public bool AnotherMethod()
        {
            OneMethod();
            return false;
        }


        public void OneMethod()
        {
            AnotherMethod();
        }


        public static void OutMethod(out IndependentClass bah)
        {
            bah = new IndependentClass(null);
        }
    }
}