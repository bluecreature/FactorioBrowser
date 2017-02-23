using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FactorioBrowser.Prototypes.Unpacker {
   internal sealed class UnpackerDispatcher {
      public T Unpack<T>(ILuaTable data, string path) {
         return (T) Unpack(typeof(T), data, path);
      }

      public T Unpack<T>(ILuaVariant data, String path) {
         Type targetType = typeof(T);
         return (T) Unpack(targetType, data, path);
      }

      internal object Unpack(Type targetType, ILuaTable data, string path) {
         Type mirrorImpl = GetUnpackerFor(targetType);
         ITableUnpacker<object> unpacker = (ITableUnpacker<object>)Activator.CreateInstance(mirrorImpl, this);
         return unpacker.Unpack(data, path);
      }

      internal object Unpack(Type targetType, ILuaVariant data, string path) {
         switch (data.ValueType) {
            case LuaValueType.String:
               return Unpack(targetType, data.AsTable, path);

            case LuaValueType.Table:
               return UnpackString(targetType, data, path);

            default:
               throw new NotImplementedException("Unsupported value type: " + data.ValueType);
         }
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
               throw  new NotImplementedException();
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
   }
}
