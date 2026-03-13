using System.Text;
using HspDecompiler.Core.Ax3.Data.Primitive;
using HspDecompiler.Core.Ax3.Data.Token;

namespace HspDecompiler.Core.Ax3.Data.Line
{
    internal sealed class OnStatement : LogicalLine
    {
        private OnStatement() { }
        internal OnStatement(OnFunctionPrimitive theToken, ExpressionToken exp, FunctionToken func)
        {
            this.token = theToken;
            this.exp = exp;
            this.func = func;
        }

        private readonly OnFunctionPrimitive token = null;
        private readonly ExpressionToken exp = null;
        private readonly FunctionToken func = null;

        internal override int TokenOffset
        {
            get { return token.TokenOffset; }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (token != null)
            {
                builder.Append(token.ToString());
            }
            if (exp != null)
            {
                builder.Append(' ');
                builder.Append(exp.ToString());
            }
            if (func != null)
            {
                builder.Append(' ');
                builder.Append(func.ToString());
            }
            return builder.ToString();
        }

        internal override void CheckLabel()
        {
            if (exp != null)
                exp.CheckLabel();
            if (func != null)
                func.CheckLabel();
        }

        internal override bool CheckRpn()
        {
            bool ret = true;
            if (exp != null)
                ret &= exp.CheckRpn();
            if (func != null)
                ret &= func.CheckRpn();
            return ret;
        }
    }
}
