using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Microsoft.Extensions.Configuration;


 
namespace Libiada.Database;

public partial class LibiadaDatabaseEntities : IdentityDbContext<AspNetUser, AspNetRole, int>
{
    private readonly IConfiguration configuration;

    public LibiadaDatabaseEntities(DbContextOptions<LibiadaDatabaseEntities> options, IConfiguration configuration)
        : base(options)
    {
        this.configuration = configuration;
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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseNpgsql(configuration.GetConnectionString("LibiadaDatabaseEntities"));

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

            entity.HasIndex(e => new { e.AccordanceCharacteristic, e.Link }, "uk_accordance_characteristic_link").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("Unique identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasColumnName("id");
            entity.Property(e => e.AccordanceCharacteristic)
                .HasComment("Characteristic enum numeric value.")
                .HasColumnName("accordance_characteristic");
            entity.Property(e => e.Link)
                .HasComment("Link enum numeric value.")
                .HasColumnName("link");
        });

        modelBuilder.Entity<AccordanceCharacteristicValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_accordance_characteristic");

            entity.ToTable("accordance_characteristic", tb => tb.HasComment("Contains numeric chracteristics of accordance of element in different sequences."));

            entity.HasIndex(e => e.FirstSequenceId, "ix_accordance_characteristic_first_chain_id");

