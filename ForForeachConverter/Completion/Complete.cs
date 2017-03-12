using System.Collections.Generic;
using ASCompletion.Completion;
using ScintillaNet;

namespace ForForeachConverter.Completion
{
    public static class Complete
    {
        static readonly IComplete NullComplete = new NullComplete();
        static readonly Dictionary<string, IComplete> LanguageToComplete = new Dictionary<string, IComplete>
        {
            {"as3", new AS3.Complete()},
            {"haxe", new Haxe.Complete()}
        };

        static IComplete GetComplete(ScintillaControl sci)
        {
            IComplete complete;
            LanguageToComplete.TryGetValue(sci.ConfigurationLanguage, out complete);
            return complete ?? NullComplete;
        }

        public static int GetStartOfStatement(ScintillaControl sci, int startPosition) => GetComplete(sci).GetStartOfStatement(sci, startPosition);

        public static int GetEndOfStatement(ScintillaControl sci, int startPosition) => GetComplete(sci).GetEndOfStatement(sci, startPosition);

        public static int GetStartOfIfStatement(ScintillaControl sci, int startPosition) => GetComplete(sci).GetStartOfIfStatement(sci, startPosition);

        public static ASResult GetVarOfForeachStatement(ScintillaControl sci, int startPosition) => GetComplete(sci).GetVarOfForeachStatement(sci, startPosition);

        public static ASResult GetCollectionOfForeachStatement(ScintillaControl sci, int startPosition) => GetComplete(sci).GetCollectionOfForeachStatement(sci, startPosition);

        public static int GetStartOfBody(ScintillaControl sci, int startPosition) => GetComplete(sci).GetStartOfBody(sci, startPosition);

        public static EForeach GetExpression(ScintillaControl sci, int position) => new EForeach
        {
            StartPosition = GetStartOfStatement(sci, position),
            EndPosition = GetEndOfStatement(sci, position),
            Variable = GetVarOfForeachStatement(sci, position),
            Collection = GetCollectionOfForeachStatement(sci, position),
            BodyPosition = GetStartOfBody(sci, position)
        };
    }

    public struct EForeach
    {
        public int StartPosition;
        public int EndPosition;
        public ASResult Variable;
        public ASResult Collection;
        public int BodyPosition;

        public bool Equals(EForeach other)
        {
            return StartPosition == other.StartPosition && EndPosition == other.EndPosition
                && (Variable.Member != null && Variable.Member.Equals(other.Variable.Member) || Variable.Member == null && other.Variable.Member == null)
                && Collection.Member.Equals(other.Collection.Member) 
                && BodyPosition == other.BodyPosition;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is EForeach && Equals((EForeach) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = StartPosition;
                hashCode = (hashCode * 397) ^ EndPosition;
                hashCode = (hashCode * 397) ^ Variable.Member.GetHashCode();
                hashCode = (hashCode * 397) ^ Collection.Member.GetHashCode();
                hashCode = (hashCode * 397) ^ BodyPosition;
                return hashCode;
            }
        }
    }

    internal interface IComplete
    {
        int GetStartOfStatement(ScintillaControl sci, int startPosition);
        int GetEndOfStatement(ScintillaControl sci, int startPosition);
        int GetStartOfIfStatement(ScintillaControl sci, int startPosition);
        ASResult GetVarOfForeachStatement(ScintillaControl sci, int startPosition);
        ASResult GetCollectionOfForeachStatement(ScintillaControl sci, int startPosition);
        int GetStartOfBody(ScintillaControl sci, int startPosition);
    }

    internal class NullComplete : IComplete
    {
        public int GetStartOfStatement(ScintillaControl sci, int startPosition) => -1;
        public int GetEndOfStatement(ScintillaControl sci, int startPosition) => -1;
        public int GetStartOfIfStatement(ScintillaControl sci, int startPosition) => -1;
        public ASResult GetVarOfForeachStatement(ScintillaControl sci, int startPosition) => new ASResult();
        public ASResult GetCollectionOfForeachStatement(ScintillaControl sci, int startPosition) => new ASResult();
        public int GetStartOfBody(ScintillaControl sci, int startPosition) => -1;
    }
}