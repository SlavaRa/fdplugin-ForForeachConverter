// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using ASCompletion.Completion;
using ASCompletion.Context;
using ForForeachConverter.Completion;
using ForForeachConverter.Helpers;
using ScintillaNet;

namespace ForForeachConverter.Commands.Haxe
{
    class ConvertForeachToKeyValueIteratorCommand : ConvertForeachToForCommand
    {
        public new static bool IsValidForConvert(ScintillaControl sci)
        {
            var expr = Complete.GetExpression(sci, sci.CurrentPos);
            if (expr.IsNull()) return false;
            var collection = expr.Collection;
            return IsMap(collection);
        }

        static bool IsMap(ASResult collection)
        {
            var type = collection.Member?.Type;
            if (string.IsNullOrEmpty(type)) return false;
            type = Reflector.ASGenerator.CleanType(type);
            return type == "Map" || type == "haxe.Constraints.IMap"
                || Reflector.ASGenerator.CleanType(collection.Type.QualifiedName) == "haxe.Constraints.IMap";
        }

        protected override string GetSnippetFor(ScintillaControl sci, EForeach expr)
        {
            var result = Reflector.SnippetManager.GetSnippet("for", sci.ConfigurationLanguage, sci.Encoding);
            result = TemplateUtils.ReplaceTemplateVariable(result, "EntryPoint", $" in {expr.Collection.Member.Name}.keys()");
            return result;
        }

        protected override string GetSnippetVar(ScintillaControl sci, EForeach expr)
        {
            var result = TemplateUtils.GetTemplate("AssignVariable");
            result = TemplateUtils.ReplaceTemplateVariable(result, "Name", expr.Variable.Context.Value);
            result = TemplateUtils.ReplaceTemplateVariable(result, "Type", GetVariableType(expr));
            result = TemplateUtils.ReplaceTemplateVariable(result, "EntryPoint", string.Empty);
            var context = (ASComplete.CurrentResolvedContext.Result ?? new ASResult()).Context;
            var iterator = ASComplete.FindFreeIterator(ASContext.Context, ASContext.Context.CurrentClass, context);
            result += $"{expr.Collection.Member.Name}.get({iterator});";
            return result;
        }

        static string GetVariableType(EForeach expr)
        {
            var type = expr.Collection.Type.IndexType;
            return string.IsNullOrEmpty(type) ? string.Empty : type.Substring(type.IndexOf(",") + 1).Trim();
        }
    }
}
