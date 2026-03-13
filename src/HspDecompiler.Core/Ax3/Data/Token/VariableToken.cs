using System.Text;
using HspDecompiler.Core.Ax3.Data.Primitive;

namespace HspDecompiler.Core.Ax3.Data.Token
{
    internal sealed class VariableToken : OperandToken
    {
        private VariableToken() { }
        internal VariableToken(VariablePrimitive var)
        {
            primitive = var;
            arg = null;
        }

        internal VariableToken(VariablePrimitive var, ArgumentToken theArg)
        {
            primitive = var;
            arg = theArg;
        }

        readonly VariablePrimitive primitive = null;
        readonly ArgumentToken arg = null;

        internal override int TokenOffset
        {
            get
            {
                if (primitive == null)
                    return -1;
                return primitive.TokenOffset;
            }
        }

        public override string ToString()
        {
            if (arg == null)
                return primitive.ToString();
            StringBuilder builder = new StringBuilder(primitive.ToString());
            builder.Append(arg.ToString());
            return builder.ToString();
        }

        internal override int Priority
        {
            get { return 100; }
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
