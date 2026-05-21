using System;
using System.Collections.Generic;
using System.Text;

namespace HspDecompiler.Core.Ax3.Data.Token;

internal sealed class ExpressionToken : CodeToken
{
    private ExpressionToken() { }

    internal ExpressionToken(List<ExpressionTermToken> elements)
    {
        _tokens = elements;
    }

    private readonly List<ExpressionTermToken>? _tokens;
    private ExpressionTermToken? _convertedToken;
    private bool _tryConvert;
    internal bool CanRpnConvert =>
        _convertedToken != null ? true
        : !_tryConvert ? RpnConvert()
        : false;

    internal bool RpnConvert()
    {
        if (_convertedToken != null)
        {
            return true;
        }

        if (_tokens!.Count == 0)
        {
            return false;
        }

        _tryConvert = true;
        if (_tokens.Count == 1)
        {
            _convertedToken = _tokens[0];
            return true;
        }
        var stack = new List<ExpressionTermToken>();
        var source = new List<ExpressionTermToken>();
        try
        {
            source.AddRange(_tokens);
            while (source.Count != 0)
            {
                ExpressionTermToken token = source[0];
                source.RemoveAt(0);
                if (token.IsOperator)
                {
                    var right = (OperandToken)stack[^1];
                    stack.RemoveAt(stack.Count - 1);
                    var left = (OperandToken)stack[^1];
                    stack.RemoveAt(stack.Count - 1);
                    stack.Add(
                        (ExpressionTermToken)(
                            new SubExpressionToken(left, right, (OperatorToken)token)
                        )
                    );
                }
                else
                {
                    stack.Add(token);
                }
            }
        }
        catch (InvalidCastException)
        {
            // RPN token list has wrong structure (non-operand/operator in wrong position)
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            // Stack underflow — malformed RPN expression
            return false;
        }
        if (stack.Count != 1)
        {
            return false;
        }

        _convertedToken = stack[0];
        return true;
    }

    internal override int TokenOffset
    {
        get
        {
            if ((_tokens == null) || (_tokens.Count == 0))
            {
                return -1;
            }

            var token = _tokens[0] as CodeToken;
            return token == null ? -1 : token.TokenOffset;
        }
    }

    internal string ToString(bool getRpnConverted)
    {
        if (getRpnConverted && (_convertedToken != null))
        {
            return _convertedToken.ToString();
        }

        var builder = new StringBuilder();
        int i = 0;
        foreach (ExpressionTermToken token in _tokens!)
        {
            if (i != 0)
            {
                builder.Append(' ');
            }

            builder.Append(token.ToString());
            i++;
        }
        return builder.ToString();
    }

    public override string ToString() => ToString(true);

    internal override void CheckLabel()
    {
        foreach (CodeToken token in _tokens!)
        {
            token.CheckLabel();
        }
    }

    internal override bool CheckRpn() => CanRpnConvert;
}
