#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Documentation.Nodes
{
    internal abstract class DocNode : IDocNode
    {
        public override sealed string ToString()
        {
            return ToString(null);
        }


        protected abstract string OnToString(Func<IDocNode, string> formattingHook);


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
    }
}