namespace Libiada.Database.Models;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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

        modelBuilder.Entity<AccordanceCharacteristicLink>(entity =>
        {
            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<AbstractSequenceEntity>(entity =>
        {
            entity.Property(e => e.Created).HasDefaultValueSql("now()");
            entity.Property(e => e.Modified).HasDefaultValueSql("now()");
            //entity.UseTptMappingStrategy();
            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<AccordanceCharacteristicValue>(entity =>
        {
            entity.HasIndex(e => e.FirstSequenceId, "ix_accordance_characteristic_first_sequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => e.SecondSequenceId, "ix_accordance_characteristic_second_sequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.FirstSequenceId, e.SecondSequenceId }, "ix_accordance_characteristic_sequences_ids_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.FirstSequenceId, e.SecondSequenceId, e.CharacteristicLinkId }, "ix_accordance_characteristic_sequences_ids_characteristic_link_").HasAnnotation("Npgsql:IndexMethod", "brin");

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
            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<BinaryCharacteristicValue>(entity =>
        {
            entity.HasIndex(e => e.SequenceId, "ix_binary_characteristic_first_sequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.SequenceId, e.CharacteristicLinkId }, "ix_binary_characteristic_sequence_id_characteristic_link_id_bri").HasAnnotation("Npgsql:IndexMethod", "brin");

            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<CalculationTask>(entity =>
        {
            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            entity.Property(e => e.Created).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<CharacteristicValue>(entity =>
        {
            entity.HasIndex(e => e.CharacteristicLinkId, "ix_full_characteristic_characteristic_link_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => e.SequenceId, "ix_full_characteristic_sequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<CombinedSequenceEntity>(entity =>
        {
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
            entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<CongenericCharacteristicValue>(entity =>
        {
            entity.HasIndex(e => e.CharacteristicLinkId, "ix_congeneric_characteristic_characteristic_link_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => e.SequenceId, "ix_congeneric_characteristic_sequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.SequenceId, e.CharacteristicLinkId }, "ix_congeneric_characteristic_sequence_id_characteristic_link_id").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.SequenceId, e.ElementId }, "ix_congeneric_characteristic_sequence_id_element_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        //modelBuilder.Entity<DataSequence>(entity =>
        //{
        //});

        //modelBuilder.Entity<DnaSequence>(entity =>
        //{
        //    entity.Property(e => e.Partial).HasDefaultValue(false);
        //});

        modelBuilder.Entity<Element>(entity =>
        {
            entity.UseTptMappingStrategy();
            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            entity.Property(e => e.Created).HasDefaultValueSql("now()");
            entity.Property(e => e.Modified).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Fmotif>(entity =>
        {
            entity.HasIndex(e => e.Alphabet, "ix_fmotif_alphabet").HasAnnotation("Npgsql:IndexMethod", "gin");
        });

        modelBuilder.Entity<FullCharacteristicLink>(entity =>
        {
            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<ImageSequence>(entity =>
        {
        });

        //modelBuilder.Entity<LiteratureSequence>(entity =>
        //{
        //    entity.Property(e => e.Original).HasDefaultValue(true);
        //});

        modelBuilder.Entity<Matter>(entity =>
        {
            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            entity.Property(e => e.Created).HasDefaultValueSql("now()");
            entity.Property(e => e.Modified).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Measure>(entity =>
        {
            entity.HasIndex(e => e.Alphabet, "ix_measure_alphabet").HasAnnotation("Npgsql:IndexMethod", "gin");
        });

        modelBuilder.Entity<Multisequence>(entity =>
        {
            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        //modelBuilder.Entity<MusicSequence>(entity =>
        //{
        //    entity.Property(e => e.SequentialTransfer).HasDefaultValue(false);
        //});

        modelBuilder.Entity<Note>(entity =>
        {
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
            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasIndex(e => e.SubsequenceId, "ix_position_subsequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        modelBuilder.Entity<SequenceAttribute>(entity =>
        {
            entity.HasIndex(e => e.SequenceId, "ix_sequence_attribute_sequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            //entity.Property(e => e.Id).HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        });

        
        modelBuilder.Entity<SequenceGroup>(entity =>
        {
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
            entity.HasIndex(e => e.Feature, "ix_subsequence_feature_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => e.SequenceId, "ix_subsequence_sequence_id_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

            entity.HasIndex(e => new { e.SequenceId, e.Notation, e.Feature }, "ix_subsequence_sequence_id_notation_feature_brin").HasAnnotation("Npgsql:IndexMethod", "brin");

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
