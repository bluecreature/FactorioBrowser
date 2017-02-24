using System;

namespace FactorioBrowser.Prototypes.Unpacker {

   internal interface ITableUnpacker<out T> {
      T Unpack(ILuaTable data, string path);
   }

   internal abstract class VariantUnpacker {
      public abstract object Unpack(Type targetType, ILuaVariant data, string path);

      public T Unpack<T>(ILuaVariant data, string path) {
         // TODO : see if it can be made an extension method
         return (T) Unpack(typeof(T), data, path);
      }
   }
}