            entity.HasIndex(e => e.FirstSequenceId, "ix_accordance_characteristic_first_sequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => e.SecondSequenceId, "ix_accordance_characteristic_second_chain_id");

            entity.HasIndex(e => e.SecondSequenceId, "ix_accordance_characteristic_second_sequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.FirstSequenceId, e.SecondSequenceId }, "ix_accordance_characteristic_sequences_ids_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.FirstSequenceId, e.SecondSequenceId, e.CharacteristicLinkId }, "ix_accordance_characteristic_sequences_ids_characteristic_link_").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.FirstSequenceId, e.SecondSequenceId, e.FirstElementId, e.SecondElementId, e.CharacteristicLinkId }, "uk_accordance_characteristic").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasColumnName("id");
            entity.Property(e => e.CharacteristicLinkId)
                .HasComment("Characteristic type id.")
                .HasColumnName("characteristic_link_id");
            entity.Property(e => e.FirstElementId)
                .HasComment("Id of the element of the first sequence for which the characteristic is calculated.")
                .HasColumnName("first_element_id");
            entity.Property(e => e.FirstSequenceId)
                .HasComment("Id of the first sequence for which the characteristic is calculated.")
                .HasColumnName("first_chain_id");
            entity.Property(e => e.SecondElementId)
                .HasComment("Id of the element of the second sequence for which the characteristic is calculated.")
                .HasColumnName("second_element_id");
            entity.Property(e => e.SecondSequenceId)
                .HasComment("Id of the second sequence for which the characteristic is calculated.")
                .HasColumnName("second_chain_id");
            entity.Property(e => e.Value)
                .HasComment("Numerical value of the characteristic.")
                .HasColumnName("value");

            entity.HasOne(d => d.AccordanceCharacteristicLink).WithMany(p => p.AccordanceCharacteristicValues)
                .HasForeignKey(d => d.CharacteristicLinkId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_accordance_characteristic_link");
        });

        modelBuilder.Entity<AspNetPushNotificationSubscriber>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.AspNetPushNotificationSubscribers");

            entity.ToTable("AspNetPushNotificationSubscribers", "dbo", tb => tb.HasComment("Table for storing data about devices that are subscribers to push notifications."));

            entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);
            entity.Property(e => e.UserId).HasDefaultValue(0);

            entity.HasOne(d => d.AspNetUser).WithMany()
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_dbo.AspNetPushNotificationSubscribers_dbo.AspNetUsers_UserId");
        });

        modelBuilder.Entity<BinaryCharacteristicLink>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_binary_characteristic_link");

            entity.ToTable("binary_characteristic_link", tb => tb.HasComment("Contatins list of possible combinations of dependence characteristics parameters."));

            entity.HasIndex(e => new { e.BinaryCharacteristic, e.Link }, "uk_binary_characteristic_link").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("Unique identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasColumnName("id");
            entity.Property(e => e.BinaryCharacteristic)
                .HasComment("Characteristic enum numeric value.")
                .HasColumnName("binary_characteristic");
            entity.Property(e => e.Link)
                .HasComment("Link enum numeric value.")
                .HasColumnName("link");
        });

        modelBuilder.Entity<BinaryCharacteristicValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_binary_characteristic");

            entity.ToTable("binary_characteristic", tb => tb.HasComment("Contains numeric chracteristics of elements dependece based on their arrangement in sequence."));

            entity.HasIndex(e => e.SequenceId, "ix_binary_characteristic_chain_id");

            entity.HasIndex(e => e.SequenceId, "ix_binary_characteristic_first_sequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.SequenceId, e.CharacteristicLinkId }, "ix_binary_characteristic_sequence_id_characteristic_link_id_bri").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.SequenceId, e.FirstElementId, e.SecondElementId, e.CharacteristicLinkId }, "uk_binary_characteristic").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasColumnName("id");
            entity.Property(e => e.CharacteristicLinkId)
                .HasComment("Characteristic type id.")
                .HasColumnName("characteristic_link_id");
            entity.Property(e => e.FirstElementId)
                .HasComment("Id of the first element of the sequence for which the characteristic is calculated.")
                .HasColumnName("first_element_id");
            entity.Property(e => e.SecondElementId)
                .HasComment("Id of the second element of the sequence for which the characteristic is calculated.")
                .HasColumnName("second_element_id");
            entity.Property(e => e.SequenceId)
                .HasComment("Id of the sequence for which the characteristic is calculated.")
                .HasColumnName("chain_id");
            entity.Property(e => e.Value)
                .HasComment("Numerical value of the characteristic.")
                .HasColumnName("value");

            entity.HasOne(d => d.BinaryCharacteristicLink).WithMany(p => p.BinaryCharacteristicValues)
                .HasForeignKey(d => d.CharacteristicLinkId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_binary_characteristic_link");
        });

        modelBuilder.Entity<CalculationTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_task");

            entity.ToTable("task", tb => tb.HasComment("Contains information about computational tasks."));

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasColumnName("id");
            entity.Property(e => e.Completed)
                .HasComment("Task completion date and time.")
                .HasColumnName("completed");
            entity.Property(e => e.Created)
                .HasComment("Task creation date and time (filled trough trigger).")
                .HasColumnName("created");
            entity.Property(e => e.Description)
                .HasComment("Task description.")
                .HasColumnName("description");
            entity.Property(e => e.Started)
                .HasComment("Task beginning of computation date and time.")
                .HasColumnName("started");
            entity.Property(e => e.Status)
                .HasComment("Task status enum numeric value.")
                .HasColumnName("status");
            entity.Property(e => e.TaskType)
                .HasComment("Task type enum numeric value.")
                .HasColumnName("task_type");
            entity.Property(e => e.UserId)
                .HasComment("Creator user id.")
                .HasColumnName("user_id");

            entity.HasOne(d => d.AspNetUser).WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_task_user");
        });

        modelBuilder.Entity<CharacteristicValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_characteristic");

            entity.ToTable("full_characteristic", tb => tb.HasComment("Contains numeric chracteristics of complete sequences."));

            entity.HasIndex(e => e.SequenceId, "ix_characteristic_chain_id");

            entity.HasIndex(e => e.CharacteristicLinkId, "ix_characteristic_characteristic_type_link");

            entity.HasIndex(e => e.CharacteristicLinkId, "ix_full_characteristic_characteristic_link_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => e.SequenceId, "ix_full_characteristic_sequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.SequenceId, e.CharacteristicLinkId }, "uk_characteristic").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('characteristics_id_seq'::regclass)")
                .HasComment("Unique internal identifier.")
                .HasColumnName("id");
            entity.Property(e => e.CharacteristicLinkId)
                .HasComment("Characteristic type id.")
                .HasColumnName("characteristic_link_id");
            entity.Property(e => e.SequenceId)
                .HasComment("Id of the sequence for which the characteristic is calculated.")
                .HasColumnName("chain_id");
            entity.Property(e => e.Value)
                .HasComment("Numerical value of the characteristic.")
                .HasColumnName("value");

            entity.HasOne(d => d.FullCharacteristicLink).WithMany(p => p.CharacteristicValues)
                .HasForeignKey(d => d.CharacteristicLinkId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_full_characteristic_link");
        });

        modelBuilder.Entity<CommonSequence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_chain");

            entity.ToTable("chain", tb => tb.HasComment("Base table for all sequences that are stored in the database as alphabet and order."));

            entity.HasIndex(e => e.Alphabet, "ix_chain_alphabet").HasAnnotation("Npgsql:IndexMethod", "gin");

            entity.HasIndex(e => e.MatterId, "ix_chain_matter_id");

            entity.HasIndex(e => e.Notation, "ix_chain_notation_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier of the sequence.")
                .HasColumnName("id");
            entity.Property(e => e.Alphabet)
                .HasComment("Sequence's alphabet (array of elements ids).")
                .HasColumnName("alphabet");
            entity.Property(e => e.Building)
                .HasComment("Sequence's order.")
                .HasColumnName("building");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Sequence creation date and time (filled trough trigger).")
                .HasColumnName("created");
            entity.Property(e => e.Description)
                .HasComment("Description of the sequence.")
                .HasColumnName("description");
            entity.Property(e => e.MatterId)
                .HasComment("Id of the research object to which the sequence belongs.")
                .HasColumnName("matter_id");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).")
                .HasColumnName("modified");
            entity.Property(e => e.Notation)
                .HasComment("Notation of the sequence (words, letters, notes, nucleotides, etc.).")
                .HasColumnName("notation");
            entity.Property(e => e.RemoteDb)
                .HasComment("Enum numeric value of the remote db from which sequence is downloaded.")
                .HasColumnName("remote_db");
            entity.Property(e => e.RemoteId)
                .HasMaxLength(255)
                .HasComment("Id of the sequence in remote database.")
                .HasColumnName("remote_id");

            entity.HasOne(d => d.Matter).WithMany(p => p.Sequence)
                .HasForeignKey(d => d.MatterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_chain_matter");
        });

        modelBuilder.Entity<CongenericCharacteristicLink>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_congeneric_characteristic_link");

            entity.ToTable("congeneric_characteristic_link", tb => tb.HasComment("Contatins list of possible combinations of congeneric characteristics parameters."));

            entity.HasIndex(e => new { e.CongenericCharacteristic, e.Link, e.ArrangementType }, "uk_congeneric_characteristic_link").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasColumnName("id");
            entity.Property(e => e.ArrangementType)
                .HasComment("Arrangement type enum numeric value.")
                .HasColumnName("arrangement_type");
            entity.Property(e => e.CongenericCharacteristic)
                .HasComment("Characteristic enum numeric value.")
                .HasColumnName("congeneric_characteristic");
            entity.Property(e => e.Link)
                .HasComment("Link enum numeric value.")
                .HasColumnName("link");
        });

        modelBuilder.Entity<CongenericCharacteristicValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_congeneric_characteristic");

            entity.ToTable("congeneric_characteristic", tb => tb.HasComment("Contains numeric chracteristics of congeneric sequences."));

            entity.HasIndex(e => new { e.SequenceId, e.ElementId }, "fki_congeneric_characteristic_alphabet_element");

            entity.HasIndex(e => e.SequenceId, "ix_congeneric_characteristic_chain_id");

            entity.HasIndex(e => e.CharacteristicLinkId, "ix_congeneric_characteristic_characteristic_link_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => e.SequenceId, "ix_congeneric_characteristic_sequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.SequenceId, e.CharacteristicLinkId }, "ix_congeneric_characteristic_sequence_id_characteristic_link_id").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.SequenceId, e.ElementId }, "ix_congeneric_characteristic_sequence_id_element_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.SequenceId, e.ElementId, e.CharacteristicLinkId }, "uk_congeneric_characteristic").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasColumnName("id");
            entity.Property(e => e.CharacteristicLinkId)
                .HasComment("Characteristic type id.")
                .HasColumnName("characteristic_link_id");
            entity.Property(e => e.ElementId)
                .HasComment("Id of the element for which the characteristic is calculated.")
                .HasColumnName("element_id");
            entity.Property(e => e.SequenceId)
                .HasComment("Id of the sequence for which the characteristic is calculated.")
                .HasColumnName("chain_id");
            entity.Property(e => e.Value)
                .HasComment("Numerical value of the characteristic.")
                .HasColumnName("value");

            entity.HasOne(d => d.CongenericCharacteristicLink).WithMany(p => p.CongenericCharacteristicValues)
                .HasForeignKey(d => d.CharacteristicLinkId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_congeneric_characteristic_link");
        });

        modelBuilder.Entity<DataSequence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("data_chain_pkey");

            entity.ToTable("data_chain", tb => tb.HasComment("Contains sequences that represent time series and other ordered data arrays."));

            entity.HasIndex(e => e.Alphabet, "data_chain_alphabet_idx").HasAnnotation("Npgsql:IndexMethod", "gin");

            entity.HasIndex(e => e.MatterId, "data_chain_matter_id_idx");

            entity.HasIndex(e => e.Notation, "data_chain_notation_id_idx");

            entity.HasIndex(e => new { e.Notation, e.MatterId }, "uk_data_chain").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier of the sequence.")
                .HasColumnName("id");
            entity.Property(e => e.Alphabet)
                .HasComment("Sequence's alphabet (array of elements ids).")
                .HasColumnName("alphabet");
            entity.Property(e => e.Building)
                .HasComment("Sequence's order.")
                .HasColumnName("building");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Sequence creation date and time (filled trough trigger).")
                .HasColumnName("created");
            entity.Property(e => e.Description)
                .HasComment("Description of the sequence.")
                .HasColumnName("description");
            entity.Property(e => e.MatterId)
                .HasComment("Id of the research object to which the sequence belongs.")
                .HasColumnName("matter_id");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).")
                .HasColumnName("modified");
            entity.Property(e => e.Notation)
                .HasComment("Notation enum numeric value.")
                .HasColumnName("notation");
            entity.Property(e => e.RemoteDb)
                .HasComment("Enum numeric value of the remote db from which sequence is downloaded.")
                .HasColumnName("remote_db");
            entity.Property(e => e.RemoteId)
                .HasMaxLength(255)
                .HasComment("Id of the sequence in remote database.")
                .HasColumnName("remote_id");

            entity.HasOne(d => d.Matter).WithMany(p => p.DataSequence)
                .HasForeignKey(d => d.MatterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_data_chain_matter");
        });

        modelBuilder.Entity<DnaSequence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_dna_chain");

            entity.ToTable("dna_chain", tb => tb.HasComment("Contains sequences that represent genetic texts (DNA, RNA, gene sequecnes, etc)."));

            entity.HasIndex(e => e.Alphabet, "ix_dna_chain_alphabet").HasAnnotation("Npgsql:IndexMethod", "gin");

            entity.HasIndex(e => e.MatterId, "ix_dna_chain_matter_id");

            entity.HasIndex(e => e.Notation, "ix_dna_chain_notation_id");

            entity.HasIndex(e => new { e.MatterId, e.Notation }, "uk_dna_chain").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier of the sequence.")
                .HasColumnName("id");
            entity.Property(e => e.Alphabet)
                .HasComment("Sequence's alphabet (array of elements ids).")
                .HasColumnName("alphabet");
            entity.Property(e => e.Building)
                .HasComment("Sequence's order.")
                .HasColumnName("building");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Sequence creation date and time (filled trough trigger).")
                .HasColumnName("created");
            entity.Property(e => e.Description)
                .HasComment("Description of the sequence.")
                .HasColumnName("description");
            entity.Property(e => e.MatterId)
                .HasComment("Id of the research object to which the sequence belongs.")
                .HasColumnName("matter_id");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).")
                .HasColumnName("modified");
            entity.Property(e => e.Notation)
                .HasComment("Notation enum numeric value.")
                .HasColumnName("notation");
            entity.Property(e => e.Partial)
                .HasDefaultValue(false)
                .HasComment("Flag indicating whether sequence is partial or complete.")
                .HasColumnName("partial");
            entity.Property(e => e.RemoteDb)
                .HasComment("Enum numeric value of the remote db from which sequence is downloaded.")
                .HasColumnName("remote_db");
            entity.Property(e => e.RemoteId)
                .HasMaxLength(255)
                .HasComment("Id of the sequence in remote database.")
                .HasColumnName("remote_id");

            entity.HasOne(d => d.Matter).WithMany(p => p.DnaSequence)
                .HasForeignKey(d => d.MatterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_dna_chain_matter");
        });

        modelBuilder.Entity<Element>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_elements");

            entity.ToTable("element", tb => tb.HasComment("Base table for all elements that are stored in the database and used in alphabets of sequences."));

            entity.HasIndex(e => e.Notation, "ix_element_notation_id");

            entity.HasIndex(e => e.Value, "ix_element_value");

            entity.HasIndex(e => new { e.Value, e.Notation }, "uk_element_value_notation").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier of the element.")
                .HasColumnName("id");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Element creation date and time (filled trough trigger).")
                .HasColumnName("created");
            entity.Property(e => e.Description)
                .HasComment("Description of the element.")
                .HasColumnName("description");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).")
                .HasColumnName("modified");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasComment("Name of the element.")
                .HasColumnName("name");
            entity.Property(e => e.Notation)
                .HasComment("Notation enum numeric value.")
                .HasColumnName("notation");
            entity.Property(e => e.Value)
                .HasMaxLength(255)
                .HasComment("Content of the element.")
                .HasColumnName("value");
        });

        modelBuilder.Entity<Fmotif>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_fmotif");

            entity.ToTable("fmotif", tb => tb.HasComment("Contains elements that represent note sequences in form of formal motifs that are used as elements of segmented music sequences."));

            entity.HasIndex(e => e.Alphabet, "ix_fmotif_alphabet").HasAnnotation("Npgsql:IndexMethod", "gin");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier of the fmotif.")
                .HasColumnName("id");
            entity.Property(e => e.Alphabet)
                .HasComment("Fmotif's alphabet (array of notes ids).")
                .HasColumnName("alphabet");
            entity.Property(e => e.Building)
                .HasComment("Fmotif's order.")
                .HasColumnName("building");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Fmotif creation date and time (filled trough trigger).")
                .HasColumnName("created");
            entity.Property(e => e.Description)
                .HasComment("Fmotif description.")
                .HasColumnName("description");
            entity.Property(e => e.FmotifType)
                .HasComment("Fmotif type enum numeric value.")
                .HasColumnName("fmotif_type");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).")
                .HasColumnName("modified");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasComment("Fmotif name.")
                .HasColumnName("name");
            entity.Property(e => e.Notation)
                .HasComment("Fmotif notation enum numeric value (always 6).")
                .HasColumnName("notation");
            entity.Property(e => e.Value)
                .HasMaxLength(255)
                .HasComment("Fmotif hash value.")
                .HasColumnName("value");
        });

        modelBuilder.Entity<FullCharacteristicLink>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_full_characteristic_link");

            entity.ToTable("full_characteristic_link", tb => tb.HasComment("Contatins list of possible combinations of characteristics parameters."));

            entity.HasIndex(e => new { e.FullCharacteristic, e.Link, e.ArrangementType }, "uk_full_characteristic_link").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasColumnName("id");
            entity.Property(e => e.ArrangementType)
                .HasComment("Arrangement type enum numeric value.")
                .HasColumnName("arrangement_type");
            entity.Property(e => e.FullCharacteristic)
                .HasComment("Characteristic enum numeric value.")
                .HasColumnName("full_characteristic");
            entity.Property(e => e.Link)
                .HasComment("Link enum numeric value.")
                .HasColumnName("link");
        });

        modelBuilder.Entity<ImageSequence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_image_sequence");

            entity.ToTable("image_sequence", tb => tb.HasComment("Contains information on image transformations and order extraction. Does not store an actual order of image and used for reference by characteristics tables."));

            entity.HasIndex(e => e.MatterId, "ix_image_sequence_matter_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier of the image sequence.")
                .HasColumnName("id");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Sequence creation date and time (filled trough trigger).")
                .HasColumnName("created");
            entity.Property(e => e.ImageTransformations)
                .HasComment("Array of image transformations applied begore the extraction of the sequence.")
                .HasColumnName("image_transformations");
            entity.Property(e => e.MatrixTransformations)
                .HasComment("Array of matrix transformations applied begore the extraction of the sequence.")
                .HasColumnName("matrix_transformations");
            entity.Property(e => e.MatterId)
                .HasComment("Id of the research object (image) to which the sequence belongs.")
                .HasColumnName("matter_id");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).")
                .HasColumnName("modified");
            entity.Property(e => e.Notation)
                .HasComment("Notation enum numeric value.")
                .HasColumnName("notation");
            entity.Property(e => e.OrderExtractor)
                .HasComment("Order extractor enum numeric value used in the process of creation of the sequence.")
                .HasColumnName("order_extractor");
            entity.Property(e => e.RemoteDb)
                .HasComment("Enum numeric value of the remote db from which sequence is downloaded.")
                .HasColumnName("remote_db");
            entity.Property(e => e.RemoteId)
                .HasComment("Id of the sequence in remote database.")
                .HasColumnName("remote_id");

            entity.HasOne(d => d.Matter).WithMany(p => p.ImageSequence)
                .HasForeignKey(d => d.MatterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_image_sequence_matter");
        });

        modelBuilder.Entity<LiteratureSequence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_literature_chain");

            entity.ToTable("literature_chain", tb => tb.HasComment("Contains sequences that represent literary works and their various translations."));

            entity.HasIndex(e => e.Alphabet, "ix_literature_chain_alphabet").HasAnnotation("Npgsql:IndexMethod", "gin");

            entity.HasIndex(e => e.MatterId, "ix_literature_chain_matter_id");

            entity.HasIndex(e => new { e.MatterId, e.Language }, "ix_literature_chain_matter_language");

            entity.HasIndex(e => e.Notation, "ix_literature_chain_notation_id");

            entity.HasIndex(e => new { e.Notation, e.MatterId, e.Language, e.Translator }, "uk_literature_chain").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier of the sequence.")
                .HasColumnName("id");
            entity.Property(e => e.Alphabet)
                .HasComment("Sequence's alphabet (array of elements ids).")
                .HasColumnName("alphabet");
            entity.Property(e => e.Building)
                .HasComment("Sequence's order.")
                .HasColumnName("building");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Sequence creation date and time (filled trough trigger).")
                .HasColumnName("created");
            entity.Property(e => e.Description)
                .HasComment("Sequence description.")
                .HasColumnName("description");
            entity.Property(e => e.Language)
                .HasComment("Primary language of literary work.")
                .HasColumnName("language");
            entity.Property(e => e.MatterId)
                .HasComment("Id of the research object to which the sequence belongs.")
                .HasColumnName("matter_id");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).")
                .HasColumnName("modified");
            entity.Property(e => e.Notation)
                .HasComment("Notation enum numeric value.")
                .HasColumnName("notation");
            entity.Property(e => e.Original)
                .HasDefaultValue(true)
                .HasComment("Flag indicating if this sequence is in original language or was translated.")
                .HasColumnName("original");
            entity.Property(e => e.RemoteDb)
                .HasComment("Enum numeric value of the remote db from which sequence is downloaded.")
                .HasColumnName("remote_db");
            entity.Property(e => e.RemoteId)
                .HasMaxLength(255)
                .HasComment("Id of the sequence in remote database.")
                .HasColumnName("remote_id");
            entity.Property(e => e.Translator)
                .HasComment("Author of translation or automated translator.")
                .HasColumnName("translator");

            entity.HasOne(d => d.Matter).WithMany(p => p.LiteratureSequence)
                .HasForeignKey(d => d.MatterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_literature_chain_matter");
        });

        modelBuilder.Entity<Matter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_matter");

            entity.ToTable("matter", tb => tb.HasComment("Contains research objects, samples, texts, etc (one research object may be represented by several sequences)."));

            entity.HasIndex(e => e.Nature, "ix_matter_nature");

            entity.HasIndex(e => new { e.Name, e.Nature }, "uk_matter").IsUnique();

            entity.HasIndex(e => new { e.MultisequenceId, e.MultisequenceNumber }, "uk_matter_multisequence").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier of the research object.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasColumnName("id");
            entity.Property(e => e.CollectionCountry)
                .HasComment("Collection country of the genetic sequence.")
                .HasColumnName("collection_country");
            entity.Property(e => e.CollectionDate)
                .HasComment("Collection date of the genetic sequence.")
                .HasColumnName("collection_date");
            entity.Property(e => e.CollectionLocation)
                .HasComment("Collection location of the genetic sequence.")
                .HasColumnName("collection_location");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Record creation date and time (filled trough trigger).")
                .HasColumnName("created");
            entity.Property(e => e.Description)
                .HasComment("Description of the research object.")
                .HasColumnName("description");
            entity.Property(e => e.Group)
                .HasComment("Group enum numeric value.")
                .HasColumnName("group");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).")
                .HasColumnName("modified");
            entity.Property(e => e.MultisequenceId)
                .HasComment("Id of the parent multisequence.")
                .HasColumnName("multisequence_id");
            entity.Property(e => e.MultisequenceNumber)
                .HasComment("Serial number in multisequence.")
                .HasColumnName("multisequence_number");
            entity.Property(e => e.Name)
                .HasComment("Research object name.")
                .HasColumnName("name");
            entity.Property(e => e.Nature)
                .HasComment("Research object nature enum numeric value.")
                .HasColumnName("nature");
            entity.Property(e => e.SequenceType)
                .HasComment("Sequence type enum numeric value.")
                .HasColumnName("sequence_type");
            entity.Property(e => e.Source)
                .HasComment("Source of the genetic sequence.")
                .HasColumnName("source");

            entity.HasOne(d => d.Multisequence).WithMany(p => p.Matters)
                .HasForeignKey(d => d.MultisequenceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_matter_multisequence");

            entity.HasMany(d => d.Groups).WithMany(p => p.Matters)
                .UsingEntity<Dictionary<string, object>>(
                    "SequenceGroupMatter",
                    r => r.HasOne<SequenceGroup>().WithMany()
                        .HasForeignKey("GroupId")
                        .HasConstraintName("fk_matter_sequence_group"),
                    l => l.HasOne<Matter>().WithMany()
                        .HasForeignKey("MatterId")
                        .HasConstraintName("fk_sequence_group_matter"),
                    j =>
                    {
                        j.HasKey("MatterId", "GroupId").HasName("sequence_group_matter_pkey");
                        j.ToTable("sequence_group_matter", tb => tb.HasComment("Intermediate table for infromation on matters belonging to groups."));
                        j.IndexerProperty<long>("MatterId")
                            .HasComment("Research object id.")
                            .HasColumnName("matter_id");
                        j.IndexerProperty<int>("GroupId")
                            .HasComment("Sequence group id.")
                            .HasColumnName("group_id");
                    });
        });

        modelBuilder.Entity<Measure>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_measure");

            entity.ToTable("measure", tb => tb.HasComment("Contains elements that represent note sequences in form of measures (bars) that are used as elements of segmented music sequences."));

            entity.HasIndex(e => e.Alphabet, "ix_measure_alphabet").HasAnnotation("Npgsql:IndexMethod", "gin");

            entity.HasIndex(e => e.Notation, "ix_measure_notation_id");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier of the measure.")
                .HasColumnName("id");
            entity.Property(e => e.Alphabet)
                .HasComment("Measure alphabet (array of notes ids).")
                .HasColumnName("alphabet");
            entity.Property(e => e.Beatbase)
                .HasComment("Time signature lower numeral (Beat denominator).")
                .HasColumnName("beatbase");
            entity.Property(e => e.Beats)
                .HasComment("Time signature upper numeral (Beat numerator).")
                .HasColumnName("beats");
            entity.Property(e => e.Building)
                .HasComment("Measure order.")
                .HasColumnName("building");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Measure creation date and time (filled trough trigger).")
                .HasColumnName("created");
            entity.Property(e => e.Description)
                .HasComment("Description of the sequence.")
                .HasColumnName("description");
            entity.Property(e => e.Fifths)
                .HasComment("Key signature of the measure (negative value represents the number of flats (bemolles) and positive represents the number of sharps (diesis)).")
                .HasColumnName("fifths");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).")
                .HasColumnName("modified");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasComment("Measure name.")
                .HasColumnName("name");
            entity.Property(e => e.Notation)
                .HasComment("Measure notation enum numeric value (always 7).")
                .HasColumnName("notation");
            entity.Property(e => e.Value)
                .HasMaxLength(255)
                .HasComment("Measure hash code.")
                .HasColumnName("value");
            entity.Property(e => e.major).HasComment("Music mode of the measure. true  represents major and false represents minor.");
        });

        modelBuilder.Entity<Multisequence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("multisequence_pkey");

            entity.ToTable("multisequence", tb => tb.HasComment("Contains information on groups of related research objects (such as series of books, chromosomes of the same organism, etc) and their order in these groups."));

            entity.HasIndex(e => e.Name, "uk_multisequence_name").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasComment("Multisequence name.")
                .HasColumnName("name");
            entity.Property(e => e.Nature)
                .HasComment("Multisequence nature enum numeric value.")
                .HasColumnName("nature");
        });

        modelBuilder.Entity<MusicSequence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_music_chain");

            entity.ToTable("music_chain", tb => tb.HasComment("Contains sequences that represent musical works in form of note, fmotive or measure sequences."));

            entity.HasIndex(e => e.Alphabet, "ix_music_chain_alphabet").HasAnnotation("Npgsql:IndexMethod", "gin");

            entity.HasIndex(e => e.MatterId, "ix_music_chain_matter_id");

            entity.HasIndex(e => e.Notation, "ix_music_chain_notation_id");

            entity.HasIndex(e => new { e.MatterId, e.Notation, e.PauseTreatment, e.SequentialTransfer }, "uk_music_chain").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier.")
                .HasColumnName("id");
            entity.Property(e => e.Alphabet)
                .HasComment("Sequence's alphabet (array of elements ids).")
                .HasColumnName("alphabet");
            entity.Property(e => e.Building)
                .HasComment("Sequence's order.")
                .HasColumnName("building");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Sequence creation date and time (filled trough trigger).")
                .HasColumnName("created");
            entity.Property(e => e.Description)
                .HasComment("Sequence description.")
                .HasColumnName("description");
            entity.Property(e => e.MatterId)
                .HasComment("Id of the research object to which the sequence belongs.")
                .HasColumnName("matter_id");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).")
                .HasColumnName("modified");
            entity.Property(e => e.Notation)
                .HasComment("Notation enum numeric value.")
                .HasColumnName("notation");
            entity.Property(e => e.PauseTreatment)
                .HasComment("Pause treatment enum numeric value.")
                .HasColumnName("pause_treatment");
            entity.Property(e => e.RemoteDb)
                .HasComment("Enum numeric value of the remote db from which sequence is downloaded.")
                .HasColumnName("remote_db");
            entity.Property(e => e.RemoteId)
                .HasMaxLength(255)
                .HasComment("Id of the sequence in the remote database.")
                .HasColumnName("remote_id");
            entity.Property(e => e.SequentialTransfer)
                .HasDefaultValue(false)
                .HasComment("Flag indicating whether or not sequential transfer was used in sequence segmentation into fmotifs.")
                .HasColumnName("sequential_transfer");

            entity.HasOne(d => d.Matter).WithMany(p => p.MusicSequence)
                .HasForeignKey(d => d.MatterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_music_chain_matter");
        });

        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_note");

            entity.ToTable("note", tb => tb.HasComment("Contains elements that represent notes that are used as elements of music sequences."));

            entity.HasIndex(e => e.Notation, "ix_note_notation_id");

            entity.HasIndex(e => e.Value, "uk_note").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier.")
                .HasColumnName("id");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Measure creation date and time (filled trough trigger).")
                .HasColumnName("created");
            entity.Property(e => e.Denominator)
                .HasComment("Note duration fraction denominator.")
                .HasColumnName("denominator");
            entity.Property(e => e.Description)
                .HasComment("Note description.")
                .HasColumnName("description");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).")
                .HasColumnName("modified");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasComment("Note name.")
                .HasColumnName("name");
            entity.Property(e => e.Notation)
                .HasComment("Measure notation enum numeric value (always 8).")
                .HasColumnName("notation");
            entity.Property(e => e.Numerator)
                .HasComment("Note duration fraction numerator.")
                .HasColumnName("numerator");
            entity.Property(e => e.Tie)
                .HasComment("Note tie type enum numeric value.")
                .HasColumnName("tie");
            entity.Property(e => e.Triplet)
                .HasComment("Flag indicating if note is a part of triplet (tuplet).")
                .HasColumnName("triplet");
            entity.Property(e => e.Value)
                .HasMaxLength(255)
                .HasComment("Note hash code.")
                .HasColumnName("value");

            entity.HasMany(d => d.Pitches).WithMany(p => p.Notes)
                .UsingEntity<Dictionary<string, object>>(
                    "NotePitch",
                    r => r.HasOne<Pitch>().WithMany()
                        .HasForeignKey("PitchId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .HasConstraintName("fk_note_pitch_pitch"),
                    l => l.HasOne<Note>().WithMany()
                        .HasForeignKey("NoteId")
                        .HasConstraintName("fk_note_pitch_note"),
                    j =>
                    {
                        j.HasKey("NoteId", "PitchId").HasName("pk_note_pitch");
                        j.ToTable("note_pitch", tb => tb.HasComment("Intermediate table representing M:M relationship between note and pitch."));
                        j.IndexerProperty<long>("NoteId")
                            .HasComment("Note id.")
                            .HasColumnName("note_id");
                        j.IndexerProperty<int>("PitchId")
                            .HasComment("Pitch id.")
                            .HasColumnName("pitch_id");
                    });
        });

        modelBuilder.Entity<Pitch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_pitch");

            entity.ToTable("pitch", tb => tb.HasComment("Note's pitch."));

            entity.HasIndex(e => new { e.Octave, e.Instrument, e.Accidental, e.NoteSymbol }, "uk_pitch").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier of the pitch.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasColumnName("id");
            entity.Property(e => e.Accidental)
                .HasComment("Pitch key signature enum numeric value.")
                .HasColumnName("accidental");
            entity.Property(e => e.Instrument)
                .HasComment("Pitch instrument enum numeric value.")
                .HasColumnName("instrument");
            entity.Property(e => e.Midinumber)
                .HasComment("Unique number by midi standard.")
                .HasColumnName("midinumber");
            entity.Property(e => e.NoteSymbol)
                .HasComment("Note symbol enum numeric value.")
                .HasColumnName("note_symbol");
            entity.Property(e => e.Octave)
                .HasComment("Octave number.")
                .HasColumnName("octave");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_piece");

            entity.ToTable("position", tb => tb.HasComment("Contains information on additional fragment positions (for subsequences concatenated from several parts)."));

            entity.HasIndex(e => e.SubsequenceId, "ix_position_subsequence_id");

            entity.HasIndex(e => e.SubsequenceId, "ix_position_subsequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.SubsequenceId, e.Start, e.Length }, "uk_piece").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('piece_id_seq'::regclass)")
                .HasComment("Unique internal identifier.")
                .HasColumnName("id");
            entity.Property(e => e.Length)
                .HasComment("Fragment length.")
                .HasColumnName("length");
            entity.Property(e => e.Start)
                .HasComment("Index of the fragment beginning (from zero).")
                .HasColumnName("start");
            entity.Property(e => e.SubsequenceId)
                .HasComment("Parent subsequence id.")
                .HasColumnName("subsequence_id");

            entity.HasOne(d => d.Subsequence).WithMany(p => p.Position)
                .HasForeignKey(d => d.SubsequenceId)
                .HasConstraintName("fk_position_subsequence");
        });

        modelBuilder.Entity<SequenceAttribute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_chain_attribute");

            entity.ToTable("chain_attribute", tb => tb.HasComment("Contains chains' attributes and their values."));

            entity.HasIndex(e => e.SequenceId, "ix_sequence_attribute_sequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.SequenceId, e.Attribute, e.Value }, "uk_chain_attribute").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasColumnName("id");
            entity.Property(e => e.Attribute)
                .HasComment("Attribute enum numeric value.")
                .HasColumnName("attribute");
            entity.Property(e => e.SequenceId)
                .HasComment("Id of the sequence to which attribute belongs.")
                .HasColumnName("chain_id");
            entity.Property(e => e.Value)
                .HasComment("Text of the attribute.")
                .HasColumnName("value");

            entity.HasOne(d => d.Subsequence).WithMany(p => p.SequenceAttribute)
                .HasForeignKey(d => d.SequenceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SequenceGroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sequence_group_pkey");

            entity.ToTable("sequence_group", tb => tb.HasComment("Contains information about sequences groups."));

            entity.HasIndex(e => e.Name, "uk_sequence_group_name").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasColumnName("id");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Sequence group creation date and time (filled trough trigger).")
                .HasColumnName("created");
            entity.Property(e => e.CreatorId)
                .HasComment("Record creator user id.")
                .HasColumnName("creator_id");
            entity.Property(e => e.Modified)
                .HasComment("Record last change date and time (updated trough trigger).")
                .HasColumnName("modified");
            entity.Property(e => e.ModifierId)
                .HasComment("Record editor user id.")
                .HasColumnName("modifier_id");
            entity.Property(e => e.Name)
                .HasComment("Sequences group name.")
                .HasColumnName("name");
            entity.Property(e => e.Nature)
                .HasComment("Sequences group nature enum numeric value.")
                .HasColumnName("nature");
            entity.Property(e => e.SequenceGroupType)
                .HasComment("Sequence group type enum numeric value.")
                .HasColumnName("sequence_group_type");

            entity.HasOne(d => d.Creator).WithMany()
                .HasForeignKey(d => d.CreatorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_sequence_group_creator");

            entity.HasOne(d => d.Modifier).WithMany()
                .HasForeignKey(d => d.ModifierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_sequence_group_modifier");
        });

        modelBuilder.Entity<Subsequence>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_subsequence");

            entity.ToTable("subsequence", tb => tb.HasComment("Contains information on location and length of the fragments within complete sequences."));

            entity.HasIndex(e => new { e.SequenceId, e.Feature }, "ix_subsequence_chain_feature");

            entity.HasIndex(e => e.SequenceId, "ix_subsequence_chain_id");

            entity.HasIndex(e => e.Feature, "ix_subsequence_feature_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => e.SequenceId, "ix_subsequence_sequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.SequenceId, e.Feature }, "ix_subsequence_sequence_id_feature_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('elements_id_seq'::regclass)")
                .HasComment("Unique internal identifier.")
                .HasColumnName("id");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("now()")
                .HasComment("Sequence group creation date and time (filled trough trigger).")
                .HasColumnName("created");
            entity.Property(e => e.Feature)
                .HasComment("Subsequence feature enum numeric value.")
                .HasColumnName("feature");
            entity.Property(e => e.Length)
                .HasComment("Fragment length.")
                .HasColumnName("length");
            entity.Property(e => e.Modified)
                .HasDefaultValueSql("now()")
                .HasComment("Record last change date and time (updated trough trigger).")
                .HasColumnName("modified");
            entity.Property(e => e.Partial)
                .HasDefaultValue(false)
                .HasComment("Flag indicating whether subsequence is partial or complete.")
                .HasColumnName("partial");
            entity.Property(e => e.RemoteId)
                .HasMaxLength(255)
                .HasComment("Id of the subsequence in the remote database (ncbi or same as paren sequence remote db).")
                .HasColumnName("remote_id");
            entity.Property(e => e.SequenceId)
                .HasComment("Parent sequence id.")
                .HasColumnName("chain_id");
            entity.Property(e => e.Start)
                .HasComment("Index of the fragment beginning (from zero).")
                .HasColumnName("start");
        });

        modelBuilder.Entity<TaskResult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("task_result_pkey");

            entity.ToTable("task_result", tb => tb.HasComment("Contains JSON results of tasks calculation. Results are stored as key/value pairs."));

            entity.HasIndex(e => new { e.TaskId, e.Key }, "uk_task_result").IsUnique();

            entity.Property(e => e.Id)
                .HasComment("Unique internal identifier.")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasColumnName("id");
            entity.Property(e => e.Key)
                .HasComment("Results element name.")
                .HasColumnName("key");
            entity.Property(e => e.TaskId)
                .HasComment("Parent task id.")
                .HasColumnName("task_id");
            entity.Property(e => e.Value)
                .HasComment("Results element value (as json).")
                .HasColumnType("json")
                .HasColumnName("value");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskResult)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("fk_task_result_task");
        });

        OnModelCreatingGeneratedFunctions(modelBuilder);
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingGeneratedFunctions(ModelBuilder modelBuilder);
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
