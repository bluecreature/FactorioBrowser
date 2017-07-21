using System;
using System.Collections.Generic;
using System.Diagnostics;
using FactorioBrowser.Prototypes.Unpacker;

namespace FactorioBrowserTests.Prototypes.Unpacker {

   internal class ObjectLuaVariant : ILuaVariant {
      private readonly object _data;

      public LuaValueType ValueType {
         get {
            if (_data == null) {
               return LuaValueType.Nil;

            } else if (_data is Boolean) {
               return LuaValueType.Boolean;

            } else if (_data is Int32 || _data is Int64 || _data is Double) {
               return LuaValueType.Number;

            } else if (_data is string) {
               return LuaValueType.String;

            } else if (_data is DictionaryLuaTable || _data is IDictionary<object, object>) {
               return LuaValueType.Table;

            } else {
               return LuaValueType.Opaque;
            }
         }
      }

      public bool AsBoolean {
         get {
            Debug.Assert(ValueType == LuaValueType.Boolean);
            return (bool) _data;
         }
      }

      public double AsNumber {
         get {
            Debug.Assert(ValueType == LuaValueType.Number);
            return Convert.ToDouble(_data);
         }
      }

      public string AsString {
         get {
            Debug.Assert(ValueType == LuaValueType.String);
            return (string) _data;
         }
      }

      public ILuaTable AsTable {
         get {
            Debug.Assert(ValueType == LuaValueType.Table);
            return _data is ILuaTable ? (ILuaTable) _data :
               new DictionaryLuaTable((IDictionary<object, object>) _data, this);
         }
      }

      public ObjectLuaVariant(object data) {
         _data = data;
      }
   }

   internal class DictionaryLuaTable : ILuaTable {
      private readonly IDictionary<object, object> _data;
      private readonly ILuaVariant _self;

      public DictionaryLuaTable(IDictionary<object, object> data, ILuaVariant self) {
         _data = data;
         _self = self;
      }

      public DictionaryLuaTable(IDictionary<object, object> data) : this(data, null) {
      }

      public IEnumerable<ILuaVariant> Keys() {
         foreach (var dataKey in _data.Keys) {
            yield return new ObjectLuaVariant(dataKey);
         }
      }

      public IEnumerable<KeyValuePair<ILuaVariant, ILuaVariant>> Entries() {
         foreach (var entry in _data) {
            yield return new KeyValuePair<ILuaVariant, ILuaVariant>(
               new ObjectLuaVariant(entry.Key), new ObjectLuaVariant(entry.Value));
         }
      }

      public ILuaVariant Get(object key) {
         object value;
         if (_data.TryGetValue(key, out value)) {
            return new ObjectLuaVariant(value);

         } else {
            return null;
         }
      }

      public ILuaVariant Self() {
         return _self ?? new ObjectLuaVariant(this);
      }
   }
}
