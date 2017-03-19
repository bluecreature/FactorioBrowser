using System;
using FactorioBrowser.Mod.Finder;

namespace FactorioBrowser.Mod.Loader {

   internal struct FcModFileInfo {

      public string Name { get; }

      public string Path { get; }

      public FcModDeploymentType DeploymentType { get; }

      public string ZipEntryBaseName { get; }

      public FcModFileInfo(string name, string path, FcModDeploymentType deploymentType,
         string zipEntryBaseName) {

         Name = name;
         Path = path;
         DeploymentType = deploymentType;
         ZipEntryBaseName = zipEntryBaseName;
      }

      public static FcModFileInfo FromMetaInfo(FcModMetaInfo meta) {
         return new FcModFileInfo(meta.Name, meta.Path, meta.DeploymentType,
            meta.DeploymentType == FcModDeploymentType.ZipFile ? meta.VersionedName() : null);
      }
   }

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
               return "data-updates.lua";

            case ReadStage.FinalFixes:
               return "data-final-fixes.lua";

            default:
               throw new NotImplementedException(
                  $"Incomplete implementation for {typeof(ReadStage).Name}");
         }
      }
   }
}
