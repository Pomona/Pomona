#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Diagnostics;

using Nancy.Hosting.Self;

namespace Pomona.Samples.DustyBoxes
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseUri = new Uri("http://localhost:1337");
            var nancyHost =
                new NancyHost(new HostConfiguration()
                {
                    UrlReservations = new UrlReservations() { CreateAutomatically = true }
                }, baseUri);
            nancyHost.Start();
            Process.Start(baseUri.ToString());
            Console.WriteLine($"Starting server on {baseUri}, press enter to stop..");
            Console.ReadLine();
            nancyHost.Stop();
        }
    }
}