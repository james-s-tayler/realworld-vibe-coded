import { ApiError } from './client';
import type { ProblemDetails } from './generated/models/index.js';

type KiotaProblemDetails = {
  responseStatusCode?: number;
  message?: string;
} & Partial<ProblemDetails>;

function isKiotaError(error: unknown): error is KiotaProblemDetails {
  return typeof error === 'object' && error !== null && 'responseStatusCode' in error;
}

export function convertKiotaError(error: unknown): never {
  if (isKiotaError(error)) {
    const status = error.status ?? error.responseStatusCode ?? 500;
    const title = error.title ?? 'Error';

    let errors: string[];
    if (error.errors && error.errors.length > 0) {
      errors = error.errors.map(
        (e) => `${e.name}: ${e.reason}`,
      );
    } else if (error.detail) {
      errors = [error.detail];
    } else {
      errors = [title];
    }

    throw new ApiError(status, errors, title);
  }

  if (error instanceof Error) {
    throw new ApiError(500, [error.message]);
  }

  throw new ApiError(500, ['An unexpected error occurred']);
}
