// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using ASCompletion.Completion;
using ASCompletion.Context;
using ForForeachConverter.Completion;
using ForForeachConverter.Helpers;
using ScintillaNet;

namespace ForForeachConverter.Commands.AS3
{
    class ConvertForeachToKeyValueIteratorCommand : ConvertForeachToForCommand
    {
        public new static bool IsValidForConvert(ScintillaControl sci)
        {
            var expr = Complete.GetExpression(sci, sci.CurrentPos);
            if (expr.IsNull()) return false;
            var collection = expr.Collection;
            return IsObject(collection) || IsDictionary(collection);
        }

        static bool IsObject(ASResult collection)
        {
            var type = collection.Member?.Type;
            return !string.IsNullOrEmpty(type) && type == ASContext.Context.Features.objectKey;
        }

        static bool IsDictionary(ASResult collection)
        {
            var type = collection.Member?.Type;
            return !string.IsNullOrEmpty(type)
                && type == "Dictionary"
                && collection.Type.InFile.Package == "flash.utils";
        }

        protected override string GetSnippetFor(ScintillaControl sci, EForeach expr)
        {
            var result = Reflector.SnippetManager.GetSnippet("forin", sci.ConfigurationLanguage, sci.Encoding);
            result = TemplateUtils.ReplaceTemplateVariable(result, "TypClosestListName", $"{expr.Collection.Member.Name}");
            if (IsDictionary(expr.Collection)) result = result.Replace("name:String in", "key:* in");
            return result;
        }

        protected override string GetSnippetVar(ScintillaControl sci, EForeach expr)
        {
            var member = expr.Variable.Member;
            var result = TemplateUtils.GetTemplate("AssignVariable");
            result = TemplateUtils.ReplaceTemplateVariable(result, "Name", member.Name);
            result = TemplateUtils.ReplaceTemplateVariable(result, "Type", member.Type);
            result = TemplateUtils.ReplaceTemplateVariable(result, "EntryPoint", string.Empty);
            result += $"{expr.Collection.Member.Name}[key];";
            return result;
        }

        protected override string InsertBody(string snippet, int position, string body)
        {
            body = body.TrimStart('\t');
            return TemplateUtils.ReplaceTemplateVariable(snippet, "EntryPoint", body);
        }
    }
}
