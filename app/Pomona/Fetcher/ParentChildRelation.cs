#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

namespace Pomona.Fetcher
{
    internal class ParentChildRelation
    {
        public ParentChildRelation(object parentId, object childId)
        {
            ParentId = parentId;
            ChildId = childId;
        }


        public object ChildId { get; }

        public object ParentId { get; }
    }
}