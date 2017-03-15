using System;
using System.Linq;
using System.Reflection;

namespace FactorioBrowser.Prototypes.Unpacker {

   public class PrototypeUnpackException : Exception {
      public string Path { get; }

      public PrototypeUnpackException(string path) : base() {
         Path = path;
      }

      public PrototypeUnpackException(string path, string message) : base(message) {
         Path = path;
      }

      public PrototypeUnpackException(string path, string message, Exception innerException) : base(message, innerException) {
         Path = path;
      }
   }

   public static class FcPrototypeDataUnpacker {
      public static T UnpackAs<T>(ILuaVariant data, string rootPath) {
         return new UnpackerDispatcher().Unpack<T>(data, rootPath);
      }
   }

   internal static class TypeTools { // TODO : move away from public API file
      internal static bool IsStructureType<T>() {
         return IsStructureType(typeof(T));
      }

      internal static bool IsStructureType(Type typeId) {
         return typeId.IsClass &&
            typeId.GetCustomAttributes(typeof(ModelMirror)).FirstOrDefault() != null;
      }
   }
}
