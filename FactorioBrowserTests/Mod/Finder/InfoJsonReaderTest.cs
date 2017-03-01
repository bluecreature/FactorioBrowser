using System.IO;
using System.Text;
using FactorioBrowser;
using FactorioBrowser.Mod.Finder;
using NUnit.Framework;

namespace FactorioBrowserTests.Mod.Finder {

   [TestFixture]
   public class InfoJsonReaderTest {

      private const string SimpleDepName = "base";


      [Test]
      public void TestReadBasicFields() {
         var info = Load(@"{
            ""name"": ""testName"",
            ""version"": ""0.0.1"",
            ""dependencies"": []
         }");

         Assert.AreEqual("testName", info.Name);
         Assert.AreEqual("0.0.1", info.Version);
         Assert.IsEmpty(info.Dependencies);
      }

      [Test]
      public void TestDependencyVersionRange() {
         var dep = LoadDep($"{SimpleDepName} > 1.2.3");
         Assert.AreEqual("base", dep.ModName);
         Assert.NotNull(dep.RequiredVersion);
         Assert.IsFalse(dep.Optional);
         Assert.AreEqual(FcVersionRange.After, dep.RequiredVersion.Range);
         Assert.AreEqual(new FcVersion(1, 2, 3), dep.RequiredVersion.Version);
      }

      [Test]
      public void TestOptionalDependency() {
         var dep = LoadDep($"? {SimpleDepName} = 1.2.3");
         Assert.AreEqual(SimpleDepName, dep.ModName);
         Assert.AreEqual(FcVersionRange.Exactly, dep.RequiredVersion.Range);
         Assert.IsTrue(dep.Optional);
      }

      [Test]
      public void TestVersionlessDependency() {
         var dep = LoadDep(SimpleDepName);
         Assert.AreEqual(SimpleDepName, dep.ModName);
         Assert.IsNull(dep.RequiredVersion);
         Assert.IsFalse(dep.Optional);
      }

      [Test]
      public void TestVersionlessOptionalDependency() {
         var dep = LoadDep($"?{SimpleDepName}");
         Assert.AreEqual(dep.ModName, SimpleDepName);
      }

      [Test]
      public void TestDepNameSpecialChars() {
         var depName = "A B_C-D1";
         var dep = LoadDep($"{depName} >= 5.0.0");
         Assert.AreEqual(depName, dep.ModName);
      }

      private FcModDependency LoadDep(string depSpec) {
         var info = Load($@"{{
            ""name"": ""testName"",
            ""version"": ""0.0.1"",
            ""dependencies"": [""{depSpec}""]
         }}");

         Assert.AreEqual(1, info.Dependencies.Length);
         return info.Dependencies[0];
      }

      private InfoJson Load(string infoJson) {
         using (var stream = new StreamReader(
            new MemoryStream(Encoding.UTF8.GetBytes(infoJson)))) {

            return new InfoJsonReader("test", stream).Read();
         }
      }
   }
}
