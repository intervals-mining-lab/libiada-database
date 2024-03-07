namespace Libiada.Database.Models;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

public partial class LibiadaDatabaseEntities : IdentityDbContext<AspNetUser, AspNetRole, int>
{
    public LibiadaDatabaseEntities(DbContextOptions<LibiadaDatabaseEntities> options)
        : base(options)
    {
    }

    public virtual DbSet<AccordanceCharacteristicLink> AccordanceCharacteristicLinks { get; set; }

    public virtual DbSet<AccordanceCharacteristicValue> AccordanceCharacteristicValues { get; set; }

    public virtual DbSet<AspNetPushNotificationSubscriber> AspNetPushNotificationSubscribers { get; set; }

    public virtual DbSet<BinaryCharacteristicLink> BinaryCharacteristicLinks { get; set; }

    public virtual DbSet<BinaryCharacteristicValue> BinaryCharacteristicValues { get; set; }

    public virtual DbSet<CalculationTask> CalculationTasks { get; set; }

    public virtual DbSet<CharacteristicValue> CharacteristicValues { get; set; }

    public virtual DbSet<CommonSequence> CommonSequences { get; set; }

    public virtual DbSet<CongenericCharacteristicLink> CongenericCharacteristicLinks { get; set; }

    public virtual DbSet<CongenericCharacteristicValue> CongenericCharacteristicValues { get; set; }

    public virtual DbSet<DataSequence> DataSequences { get; set; }

    public virtual DbSet<DnaSequence> DnaSequences { get; set; }

    public virtual DbSet<Element> Elements { get; set; }

    public virtual DbSet<Fmotif> Fmotifs { get; set; }

    public virtual DbSet<FullCharacteristicLink> FullCharacteristicLinks { get; set; }

    public virtual DbSet<ImageSequence> ImageSequences { get; set; }

    public virtual DbSet<LiteratureSequence> LiteratureSequences { get; set; }

    public virtual DbSet<Matter> Matters { get; set; }

    public virtual DbSet<Measure> Measures { get; set; }

    public virtual DbSet<Multisequence> Multisequences { get; set; }

    public virtual DbSet<MusicSequence> MusicSequences { get; set; }

    public virtual DbSet<Note> Notes { get; set; }

    public virtual DbSet<Pitch> Pitches { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<SequenceAttribute> SequenceAttributes { get; set; }

    public virtual DbSet<SequenceGroup> SequenceGroups { get; set; }

    public virtual DbSet<Subsequence> Subsequences { get; set; }

    public virtual DbSet<TaskResult> TaskResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .HasDefaultSchema("public")
            .HasAnnotation("Npgsql:PostgresExtension:public.pg_trgm", ",,1.6");

        modelBuilder.Entity<AccordanceCharacteristicLink>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_accordance_characteristic_link");

            entity.ToTable("accordance_characteristic_link", tb => tb.HasComment("Contatins list of possible combinations of accordance characteristics parameters."));

            entity.Property(e => e.Id)
                .HasComment("Unique identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);
            entity.Property(e => e.AccordanceCharacteristic).HasComment("Characteristic enum numeric value.");
            entity.Property(e => e.Link).HasComment("Link enum numeric value.");
        });

        modelBuilder.Entity<AccordanceCharacteristicValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_accordance_characteristic");

            entity.ToTable("accordance_characteristic", tb => tb.HasComment("Contains numeric chracteristics of accordance of element in different sequences."));

            entity.HasIndex(e => e.FirstSequenceId, "ix_accordance_characteristic_first_sequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => e.SecondSequenceId, "ix_accordance_characteristic_second_sequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.FirstSequenceId, e.SecondSequenceId }, "ix_accordance_characteristic_sequences_ids_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.FirstSequenceId, e.SecondSequenceId, e.CharacteristicLinkId }, "ix_accordance_characteristic_sequences_ids_characteristic_link_").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);
            entity.Property(e => e.CharacteristicLinkId).HasComment("Characteristic type id.");
            entity.Property(e => e.FirstElementId).HasComment("Id of the element of the first sequence for which the characteristic is calculated.");
            entity.Property(e => e.FirstSequenceId).HasComment("Id of the first sequence for which the characteristic is calculated.");
            entity.Property(e => e.SecondElementId).HasComment("Id of the element of the second sequence for which the characteristic is calculated.");
            entity.Property(e => e.SecondSequenceId).HasComment("Id of the second sequence for which the characteristic is calculated.");
            entity.Property(e => e.Value).HasComment("Numerical value of the characteristic.");

