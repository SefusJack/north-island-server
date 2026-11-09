using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NorthIsland.Server.Features.PetCare;

/// <summary>Registers the pet care feature.</summary>
public static class PetCareFeature
{
    /// <summary>Registers the pet options and the pet, storage, reborn wish, voucher, express, forage, lost property and potential handlers.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="section">The configuration section bound to <see cref="PetOptions"/>.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddPetCare(this IServiceCollection services, IConfiguration section)
    {
        services.Configure<PetOptions>(section);
        services.AddSingleton<PetHandlers>();
        services.AddSingleton<PetStorageHandlers>();
        services.AddSingleton<RebornWishHandlers>();
        services.AddSingleton<PetVoucherHandlers>();
        services.AddSingleton<ExpressHandlers>();
        services.AddSingleton<ForageHandlers>();
        services.AddSingleton<LostPropertyHandlers>();
        services.AddSingleton<PotentialHandlers>();
        return services;
    }
}
