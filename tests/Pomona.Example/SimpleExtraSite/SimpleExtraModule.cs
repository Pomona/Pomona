using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pomona.Example.SimpleExtraSite;

namespace Pomona.Example.SimpleExtraSite
{
    public class SimpleExtraModule:PomonaModule
    {
        public SimpleExtraModule()
            : base(new SimpleDataSource(), new TypeMapper(new SimplePomonaConfiguration()),"/Extra")
        {
        }
    }
}
