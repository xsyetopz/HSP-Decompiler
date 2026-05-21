using System.Text;
using HspDecompiler.Core.Ax3.Data.Token;

namespace HspDecompiler.Core.Ax3.Data.Line;

internal sealed class Assignment : LogicalLine
{
    private Assignment() { }

    internal Assignment(VariableToken theVar, OperatorToken theOp)
    {
        _var = theVar;
        _op = theOp;
    }

    internal Assignment(VariableToken theVar, OperatorToken theOp, ArgumentToken theArg)
    {
        _var = theVar;
        _op = theOp;
        _arg = theArg;
    }

    private readonly VariableToken? _var;
    private readonly OperatorToken? _op;
    private readonly ArgumentToken? _arg;

    internal override int TokenOffset => _var == null ? -1 : _var.TokenOffset;

    public override string ToString()
    {
        var builder = new StringBuilder(_var!.ToString());
        if (_arg != null)
        {
            builder.Append(' ');
            builder.Append(_op!.ToString(true, _arg != null));
            builder.Append(_arg!.ToString());
        }
        else
        {
            builder.Append(_op!.ToString(true, _arg != null));
        }
        return builder.ToString();
    }

    internal override void CheckLabel()
    {
        _var?.CheckLabel();

        _op?.CheckLabel();

        _arg?.CheckLabel();
    }

    internal override bool CheckRpn()
    {
        bool ret = true;
        if (_var != null)
        {
            ret &= _var.CheckRpn();
        }

        if (_arg != null)
        {
            ret &= _arg.CheckRpn();
        }

        return true;
    }
}
