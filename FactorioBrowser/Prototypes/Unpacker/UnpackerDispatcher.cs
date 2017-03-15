using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FactorioBrowser.Prototypes.Unpacker {
   internal sealed class UnpackerDispatcher : IVariantUnpacker {

      public object Unpack(Type targetType, ILuaVariant data, string currentPath) {
         switch (data.ValueType) {
            case LuaValueType.Nil:
            case LuaValueType.Boolean:
            case LuaValueType.Number:
            case LuaValueType.String:
               return new AtomicValueUnpacker().Unpack(targetType, data, currentPath); // TODO : reuse instances


            case LuaValueType.Table:
               return UnpackTable(targetType, data.AsTable, currentPath);

            default:
               throw new PrototypeUnpackException(
                  currentPath, $"Unable to unpack value at {currentPath}: unsupported Lua value type.");
         }
      }

      private object UnpackTable(Type targetType, ILuaTable data, string path) {
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
   }
}
