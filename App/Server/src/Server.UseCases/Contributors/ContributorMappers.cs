using Server.Core.ContributorAggregate;

namespace Server.UseCases.Contributors;

/// <summary>
/// Static mappers for Contributor-related entities to DTOs to reduce duplication across handlers
/// </summary>
public static class ContributorMappers
{
  /// <summary>
  /// Maps Contributor entity to ContributorDTO
  /// </summary>
  public static ContributorDTO MapToDto(Contributor contributor)
  {
    return new ContributorDTO(
      contributor.Id,
      contributor.Name,
      contributor.PhoneNumber?.Number ?? ""
    );
  }
}