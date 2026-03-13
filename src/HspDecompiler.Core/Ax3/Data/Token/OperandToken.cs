namespace HspDecompiler.Core.Ax3.Data.Token
{
    abstract class OperandToken : ExpressionTermToken
    {
        internal OperandToken() { }

        internal override bool IsOperand
        {
            get { return true; }
        }

        internal override bool IsOperator
        {
            get { return false; }
        }
    }
}
