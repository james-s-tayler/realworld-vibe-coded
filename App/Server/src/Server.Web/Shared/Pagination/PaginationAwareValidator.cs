using FluentValidation;
using Server.SharedKernel.Pagination;

namespace Server.Web.Shared.Pagination;

public abstract class PaginationAwareValidator<T> : Validator<T>
  where T : IPaginatedRequest
{
  protected PaginationAwareValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Limit)
      .GreaterThan(0)
      .LessThanOrEqualTo(100)
      .OverridePropertyName("limit");

    RuleFor(x => x.Offset)
      .GreaterThanOrEqualTo(0)
      .OverridePropertyName("offset");
  }
}
