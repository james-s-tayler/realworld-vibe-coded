/**
 * Default profile image URL used when a user has no profile image set.
 */
export const DEFAULT_PROFILE_IMAGE = 'https://placehold.co/200';

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

export const SUPPORTED_LANGUAGES = [
  { id: 'en', label: 'English' },
  { id: 'ja', label: '日本語' },
] as const;
