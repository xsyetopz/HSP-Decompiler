namespace HspDecompiler.Core.Ax3.Data;

internal abstract class CodeToken
{
    internal abstract int TokenOffset { get; }

    public abstract override string ToString();

    internal virtual void CheckLabel() { }

    internal virtual bool CheckRpn() => true;
}
