using System;
using System.Diagnostics;
using System.Reflection;
using System.Linq;

namespace FactorioBrowser.Prototypes.Unpacker {
   internal sealed class StructureUnpacker<T> : ITableUnpacker<T> where T : class {
      private readonly UnpackerDispatcher _dispatcher;

      public StructureUnpacker(UnpackerDispatcher dispatcher) {
         _dispatcher = dispatcher;
      }

      public T Unpack(ILuaTable data, string path) {
         Debug.Assert(TypeTools.IsStructureType<T>());

         Type targetType = ResolveTargetType(typeof(T), data);

         if (targetType != null) {
            T target = (T) Activator.CreateInstance(targetType);
            foreach (var propInfo in targetType.GetProperties()) {
               DataFieldMirror dataFieldDef = propInfo.GetCustomAttribute<DataFieldMirror>();
               if (dataFieldDef != null) {
                  PopulateDataField(data, path, target, propInfo, dataFieldDef);
               }
            }

            return target;

         } else {
            return null;
         }
      }

      private Type ResolveTargetType(Type declaredType, ILuaTable data) {
         if (declaredType.IsAbstract) {
            return SelectPolymorphicTarget(declaredType, data);
         } else {
            return declaredType;
         }
      }

      private Type SelectPolymorphicTarget(Type baseType, ILuaTable data) {
         var candidate = baseType.Assembly.GetTypes()
            .FirstOrDefault(t => TypeHasMatchingDiscriminator(t, data));
         return candidate;
      }

      private bool TypeHasMatchingDiscriminator(Type type, ILuaTable data) {
         Debug.Assert(type.GetCustomAttributes<TypeDiscriminatorField>().Count() <= 1);
         TypeDiscriminatorField typeDiscriminator = type.GetCustomAttribute<TypeDiscriminatorField>();
         return typeDiscriminator != null && TypeDiscriminatorMatches(typeDiscriminator, data);
      }

      private bool TypeDiscriminatorMatches(TypeDiscriminatorField attr, ILuaTable data) {
         var discFieldValue = data.Get(attr.FieldName);
         return discFieldValue != null &&
            discFieldValue.ValueType == LuaValueType.String &&
            attr.FieldValues.Contains(discFieldValue.ToString());
      }

      private void PopulateDataField(ILuaTable data, string path, T target, PropertyInfo propInfo,
         DataFieldMirror dataFieldDef) {

         string propName = dataFieldDef.Name?.ToString() ?? propInfo.Name;
         ILuaVariant rawValue = data.Get(propName);

         if (rawValue == null) {
            if (dataFieldDef.Required) {
               throw new PrototypeUnpackException(path);
            } else {
               PopulateDataField(target, propInfo, null);
            }

         } else {
            object value = _dispatcher.Unpack(propInfo.PropertyType, rawValue, path); // XXX : support for null-s
            PopulateDataField(target, propInfo, value);
         }
      }

      private void PopulateDataField(T target, PropertyInfo propInfo, object value) {
         propInfo.SetValue(target, value,
            BindingFlags.Public | BindingFlags.NonPublic, null, null, null);
      }
   }
}
