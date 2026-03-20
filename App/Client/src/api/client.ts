export class ApiError extends Error {
  status: number;
  errors: string[];
  title: string;

  constructor(status: number, errors: string[], title?: string) {
    super(errors.join(', '));
    this.name = 'ApiError';
    this.status = status;
    this.errors = errors;
    this.title = title ?? 'Error';
  }
}
