namespace Libiada.Database.Tests.Models.CalculatorsData;

using Libiada.Database.Models.CalculatorsData;

[TestFixture(TestOf = typeof(AttributeValue))]
public class AttributeValueTests
{
    [Test]
    public void AttributeValueConstructorTest()
    {
        AttributeValue attributeValue = new(1, "gene");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(attributeValue.AttributeId, Is.EqualTo(1));
            Assert.That(attributeValue.Value, Is.EqualTo("gene"));
        }
    }

    [Test]
    public void AttributeValueReloadedOperationsTests()
    {
        AttributeValue first = new(2, "CDS");
        AttributeValue second = new(2, "CDS");
        AttributeValue third = new(2, "gene");
        AttributeValue fourth = new(3, "CDS");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(first == second, Is.True);
            Assert.That(first != second, Is.False);
            Assert.That(first == third, Is.False);
            Assert.That(first != third, Is.True);
            Assert.That(first == fourth, Is.False);
            Assert.That(first != fourth, Is.True);
            Assert.That(first.Equals(second), Is.True);
            Assert.That(first.Equals(third), Is.False);
            Assert.That(first.Equals(fourth), Is.False);
            Assert.That(first.Equals((object)second), Is.True);
            Assert.That(first.Equals((object)third), Is.False);
            Assert.That(first.Equals((object)fourth), Is.False);
        }
    }

    [Test]
    public void AttributeValueGetHashCodeTest()
    {
        AttributeValue first = new(2, "CDS");
        AttributeValue second = new(2, "CDS");
        AttributeValue third = new(2, "gene");
        AttributeValue fourth = new(3, "CDS");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
            Assert.That(first.GetHashCode(), Is.Not.EqualTo(third.GetHashCode()));
            Assert.That(first.GetHashCode(), Is.Not.EqualTo(fourth.GetHashCode()));
            Assert.That(third.GetHashCode(), Is.Not.EqualTo(fourth.GetHashCode()));
        }
    }
}
