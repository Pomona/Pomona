#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Example.Models.Existence
{
    public abstract class CelestialObject : EntityBase
    {
        protected CelestialObject(string name)
            : this()
        {
            Name = name;
        }


        protected CelestialObject()
        {
            ETag = Guid.NewGuid().ToString();
        }


        public string ETag { get; set; }
        public string Name { get; set; }
    }
}