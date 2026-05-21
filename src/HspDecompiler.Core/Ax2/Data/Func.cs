using System.Globalization;

namespace HspDecompiler.Core.Ax2.Data;

internal struct Func
{
    private string _name;
    private int _hikiType;
    private int _dllIndex;

    internal string Name
    {
        readonly get => _name;
        set => _name = value;
    }

    internal int HikiType
    {
        readonly get => _hikiType;
        set => _hikiType = value;
    }

    internal int DllIndex
    {
        readonly get => _dllIndex;
        set => _dllIndex = value;
    }

    public override readonly string ToString() =>
        "#func func_"
        + _name
        + " "
        + _name
        + " $"
        + _hikiType.ToString("x4", CultureInfo.InvariantCulture);
}
