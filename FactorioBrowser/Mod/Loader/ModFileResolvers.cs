using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using FactorioBrowser.Mod.Finder;
using Ninject.Infrastructure.Language;
using NLog;

namespace FactorioBrowser.Mod.Loader {

   internal interface IModFileResolver : IDisposable {

      /// <summary>
      /// Checks whether there exists a file with the supplied <param name="relPath">path</param>.
      /// </summary>
      bool Exists(string relPath);

      /// <summary>
      /// Returns a human-friendly name for the supplied <param name="relPath">path</param>.
      /// No check is performed whether it actually exists.
      /// </summary>
      string FriendlyName(string relPath);

      /// <summary>
      /// Returns the list of files (and files only, no child directories) in the specified
      /// directory. If no such directory exists, an empty list is returned.
      /// </summary>
      IImmutableList<string> ListDirectory(string relPath);

      /// <summary>
      /// Open the file at the supplied <param name="relPath">path</param> for
      /// reading. Throws <exception cref="FileNotFoundException">FileNotFoundException</exception>
      /// if the file doesn't exist.
      /// </summary>
      Stream Open(string relPath);
   }

   internal sealed class DirModFileResolver : IModFileResolver {
      private static readonly Logger Log = LogManager.GetCurrentClassLogger();

      private readonly string _path;

      public DirModFileResolver(string path) {
         _path = path;
      }

      public void Dispose() {
      }

      public bool Exists(string relPath) {
         string fullPath = GetFullPath(relPath);
         return File.Exists(fullPath);
      }

      public string FriendlyName(string relPath) {
         return GetFullPath(relPath);
      }

      public IImmutableList<string> ListDirectory(string relPath) {
         string fullPath = GetFullPath(relPath);
         try {
            if (File.GetAttributes(fullPath).HasFlag(FileAttributes.Directory)) {
               var entries = Directory.GetFileSystemEntries(fullPath)
                  .Where(e => !File.GetAttributes(e).HasFlag(FileAttributes.Directory))
                  .ToImmutableList();
               return entries;

            } else {
               return ImmutableList.Create<string>();
            }

         } catch (FileNotFoundException) {
            return ImmutableList.Create<string>();

         } catch (DirectoryNotFoundException) {
            return ImmutableList.Create<string>();
         }
      }


      public Stream Open(string relPath) {
         string fullPath = GetFullPath(relPath);
         Log.Trace("Loading file {0}", fullPath);
         try {
            return new FileStream(fullPath, FileMode.Open, FileAccess.Read);

         } catch (DirectoryNotFoundException e) {
            throw new FileNotFoundException($"File not found: {fullPath}", fullPath, e);
         }
      }

      private string GetFullPath(string relPath) {
         return Path.GetFullPath(Path.Combine(_path, PathTools.NormalizePath(relPath)));
      }
   }

   internal sealed class ZipModFileResolver : IModFileResolver {
      private static readonly Logger Log = LogManager.GetCurrentClassLogger();

      private readonly string _entryBaseName;
      private readonly ZipArchive _modFile;
      private readonly string _modFilePath;

