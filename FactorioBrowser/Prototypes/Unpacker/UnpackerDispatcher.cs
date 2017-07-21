using System;
using System.Collections.Generic;
using System.Linq;

namespace FactorioBrowser.Prototypes.Unpacker {

   internal sealed class UnpackerDispatcher : IVariantUnpacker {

      private static readonly Type[] AtomicTypes = {
         typeof(bool), typeof(int), typeof(long), typeof(double), typeof(string),
         typeof(bool?), typeof(int?), typeof(long?), typeof(double?)
      };

      private readonly IVariantUnpacker _atomicValueUnpacker;
      private readonly IDictionary<Type, ITypedUnpacker<object>> _typedUnpackers =
         new Dictionary<Type, ITypedUnpacker<object>>();

      public UnpackerDispatcher() {
         _atomicValueUnpacker = new AtomicValueUnpacker();
      }

      public object Unpack(Type targetType, ILuaVariant data, string currentPath) {

         if (AtomicTypes.Contains(targetType)) {
            return _atomicValueUnpacker.Unpack(targetType, data, currentPath);
         }

         ILuaTable dataAsTable;
         if (data == null || data.ValueType == LuaValueType.Nil) {
            dataAsTable = null;
         } else if (data.ValueType == LuaValueType.Table) {
            dataAsTable = data.AsTable;

         } else {
            throw new PrototypeUnpackException(currentPath,
               $"Expected a table or Nil, but the value was of type {data.ValueType}");
         }

         ITypedUnpacker<object> unpacker = _typedUnpackers.GetOrAdd(targetType,
            () => CreateTypedUnpacker(targetType, currentPath));
         return unpacker.Unpack(dataAsTable, currentPath);
      }

      private ITypedUnpacker<object> CreateTypedUnpacker(Type targetType, string currentPath) {
         ITypedUnpacker<object> unpacker;
         if (TypeTools.IsStructureType(targetType)) {
            unpacker = CreateStructUnpacker(targetType);

         } else if (TypeIsAnyList(targetType)) {
            unpacker = CreateListUnpacker(targetType);

         } else if (TypeIsAnyDict(targetType)) {
            unpacker = CreateDictUnpacker(targetType);

         } else {
            throw new PrototypeUnpackException(currentPath,
               "Unable to unpack to unsupported target type: " + targetType);
         }

         return unpacker;
      }

      private ITypedUnpacker<object> CreateListUnpacker(Type targetType) {
         var unpackerType = typeof(ListUnpacker<>).MakeGenericType(targetType.GenericTypeArguments);
         return (ITypedUnpacker<object>) Activator.CreateInstance(unpackerType, this);
      }

      private ITypedUnpacker<object> CreateDictUnpacker(Type targetType) {
         var unpackerType = typeof(DictionaryUnpacker<,>).MakeGenericType(targetType.GenericTypeArguments);
         return (ITypedUnpacker<object>)Activator.CreateInstance(unpackerType, this);
      }

      private ITypedUnpacker<object> CreateStructUnpacker(Type structType) {
         var unpackerClass = typeof(StructureUnpacker<>).MakeGenericType(structType);
         return (ITypedUnpacker<object>) Activator.CreateInstance(unpackerClass, this);
      }

      private bool TypeIsAnyList(Type type) {
         return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IList<>);
      }

      private bool TypeIsAnyDict(Type type) {
         return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>);
      }
   }
}
