using System.Collections.Generic;
using System.Text;

namespace HspDecompiler.Core.Ax3.Data.Token
{
    internal sealed class ArgumentToken : CodeToken
    {
        private ArgumentToken() { }
        internal ArgumentToken(List<ExpressionToken> theExps, bool hasBrackets, bool firstArgIsNull)
        {
            exps = theExps;
            this.hasBrackets = hasBrackets;
            this.firstArgIsNull = firstArgIsNull;
        }

        readonly List<ExpressionToken> exps = null;
        readonly bool hasBrackets = false;
        readonly bool firstArgIsNull = false;
        internal List<ExpressionToken> Exps
        {
            get { return exps; }
        }
        internal override int TokenOffset
        {
            get
            {
                if ((exps == null) || (exps.Count == 0))
                    return -1;
                return exps[0].TokenOffset;
            }
        }

        public override string ToString()
        {
            return ToString(false);
        }

        public string ToString(bool mcall)
        {
            StringBuilder builder = new StringBuilder();
            if (hasBrackets)
                builder.Append('(');
            else
                builder.Append(' ');

            int i = 0;
            foreach (ExpressionToken exp in exps)
            {
                if ((i != 0) || (firstArgIsNull && !mcall))
                    builder.Append(", ");
                i++;
                builder.Append(exp.ToString());
            }
            if (hasBrackets)
                builder.Append(')');
            return builder.ToString();
        }

        internal override void CheckLabel()
        {
            if (exps != null)
                foreach (ExpressionToken token in exps)
                    token.CheckLabel();
        }

        internal override bool CheckRpn()
        {
            bool ret = true;
            if (exps != null)
                foreach (ExpressionToken token in exps)
                    ret &= token.CheckRpn();
            return ret;
        }
    }
}
