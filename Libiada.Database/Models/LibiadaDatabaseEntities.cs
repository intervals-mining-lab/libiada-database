namespace Libiada.Database.Models;

using Libiada.Core.Core;
using Libiada.Core.Core.Characteristics.Calculators.AccordanceCalculators;
using Libiada.Core.Core.Characteristics.Calculators.BinaryCalculators;
using Libiada.Core.Core.SimpleTypes;
using Libiada.Core.Extensions;

using Libiada.Database.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public partial class LibiadaDatabaseEntities : IdentityDbContext<AspNetUser, IdentityRole<int>, int>
{
    public LibiadaDatabaseEntities(DbContextOptions<LibiadaDatabaseEntities> options)
        : base(options)
    {
    }

    public virtual DbSet<AbstractSequenceEntity> AbstractSequenceEntities { get; set; }

    public virtual DbSet<AccordanceCharacteristicLink> AccordanceCharacteristicLinks { get; set; }

    public virtual DbSet<AccordanceCharacteristicValue> AccordanceCharacteristicValues { get; set; }

    public virtual DbSet<AspNetPushNotificationSubscriber> AspNetPushNotificationSubscribers { get; set; }

    public virtual DbSet<BinaryCharacteristicLink> BinaryCharacteristicLinks { get; set; }

    public virtual DbSet<BinaryCharacteristicValue> BinaryCharacteristicValues { get; set; }

    public virtual DbSet<CalculationTask> CalculationTasks { get; set; }

    public virtual DbSet<CharacteristicValue> CharacteristicValues { get; set; }

    public virtual DbSet<CombinedSequenceEntity> CombinedSequenceEntities { get; set; }

    public virtual DbSet<CongenericCharacteristicLink> CongenericCharacteristicLinks { get; set; }

    public virtual DbSet<CongenericCharacteristicValue> CongenericCharacteristicValues { get; set; }

    public virtual DbSet<Element> Elements { get; set; }

    public virtual DbSet<Fmotif> Fmotifs { get; set; }

    public virtual DbSet<FullCharacteristicLink> FullCharacteristicLinks { get; set; }

    public virtual DbSet<ImageSequence> ImageSequences { get; set; }

    public virtual DbSet<Matter> Matters { get; set; }

    public virtual DbSet<Measure> Measures { get; set; }

    public virtual DbSet<Multisequence> Multisequences { get; set; }

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

        modelBuilder.Entity<AbstractSequenceEntity>(entity =>
        {
            entity.ToTable(t => t.HasCheckConstraint("chk_remote_id", "remote_db IS NULL AND remote_id IS NULL OR remote_db IS NOT NULL AND remote_id IS NOT NULL"));

            entity.Property(e => e.Created).HasDefaultValueSql("now()");
            entity.Property(e => e.Modified).HasDefaultValueSql("now()");

            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<AccordanceCharacteristicLink>(entity =>
        {
            entity.ToTable(t => t.HasCheckConstraint("chk_accordance_characteristic",
                @$"accordance_characteristic IN ({(byte)AccordanceCharacteristic.PartialComplianceDegree},
                                                 {(byte)AccordanceCharacteristic.MutualComplianceDegree})"));

            entity.ToTable(t => t.HasCheckConstraint("chk_accordance_characteristic_link", @$"link IN ({(byte)Link.Start},
                                                                                                       {(byte)Link.End}, 
                                                                                                       {(byte)Link.CycleStart}, 
                                                                                                       {(byte)Link.CycleEnd})"));

            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<AccordanceCharacteristicValue>(entity =>
        {
            entity.HasIndex(e => e.FirstSequenceId, "ix_accordance_characteristic_first_sequence_id_brin")
                  .HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => e.SecondSequenceId, "ix_accordance_characteristic_second_sequence_id_brin")
                  .HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.FirstSequenceId, e.SecondSequenceId }, "ix_accordance_characteristic_sequences_ids_brin")
                  .HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.FirstSequenceId, e.SecondSequenceId, e.CharacteristicLinkId },
                "ix_accordance_characteristic_sequences_ids_characteristic_link_")
                  .HasAnnotation("Npgsql:IndexMethod", "brin");

            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<AspNetPushNotificationSubscriber>(entity =>
        {
            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.Property(e => e.Created).HasDefaultValueSql("now()");
            entity.Property(e => e.Modified).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<BinaryCharacteristicLink>(entity =>
        {
            entity.ToTable(t => t.HasCheckConstraint("chk_binary_characteristic",
                @$"binary_characteristic IN ({(byte)BinaryCharacteristic.GeometricMean},
                                             {(byte)BinaryCharacteristic.InvolvedPartialDependenceCoefficient},
                                             {(byte)BinaryCharacteristic.MutualDependenceCoefficient},
                                             {(byte)BinaryCharacteristic.NormalizedPartialDependenceCoefficient},
                                             {(byte)BinaryCharacteristic.PartialDependenceCoefficient},
                                             {(byte)BinaryCharacteristic.Redundancy})"));

            entity.ToTable(t => t.HasCheckConstraint("chk_binary_characteristic_link", @$"link IN ({(byte)Link.Start},
                                                                                                   {(byte)Link.End},
                                                                                                   {(byte)Link.Both})"));

            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<BinaryCharacteristicValue>(entity =>
        {
            entity.HasIndex(e => e.SequenceId, "ix_binary_characteristic_first_sequence_id_brin")
                  .HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.SequenceId, e.CharacteristicLinkId }, "ix_binary_characteristic_sequence_id_characteristic_link_id_bri")
                  .HasAnnotation("Npgsql:IndexMethod", "brin");

            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<CalculationTask>(entity =>
        {
            var taskTypesIds = EnumExtensions.ToArray<TaskType>().Select(n => (byte)n);
            string taskTypesIdsString = string.Join(", ", taskTypesIds);
            entity.ToTable(t => t.HasCheckConstraint("chk_task_type", $"task_type IN ({taskTypesIdsString})"));

            var taskStatusesIds = EnumExtensions.ToArray<TaskState>().Select(n => (byte)n);
            string taskStatusesIdsString = string.Join(", ", taskStatusesIds);
            entity.ToTable(t => t.HasCheckConstraint("chk_task_status", $"status IN ({taskStatusesIdsString})"));

            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            entity.Property(e => e.Created).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<CharacteristicValue>(entity =>
        {
            entity.HasIndex(e => e.CharacteristicLinkId, "ix_full_characteristic_characteristic_link_id_brin")
                  .HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => e.SequenceId, "ix_full_characteristic_sequence_id_brin")
                  .HasAnnotation("Npgsql:IndexMethod", "brin");

            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<CombinedSequenceEntity>(entity =>
        {
            // TODO: add check constraints

            entity.HasIndex(e => e.Alphabet, "ix_chain_alphabet").HasAnnotation("Npgsql:IndexMethod", "gin");

            entity.HasOne(s => s.Matter)
                  .WithMany(r => r.Sequences)
                  .HasForeignKey(s => new { s.MatterId, s.Nature })
                  .HasPrincipalKey(r => new { r.Id, r.Nature });

            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            entity.Property(e => e.Partial).HasDefaultValue(false);
            entity.Property(e => e.Original).HasDefaultValue(true);
            entity.Property(e => e.SequentialTransfer).HasDefaultValue(false);
        });

        modelBuilder.Entity<CongenericCharacteristicLink>(entity =>
        {
            // TODO: try to formulate check constraints including arrangement type

            // entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<CongenericCharacteristicValue>(entity =>
        {
            entity.HasIndex(e => e.CharacteristicLinkId, "ix_congeneric_characteristic_characteristic_link_id_brin")
                  .HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => e.SequenceId, "ix_congeneric_characteristic_sequence_id_brin")
                  .HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.SequenceId, e.CharacteristicLinkId }, "ix_congeneric_characteristic_sequence_id_characteristic_link_id")
                  .HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.SequenceId, e.ElementId }, "ix_congeneric_characteristic_sequence_id_element_id_brin")
                  .HasAnnotation("Npgsql:IndexMethod", "brin");

            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<Element>(entity =>
        {
            var notationsIds = EnumExtensions.ToArray<Notation>().Select(n => (byte)n);
            string notationsIdsString = string.Join(", ", notationsIds);
            entity.ToTable(t => t.HasCheckConstraint("chk_element_notation", $"notation IN ({notationsIdsString})"));

            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            entity.Property(e => e.Created).HasDefaultValueSql("now()");
            entity.Property(e => e.Modified).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Fmotif>(entity =>
        {
            var fmotifTypesIds = EnumExtensions.ToArray<FmotifType>().Select(n => (byte)n);
            string fmotifTypesIdsString = string.Join(", ", fmotifTypesIds);
            entity.ToTable(t => t.HasCheckConstraint("chk_fmotif_type", $"fmotif_type IN ({fmotifTypesIdsString})"));


            entity.HasIndex(e => e.Alphabet, "ix_fmotif_alphabet")
                  .HasAnnotation("Npgsql:IndexMethod", "gin");
        });

        modelBuilder.Entity<FullCharacteristicLink>(entity =>
        {
            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<ImageSequence>(entity =>
        {
        });

        modelBuilder.Entity<Matter>(entity =>
        {
            entity.ToTable(t => t.HasCheckConstraint("chk_multisequence_reference",
                "(multisequence_id IS NULL AND multisequence_number IS NULL) OR (multisequence_id IS NOT NULL AND multisequence_number IS NOT NULL)"));

            var naturesIds = EnumExtensions.ToArray<Nature>().Select(n => (byte)n);
            string naturesIdsString = string.Join(", ", naturesIds);
            entity.ToTable(t => t.HasCheckConstraint("chk_research_object_nature", $"nature IN ({naturesIdsString})"));


            var sequenceTypesIds = EnumExtensions.ToArray<SequenceType>().Select(n => (byte)n);
            string sequenceTypesIdsString = string.Join(", ", sequenceTypesIds);
            entity.ToTable(t => t.HasCheckConstraint("chk_research_object_sequence_type", $"sequence_type IN ({sequenceTypesIdsString})"));

            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            entity.Property(e => e.Created).HasDefaultValueSql("now()");
            entity.Property(e => e.Modified).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Measure>(entity =>
        {
            entity.HasIndex(e => e.Alphabet, "ix_measure_alphabet")
                  .HasAnnotation("Npgsql:IndexMethod", "gin");
        });

        modelBuilder.Entity<Multisequence>(entity =>
        {
            var naturesIds = EnumExtensions.ToArray<Nature>().Select(n => (byte)n);
            string naturesIdsString = string.Join(", ", naturesIds);
            entity.ToTable(t => t.HasCheckConstraint("chk_multisequence_nature", $"nature IN ({naturesIdsString})"));

            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<Note>(entity =>
        {            
            var tiesIds = EnumExtensions.ToArray<Tie>().Select(n => (byte)n);
            string tiesIdsString = string.Join(", ", tiesIds);
            entity.ToTable(t => t.HasCheckConstraint("chk_note_tie", $"tie IN ({tiesIdsString})"));

            entity.HasMany(d => d.Pitches)
                  .WithMany(p => p.Notes)
                  .UsingEntity("note_pitch",
                               j =>
                                 {
                                     j.Property("NoteId").HasColumnName("note_id");
                                     j.Property("PitchId").HasColumnName("pitch_id");
                                 });
        });

        modelBuilder.Entity<Pitch>(entity =>
        {
            var accidentalsIds = EnumExtensions.ToArray<Accidental>().Select(n => (sbyte)n);
            string accidentalsIdsString = string.Join(", ", accidentalsIds);
            entity.ToTable(t => t.HasCheckConstraint("chk_pitch_accidental", $"accidental IN ({accidentalsIdsString})"));

            var noteSymbolsIds = EnumExtensions.ToArray<NoteSymbol>().Select(n => (byte)n);
            string noteSymbolsIdsString = string.Join(", ", noteSymbolsIds);
            entity.ToTable(t => t.HasCheckConstraint("chk_pitch_note_symbol", $"note_symbol IN ({noteSymbolsIdsString})"));

            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasIndex(e => e.SubsequenceId, "ix_position_subsequence_id_brin")
                  .HasAnnotation("Npgsql:IndexMethod", "brin");

            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<SequenceAttribute>(entity =>
        {
            var attributesIds = EnumExtensions.ToArray<AnnotationAttribute>().Select(n => (byte)n);
            string attributesIdsString = string.Join(", ", attributesIds);
            entity.ToTable(t => t.HasCheckConstraint("chk_sequence_attribute_attribute", $"attribute IN ({attributesIdsString})"));

            entity.HasIndex(e => e.SequenceId, "ix_sequence_attribute_sequence_id_brin")
                  .HasAnnotation("Npgsql:IndexMethod", "brin");

            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });


        modelBuilder.Entity<SequenceGroup>(entity =>
        {
            var naturesIds = EnumExtensions.ToArray<Nature>().Select(n => (byte)n);
            string naturesIdsString = string.Join(", ", naturesIds);
            entity.ToTable(t => t.HasCheckConstraint("chk_sequence_group_nature", $"nature IN ({naturesIdsString})"));

            var sequenceGroupTypesIds = EnumExtensions.ToArray<SequenceGroupType>().Select(n => (byte)n);
            string sequenceGroupTypesString = string.Join(", ", sequenceGroupTypesIds);
            entity.ToTable(t => t.HasCheckConstraint("chk_sequence_group_type", $"sequence_group_type IN ({sequenceGroupTypesString})"));

            var sequenceTypesIds = EnumExtensions.ToArray<SequenceType>().Select(n => (byte)n);
            string sequenceTypesIdsString = string.Join(", ", sequenceTypesIds);
            entity.ToTable(t => t.HasCheckConstraint("chk_sequence_group_sequence_type", $"sequence_type IN ({sequenceTypesIdsString})"));

            var groupsIds = EnumExtensions.ToArray<Group>().Select(n => (byte)n);
            string groupsIdsString = string.Join(", ", groupsIds);
            entity.ToTable(t => t.HasCheckConstraint("chk_sequence_group_group", $"group IN ({groupsIdsString})"));

            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            entity.Property(e => e.Created).HasDefaultValueSql("now()");
            entity.Property(e => e.Modified).HasDefaultValueSql("now()");

            entity.HasMany(d => d.Matters)
                  .WithMany(p => p.Groups)
                  .UsingEntity("sequence_group_research_object",
                               j =>
                                 {
                                     j.Property("MatterId").HasColumnName("research_object_id");
                                     j.Property("GroupId").HasColumnName("group_id");
                                 });
        });

        modelBuilder.Entity<Subsequence>(entity =>
        {
            var featuresIds = EnumExtensions.ToArray<Feature>().Select(n => (byte)n);
            string featuresIdsString = string.Join(", ", featuresIds);
            entity.ToTable(t => t.HasCheckConstraint("chk_subsequence_feature", $"feature IN ({featuresIdsString})"));

            var notationsIds = EnumExtensions.ToArray<Notation>().Select(n => (byte)n);
            string notationsIdsString = string.Join(", ", notationsIds);
            entity.ToTable(t => t.HasCheckConstraint("chk_subsequence_notation", $"notation IN ({notationsIdsString})"));

            entity.HasIndex(e => e.Feature, "ix_subsequence_feature_brin")
                  .HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => e.SequenceId, "ix_subsequence_sequence_id_brin")
                  .HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.SequenceId, e.Notation, e.Feature }, "ix_subsequence_sequence_id_notation_feature_brin")
                  .HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.Property(e => e.Partial).HasDefaultValue(false);
        });

        modelBuilder.Entity<TaskResult>(entity =>
        {
            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        OnModelCreatingGeneratedFunctions(modelBuilder);
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingGeneratedFunctions(ModelBuilder modelBuilder);
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
