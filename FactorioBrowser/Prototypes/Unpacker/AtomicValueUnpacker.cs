using System;
using System.Diagnostics;

namespace FactorioBrowser.Prototypes.Unpacker {

   internal sealed class AtomicValueUnpacker : IVariantUnpacker {
      public object Unpack(Type targetType, ILuaVariant data, string currentPath) {
         if (data == null || data.ValueType == LuaValueType.Nil) {
            return null;  // TODO : check if targetType allows null-s

         } else {
            targetType = StripNullable(targetType);
            switch (data.ValueType) {
               case LuaValueType.Boolean:
                  return UnpackBoolean(targetType, data, currentPath);

               case LuaValueType.Number:
                  return UnpackNumber(targetType, data, currentPath);

               case LuaValueType.String:
                  return UnpackString(targetType, data, currentPath);

               default:
                  throw new PrototypeUnpackException(currentPath,
                     "Atomic value (a boolean, a number, or a string) expected, but it was " + data.ValueType);
            }
         }
      }

      private object UnpackBoolean(Type targetType, ILuaVariant data, string path) {
         Debug.Assert(data.ValueType == LuaValueType.Boolean);

         if (targetType == typeof(bool)) {
            return data.AsBoolean;

         } else {
            throw new PrototypeUnpackException(path); // TODO : message
         }
      }

      private object UnpackNumber(Type targetType, ILuaVariant data, string path) {
         Debug.Assert(data.ValueType == LuaValueType.Number);

         if (targetType == typeof(int)) {
            return Convert.ToInt32(data.AsNumber);

         } else if (targetType == typeof(long)) {
            return Convert.ToInt64(data.AsNumber);

         } else if (targetType == typeof(double)) {
            return data.AsNumber;

         } else {
            throw new PrototypeUnpackException(path); // TODO : message
         }
      }

      private object UnpackString(Type targetType, ILuaVariant data, string path) {
         Debug.Assert(data.ValueType == LuaValueType.String);
         var value = data.AsString;

         if (targetType == typeof(string)) {
            return value;

         } else if (targetType == typeof(bool)) {
            bool result;
            if (!bool.TryParse(value, out result)) {
               throw new PrototypeUnpackException(path, $"Cannot coerce string `{value}' to boolean.");
            }

            return result;

         } else {
            throw new PrototypeUnpackException(path, $"Cannot coerce string to {targetType}");
         }
      }

      private Type StripNullable(Type targetType) {
         if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
            return targetType.GetGenericArguments()[0];
         } else {
            return targetType;
         }
      }

   }
}
