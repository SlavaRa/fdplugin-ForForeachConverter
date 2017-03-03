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
