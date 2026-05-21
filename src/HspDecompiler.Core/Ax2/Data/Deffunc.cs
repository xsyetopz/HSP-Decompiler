namespace HspDecompiler.Core.Ax2.Data;

internal struct Deffunc
{
    private string _name;
    private int _hikiType;
    private int _hikiCount;

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

    internal int HikiCount
    {
        readonly get => _hikiCount;
        set => _hikiCount = value;
    }

    public override readonly string ToString()
    {
        string hiki = "";
        if (_hikiCount >= 1)
        {
            hiki =
                (_hikiType & 1) != 0 ? "val"
                : (_hikiType & 2) != 0 ? "str"
                : "int";
        }
        if (_hikiCount >= 2)
        {
            if ((_hikiType & 0x10) != 0)
            {
                hiki += ", val";
            }
            else if ((_hikiType & 0x20) != 0)
            {
                hiki += ", str";
            }
            else
            {
                hiki += ", int";
            }
        }
        for (int i = 0; i < (_hikiCount - 2); i++)
        {
            hiki += ", int";
        }

        return "#deffunc " + _name + " " + hiki;
    }
}
