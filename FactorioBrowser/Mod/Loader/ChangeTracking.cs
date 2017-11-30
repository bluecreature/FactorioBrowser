using System;
using MoonSharp.Interpreter;

namespace FactorioBrowser.Mod.Loader {

   public sealed class ChangeTracking {
      private readonly Script _script;
      private string _currentMod;

      public ChangeTracking(Script script, Table dataRaw) {
         _script = script;
         _currentMod = null;
         SetupTracking(dataRaw, currentLevel: 0);
      }

      public void SetCurrentMod(string currentMod) {
         _currentMod =  currentMod;
      }

      private void WriteIndexProxied(Table table, DynValue key, DynValue value,
         int currentLevel) {

         table.Set(key, value);
         if (value?.Type == DataType.Table && currentLevel <= 1) {
            if (currentLevel == 1) {
               value.Table.Set("__mod__", DynValue.NewString(_currentMod));
            } else {
               SetupTracking(value.Table, currentLevel + 1);
            }
         }
      }

      private void SetupTracking(Table table, int currentLevel) {
         Table meta = new Table(_script) {
            ["__newindex"] = (Action<Table, DynValue, DynValue>)
               ((t, k, v) => WriteIndexProxied(t, k, v, currentLevel))
         };
         table.MetaTable = meta;
      }
   }
}
