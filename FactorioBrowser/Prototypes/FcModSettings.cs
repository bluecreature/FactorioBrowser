using System.Collections.Generic;

namespace FactorioBrowser.Prototypes {

   abstract class FcModSetting<TValueType> : FcPrototype {

      [DataFieldMirror(name: "setting_type", required: true)]
      public string SettingType { get; private set; }

      [DataFieldMirror(name: "default_value", required: true)]
      public TValueType DefaultValue { get; private set; }
   }

   [ModelMirror]
   [TypeDiscriminatorField(fieldName: "type", fieldValues: "bool-setting")]
   sealed class FcBooleanSetting : FcModSetting<bool> {
   }

   abstract class FcNumericSetting<TValueType> : FcModSetting<TValueType> {

      [DataFieldMirror(name: "minimum_value", required: false)]
      public TValueType Minimum { get; private set; }

      [DataFieldMirror(name: "maximum_value", required: false)]
      public TValueType Maximum { get; private set; }

      [DataFieldMirror(name: "allowed_values", required: false)]
      public IList<TValueType> AllowedValues { get; private set; }
   }

   [ModelMirror]
   [TypeDiscriminatorField(fieldName: "type", fieldValues: "int-setting")]
   sealed class FcIntegerSetting : FcModSetting<int> {
   }

   [ModelMirror]
   [TypeDiscriminatorField(fieldName: "type", fieldValues: "double-setting")]
   sealed class FcDoubleSetting : FcModSetting<double> {
   }

   [ModelMirror]
   [TypeDiscriminatorField(fieldName: "type", fieldValues: "string-setting")]
   sealed class FcStringSetting : FcModSetting<string> {

      [DataFieldMirror(name: "allow_blank", required: false)]
      public bool AllowBlank { get; private set; } = false;

      [DataFieldMirror(name: "allowed_values", required: false)]
      public IList<string> AllowedValues { get; private set; }
   }
}
