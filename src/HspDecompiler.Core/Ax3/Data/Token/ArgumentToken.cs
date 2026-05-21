using System.Collections.Generic;
using System.Text;

namespace HspDecompiler.Core.Ax3.Data.Token;

internal sealed class ArgumentToken : CodeToken
{
    private ArgumentToken() { }

    internal ArgumentToken(List<ExpressionToken> theExps, bool hasBrackets, bool firstArgIsNull)
    {
        _exps = theExps;
        _hasBrackets = hasBrackets;
        _firstArgIsNull = firstArgIsNull;
    }

    private readonly List<ExpressionToken>? _exps;
    private readonly bool _hasBrackets;
    private readonly bool _firstArgIsNull;
    internal List<ExpressionToken> Exps => _exps!;
    internal override int TokenOffset =>
        (_exps == null) || (_exps.Count == 0) ? -1 : _exps[0].TokenOffset;

    public override string ToString() => ToString(false);

    public string ToString(bool mcall)
    {
        var builder = new StringBuilder();
        if (_hasBrackets)
        {
            builder.Append('(');
        }
        else
        {
            builder.Append(' ');
        }

        int i = 0;
        foreach (ExpressionToken exp in _exps!)
        {
            if ((i != 0) || (_firstArgIsNull && !mcall))
            {
                builder.Append(", ");
            }

            i++;
            builder.Append(exp.ToString());
        }
        if (_hasBrackets)
        {
            builder.Append(')');
        }

        return builder.ToString();
    }

    internal override void CheckLabel()
    {
        if (_exps != null)
        {
            foreach (ExpressionToken token in _exps)
            {
                token.CheckLabel();
            }
        }
    }

    internal override bool CheckRpn()
    {
        bool ret = true;
        if (_exps != null)
        {
            foreach (ExpressionToken token in _exps)
            {
                ret &= token.CheckRpn();
            }
        }

        return ret;
    }
}
