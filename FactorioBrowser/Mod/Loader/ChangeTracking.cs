using System;
using MoonSharp.Interpreter;

namespace FactorioBrowser.Mod.Loader {

   public sealed class ChangeTracking {
      private readonly Script _script;

      public ChangeTracking(Script script, Table dataRaw) {
         _script = script;
         SetupTracking(dataRaw, currentLevel: 0);
      }

      public string CurrentMod { get; set; }

      private void WriteIndexProxied(Table table, DynValue key, DynValue value,
         int currentLevel) {

         table.Set(key, value);
         if (value?.Type == DataType.Table && currentLevel <= 1) {
            if (currentLevel == 1) {
               value.Table.Set("__mod__", DynValue.NewString(CurrentMod));
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
