namespace Libiada.Database.Tests.Extensions;

using Libiada.Database.Attributes;
using Libiada.Database.Extensions;

[TestFixture(TestOf = typeof(EnumExtensions))]
public class EnumExtensionsTests
{
    /// <summary>
    /// Sample enum for testing.
    /// </summary>
    private enum TestEnum : byte
    {
        [Nature(Nature.Music)]
        First = 1,

        [Nature(Nature.Genetic)]
        Second = 2,

        Third = 3
    }

    /// <summary>
    /// Test for <See cref="NatureAttribute"> value extraction method 
    /// of Enum extensions class.
    /// </summary>
    [Test]
    public void GetNatureTest()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(TestEnum.First.GetNature(), Is.EqualTo(Nature.Music));
            Assert.That(TestEnum.Second.GetNature(), Is.EqualTo(Nature.Genetic));
        }
    }

    /// <summary>
    /// Test that <See cref="NatureAttribute"> value extraction method throws exceprion 
    /// if enum value has is no nature attribute.
    /// </summary>
    [Test]
    public void GetLinkThrowsForValueWithoutAttribute()
    {
        Assert.Throws<NullReferenceException>(() => TestEnum.Third.GetNature());
    }
}
