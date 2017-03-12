// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System.Collections.Generic;
using ASCompletion.Completion;
using ASCompletion.Model;
using PluginCore;

namespace ForForeachConverter.Provider
{
    public static class CommandFactoryProvider
    {
        public static readonly ICommandFactory DefaultFactory = new CommandFactory();
        static readonly Dictionary<string, ICommandFactory> LanguageToFactory = new Dictionary<string, ICommandFactory>();

        static CommandFactoryProvider()
        {
            Register("as3", new AS3.CommandFactory());
            Register("haxe", new Haxe.CommandFactory());
        }

        public static void Register(string language, ICommandFactory factory)
        {
            if (ContainsLanguage(language)) LanguageToFactory.Remove(language);
            LanguageToFactory.Add(language, factory);
        }

        public static bool ContainsLanguage(string language) => LanguageToFactory.ContainsKey(language);

        public static ICommandFactory GetFactoryForCurrentDocument()
        {
            var document = PluginBase.MainForm.CurrentDocument;
            if (document == null || !document.IsEditable) return null;
            return GetFactory(document);
        }

        public static ICommandFactory GetFactory(ITabbedDocument document) => GetFactory(document.SciControl.ConfigurationLanguage);

        public static ICommandFactory GetFactory(ASResult target) => GetFactory(target.InFile ?? target.Type.InFile);

        public static ICommandFactory GetFactory(FileModel file)
        {
            var language = PluginBase.MainForm.SciConfig.GetLanguageFromFile(file.FileName);
            return GetFactory(language);
        }

        public static ICommandFactory GetFactory(string language) => LanguageToFactory[language];
    }
}
