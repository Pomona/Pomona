#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

using Pomona.Example;

namespace Pomona
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var critterHost = new CritterHost(new Uri("http://localhost:2211"));
            critterHost.Start();
            Console.WriteLine("Started critter host on " + critterHost.BaseUri);
            Console.ReadKey();
            critterHost.Stop();
        }
    }
}