using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using FactorioBrowser.Mod.Finder;
using NLog;

namespace FactorioBrowser.Mod.Loader {
   internal interface IModFileResolver : IDisposable {
      bool Exists(string relPath);

      string FriendlyName(string relPath);

      Stream Open(string relPath);
   }

   internal sealed class DirModFileResolver : IModFileResolver {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly FcModMetaInfo _modInfo;

      public DirModFileResolver(FcModMetaInfo modInfo) {
         _modInfo = modInfo;
      }

      public void Dispose() {
      }

      public bool Exists(string relPath) {
         string fullPath = GetFilePath(relPath);
         return File.Exists(fullPath);
      }

      public string FriendlyName(string relPath) {
         return GetFilePath(relPath);
      }

      public Stream Open(string relPath) {
         string fullPath = GetFilePath(relPath);
         logger.Trace("Loading file {0}", fullPath);
         return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
      }

      private string GetFilePath(string relPath) {
         return Path.GetFullPath(Path.Combine(_modInfo.Path, relPath)); // XXX : safeguard against absolute relPath-s
      }
   }

   internal sealed class ZipModFileResolver : IModFileResolver {
      private static readonly Logger logger = LogManager.GetCurrentClassLogger();

      private readonly string _entryBaseName;
      private readonly ZipArchive _modFile;
      private readonly string _modFilePath;

      public ZipModFileResolver(FcModMetaInfo modInfo) {
         Debug.Assert(modInfo.DeploymentType == FcModDeploymentType.ZipFile);
         _entryBaseName = $"{modInfo.Name}_${modInfo.Version.ToDotNotation()}"; // TODO : move the logic to the ModFinder?
         _modFile = ZipFile.OpenRead(modInfo.Path);
         _modFilePath = modInfo.Path;
      }

      public void Dispose() {
         _modFile.Dispose();
      }

      public bool Exists(string relPath) {
         string entryPath = GetEntryPath(relPath);
         return _modFile.GetEntry(entryPath) != null;
      }

      public string FriendlyName(string relPath) {
         return Path.Combine(
            Path.GetFullPath(_modFilePath),
            GetEntryPath(relPath));
      }

      public Stream Open(string relPath) {
         string entryPath = GetEntryPath(relPath);
         ZipArchiveEntry entry = _modFile.GetEntry(entryPath);
         if (entry == null) {
            throw new FileNotFoundException(entryPath); // TODO: message
         }

         logger.Trace("Loading ZIP entry {0}/{1}", _modFilePath, entryPath);
         return entry.Open();
      }

      private string GetEntryPath(string relPath) {
         return Path.Combine(_entryBaseName, relPath);
      }
   }

   internal sealed class ModScriptLoader : IScriptLoader {
      private readonly IModFileResolver _fileResolver;
      private readonly IScriptLoader _commonLibLoader;

      public ModScriptLoader(IScriptLoader commonLibLoader, IModFileResolver fileResolver) {
         _commonLibLoader = commonLibLoader;
         _fileResolver = fileResolver;
      }

      public object LoadFile(string file, Table globalContext) {
         return _fileResolver.Exists(file) ? _fileResolver.Open(file) :
            _commonLibLoader.LoadFile(file, globalContext); // TODO : exception message if both fail?
      }

      public string ResolveModuleName(string modname, Table globalContext) {
         Debug.Assert(modname != null);
         string modRelPath = modname.Replace('.', '/') + ".lua";
         return _fileResolver.Exists(modRelPath) ? modRelPath :
            _commonLibLoader.ResolveModuleName(modname, globalContext);
      }

      [Obsolete]
      public string ResolveFileName(string filename, Table globalContext) {
         return filename;
      }
   }
}
