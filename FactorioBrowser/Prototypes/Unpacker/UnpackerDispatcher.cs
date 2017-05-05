using System;
using System.Collections.Generic;
using System.Linq;

namespace FactorioBrowser.Prototypes.Unpacker {

   internal sealed class UnpackerDispatcher : IVariantUnpacker {

      private static readonly Type[] AtomicTypes = {
         typeof(bool), typeof(int), typeof(double), typeof(string)
      };

      private readonly IVariantUnpacker _atomicValueUnpacker = new AtomicValueUnpacker();
      private readonly IDictionary<Type, ITableUnpacker<object>> _typedUnpackers =
         new Dictionary<Type, ITableUnpacker<object>>();

      public object Unpack(Type targetType, ILuaVariant data, string currentPath) {

         if (AtomicTypes.Contains(targetType)) {
            return _atomicValueUnpacker.Unpack(targetType, data, currentPath);
         }

         if (data != null && data.ValueType != LuaValueType.Table) {
            throw new PrototypeUnpackException(currentPath,
               $"Expected a table, but the value was {data.ValueType}");
         }

         ITableUnpacker<object> unpacker;
         if (TypeTools.IsStructureType(targetType)) {
            unpacker = _typedUnpackers.GetOrAdd(targetType,
               () => CreateStructUnpacker(targetType));

         } else if (targetType.IsGenericType) {
            Type unapckerClassTemplate;
            var targetTemplate = targetType.GetGenericTypeDefinition();
            if (targetTemplate == typeof(IDictionary<,>)) {
               unapckerClassTemplate = typeof(DictionaryUnpacker<,>);

            } else if (targetTemplate == typeof(IList<>)) {
               unapckerClassTemplate = typeof(ListUnpacker<>);

            } else {
               throw new NotImplementedException("Unable to unpack to " + targetType);
            }

            Type unpackerClass = unapckerClassTemplate.MakeGenericType(targetType.GenericTypeArguments);
            unpacker = _typedUnpackers.GetOrAdd(targetType,
               () => (ITableUnpacker<object>)Activator.CreateInstance(unpackerClass, this));

         } else {
            throw new PrototypeUnpackException(currentPath, "Unable to unpack to " + targetType);
         }

         return unpacker.Unpack(data?.AsTable, currentPath);
      }

      private ITableUnpacker<object> CreateStructUnpacker(Type structType) {
         var unpackerClass = typeof(StructureUnpacker<>).MakeGenericType(structType);
         return (ITableUnpacker<object>) Activator.CreateInstance(unpackerClass, this);
      }
   }
}
