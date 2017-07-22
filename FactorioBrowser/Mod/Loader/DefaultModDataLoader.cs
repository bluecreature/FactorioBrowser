using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using FactorioBrowser.Mod.Finder;
using FactorioBrowser.Prototypes;
using FactorioBrowser.Prototypes.Unpacker;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using NLog;

namespace FactorioBrowser.Mod.Loader {

   internal sealed class DefaultModDataLoader : IFcModDataLoader {
      private const string CoreModRelPath = "data/core";
      private const string LuaLibRelPath = CoreModRelPath + "/lualib";

      private static readonly EntryPoint[] SettingsEntrypoints = new[] {
         EntryPoint.Settings, EntryPoint.SettingsUpdate, EntryPoint.SettingsFinalFixes
      };
      private static readonly EntryPoint[] PrototypesEntrypoints = new[] {
         EntryPoint.Data, EntryPoint.DataUpdate, EntryPoint.DataFinalFixes
      };

      private static readonly Logger Log = LogManager.GetCurrentClassLogger();

      private readonly IFcSettingDefsUnpacker _settingDefsUnpacker;
      private readonly IFcPrototypeUnpacker _prototypeUnpacker;
      private readonly string _luaLibPath;
      private readonly FcModFileInfo _coreModInfo;

      public DefaultModDataLoader(string gamePath, IFcSettingDefsUnpacker settingDefsUnpacker,
         IFcPrototypeUnpacker prototypeUnpacker) {
         _settingDefsUnpacker = settingDefsUnpacker;
         _prototypeUnpacker = prototypeUnpacker;

         _luaLibPath = Path.Combine(gamePath, LuaLibRelPath);
         _coreModInfo = new FcModFileInfo("core", new FcVersion(1, 0, 0),  // TODO : get the actual version
            Path.Combine(gamePath, CoreModRelPath),
            FcModDeploymentType.Directory, null);
      }

      public IImmutableList<FcModSetting> LoadSettings(IEnumerable<FcModFileInfo> mods) {
         var rawData = LoadEntryPoints(mods, null, SettingsEntrypoints);
         return _settingDefsUnpacker.Unpack(rawData);
      }

      public FcPrototypes LoadPrototypes(IEnumerable<FcModFileInfo> mods,
         IImmutableDictionary<string, object> settings) {

         var rawData = LoadEntryPoints(mods, settings, PrototypesEntrypoints);
         return _prototypeUnpacker.Unpack(rawData);
      }

      private ILuaTable LoadEntryPoints(IEnumerable<FcModFileInfo> mods,
         IImmutableDictionary<string, object> settings, EntryPoint[] entryPoints) {

         Script sharedState = SetupLuaState();
         if (settings != null) {
            sharedState.Globals["settings"] = CreateSettingsTable(sharedState, settings);
         }

         IList<FcModFileInfo> combinedModList = new[] { _coreModInfo }.Concat(mods).ToList();
         var coreLibLoader = new DirModFileResolver(_luaLibPath);

         sharedState.Globals["mods"] = CreateModsTable(sharedState, combinedModList);

         var stageLoader = new EntryPointLoader(coreLibLoader, sharedState);
         foreach (var stage in entryPoints) {
            foreach (var modFile in combinedModList) {
               stageLoader.LoadEntryPoint(modFile, stage);
            }
         }

         return GetRawData(sharedState);
      }

      private Script SetupLuaState() {
         Script sharedState = new Script(CoreModules.Preset_SoftSandbox | CoreModules.LoadMethods);
         sharedState.Options.ScriptLoader = new FileSystemScriptLoader {
            IgnoreLuaPathGlobal = true,
            ModulePaths = new[] { Path.Combine(_luaLibPath, "?.lua") },
         };

         sharedState.Globals["serpent"] = LoadBuiltinLibrary(sharedState, "serpent");
         sharedState.Globals["defines"] = LoadBuiltinLibrary(sharedState, "defines");
         sharedState.Globals["module"] = (Action)NoOp;
         sharedState.Globals["log"] = (Action<DynValue>)ModLogFunction;

         DynValue funcToNumber = sharedState.Globals.RawGet("tonumber");
         Debug.Assert(funcToNumber != null);
         sharedState.Globals["tonumber"] = (Func<DynValue, DynValue, DynValue>)
            ((n, b) => WrapToNumber(sharedState, funcToNumber, n, b));

         new LegacyLuaModuleEmulator(sharedState, "util").LoadWithEmulation();
         sharedState.DoFile(Path.Combine(_luaLibPath, "dataloader.lua"));

         return sharedState;
      }

      private DynValue LoadBuiltinLibrary(Script sharedState, string libName) {
         using (var libSrc = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream($"FactorioBrowser.Mod.BuiltinLibs.{libName}.lua")) {

            var library = sharedState.LoadStream(libSrc, null, $"builtin/{libName}.lua");
            return sharedState.Call(library);
         }
      }

      private Table CreateModsTable(Script sharedState, IEnumerable<FcModFileInfo> allMods) {
         Table modsTable = new Table(sharedState);
         foreach (var mod in allMods) {
            modsTable.Set(mod.Name, DynValue.NewString(mod.Version.ToDotNotation()));
         }

         return modsTable;
      }

      private Table CreateSettingsTable(Script sharedState,
         IImmutableDictionary<string, object> settings) {

         var startupSettingsTable = new Table(sharedState);

         foreach (var setting in settings) {
            var value = setting.Value;
            DynValue luaValue;
            if (value is bool) {
               luaValue = DynValue.NewBoolean((bool) value);

            } else if (value is int || value is long || value is double) {
               luaValue = DynValue.NewNumber(Convert.ToDouble(value));

            } else if (value is string) {
               luaValue = DynValue.NewString((string) value);

            } else {
               throw new NotImplementedException(
                  "Internal error/incomplete implementation: can't handle settings of type " + value?.GetType());
            }

            var valueHolder = new Table(sharedState);
            valueHolder.Set("value", luaValue);
            startupSettingsTable.Set(setting.Key, DynValue.NewTable(valueHolder));
         }

         var settingsTable = new Table(sharedState);
         settingsTable.Set("startup", DynValue.NewTable(startupSettingsTable));

         return settingsTable;
      }

      private static DynValue WrapToNumber(Script script, DynValue origToNumber, DynValue argNum, DynValue argBase) {
         if ((argBase == null || argBase.IsNil())
             && argNum.Type == DataType.String && argNum.String.StartsWith("0x")) {

            argNum = DynValue.NewString(argNum.String.Substring(2));
            argBase = DynValue.NewNumber(16);
         }

         try {
            var origResult = script.Call(origToNumber, argNum, argBase);
            return origResult;

         } catch (FormatException) {
            return DynValue.Nil;
         }
      }

      private static ILuaTable GetRawData(Script sharedState) {
         var rawData = sharedState.Globals.RawGet("data").Table.RawGet("raw");
         return new MoonSharpTable(rawData.Table, rawData);
      }

      private void ModLogFunction(DynValue param) {
         var msg = param.Type == DataType.String ? param.String : param.ToString();
         Log.Info("Lua Log: {0}", msg); // TODO : identify the calling mod/file
      }

      private static void NoOp() {
      }
   }
}
