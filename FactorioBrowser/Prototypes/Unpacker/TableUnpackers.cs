using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using FactorioBrowser.Mirror;

namespace FactorioBrowser.Prototypes.Unpacker {

   internal interface ITableUnpacker<out T> {
      T Unpack(ILuaTable data, string path);
   }

   internal sealed class DictionaryUnpacker<TKey, TValue> : ITableUnpacker<IDictionary<TKey, TValue>> {
      private readonly UnpackerDispatcher _dispatcher;

      public DictionaryUnpacker(UnpackerDispatcher dispatcher) {
         _dispatcher = dispatcher;
      }

      public IDictionary<TKey, TValue> Unpack(ILuaTable data, string path) {
         IDictionary<TKey, TValue> mirrorData = new Dictionary<TKey, TValue>();
         foreach (var entry in data.Entries()) {
            var key = _dispatcher.Unpack<TKey>(entry.Key, path);
            var value = _dispatcher.Unpack<TValue>(entry.Value, path + "." + key);
            mirrorData[key] = value;
         }

         return mirrorData;
      }
   }

   internal sealed class ListUnpacker<T> : ITableUnpacker<IList<T>> {
      private readonly UnpackerDispatcher _dispatcher;

      public ListUnpacker(UnpackerDispatcher dispatcher) {
         _dispatcher = dispatcher;
      }

      public IList<T> Unpack(ILuaTable data, string path) {
         IList<T> mirror = new List<T>();
         int i = 1;
         ILuaVariant item;
         while ((item = data.Get(i)) != null) {
            T v = _dispatcher.Unpack<T>(item, path + $"[{i}]");
            mirror.Add(v);
            i++;
         }

         return mirror;
      }
   }

   internal sealed class StructureUnpacker<T> : ITableUnpacker<T> where T : class {
      private readonly UnpackerDispatcher _dispatcher;

      public StructureUnpacker(UnpackerDispatcher dispatcher) {
         _dispatcher = dispatcher;
      }

      public T Unpack(ILuaTable data, string path) {
         Debug.Assert(TypeTools.IsStructureType<T>());

         Type targetType = typeof(T);
         T target = Activator.CreateInstance<T>();

         foreach (var propInfo in targetType.GetProperties()) {
            FcDataFieldMirror dataFieldDef = propInfo.GetCustomAttribute<FcDataFieldMirror>();
            if (dataFieldDef != null) {
               PopulateDataField(data, path, target, propInfo, dataFieldDef);
            }
         }

         return target;
      }

      private void PopulateDataField(ILuaTable data, string path, T target, PropertyInfo propInfo,
         FcDataFieldMirror dataFieldDef) {

         string propName = dataFieldDef.Name?.ToString() ?? propInfo.Name;
         ILuaVariant rawValue = data.Get(propName);

         if (rawValue == null) {
            if (dataFieldDef.Required) {
               throw new PrototypeUnpackException(path);
            } else {
               PopulateDataField(target, propInfo, null);
            }

         } else {
            object value = _dispatcher.Unpack(propInfo.PropertyType, rawValue, path); // XXX : Make Mirror() support null-s?
            PopulateDataField(target, propInfo, value);
         }
      }

      private void PopulateDataField(T target, PropertyInfo propInfo, object value) {
         propInfo.SetValue(target, value,
            BindingFlags.Public | BindingFlags.NonPublic, null, null, null);
      }
   }
}
