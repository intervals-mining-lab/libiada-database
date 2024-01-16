namespace Libiada.Database.Tests;

using Libiada.Core.Extensions;

/// <summary>
/// Attribute enum tests.
/// </summary>
[TestFixture(TestOf = typeof(AnnotationAttribute))]
public class AttributeTests
{
    /// <summary>
    /// The attributes count.
    /// </summary>
    private const int AttributesCount = 48;

    /// <summary>
    /// Array of all attributes.
    /// </summary>
    private readonly AnnotationAttribute[] attributes = EnumExtensions.ToArray<AnnotationAttribute>();

    /// <summary>
    /// Tests count of attributes.
    /// </summary>
    [Test]
    public void AttributesCountTest() => Assert.That(attributes.Length, Is.EqualTo(AttributesCount));

    /// <summary>
    /// Tests values of attributes.
    /// </summary>
    [Test]
    public void AttributeValuesTest()
    {
        for (int i = 1; i <= AttributesCount; i++)
        {
            Assert.That(attributes, Has.Member((AnnotationAttribute)i));
        }
    }

    /// <summary>
    /// Tests names of attributes.
    /// </summary>
    /// <param name="attribute">
    /// The attribute.
    /// </param>
    /// <param name="name">
    /// The name.
    /// </param>
    [TestCase((AnnotationAttribute)1, "db_xref")]
    [TestCase((AnnotationAttribute)2, "protein_id")]
    [TestCase((AnnotationAttribute)3, "complement")]
    [TestCase((AnnotationAttribute)4, "complement_join")]
    [TestCase((AnnotationAttribute)5, "product")]
    [TestCase((AnnotationAttribute)6, "note")]
    [TestCase((AnnotationAttribute)7, "codon_start")]
    [TestCase((AnnotationAttribute)8, "transl_table")]
    [TestCase((AnnotationAttribute)9, "inference")]
    [TestCase((AnnotationAttribute)10, "rpt_type")]
    [TestCase((AnnotationAttribute)11, "locus_tag")]
    [TestCase((AnnotationAttribute)12, "old_locus_tag")]
    [TestCase((AnnotationAttribute)13, "gene")]
    [TestCase((AnnotationAttribute)14, "anticodon")]
    [TestCase((AnnotationAttribute)15, "EC_number")]
    [TestCase((AnnotationAttribute)16, "exception")]
    [TestCase((AnnotationAttribute)17, "gene_synonym")]
    [TestCase((AnnotationAttribute)18, "pseudo")]
    [TestCase((AnnotationAttribute)19, "ncRNA_class")]
    [TestCase((AnnotationAttribute)20, "standard_name")]
    [TestCase((AnnotationAttribute)21, "rpt_family")]
    [TestCase((AnnotationAttribute)22, "direction")]
    [TestCase((AnnotationAttribute)23, "ribosomal_slippage")]
    [TestCase((AnnotationAttribute)24, "partial")]
    [TestCase((AnnotationAttribute)25, "codon_recognized")]
    [TestCase((AnnotationAttribute)26, "bound_moiety")]
    [TestCase((AnnotationAttribute)27, "rpt_unit_range")]
    [TestCase((AnnotationAttribute)28, "rpt_unit_seq")]
    [TestCase((AnnotationAttribute)29, "function")]
    [TestCase((AnnotationAttribute)30, "transl_except")]
    [TestCase((AnnotationAttribute)31, "pseudogene")]
    [TestCase((AnnotationAttribute)32, "mobile_element_type")]
    [TestCase((AnnotationAttribute)33, "experiment")]
    [TestCase((AnnotationAttribute)34, "citation")]
    [TestCase((AnnotationAttribute)35, "regulatory_class")]
    [TestCase((AnnotationAttribute)36, "artificial_location")]
    [TestCase((AnnotationAttribute)37, "proviral")]
    [TestCase((AnnotationAttribute)38, "operon")]
    [TestCase((AnnotationAttribute)39, "number")]
    [TestCase((AnnotationAttribute)40, "replace")]
    [TestCase((AnnotationAttribute)41, "compare")]
    [TestCase((AnnotationAttribute)42, "allele")]
    [TestCase((AnnotationAttribute)43, "trans_splicing")]
    [TestCase((AnnotationAttribute)44, "frequency")]
    [TestCase((AnnotationAttribute)45, "GO_function")]
    [TestCase((AnnotationAttribute)46, "GO_component")]
    [TestCase((AnnotationAttribute)47, "GO_process")]
    [TestCase((AnnotationAttribute)48, "satellite")]
    public void AttributesDisplayValuesTest(AnnotationAttribute attribute, string name) => Assert.That(attribute.GetDisplayValue(), Is.EqualTo(name));

    /// <summary>
    /// Tests that all attributes values are unique.
    /// </summary>
    [Test]
    public void AttributeValuesUniqueTest() => Assert.That(attributes.Cast<byte>(), Is.Unique);
}
