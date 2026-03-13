using System.Collections.Generic;
using System.Text;

namespace HspDecompiler.Core.Ax3.Data.Line
{
    internal sealed class UnknownLine : LogicalLine
    {
        private UnknownLine() { }
        internal UnknownLine(List<PrimitiveToken> primitives)
        {
            tokens = new PrimitiveToken[primitives.Count];
            primitives.CopyTo(tokens);
        }

        readonly PrimitiveToken[] tokens;

        internal override int TokenOffset
        {
            get
            {
                if ((tokens == null) || (tokens.Length == 0))
                    return -1;
                return tokens[0].TokenOffset;
            }
        }

        public override string ToString()
        {
            if ((tokens == null) || (tokens.Length == 0))
                return "//空";
            StringBuilder builder = new StringBuilder("//");
            foreach (PrimitiveToken token in tokens)
            {
                builder.Append(' ');
                builder.Append(token.ToString());
            }
            return builder.ToString();
        }
    }
}
