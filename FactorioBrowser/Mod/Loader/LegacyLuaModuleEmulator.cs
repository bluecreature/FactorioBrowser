using System;
using MoonSharp.Interpreter;

namespace FactorioBrowser.Mod.Loader {
   internal sealed class LegacyLuaModuleEmulator {
      private readonly Script _script;
      private readonly string _modName;
      private readonly Table _modulePrivateTable;

      public LegacyLuaModuleEmulator(Script script, string modName) {
         _script = script;
         _modName = modName;
         _modulePrivateTable = new Table(_script);
      }

      public void LoadWithEmulation() {
         _script.Globals[_modName] = _modulePrivateTable;

         Table symbolProxy = CreateSymbolProxy();
         var module = _script.RequireModule(_modName, symbolProxy);

         _script.Call(module);
         _script.Globals.RawGet("package").Table[_modName] = _modulePrivateTable;
      }

      private DynValue DecorateGetIndex(Table proxy, DynValue key) {
         DynValue value = _script.Globals.RawGet(key);
         if (value.Type == DataType.Table) {
            if (_modulePrivateTable.RawGet(key) == null) {
               _modulePrivateTable[key] = value;
            }
         }

         return value;
      }

      private void DecorateSetIndex(Table proxy, DynValue index, DynValue value) {
         _script.Globals.Set(index, value);
         _modulePrivateTable.Set(index, value);
      }

      private Table CreateSymbolProxy() {
         Table symbolProxy = new Table(_script);
         symbolProxy.MetaTable = new Table(_script);
         symbolProxy.MetaTable["__index"] = (Func<Table, DynValue, DynValue>) DecorateGetIndex;
         symbolProxy.MetaTable["__newindex"] = (Action<Table, DynValue, DynValue>) DecorateSetIndex;

         return symbolProxy;
      }
   }
}
