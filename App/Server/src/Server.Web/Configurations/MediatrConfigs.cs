using System.Reflection;
using Server.Core.ArticleAggregate;
using Server.Core.ArticleAggregate.Dtos;
using Server.Core.UserAggregate;
using Server.UseCases.Articles.Comments.Create;
using Server.UseCases.Articles.Comments.Delete;
using Server.UseCases.Articles.Comments.Get;
using Server.UseCases.Articles.Create;
using Server.UseCases.Articles.Delete;
using Server.UseCases.Articles.Favorite;
using Server.UseCases.Articles.Feed;
using Server.UseCases.Articles.Get;
using Server.UseCases.Articles.List;
using Server.UseCases.Articles.Unfavorite;
using Server.UseCases.Articles.Update;
using Server.UseCases.ErrorTest;
using Server.UseCases.Profiles.Follow;
using Server.UseCases.Profiles.Get;
using Server.UseCases.Profiles.Unfollow;
using Server.UseCases.Tags;
using Server.UseCases.Tags.List;
using Server.UseCases.Users.GetCurrent;
using Server.UseCases.Users.Login;
using Server.UseCases.Users.Register;
using Server.UseCases.Users.Update;

namespace Server.Web.Configurations;

public static class MediatrConfigs
{
  public static IServiceCollection AddMediatrConfigs(this IServiceCollection services)
  {
    var mediatRAssemblies = new[]
      {
        Assembly.GetAssembly(typeof(User)), // Core
        Assembly.GetAssembly(typeof(RegisterUserCommand)) // UseCases
      };

    services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(mediatRAssemblies!))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
            .AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

    // Register TransactionBehavior and ExceptionHandlingBehavior explicitly for each command/query type
    // This is required because these behaviors use constrained generic parameters (IResultRequest<T>)
    // which cannot be resolved through open generic registration

    // Article Commands
    services.AddScoped<IPipelineBehavior<CreateArticleCommand, Result<Article>>, TransactionBehavior<CreateArticleCommand, Article>>();
    services.AddScoped<IPipelineBehavior<CreateArticleCommand, Result<Article>>, ExceptionHandlingBehavior<CreateArticleCommand, Article>>();

    services.AddScoped<IPipelineBehavior<DeleteArticleCommand, Result<Unit>>, TransactionBehavior<DeleteArticleCommand, Unit>>();
    services.AddScoped<IPipelineBehavior<DeleteArticleCommand, Result<Unit>>, ExceptionHandlingBehavior<DeleteArticleCommand, Unit>>();

    services.AddScoped<IPipelineBehavior<FavoriteArticleCommand, Result<Article>>, TransactionBehavior<FavoriteArticleCommand, Article>>();
    services.AddScoped<IPipelineBehavior<FavoriteArticleCommand, Result<Article>>, ExceptionHandlingBehavior<FavoriteArticleCommand, Article>>();

    services.AddScoped<IPipelineBehavior<UnfavoriteArticleCommand, Result<Article>>, TransactionBehavior<UnfavoriteArticleCommand, Article>>();
    services.AddScoped<IPipelineBehavior<UnfavoriteArticleCommand, Result<Article>>, ExceptionHandlingBehavior<UnfavoriteArticleCommand, Article>>();

    services.AddScoped<IPipelineBehavior<UpdateArticleCommand, Result<Article>>, TransactionBehavior<UpdateArticleCommand, Article>>();
    services.AddScoped<IPipelineBehavior<UpdateArticleCommand, Result<Article>>, ExceptionHandlingBehavior<UpdateArticleCommand, Article>>();

    // Comment Commands
    services.AddScoped<IPipelineBehavior<CreateCommentCommand, Result<CommentResponse>>, TransactionBehavior<CreateCommentCommand, CommentResponse>>();
    services.AddScoped<IPipelineBehavior<CreateCommentCommand, Result<CommentResponse>>, ExceptionHandlingBehavior<CreateCommentCommand, CommentResponse>>();

    services.AddScoped<IPipelineBehavior<DeleteCommentCommand, Result<Unit>>, TransactionBehavior<DeleteCommentCommand, Unit>>();
    services.AddScoped<IPipelineBehavior<DeleteCommentCommand, Result<Unit>>, ExceptionHandlingBehavior<DeleteCommentCommand, Unit>>();

    // Article Queries
    services.AddScoped<IPipelineBehavior<GetArticleQuery, Result<Article>>, TransactionBehavior<GetArticleQuery, Article>>();
    services.AddScoped<IPipelineBehavior<GetArticleQuery, Result<Article>>, ExceptionHandlingBehavior<GetArticleQuery, Article>>();

    services.AddScoped<IPipelineBehavior<ListArticlesQuery, Result<IEnumerable<Article>>>, TransactionBehavior<ListArticlesQuery, IEnumerable<Article>>>();
    services.AddScoped<IPipelineBehavior<ListArticlesQuery, Result<IEnumerable<Article>>>, ExceptionHandlingBehavior<ListArticlesQuery, IEnumerable<Article>>>();

