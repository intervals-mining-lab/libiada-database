namespace Libiada.Database.Tests.Extensions
{
    using Libiada.Database.Extensions;

    [TestFixture(TestOf = typeof(Libiada.Database.Extensions.CollectionExtensions))]
    public class CollectionExtensionsTests
    {
        [Test]
        public void TestArrayIsNullOrEmpty()
        {
            var collection = Array.Empty<int>();
            Assert.That(collection.IsNullOrEmpty());
            collection = new int[0];
            Assert.That(collection.IsNullOrEmpty());
            collection = null;
            Assert.That(collection.IsNullOrEmpty());
            collection = new int[1];
            Assert.That(collection.IsNullOrEmpty(), Is.False);
        }

        [Test]
        public void TestListIsNullOrEmpty()
        {
            var collection = new List<string?>();
            Assert.That(collection.IsNullOrEmpty());
            collection = new List<string?>(0);
            Assert.That(collection.IsNullOrEmpty());
            collection = null;
            Assert.That(collection.IsNullOrEmpty());
            collection = new List<string?>(1);
            Assert.That(collection.IsNullOrEmpty());
            collection = new List<string?>() { "" };
            Assert.That(collection.IsNullOrEmpty(), Is.False);
            collection = new List<string?>() { null };
            Assert.That(collection.IsNullOrEmpty(), Is.False);
        }

        [Test]
        public void TestSetIsNullOrEmpty()
        {
            var collection = new HashSet<string?>();
            Assert.That(collection.IsNullOrEmpty());
            collection = new HashSet<string?>(0);
            Assert.That(collection.IsNullOrEmpty());
            collection = null;
            Assert.That(collection.IsNullOrEmpty());
            collection = new HashSet<string?>(1);
            Assert.That(collection.IsNullOrEmpty());
            collection = new HashSet<string?>() { "" };
            Assert.That(collection.IsNullOrEmpty(), Is.False);
            collection = new HashSet<string?>() { null };
            Assert.That(collection.IsNullOrEmpty(), Is.False);
        }
    }
}
