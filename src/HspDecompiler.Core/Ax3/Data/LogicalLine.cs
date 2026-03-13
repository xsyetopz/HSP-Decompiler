using System;
using System.Collections.Generic;

namespace HspDecompiler.Core.Ax3.Data
{
    abstract class LogicalLine
    {
        internal abstract int TokenOffset
        {
            get;
        }

        protected int tabCount = 0;

        internal virtual int TabCount
        {
            get { return tabCount; }
            set { tabCount = value; }
        }

        protected List<string> errorMes = new List<string>();
        internal List<string> GetErrorMes() { return errorMes; }
        internal void AddError(string error) { errorMes.Add(error); }
        public override abstract string ToString();

        private bool visible = true;
        internal bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                visible = value;
            }
        }

        internal virtual bool TabIncrement { get { return false; } }
        internal virtual bool TabDecrement { get { return false; } }
        internal virtual bool HasFlagGhostGoto { get { return false; } }
        internal virtual bool HasFlagIsGhost { get { return false; } }

        internal virtual void CheckLabel() { }
        internal virtual bool CheckRpn() { return true; }
    }
}
