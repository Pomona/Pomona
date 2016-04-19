#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;
using System.Linq;

namespace Pomona.Common.TypeSystem
{
    public class StructuredTypeDetails
    {
        private readonly StructuredType type;


        public StructuredTypeDetails(StructuredType type,
                                     HttpMethod allowedMethods,
                                     Action<object> onDeserialized,
                                     bool mappedAsValueObject,
                                     bool alwaysExpand,
                                     bool isAbstract)
        {
            this.type = type;
            AllowedMethods = allowedMethods;
            OnDeserialized = onDeserialized;
            MappedAsValueObject = mappedAsValueObject;
            this.type = type;
            AlwaysExpand = alwaysExpand;
            IsAbstract = isAbstract;
        }


        public HttpMethod AllowedMethods { get; }

        public bool AlwaysExpand { get; }

        public bool IsAbstract { get; }

        public bool MappedAsValueObject { get; }

        public Action<object> OnDeserialized { get; }

        public StructuredProperty PrimaryId
        {
            get { return this.type.AllProperties.OfType<StructuredProperty>().FirstOrDefault(x => x.IsPrimaryKey); }
        }
    }
}