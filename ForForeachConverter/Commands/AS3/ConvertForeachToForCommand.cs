using System.Collections.Generic;
using System.Text;
using ASCompletion.Completion;
using ASCompletion.Context;
using CodeRefactor.Commands;
using PluginCore;
using PluginCore.FRService;
using PluginCore.Managers;
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

        internal static int GetStartOfStatement(ScintillaControl sci, int position)
        {
            var word = sci.GetWordFromPosition(position);
            if (word == "each") position = sci.WordStartPosition(position, true);
            word = sci.GetWordFromPosition(position);
            if (word == "for") position = sci.WordStartPosition(position - 1, true);
            return position;
        }

        public override bool IsValid() => true;

        protected override void ExecutionImplementation()
        {
            var sci = PluginBase.MainForm.CurrentDocument.SciControl;
            var expr = GetExpression(sci, sci.CurrentPos);
            sci.SetSel(expr.StartPosition, expr.StartPosition);
        }

        public EForeach GetExpression(ScintillaControl sci, int position)
        {
            TraceManager.Add("GetExpression");
            var result = new EForeach {StartPosition = GetStartOfStatement(sci, position)};
            var owner = ASContext.Context.GetDeclarationAtLine(sci.CurrentLine);
            var endOfOwner = sci.PositionFromLine(owner.Member.LineTo);
            var characterClass = ScintillaControl.Configuration.GetLanguage(sci.ConfigurationLanguage).characterclass.Characters;
            var parBraces = 0;
            var sb = new StringBuilder();
            for (var i = result.StartPosition; i < endOfOwner; i++)
            {
                var c = (char)sci.CharAt(i);
                if (c == '(') parBraces++;
                else if (c == ')')
                {
                    parBraces--;
                    if (parBraces == 0)
                    {
                        TraceManager.Add(sb.ToString());
                        break;
                    }
                }
                else if (parBraces > 0) sb.Append(c);
            }
            return result;
        }
    }

    struct EForeach
    {
        public int StartPosition;
        public int EndPosition;
        public ASResult Variable;
        public ASResult Collection;
        public string Statement;
    }
}
