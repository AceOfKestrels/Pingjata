using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pingjata.Persistence.Models;

namespace Pingjata.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<ChannelEntity> Channels { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureChannelEntity(modelBuilder.Entity<ChannelEntity>());
    }

    private static void ConfigureChannelEntity(EntityTypeBuilder<ChannelEntity> builder)
    {
        builder.ToTable("channels");

        builder.HasKey(b => b.ChannelId);

        builder.Property(b => b.ChannelId).HasColumnName("id").HasColumnType("text").IsRequired();
        builder.Property(b => b.GreetingMessage).HasColumnName("greeting").HasColumnType("text");
        builder.Property(b => b.CurrentCounter).HasColumnName("current_counter");
        builder.Property(b => b.Threshold).HasColumnName("current_threshold");
        builder.Property(b => b.ThresholdRange).HasColumnName("threshold_range").HasColumnType("text");
        builder.Property(b => b.WinnerId).HasColumnName("winner_id").HasColumnType("text");
        builder.Property(b => b.RoundEndedAt).HasColumnName("round_ended_at");

        builder.Property(b => b.RowVersion).HasColumnName("row_version").IsRowVersion();
    }
}