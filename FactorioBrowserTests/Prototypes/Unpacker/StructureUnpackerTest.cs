using System.Collections.Generic;
using FactorioBrowser.Prototypes;
using FactorioBrowser.Prototypes.Unpacker;
using NUnit.Framework;

namespace FactorioBrowserTests.Prototypes.Unpacker {

   [ModelMirror]
   internal sealed class StructA {

      [DataFieldMirror("f1")]
      public int FieldOne { get; private set; }


      public StructA() {
         FieldOne = default(int);
      }

      public StructA(int fieldOne) {
         FieldOne =  fieldOne;
      }
   }

   [ModelMirror]
   internal sealed class StructB {

      [DataFieldMirror("f2")]
      public StructA Nested { get; private set; }
   }

   [ModelMirror]
   internal sealed class StructC {

      [DataFieldMirror("custom")]
      [CustomUnpacker(nameof(CustomNestedUnpacker))]
      public StructA A { get; private set; }

      private static StructA CustomNestedUnpacker(IVariantUnpacker unpacker, ILuaVariant data,
         string path) {

         Assert.AreEqual(StructureUnpackerTest.TestFieldValue, data.AsTable.Get("f1").AsNumber);
         return new StructA(StructureUnpackerTest.CustomFieldValue);
      }
   }

   [ModelMirror]
   internal sealed class StructD {

      [SelfMirror]
      [CustomUnpacker("CustomSelfUnpacker")]
      public StructA A { get; private set; }

      [DataFieldMirror("f2")]
      public int FieldTwo { get; private set; }


      public static StructA CustomSelfUnpacker(IVariantUnpacker unpacker, ILuaVariant data,
         string path) {

         Assert.AreEqual(StructureUnpackerTest.TestFieldValue, data.AsTable.Get("f2").AsNumber);
         return new StructA(StructureUnpackerTest.CustomFieldValue);
      }
   }

   [TestFixture]
   public class StructureUnpackerTest {
      public const int TestFieldValue = 100;
      public const int CustomFieldValue = 105;


      [Test]
      public void TestUnpackConcreteClass() {
         var unpacked = Unpack<StructA>(new Dictionary<object, object> {
            ["f1"] = TestFieldValue,
         });

         Assert.AreEqual(TestFieldValue, unpacked.FieldOne);
      }

      [Test]
      public void TestUnpackNestedClass() {
         var unpacked = Unpack<StructB>(new Dictionary<object, object> {
            ["f2"] = new Dictionary<object, object>() {
               ["f1"] = TestFieldValue,
            },
         });

         Assert.AreEqual(TestFieldValue, unpacked.Nested.FieldOne);
      }

      [Test]
      public void TestCustomUnpacker() {
         var unpacked = Unpack<StructC>(new Dictionary<object, object> {
            ["custom"] = new Dictionary<object, object> {
               ["f1"] = TestFieldValue
            }
         });

         Assert.AreEqual(CustomFieldValue, unpacked.A.FieldOne);
      }

      [Test]
      public void TestCustomUnpackSelf() {
         var unpacked = Unpack<StructD>(new Dictionary<object, object> {
            ["f2"] = TestFieldValue
         });

         Assert.AreEqual(CustomFieldValue, unpacked.A.FieldOne);
         Assert.AreEqual(TestFieldValue, unpacked.FieldTwo);
      }

      private T Unpack<T>(IDictionary<object, object> table) where T : class {
         return new StructureUnpacker<T>(new UnpackerDispatcher())
            .Unpack(new DictionaryLuaTable(table), "root");
      }
   }
}
