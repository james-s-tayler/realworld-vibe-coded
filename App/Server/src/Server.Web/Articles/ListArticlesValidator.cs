using FluentValidation;

namespace Server.Web.Articles;

public class ListArticlesValidator : Validator<ListArticlesRequest>
{
  public ListArticlesValidator()
  {
    RuleLevelCascadeMode = CascadeMode.Stop;

    RuleFor(x => x.Limit)
      .Must(BeValidInteger)
      .When(x => !string.IsNullOrEmpty(x.Limit))
      .WithMessage("must be a valid integer")
      .DependentRules(() =>
      {
        RuleFor(x => x.Limit)
          .Must(x => int.Parse(x!) > 0)
          .When(x => !string.IsNullOrEmpty(x.Limit))
          .WithMessage("must be greater than 0")
          .Must(x => int.Parse(x!) <= 100)
          .When(x => !string.IsNullOrEmpty(x.Limit))
          .WithMessage("must be less than or equal to 100");
      })
      .OverridePropertyName("limit");

    RuleFor(x => x.Offset)
      .Must(BeValidInteger)
      .When(x => !string.IsNullOrEmpty(x.Offset))
      .WithMessage("must be a valid integer")
      .DependentRules(() =>
      {
        RuleFor(x => x.Offset)
          .Must(x => int.Parse(x!) >= 0)
          .When(x => !string.IsNullOrEmpty(x.Offset))
          .WithMessage("must be greater than or equal to 0");
      })
      .OverridePropertyName("offset");

    RuleFor(x => x.Tag)
      .NotEmpty()
      .When(x => x.Tag != null)
      .WithMessage("cannot be empty")
      .OverridePropertyName("tag");

    RuleFor(x => x.Author)
      .NotEmpty()
      .When(x => x.Author != null)
      .WithMessage("cannot be empty")
      .OverridePropertyName("author");

    RuleFor(x => x.Favorited)
      .NotEmpty()
      .When(x => x.Favorited != null)
      .WithMessage("cannot be empty")
      .OverridePropertyName("favorited");
  }

  private static bool BeValidInteger(string? value)
  {
    return int.TryParse(value, out _);
  }
}
