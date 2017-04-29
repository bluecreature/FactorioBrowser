using System;
using System.Linq;
using System.Reflection;

namespace FactorioBrowser.Prototypes.Unpacker {

   internal static class TypeTools {
      internal static bool IsStructureType<T>() {
         return IsStructureType(typeof(T));
      }

      internal static bool IsStructureType(Type typeId) {
         return typeId.IsClass &&
            typeId.GetCustomAttributes(typeof(ModelMirror)).FirstOrDefault() != null;
      }
   }
}
