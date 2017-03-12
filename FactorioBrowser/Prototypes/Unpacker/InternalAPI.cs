using System;

namespace FactorioBrowser.Prototypes.Unpacker {

   internal interface ITableUnpacker<out T> {
      T Unpack(ILuaTable data, string path);
   }

   internal interface IVariantUnpacker {
      object Unpack(Type targetType, ILuaVariant data, string path);
   }

   internal static class VariantUnpackerExtensions {
      public static T Unpack<T>(this IVariantUnpacker unpacker, ILuaVariant data, string path) {
         return (T) unpacker.Unpack(typeof(T), data, path);
      }
   }
}
