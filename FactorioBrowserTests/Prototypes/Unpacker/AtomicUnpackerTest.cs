using FactorioBrowser.Prototypes.Unpacker;
using NUnit.Framework;

namespace FactorioBrowserTests.Prototypes.Unpacker {

   [TestFixture]
   public class AtomicUnpackerTest {

      [Test]
      public void TestUnpackNull() {
         Assert.IsNull(Unpack<object>(null));
      }

      [Test]
      public void TestUnpackString() {
         string value = "value";
         string unpacked = Unpack<string>("value");
         Assert.AreEqual(value, unpacked);
      }

      [Test]
      public void TestUnpackInteger() {
         int value = 100;
         int unpacked = Unpack<int>(value);
         Assert.AreEqual(value, unpacked);
      }

      [Test]
      public void TestUnpackInt64() {
         long value = 100;
         long unpacked = Unpack<long>(value);
         Assert.AreEqual(value, unpacked);
      }

      [Test]
      public void TestUnpackFloatingPoint() {
         double value = 100.5;
         double unpacked = Unpack<double>(value);
         Assert.AreEqual(value, unpacked);
      }

      [TestCase(false)]
      [TestCase(true)]
      public void TestUnpackBoolean(bool value) {
         bool unpacked = Unpack<bool>(value);
         Assert.AreEqual(value, unpacked);
      }

      [Test]
      public void TestUnpackEmptyNullable() {
         int? unpacked = new AtomicValueUnpacker().Unpack<int?>(null, "root");
         Assert.AreEqual(false, unpacked.HasValue);
      }

      [Test]
      public void TestUnpackNullabkeWithValue() {
         int value = 100;
         int? unpacked = Unpack<int?>(value);
         Assert.IsTrue(unpacked.HasValue);
         Assert.AreEqual(value, unpacked.Value);
      }

      private T Unpack<T>(object data) {
         return new AtomicValueUnpacker().Unpack<T>(new ObjectLuaVariant(data), "root");
      }
   }
}
