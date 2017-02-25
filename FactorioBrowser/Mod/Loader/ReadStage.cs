using System;

namespace FactorioBrowser.Mod.Loader {
   internal enum ReadStage {
      Data,
      Update,
      FinalFixes,
   }

   internal static class ReadStageExtension {
      public static string EntryPoint(this ReadStage stage) {
         switch (stage) {
            case ReadStage.Data:
               return "data.lua";

            case ReadStage.Update:
               return "data-update.lua";

            case ReadStage.FinalFixes:
               return "data-final-fixes.lua";

            default:
               throw new NotImplementedException(
                  $"Incomplete implementation for {typeof(ReadStage).Name}");
         }
      }
   }
}
