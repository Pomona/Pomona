#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Collections.Concurrent;

namespace Pomona.Nancy
{
    /// <summary>
    /// Class responsible for keeping a map of session factories related to modules.
    /// </summary>
    internal class PomonaModuleConfigurationBinder : IPomonaModuleConfigurationBinder
    {
        private readonly ConcurrentDictionary<Type, IPomonaSessionFactory> moduleFactoryMap =
            new ConcurrentDictionary<Type, IPomonaSessionFactory>();


        static PomonaModuleConfigurationBinder()
        {
            Current = new PomonaModuleConfigurationBinder();
        }


        public static IPomonaModuleConfigurationBinder Current { get; set; }


        private IPomonaSessionFactory CreateFactory(PomonaModule module)
        {
            var conf = module.GetConfiguration();
            return conf.CreateSessionFactory();
        }


        public IPomonaSessionFactory GetFactory(PomonaModule module)
        {
            return this.moduleFactoryMap.GetOrAdd(module.GetType(), t => CreateFactory(module));
        }
    }
}