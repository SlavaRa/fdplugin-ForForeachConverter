using System;
using System.Collections.Generic;
using CodeRefactor.Commands;
using PluginCore.FRService;

namespace ForForeachConverter.Commands
{
    class ConvertForeachToForCommand : RefactorCommand<IDictionary<string, List<SearchMatch>>>
    {
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
