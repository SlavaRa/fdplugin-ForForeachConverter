using System.Windows.Forms;

namespace ForForeachConverter.Controls
{
    class RefactorMenu
    {
        public RefactorMenu()
        {
            ConvertToFor = new ToolStripMenuItem("for");
            ConvertToForeach = new ToolStripMenuItem("foreach");
        }

        public ToolStripMenuItem ConvertToFor { get; private set; }
        public ToolStripMenuItem ConvertToForeach { private set; get; }
    }
}
