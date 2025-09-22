using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentCarServer.Domain.LoginTokens;

namespace RentCarServer.Infrastructure.Configurations;

internal sealed class LoginTokenConfigurations : IEntityTypeConfiguration<LoginToken>
{
    public void Configure(EntityTypeBuilder<LoginToken> builder)
    {
        builder.HasKey(x => x.Id);
        builder.OwnsOne(x => x.Token);
        builder.OwnsOne(x => x.IsActive);
        builder.OwnsOne(x => x.ExpiresDate);
    }
}
