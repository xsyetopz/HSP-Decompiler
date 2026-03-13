using System;

namespace HspDecompiler.Core.Ax2.Data
{
    internal struct Module
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
            return "#module " + name;
        }
    }
}
