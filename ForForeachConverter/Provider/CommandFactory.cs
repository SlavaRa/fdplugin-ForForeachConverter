using System.Collections.Generic;
using CodeRefactor.Commands;
using ForForeachConverter.Commands.AS3;
using PluginCore.FRService;
using ScintillaNet;

namespace ForForeachConverter.Provider
{
    using Command = RefactorCommand<IDictionary<string, List<SearchMatch>>>;

    class CommandFactory : ICommandFactory
    {
        public bool IsValidForConvertForeachToFor(ScintillaControl sci) => ConvertForeachToForCommand.IsValidForConvert(sci);

        public Command CreateConvertForeachToForCommand() => new ConvertForeachToForCommand();
    }
}
