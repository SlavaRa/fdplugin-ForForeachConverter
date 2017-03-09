// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Windows.Forms;

namespace ForForeachConverter.Controls
{
    class RefactorMenu
    {
        public RefactorMenu()
        {
            ConvertForeachToFor = new ToolStripMenuItem("for");
            ConvertToForeach = new ToolStripMenuItem("foreach");
        }

        public ToolStripMenuItem ConvertForeachToFor { get; private set; }
        public ToolStripMenuItem ConvertToForeach { private set; get; }
    }
}
