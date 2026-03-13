using System;

namespace HspDecompiler.Core.Ax2.Data
{
    internal class Label
    {
        private Label()
        {
        }

        internal Label(int p_index, int p_tokenIndex)
        {
            index = p_index;
            tokenIndex = p_tokenIndex;
            name = "*label_" + index.ToString();
        }

        private int index;
        private int tokenIndex;
        private int loadCount = 0;
        private string name;
        private bool enabled;
        private int deffunc = -1;

        internal string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }

        internal int TokenIndex
        {
            get
            {
                return tokenIndex;
            }
        }

        internal int Deffunc
        {
            get
            {
                return deffunc;
            }
            set
            {
                deffunc = value;
            }
        }

        internal int LoadCount
        {
            get
            {
                return loadCount;
            }
            set
            {
                loadCount = value;
            }
        }

        internal bool Enabled
        {
            get
            {
                if (deffunc != -1)
                    return true;
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }
    }
}
