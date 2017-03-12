// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Collections.Generic;
using ASCompletion.Completion;
using ASCompletion.Context;
using CodeRefactor.Commands;
using ForForeachConverter.Completion;
using ForForeachConverter.Helpers;
using PluginCore.FRService;
using ScintillaNet;

namespace ForForeachConverter.Commands.Haxe
{
    internal class ConvertForeachToForCommand : RefactorCommand<IDictionary<string, List<SearchMatch>>>
    {
        public static bool IsValidForConvert(ScintillaControl sci)
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

        public override bool IsValid() => true;

        protected override void ExecutionImplementation()
        {
        }
    }
}
