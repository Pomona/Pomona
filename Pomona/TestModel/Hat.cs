using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pomona.TestModel
{
    public class Hat : EntityBase
    {
        public Hat()
        {
            HatType = "Hat#" + new Random().Next();
        }
        public string HatType { get; set; }
    }
}
