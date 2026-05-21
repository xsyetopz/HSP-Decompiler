namespace HspDecompiler.Core.Ax3.Data.Line;

internal class EndOfModule : LogicalLine
{
    internal override int TokenOffset => -1;
    internal override int TabCount => 0;

    public override string ToString() => "#global";
}
