---
paths:
  - "App/Client/**"
---

## Project Structure

- `src/api/` — API client modules, one per domain (e.g., `articlesApi`, `commentsApi`)
- `src/components/` — Reusable UI components (Carbon Design System)
- `src/context/` — React Context providers (types in separate `*Type.ts` files)
- `src/hooks/` — Custom hooks (e.g., `useAuth`)
- `src/pages/` — Route-level page components
- `src/types/` — TypeScript type definitions

## API Layer

Export API functions as methods on a single object per domain. Use shared `apiRequest` helper from `client.ts`. Handle errors with `ApiError` class.

## State Management

- Use `useAuth` hook (not `useContext(AuthContext)` directly)
- `AuthContextType` must include: `user`, `loading`, `login`, `register`, `logout`, `updateUser`

## Components

- Named exports (`export const ComponentName: React.FC`)
- Carbon components for UI (Loading, InlineNotification, etc.)
- `react-router` for routing, `useParams`/`useNavigate` for navigation

## Testing (Vitest)

- Mock API modules matching their export structure (object with methods)
- `vi.clearAllMocks()` in `beforeEach`
- Provide complete mock values matching TypeScript interfaces
- Use `MemoryRouter` for components requiring routes
