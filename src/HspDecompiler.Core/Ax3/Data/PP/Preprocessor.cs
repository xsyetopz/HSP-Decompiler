namespace HspDecompiler.Core.Ax3.Data.PP;

internal abstract class Preprocessor
{
    protected Preprocessor() { }

    protected Preprocessor(int index)
    {
        _index = index;
    }

    protected readonly int _index;
    public abstract override string ToString();
}
