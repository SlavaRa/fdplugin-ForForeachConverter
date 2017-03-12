// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Windows.Forms;

namespace ForForeachConverter.Controls
{
    class RefactorMenu
    {
        public RefactorMenu()
        {
            ConvertForeachToFor = new ToolStripMenuItem("To for");
            ConvertForeachToForin = new ToolStripMenuItem("To forin");
            ConvertToForeach = new ToolStripMenuItem("To foreach");
        }

        public ToolStripMenuItem ConvertForeachToFor { get; private set; }
        public ToolStripMenuItem ConvertForeachToForin { get; private set; }
        public ToolStripMenuItem ConvertToForeach { private set; get; }
    }
}
