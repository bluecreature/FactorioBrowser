namespace FactorioBrowser.Prototypes {

   [ModelMirror]
   public abstract class FcPrototype {

      [DataFieldMirror("type")]
      public string Type { get; private set; }

      [DataFieldMirror("name")]
      public string Name { get; private set; }
   }
}
