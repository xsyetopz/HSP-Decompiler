using System.Text;
using HspDecompiler.Core.Ax3.Data.Primitive;
using HspDecompiler.Core.Ax3.Data.Token;

namespace HspDecompiler.Core.Ax3.Data.Line
{
    internal sealed class IfStatement : LogicalLine
    {
        private IfStatement() { }
        internal IfStatement(IfStatementPrimitive token)
        {
            ifToken = token;
        }

        internal IfStatement(IfStatementPrimitive token, ArgumentToken theArg)
        {
            ifToken = token;
            arg = theArg;
        }

        readonly IfStatementPrimitive ifToken = null;
        readonly ArgumentToken arg = null;

        internal override int TokenOffset
        {
            get
            {
                if (ifToken == null)
                    return -1;
                return ifToken.TokenOffset;
            }
        }

        internal int JumpToOffset
        {
            get
            {
                return ifToken.JumpToOffset;
            }
        }

        internal bool isIfStatement
        {
            get
            {
                if ((ifToken.CodeType & HspCodeType.IfStatement) == HspCodeType.IfStatement)
                    return true;
                return false;
            }
        }

        internal bool isElseStatement
        {
            get
            {
                if ((ifToken.CodeType & HspCodeType.ElseStatement) == HspCodeType.ElseStatement)
                    return true;
                return false;
            }
        }

        private bool scoopEndIsDefined = false;
        internal bool ScoopEndIsDefined { get { return scoopEndIsDefined; } set { scoopEndIsDefined = value; } }
        internal override bool TabIncrement { get { return scoopEndIsDefined; } }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(ifToken.ToString());
            if (arg != null)
            {
                builder.Append(" (");
                builder.Append(arg.ToString());
                builder.Append(" )");
            }
            builder.Append(" {");
            return builder.ToString();
        }

        internal override void CheckLabel()
        {
            if (arg != null)
                arg.CheckLabel();
        }

        internal override bool CheckRpn()
        {
            if (arg != null)
                return arg.CheckRpn();
            return true;
        }
    }
}
