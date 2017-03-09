// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Collections.Generic;
using CodeRefactor.Commands;
using ForForeachConverter.Completion;
using PluginCore;
using PluginCore.FRService;
using ScintillaNet;

namespace ForForeachConverter.Commands.AS3
{
    class ConvertForeachToForCommand : RefactorCommand<IDictionary<string, List<SearchMatch>>>
    {
        public static bool IsValidForConvert(ScintillaControl sci)
        {
            var pos = sci.CurrentPos;
            var word = sci.GetWordFromPosition(pos);
            if (word == "each") return true;
            if (word != "for") return false;
            pos = sci.WordEndPosition(pos, true) + 1;
            word = sci.GetWordFromPosition(pos);
            return word == "each";
        }

        public override bool IsValid() => true;

        protected override void ExecutionImplementation()
        {
            var sci = PluginBase.MainForm.CurrentDocument.SciControl;
            var expr = Complete.GetExpression(sci, sci.CurrentPos);
            sci.SetSel(expr.StartPosition, expr.StartPosition);
        }
    }
}
