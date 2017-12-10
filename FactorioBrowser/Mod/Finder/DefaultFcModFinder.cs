using System;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Text;
using NLog;

namespace FactorioBrowser.Mod.Finder {

   public class DefaultFcModFinder : IFcModFinder {
      private const string BaseModName = "base";
      private const string CoreModName = "core";
      private const string InfoJsonName = "info.json";

      private static readonly Logger Log = LogManager.GetCurrentClassLogger();

      public FcModList FindAll(string gamePath, string modsPath) {
         var builder = ImmutableArray.CreateBuilder<FcModMetaInfo>(2);

         var baseModInfo = ReadGameDataMod(gamePath, BaseModName, ReadModInfo);
         builder.Add(baseModInfo);

         // The 'core' mod isn't (always?) versioned, so pretend its version is the same as the game's version.
         var coreModInfo = ReadGameDataMod(gamePath, CoreModName,
            (path) => ReadCoreModInfo(GameVersionFromBase(baseModInfo.Version), path));

         foreach (string modDirEntry in Directory.GetFileSystemEntries(modsPath)) {
            var modInfo = ReadModInfo(modDirEntry);
            if (modInfo != null) {
               builder.Add(modInfo);
            }
         }

         return new FcModList(coreModInfo, builder.ToImmutable());
      }

      private FcModMetaInfo ReadGameDataMod(string gamePath, string modName,
         Func<string, FcModMetaInfo> readerMethod) {
         string baseModPath = Path.Combine(gamePath, "data", modName);
         if (!Directory.Exists(baseModPath)) {
            throw new FcModInfoException($"Required data directory {baseModPath} is missing.");
         }

         var baseInfo = readerMethod.Invoke(baseModPath);
         if (baseInfo == null) {
            throw new FcModInfoException($"{InfoJsonName} missing in ${baseModPath}.");
         }

         return baseInfo;
      }

      private FcModMetaInfo ReadCoreModInfo(FcVersion gameVersion, string sourcePath) {
         // TODO : less hacky way to handle a mod without a version?
         return new FcModMetaInfo(sourcePath,
            FcModDeploymentType.Directory,
            CoreModName,
            gameVersion,
            dependencies: null);
      }

      private FcModMetaInfo ReadModInfo(string sourcePath) {
         InfoJson info;
         FcModDeploymentType deploymentType;
         if (IsDir(sourcePath)) {
            deploymentType = FcModDeploymentType.Directory;
            info = ReadDirModInfo(sourcePath);
         } else if (sourcePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) {
            deploymentType = FcModDeploymentType.ZipFile;
            info = ReadZipModInfo(sourcePath);

         } else {
            Log.Info("Skipping entry {0}: it's neither a directory nor a .zip file.", sourcePath);
            return null;
         }

         if (info == null) {
            Log.Info("No {0} loaded from {1}. Skipped.", InfoJsonName, sourcePath);
            return null;
         }

         return new FcModMetaInfo(sourcePath, deploymentType, info.Name, info.Version, info.Dependencies);
      }

      private InfoJson ReadDirModInfo(string sourcePath) {
         string infoJsonPath = Path.Combine(sourcePath, InfoJsonName);
         if (File.Exists(infoJsonPath)) {
            using (var infoJsonReader = new StreamReader(
               new FileStream(infoJsonPath, FileMode.Open, FileAccess.Read), Encoding.UTF8)) {
               return ReadInfoJson(sourcePath, infoJsonReader);
            }
         }

         return null;
      }


      private InfoJson ReadZipModInfo(string sourcePath) {
         using (var modZip = ZipFile.OpenRead(sourcePath)) {
            var infoJsonEntry = FindInfoJsonEntry(modZip);
            if (infoJsonEntry != null) {
               using (var infoJson = new StreamReader(infoJsonEntry.Open(), Encoding.UTF8)) {
                  return ReadInfoJson(sourcePath, infoJson);
               }
            }
         }

         return null;
      }

      private ZipArchiveEntry FindInfoJsonEntry(ZipArchive archive) {
         foreach (var entry in archive.Entries) {
            if (entry.FullName.EndsWith(InfoJsonName)) {
               return entry;
            }
         }

         return null;
      }

      private InfoJson ReadInfoJson(string source, StreamReader reader) {
         return new InfoJsonReader(source, reader).Read();
      }

      private static FcVersion GameVersionFromBase(FcVersion version) {
         return new FcVersion(version.Major, version.Minor, 0);
      }

      private static bool IsDir(string path) {
         FileAttributes attr = File.GetAttributes(path); // XXX : error handling on access denied?
         return attr.HasFlag(FileAttributes.Directory);
      }
   }
}
