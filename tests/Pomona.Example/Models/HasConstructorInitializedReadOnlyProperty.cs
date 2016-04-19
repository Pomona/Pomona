#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Example.Models
{
    public class HasConstructorInitializedReadOnlyProperty : EntityBase
    {
        public HasConstructorInitializedReadOnlyProperty(CrazyValueObject crazy)
        {
            Crazy = crazy;
        }


        public CrazyValueObject Crazy { get; }
    }
}