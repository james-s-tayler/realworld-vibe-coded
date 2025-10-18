namespace Server.Web.Articles.Feed;

public class FeedRequestValidator : Validator<FeedRequest>
{
  public FeedRequestValidator()
  {
    /*RuleFor(x => x.Limit)
      .GreaterThan(0)
      .LessThanOrEqualTo(100)
      .OverridePropertyName("limit");
    
    RuleFor(x => x.Offset)
      .GreaterThanOrEqualTo(0)
      .OverridePropertyName("offset");*/
  }
}
