using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FactorioBrowser.Mod.Finder;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using NLog;

namespace FactorioBrowser.Mod.Loader {

   internal sealed class ModListLoader {
      private const string CoreModRelPath = "data/core";
      private const string LuaLibRelPath = CoreModRelPath + "/lualib";

      private static readonly Logger Log = LogManager.GetCurrentClassLogger();

      private readonly string _gamePath;
      private readonly string _luaLibPath;

      public ModListLoader(string gamePath) {
         _gamePath = gamePath;
         _luaLibPath = Path.Combine(gamePath, LuaLibRelPath);
      }

      public Script LoadAll(IEnumerable<FcModFileInfo> allMods) {
         Script sharedState = new Script(CoreModules.Preset_SoftSandbox | CoreModules.LoadMethods);
         sharedState.Options.ScriptLoader = new FileSystemScriptLoader {
            IgnoreLuaPathGlobal = true,
            ModulePaths = new[] { Path.Combine(_luaLibPath, "?.lua") },
         };

         sharedState.Globals["module"] = (Action) NoOp;
         sharedState.Globals["log"] = (Action<DynValue>) ModLogFunction;

         DynValue funcToNumber = sharedState.Globals.RawGet("tonumber");
         Debug.Assert(funcToNumber != null);
         sharedState.Globals["tonumber"] = (Func<DynValue, DynValue, DynValue>)
            ((n, b) => WrapToNumber(sharedState, funcToNumber, n, b));

         new LegacyLuaModuleEmulator(sharedState, "util").LoadWithEmulation();
         sharedState.DoFile(Path.Combine(_luaLibPath, "dataloader.lua"));

         var coreLibLoader = new DirModFileResolver(_luaLibPath);
         LoadModData(allMods, coreLibLoader, sharedState);

         return sharedState;
      }

      private void ModLogFunction(DynValue param) {
         var msg = param.Type == DataType.String ? param.String : param.ToString();
         Log.Info("Lua Log: {0}", param); // TODO : identify the calling mod/file
      }

      private void LoadModData(IEnumerable<FcModFileInfo> allMods, IModFileResolver coreLibLoader,
         Script sharedState) {

         var stageLoader = new StageLoader(coreLibLoader, sharedState);

         FcModFileInfo hardcodedCore = new FcModFileInfo("core", Path.Combine(_gamePath, CoreModRelPath),
            FcModDeploymentType.Directory, null);

         stageLoader.LoadStage(hardcodedCore, ReadStage.Data);

         ReadStage[] stages = {ReadStage.Data, ReadStage.Update, ReadStage.FinalFixes,};
         IList<FcModFileInfo> modList = new List<FcModFileInfo>(allMods);
         foreach (var stage in stages) {
            foreach (var modFile in modList) {
               stageLoader.LoadStage(modFile, stage);
            }
         }
      }

      private static DynValue WrapToNumber(Script script, DynValue origToNumber, DynValue argNum, DynValue argBase) {
         if ((argBase == null || argBase.IsNil())
             && argNum.Type == DataType.String && argNum.String.StartsWith("0x")) {

            argNum = DynValue.NewString(argNum.String.Substring(2));
            argBase = DynValue.NewNumber(16);
         }

         var origResult = script.Call(origToNumber, argNum, argBase);
         return origResult;
      }

      private static void NoOp() {
      }
   }
}
