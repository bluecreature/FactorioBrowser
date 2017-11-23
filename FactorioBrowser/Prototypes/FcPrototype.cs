using System.Collections.Immutable;

namespace FactorioBrowser.Prototypes {

   [ModelMirror]
   public abstract class FcPrototype {

      [DataFieldMirror("type")]
      public string Type { get; private set; }

      [DataFieldMirror("name")]
      public string Name { get; private set; }

      [DataFieldMirror("__mod__")]
      public string SourceMod { get; private set; } = "base";
   }
}
