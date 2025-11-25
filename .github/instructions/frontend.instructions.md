---
applyTo: "App/Client/**"
---

# Conduit Frontend Guidelines

## Project Structure

* **`src/api/`**: API client modules. Each domain (articles, comments, profiles, auth, tags) has its own file exporting an API object (e.g., `articlesApi`, `commentsApi`).
* **`src/components/`**: Reusable UI components.
* **`src/context/`**: React Context providers. Keep context type definitions in separate `*Type.ts` files.
* **`src/hooks/`**: Custom React hooks (e.g., `useAuth`).
* **`src/pages/`**: Page-level components corresponding to routes.
* **`src/types/`**: TypeScript type definitions.

## API Layer Patterns

* Export API functions as methods on a single object per domain:
  ```typescript
  export const articlesApi = {
    getArticle: async (slug: string): Promise<ArticleResponse> => { ... },
    createArticle: async (data: CreateArticleRequest): Promise<ArticleResponse> => { ... },
  };
  ```
* Use the shared `apiRequest` helper from `client.ts` for all HTTP requests.
* Handle errors using the `ApiError` class which wraps HTTP errors with structured error messages.

## Context & State Management

* **AuthContext Pattern**: 
  - Define the context type interface in a separate `AuthContextType.ts` file.
  - Export both `AuthContext` and `AuthContextType` from the type file.
  - Re-export `AuthContext` from the provider file (`AuthContext.tsx`) for convenient imports.
  - The `AuthContextType` interface must include: `user`, `loading`, `login`, `register`, `logout`, `updateUser`.

* **Using Context in Components**:
  - Use the `useAuth` hook instead of `useContext(AuthContext)` directly.
  - The hook includes a runtime check that throws if used outside the provider.

## Component Patterns

* Use named exports for components: `export const ArticlePage: React.FC = () => { ... }`.
* Use Carbon Design System components for UI elements.
* Handle loading states with Carbon's `Loading` component.
* Display errors using Carbon's `InlineNotification` component.

## Testing Patterns

### General Guidelines
* Use Vitest for testing with `@testing-library/react`.
* Follow the AAA pattern (Arrange, Act, Assert).
* Clear mocks in `beforeEach` with `vi.clearAllMocks()`.

### Mocking APIs
* Mock API modules by matching their export structure (object with methods):
  ```typescript
  vi.mock('../api/articles', () => ({
    articlesApi: {
      getArticle: vi.fn(),
      createArticle: vi.fn(),
    },
  }));
  ```
* Configure mock return values in `beforeEach`:
  ```typescript
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(articlesApi.getArticle).mockResolvedValue({ article: mockArticle });
  });
  ```

### Mocking AuthContext
* When using `AuthContext.Provider` directly in tests, provide all required properties:
  ```typescript
  <AuthContext.Provider value={{ 
    user: null, 
    loading: false,
    login: vi.fn(), 
    register: vi.fn(),
    logout: vi.fn(), 
    updateUser: vi.fn()
  }}>
  ```
* Alternatively, use `AuthProvider` with mocked `authApi` for integration-style tests.

### Component Imports
* Use named imports matching the component's export style:
  ```typescript
  import { ArticlePage } from './ArticlePage';  // ✓ Named export
  import ArticlePage from './ArticlePage';      // ✗ Default import (won't work)
  ```

## Error Handling

* Use `try/catch` blocks with `ApiError` type checking:
  ```typescript
  try {
    await articlesApi.createArticle(data);
  } catch (error) {
    if (error instanceof ApiError) {
      setError(error.errors.join(', '));
    } else {
      setError('An unexpected error occurred');
    }
  }
  ```

## Routing

* Use `react-router` for routing.
* Use `useParams` for route parameters.
* Use `useNavigate` for programmatic navigation.
* Wrap components requiring routes in `MemoryRouter` for testing.

## When Copilot Makes Changes

1. Ensure API mocks match the actual API object structure.
2. Use consistent import styles (named vs default) matching the source exports.
3. Provide complete mock values matching TypeScript interfaces.
4. Run `./build.sh TestClient` to verify tests pass.
5. Run `./build.sh LintClientVerify` to check for linting issues.

---

**Scope:** Frontend only. Do not modify backend or infra from instructions in this file.
