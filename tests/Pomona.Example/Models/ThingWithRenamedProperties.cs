#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.Generic;

namespace Pomona.Example.Models
{
    public class ThingWithRenamedProperties : EntityBase
    {
        private IList<JunkWithRenamedProperty> relatedJunks;


        public ThingWithRenamedProperties()
        {
            this.relatedJunks = new List<JunkWithRenamedProperty>();
        }


        /// <summary>
        /// This property will be called DiscoFunky when mapped.
        /// </summary>
        public virtual JunkWithRenamedProperty Junky { get; set; }

        /// <summary>
        /// This property will be called PrettyThings when renamed.
        /// </summary>
        public virtual IList<JunkWithRenamedProperty> RelatedJunks
        {
            get { return this.relatedJunks; }
            set { this.relatedJunks = value; }
        }
    }
}