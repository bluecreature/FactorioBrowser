using System;
using System.Diagnostics;
using System.Reflection;
using System.Linq;

namespace FactorioBrowser.Prototypes.Unpacker {

   internal sealed class CustomUnpackerSupport: IVariantUnpacker {
      private readonly IVariantUnpacker _dispatcher;
      private readonly Type _containingType;
      private readonly CustomUnpacker _configuration;

      public CustomUnpackerSupport(IVariantUnpacker dispatcher, Type containingType,
         CustomUnpacker configuration) {

         _dispatcher = dispatcher;
         _containingType = containingType;
         _configuration = configuration;
      }

      public object Unpack(Type type, ILuaVariant data, string path) {
         var unpacker = FindCustomUnpackerMethod(type, _configuration.Unpacker);
         if (unpacker != null) {
            object[] convertParams = { _dispatcher, data, path };
            object result = unpacker.Invoke(null,
               BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
               null, convertParams, null);

            return result;
         } else {
            throw new PrototypeUnpackException(path,
               $"Internal error: unable to find compatible unpacker method {_configuration.Unpacker}");
         }
      }

      private MethodInfo FindCustomUnpackerMethod(Type returnType, string name) {
         return _containingType
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .FirstOrDefault(m => m.Name.Equals(name) && IsCompatibleUnpackerMethod(returnType, m));
      }

      private bool IsCompatibleUnpackerMethod(Type returnType, MethodInfo method) {
         var methodParams = method.GetParameters();
         return method.IsStatic &&
             returnType.IsAssignableFrom(method.ReturnType) &&
            methodParams.Length == 3 &&
            methodParams[0].ParameterType.IsAssignableFrom(typeof(IVariantUnpacker)) &&
            methodParams[1].ParameterType.IsAssignableFrom(typeof(ILuaVariant)) &&
            methodParams[2].ParameterType.IsAssignableFrom(typeof(string));
      }
   }


   internal sealed class StructureUnpacker<T> : ITableUnpacker<T> where T : class {
      private readonly IVariantUnpacker _dispatcher;

      public StructureUnpacker(IVariantUnpacker dispatcher) {
         _dispatcher = dispatcher;
      }

      public T Unpack(ILuaTable data, string path) {
         Debug.Assert(TypeTools.IsStructureType<T>());

         Type targetType = ResolveTargetType(typeof(T), data);

         if (targetType != null) {
            T target = (T) Activator.CreateInstance(targetType);
            foreach (var propInfo in targetType.GetProperties()) {
               DataFieldMirror dataFieldDef = propInfo.GetCustomAttribute<DataFieldMirror>();
               SelfMirror selfMirror = propInfo.GetCustomAttribute<SelfMirror>();
               if (dataFieldDef != null) {
                  HandleDataFieldMirror(data, path, target, propInfo, dataFieldDef);

               } else if (selfMirror != null) {
                  HandleSelfMirror(data, path, target, propInfo);
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
         return typeDiscriminator != null && DiscriminatorMatches(typeDiscriminator, data);
      }

      private bool DiscriminatorMatches(TypeDiscriminatorField attr, ILuaTable data) {
         var discFieldValue = data.Get(attr.FieldName);
         return discFieldValue != null &&
            discFieldValue.ValueType == LuaValueType.String &&
            attr.FieldValues.Contains(discFieldValue.ToString());
      }

      private void HandleDataFieldMirror(ILuaTable data, string path, T target,
         PropertyInfo propInfo, DataFieldMirror dataFieldDef) {

         string propName = dataFieldDef.Name?.ToString() ?? propInfo.Name;
         string propPath = path + "." + propName;
         ILuaVariant rawValue = data.Get(propName);
         var customUnpacker = propInfo.GetCustomAttribute<CustomUnpacker>();

         if (rawValue == null) {
            if (dataFieldDef.Required) {
               throw new PrototypeUnpackException(path);
            } else {
               PopulateDataField(target, propInfo, null);
            }

         } else if (customUnpacker != null) {
            HandleCustomUnpacker(target, rawValue, propPath, propInfo, customUnpacker);

         } else {
            object value = _dispatcher.Unpack(propInfo.PropertyType, rawValue, propPath); // XXX : support for null-s
            PopulateDataField(target, propInfo, value);
         }
      }

      private void HandleSelfMirror(ILuaTable data, string path, T target, PropertyInfo propInfo) {
         var customUnpacker = propInfo.GetCustomAttribute<CustomUnpacker>();
         if (customUnpacker == null) {
            throw new NotImplementedException(
               "SelfMirror without CustomUnpacker is not implemented.");
         }

         HandleCustomUnpacker(target, data.Self(), path, propInfo, customUnpacker);
      }

      private void HandleCustomUnpacker(T target, ILuaVariant data, string path,
         PropertyInfo propInfo, CustomUnpacker customUnpacker) {

         var unpacker = FindCustomUnpackerMethod(propInfo.PropertyType, customUnpacker.Unpacker);
         if (unpacker != null) {
            object[] convertParams = { _dispatcher, data, path };
            object result = unpacker.Invoke(null,
               BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static,
               null, convertParams, null);

            PopulateDataField(target, propInfo, result);

         } else {
            throw new PrototypeUnpackException(path,
               $"Internal error: unable to find compatible unpacker method {customUnpacker.Unpacker}");
         }
      }

      private MethodInfo FindCustomUnpackerMethod(Type returnType, string name) {
         return typeof(T)
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .FirstOrDefault(m => m.Name.Equals(name) && IsCompatibleUnpackerMethod(returnType, m));
      }

      private bool IsCompatibleUnpackerMethod(Type returnType, MethodInfo method) {
         var methodParams = method.GetParameters();
         return method.IsStatic &&
             returnType.IsAssignableFrom(method.ReturnType) &&
            methodParams.Length == 3 &&
            methodParams[0].ParameterType.IsAssignableFrom(typeof(IVariantUnpacker)) &&
            methodParams[1].ParameterType.IsAssignableFrom(typeof(ILuaVariant)) &&
            methodParams[2].ParameterType.IsAssignableFrom(typeof(string));
      }

      private void PopulateDataField(T target, PropertyInfo propInfo, object value) {
         propInfo.SetValue(target, value,
            BindingFlags.Public | BindingFlags.NonPublic, null, null, null);
      }
   }
}
