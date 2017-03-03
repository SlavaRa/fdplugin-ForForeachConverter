using System.Collections.Generic;
using CodeRefactor.Commands;
using PluginCore.FRService;

namespace ForForeachConverter.Provider
{
    using Command = RefactorCommand<IDictionary<string, List<SearchMatch>>>;

    public interface ICommandFactory
    {
        Command CreateConvertForeachToForCommand();
    }
}
