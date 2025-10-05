const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

export class ApiError extends Error {
  status: number;
  errors: string[];
  
  constructor(status: number, errors: string[]) {
    super(errors.join(', '));
    this.name = 'ApiError';
    this.status = status;
    this.errors = errors;
  }
}

export async function apiRequest<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const token = localStorage.getItem('token');
  
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
  };

  if (options.headers) {
    Object.assign(headers, options.headers);
  }

  if (token) {
    headers['Authorization'] = `Token ${token}`;
  }

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers,
  });

  const data = await response.json();

  if (!response.ok) {
    const errors = data?.errors?.body || ['An error occurred'];
    throw new ApiError(response.status, errors);
  }

  return data;
}
