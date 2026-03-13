using System.Text;

namespace HspDecompiler.Core.Ax3.Data.PP
{
    class Runtime : Preprocessor
    {
        private Runtime() { }
        internal Runtime(string theName)
        {
            name = theName;
        }

        string name;

        public override string ToString()
        {
            StringBuilder strbd = new StringBuilder();
            strbd.Append("#runtime ");
            strbd.Append(@"""");
            strbd.Append(name);
            strbd.Append(@"""");
            return strbd.ToString();
        }
    }
}
