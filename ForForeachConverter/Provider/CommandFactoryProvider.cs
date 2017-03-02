using System.Collections.Generic;
using ASCompletion.Completion;
using ASCompletion.Model;
using PluginCore;

namespace ForForeachConverter.Provider
{
    public static class CommandFactoryProvider
    {
        public static readonly CodeRefactor.Provider.ICommandFactory DefaultFactory = new CodeRefactor.Provider.CommandFactory();
        static readonly Dictionary<string, CodeRefactor.Provider.ICommandFactory> LanguageToFactory = new Dictionary<string, CodeRefactor.Provider.ICommandFactory>();

        static CommandFactoryProvider()
        {
            Register("as3", DefaultFactory);
            Register("haxe", DefaultFactory);
        }

        public static void Register(string language, CodeRefactor.Provider.ICommandFactory factory)
        {
            if (ContainsLanguage(language)) LanguageToFactory.Remove(language);
            LanguageToFactory.Add(language, factory);
        }

        public static bool ContainsLanguage(string language)
        {
            return LanguageToFactory.ContainsKey(language);
        }

        public static CodeRefactor.Provider.ICommandFactory GetFactoryFromCurrentDocument()
        {
            var document = PluginBase.MainForm.CurrentDocument;
            if (document == null || !document.IsEditable) return null;
            return GetFactoryFromDocument(document);
        }

        public static CodeRefactor.Provider.ICommandFactory GetFactoryFromDocument(ITabbedDocument document)
        {
            var language = document.SciControl.ConfigurationLanguage;
            return GetFactoryFromLanguage(language);
        }

        public static CodeRefactor.Provider.ICommandFactory GetFactoryFromTarget(ASResult target)
        {
            return GetFactoryFromFile(target.InFile ?? target.Type.InFile);
        }

        public static CodeRefactor.Provider.ICommandFactory GetFactoryFromFile(FileModel file)
        {
            var language = PluginBase.MainForm.SciConfig.GetLanguageFromFile(file.FileName);
            return GetFactoryFromLanguage(language);
        }

        public static CodeRefactor.Provider.ICommandFactory GetFactoryFromLanguage(string language)
        {
            return LanguageToFactory[language];
        }
    }
}
