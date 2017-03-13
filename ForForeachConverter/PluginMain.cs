// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using System;
using System.Collections.Generic;
using System.IO;
using ForForeachConverter.Controls;
using PluginCore;
using PluginCore.Helpers;
using PluginCore.Managers;
using PluginCore.Utilities;
using ScintillaNet;
using CommandFactoryProvider = ForForeachConverter.Provider.CommandFactoryProvider;

namespace ForForeachConverter
{
    public class PluginMain : IPlugin
    {
        string settingFilename;
        RefactorMenu refactorMainMenu;

        #region Required Properties

        public int Api => 1;
        public string Name => nameof(ForForeachConverter);
        public string Guid => "21d9ab3e-93e4-4460-9298-c62f87eed7ba";
        public string Help => string.Empty;
        public string Author => "SlavaRa";
        public string Description => string.Empty;
        public object Settings { get; private set; }

        #endregion

        /// <summary>
        /// Initializes the plugin
        /// </summary>
        public void Initialize()
        {
            InitBasics();
            LoadSettings();
            CreateMenuItems();
            AddEventHandlers();
        }

        /// <summary>
        /// Disposes the plugin
        /// </summary>
        public void Dispose() => SaveSettings();

        /// <summary>
        /// Handles the incoming events
        /// </summary>
        public void HandleEvent(object sender, NotifyEvent e, HandlingPriority priority)
        {
            switch (e.Type)
            {
                case EventType.Command:
                    var de = (DataEvent) e;
                    switch (de.Action)
                    {
                        case "ASCompletion.ContextualGenerator.AddOptions":
                            OnAddRefactorOptions(de.Data as List<ICompletionListItem>);
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// Initializes important variables
        /// </summary>
        void InitBasics()
        {
            var path = Path.Combine(PathHelper.DataDir, Name);
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            settingFilename = Path.Combine(path, "Settings.fdb");
        }

        /// <summary>
        /// Loads the plugin settings
        /// </summary>
        void LoadSettings()
        {
            Settings = new Settings();
            if (!File.Exists(settingFilename)) SaveSettings();
            else Settings = (Settings) ObjectSerializer.Deserialize(settingFilename, Settings);
        }

        /// <summary>
        /// Saves the plugin settings
        /// </summary>
        void SaveSettings() => ObjectSerializer.Serialize(settingFilename, Settings);

        void CreateMenuItems()
        {
            refactorMainMenu = new RefactorMenu();
            refactorMainMenu.ConvertForeachToFor.Click += ConvertForeachToForOnClick;
            refactorMainMenu.ConvertForeachToKeyValueIterator.Click += ConvertForeachToKeyValueIteratorOnClick;
            CompletionMenuProvider.Menu = refactorMainMenu;
        }

        /// <summary>
        /// Adds the required event handlers
        /// </summary>
        void AddEventHandlers() => EventManager.AddEventHandler(this, EventType.Command);

        static void OnAddRefactorOptions(List<ICompletionListItem> list)
        {
            var doc = PluginBase.MainForm.CurrentDocument;
            if (!doc.IsEditable) return;
            var sci = doc.SciControl;
            if (CommandFactoryProvider.ContainsLanguage(sci.ConfigurationLanguage))
                list.AddRange(CompletionMenuProvider.GetItems(sci));
        }

        static void ConvertForeachToForOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                CommandFactoryProvider.GetFactoryForCurrentDocument()
                    .CreateConvertForeachToForCommand()
                    .Execute();
            }
            catch (Exception e)
            {
                ErrorManager.ShowError(e);
            }
        }

        static void ConvertForeachToKeyValueIteratorOnClick(object sender, EventArgs eventArgs)
        {
            try
            {
                CommandFactoryProvider.GetFactoryForCurrentDocument()
                    .CreateConvertForeachToKeyValueIteratorCommand()
                    .Execute();
            }
            catch (Exception e)
            {
                ErrorManager.ShowError(e);
            }
        }
    }

    class CompletionMenuProvider
    {
        public static RefactorMenu Menu;

        public static List<ICompletionListItem> GetItems(ScintillaControl sci)
        {
            var result = new List<ICompletionListItem>();
            var factory = CommandFactoryProvider.GetFactoryForCurrentDocument();
            if (factory.IsValidForConvertForeachToFor(sci)) result.Add(new RefactorCompletionItem(Menu.ConvertForeachToFor));
            if (factory.IsValidForConvertForeachToKeyValueIterator(sci))
            {
                //FIXME slavara:
                Menu.ConvertForeachToKeyValueIterator.Text = sci.ConfigurationLanguage == "haxe" ? "To key-value iterator" : "To for..in";
                result.Add(new RefactorCompletionItem(Menu.ConvertForeachToKeyValueIterator));
            }
            return result;
        }
    }
}