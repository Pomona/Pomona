using System;

namespace Pomona.Documentation.Nodes
{
    internal abstract class DocNode : IDocNode
    {
        public string ToString(Func<IDocNode, string> formattingHook)
        {
            if (formattingHook != null)
            {
                var result = formattingHook(this);
                if (result != null)
                    return result;
            }
            return OnToString(formattingHook);
        }


        protected abstract string OnToString(Func<IDocNode, string> formattingHook);

        public sealed override string ToString()
        {
            return ToString(null);
        }
    }
}