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
      .LessThanOrEqualTo(100);

    RuleFor(x => x.Offset)
      .GreaterThanOrEqualTo(0);
  }
}
