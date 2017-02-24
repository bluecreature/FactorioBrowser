using System.Collections.Generic;

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
}
