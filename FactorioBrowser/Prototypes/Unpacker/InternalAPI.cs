using System;

namespace FactorioBrowser.Prototypes.Unpacker {

   /// <summary>
   /// Monomorphic unpacker from a Lua table to a single supported target type.
   /// </summary>
   /// <typeparam name="T">The target type this unpacker supports.</typeparam>
   internal interface ITypedUnpacker<out T> {
      T Unpack(ILuaTable data, string currentPath);
   }

   /// <summary>
   /// Polymorhpic unpacker from a Lua variant value to multiple target types.
   /// </summary>
   internal interface IVariantUnpacker {
      object Unpack(Type targetType, ILuaVariant data, string currentPath);
   }

   internal static class VariantUnpackerExtensions {
      public static T Unpack<T>(this IVariantUnpacker unpacker, ILuaVariant data, string path) {
         return (T) unpacker.Unpack(typeof(T), data, path);
      }
   }
}
