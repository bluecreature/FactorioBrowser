﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using MoonSharp.Interpreter;

namespace FactorioBrowser.Prototypes.Unpacker {

   internal class MoonSharpVariantValue : ILuaVariant {
      private readonly DynValue _value;

      public LuaValueType ValueType {
         get {
            switch (_value.Type) {
               case DataType.Nil:
                  return LuaValueType.Nil;

               case DataType.Boolean:
                  return LuaValueType.Boolean;

               case DataType.Number:
                  return LuaValueType.Number;

               case DataType.String:
                  return LuaValueType.String;

               case DataType.Table:
                  return LuaValueType.Table;

               default:
                  return LuaValueType.Opaque;
            }
         }
      }

      public bool AsBoolean {
         get {
            EnsureType(LuaValueType.Boolean);
            return _value.Boolean;
         }
      }

      public double AsNumber {
         get {
            EnsureType(LuaValueType.Number);
            return _value.Number;
         }
      }

      public string AsString {
         get {
            EnsureType(LuaValueType.String);
            return _value.String;
         }
      }

      public ILuaTable AsTable {
         get {
            EnsureType(LuaValueType.Table);
            return new MoonSharpTable(_value.Table);
         }
      }

      internal DynValue Underlying() {
         return _value;
      }

      public MoonSharpVariantValue(DynValue value) {
         Debug.Assert(value != null);
         _value = value;
      }

      public override string ToString() {
         return _value.ToString();
      }

      private void EnsureType(LuaValueType expected) {
         var currentType = ValueType;
         if (currentType != expected) {
            throw new InvalidOperationException(
               $"Lua value expected of type `{expected}', but was {currentType}");
         }
      }
   }

   internal class MoonSharpTable : ILuaTable {
      private readonly Table _table;

      public MoonSharpTable(Table table) {
         _table = table;
      }

      public IEnumerable<ILuaVariant> Keys() {
         foreach (var tableKey in _table.Keys) {
            yield return new MoonSharpVariantValue(tableKey);
         }
      }

      public IEnumerable<KeyValuePair<ILuaVariant, ILuaVariant>> Entries() {
         foreach (var tableEntry in _table.Pairs) {
            yield return new KeyValuePair<ILuaVariant, ILuaVariant>(
               new MoonSharpVariantValue(tableEntry.Key),
               new MoonSharpVariantValue(tableEntry.Value));
         }
      }

      public ILuaVariant Get(object key) {
         if (key is MoonSharpVariantValue) {
            key = ((MoonSharpVariantValue) key).Underlying();
         }

         var rawValue = _table.RawGet(key);
         return rawValue == null || rawValue.Type == DataType.Nil ? null :
            new MoonSharpVariantValue(rawValue);
      }
   }
}
