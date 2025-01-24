namespace Libiada.Database.Tasks;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// The task type.
/// </summary>
public enum TaskType : byte
{
    /// <summary>
    /// The accordance characteristics calculation task.
    /// </summary>
    [Display(Name = "Accordance characteristics calculation")]
    AccordanceCalculation = 1,

    /// <summary>
    /// The integral characterisctics calculation task.
    /// </summary>
    [Display(Name = "Characteristics calculation")]
    Calculation = 2,

    /// <summary>
    /// The cluster analysis task.
    /// </summary>
    [Display(Name = "Clusterization")]
    Clusterization = 3,

    /// <summary>
    /// The congeneric characteristics calculation task.
    /// </summary>
    [Display(Name = "Congeneric characteristics calculation")]
    CongenericCalculation = 4,

    /// <summary>
    /// Calculates characteristics of custom sequences.
    /// </summary>
    [Display(Name = "Custom sequences characteristics calculation")]
    CustomSequenceCalculation = 5,

    /// <summary>
    /// Calculates characteristics for higher order / derivative of custom sequence.
    /// </summary>
    [Display(Name = "Custom sequences order transformation/derivative characteristics calculation")]
    CustomSequenceOrderTransformationCalculation = 6,

    /// <summary>
    /// Research object creation and sequence import task.
    /// </summary>
    [Display(Name = "Music files processing")]
    MusicFiles = 7,

    /// <summary>
    /// The local characteristics calculation task.
    /// </summary>
    [Display(Name = "Sliding window calculation")]
    LocalCalculation = 8,

    /// <summary>
    /// Calculates characteristics for higher order / derivative.
    /// </summary>
    [Display(Name = "Order transformation/derivative characteristics calculation")]
    OrderTransformationCalculation = 9,

    /// <summary>
    /// Calculates relation characteristics.
    /// </summary>
    [Display(Name = "Relation characteristics calculation")]
    RelationCalculation = 10,

    /// <summary>
    /// The sequences alignment task.
    /// </summary>
    [Display(Name = "Sequences alignment")]
    SequencesAlignment = 11,

    /// <summary>
    /// The subsequences characteristics calculation task.
    /// </summary>
    [Display(Name = "Subsequences characteristics calculation")]
    SubsequencesCalculation = 12,

    /// <summary>
    /// Calculates subsequences similarity matrix.
    /// </summary>
    [Display(Name = "Subsequences similarity matrix")]
    SubsequencesComparer = 13,

    /// <summary>
    /// Calculates subsequences distribution.
    /// </summary>
    [Display(Name = "Map of genes")]
    SubsequencesDistribution = 14,

    /// <summary>
    /// Calculates subsequences similarity.
    /// </summary>
    [Display(Name = "Subsequences similarity")]
    SubsequencesSimilarity = 15,

    /// <summary>
    /// Calculates distribution of sequences by order.
    /// </summary>
    [Display(Name = "Distribution of sequences by order")]
    SequencesOrderDistribution = 16,

    /// <summary>
    /// The batch genes import task.
    /// </summary>
    [Display(Name = "Batch genes import")]
    BatchGenesImport = 17,

    /// <summary>
    /// The batch sequences import task.
    /// </summary>
    [Display(Name = "Batch sequences import")]
    BatchSequenceImport = 18,

    /// <summary>
    /// The custom sequence order transformation.
    /// </summary>
    [Display(Name = "Custom sequences order transformation")]
    CustomSequenceOrderTransformer = 19,

    /// <summary>
    /// The genes import task.
    /// </summary>
    [Display(Name = "Genes import")]
    GenesImport = 20,

    /// <summary>
    /// The order transformation task.
    /// </summary>
    [Display(Name = "Order transformation")]
    OrderTransformer = 21,

    /// <summary>
    /// Checks if sequence in database equals one in file.
    /// </summary>
    [Display(Name = "Sequence check")]
    SequenceCheck = 22,

    /// <summary>
    /// Sequences import for existing research object.
    /// </summary>
    [Display(Name = "Sequence upload")]
    SequencesUpload = 23,

    /// <summary>
    /// Research object creation and sequence import task.
    /// </summary>
    [Display(Name = "Sequence import")]
    ResearchObjectImport = 24,

    /// <summary>
    /// The sequence prediction task.
    /// </summary>
    [Display(Name = "Sequence prediction")]
    SequencePrediction = 25,

    /// <summary>
    /// Batch poems import task.
    /// </summary>
    [Display(Name = "Batch poems import")]
    BatchPoemsImport = 26,

    /// <summary>
    /// Calculates distribution of sequences by order.
    /// </summary>
    [Display(Name = "Order calculation")]
    OrderCalculation = 27,

    /// <summary>
    /// Calculates order transformations convergence.
    /// </summary>
    [Display(Name = "Order transformation convergence")]
    OrderTransformationConvergence = 28,

    /// <summary>
    /// Batch music import task.
    /// </summary>
    [Display(Name = "Batch music import")]
    BatchMusicImport = 29,

    /// <summary>
    /// Visualizes order transformations.
    /// </summary>
    [Display(Name = "Order transformation visualization")]
    OrderTransformationVisualization = 30,

    /// <summary>
    /// Fmotifs dictionary task.
    /// </summary>
    [Display(Name = "Fmotifs dictionary")]
    FmotifsDictionary = 31,

    /// <summary>
    /// Calculates dynamic visualization of order transformations characteristics.
    /// </summary>
    [Display(Name = "Order transformation characteristics dynamic visualization")]
    OrderTransformationCharacteristicsDynamicVisualization = 32,

    /// <summary>
    /// Calculates accordance of orders by intervals distributions.
    /// </summary>
    [Display(Name = "Calculate accordance of orders by intervals distributions")]
    OrdersIntervalsDistributionsAccordance = 33,

    /// <summary>
    /// Calculates characteristics of intervals distributions.
    /// </summary>
    [Display(Name = "Calculate characteristics of intervals distributions")]
    IntervalsCharacteristicsDistribution = 34,

    /// <summary>
    /// Batch images import from files.
    /// </summary>
    [Display(Name = "Batch images import")]
    BatchImagesImport = 35,

    /// <summary>
    /// Imports genetic sequences and their annotations
    /// using genbank search results file.
    /// </summary>
    [Display(Name = "Batch genetic import from GenBank search file")]
    BatchGeneticImportFromGenBankSearchFile = 36,

    /// <summary>
    /// Imports genetic sequences and their annotations
    /// using genbank search results query.
    /// </summary>
    [Display(Name = "Batch genetic import from GenBank search query")]
    BatchGeneticImportFromGenBankSearchQuery = 37,

    /// <summary>
    /// Searches genbank nuccore database as in
    /// <see cref="TaskType.BatchGeneticImportFromGenBankSearchQuery"/>
    /// and displays results in tabular form.
    /// </summary>
    [Display(Name = "Ncbi nuccore search")]
    NcbiNuccoreSearch = 38,

    /// <summary>
    /// Checks if there is GenBank accession versions update 
    /// and displays results in tabular form.
    /// </summary>
    [Display(Name = "GenBank accession versions update check")]
    GenBankAccessionVersionUpdateChecker = 39,

    /// <summary>
    /// Poem segmentation task.
    /// </summary>
    [Display(Name = "Poem segmentation")]
    PoemSegmentation = 40,

    /// <summary>
    /// Custom sequence segmentation task.
    /// </summary>
    [Display(Name = "Custom sequence segmentation")]
    CustomSequenceSegmentation = 41,
}
