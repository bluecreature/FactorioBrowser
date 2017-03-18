namespace FactorioBrowser.Prototypes {

   [ModelMirror]
   [TypeDiscriminatorField("type", "item")] // TODO
   public sealed class FcItem : FcEntity {
      [DataFieldMirror("icon")]
      public string Icon { get; private set; }
   }
}
