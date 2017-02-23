using System;
using System.Linq;
using System.Reflection;
using FactorioBrowser.Mirror;

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

   public sealed class FcPrototypeDataUnpacker {
      public static T UnpackAs<T>(ILuaTable data, string rootPath) {
         return new UnpackerDispatcher().Unpack<T>(data, rootPath);
      }
   }

   internal sealed class TypeTools {
      internal static bool IsStructureType<T>() {
         return IsStructureType(typeof(T));
      }

      internal static bool IsStructureType(Type typeId) {
         return typeId.IsClass &&
            typeId.GetCustomAttributes(typeof(FcModelMirror)).FirstOrDefault() != null;
      }
   }
}
