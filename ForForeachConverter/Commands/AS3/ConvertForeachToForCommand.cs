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
            var type = expr.Collection.Member?.Type;
            return !string.IsNullOrEmpty(type) && (type == ASContext.Context.Features.arrayKey || type.StartsWith("Vector."));
        }

        public override bool IsValid() => true;

        protected override void ExecutionImplementation()
        {
            var sci = PluginBase.MainForm.CurrentDocument.SciControl;
            var expr = Complete.GetExpression(sci, sci.CurrentPos);
            var startPosition = expr.StartPosition;
            sci.SetSel(startPosition, expr.EndPosition);
            sci.ReplaceSel(string.Empty);
            var snippet = GetSnippet(sci, expr);
            SnippetHelper.InsertSnippetText(sci, startPosition, snippet);
            sci.SetSel(startPosition, startPosition);
        }

        static string GetSnippet(ScintillaControl sci, EForeach expr)
        {
            var template = Reflector.SnippetManager.GetSnippet("for", sci.ConfigurationLanguage, sci.Encoding);
            template = TemplateUtils.ReplaceTemplateVariable(template, "EntryPoint", $"{expr.Collection.Member.Name}.length");
            var parCount = 0;
            var brCount = 0;
            for (var i = 0; i < template.Length; i++)
            {
                var c = template[i];
                if (c == '(') parCount++;
                else if (c == ')') parCount--;
                else if (parCount == 0 && c == '{') brCount++;
                else if (brCount == 1 && c == '\n')
                {
                    var body = GetBody(sci, expr);
                    template = template.Insert(i + 1, body);
                    break;
                }
            }
            return template;
        }

        static string GetBody(ScintillaControl sci, EForeach expr)
        {
            var body = sci.GetTextRange(expr.BodyPosition, expr.EndPosition);
            var indentation = sci.GetLineIndentation(sci.LineFromPosition(expr.StartPosition)) / sci.Indent;
            if (indentation > 0)
            {
                var s = string.Empty;
                for (var i = 0; i < indentation; i++) s += '\t';
                body = body.Replace(s, string.Empty);
            }
            body = body.Trim();
            body = body.Trim('{', '\r', '\n');
            body = body.TrimEnd('}');
            body = body.TrimEnd('\r', '\n');
            var member = expr.Variable.Member;
            var template = TemplateUtils.GetTemplate("AssignVariable");
            template = TemplateUtils.ReplaceTemplateVariable(template, "Name", member.Name);
            template = TemplateUtils.ReplaceTemplateVariable(template, "Type", member.Type);
            template = TemplateUtils.ReplaceTemplateVariable(template, "EntryPoint", string.Empty);
            var result = ASComplete.CurrentResolvedContext.Result ?? new ASResult();
            var iterator = ASComplete.FindFreeIterator(ASContext.Context, ASContext.Context.CurrentClass, result.Context);
            template += $"{expr.Collection.Member.Name}[{iterator}];{sci.NewLineMarker}";
            body = body.Insert(0, $"{'\t'}{template}");
            return body;
        }
    }
}
