// Use relative API path so Vite proxy can handle it in development
// In production, the API is expected to be served from the same origin
const API_BASE_URL = '';

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
    // Handle ProblemDetails format: errors is an array of {name, reason}
    let errors: string[];
    if (data?.errors && Array.isArray(data.errors)) {
      // ProblemDetails format
      errors = data.errors.map((err: { name: string; reason: string }) => 
        `${err.name} ${err.reason}`
      );
    } else {
      // Fallback for other error formats
      errors = ['An error occurred'];
    }
    throw new ApiError(response.status, errors);
  }

  return data;
}
