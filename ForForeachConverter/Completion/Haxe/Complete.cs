using ASCompletion.Completion;
using ScintillaNet;

namespace ForForeachConverter.Completion.Haxe
{
    public class Complete : AS3.Complete
    {
        public override int GetStartOfStatement(ScintillaControl sci, int startPosition)
        {
            var result = -1;
            var pos = startPosition;
            if (sci.GetWordFromPosition(pos) == "for")
            {
                var parCount = 0;
                var dots = string.Empty;
                var endPosition = sci.TextLength;
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
                                    result = sci.WordStartPosition(startPosition, true);
                                    break;
                                }
                            }
                            else if (c == '.')
                            {
                                dots += '.';
                                if (dots.Length == 3)
                                {
                                    result = -1;
                                    break;
                                }
                            }
                            else dots = string.Empty;
                        }
                    }
                    pos++;
                }
            }
            return result;
        }

        public new ASResult GetVarOfForeachStatement(ScintillaControl sci, int startPosition)
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

    }
}