using System;
using System.Collections.Generic;
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
      private static readonly ReadStage[] SettingsStages = new[] {
         ReadStage.Settings, ReadStage.SettingsUpdate, ReadStage.SettingsFinalFixes
      };
      private static readonly ReadStage[] PrototypesStages = new[] {
         ReadStage.Data, ReadStage.DataUpdate, ReadStage.DataFinalFixes
      };

      private static readonly Logger Log = LogManager.GetCurrentClassLogger();

      private readonly string _luaLibPath;
      private readonly FcModFileInfo _coreModInfo;
      private readonly IFcSettingsDefsUnpacker _settingsUnpacker;

      public DefaultModDataLoader(string gamePath, IFcSettingsDefsUnpacker settingsUnpacker) {

         _luaLibPath = Path.Combine(gamePath, LuaLibRelPath);
         _coreModInfo = new FcModFileInfo("core", Path.Combine(gamePath, CoreModRelPath),
            FcModDeploymentType.Directory, null);
         // TODO : find a cleaner way than calling back and forth between loader and unpacker
         _settingsUnpacker = settingsUnpacker;
      }

      public ILuaTable LoadRawData(IEnumerable<FcModFileInfo> allMods) {
         Script sharedState = new Script(CoreModules.Preset_SoftSandbox | CoreModules.LoadMethods);
         sharedState.Options.ScriptLoader = new FileSystemScriptLoader {
            IgnoreLuaPathGlobal = true,
            ModulePaths = new[] { Path.Combine(_luaLibPath, "?.lua") },
         };

         Setup(sharedState);

         IList<FcModFileInfo> combinedModList = new[] { _coreModInfo }.Concat(allMods).ToList();
         var coreLibLoader = new DirModFileResolver(_luaLibPath);
         CreateModsTable(sharedState, combinedModList);
         LoadStages(sharedState, coreLibLoader, combinedModList, SettingsStages);
         CreateSettingsTable(sharedState); // TODO : split the code
         LoadStages(sharedState, coreLibLoader, combinedModList, PrototypesStages);

         return GetRawData(sharedState);
      }

      private void CreateModsTable(Script sharedState, IEnumerable<FcModFileInfo> allMods) {
         Table modsTable = new Table(sharedState);
         foreach (var mod in allMods) {
            modsTable.Set(mod.Name, DynValue.NewString("1.0")); // TODO : set the real version
         }

         sharedState.Globals["mods"] = modsTable;
      }

      private void CreateSettingsTable(Script sharedState) {
         var startupSettingsTable = new Table(sharedState);

         var settings = _settingsUnpacker.Unpack(GetRawData(sharedState));
         foreach (var setting in settings.Where(s => s.SettingType == "startup")) {
            var valueHolder = new Table(sharedState);
            DynValue value;
            if (setting is FcBooleanSetting) {
               value = DynValue.NewBoolean(((FcBooleanSetting) setting).DefaultValue);

            } else if (setting is FcIntegerSetting) {
               value = DynValue.NewNumber(((FcIntegerSetting) setting).DefaultValue);

            } else if (setting is FcDoubleSetting) {
               value = DynValue.NewNumber(((FcDoubleSetting)setting).DefaultValue);

            } else if (setting is FcStringSetting) {
               value = DynValue.NewString(((FcStringSetting)setting).DefaultValue);

            } else {
               throw new NotImplementedException();
            }

            valueHolder.Set("value", value);
            startupSettingsTable.Set(setting.Name, DynValue.NewTable(valueHolder));
         }

         var settingsTable = new Table(sharedState);
         settingsTable.Set("startup", DynValue.NewTable(startupSettingsTable));

         sharedState.Globals["settings"] = settingsTable;
      }

      private void LoadStages(Script sharedState, IModFileResolver coreLibLoader,
         IList<FcModFileInfo> mods, ReadStage[] stages) {

         var stageLoader = new StageLoader(coreLibLoader, sharedState);
         foreach (var stage in stages) {
            foreach (var modFile in mods) {
               stageLoader.LoadStage(modFile, stage);
            }
         }
      }

      private void Setup(Script sharedState) {
         LoadBuiltinLibrary(sharedState, "serpent");
         LoadBuiltinLibrary(sharedState, "defines");

         sharedState.Globals["module"] = (Action)NoOp;
         sharedState.Globals["log"] = (Action<DynValue>)ModLogFunction;

         DynValue funcToNumber = sharedState.Globals.RawGet("tonumber");
         Debug.Assert(funcToNumber != null);
         sharedState.Globals["tonumber"] = (Func<DynValue, DynValue, DynValue>)
            ((n, b) => WrapToNumber(sharedState, funcToNumber, n, b));

         new LegacyLuaModuleEmulator(sharedState, "util").LoadWithEmulation();
         sharedState.DoFile(Path.Combine(_luaLibPath, "dataloader.lua"));
      }

      private void LoadBuiltinLibrary(Script sharedState, string libName) {
         using (var libSrc = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream($"FactorioBrowser.Mod.BuiltinLibs.{libName}.lua")) {

            var library = sharedState.LoadStream(libSrc, null, $"{libName}.lua");
            sharedState.Globals[libName] = sharedState.Call(library);
         }
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
