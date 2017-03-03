using System;
using System.Collections.Generic;
using CodeRefactor.Commands;
using PluginCore.FRService;
using ScintillaNet;

namespace ForForeachConverter.Commands.AS3
{
    class ConvertForeachToForCommand : RefactorCommand<IDictionary<string, List<SearchMatch>>>
    {
        public static bool IsValidForConvert(ScintillaControl sci)
        {
            var pos = sci.CurrentPos;
            var word = sci.GetWordFromPosition(pos);
            if (word == "each") return true;
            if (word != "for") return false;
            pos = sci.WordEndPosition(pos, true) + 1;
            word = sci.GetWordFromPosition(pos);
            return word == "each";
        }

        protected override void ExecutionImplementation()
        {
            throw new NotImplementedException();
        }

        public override bool IsValid()
        {
            throw new NotImplementedException();
        }
    }
}
