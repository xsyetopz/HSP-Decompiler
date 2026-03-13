using HspDecompiler.Core.Ax3.Data.Primitive;
using HspDecompiler.Core.Resources;
using HspDecompiler.Core.Ax3.Data.Token;

namespace HspDecompiler.Core.Ax3.Data.Analyzer
{
partial class LogicalLineFactory
{
    private static class CodeTokenFactory
    {
        internal static ExpressionToken ReadExpression(TokenCollection stream)
        {
            if (stream.NextIsEndOfLine)
                throw new HspLogicalLineException(Strings.ExpressionStackEmpty);
            ExpressionTermToken elem = null;
            System.Collections.Generic.List<ExpressionTermToken> elements = new System.Collections.Generic.List<ExpressionTermToken>();
            do
            {
                if (stream.NextIsBracketEnd)
                    break;
                if (stream.NextToken is OperatorPrimitive)
                    elem = (ExpressionTermToken)(ReadOperator(stream));
                else if (stream.NextToken is LiteralPrimitive)
                    elem = (ExpressionTermToken)(ReadLiteral(stream));
                else if (stream.NextToken is FunctionPrimitive)
                    elem = (ExpressionTermToken)(ReadFunction(stream, true));
                else if (stream.NextToken is VariablePrimitive)
                    elem = (ExpressionTermToken)(ReadVariable(stream));
                else
                    throw new HspLogicalLineException(Strings.ExpressionInvalidElement);
                elements.Add(elem);
            } while (!stream.NextIsEndOfParam);
            ExpressionToken ret = new ExpressionToken(elements);
            ret.RpnConvert();
            return ret;
        }

        private static object ReadLiteral(TokenCollection stream)
        {
            if (stream.NextIsEndOfLine)
                throw new HspLogicalLineException(Strings.LiteralExpressionStackEmpty);
            LiteralPrimitive token = stream.GetNextToken() as LiteralPrimitive;
            if (token == null)
                throw new HspLogicalLineException(Strings.LiteralExpressionInvalidElement);
            return new LiteralToken(token);
        }

        internal static ArgumentToken ReadArgument(TokenCollection stream)
        {
            if (stream.NextIsEndOfLine)
                throw new HspLogicalLineException(Strings.ArgumentStackEmpty);
            bool hasBracket = stream.NextIsBracketStart;
            if (hasBracket)
                stream.GetNextToken();
            System.Collections.Generic.List<ExpressionToken> exps = new System.Collections.Generic.List<ExpressionToken>();
            bool firstArgIsNull = stream.NextIsEndOfParam;
            while (!stream.NextIsEndOfLine)
            {
                if (hasBracket & stream.NextIsBracketEnd)
                {
                    stream.GetNextToken();
                    break;
                }
                exps.Add(ReadExpression(stream));
            }
            return new ArgumentToken(exps, hasBracket, firstArgIsNull);
        }

        internal static FunctionToken ReadFunction(TokenCollection stream, bool hasBracket)
        {
            if (stream.NextIsEndOfStream)
                throw new HspLogicalLineException(Strings.FunctionStackEmpty);
            FunctionPrimitive token = stream.GetNextToken() as FunctionPrimitive;
            if (token == null)
                throw new HspLogicalLineException(Strings.FunctionInvalidStart);
            if (stream.NextIsEndOfLine)
                return new FunctionToken(token);
            if (token.HasGhostLabel && (stream.NextToken.CodeType == HspCodeType.Label))
            {
                stream.GetNextToken();
                if (stream.NextIsEndOfLine)
                    return new FunctionToken(token);
            }

            if (stream.NextIsBracketStart)
                return new FunctionToken(token, ReadArgument(stream));
            if (hasBracket)
                return new FunctionToken(token);
            else
                return new FunctionToken(token, ReadArgument(stream));
        }

        internal static VariableToken ReadVariable(TokenCollection stream)
        {
            if (stream.NextIsEndOfStream)
                throw new HspLogicalLineException(Strings.VariableStackEmpty);
            VariablePrimitive token = stream.GetNextToken() as VariablePrimitive;
            if (token == null)
                throw new HspLogicalLineException(Strings.VariableInvalidStart);

            if (stream.NextIsBracketStart)
            {
                return new VariableToken(token, ReadArgument(stream));
            }
            return new VariableToken(token);
        }

        internal static OperatorToken ReadOperator(TokenCollection stream)
        {
            if (stream.NextIsEndOfLine)
                throw new HspLogicalLineException(Strings.OperatorStackEmpty);
            OperatorPrimitive token = stream.GetNextToken() as OperatorPrimitive;
            if (token == null)
                throw new HspLogicalLineException(Strings.OperatorInvalidElement);
            return new OperatorToken(token);
        }
    }
}

}
