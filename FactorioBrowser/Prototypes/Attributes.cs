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

   [AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false)]
   public sealed class DataFieldMirror : Attribute {
      public object Name { get; set; }

      public bool Required { get; set; }

      public DataFieldMirror(object name) {
         this.Name = name;
      }

      public DataFieldMirror() {
         this.Name = null;
      }
   }
}
