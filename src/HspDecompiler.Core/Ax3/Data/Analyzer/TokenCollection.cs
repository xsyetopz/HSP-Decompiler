using System;
using System.Collections.Generic;

namespace HspDecompiler.Core.Ax3.Data.Analyzer;

internal sealed class TokenCollection
{
    private List<PrimitiveToken> _primitives = new();

    internal List<PrimitiveToken> Primitives => _primitives;

    private int _position;
    internal int Position
    {
        get => _position;
        set
        {
            if (_position < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            if (_position > _primitives.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            _position = value;
        }
    }

    internal PrimitiveToken this[int i] => _primitives[i];

    internal int Count => _primitives.Count;

    internal PrimitiveToken? NextToken => NextIsEndOfStream ? null : _primitives[_position];
    internal bool NextNextTokenIsGotoFunction
    {
        get
        {
            if (NextIsEndOfStream)
            {
                return false;
            }

            if ((_position + 1) >= _primitives.Count)
            {
                return false;
            }

            PrimitiveToken token = _primitives[_position + 1];
            return (
                (token.CodeExtraFlags & HspCodeExtraOptions.GotoFunction)
                == HspCodeExtraOptions.GotoFunction
            );
        }
    }

    internal TokenCollection? GetLine()
    {
        if (NextIsEndOfStream)
        {
            return null;
        }

        var list = new List<PrimitiveToken> { _primitives[_position] };
        _position++;
        while (_position < _primitives.Count)
        {
            if (_primitives[_position].IsLineHead)
            {
                break;
            }

            list.Add(_primitives[_position]);
            _position++;
        }
        var ret = new TokenCollection { _primitives = list };
        return ret;
    }

    internal PrimitiveToken? GetNextToken()
    {
        if (_position >= _primitives.Count)
        {
            return null;
        }

        PrimitiveToken ret = _primitives[_position];
        _position++;
        return ret;
    }

    internal bool NextIsEndOfStream => (_position >= _primitives.Count);

    internal bool NextIsEndOfLine => NextIsEndOfStream ? true : _primitives[_position].IsLineHead;

    internal bool NextIsEndOfParam =>
        NextIsEndOfStream ? true
        : _primitives[_position].IsLineHead ? true
        : _primitives[_position].IsParamHead;

    internal bool NextIsBracketStart
    {
        get
        {
            if (NextIsEndOfStream)
            {
                return false;
            }

            PrimitiveToken token = _primitives[_position];
            return (token.CodeExtraFlags & HspCodeExtraOptions.BracketStart)
                == HspCodeExtraOptions.BracketStart;
        }
    }

    internal bool NextIsBracketEnd
    {
        get
        {
            if (NextIsEndOfStream)
            {
                return false;
            }

            PrimitiveToken token = _primitives[_position];
            return (token.CodeExtraFlags & HspCodeExtraOptions.BracketEnd)
                == HspCodeExtraOptions.BracketEnd;
        }
    }

    internal void Add(PrimitiveToken token) => _primitives.Add(token);
}
