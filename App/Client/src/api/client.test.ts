import { describe, expect,it } from 'vitest';

import { ApiError } from './client';

describe('ApiError', () => {
  it('creates an error with status, errors, and title', () => {
    const error = new ApiError(401, ['body: email or password is invalid'], 'Unauthorized');

    expect(error).toBeInstanceOf(Error);
    expect(error.name).toBe('ApiError');
    expect(error.status).toBe(401);
    expect(error.errors).toEqual(['body: email or password is invalid']);
    expect(error.title).toBe('Unauthorized');
    expect(error.message).toBe('body: email or password is invalid');
  });

  it('defaults title to Error when not provided', () => {
    const error = new ApiError(500, ['Something went wrong']);

    expect(error.title).toBe('Error');
  });

  it('joins multiple errors in the message', () => {
    const error = new ApiError(422, ['title: is required', 'body: is required']);

    expect(error.message).toBe('title: is required, body: is required');
  });
});
