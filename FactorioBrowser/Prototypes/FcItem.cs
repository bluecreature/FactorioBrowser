namespace FactorioBrowser.Prototypes {

   [ModelMirror]
   [TypeDiscriminatorField("type", "item", "fluid")]
   public sealed class FcItem : FcPrototype {

      [DataFieldMirror("icon")]
      public string Icon { get; private set; }
   }
}
