using System;
using System.Diagnostics;

namespace FactorioBrowser.Prototypes.Unpacker {
   internal sealed class AtomicValueUnpacker : IVariantUnpacker {
      public object Unpack(Type targetType, ILuaVariant data, string path) {
         switch (data.ValueType) {
            case LuaValueType.Nil:
               return null; // TODO : check if targetType allows null-s

            case LuaValueType.Boolean:
               return UnpackBoolean(targetType, data, path);

            case LuaValueType.Number:
               return UnpackNumber(targetType, data, path);

            case LuaValueType.String:
               return UnpackString(targetType, data, path);

            default:
               throw new InvalidOperationException("This class only supports atomic values.");
         }
      }

      private object UnpackBoolean(Type targetType, ILuaVariant data, string path) {
         Debug.Assert(data.ValueType == LuaValueType.Boolean);

         if (targetType == typeof(bool) || targetType == typeof(Boolean)) {
            return data.AsBoolean;

         } else {
            throw new PrototypeUnpackException(path); // TODO : message
         }
      }

      private object UnpackNumber(Type targetType, ILuaVariant data, string path) {
         Debug.Assert(data.ValueType == LuaValueType.Number);

         if (targetType == typeof(Int32) || targetType == typeof(int)) {
            return Convert.ToInt32(data.AsNumber);

         } else if (targetType == typeof(double)) {
            return data.AsNumber;

         } else {
            throw new PrototypeUnpackException(path); // TODO : message
         }
      }

      private object UnpackString(Type targetType, ILuaVariant data, string path) {
         Debug.Assert(data.ValueType == LuaValueType.String);

         if (targetType != typeof(string)) {
            throw new PrototypeUnpackException(path); // TODO : message
         }

         return data.AsString;
      }
   }
}
