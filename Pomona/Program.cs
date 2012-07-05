using System;
using System.Collections.Generic;
using System.Text;

using Nancy.Hosting.Self;
using System.Linq;
using Newtonsoft.Json;

using Pomona.TestModel;

namespace Pomona
{
    class Program
    {
        static void Main2(string[] args)
        {
            var repository = new CritterRepository();
            var expandedPaths = ExpandPathsUtils.GetExpandedPaths("");
            var critters =
                repository.GetAll<Critter>().Select(x => new PathTrackingProxy(x, "Critter", expandedPaths)).ToList();

            Console.WriteLine(JsonConvert.SerializeObject(critters, Formatting.Indented));
        }

        static void Main(string[] args)
        {
            //Console.ReadKey();

            
            var host = new NancyHost(new Uri("http://localhost:2211"));
            host.Start();
            Console.ReadKey();
            host.Stop();
          
        }
    }
}
