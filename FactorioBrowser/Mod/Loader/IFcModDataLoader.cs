using System.Collections.Generic;
using FactorioBrowser.Mod.Finder;
using FactorioBrowser.Prototypes.Unpacker;

namespace FactorioBrowser.Mod.Loader {

   public struct FcModFileInfo {

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

   public interface IFcModDataLoader {

      ILuaTable LoadRawData(IEnumerable<FcModFileInfo> list);
   }
}
