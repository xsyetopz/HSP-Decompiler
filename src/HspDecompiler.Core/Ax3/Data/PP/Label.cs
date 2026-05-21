using System.IO;

namespace HspDecompiler.Core.Ax3.Data.PP;

internal class Label : Preprocessor, System.IComparable<Label>
{
    private Label() { }

    private Label(int index)
        : base(index) { }

    private int _tokenOffset = -1;

    internal int TokenOffset => _tokenOffset;

    internal static Label FromBinaryReader(BinaryReader reader, AxData _, int index)
    {
        var ret = new Label(index) { _tokenOffset = reader.ReadInt32() };
        return ret;
    }

    private bool _visible;
    internal bool Visible
    {
        get => _function != null ? true : _visible;
        set => _visible = value;
    }
    private string _labelName = "*label";
    internal string LabelName
    {
        get => _labelName;
        set => _labelName = value;
    }

    public override string ToString() => _function != null ? _function.ToString() : _labelName;

    public int CompareTo(Label? other)
    {
        if (other == null)
        {
            return 1;
        }

        int ret = _tokenOffset.CompareTo(other._tokenOffset);
        return ret != 0 ? ret : _index.CompareTo(other._index);
    }

    private Function? _function;

    internal void SetFunction(Function f)
    {
        _function = f;
        _visible = true;
    }
}
