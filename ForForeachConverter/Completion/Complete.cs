using System.Text;
using ASCompletion.Completion;
using ASCompletion.Context;
using PluginCore.Managers;
using ScintillaNet;

namespace ForForeachConverter.Completion
{
    public static class Complete
    {
        public static int GetStartOfStatement(ScintillaControl sci, int startPosition)
        {
            var pos = startPosition;
            switch (sci.GetWordFromPosition(pos))
            {
                case "for":
                    pos = sci.WordEndPosition(pos, true);
                    if (sci.GetWordRight(pos + 1, true) == "each") return sci.WordStartPosition(startPosition, true);
                    break;
                case "each":
                    pos = sci.WordStartPosition(pos, true) - 1;
                    if (sci.GetWordLeft(pos, true) == "for") return sci.WordStartPosition(pos, true);
                    break;
            }
            return -1;
        }

        public static EForeach GetExpression(ScintillaControl sci, int position)
        {
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

        public struct EForeach
        {
            public int StartPosition;
            public int EndPosition;
            public ASResult Variable;
            public ASResult Collection;
            public string[] Statements;
        }
    }
}
