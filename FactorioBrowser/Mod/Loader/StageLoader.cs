using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FactorioBrowser.Mod.Finder;
using MoonSharp.Interpreter;
using NLog;

namespace FactorioBrowser.Mod.Loader {

   internal sealed class StageLoader {
      private static readonly Logger Log = LogManager.GetCurrentClassLogger();

      private readonly IModFileResolver _commonLibLoader;
      private readonly Script _sharedState;

      public StageLoader(IModFileResolver commonLibLoader, Script sharedState) {
         _commonLibLoader = commonLibLoader;
         _sharedState = sharedState;
      }

      public void LoadStage(FcModFileInfo fileInfo, ReadStage stage) {
         using (IModFileResolver resolver = CreateFileResolver(fileInfo)) {
            string entryPoint = stage.EntryPoint();
            if (resolver.Exists(entryPoint)) {
               Log.Debug("Loading stage {0} for mod {1}", stage, fileInfo.Name);
               LoadEntryPoint(resolver, entryPoint);

            } else {
               Log.Debug("Skipping stage {0} for mod {1} because {2} doesn't exist.",
                  stage, fileInfo.Name, entryPoint);
            }
         }
      }

      private void LoadEntryPoint(IModFileResolver fileResolver, string entryPointPath) {
         DynValue originalRequire = _sharedState.Globals.RawGet("require");
         try {
            _sharedState.Globals["require"] = (Func<ScriptExecutionContext, CallbackArguments, DynValue>)
               (new ModLocalRequireImpl(_sharedState, fileResolver, _commonLibLoader).RequireModule);

            Stream entryPoint = fileResolver.Open(entryPointPath);
            _sharedState.DoStream(entryPoint, null, entryPointPath);

         } finally {
            _sharedState.Globals["require"] = originalRequire;
         }
      }

      private IModFileResolver CreateFileResolver(FcModFileInfo file) {
         if (file.DeploymentType == FcModDeploymentType.Directory) {
            return new DirModFileResolver(file.Path);

         } else {
            Debug.Assert(file.DeploymentType == FcModDeploymentType.ZipFile);
            return new ZipModFileResolver(file.Path, file.ZipEntryBaseName);
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
            var found = TryLoadFromModRoot(moduleName, attemptedLocations) ??
               TryLoadCallingModuleSibling(moduleName, attemptedLocations) ??
               TryLoadFromGlobalLib(moduleName, attemptedLocations);

            if (found == null) {
               throw new ScriptRuntimeException(
                  $"Required module `{moduleName}' not found on search " +
                  $"path:\n {string.Join("\n ", attemptedLocations)}");
            }

            var location = found.Item1;
            var stream = found.Item2;
            _loadPath.AddLast(moduleName);
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

         private Tuple<string, Stream> TryLoadCallingModuleSibling(string moduleName,
            List<string> attemptedLocations) {

            var callingModule = _loadPath.Count > 0 ? _loadPath.Last.Value : null;
            int lastPathSep;
            if (callingModule != null &&
               (lastPathSep = callingModule.LastIndexOf(".", StringComparison.Ordinal)) > 0) {

               string siblingModuleFqName = callingModule.Substring(0, lastPathSep + 1) + moduleName;
               return TryLoadFromModRoot(siblingModuleFqName, attemptedLocations);

            } else {
               return null;
            }
         }

         private Tuple<string, Stream> TryLoadFromModRoot(string moduleName,
            List<string> attemptedLocations) {

            return TryOpenWithResolver(_modFileResolver, moduleName, attemptedLocations);
         }

         private Tuple<string, Stream> TryLoadFromGlobalLib(string moduleName,
            List<string> attemptedLocations) {

            return TryOpenWithResolver(_commonLibResolver, moduleName, attemptedLocations);
         }

         private Tuple<string, Stream> TryOpenWithResolver(IModFileResolver resolver,
            string moduleName, List<string> attemptedLocations) {

            string modRelPath = moduleName.Replace(".", "/") + ".lua";
            try {
               string friendlyName = resolver.FriendlyName(modRelPath);
               Stream stream = resolver.Open(modRelPath);
               return Tuple.Create(friendlyName, stream);

            } catch (FileNotFoundException e) {
               attemptedLocations.Add(e.FileName);
               return null;
            }
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
