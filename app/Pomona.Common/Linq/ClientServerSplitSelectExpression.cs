#region License

// Pomona is open source software released under the terms of the LICENSE specified in the
// project's repository, or alternatively at http://pomona.io/

#endregion

using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Pomona.Common.Linq
{
    internal class ClientServerSplitSelectExpression : PomonaExtendedExpression
    {
        public ClientServerSplitSelectExpression(PomonaExtendedExpression serverExpression,
                                                 LambdaExpression clientSideExpression)
            : base(serverExpression.Type)
        {
            ServerExpression = serverExpression;
            ClientSideExpression = clientSideExpression;
        }


        public override ReadOnlyCollection<object> Children
        {
            get { return new ReadOnlyCollection<object>(new object[] { ServerExpression }); }
        }

        public LambdaExpression ClientSideExpression { get; }

        public PomonaExtendedExpression ServerExpression { get; }

        public override bool SupportedOnServer
        {
            get { return true; }
        }
    }
}