using System.Text;
using HspDecompiler.Core.Ax3.Data.Primitive;
using HspDecompiler.Core.Ax3.Data.Token;

namespace HspDecompiler.Core.Ax3.Data.Line
{
    internal sealed class OnEventStatement : LogicalLine
    {
        private OnEventStatement() { }
        internal OnEventStatement(OnEventFunctionPrimitive theToken, FunctionToken func)
        {
            this.token = theToken;
            this.func = func;
        }

        private readonly OnEventFunctionPrimitive token = null;
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
            if (func != null)
            {
                builder.Append(' ');
                builder.Append(func.ToString());
            }
            return builder.ToString();
        }

        internal override void CheckLabel()
        {
            if (func != null)
                func.CheckLabel();
        }

        internal override bool CheckRpn()
        {
            if (func != null)
                return func.CheckRpn();
            return true;
        }
    }
}