            entity.HasOne(d => d.AccordanceCharacteristicLink).WithMany(p => p.AccordanceCharacteristicValues)
                .HasConstraintName("fk_accordance_characteristic_link");
        });

        modelBuilder.Entity<AspNetPushNotificationSubscriber>(entity =>
        {
            entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            entity.HasOne(d => d.AspNetUser).WithMany(p => p.AspNetPushNotificationSubscribers)
                .HasConstraintName("FK.AspNetPushNotificationSubscribers.AspNetUsers_UserId");
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("User creation date and time (filled trough trigger).");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("User last change date and time (updated trough trigger).");
        });

        modelBuilder.Entity<BinaryCharacteristicLink>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_binary_characteristic_link");

            entity.ToTable("binary_characteristic_link", tb => tb.HasComment("Contatins list of possible combinations of dependence characteristics parameters."));

            entity.Property(e => e.Id)
                .HasComment("Unique identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);
            entity.Property(e => e.BinaryCharacteristic).HasComment("Characteristic enum numeric value.");
            entity.Property(e => e.Link).HasComment("Link enum numeric value.");
        });

        modelBuilder.Entity<BinaryCharacteristicValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_binary_characteristic");

            entity.ToTable("binary_characteristic", tb => tb.HasComment("Contains numeric chracteristics of elements dependece based on their arrangement in sequence."));

            entity.HasIndex(e => e.SequenceId, "ix_binary_characteristic_first_sequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.SequenceId, e.CharacteristicLinkId }, "ix_binary_characteristic_sequence_id_characteristic_link_id_bri").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);
            entity.Property(e => e.CharacteristicLinkId).HasComment("Characteristic type id.");
            entity.Property(e => e.FirstElementId).HasComment("Id of the first element of the sequence for which the characteristic is calculated.");
            entity.Property(e => e.SecondElementId).HasComment("Id of the second element of the sequence for which the characteristic is calculated.");
            entity.Property(e => e.SequenceId).HasComment("Id of the sequence for which the characteristic is calculated.");
            entity.Property(e => e.Value).HasComment("Numerical value of the characteristic.");

            entity.HasOne(d => d.BinaryCharacteristicLink).WithMany(p => p.BinaryCharacteristicValues)
                .HasConstraintName("fk_binary_characteristic_link");
        });

        modelBuilder.Entity<CalculationTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_task");

            entity.ToTable("task", tb => tb.HasComment("Contains information about computational tasks."));

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);
            entity.Property(e => e.Completed).HasComment("Task completion date and time.");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Task creation date and time (filled trough trigger).");
            entity.Property(e => e.Description).HasComment("Task description.");
            entity.Property(e => e.Started).HasComment("Task beginning of computation date and time.");
            entity.Property(e => e.Status).HasComment("Task status enum numeric value.");
            entity.Property(e => e.TaskType).HasComment("Task type enum numeric value.");
            entity.Property(e => e.UserId).HasComment("Creator user id.");

            entity.HasOne(d => d.AspNetUser).WithMany(p => p.CalculationTasks)
                .HasConstraintName("fk_task_user");
        });

        modelBuilder.Entity<CharacteristicValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_characteristic");

            entity.ToTable("full_characteristic", tb => tb.HasComment("Contains numeric chracteristics of complete sequences."));

            entity.HasIndex(e => e.CharacteristicLinkId, "ix_full_characteristic_characteristic_link_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => e.SequenceId, "ix_full_characteristic_sequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('characteristics_id_seq'::regclass)")
                .HasComment("Unique internal identifier.");
            entity.Property(e => e.CharacteristicLinkId).HasComment("Characteristic type id.");
            entity.Property(e => e.SequenceId).HasComment("Id of the sequence for which the characteristic is calculated.");
            entity.Property(e => e.Value).HasComment("Numerical value of the characteristic.");

            entity.HasOne(d => d.FullCharacteristicLink).WithMany(p => p.CharacteristicValues)
                .HasConstraintName("fk_full_characteristic_link");
        });

        modelBuilder.Entity<CommonSequence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_chain");

            entity.ToTable("chain", tb => tb.HasComment("Base table for all sequences that are stored in the database as alphabet and order."));

            entity.HasIndex(e => e.Alphabet, "ix_chain_alphabet").HasAnnotation("Npgsql:IndexMethod", "gin");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier of the sequence.");
            entity.Property(e => e.Alphabet).HasComment("Sequence's alphabet (array of elements ids).");
            entity.Property(e => e.Order).HasComment("Sequence's order.");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Sequence creation date and time (filled trough trigger).");
            entity.Property(e => e.Description).HasComment("Description of the sequence.");
            entity.Property(e => e.MatterId).HasComment("Id of the research object to which the sequence belongs.");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).");
            entity.Property(e => e.Notation).HasComment("Notation of the sequence (words, letters, notes, nucleotides, etc.).");
            entity.Property(e => e.RemoteDb).HasComment("Enum numeric value of the remote db from which sequence is downloaded.");
            entity.Property(e => e.RemoteId).HasComment("Id of the sequence in remote database.");

            entity.HasOne(d => d.Matter).WithMany(p => p.Sequence)
                .HasConstraintName("fk_chain_matter");
        });

        modelBuilder.Entity<CongenericCharacteristicLink>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_congeneric_characteristic_link");

            entity.ToTable("congeneric_characteristic_link", tb => tb.HasComment("Contatins list of possible combinations of congeneric characteristics parameters."));

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);
            entity.Property(e => e.ArrangementType).HasComment("Arrangement type enum numeric value.");
            entity.Property(e => e.CongenericCharacteristic).HasComment("Characteristic enum numeric value.");
            entity.Property(e => e.Link).HasComment("Link enum numeric value.");
        });

        modelBuilder.Entity<CongenericCharacteristicValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_congeneric_characteristic");

            entity.ToTable("congeneric_characteristic", tb => tb.HasComment("Contains numeric chracteristics of congeneric sequences."));

            entity.HasIndex(e => e.CharacteristicLinkId, "ix_congeneric_characteristic_characteristic_link_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => e.SequenceId, "ix_congeneric_characteristic_sequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.SequenceId, e.CharacteristicLinkId }, "ix_congeneric_characteristic_sequence_id_characteristic_link_id").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.SequenceId, e.ElementId }, "ix_congeneric_characteristic_sequence_id_element_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);
            entity.Property(e => e.CharacteristicLinkId).HasComment("Characteristic type id.");
            entity.Property(e => e.ElementId).HasComment("Id of the element for which the characteristic is calculated.");
            entity.Property(e => e.SequenceId).HasComment("Id of the sequence for which the characteristic is calculated.");
            entity.Property(e => e.Value).HasComment("Numerical value of the characteristic.");

            entity.HasOne(d => d.CongenericCharacteristicLink).WithMany(p => p.CongenericCharacteristicValues)
                .HasConstraintName("fk_congeneric_characteristic_link");
        });

        modelBuilder.Entity<DataSequence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("data_chain_pkey");

            entity.ToTable("data_chain", tb => tb.HasComment("Contains sequences that represent time series and other ordered data arrays."));

            entity.HasIndex(e => e.Alphabet, "data_chain_alphabet_idx").HasAnnotation("Npgsql:IndexMethod", "gin");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier of the sequence.");
            entity.Property(e => e.Alphabet).HasComment("Sequence's alphabet (array of elements ids).");
            entity.Property(e => e.Order).HasComment("Sequence's order.");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Sequence creation date and time (filled trough trigger).");
            entity.Property(e => e.Description).HasComment("Description of the sequence.");
            entity.Property(e => e.MatterId).HasComment("Id of the research object to which the sequence belongs.");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).");
            entity.Property(e => e.Notation).HasComment("Notation enum numeric value.");
            entity.Property(e => e.RemoteDb).HasComment("Enum numeric value of the remote db from which sequence is downloaded.");
            entity.Property(e => e.RemoteId).HasComment("Id of the sequence in remote database.");

            entity.HasOne(d => d.Matter).WithMany(p => p.DataSequence)
                .HasConstraintName("fk_data_chain_matter");
        });

        modelBuilder.Entity<DnaSequence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_dna_chain");

            entity.ToTable("dna_chain", tb => tb.HasComment("Contains sequences that represent genetic texts (DNA, RNA, gene sequecnes, etc)."));

            entity.HasIndex(e => e.Alphabet, "ix_dna_chain_alphabet").HasAnnotation("Npgsql:IndexMethod", "gin");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier of the sequence.");
            entity.Property(e => e.Alphabet).HasComment("Sequence's alphabet (array of elements ids).");
            entity.Property(e => e.Order).HasComment("Sequence's order.");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Sequence creation date and time (filled trough trigger).");
            entity.Property(e => e.Description).HasComment("Description of the sequence.");
            entity.Property(e => e.MatterId).HasComment("Id of the research object to which the sequence belongs.");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).");
            entity.Property(e => e.Notation).HasComment("Notation enum numeric value.");
            entity.Property(e => e.Partial)
                .HasDefaultValue(false)
                .HasComment("Flag indicating whether sequence is partial or complete.");
            entity.Property(e => e.RemoteDb).HasComment("Enum numeric value of the remote db from which sequence is downloaded.");
            entity.Property(e => e.RemoteId).HasComment("Id of the sequence in remote database.");

            entity.HasOne(d => d.Matter).WithMany(p => p.DnaSequence)
                .HasConstraintName("fk_dna_chain_matter");
        });

        modelBuilder.Entity<Element>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_elements");

            entity.ToTable("element", tb => tb.HasComment("Base table for all elements that are stored in the database and used in alphabets of sequences."));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier of the element.");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Element creation date and time (filled trough trigger).");
            entity.Property(e => e.Description).HasComment("Description of the element.");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).");
            entity.Property(e => e.Name).HasComment("Name of the element.");
            entity.Property(e => e.Notation).HasComment("Notation enum numeric value.");
            entity.Property(e => e.Value).HasComment("Content of the element.");
        });

        modelBuilder.Entity<Fmotif>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_fmotif");

            entity.ToTable("fmotif", tb => tb.HasComment("Contains elements that represent note sequences in form of formal motifs that are used as elements of segmented music sequences."));

            entity.HasIndex(e => e.Alphabet, "ix_fmotif_alphabet").HasAnnotation("Npgsql:IndexMethod", "gin");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier of the fmotif.");
            entity.Property(e => e.Alphabet).HasComment("Fmotif's alphabet (array of notes ids).");
            entity.Property(e => e.Order).HasComment("Fmotif's order.");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Fmotif creation date and time (filled trough trigger).");
            entity.Property(e => e.Description).HasComment("Fmotif description.");
            entity.Property(e => e.FmotifType).HasComment("Fmotif type enum numeric value.");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).");
            entity.Property(e => e.Name).HasComment("Fmotif name.");
            entity.Property(e => e.Notation).HasComment("Fmotif notation enum numeric value (always 6).");
            entity.Property(e => e.Value).HasComment("Fmotif hash value.");
        });

        modelBuilder.Entity<FullCharacteristicLink>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_full_characteristic_link");

            entity.ToTable("full_characteristic_link", tb => tb.HasComment("Contatins list of possible combinations of characteristics parameters."));

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);
            entity.Property(e => e.ArrangementType).HasComment("Arrangement type enum numeric value.");
            entity.Property(e => e.FullCharacteristic).HasComment("Characteristic enum numeric value.");
            entity.Property(e => e.Link).HasComment("Link enum numeric value.");
        });

        modelBuilder.Entity<ImageSequence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_image_sequence");

            entity.ToTable("image_sequence", tb => tb.HasComment("Contains information on image transformations and order extraction. Does not store an actual order of image and used for reference by characteristics tables."));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier of the image sequence.");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Sequence creation date and time (filled trough trigger).");
            entity.Property(e => e.ImageTransformations).HasComment("Array of image transformations applied begore the extraction of the sequence.");
            entity.Property(e => e.MatrixTransformations).HasComment("Array of matrix transformations applied begore the extraction of the sequence.");
            entity.Property(e => e.MatterId).HasComment("Id of the research object (image) to which the sequence belongs.");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).");
            entity.Property(e => e.Notation).HasComment("Notation enum numeric value.");
            entity.Property(e => e.OrderExtractor).HasComment("Order extractor enum numeric value used in the process of creation of the sequence.");
            entity.Property(e => e.RemoteDb).HasComment("Enum numeric value of the remote db from which sequence is downloaded.");
            entity.Property(e => e.RemoteId).HasComment("Id of the sequence in remote database.");

            entity.HasOne(d => d.Matter).WithMany(p => p.ImageSequence)
                .HasConstraintName("fk_image_sequence_matter");
        });

        modelBuilder.Entity<LiteratureSequence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_literature_chain");

            entity.ToTable("literature_chain", tb => tb.HasComment("Contains sequences that represent literary works and their various translations."));

            entity.HasIndex(e => e.Alphabet, "ix_literature_chain_alphabet").HasAnnotation("Npgsql:IndexMethod", "gin");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier of the sequence.");
            entity.Property(e => e.Alphabet).HasComment("Sequence's alphabet (array of elements ids).");
            entity.Property(e => e.Order).HasComment("Sequence's order.");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Sequence creation date and time (filled trough trigger).");
            entity.Property(e => e.Description).HasComment("Sequence description.");
            entity.Property(e => e.Language).HasComment("Primary language of literary work.");
            entity.Property(e => e.MatterId).HasComment("Id of the research object to which the sequence belongs.");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).");
            entity.Property(e => e.Notation).HasComment("Notation enum numeric value.");
            entity.Property(e => e.Original)
                .HasDefaultValue(true)
                .HasComment("Flag indicating if this sequence is in original language or was translated.");
            entity.Property(e => e.RemoteDb).HasComment("Enum numeric value of the remote db from which sequence is downloaded.");
            entity.Property(e => e.RemoteId).HasComment("Id of the sequence in remote database.");
            entity.Property(e => e.Translator).HasComment("Author of translation or automated translator.");

            entity.HasOne(d => d.Matter).WithMany(p => p.LiteratureSequence)
                .HasConstraintName("fk_literature_chain_matter");
        });

        modelBuilder.Entity<Matter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_matter");

            entity.ToTable("matter", tb => tb.HasComment("Contains research objects, samples, texts, etc (one research object may be represented by several sequences)."));

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier of the research object.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);
            entity.Property(e => e.CollectionCountry).HasComment("Collection country of the genetic sequence.");
            entity.Property(e => e.CollectionDate).HasComment("Collection date of the genetic sequence.");
            entity.Property(e => e.CollectionLocation).HasComment("Collection location of the genetic sequence.");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Record creation date and time (filled trough trigger).");
            entity.Property(e => e.Description).HasComment("Description of the research object.");
            entity.Property(e => e.Group).HasComment("Group enum numeric value.");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).");
            entity.Property(e => e.MultisequenceId).HasComment("Id of the parent multisequence.");
            entity.Property(e => e.MultisequenceNumber).HasComment("Serial number in multisequence.");
            entity.Property(e => e.Name).HasComment("Research object name.");
            entity.Property(e => e.Nature).HasComment("Research object nature enum numeric value.");
            entity.Property(e => e.SequenceType).HasComment("Sequence type enum numeric value.");
            entity.Property(e => e.Source).HasComment("Source of the genetic sequence.");

            entity.HasOne(d => d.Multisequence).WithMany(p => p.Matters)
                .HasConstraintName("fk_matter_multisequence");

            entity.HasMany(d => d.Groups)
                .WithMany(p => p.Matters)
                .UsingEntity(
                    "sequence_group_matter",
                     j =>
                     {
                         j.Property("MatterId").HasColumnName("matter_id");
                         j.Property("GroupId").HasColumnName("group_id");
                     });
        });

        modelBuilder.Entity<Measure>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_measure");

            entity.ToTable("measure", tb => tb.HasComment("Contains elements that represent note sequences in form of measures (bars) that are used as elements of segmented music sequences."));

            entity.HasIndex(e => e.Alphabet, "ix_measure_alphabet").HasAnnotation("Npgsql:IndexMethod", "gin");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier of the measure.");
            entity.Property(e => e.Alphabet).HasComment("Measure alphabet (array of notes ids).");
            entity.Property(e => e.Beatbase).HasComment("Time signature lower numeral (Beat denominator).");
            entity.Property(e => e.Beats).HasComment("Time signature upper numeral (Beat numerator).");
            entity.Property(e => e.Order).HasComment("Measure order.");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Measure creation date and time (filled trough trigger).");
            entity.Property(e => e.Description).HasComment("Description of the sequence.");
            entity.Property(e => e.Fifths).HasComment("Key signature of the measure (negative value represents the number of flats (bemolles) and positive represents the number of sharps (diesis)).");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).");
            entity.Property(e => e.Name).HasComment("Measure name.");
            entity.Property(e => e.Notation).HasComment("Measure notation enum numeric value (always 7).");
            entity.Property(e => e.Value).HasComment("Measure hash code.");
            entity.Property(e => e.Major).HasComment("Music mode of the measure. true  represents major and false represents minor.");
        });

        modelBuilder.Entity<Multisequence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("multisequence_pkey");

            entity.ToTable("multisequence", tb => tb.HasComment("Contains information on groups of related research objects (such as series of books, chromosomes of the same organism, etc) and their order in these groups."));

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);
            entity.Property(e => e.Name).HasComment("Multisequence name.");
            entity.Property(e => e.Nature).HasComment("Multisequence nature enum numeric value.");
        });

        modelBuilder.Entity<MusicSequence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_music_chain");

            entity.ToTable("music_chain", tb => tb.HasComment("Contains sequences that represent musical works in form of note, fmotive or measure sequences."));

            entity.HasIndex(e => e.Alphabet, "ix_music_chain_alphabet").HasAnnotation("Npgsql:IndexMethod", "gin");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier.");
            entity.Property(e => e.Alphabet).HasComment("Sequence's alphabet (array of elements ids).");
            entity.Property(e => e.Order).HasComment("Sequence's order.");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Sequence creation date and time (filled trough trigger).");
            entity.Property(e => e.Description).HasComment("Sequence description.");
            entity.Property(e => e.MatterId).HasComment("Id of the research object to which the sequence belongs.");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).");
            entity.Property(e => e.Notation).HasComment("Notation enum numeric value.");
            entity.Property(e => e.PauseTreatment).HasComment("Pause treatment enum numeric value.");
            entity.Property(e => e.RemoteDb).HasComment("Enum numeric value of the remote db from which sequence is downloaded.");
            entity.Property(e => e.RemoteId).HasComment("Id of the sequence in the remote database.");
            entity.Property(e => e.SequentialTransfer)
                .HasDefaultValue(false)
                .HasComment("Flag indicating whether or not sequential transfer was used in sequence segmentation into fmotifs.");

            entity.HasOne(d => d.Matter).WithMany(p => p.MusicSequence)
                .HasConstraintName("fk_music_chain_matter");
        });

        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_note");

            entity.ToTable("note", tb => tb.HasComment("Contains elements that represent notes that are used as elements of music sequences."));

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier.");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Note creation date and time (filled trough trigger).");
            entity.Property(e => e.Denominator).HasComment("Note duration fraction denominator.");
            entity.Property(e => e.Description).HasComment("Note description.");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).");
            entity.Property(e => e.Name).HasComment("Note name.");
            entity.Property(e => e.Notation).HasComment("Measure notation enum numeric value (always 8).");
            entity.Property(e => e.Numerator).HasComment("Note duration fraction numerator.");
            entity.Property(e => e.Tie).HasComment("Note tie type enum numeric value.");
            entity.Property(e => e.Triplet).HasComment("Flag indicating if note is a part of triplet (tuplet).");
            entity.Property(e => e.Value).HasComment("Note hash code.");

            entity.HasMany(d => d.Pitches)
                .WithMany(p => p.Notes)
                .UsingEntity(
                    "note_pitch",
                     j =>
                     {
                         j.Property("NoteId").HasColumnName("note_id");
                         j.Property("PitchId").HasColumnName("pitch_id");
                     });
        });

        modelBuilder.Entity<Pitch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_pitch");

            entity.ToTable("pitch", tb => tb.HasComment("Note's pitch."));

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier of the pitch.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);
            entity.Property(e => e.Accidental).HasComment("Pitch key signature enum numeric value.");
            entity.Property(e => e.Instrument).HasComment("Pitch instrument enum numeric value.");
            entity.Property(e => e.Midinumber).HasComment("Unique number by midi standard.");
            entity.Property(e => e.NoteSymbol).HasComment("Note symbol enum numeric value.");
            entity.Property(e => e.Octave).HasComment("Octave number.");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_piece");

            entity.ToTable("position", tb => tb.HasComment("Contains information on additional fragment positions (for subsequences concatenated from several parts)."));

            entity.HasIndex(e => e.SubsequenceId, "ix_position_subsequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('piece_id_seq'::regclass)")
                .HasComment("Unique internal identifier.");
            entity.Property(e => e.Length).HasComment("Fragment length.");
            entity.Property(e => e.Start).HasComment("Index of the fragment beginning (from zero).");
            entity.Property(e => e.SubsequenceId).HasComment("Parent subsequence id.");

            entity.HasOne(d => d.Subsequence).WithMany(p => p.Position).HasConstraintName("fk_position_subsequence");
        });

        modelBuilder.Entity<SequenceAttribute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_chain_attribute");

            entity.ToTable("chain_attribute", tb => tb.HasComment("Contains chains' attributes and their values."));

            entity.HasIndex(e => e.SequenceId, "ix_sequence_attribute_sequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);
            entity.Property(e => e.Attribute).HasComment("Attribute enum numeric value.");
            entity.Property(e => e.SequenceId).HasComment("Id of the sequence to which attribute belongs.");
            entity.Property(e => e.Value).HasComment("Text of the attribute.");

            entity.HasOne(d => d.Subsequence).WithMany(p => p.SequenceAttribute);
        });

        modelBuilder.Entity<SequenceGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sequence_group_pkey");

            entity.ToTable("sequence_group", tb => tb.HasComment("Contains information about sequences groups."));

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Sequence group creation date and time (filled trough trigger).");
            entity.Property(e => e.CreatorId).HasComment("Record creator user id.");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).");
            entity.Property(e => e.ModifierId).HasComment("Record editor user id.");
            entity.Property(e => e.Name).HasComment("Sequences group name.");
            entity.Property(e => e.Nature).HasComment("Sequences group nature enum numeric value.");
            entity.Property(e => e.SequenceGroupType).HasComment("Sequence group type enum numeric value.");
        });

        modelBuilder.Entity<Subsequence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_subsequence");

            entity.ToTable("subsequence", tb => tb.HasComment("Contains information on location and length of the fragments within complete sequences."));

            entity.HasIndex(e => e.Feature, "ix_subsequence_feature_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => e.SequenceId, "ix_subsequence_sequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.SequenceId, e.Feature }, "ix_subsequence_sequence_id_feature_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier.");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Subsequence creation date and time (filled trough trigger).");
            entity.Property(e => e.Feature).HasComment("Subsequence feature enum numeric value.");
            entity.Property(e => e.Length).HasComment("Fragment length.");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).");
            entity.Property(e => e.Partial)
                .HasDefaultValue(false)
                .HasComment("Flag indicating whether subsequence is partial or complete.");
            entity.Property(e => e.RemoteId).HasComment("Id of the subsequence in the remote database (ncbi or same as paren sequence remote db).");
            entity.Property(e => e.SequenceId).HasComment("Parent sequence id.");
            entity.Property(e => e.Start).HasComment("Index of the fragment beginning (from zero).");
        });

        modelBuilder.Entity<TaskResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("task_result_pkey");

            entity.ToTable("task_result", tb => tb.HasComment("Contains JSON results of tasks calculation. Results are stored as key/value pairs."));

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);
            entity.Property(e => e.Key).HasComment("Results element name.");
            entity.Property(e => e.TaskId).HasComment("Parent task id.");
            entity.Property(e => e.Value).HasComment("Results element value (as json).");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskResult).HasConstraintName("fk_task_result_task");
        });

        OnModelCreatingGeneratedFunctions(modelBuilder);
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingGeneratedFunctions(ModelBuilder modelBuilder);
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
