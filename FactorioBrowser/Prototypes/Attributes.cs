using System;

namespace FactorioBrowser.Mirror {

   [AttributeUsage(validOn: AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
   public class FcModelMirror : Attribute {
   }

   [AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false)]
   public class FcDataFieldMirror : Attribute {
      public object Name { get; set; }

      public bool Required { get; set; }

      public FcDataFieldMirror(object name) {
         this.Name = name;
      }

      public FcDataFieldMirror() {
         this.Name = null;
      }
   }
}
