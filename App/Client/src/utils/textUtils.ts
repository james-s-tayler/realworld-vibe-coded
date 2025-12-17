/**
 * Truncates text to a maximum length and adds ellipsis if truncated.
 * @param text - The text to truncate
 * @param maxLength - Maximum length before truncation
 * @returns Truncated text with ellipsis if needed
 */
export const truncateText = (text: string, maxLength: number): string => {
  if (text.length <= maxLength) {
    return text;
  }
  return text.substring(0, maxLength) + '...';
};

/**
 * Truncates a username/email for display to prevent UI overflow.
 * Uses a reasonable max length (50 chars) for visual clarity.
 * @param username - The username or email to display
 * @returns Truncated username with ellipsis if needed
 */
export const truncateUsername = (username: string): string => {
  const MAX_USERNAME_DISPLAY_LENGTH = 50;
  return truncateText(username, MAX_USERNAME_DISPLAY_LENGTH);
};
