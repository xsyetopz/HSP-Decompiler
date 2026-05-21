using System.Collections.Generic;

namespace HspDecompiler.Core.Ax3.Data;

internal abstract class LogicalLine
{
    internal abstract int TokenOffset { get; }

    protected int _tabCount;

    internal virtual int TabCount
    {
        get => _tabCount;
        set => _tabCount = value;
    }

    protected List<string> _errorMes = new();

    internal List<string> GetErrorMes() => _errorMes;

    internal void AddError(string error) => _errorMes.Add(error);

    public abstract override string ToString();

    private bool _visible = true;
    internal bool Visible
    {
        get => _visible;
        set => _visible = value;
    }

    internal virtual bool TabIncrement => false;
    internal virtual bool TabDecrement => false;
    internal virtual bool HasFlagGhostGoto => false;
    internal virtual bool HasFlagIsGhost => false;

    internal virtual void CheckLabel() { }

    internal virtual bool CheckRpn() => true;
}
