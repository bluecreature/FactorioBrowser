using System;

namespace FactorioBrowser.Mod.Loader {

   internal enum EntryPoint {
      Settings,
      SettingsUpdate,
      SettingsFinalFixes,
      Data,
      DataUpdate,
      DataFinalFixes,
   }

   internal static class EntryPointExtension {
      public static string Filename(this EntryPoint ep) {
         switch (ep) {
            case EntryPoint.Settings:
               return "settings.lua";

            case EntryPoint.SettingsUpdate:
               return "settings-updates.lua";

            case EntryPoint.SettingsFinalFixes:
               return "settings-final-fixes.lua";

            case EntryPoint.Data:
               return "data.lua";

            case EntryPoint.DataUpdate:
               return "data-updates.lua";

            case EntryPoint.DataFinalFixes:
               return "data-final-fixes.lua";

            default:
               throw new NotImplementedException(
                  $"Incomplete implementation for {typeof(EntryPoint).Name}");
         }
      }
   }
}
