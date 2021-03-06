﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MoonSharp.Interpreter;
using NLog;

namespace FactorioBrowser.Mod.Loader {

   /// <summary>
   /// Load a mod's <see cref="EntryPoint"/> into a shared Lua state.
   /// </summary>
   internal sealed class EntryPointLoader {
      private static readonly Logger Log = LogManager.GetCurrentClassLogger();

      private readonly IModFileResolver _commonLibLoader;
      private readonly Script _sharedState;

      public EntryPointLoader(IModFileResolver commonLibLoader, Script sharedState) {
         _commonLibLoader = commonLibLoader;
         _sharedState = sharedState;
      }

      public void LoadEntryPoint(FcModFileInfo fileInfo, EntryPoint entryPoint) {
         string entryPointFn = entryPoint.Filename();
         using (IModFileResolver resolver = ModFileResolverFactory.CreateResolver(fileInfo)) {
            if (resolver.Exists(entryPointFn)) {
               Log.Debug("Loading entry point {0} from mod {1}", entryPoint, fileInfo.Name);
               LoadEntryPointWithResolver(resolver, entryPointFn);

            } else {
               Log.Debug("Skipping entry point {0} for mod {1} because it doesn't exist.",
                  entryPointFn, fileInfo.Name);
            }
         }
      }

      private void LoadEntryPointWithResolver(IModFileResolver fileResolver, string entryPointPath) {
         DynValue originalRequireFunction = _sharedState.Globals.RawGet("require");
         try {
            _sharedState.Globals["require"] = (Func<ScriptExecutionContext, CallbackArguments, DynValue>)
               (new ModLocalRequireImpl(_sharedState, fileResolver, _commonLibLoader).RequireModule);

            Stream entryPoint = fileResolver.Open(entryPointPath);
            _sharedState.DoStream(entryPoint, null, fileResolver.FriendlyName(entryPointPath));

         } finally {
            _sharedState.Globals["require"] = originalRequireFunction;
         }
      }

      private sealed class ModLocalRequireImpl {

         private readonly LinkedList<string> _loadPath = new LinkedList<string>();
         private readonly IDictionary<string, DynValue> _locallyLoadedModules = new Dictionary<string, DynValue>();
         private readonly Script _script;
         private readonly IModFileResolver _modFileResolver;
         private readonly IModFileResolver _commonLibResolver;

         public ModLocalRequireImpl(Script script, IModFileResolver modFileResolver,
            IModFileResolver commonLibResolver) {

            _script = script;
            _modFileResolver = modFileResolver;
            _commonLibResolver = commonLibResolver;
         }

         public DynValue RequireModule(ScriptExecutionContext ctx, CallbackArguments args) {
            DynValue moduleNameValue;
            if (args.Count != 1 ||
               (moduleNameValue = args.RawGet(0, false)).Type != DataType.String) {

               throw new ScriptRuntimeException(
                  "The `require' function must be called with a single string-type argument.");
            }

            String moduleName = moduleNameValue.String;
            DynValue existing = TryGetLoadedModule(moduleName);
            return existing ?? LoadNewModule(moduleName);
         }

         private DynValue LoadNewModule(string moduleName) {
            var attemptedLocations = new List<string>();
            string nameAsRequired = moduleName;
            if (moduleName.IndexOfAny(PathTools.PathSeparators) < 0) {
               moduleName = moduleName.Replace('.', '/');
            }
            moduleName += ".lua";

            FoundModule? findResult = TryLoadFromModRoot(moduleName, attemptedLocations) ??
                  TryLoadModuleSibling(moduleName, attemptedLocations) ??
                  TryLoadFromGlobalLib(moduleName, attemptedLocations);

            if (!findResult.HasValue) {
               throw new ScriptRuntimeException(
                  $"Required module `{nameAsRequired}' not found. Searched on " +
                  $"path:\n {string.Join("\n ", attemptedLocations)}");
            }

            var found = findResult.Value;
            var location = found.FriendlyName;
            var stream = found.Stream;
            _loadPath.AddLast(found.RelativePath);
            try {
               var moduleLoader = _script.LoadStream(stream, null, location);
               var module = _script.Call(moduleLoader);
               _locallyLoadedModules[moduleName] = module;
               return module;
            } finally {
               _loadPath.RemoveLast();
               stream.Close();
            }
         }

         private FoundModule? TryLoadModuleSibling(string moduleRelPath,
            List<string> attemptedLocations) {

            var callingModule = _loadPath.Count > 0 ? _loadPath.Last.Value : null;
            int lastPathSeparator;
            if (callingModule != null
               && (lastPathSeparator = callingModule.LastIndexOfAny(PathTools.PathSeparators)) >= 0) {
               var path = callingModule.Substring(0, lastPathSeparator + 1);
               return TryOpenWithResolver(_modFileResolver, path + moduleRelPath, attemptedLocations);

            } else {
               return null;
            }
         }

         private FoundModule? TryLoadFromModRoot(string moduleRelPath,
            List<string> attemptedLocations) {

            return TryOpenWithResolver(_modFileResolver, moduleRelPath, attemptedLocations);
         }

         private FoundModule? TryLoadFromGlobalLib(string moduleRelPath,
            List<string> attemptedLocations) {

            return TryOpenWithResolver(_commonLibResolver, moduleRelPath, attemptedLocations);
         }

         private FoundModule? TryOpenWithResolver(IModFileResolver resolver,
            string moduleRelPath, List<string> attemptedLocations) {

            string friendlyName = resolver.FriendlyName(moduleRelPath);
            if (resolver.Exists(moduleRelPath)) {
               try {
                  Stream stream = resolver.Open(moduleRelPath);
                  return new FoundModule {
                     RelativePath = moduleRelPath,
                     FriendlyName = friendlyName,
                     Stream = stream
                  };

               } catch (FileNotFoundException) {
               }
            }

            // Either Exists returned false or Open threw
            attemptedLocations.Add(friendlyName);
            return null;
         }

         private struct FoundModule {
            public string RelativePath;
            public string FriendlyName;
            public Stream Stream;
         }

         private DynValue TryGetLoadedModule(string moduleName) {
            DynValue loaded = GetLoadedPackages().RawGet(moduleName);
            if (loaded != null) {
               return loaded;
            }

            if (_locallyLoadedModules.TryGetValue(moduleName, out loaded)) {
               Debug.Assert(loaded != null);
               return loaded;
            }

            return null;
         }

         private Table GetLoadedPackages() {
            var packageTable = _script.Globals.RawGet("package");
            Debug.Assert(packageTable.Type == DataType.Table);

            var loadedTable = packageTable.Table.RawGet("loaded");
            Debug.Assert(loadedTable.Type == DataType.Table);

            return loadedTable.Table;
         }
      }
   }
}
