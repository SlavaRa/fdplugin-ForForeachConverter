// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using ASCompletion.Completion;
using ASCompletion.Context;
using ForForeachConverter.Completion;
using ForForeachConverter.Helpers;
using ScintillaNet;

namespace ForForeachConverter.Commands.Haxe
{
    internal class ConvertForeachToForCommand : AS3.ConvertForeachToForCommand
    {
        public new static bool IsValidForConvert(ScintillaControl sci)
        {
            var expr = Complete.GetExpression(sci, sci.CurrentPos);
            if (expr.IsNull()) return false;
            var collection = expr.Collection;
            return IsArray(collection) || IsVector(collection) || IsList(collection);
        }

        static bool IsArray(ASResult result)
        {
            var type = result.Member?.Type;
            return !string.IsNullOrEmpty(type)
                && Reflector.ASGenerator.CleanType(type) == Reflector.ASGenerator.CleanType(ASContext.Context.Features.arrayKey);
        }

        static bool IsVector(ASResult result)
        {
            var type = result.Member?.Type;
            if (string.IsNullOrEmpty(type)) return false;
            type = Reflector.ASGenerator.CleanType(type);
            return type == "haxe.ds.Vector"
                || type == "Vector" && result.Type.InFile.Package == "haxe.ds";
        }

        static bool IsList(ASResult result)
        {
            var type = result.Member?.Type;
            return !string.IsNullOrEmpty(type)
                && Reflector.ASGenerator.CleanType(type) == "List";
        }

        protected override string GetSnippetFor(ScintillaControl sci, EForeach expr)
        {
            var result = Reflector.SnippetManager.GetSnippet("for", sci.ConfigurationLanguage, sci.Encoding);
            result = TemplateUtils.ReplaceTemplateVariable(result, "EntryPoint", $" in 0...{expr.Collection.Member.Name}.length");
            return result;
        }

        protected override string GetSnippetVar(ScintillaControl sci, EForeach expr)
        {
            var result = TemplateUtils.GetTemplate("AssignVariable");
            result = TemplateUtils.ReplaceTemplateVariable(result, "Name", expr.Variable.Context.Value);
            result = TemplateUtils.ReplaceTemplateVariable(result, "Type", expr.Collection.Type.IndexType ?? string.Empty);
            result = TemplateUtils.ReplaceTemplateVariable(result, "EntryPoint", string.Empty);
            var context = (ASComplete.CurrentResolvedContext.Result ?? new ASResult()).Context;
            var iterator = ASComplete.FindFreeIterator(ASContext.Context, ASContext.Context.CurrentClass, context);
            result += $"{expr.Collection.Member.Name}[{iterator}];";
            return result;
        }
    }
}
