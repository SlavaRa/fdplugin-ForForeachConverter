// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Text;
using ASCompletion.Completion;
using ASCompletion.Context;
using ForForeachConverter.Helpers;
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

        public static int GetStartOfIFStatement(ScintillaControl sci, int startPosition)
        {
            var characterClass = ScintillaControl.Configuration.GetLanguage(sci.ConfigurationLanguage).characterclass.Characters;
            var endPosition = sci.TextLength;
            var pos = GetStartOfBody(sci, startPosition);
            while (pos < endPosition)
            {
                if (!sci.PositionIsOnComment(pos))
                {
                    var c = (char) sci.CharAt(pos);
                    if (c > ' ' && (characterClass.IndexOf(c) != -1 || c == '{'))
                    {
                        if (c != '{')
                        {
                            var word = sci.GetWordRight(pos, true);
                            switch (word)
                            {
                                case "for":
                                case "while":
                                case "do":
                                case "switch":
                                case "try":
                                    return GetEndOfStatement(sci, pos + word.Length);
                            }
                        }
                        pos = Reflector.ASGenerator.GetEndOfStatement(pos - 1, endPosition, sci);
                        if (pos == endPosition) return pos;
                        var p = pos;
                        while (p < endPosition)
                        {
                            if (!sci.PositionIsOnComment(p) && characterClass.IndexOf((char) sci.CharAt(p)) != -1)
                            {
                                var word = sci.GetWordRight(p, false);
                                if (word == "else")
                                {
                                    pos = p + word.Length;
                                    word = sci.GetWordRight(pos, true);
                                    if (word == "if")
                                    {
                                        pos = sci.WordStartPosition(pos + word.Length, false);
                                        return GetStartOfIFStatement(sci, pos);
                                    }
                                    break;
                                }
                                return pos;
                            }
                            p++;
                        }
                    }
                }
                pos++;
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
                                        return GetStartOfIFStatement(sci, pos + word.Length);
                                    case "for":
                                    case "while":
                                    case "switch":
                                        pos += word.Length;
                                        return GetEndOfStatement(sci, pos);
                                    case "do":
                                    case "try":
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
                    else if (characterClass.IndexOf(c) != -1)
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
                pos++;
            }
            return -1;
        }

        static int GetStartOfBody(ScintillaControl sci, int startPosition)
        {
            var parCount = 0;
            var pos = startPosition;
            var endPosition = sci.TextLength;
            while (pos < endPosition)
            {
                if (!sci.PositionIsOnComment(pos))
                {
                    var c = (char)sci.CharAt(pos);
                    if (c == '(') parCount++;
                    else if (c == ')')
                    {
                        parCount--;
                        if (parCount == 0) break;
                    }
                }
                pos++;
            }
            return pos;
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
