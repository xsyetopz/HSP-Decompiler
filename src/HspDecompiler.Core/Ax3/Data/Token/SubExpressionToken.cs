using System.Text;

namespace HspDecompiler.Core.Ax3.Data.Token
{
    internal sealed class SubExpressionToken : OperandToken
    {
        private SubExpressionToken() { }
        internal SubExpressionToken(OperandToken leftToken, OperandToken rightToken, OperatorToken opToken)
        {
            p1 = leftToken;
            p2 = rightToken;
            op = opToken;
        }

        private readonly OperandToken p1;
        private readonly OperandToken p2;
        private readonly OperatorToken op;

        internal override int TokenOffset
        {
            get { return p1.TokenOffset; }
        }

        private string ToStringForceDefault()
        {
            StringBuilder builder = new StringBuilder();
            if (p1.Priority < op.Priority)
            {
                builder.Append('(');
                builder.Append(p1.ToString());
                builder.Append(')');
            }
            else
            {
                builder.Append(p1.ToString());
            }
            builder.Append(' ');
            builder.Append(op.ToString(false, true));
            builder.Append(' ');
            if (p2.Priority <= op.Priority)
            {
                builder.Append('(');
                builder.Append(p2.ToString());
                builder.Append(')');
            }
            else
            {
                builder.Append(p2.ToString());
            }
            return builder.ToString();
        }

        internal string ToString(bool force_default)
        {
            if (force_default)
                return ToStringForceDefault();

            LiteralToken lit = p1 as LiteralToken;
            VariableToken var = p2 as VariableToken;
            if ((lit == null) || (var == null))
            {
                lit = p2 as LiteralToken;
                var = p1 as VariableToken;
            }
            if ((lit == null) || (var == null))
                return ToStringForceDefault();
            if (!lit.IsMinusOne)
                return ToStringForceDefault();
            if (op.ToString() != "*")
                return ToStringForceDefault();

            StringBuilder builder = new StringBuilder();
            builder.Append('-');
            builder.Append(var.ToString());
            return builder.ToString();
        }

        public override string ToString()
        {
            return ToString(false);
        }

        internal override int Priority
        {
            get { return op.Priority; }
        }

        internal override void CheckLabel()
        {
            if (p1 != null)
                p1.CheckLabel();
            if (p2 != null)
                p2.CheckLabel();
            if (op != null)
                op.CheckLabel();
        }
    }
}
