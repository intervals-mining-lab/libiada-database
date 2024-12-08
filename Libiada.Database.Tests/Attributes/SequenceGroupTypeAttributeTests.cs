namespace Libiada.Database.Tests.Attributes;

using Libiada.Core.Extensions;

using Libiada.Database.Attributes;

/// <summary>
/// The sequence group type attribute tests.
/// </summary>
[TestFixture(TestOf = typeof(SequenceGroupTypeAttribute))]
public class SequenceGroupTypeAttributeTests
{
    /// <summary>
    /// Invalid sequence group type value test.
    /// </summary>
    [Test]
    public void InvalidSequenceGroupTypeValueTest()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() => new SequenceGroupTypeAttribute((SequenceGroupType)6));
            Assert.Throws<ArgumentException>(() => new SequenceGroupTypeAttribute((SequenceGroupType)0));
        });

    }

    /// <summary>
    /// Sequence group type attribute value test.
    /// </summary>
    /// <param name="value">
    /// The value.
    /// </param>
    [Test]
    public void SequenceGroupTypeAttributeValueTest([Values] SequenceGroupType value)
    {
        SequenceGroupTypeAttribute attribute = new(value);
        Assert.Multiple(() =>
        {
            Assert.That(attribute.Value, Is.EqualTo(value));
            Assert.That(attribute.Value.GetDisplayValue(), Is.EqualTo(value.GetDisplayValue()));
        });
    }
}
