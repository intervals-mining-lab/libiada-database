namespace Libiada.Database.Tests.Attributes;

using Libiada.Core.Images;
using Libiada.Database.Attributes;

/// <summary>
/// The image order extractor attribute tests.
/// </summary>
[TestFixture(TestOf = typeof(ImageOrderExtractorAttribute))]
public class ImageOrderExtractorAttributeTests
{
    /// <summary>
    /// Invalid image order extractor value test.
    /// </summary>
    [Test]
    public void InvalidImageOrderExtractorValueTest()
    {
        Assert.Multiple(() =>
        {
            Assert.Throws<ArgumentException>(() => new ImageOrderExtractorAttribute(typeof(object)));
            Assert.Throws<ArgumentException>(() => new ImageOrderExtractorAttribute(typeof(List<int>)));
        });

    }

    /// <summary>
    /// Image order extractor attribute value test.
    /// </summary>
    [Test]
    public void ImageOrderExtractorAttributeValueTest()
    {
        ImageOrderExtractorAttribute attribute = new(typeof(LineOrderExtractor));
        Assert.That(typeof(LineOrderExtractor), Is.EqualTo(attribute.Value));
    }
}
