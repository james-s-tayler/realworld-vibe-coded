using FluentValidation;
using Server.Web.Shared.Pagination;

namespace Server.Web.Articles.List;

public class ListArticlesValidator : PaginationAwareValidator<ListArticlesRequest>
{
  public ListArticlesValidator()
  {
    RuleFor(x => x.Tag)
      .NotEmpty()
      .When(x => x.Tag != null);

    RuleFor(x => x.Author)
      .NotEmpty()
      .When(x => x.Author != null);

    RuleFor(x => x.Favorited)
      .NotEmpty()
      .When(x => x.Favorited != null);
  }
}
