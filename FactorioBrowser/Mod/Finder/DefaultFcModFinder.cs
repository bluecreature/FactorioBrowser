using System;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Text;
using NLog;

namespace FactorioBrowser.Mod.Finder {

   public class DefaultFcModFinder : IFcModFinder {
      private const string BaseModRelativePath = "data/base";
      private const string InfoJsonName = "info.json";

      private static readonly Logger Log = LogManager.GetCurrentClassLogger();

      private readonly string _gamePath;
      private readonly string _modsPath;

      public DefaultFcModFinder(string gamePath, string modsPath) {
         Contract.Requires(gamePath != null);
         Contract.Requires(modsPath != null);
         _gamePath = gamePath;
         _modsPath = modsPath;
      }

      public IImmutableList<FcModMetaInfo> FindAll() {
         var builder = ImmutableArray.CreateBuilder<FcModMetaInfo>(2);

         var baseModInfo = ReadBaseMod();
         builder.Add(baseModInfo);

         foreach (string modDirEntry in Directory.GetFileSystemEntries(_modsPath)) {
            var modInfo = ReadModInfo(modDirEntry);
            if (modInfo != null) {
               builder.Add(modInfo);
            }
         }

         return builder.ToImmutable();
      }

      private FcModMetaInfo ReadBaseMod() {
         string baseModPath = Path.Combine(_gamePath, BaseModRelativePath);
         if (!Directory.Exists(baseModPath)) {
            throw new FcModInfoException($"Base data directory {baseModPath} is missing.");
         }

         var baseInfo = ReadModInfo(baseModPath);
         if (baseInfo == null) {
            throw new FcModInfoException($"{InfoJsonName} missing in ${BaseModRelativePath}.");
         }

         return baseInfo;
      }

      private FcModMetaInfo ReadModInfo(string sourcePath) {
         InfoJson info;
         FcModDeploymentType depType;
         if (IsDir(sourcePath)) {
            depType = FcModDeploymentType.Directory;
            info = ReadDirModeInfo(sourcePath);
         } else if (sourcePath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) {
            depType = FcModDeploymentType.ZipFile;
            info = ReadZipModInfo(sourcePath);

         } else {
            Log.Info("Skipping entry {0}: it's neither a directory nor a .zip file.", sourcePath);
            return null;
         }

         if (info == null) {
            Log.Info("No {0} loaded from {1}. Skipped.", InfoJsonName, sourcePath);
            return null;
         }

         return new FcModMetaInfo(sourcePath, depType, info.Name, info.Version, info.Dependencies);
      }

      private InfoJson ReadDirModeInfo(string sourcePath) {
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

      private static bool IsDir(string path) {
         FileAttributes attr = File.GetAttributes(path); // XXX : error handling on access denied?
         return attr.HasFlag(FileAttributes.Directory);
      }
   }
}
