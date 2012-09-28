using System;

namespace Pomona.Client
{
    public class PomonaResourceInfo
    {
        public Type InterfaceType { get; set; }
        public Type PocoType { get; set; }
        public Type PutFormType { get; set; }
        public Type PostFormType { get; set; }
        public Type LaxyProxyType { get; set; }
    }
}