      public ZipModFileResolver(string path, string entryBaseName) {
         _entryBaseName = entryBaseName;
         _modFile = ZipFile.OpenRead(path);
         _modFilePath = path;
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

      public IImmutableList<string> ListDirectory(string relPath) {
         string entryPath = GetEntryPath(relPath);
         return _modFile.Entries
            .Select(e => e.FullName)
            .Where(e => e.StartsWith(entryPath, StringComparison.OrdinalIgnoreCase) &&
                  !e.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            .Select(e => e.Substring(_entryBaseName.Length + 1))
            .ToImmutableList();
      }

      public Stream Open(string relPath) {
         string entryPath = GetEntryPath(relPath);
         ZipArchiveEntry entry = _modFile.GetEntry(entryPath);
         if (entry == null) {
            throw new FileNotFoundException($"Entry `{entryPath}' doesn't exist.",
               FriendlyName(entryPath));
         }

         Log.Trace("Loading ZIP entry {0}/{1}", _modFilePath, entryPath);
         return ToSeekableStream(entry.Open());
      }

      private string GetEntryPath(string relPath) {
         return _entryBaseName + "/" + PathTools.NormalizePath(relPath, "/");
      }

      private static Stream ToSeekableStream(Stream stream) {
         if (stream.CanSeek) {
            return stream;
         } else {
            var mem = new MemoryStream();
            try {
               stream.CopyTo(mem);
               mem.Seek(0, SeekOrigin.Begin);
               return mem;
            } finally {
               stream.Close(); ;
            }
         }
      }
   }

   internal static class PathTools {
      public static readonly char[] PathSeparators = new char[] {
         Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar
      };

      public static string NormalizePath(string relPath, string targetSeparator = "\\") {
         List<string> normalizedPath = new List<string>();

         string[] pathComponents = relPath.Split(PathSeparators);
         foreach (var component in pathComponents) {
            if (component.Length == 0 || component.Equals(".")) {
               continue;

            } else if (component.Equals("..")) {
               if (normalizedPath.Count > 0) {
                  normalizedPath.RemoveAt(normalizedPath.Count - 1);
               }

            } else {
               normalizedPath.Add(component);
            }
         }

         return string.Join(targetSeparator, normalizedPath);
      }
   }

   internal sealed class PathRoutingFileResolver : IModFileResolver {
      private static readonly char[] PathSeparators = {
         Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar

      };

      private static readonly Regex QualifiedPathPattern = new Regex("^__([^/]+)__/(.+)$");
      private readonly IImmutableDictionary<string, IModFileResolver> _modSpecificResolvers;

      public PathRoutingFileResolver(IEnumerable<FcModFileInfo> modList) {
         var resolverDictBuilder = ImmutableDictionary.CreateBuilder<string, IModFileResolver>();
         foreach (var modInfo in modList) {
            resolverDictBuilder.Add(modInfo.Name, ModFileResolverFactory.CreateResolver(modInfo));
         }

         _modSpecificResolvers = resolverDictBuilder.ToImmutable();
      }

      public bool Exists(string relPath) {
         IModFileResolver resolver;
         string modRelPath;
         return RouteToSpecificResolver(relPath, out resolver, out modRelPath) && resolver.Exists(modRelPath);
      }

      public string FriendlyName(string relPath) {
         return relPath;
      }

      public IImmutableList<string> ListDirectory(string relPath) {
         IModFileResolver resolver;
         string modRelPath;
         if (RouteToSpecificResolver(relPath, out resolver, out modRelPath)) {
            return resolver.ListDirectory(modRelPath);

         } else {
            return ImmutableList.Create<string>();
         }
      }

      public Stream Open(string relPath) {
         IModFileResolver resolver;
         string modRelPath;
         if (!RouteToSpecificResolver(relPath, out resolver, out modRelPath)) {
            throw new FileNotFoundException(
               $"Asset path invalid or references non-existent mod: {relPath}", relPath);
         }

         return resolver.Open(modRelPath);
      }

      public void Dispose() {
         foreach (var modFileResolver in _modSpecificResolvers.Values) {
            modFileResolver.Dispose();
         }
      }

      private bool RouteToSpecificResolver(string relPath, out IModFileResolver resolver,
         out string modRelPath) {

         resolver = null;
         modRelPath = null;
         var match = QualifiedPathPattern.Match(relPath);
         if (!(match.Success && _modSpecificResolvers.TryGetValue(match.Groups[1].Value, out resolver))) {
            return false;

         } else {
            modRelPath = match.Groups[2].Value;
            return true;
         }
      }
   }

   internal static class ModFileResolverFactory {

      public static IModFileResolver CreateResolver(FcModFileInfo modInfo) {
         if (modInfo.DeploymentType == FcModDeploymentType.Directory) {
            return new DirModFileResolver(modInfo.Path);

         } else {
            Debug.Assert(modInfo.DeploymentType == FcModDeploymentType.ZipFile);
            return new ZipModFileResolver(modInfo.Path, modInfo.ZipEntryBaseName);
         }
      }

      public static IModFileResolver CreateRoutingResolver(IEnumerable<FcModFileInfo> modList) {
         return new PathRoutingFileResolver(modList);
      }
   }
}
