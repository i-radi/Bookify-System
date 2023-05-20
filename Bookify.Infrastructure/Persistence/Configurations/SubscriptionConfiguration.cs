namespace Bookify.Infrastructure.Persistence.Configurations;
internal class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.Property(e => e.CreatedOn).HasDefaultValueSql("GETDATE()");
    }
}