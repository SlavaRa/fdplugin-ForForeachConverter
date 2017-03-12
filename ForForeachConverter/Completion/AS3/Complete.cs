using ASCompletion.Completion;
using ForForeachConverter.Helpers;
using ScintillaNet;

namespace ForForeachConverter.Completion.AS3
{
    public class Complete : IComplete
    {
        public virtual int GetStartOfStatement(ScintillaControl sci, int startPosition)
        {
            var result = -1;
            var pos = startPosition;
            switch (sci.GetWordFromPosition(pos))
            {
                case "for":
                    pos = sci.WordEndPosition(pos, true);
                    if (sci.GetWordRight(pos + 1, true) == "each") result = sci.WordStartPosition(startPosition, true);
                    break;
                case "each":
                    pos = sci.WordStartPosition(pos, true) - 1;
                    if (sci.GetWordLeft(pos, true) == "for") result = sci.WordStartPosition(pos, true);
                    break;
            }
            return result;
        }

        public virtual int GetEndOfStatement(ScintillaControl sci, int startPosition)
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

        public virtual int GetStartOfIFStatement(ScintillaControl sci, int startPosition)
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

        public virtual ASResult GetVarOfForeachStatement(ScintillaControl sci, int startPosition)
        {
            var result = new ASResult();
            var parCount = 0;
            var characterClass = ScintillaControl.Configuration.GetLanguage(sci.ConfigurationLanguage).characterclass.Characters;
            var endPosition = sci.TextLength;
            var pos = startPosition;
            while (pos < endPosition)
            {
                if (!sci.PositionIsOnComment(pos))
                {
                    var c = (char)sci.CharAt(pos);
                    if (c > ' ')
                    {
                        if (parCount == 0 && c == '(') parCount++;
                        else if (parCount == 1 && characterClass.IndexOf(c) != -1)
                        {
                            var word = sci.GetWordRight(pos, true);
                            if (word == "var") pos += word.Length + 1;
                            pos = sci.WordEndPosition(pos, true);
                            result = ASComplete.GetExpressionType(sci, pos);
                            break;
                        }
                    }
                }
                pos++;
            }
            return result;
        }

        public virtual ASResult GetCollectionOfForeachStatement(ScintillaControl sci, int startPosition)
        {
            var result = new ASResult();
            var parCount = 0;
            var endPosition = sci.TextLength;
            var pos = startPosition;
            while (pos < endPosition)
            {
                if (!sci.PositionIsOnComment(pos))
                {
                    var c = (char) sci.CharAt(pos);
                    if (c > ' ')
                    {
                        if (c == '(') parCount++;
                        else if (c == ')')
                        {
                            parCount--;
                            if (parCount == 0)
                            {
                                result = ASComplete.GetExpressionType(sci, pos);
                                break;
                            }
                        }
                    }
                }
                pos++;
            }
            return result;
        }

        public virtual int GetStartOfBody(ScintillaControl sci, int startPosition)
        {
            var result = -1;
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
                        if (parCount == 0)
                        {
                            result = pos + 1;
                            break;
                        }
                    }
                }
                pos++;
            }
            return result;
        }
    }
}