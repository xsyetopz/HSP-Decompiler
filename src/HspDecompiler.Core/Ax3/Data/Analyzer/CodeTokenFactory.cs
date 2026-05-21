using HspDecompiler.Core.Ax3.Data.Primitive;
using HspDecompiler.Core.Ax3.Data.Token;
using HspDecompiler.Core.Resources;

namespace HspDecompiler.Core.Ax3.Data.Analyzer;

internal partial class LogicalLineFactory
{
    private static class CodeTokenFactory
    {
        internal static ExpressionToken ReadExpression(TokenCollection stream)
        {
            if (stream.NextIsEndOfLine)
            {
                throw new HspLogicalLineException(Strings.ExpressionStackEmpty);
            }

            ExpressionTermToken? elem = null;
            var elements = new System.Collections.Generic.List<ExpressionTermToken>();
            do
            {
                if (stream.NextIsBracketEnd)
                {
                    break;
                }

                elem =
                    stream.NextToken is OperatorPrimitive
                        ? (ExpressionTermToken)(ReadOperator(stream))
                    : stream.NextToken is LiteralPrimitive
                        ? (ExpressionTermToken)(ReadLiteral(stream))
                    : stream.NextToken is FunctionPrimitive
                        ? (ExpressionTermToken)(ReadFunction(stream, true))
                    : stream.NextToken is VariablePrimitive
                        ? (ExpressionTermToken)(ReadVariable(stream))
                    : throw new HspLogicalLineException(Strings.ExpressionInvalidElement);

                elements.Add(elem);
            } while (!stream.NextIsEndOfParam);
            var ret = new ExpressionToken(elements);
            ret.RpnConvert();
            return ret;
        }

        private static LiteralToken ReadLiteral(TokenCollection stream)
        {
            if (stream.NextIsEndOfLine)
            {
                throw new HspLogicalLineException(Strings.LiteralExpressionStackEmpty);
            }

            LiteralPrimitive? token =
                stream.GetNextToken() as LiteralPrimitive
                ?? throw new HspLogicalLineException(Strings.LiteralExpressionInvalidElement);
            return new LiteralToken(token);
        }

        internal static ArgumentToken ReadArgument(TokenCollection stream)
        {
            if (stream.NextIsEndOfLine)
            {
                throw new HspLogicalLineException(Strings.ArgumentStackEmpty);
            }

            bool hasBracket = stream.NextIsBracketStart;
            if (hasBracket)
            {
                stream.GetNextToken();
            }

            var exps = new System.Collections.Generic.List<ExpressionToken>();
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
            {
                throw new HspLogicalLineException(Strings.FunctionStackEmpty);
            }

            FunctionPrimitive? token =
                stream.GetNextToken() as FunctionPrimitive
                ?? throw new HspLogicalLineException(Strings.FunctionInvalidStart);
            if (stream.NextIsEndOfLine)
            {
                return new FunctionToken(token);
            }

            if (token.HasGhostLabel && (stream.NextToken!.CodeType == HspCodeType.Label))
            {
                stream.GetNextToken();
                if (stream.NextIsEndOfLine)
                {
                    return new FunctionToken(token);
                }
            }

            return stream.NextIsBracketStart ? new FunctionToken(token, ReadArgument(stream))
                : hasBracket ? new FunctionToken(token)
                : new FunctionToken(token, ReadArgument(stream));
        }

        internal static VariableToken ReadVariable(TokenCollection stream)
        {
            if (stream.NextIsEndOfStream)
            {
                throw new HspLogicalLineException(Strings.VariableStackEmpty);
            }

            VariablePrimitive? token =
                stream.GetNextToken() as VariablePrimitive
                ?? throw new HspLogicalLineException(Strings.VariableInvalidStart);
            return stream.NextIsBracketStart
                ? new VariableToken(token, ReadArgument(stream))
                : new VariableToken(token);
        }

        internal static OperatorToken ReadOperator(TokenCollection stream)
        {
            if (stream.NextIsEndOfLine)
            {
                throw new HspLogicalLineException(Strings.OperatorStackEmpty);
            }

            OperatorPrimitive? token =
                stream.GetNextToken() as OperatorPrimitive
                ?? throw new HspLogicalLineException(Strings.OperatorInvalidElement);
            return new OperatorToken(token);
        }
    }
}
