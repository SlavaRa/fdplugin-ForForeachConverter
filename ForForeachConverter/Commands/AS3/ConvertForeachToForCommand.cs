// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Collections.Generic;
using ASCompletion.Completion;
using ASCompletion.Context;
using CodeRefactor.Commands;
using ForForeachConverter.Completion;
using ForForeachConverter.Helpers;
using PluginCore;
using PluginCore.FRService;
using PluginCore.Helpers;
using ScintillaNet;

namespace ForForeachConverter.Commands.AS3
{
    public class ConvertForeachToForCommand : RefactorCommand<IDictionary<string, List<SearchMatch>>>
    {
        public static bool IsValidForConvert(ScintillaControl sci)
        {
            var expr = Complete.GetExpression(sci, sci.CurrentPos);
            if (expr.IsNull()) return false;
            var collection = expr.Collection;
            return IsArray(collection) || IsVector(collection);
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
            return !string.IsNullOrEmpty(type) && type.StartsWith("Vector.");
        }

        public override bool IsValid() => true;

        protected override void ExecutionImplementation()
        {
            var sci = PluginBase.MainForm.CurrentDocument.SciControl;
            var expr = Complete.GetExpression(sci, sci.CurrentPos);
            var snippet = GetSnippet(sci, expr);
            var startPosition = expr.StartPosition;
            sci.SetSel(startPosition, expr.EndPosition);
            sci.ReplaceSel(string.Empty);
            SnippetHelper.InsertSnippetText(sci, startPosition, snippet);
            sci.SetSel(startPosition, startPosition);
        }

        string GetSnippet(ScintillaControl sci, EForeach expr)
        {
            var result = GetSnippetFor(sci, expr);
            var parCount = 0;
            var brCount = 0;
            for (var i = 0; i < result.Length; i++)
            {
                var c = result[i];
                if (c == '(') parCount++;
                else if (c == ')') parCount--;
                else if (parCount == 0 && c == '{') brCount++;
                else if (brCount == 1 && c == '\t')
                {
                    var body = GetBody(sci, expr);
                    result = result.Remove(i, 1).Insert(i, body);
                    break;
                }
            }
            return result;
        }

        string GetBody(ScintillaControl sci, EForeach expr)
        {
            var result = sci.GetTextRange(sci.MBSafePosition(expr.BodyPosition), sci.MBSafePosition(expr.EndPosition)).Trim();
            var indentation = sci.GetLineIndentation(sci.LineFromPosition(expr.StartPosition)) / sci.Indent;
            if (indentation > 0)
            {
                var s = string.Empty;
                for (var i = 0; i < indentation; i++) s += '\t';
                result = result.Replace(s, string.Empty);
            }
            result = result.Trim('{', '\r', '\n');
            result = result.TrimEnd('}');
            result = result.TrimEnd('\r', '\n');
            var variable = GetSnippetVar(sci, expr);
            if (result.TrimStart('\t', ' ').Length > 0) variable += sci.NewLineMarker;
            result = result.Insert(0, $"{'\t'}{variable}");
            return result;
        }

        protected virtual string GetSnippetFor(ScintillaControl sci, EForeach expr)
        {
            var result = Reflector.SnippetManager.GetSnippet("for", sci.ConfigurationLanguage, sci.Encoding);
            result = TemplateUtils.ReplaceTemplateVariable(result, "EntryPoint", $"{expr.Collection.Member.Name}.length");
            return result;
        }

        protected virtual string GetSnippetVar(ScintillaControl sci, EForeach expr)
        {
            var member = expr.Variable.Member;
            var result = TemplateUtils.GetTemplate("AssignVariable");
            result = TemplateUtils.ReplaceTemplateVariable(result, "Name", member.Name);
            result = TemplateUtils.ReplaceTemplateVariable(result, "Type", member.Type);
            result = TemplateUtils.ReplaceTemplateVariable(result, "EntryPoint", string.Empty);
            var context = (ASComplete.CurrentResolvedContext.Result ?? new ASResult()).Context;
            var iterator = ASComplete.FindFreeIterator(ASContext.Context, ASContext.Context.CurrentClass, context);
            result += $"{expr.Collection.Member.Name}[{iterator}];";
            return result;
        }
    }
}
