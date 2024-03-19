namespace Libiada.Database.Tests.Extensions;

using Libiada.Database.Extensions;

[TestFixture(TestOf = typeof(CollectionExtensions))]
public class CollectionExtensionsTests
{
    [Test]
    public void TestArrayIsNullOrEmpty()
    {
        int[]? collection = Array.Empty<int>();
        Assert.That(collection.IsNullOrEmpty());
        collection = [];
        Assert.That(collection.IsNullOrEmpty());
        collection = null;
        Assert.That(collection.IsNullOrEmpty());
        collection = new int[1];
        Assert.That(collection.IsNullOrEmpty(), Is.False);
    }

    [Test]
    public void TestListIsNullOrEmpty()
    {
        List<string?>? collection = [];
        Assert.That(collection.IsNullOrEmpty());
        collection = null;
        Assert.That(collection.IsNullOrEmpty());
        collection = new List<string?>(1);
        Assert.That(collection.IsNullOrEmpty());
        collection = [""];
        Assert.That(collection.IsNullOrEmpty(), Is.False);
        collection = [null];
        Assert.That(collection.IsNullOrEmpty(), Is.False);
    }

    [Test]
    public void TestSetIsNullOrEmpty()
    {
        HashSet<string?>? collection = [];
        Assert.That(collection.IsNullOrEmpty());
        collection = null;
        Assert.That(collection.IsNullOrEmpty());
        collection = new HashSet<string?>(1);
        Assert.That(collection.IsNullOrEmpty());
        collection = [""];
        Assert.That(collection.IsNullOrEmpty(), Is.False);
        collection = [null];
        Assert.That(collection.IsNullOrEmpty(), Is.False);
    }
}
