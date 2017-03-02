using System.Collections.Generic;
using System.IO;
using PluginCore;
using PluginCore.Helpers;
using PluginCore.Managers;
using PluginCore.Utilities;

namespace ForForeachConverter
{
    public class PluginMain : IPlugin
    {
        string settingFilename;

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
        /// Adds the required event handlers
        /// </summary>
        void AddEventHandlers() => EventManager.AddEventHandler(this, EventType.Command);

        /// <summary>
        /// Saves the plugin settings
        /// </summary>
        void SaveSettings() => ObjectSerializer.Serialize(settingFilename, Settings);

        void OnAddRefactorOptions(List<ICompletionListItem> list)
        {
            //throw new System.NotImplementedException();
        }
    }
}