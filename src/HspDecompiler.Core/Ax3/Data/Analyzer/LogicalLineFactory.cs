using HspDecompiler.Core.Ax3.Data.Line;
using HspDecompiler.Core.Ax3.Data.Primitive;
using HspDecompiler.Core.Ax3.Data.Token;
using HspDecompiler.Core.Resources;

namespace HspDecompiler.Core.Ax3.Data.Analyzer;

internal partial class LogicalLineFactory
{
    internal static LogicalLine? GetCodeToken(TokenCollection? stream)
    {
        if (stream == null)
        {
            return null;
        }

        if (stream.Count == 0)
        {
            return null;
        }

        if (stream.NextIsEndOfStream)
        {
            return null;
        }

        LogicalLine? line = null;
        try
        {
            if (stream.NextToken is IfStatementPrimitive)
            {
                return (LogicalLine)readIf(stream);
            }

            if (stream.NextToken is McallFunctionPrimitive)
            {
                return (LogicalLine)readMcall(stream);
            }

            if (stream.NextToken is OnEventFunctionPrimitive)
            {
                return stream.NextNextTokenIsGotoFunction
                    ? (LogicalLine)readOnEvent(stream)
                    : (LogicalLine)readCommand(stream);
            }
            if (stream.NextToken is OnFunctionPrimitive)
            {
                return (LogicalLine)readOn(stream);
            }

            if (stream.NextToken is FunctionPrimitive)
            {
                return (LogicalLine)readCommand(stream);
            }

            if (stream.NextToken is VariablePrimitive)
            {
                return (LogicalLine)readAssignment(stream);
            }
        }
        catch (HspLogicalLineException e)
        {
            line = new UnknownLine(stream.Primitives);
            line.AddError(e.Message);
            return line;
        }
        line = new UnknownLine(stream.Primitives);
        line.AddError(Strings.CannotDetermineLeadToken);
        return line;
    }

    private static LogicalLine readMcall(TokenCollection stream)
    {
        int start = stream.Position;
        McallFunctionPrimitive? mcall =
            stream.GetNextToken() as McallFunctionPrimitive
            ?? throw new HspLogicalLineException(Strings.McallInvalidStart);
        if (stream.NextIsEndOfLine)
        {
            stream.Position = start;
            return (LogicalLine)readCommand(stream);
        }
        ExpressionToken exp = CodeTokenFactory.ReadExpression(stream);
        if (exp.CanRpnConvert)
        {
            stream.Position = start;
            return (LogicalLine)readCommand(stream);
        }

        stream.Position = start;
        stream.GetNextToken();
        VariablePrimitive? var =
            stream.GetNextToken() as VariablePrimitive
            ?? throw new HspLogicalLineException(Strings.McallInconvertibleFormat);
        if (stream.NextIsBracketStart)
        {
            throw new HspLogicalLineException(Strings.McallInconvertibleFormat);
        }

        if (stream.NextIsEndOfLine)
        {
            throw new HspLogicalLineException(Strings.McallInconvertibleFormat);
        }

        exp = CodeTokenFactory.ReadExpression(stream);
        if (stream.NextIsEndOfLine)
        {
            return new McallStatement(mcall, var, exp, null);
        }

        ArgumentToken arg = CodeTokenFactory.ReadArgument(stream);
        return stream.NextIsEndOfLine
            ? (LogicalLine)new McallStatement(mcall, var, exp, arg)
            : throw new HspLogicalLineException(Strings.McallUnexpectedToken);
    }

    private static OnStatement readOn(TokenCollection stream)
    {
        OnFunctionPrimitive? token =
            stream.GetNextToken() as OnFunctionPrimitive
            ?? throw new HspLogicalLineException(Strings.OnBranchInvalidStart);
        if (stream.NextIsEndOfLine)
        {
            return new OnStatement(token, null, null);
        }

        ExpressionToken exp = CodeTokenFactory.ReadExpression(stream);
        if (stream.NextIsEndOfLine)
        {
            return new OnStatement(token, exp, null);
        }

        FunctionToken func = CodeTokenFactory.ReadFunction(stream, false);
        return stream.NextIsEndOfLine
            ? new OnStatement(token, exp, func)
            : throw new HspLogicalLineException(Strings.OnBranchUnexpectedToken);
    }

    private static OnEventStatement readOnEvent(TokenCollection stream)
    {
        OnEventFunctionPrimitive? token =
            stream.GetNextToken() as OnEventFunctionPrimitive
            ?? throw new HspLogicalLineException(Strings.OnBranchInvalidStart);
        if (stream.NextIsEndOfLine)
        {
            return new OnEventStatement(token, null);
        }

        FunctionToken func = CodeTokenFactory.ReadFunction(stream, false);
        return stream.NextIsEndOfLine
            ? new OnEventStatement(token, func)
            : throw new HspLogicalLineException(Strings.OnBranchUnexpectedToken);
    }

    private static Assignment readAssignment(TokenCollection stream)
    {
        VariableToken token = CodeTokenFactory.ReadVariable(stream);
        if (stream.NextIsEndOfLine)
        {
            throw new HspLogicalLineException(Strings.AssignmentNoOperator);
        }

        OperatorToken op = CodeTokenFactory.ReadOperator(stream);
        if (stream.NextIsEndOfLine)
        {
            return new Assignment(token, op);
        }
        else
        {
            ArgumentToken arg = CodeTokenFactory.ReadArgument(stream);
            if (stream.NextIsEndOfLine)
            {
                return new Assignment(token, op, arg);
            }
        }
        throw new HspLogicalLineException(Strings.AssignmentUnexpectedToken);
    }

    private static IfStatement readIf(TokenCollection stream)
    {
        IfStatementPrimitive? token =
            stream.GetNextToken() as IfStatementPrimitive
            ?? throw new HspLogicalLineException(Strings.IfBranchInvalidStart);
        if (stream.NextIsEndOfLine)
        {
            return new IfStatement(token);
        }
        else
        {
            ArgumentToken arg = CodeTokenFactory.ReadArgument(stream);
            if (stream.NextIsEndOfLine)
            {
                return new IfStatement(token, arg);
            }
        }
        throw new HspLogicalLineException(Strings.IfBranchUnexpectedToken);
    }

    private static Command readCommand(TokenCollection stream)
    {
        FunctionToken func = CodeTokenFactory.ReadFunction(stream, false);
        return stream.NextIsEndOfLine
            ? new Command(func)
            : throw new HspLogicalLineException(Strings.CommandUnexpectedToken);
    }
}
