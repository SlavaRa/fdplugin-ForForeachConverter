// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Collections.Generic;
using CodeRefactor.Commands;
using PluginCore.FRService;
using ScintillaNet;

namespace ForForeachConverter.Commands.Haxe
{
    class ConvertForeachToForCommand : RefactorCommand<IDictionary<string, List<SearchMatch>>>
    {
        public static bool IsValidForConvert(ScintillaControl sci) => false;

        public override bool IsValid() => false;

        protected override void ExecutionImplementation()
        {
        }
    }
}
