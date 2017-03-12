// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Collections.Generic;
using CodeRefactor.Commands;
using PluginCore.FRService;
using ScintillaNet;

namespace ForForeachConverter.Provider
{
    using Command = RefactorCommand<IDictionary<string, List<SearchMatch>>>;

    class NullCommandFactory : ICommandFactory
    {
        public bool IsValidForConvertForeachToFor(ScintillaControl sci) => false;
        public bool IsValidForConvertForeachToForin(ScintillaControl sci) => false;
        public Command CreateConvertForeachToForCommand() => null;
        public Command CreateConvertForeachToForinCommand() => null;
    }
}
