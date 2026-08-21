namespace Libiada.Database.Tests.Extensions;
﻿
using Libiada.Database.Extensions;

[TestFixture(TestOf = typeof(FeatureExtensions))]
public class FeatureExtensionsTests
{
    /// <summary>
    /// Test for genbank name attribute value extraction method of <see cref="Feature" extension class/>.
    /// </summary>
    [Test]
    public void GetGenBankNameTest()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(Feature.NonCodingSequence.GetGenBankName(), Is.EqualTo(string.Empty));
            Assert.That(Feature.CodingSequence.GetGenBankName(), Is.EqualTo("CDS"));
        }
    }
}
