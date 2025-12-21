// Use empty string to make API calls relative to the current origin
// This works in all scenarios: local dev, docker, and E2E tests
const API_BASE_URL = import.meta.env.VITE_API_URL || '';

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

export async function apiRequest<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const token = localStorage.getItem('token');
  
  const headers: Record<string, string> = {};

  // Only add Content-Type header for requests with a body
  if (options.body && options.method !== 'GET' && options.method !== 'HEAD') {
    headers['Content-Type'] = 'application/json';
  }

  if (options.headers) {
    Object.assign(headers, options.headers);
  }

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers,
  });

  // Check if response has content before trying to parse JSON
  const contentType = response.headers.get('content-type');
  const hasJsonContent = contentType && (contentType.includes('application/json') || contentType.includes('application/problem+json'));
  
  // Handle empty responses (204 No Content or empty body)
  const text = await response.text();
  let data: unknown = null;
  
  if (text && hasJsonContent) {
    try {
      data = JSON.parse(text);
    } catch (error) {
      console.error('Failed to parse JSON response:', text);
      if (!response.ok) {
        throw new ApiError(response.status, ['Failed to parse response']);
      }
      throw error;
    }
  }

  if (!response.ok) {
    // Handle ProblemDetails format
    let errors: string[];
    let title: string | undefined;
    
    const errorData = data as Record<string, unknown>;
    
    // Extract title from ProblemDetails if available
    if (errorData?.title) {
      title = String(errorData.title);
    }
    
    if (errorData?.errors) {
      // ProblemDetails format: errors can be an array or object
      if (Array.isArray(errorData.errors)) {
        errors = errorData.errors.map((err: { name: string; reason: string }) => 
          `${err.name}: ${err.reason}`
        );
      } else if (typeof errorData.errors === 'object' && errorData.errors !== null) {
        // Check if it's ValidationProblemDetails format (dictionary of field -> string[])
        const errorValues = Object.values(errorData.errors);
        if (errorValues.length > 0) {
          const firstValue = errorValues[0];
          if (Array.isArray(firstValue)) {
            // ValidationProblemDetails format: { "Email": ["error1", "error2"] }
            errors = errorValues.flatMap((fieldErrors) => fieldErrors as string[]);
          } else if (typeof firstValue === 'object' && firstValue !== null && 'name' in firstValue && 'reason' in firstValue) {
            // Custom format: {0: {name, reason}, 1: {name, reason}}
            errors = errorValues.map((err: unknown) => {
              const error = err as { name: string; reason: string };
              return `${error.name}: ${error.reason}`;
            });
          } else {
            // Fallback: convert to strings
            errors = errorValues.map(val => String(val));
          }
        } else {
          errors = [String(errorData.errors)];
        }
      } else {
        errors = [String(errorData.errors)];
      }
    } else if (title) {
      // ProblemDetails with title but no errors array
      errors = [title];
      if (errorData.detail) {
        errors.push(String(errorData.detail));
      }
    } else if (errorData?.message) {
      errors = [String(errorData.message)];
    } else {
      errors = [`Request failed with status ${response.status}`];
    }
    
    throw new ApiError(response.status, errors, title);
  }

  return data as T;
}
