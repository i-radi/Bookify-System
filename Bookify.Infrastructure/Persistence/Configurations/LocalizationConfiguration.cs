namespace Bookify.Infrastructure.Persistence.Configurations;
internal class LocalizationConfiguration : IEntityTypeConfiguration<Localization>
{
    public void Configure(EntityTypeBuilder<Localization> builder)
    {
        builder.HasKey(e => new { e.LocalizationSetId, e.CultureCode });

        builder.Property(e => e.CultureCode).HasMaxLength(20);
    }
}