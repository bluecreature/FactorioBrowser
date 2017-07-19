using System;

namespace FactorioBrowser.Mod.Loader {

   internal enum ReadStage {
      Settings,
      SettingsUpdate,
      SettingsFinalFixes,
      Data,
      DataUpdate,
      DataFinalFixes,
   }

   internal static class ReadStageExtension {
      public static string EntryPoint(this ReadStage stage) {
         switch (stage) {
            case ReadStage.Settings:
               return "settings.lua";

            case ReadStage.SettingsUpdate:
               return "settings-updates.lua";

            case ReadStage.SettingsFinalFixes:
               return "settings-final-fixes.lua";

            case ReadStage.Data:
               return "data.lua";

            case ReadStage.DataUpdate:
               return "data-updates.lua";

            case ReadStage.DataFinalFixes:
               return "data-final-fixes.lua";

            default:
               throw new NotImplementedException(
                  $"Incomplete implementation for {typeof(ReadStage).Name}");
         }
      }
   }
}
