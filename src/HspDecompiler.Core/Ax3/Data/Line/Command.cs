using HspDecompiler.Core.Ax3.Data.Token;

namespace HspDecompiler.Core.Ax3.Data.Line;

internal sealed class Command : LogicalLine
{
    private Command() { }

    internal Command(FunctionToken function)
    {
        _function = function;
    }

    private readonly FunctionToken? _function;

    internal override bool TabIncrement =>
        (_function!.Primitive.CodeExtraFlags & HspCodeExtraOptions.AddTab)
        == HspCodeExtraOptions.AddTab;
    internal override bool TabDecrement =>
        (_function!.Primitive.CodeExtraFlags & HspCodeExtraOptions.RemoveTab)
        == HspCodeExtraOptions.RemoveTab;
    internal override bool HasFlagGhostGoto =>
        (_function!.Primitive.CodeExtraFlags & HspCodeExtraOptions.HasGhostGoto)
        == HspCodeExtraOptions.HasGhostGoto;
    internal override bool HasFlagIsGhost =>
        (_function!.Primitive.CodeExtraFlags & HspCodeExtraOptions.IsGhost)
        == HspCodeExtraOptions.IsGhost;

    internal override int TokenOffset => _function!.TokenOffset;

    public override string ToString() => _function!.ToString();

    internal override void CheckLabel() => _function?.CheckLabel();

    internal override bool CheckRpn() => _function != null ? _function.CheckRpn() : true;
}
