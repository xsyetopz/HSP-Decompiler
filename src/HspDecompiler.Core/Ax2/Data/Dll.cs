using System;

namespace HspDecompiler.Core.Ax2.Data
{
    internal struct Dll
    {
        private string name;

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

        public override string ToString()
        {
            return "#uselib " + "\"" + name + "\"";
        }
    }
}
