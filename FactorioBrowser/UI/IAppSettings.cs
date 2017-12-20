using System.Diagnostics;
using System.IO;
using System.Reflection;
using Config.Net;

namespace FactorioBrowser.UI {

   public interface IAppSettings {

      [Option(Alias = "game_path")]
      string GamePath { get; set; }

      [Option(Alias = "mods_path")]
      string ModsPath { get; set; }

      [Option(Alias = "use_saved_settings", DefaultValue = false)]
      bool UseSavedSettings { get; set; }

   }
   public static class AppSettingsFactory {

      public static IAppSettings Create() {
         var exePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
         Debug.Assert(exePath != null);
         var cfgPath = Path.Combine(exePath, "config.ini");

         return new ConfigurationBuilder<IAppSettings>()
            .UseIniFile(cfgPath)
            .Build();
      }
   }
}
