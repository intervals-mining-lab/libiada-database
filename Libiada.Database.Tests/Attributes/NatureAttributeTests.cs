namespace Libiada.Database.Tests.Attributes;

using Libiada.Core.Extensions;

using Libiada.Database.Attributes;

/// <summary>
/// The nature attribute tests.
/// </summary>
[TestFixture(TestOf = typeof(NatureAttribute))]
public class NatureAttributeTests
{
    /// <summary>
    /// Invalid nature value test.
    /// </summary>
    [Test]
    public void InvalidNatureValueTest()
    {
        Assert.Throws<ArgumentException>(() => new NatureAttribute((Nature)10));
        Assert.Throws<ArgumentException>(() => new NatureAttribute((Nature)0));
    }

    /// <summary>
    /// Nature attribute value test.
    /// </summary>
    /// <param name="value">
    /// The value.
    /// </param>
    [Test]
    public void NatureAttributeValueTest([Values] Nature value)
    {
        var attribute = new NatureAttribute(value);
        Assert.That(attribute.Value, Is.EqualTo(value));
        Assert.That(attribute.Value.GetDisplayValue(), Is.EqualTo(value.GetDisplayValue()));
    }
}
