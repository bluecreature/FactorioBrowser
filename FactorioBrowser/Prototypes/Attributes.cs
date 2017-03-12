using System;

namespace FactorioBrowser.Prototypes {

   [AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
   public sealed class ModelMirror : Attribute {
   }

   [AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
   public sealed class TypeDiscriminatorField : Attribute {
      public string FieldName { get; set; }

      public string[] FieldValues { get; set; }

      public TypeDiscriminatorField(string fieldName, params string[] fieldValues) {
         FieldName = fieldName;
         FieldValues = fieldValues;
      }
   }

   [AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
   public sealed class DataFieldMirror : Attribute {
      public object Name { get; set; }

      public bool Required { get; set; }

      public DataFieldMirror(object name) {
         Name = name;
      }

      public DataFieldMirror() {
         Name = null;
      }
   }

   [AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
   public sealed class SelfMirror : Attribute {
   }

   [AttributeUsage(validOn: AttributeTargets.Property | AttributeTargets.Parameter,
      AllowMultiple = false, Inherited = false)]
   public sealed class CustomUnpacker : Attribute {
      public string Unpacker { get; set; }

      public CustomUnpacker() {
         Unpacker = null;
      }

      public CustomUnpacker(string unpacker) {
         Unpacker = unpacker;
      }
   }
}
