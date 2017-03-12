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

      ILuaVariant Self(); // TODO : refactor the unpackers to eliminate the need and remove
   }
}
