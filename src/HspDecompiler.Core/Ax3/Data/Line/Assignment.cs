using System.Text;
using HspDecompiler.Core.Ax3.Data.Token;

namespace HspDecompiler.Core.Ax3.Data.Line
{
    internal sealed class Assignment : LogicalLine
    {
        private Assignment() { }
        internal Assignment(VariableToken theVar, OperatorToken theOp)
        {
            var = theVar;
            op = theOp;
        }
        internal Assignment(VariableToken theVar, OperatorToken theOp, ArgumentToken theArg)
        {
            var = theVar;
            op = theOp;
            arg = theArg;
        }

        readonly VariableToken var = null;
        readonly OperatorToken op = null;
        readonly ArgumentToken arg = null;

        internal override int TokenOffset
        {
            get
            {
                if (var == null)
                    return -1;
                return var.TokenOffset;
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(var.ToString());
            if (arg != null)
            {
                builder.Append(' ');
                builder.Append(op.ToString(true, arg != null));
                builder.Append(arg.ToString());
            }
            else
            {
                builder.Append(op.ToString(true, arg != null));
            }
            return builder.ToString();
        }

        internal override void CheckLabel()
        {
            if (var != null)
                var.CheckLabel();
            if (op != null)
                op.CheckLabel();
            if (arg != null)
                arg.CheckLabel();
        }

        internal override bool CheckRpn()
        {
            bool ret = true;
            if (var != null)
                ret &= var.CheckRpn();
            if (arg != null)
                ret &= arg.CheckRpn();
            return true;
        }
    }
}
