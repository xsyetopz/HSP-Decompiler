using HspDecompiler.Core.Ax3.Data.Primitive;

namespace HspDecompiler.Core.Ax3.Data.Token
{
    internal sealed class OperatorToken : ExpressionTermToken
    {
        private OperatorToken() { }
        internal OperatorToken(OperatorPrimitive source)
        {
            primitive = source;
        }

        readonly OperatorPrimitive primitive = null;
        internal override int TokenOffset
        {
            get { return primitive.TokenOffset; }
        }

        public override string ToString()
        {
            return primitive.ToString();
        }

        internal string ToString(bool isAssignment, bool hasExpression)
        {
            string ret = primitive.ToString();
            if (primitive.CodeType != HspCodeType.Operator)
                return primitive.ToString();
            if (isAssignment)
            {
                if ((!hasExpression) && (ret == "+"))
                    return "++";
                else if ((!hasExpression) && (ret == "-"))
                    return "--";
                switch (ret)
                {
                    case "=":
                    case ">":
                    case "<":
                        return ret;
                    default:
                        return ret + "=";
                }
            }
            else
            {
                switch (ret)
                {
                    case "=":
                    case "!":
                        return ret + "=";
                }
            }
            return ret;
        }

        internal override bool IsOperand
        {
            get { return false; }
        }

        internal override bool IsOperator
        {
            get { return true; }
        }

        internal override int Priority
        {
            get { return primitive.OperatorPriority; }
        }
    }
}
