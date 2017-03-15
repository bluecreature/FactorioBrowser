using System.Collections.Generic;

namespace FactorioBrowser.Prototypes.Unpacker {

   internal sealed class DictionaryUnpacker<TKey, TValue> : ITableUnpacker<IDictionary<TKey, TValue>> {
      private readonly IVariantUnpacker _dispatcher;

      public DictionaryUnpacker(IVariantUnpacker dispatcher) {
         _dispatcher = dispatcher;
      }

      public IDictionary<TKey, TValue> Unpack(ILuaTable data, string currentPath) {
         IDictionary<TKey, TValue> unpackedData = new Dictionary<TKey, TValue>();
         foreach (var entry in data.Entries()) {
            var key = _dispatcher.Unpack<TKey>(entry.Key, currentPath);
            var value = _dispatcher.Unpack<TValue>(entry.Value, currentPath + "." + key);
            unpackedData[key] = value;
         }

         return unpackedData;
      }
   }

   internal sealed class ListUnpacker<T> : ITableUnpacker<IList<T>> {
      private readonly IVariantUnpacker _dispatcher;

      public ListUnpacker(IVariantUnpacker dispatcher) {
         _dispatcher = dispatcher;
      }

      public IList<T> Unpack(ILuaTable data, string currentPath) {
         IList<T> mirror = new List<T>();
         int i = 1;
         ILuaVariant item;
         while ((item = data.Get(i)) != null) {
            T v = _dispatcher.Unpack<T>(item, currentPath + $"[{i}]");
            mirror.Add(v);
            i++;
         }

         return mirror;
      }
   }
}
