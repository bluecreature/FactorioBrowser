using System;
using System.Collections.Generic;

namespace FactorioBrowser.Prototypes.Unpacker {

   public enum LuaValueType {
      Nil,
      Boolean,
      Number,
      String,
      Table,
      Opaque,
   }

   public interface ILuaVariant {
      LuaValueType ValueType { get; }

      bool AsBoolean { get; }

      double AsNumber { get; }

      string AsString { get; }

      ILuaTable AsTable { get; }
   }

   public interface ILuaTable {
      IEnumerable<ILuaVariant> Keys();

      IEnumerable<KeyValuePair<ILuaVariant, ILuaVariant>> Entries();

      ILuaVariant Get(object key);
   }

   public static class LuaTableExtension {

      public static ILuaVariant ToVariant(this ILuaTable table) {
         return new TableVariantWrapper(table);

      }

      private class TableVariantWrapper : ILuaVariant {

         public TableVariantWrapper(ILuaTable table) {
            AsTable = table;
         }

         public LuaValueType ValueType => LuaValueType.Table;

         public bool AsBoolean {
            get {
               throw new InvalidOperationException();
            }
         }

         public double AsNumber {
            get {
               throw new InvalidOperationException();
            }
         }

         public string AsString {
            get {
               throw new InvalidOperationException();
            }
         }

         public ILuaTable AsTable { get; }
      }
   }
}
