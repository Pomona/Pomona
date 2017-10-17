﻿#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Nancy
{
    /// <summary>
    /// Interface responsible for keeping a map of session factories related to modules.
    /// </summary>
    internal interface IPomonaModuleConfigurationBinder
    {
        IPomonaSessionFactory GetFactory(PomonaModule module);
    }
}