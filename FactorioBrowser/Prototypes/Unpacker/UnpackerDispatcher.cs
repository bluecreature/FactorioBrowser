using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FactorioBrowser.Prototypes.Unpacker {
   internal sealed class UnpackerDispatcher : VariantUnpacker {

      public override object Unpack(Type targetType, ILuaVariant data, string path) {
         switch (data.ValueType) {

            case LuaValueType.Nil:
               return null;

            case LuaValueType.Boolean:
               return false; // TODO

            case LuaValueType.Number:
               return UnpackNumber(targetType, data, path);

            case LuaValueType.String:
               return UnpackString(targetType, data, path);

            case LuaValueType.Table:
               return Unpack(targetType, data.AsTable, path);

            default:
               throw new PrototypeUnpackException(
                  path, $"Unable to unpack value at {path}: unsupported Lua value type.");
         }
      }

      private object Unpack(Type targetType, ILuaTable data, string path) {
         Type mirrorImpl = GetUnpackerFor(targetType);
         ITableUnpacker<object> unpacker = (ITableUnpacker<object>)Activator.CreateInstance(mirrorImpl, this);
         return unpacker.Unpack(data, path);
      }

      private Type GetUnpackerFor(Type targetType) {
         Type unpackerImpl;
         if (targetType.IsGenericType) {
            Type genTarget = targetType.GetGenericTypeDefinition();
            if (genTarget == typeof(IDictionary<,>)) {
               unpackerImpl = typeof(DictionaryUnpacker<,>);

            } else if (genTarget == typeof(IList<>)) {
               unpackerImpl = typeof(ListUnpacker<>);

            } else {
               throw new NotImplementedException();
            }

         } else if (TypeTools.IsStructureType(targetType)) {
            unpackerImpl = typeof(StructureUnpacker<>).MakeGenericType(targetType);

         } else {
            throw new NotImplementedException();
         }

         if (targetType.IsGenericType) {
            Debug.Assert(unpackerImpl.IsGenericType);
            unpackerImpl = unpackerImpl.MakeGenericType(targetType.GenericTypeArguments);
         }

         return unpackerImpl;
      }

      private object UnpackString(Type targetType, ILuaVariant data, string path) {
         if (targetType != typeof(string)) {
            throw new PrototypeUnpackException(path);
         }

         return data.AsString;
      }
      private object UnpackNumber(Type targetType, ILuaVariant data, string path) {
         if (targetType == typeof(Int32) || targetType == typeof(int)) {
            return Convert.ToInt32(data.AsNumber);

         } else if (targetType == typeof(double)) {
            return data.AsNumber;

         } else {
            throw new PrototypeUnpackException(path);
         }
      }
   }
}
