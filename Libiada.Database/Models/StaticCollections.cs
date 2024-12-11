namespace Libiada.Database.Models;

using System.Collections.ObjectModel;

using Libiada.Core.Core;
using Libiada.Core.Core.ArrangementManagers;
using Libiada.Core.Core.Characteristics.Calculators.AccordanceCalculators;
using Libiada.Core.Core.Characteristics.Calculators.BinaryCalculators;
using Libiada.Core.Core.Characteristics.Calculators.CongenericCalculators;
using Libiada.Core.Core.Characteristics.Calculators.FullCalculators;

/// <summary>
/// Filtered collections available to ordinary users.
/// And some other static collections.
/// </summary>
public static class StaticCollections
{
    /// <summary>
    /// The user available arrangement types.
    /// </summary>
    public static readonly ReadOnlyCollection<ArrangementType> UserAvailableArrangementTypes = new(
    [
        ArrangementType.Intervals
    ]);

    /// <summary>
    /// The user available links.
    /// </summary>
    public static readonly ReadOnlyCollection<Link> UserAvailableLinks = new(
    [
        Link.NotApplied,
        Link.Start,
        Link.Cycle
    ]);

    /// <summary>
    /// The user available characteristics.
    /// </summary>
    public static readonly ReadOnlyCollection<FullCharacteristic> UserAvailableFullCharacteristics = new(
    [
        FullCharacteristic.ATSkew,
        FullCharacteristic.AlphabetCardinality,
        FullCharacteristic.AverageRemoteness,
        FullCharacteristic.GCRatio,
        FullCharacteristic.GCSkew,
        FullCharacteristic.GCToATRatio,
        FullCharacteristic.IdentifyingInformation,
        FullCharacteristic.Length,
        FullCharacteristic.MKSkew,
        FullCharacteristic.RYSkew,
        FullCharacteristic.SWSkew
    ]);

    /// <summary>
    /// The user available congeneric characteristics.
    /// </summary>
    public static readonly ReadOnlyCollection<CongenericCharacteristic> UserAvailableCongenericCharacteristics = new(
    [
        CongenericCharacteristic.AverageRemoteness,
        CongenericCharacteristic.IdentifyingInformation,
        CongenericCharacteristic.Length
    ]);

    /// <summary>
    /// The user available accordance characteristics.
    /// </summary>
    public static readonly ReadOnlyCollection<AccordanceCharacteristic> UserAvailableAccordanceCharacteristics = new(
    [
        AccordanceCharacteristic.PartialComplianceDegree,
        AccordanceCharacteristic.MutualComplianceDegree
    ]);

    /// <summary>
    /// The user available binary characteristics.
    /// </summary>
    public static readonly ReadOnlyCollection<BinaryCharacteristic> UserAvailableBinaryCharacteristics = new(
    [
        BinaryCharacteristic.GeometricMean,
        BinaryCharacteristic.Redundancy,
        BinaryCharacteristic.InvolvedPartialDependenceCoefficient,
        BinaryCharacteristic.PartialDependenceCoefficient,
        BinaryCharacteristic.NormalizedPartialDependenceCoefficient,
        BinaryCharacteristic.MutualDependenceCoefficient,
    ]);

    /// <summary>
    /// Notations elements of which will not change.
    /// </summary>
    public static readonly ReadOnlyCollection<Notation> StaticNotations = new(
    [
        Notation.Nucleotides,
        Notation.Triplets,
        Notation.AminoAcids,
        Notation.Letters
    ]);

    /// <summary>
    /// Sequence types with subsequences.
    /// </summary>
    public static readonly SequenceType[] SequenceTypesWithSubsequences =
    [
            SequenceType.CompleteGenome,
            SequenceType.MitochondrialGenome,
            SequenceType.ChloroplastGenome,
            SequenceType.Plasmid,
            SequenceType.Plastid,
            SequenceType.MitochondrialPlasmid
    ];
}