    services.AddScoped<IPipelineBehavior<GetFeedQuery, Result<IEnumerable<Article>>>, TransactionBehavior<GetFeedQuery, IEnumerable<Article>>>();
    services.AddScoped<IPipelineBehavior<GetFeedQuery, Result<IEnumerable<Article>>>, ExceptionHandlingBehavior<GetFeedQuery, IEnumerable<Article>>>();

    services.AddScoped<IPipelineBehavior<GetCommentsQuery, Result<CommentsResponse>>, TransactionBehavior<GetCommentsQuery, CommentsResponse>>();
    services.AddScoped<IPipelineBehavior<GetCommentsQuery, Result<CommentsResponse>>, ExceptionHandlingBehavior<GetCommentsQuery, CommentsResponse>>();

    // User Commands
    services.AddScoped<IPipelineBehavior<RegisterUserCommand, Result<User>>, TransactionBehavior<RegisterUserCommand, User>>();
    services.AddScoped<IPipelineBehavior<RegisterUserCommand, Result<User>>, ExceptionHandlingBehavior<RegisterUserCommand, User>>();

    services.AddScoped<IPipelineBehavior<UpdateUserCommand, Result<User>>, TransactionBehavior<UpdateUserCommand, User>>();
    services.AddScoped<IPipelineBehavior<UpdateUserCommand, Result<User>>, ExceptionHandlingBehavior<UpdateUserCommand, User>>();

    services.AddScoped<IPipelineBehavior<FollowUserCommand, Result<User>>, TransactionBehavior<FollowUserCommand, User>>();
    services.AddScoped<IPipelineBehavior<FollowUserCommand, Result<User>>, ExceptionHandlingBehavior<FollowUserCommand, User>>();

    services.AddScoped<IPipelineBehavior<UnfollowUserCommand, Result<User>>, TransactionBehavior<UnfollowUserCommand, User>>();
    services.AddScoped<IPipelineBehavior<UnfollowUserCommand, Result<User>>, ExceptionHandlingBehavior<UnfollowUserCommand, User>>();

    // User Queries
    services.AddScoped<IPipelineBehavior<GetCurrentUserQuery, Result<User>>, TransactionBehavior<GetCurrentUserQuery, User>>();
    services.AddScoped<IPipelineBehavior<GetCurrentUserQuery, Result<User>>, ExceptionHandlingBehavior<GetCurrentUserQuery, User>>();

    services.AddScoped<IPipelineBehavior<LoginUserQuery, Result<User>>, TransactionBehavior<LoginUserQuery, User>>();
    services.AddScoped<IPipelineBehavior<LoginUserQuery, Result<User>>, ExceptionHandlingBehavior<LoginUserQuery, User>>();

    services.AddScoped<IPipelineBehavior<GetProfileQuery, Result<User>>, TransactionBehavior<GetProfileQuery, User>>();
    services.AddScoped<IPipelineBehavior<GetProfileQuery, Result<User>>, ExceptionHandlingBehavior<GetProfileQuery, User>>();

    // Tag Queries
    services.AddScoped<IPipelineBehavior<ListTagsQuery, Result<TagsResponse>>, TransactionBehavior<ListTagsQuery, TagsResponse>>();
    services.AddScoped<IPipelineBehavior<ListTagsQuery, Result<TagsResponse>>, ExceptionHandlingBehavior<ListTagsQuery, TagsResponse>>();

    // Error Test Queries
    services.AddScoped<IPipelineBehavior<ThrowConcurrencyQuery, Result<string>>, TransactionBehavior<ThrowConcurrencyQuery, string>>();
    services.AddScoped<IPipelineBehavior<ThrowConcurrencyQuery, Result<string>>, ExceptionHandlingBehavior<ThrowConcurrencyQuery, string>>();

    services.AddScoped<IPipelineBehavior<ThrowConcurrencyNonGenericQuery, Result<Unit>>, TransactionBehavior<ThrowConcurrencyNonGenericQuery, Unit>>();
    services.AddScoped<IPipelineBehavior<ThrowConcurrencyNonGenericQuery, Result<Unit>>, ExceptionHandlingBehavior<ThrowConcurrencyNonGenericQuery, Unit>>();

    services.AddScoped<IPipelineBehavior<ThrowInUseCaseQuery, Result<string>>, TransactionBehavior<ThrowInUseCaseQuery, string>>();
    services.AddScoped<IPipelineBehavior<ThrowInUseCaseQuery, Result<string>>, ExceptionHandlingBehavior<ThrowInUseCaseQuery, string>>();

    services.AddScoped<IPipelineBehavior<ThrowInUseCaseNonGenericQuery, Result<Unit>>, TransactionBehavior<ThrowInUseCaseNonGenericQuery, Unit>>();
    services.AddScoped<IPipelineBehavior<ThrowInUseCaseNonGenericQuery, Result<Unit>>, ExceptionHandlingBehavior<ThrowInUseCaseNonGenericQuery, Unit>>();

    return services;
  }
}
