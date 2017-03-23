using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using NLog;

namespace FactorioBrowser.Mod.Loader {

   internal interface IModFileResolver : IDisposable {

      /// <summary>
      /// Checks whether the supplied <param name="relPath">path</param> exists.
      /// </summary>
      bool Exists(string relPath);

      /// <summary>
      /// Returns a human-friendly name for the supplied <param name="relPath">path</param>.
      /// No check is performed whether it actually exists.
      /// </summary>
      string FriendlyName(string relPath);

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
         string fullPath = GetFilePath(relPath);
         return File.Exists(fullPath);
      }

      public string FriendlyName(string relPath) {
         return GetFilePath(relPath);
      }

      public Stream Open(string relPath) {
         string fullPath = GetFilePath(relPath);
         Log.Trace("Loading file {0}", fullPath);
         try {
            return new FileStream(fullPath, FileMode.Open, FileAccess.Read);

         } catch (DirectoryNotFoundException e) {
            throw new FileNotFoundException($"File not found: {fullPath}", fullPath, e);
         }
      }

      private string GetFilePath(string relPath) {
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

   internal sealed class AssetPathFileResolver : IModFileResolver {
      private static readonly char[] PathSeparators = {
         Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar

      };

      private static readonly Regex QualifiedPathPattern = new Regex("^__([^/]+)__/(.+)$");
      private readonly IDictionary<string, IModFileResolver> _modSpecificResolvers;

      public AssetPathFileResolver(IDictionary<string, IModFileResolver> modSpecificResolvers) {
         _modSpecificResolvers = modSpecificResolvers;
      }

      public bool Exists(string relPath) {
         IModFileResolver resolver;
         string modRelPath;
         return RouteToSpecificResolver(relPath, out resolver, out modRelPath) && resolver.Exists(modRelPath);
      }

      public string FriendlyName(string relPath) {
         return relPath;
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
      }

      private bool RouteToSpecificResolver(string relPath, out IModFileResolver resolver, out string modRelPath) {
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
}
