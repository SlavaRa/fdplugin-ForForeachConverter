using System.Drawing;
using System.Windows.Forms;
using PluginCore;
using PluginCore.Localization;

namespace ForForeachConverter.Controls
{
    class RefactorCompletionItem : ICompletionListItem
    {
        readonly ToolStripItem item;

        public RefactorCompletionItem(ToolStripItem item)
        {
            this.item = item;
            Label = TextHelper.RemoveMnemonicsAndEllipsis(item.Text);
            Description = TextHelper.GetStringWithoutMnemonics("Label.Refactor");
            Icon = (Bitmap) (item.Image ?? PluginBase.MainForm.FindImage("452")); //452 or 473
        }

        public string Description { get; }
        public Bitmap Icon { get; }
        public string Label { get; }

        public string Value
        {
            get
            {
                item.PerformClick();
                return null;
            }
        }
    }
}
