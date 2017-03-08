using System.Text;
using ASCompletion.Completion;
using ASCompletion.Context;
using ForForeachConverter.Helpers;
using PluginCore.Managers;
using ScintillaNet;
using ScintillaNet.Enums;

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

        public static int GetEndOfStatement(ScintillaControl sci, int startPosition)
        {
            var parCount = 0;
            var parseBody = false;
            var characterClass = ScintillaControl.Configuration.GetLanguage(sci.ConfigurationLanguage).characterclass.Characters;
            var word = string.Empty;
            var endPosition = sci.TextLength;
            var pos = startPosition;
            while (pos < endPosition)
            {
                if (!sci.PositionIsOnComment(pos))
                {
                    var c = (char)sci.CharAt(pos);
                    if (parseBody)
                    {
                        if (c == '{')
                        {
                            if (word == "catch")
                            {
                                pos = Reflector.ASGenerator.GetEndOfStatement(pos - 1, endPosition, sci);
                                return pos == endPosition ? pos : GetEndOfStatement(sci, pos);
                            }
                            return Reflector.ASGenerator.GetEndOfStatement(pos - 1, endPosition, sci);
                        }
                        if (c > ' ')
                        {
                            if (characterClass.IndexOf(c) != -1)
                            {
                                word = sci.GetWordRight(pos, false);
                                switch (word)
                                {
                                    case "if":
                                    case "for":
                                    case "while":
                                    case "switch":
                                        pos += word.Length;
                                        return GetEndOfStatement(sci, pos);
                                    case "do":
                                    case "try":
                                    case "catch":
                                    case "finally":
                                        pos += word.Length;
                                        pos = Reflector.ASGenerator.GetEndOfStatement(pos, endPosition, sci);
                                        return pos == endPosition ? pos : GetEndOfStatement(sci, pos);
                                    default:
                                        pos += word.Length;
                                        return Reflector.ASGenerator.GetEndOfStatement(pos, endPosition, sci);
                                }
                            }
                            if (c == ';') return pos + 1;
                        }
                    }
                    else
                    {
                        if (characterClass.IndexOf(c) != -1)
                        {
                            if (string.IsNullOrEmpty(word)) word = sci.GetWordRight(pos, false);
                            if (word == "finally")
                            {
                                pos += word.Length;
                                parseBody = true;
                            }
                        }
                        else if (c == '(') parCount++;
                        else if (c == ')')
                        {
                            parCount--;
                            if (parCount == 0) parseBody = true;
                        }
                    }
                }
                pos++;
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
