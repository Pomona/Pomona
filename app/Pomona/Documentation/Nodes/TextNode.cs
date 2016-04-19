#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System;

namespace Pomona.Documentation.Nodes
{
    internal class TextNode : DocNode, ITextNode
    {
        public TextNode(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            Text = text;
        }


        public string Text { get; private set; }


        protected override string OnToString(Func<IDocNode, string> formattingHook)
        {
            return Text;
        }


        public string Value => Text;
    }
}