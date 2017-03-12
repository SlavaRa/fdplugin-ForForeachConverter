// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Collections.Generic;
using CodeRefactor.Commands;
using PluginCore.FRService;
using ScintillaNet;

namespace ForForeachConverter.Provider
{
    using Command = RefactorCommand<IDictionary<string, List<SearchMatch>>>;

    public interface ICommandFactory
    {
        bool IsValidForConvertForeachToFor(ScintillaControl sci);
        bool IsValidForConvertForeachToForin(ScintillaControl sci);
        Command CreateConvertForeachToForCommand();
        Command CreateConvertForeachToForinCommand();
    }
}
