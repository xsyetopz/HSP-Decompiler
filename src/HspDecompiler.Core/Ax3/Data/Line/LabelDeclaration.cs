using HspDecompiler.Core.Ax3.Data.PP;

namespace HspDecompiler.Core.Ax3.Data.Line;

internal sealed class PreprocessorDeclaration : LogicalLine
{
    private PreprocessorDeclaration() { }

    internal PreprocessorDeclaration(Preprocessor pp)
    {
        _pp = pp;
    }

    private readonly Preprocessor? _pp;

    internal override int TabCount => 0;

    internal override int TokenOffset => -1;

    public override string ToString() => _pp!.ToString();
}
