using System.Collections.Generic;
using System.Collections.Immutable;
using FactorioBrowser.Mod.Finder;
using FactorioBrowser.Prototypes;
using FactorioBrowser.Prototypes.Unpacker;

namespace FactorioBrowser.Mod.Loader {

   public struct FcModFileInfo {

      public string Name { get; }

      public FcVersion Version { get; }

      public string Path { get; }

      public FcModDeploymentType DeploymentType { get; }

      public string ZipEntryBaseName { get; }

      public FcModFileInfo(string name, FcVersion version, string path,
         FcModDeploymentType deploymentType, string zipEntryBaseName) {

         Name = name;
         Version = version;
         Path = path;
         DeploymentType = deploymentType;
         ZipEntryBaseName = zipEntryBaseName;
      }

      public static FcModFileInfo FromMetaInfo(FcModMetaInfo meta) {
         return new FcModFileInfo(meta.Name, meta.Version, meta.Path, meta.DeploymentType,
            meta.DeploymentType == FcModDeploymentType.ZipFile ? meta.VersionedName() : null);
      }
   }

   public interface IFcModDataLoader {

      IImmutableList<FcModSetting> LoadSettings(IEnumerable<FcModFileInfo> mods);

      FcPrototypes LoadPrototypes(IEnumerable<FcModFileInfo> mods,
         IImmutableDictionary<string, object> settings);
   }
}
