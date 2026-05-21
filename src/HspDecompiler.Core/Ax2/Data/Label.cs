using System.Globalization;

namespace HspDecompiler.Core.Ax2.Data;

internal class Label
{
    private Label() { }

    internal Label(int p_index, int p_tokenIndex)
    {
        _index = p_index;
        _tokenIndex = p_tokenIndex;
        _name = "*label_" + _index.ToString(CultureInfo.InvariantCulture);
    }

    private readonly int _index;
    private readonly int _tokenIndex;
    private int _loadCount;
    private string _name = "";
    private bool _enabled;
    private int _deffunc = -1;

    internal string Name
    {
        get => _name;
        set => _name = value;
    }

    internal int TokenIndex => _tokenIndex;

    internal int Deffunc
    {
        get => _deffunc;
        set => _deffunc = value;
    }

    internal int LoadCount
    {
        get => _loadCount;
        set => _loadCount = value;
    }

    internal bool Enabled
    {
        get => _deffunc != -1 ? true : _enabled;
        set => _enabled = value;
    }
}
