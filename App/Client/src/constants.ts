/**
 * Default profile image URL used when a user has no profile image set.
 */
export const DEFAULT_PROFILE_IMAGE = 'https://placehold.co/200';

/**
 * Schema length constraints for Article entity.
 * These must match the constants defined in Server.Core.ArticleAggregate.Article
 */
export const ARTICLE_CONSTRAINTS = {
  TITLE_MAX_LENGTH: 200,
  DESCRIPTION_MAX_LENGTH: 500,
  SLUG_MAX_LENGTH: 250,
} as const;

/**
 * Schema length constraints for User entity.
 * These must match the constants defined in Server.Core.UserAggregate.User
 */
export const USER_CONSTRAINTS = {
  EMAIL_MAX_LENGTH: 255,
  USERNAME_MIN_LENGTH: 2,
  USERNAME_MAX_LENGTH: 100,
  PASSWORD_MIN_LENGTH: 6,
  HASHED_PASSWORD_MAX_LENGTH: 255,
  BIO_MAX_LENGTH: 1000,
  IMAGE_URL_MAX_LENGTH: 500,
} as const;

/**
 * Schema length constraints for Tag entity.
 * These must match the constants defined in Server.Core.TagAggregate.Tag
 */
export const TAG_CONSTRAINTS = {
  NAME_MAX_LENGTH: 50,
} as const;

/**
 * Schema length constraints for Comment entity.
 * These must match the constants defined in Server.Core.ArticleAggregate.Comment
 */
export const COMMENT_CONSTRAINTS = {
  BODY_MAX_LENGTH: 5000,
} as const;
