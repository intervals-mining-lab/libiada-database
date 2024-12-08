namespace Libiada.Database.Tests.Attributes;

using Libiada.Core.Images;
using Libiada.Database.Attributes;

/// <summary>
/// The image transformer type attribute tests.
/// </summary>
[TestFixture(TestOf = typeof(ImageTransformerTypeAttribute))]
public class ImageTransformerTypeAttributeTests
{
    /// <summary>
    /// Invalid image transformer type value test.
    /// </summary>
    [Test]
    public void InvalidImageTransformerTypeValueTest()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() => new ImageTransformerTypeAttribute(typeof(object)));
            Assert.Throws<ArgumentException>(() => new ImageTransformerTypeAttribute(typeof(List<int>)));
        });

    }

    /// <summary>
    /// Image transformer type attribute value test.
    /// </summary>
    [Test]
    public void ImageTransformerTypeAttributeValueTest()
    {
        ImageTransformerTypeAttribute attribute = new(typeof(ImageResizer));
        Assert.That(typeof(ImageResizer), Is.EqualTo(attribute.Value));
    }
}
