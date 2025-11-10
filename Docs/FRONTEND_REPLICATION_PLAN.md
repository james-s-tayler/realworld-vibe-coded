# RealWorld Frontend Replication Plan with Carbon Design System

## Executive Summary

This document outlines a comprehensive plan to replicate the RealWorld demo application frontend using the Carbon Design System. The plan maps all UI components, user flows, and features from the RealWorld specification to Carbon components, providing a detailed implementation roadmap.

### Note on Demo Site Access

**Issue Encountered:** The demo sites (https://demo.realworld.show and https://angular.realworld.io) are blocked in this environment due to `ERR_BLOCKED_BY_CLIENT` errors when accessing via Playwright MCP server.

**Root Cause:** The error `ERR_BLOCKED_BY_CLIENT` indicates that the browser or environment has content blocking policies in place. This is a client-side restriction, likely due to:
- Network-level domain filtering in the GitHub Actions runner environment
- Security policies blocking certain domains
- Ad blocker or content filtering at the infrastructure level

**Alternative Research Methods Used:**
1. ✅ Web search for RealWorld specification and UI documentation
2. ✅ Review of RealWorld GitHub repository documentation
3. ✅ Analysis of existing backend API implementation in this repository
4. ✅ Study of Carbon Design System component library
5. ✅ Examination of existing partial frontend implementation

**Impact:** While we couldn't interact with the live demo via Playwright, we gathered comprehensive information through alternative methods to create a complete replication plan. The RealWorld specification is well-documented and standardized, making this feasible without live interaction.

## Current State Assessment

### Existing Implementation

The repository already has a partial frontend implementation with:

**Implemented Pages:**
- Home Page (basic welcome screen)
- Login Page (with Carbon Form, TextInput, Button components)
- Register Page
- Profile Page (protected route)

**Existing Infrastructure:**
- React 19.1.1 with React Router 7.9.3
- Carbon Design System @carbon/react 1.92.1
- Vite build system
- Vitest testing framework
- TypeScript configuration
- Authentication context and protected routes
- API client with error handling

**Backend API Status:**
The backend implements all required RealWorld API endpoints:
- Authentication: `/api/users/login`, `/api/users` (register)
- Current User: `/api/user` (GET, PUT)
- Profiles: `/api/profiles/:username` (GET), follow/unfollow
- Articles: Full CRUD + feed, list, favorite/unfavorite
- Comments: Create, read, delete
- Tags: `/api/tags`

## RealWorld UI Specification

### Required Pages/Routes

| Page | Route | Auth Required | Description |
|------|-------|---------------|-------------|
| Home | `/` | No | Article feed with global/personal tabs, popular tags |
| Sign In | `/login` | No | ✅ Implemented |
| Sign Up | `/register` | No | ✅ Implemented (needs verification) |
| Settings | `/settings` | Yes | User profile settings, logout |
| Editor (New) | `/editor` | Yes | Create new article |
| Editor (Edit) | `/editor/:slug` | Yes | Edit existing article |
| Article View | `/article/:slug` | No | Single article display with comments |
| Profile | `/profile/:username` | No | User profile with articles/favorites tabs |
| Profile (Self) | `/profile` | Yes | ✅ Implemented (needs enhancement) |

### User Flows

#### Anonymous User Flow
1. **Landing** → Home page (global feed)
2. **Browse** → View articles, filter by tags
3. **Read** → Click article → View full article + comments (read-only)
4. **Sign Up** → Register → Redirect to home (personalized feed)
5. **Sign In** → Login → Redirect to home (personalized feed)

#### Authenticated User Flow
1. **Home** → Personal feed + global feed tabs
2. **Create** → Click "New Article" → Editor → Publish → View article
3. **Interact** → Favorite articles, follow users, comment
4. **Profile** → View own/others' profiles, articles, favorites
5. **Edit** → Edit own articles, update settings
6. **Delete** → Delete own articles/comments

## Component Mapping: RealWorld → Carbon Design System

### Layout Components

| RealWorld Component | Carbon Component | Status | Priority |
|---------------------|------------------|--------|----------|
| App Header/Navbar | `HeaderContainer`, `Header`, `HeaderName`, `HeaderNavigation`, `HeaderMenuItem` | Not Implemented | High |
| Footer | Custom div with Carbon `Link` | Not Implemented | Low |
| Page Container | `Grid`, `Column` layout | Partial | High |
| Sidebar (Tags) | `Tile` with custom styling | Not Implemented | Medium |

### Form Components

| RealWorld Component | Carbon Component | Status | Priority |
|---------------------|------------------|--------|----------|
| Login Form | ✅ `Form`, `TextInput`, `Button` | Implemented | - |
| Register Form | `Form`, `TextInput`, `Button` | Needs Verification | High |
| Settings Form | `Form`, `TextInput`, `TextArea`, `Button` | Not Implemented | High |
| Article Editor | `Form`, `TextInput`, `TextArea`, `TagInput` | Not Implemented | High |
| Comment Form | `Form`, `TextArea`, `Button` | Not Implemented | Medium |

### Display Components

| RealWorld Component | Carbon Component | Status | Priority |
|---------------------|------------------|--------|----------|
| Article Preview Card | `Tile`, `Tag`, `Button` (favorite) | Not Implemented | High |
| Article Full View | Custom layout with `Tile` | Not Implemented | High |
| Comment Display | `Tile` with user info | Not Implemented | Medium |
| User Profile Card | `Tile` with avatar, bio | Not Implemented | Medium |
| Tag List/Pills | `Tag` | Not Implemented | Medium |
| Pagination | `Pagination` | Not Implemented | Medium |
| Loading State | `InlineLoading`, `Loading` | Partial | High |
| Error Notification | ✅ `InlineNotification`, `ToastNotification` | Implemented | - |

### Navigation Components

| RealWorld Component | Carbon Component | Status | Priority |
|---------------------|------------------|--------|----------|
| Tab Navigation (Feeds) | `Tabs`, `TabList`, `Tab`, `TabPanels`, `TabPanel` | Not Implemented | High |
| Profile Tabs | `Tabs`, `TabList`, `Tab`, `TabPanels`, `TabPanel` | Not Implemented | Medium |
| Breadcrumbs | Not needed | - | - |

### Interactive Components

| RealWorld Component | Carbon Component | Status | Priority |
|---------------------|------------------|--------|----------|
| Favorite Button | `Button` with heart icon | Not Implemented | Medium |
| Follow Button | `Button` | Not Implemented | Medium |
| Delete Button | `Button` kind="danger" with modal | Not Implemented | Medium |
| Edit Link | `Link` or `Button` | Not Implemented | Low |

## Feature Parity Matrix

### Authentication & User Management

| Feature | Backend API | Frontend UI | Status | Priority |
|---------|-------------|-------------|--------|----------|
| User Registration | ✅ `/api/users` | Partial | Needs Testing | High |
| User Login | ✅ `/api/users/login` | ✅ Implemented | Complete | - |
| JWT Token Storage | ✅ | ✅ localStorage | Complete | - |
| Get Current User | ✅ `/api/user` | ✅ AuthContext | Complete | - |
| Update User Settings | ✅ `/api/user` PUT | Not Implemented | Missing | High |
| Logout | N/A | Partial | Needs Enhancement | High |

### Articles

| Feature | Backend API | Frontend UI | Status | Priority |
|---------|-------------|-------------|--------|----------|
| List Global Articles | ✅ `/api/articles` | Not Implemented | Missing | High |
| List Personal Feed | ✅ `/api/articles/feed` | Not Implemented | Missing | High |
| Filter by Tag | ✅ `/api/articles?tag=` | Not Implemented | Missing | High |
| Filter by Author | ✅ `/api/articles?author=` | Not Implemented | Missing | Medium |
| Filter by Favorited | ✅ `/api/articles?favorited=` | Not Implemented | Missing | Medium |
| Pagination | ✅ `/api/articles?limit=&offset=` | Not Implemented | Missing | High |
| View Single Article | ✅ `/api/articles/:slug` | Not Implemented | Missing | High |
| Create Article | ✅ `/api/articles` POST | Not Implemented | Missing | High |
| Update Article | ✅ `/api/articles/:slug` PUT | Not Implemented | Missing | High |
| Delete Article | ✅ `/api/articles/:slug` DELETE | Not Implemented | Missing | Medium |
| Favorite Article | ✅ `/api/articles/:slug/favorite` POST | Not Implemented | Missing | Medium |
| Unfavorite Article | ✅ `/api/articles/:slug/favorite` DELETE | Not Implemented | Missing | Medium |

### Comments

| Feature | Backend API | Frontend UI | Status | Priority |
|---------|-------------|-------------|--------|----------|
| List Comments | ✅ `/api/articles/:slug/comments` | Not Implemented | Missing | Medium |
| Add Comment | ✅ `/api/articles/:slug/comments` POST | Not Implemented | Missing | Medium |
| Delete Comment | ✅ `/api/articles/:slug/comments/:id` DELETE | Not Implemented | Missing | Low |

### Profiles

| Feature | Backend API | Frontend UI | Status | Priority |
|---------|-------------|-------------|--------|----------|
| View Profile | ✅ `/api/profiles/:username` | Partial | Needs Enhancement | High |
| Follow User | ✅ `/api/profiles/:username/follow` POST | Not Implemented | Missing | Medium |
| Unfollow User | ✅ `/api/profiles/:username/follow` DELETE | Not Implemented | Missing | Medium |

### Tags

| Feature | Backend API | Frontend UI | Status | Priority |
|---------|-------------|-------------|--------|----------|
| List All Tags | ✅ `/api/tags` | Not Implemented | Missing | High |
| Filter Articles by Tag | ✅ `/api/articles?tag=` | Not Implemented | Missing | High |

## Implementation Roadmap

### Phase 1: Core Infrastructure (High Priority)

**1.1 Header/Navigation Component**
- Create `AppHeader` component using Carbon's `HeaderContainer`
- Implement responsive navigation (logged in vs. logged out states)
- Add navigation links: Home, Sign in/up (or New Article, Settings, Profile)
- **Estimated Effort:** 4-6 hours
- **Dependencies:** None
- **Testing:** Navigation state changes, route linking

**1.2 Main Layout Component**
- Create `MainLayout` wrapper with Carbon Grid
- Implement responsive columns (sidebar + main content)
- Add footer component
- **Estimated Effort:** 2-3 hours
- **Dependencies:** 1.1
- **Testing:** Responsive layout, grid behavior

**1.3 API Client Enhancement**
- Add API methods for articles, comments, profiles, tags
- Implement request/response types
- Add error handling for all endpoints
- **Estimated Effort:** 6-8 hours
- **Dependencies:** None
- **Testing:** Unit tests for each API method

### Phase 2: Article Features (High Priority)

**2.1 Article Preview Component**
- Create `ArticlePreview` card component
- Display: title, description, author info, date, tags, favorite count
- Implement favorite button (with authentication check)
- **Estimated Effort:** 4-6 hours
- **Dependencies:** 1.3
- **Testing:** Display logic, favorite interaction

**2.2 Home Page - Article Feed**
- Implement tabbed interface (Global Feed / Your Feed)
- Integrate article list API
- Add pagination component
- Display article previews in feed
- **Estimated Effort:** 8-10 hours
- **Dependencies:** 2.1, 1.3
- **Testing:** Tab switching, pagination, feed loading

**2.3 Tag Sidebar Component**
- Create `PopularTags` component
- Fetch and display tags from API
- Implement tag click to filter articles
- **Estimated Effort:** 3-4 hours
- **Dependencies:** 1.3
- **Testing:** Tag loading, filter interaction

**2.4 Article Detail Page**
- Create `ArticlePage` component
- Display full article content with metadata
- Show author info and actions (edit/delete for owner)
- Add favorite and follow buttons
- **Estimated Effort:** 6-8 hours
- **Dependencies:** 1.3
- **Testing:** Article loading, owner actions, interactions

**2.5 Article Editor Page**
- Create `EditorPage` component
- Implement form: title, description, body, tags
- Handle create and edit modes (based on route)
- Add tag input functionality
- **Estimated Effort:** 8-10 hours
- **Dependencies:** 1.3
- **Testing:** Form validation, create/edit modes, tag management

### Phase 3: Comments & Social Features (Medium Priority)

**3.1 Comments Section**
- Create `CommentSection` component
- Display list of comments
- Implement add comment form
- Add delete button for own comments
- **Estimated Effort:** 6-8 hours
- **Dependencies:** 2.4, 1.3
- **Testing:** Comment display, add/delete operations

**3.2 Profile Page Enhancement**
- Enhance existing `ProfilePage`
- Add tabs: My Articles / Favorited Articles
- Display user bio, image, follow button
- List user's articles or favorites based on tab
- **Estimated Effort:** 8-10 hours
- **Dependencies:** 2.1, 1.3
- **Testing:** Profile loading, tab switching, article lists

**3.3 Settings Page**
- Create `SettingsPage` component
- Form fields: image, username, bio, email, password
- Implement update user API call
- Add logout functionality
- **Estimated Effort:** 4-6 hours
- **Dependencies:** 1.3
- **Testing:** Form validation, update operations, logout

### Phase 4: Polish & Optimization (Low Priority)

**4.1 Loading States**
- Implement loading skeletons for all data-fetching components
- Use Carbon's `SkeletonText`, `SkeletonPlaceholder`
- Add loading indicators to buttons
- **Estimated Effort:** 4-6 hours
- **Dependencies:** All previous phases
- **Testing:** Loading state display

**4.2 Error Handling**
- Standardize error display across all pages
- Implement toast notifications for background actions
- Add error boundaries for component failures
- **Estimated Effort:** 4-6 hours
- **Dependencies:** All previous phases
- **Testing:** Error scenarios, boundary behavior

**4.3 Responsive Design**
- Test and refine mobile/tablet layouts
- Adjust Carbon Grid breakpoints
- Optimize touch targets
- **Estimated Effort:** 6-8 hours
- **Dependencies:** All previous phases
- **Testing:** Multiple device sizes

**4.4 Accessibility**
- Audit with Carbon's a11y tools
- Add ARIA labels where needed
- Ensure keyboard navigation
- Test with screen readers
- **Estimated Effort:** 4-6 hours
- **Dependencies:** All previous phases
- **Testing:** a11y audits, keyboard navigation

## Technical Considerations

### State Management
- **Current:** React Context API for authentication
- **Recommendation:** Continue with Context for now; consider React Query or Zustand if state becomes complex
- **Article State:** Each component fetches its own data; consider caching strategy

### Routing Strategy
- **Current:** React Router 7.9.3
- **URL Structure:** Follow RealWorld spec exactly for compatibility
- **Protected Routes:** Enhance existing `ProtectedRoute` component

### API Integration
- **Base URL:** Configure in environment variables
- **Authentication:** JWT token in `Authorization: Token {token}` header
- **Error Handling:** Standardized `ApiError` class (already exists)
- **Request/Response Types:** Define TypeScript interfaces for all API calls

### Testing Strategy
- **Unit Tests:** All components and utilities (Vitest)
- **Integration Tests:** User flows (Testing Library)
- **E2E Tests:** Consider Playwright for critical paths
- **Coverage Goal:** >80% for new code

### Performance Optimization
- **Code Splitting:** Lazy load routes
- **Image Optimization:** Use appropriate formats, lazy loading
- **Bundle Size:** Monitor with Vite's bundle analyzer
- **Pagination:** Implement proper pagination to limit data fetching

## Carbon Design System Guidelines

### Theme & Styling
- Use Carbon's default white theme (or gray theme as preference)
- Follow Carbon's spacing scale (4px base)
- Use Carbon tokens for colors, typography
- Leverage Carbon's responsive grid system

### Component Best Practices
- Import components from `@carbon/react`
- Use TypeScript props for type safety
- Follow Carbon's component composition patterns
- Utilize Carbon icons from `@carbon/icons-react`

### Common Carbon Components for RealWorld

| Use Case | Carbon Component |
|----------|------------------|
| Navigation | `Header`, `HeaderContainer`, `HeaderNavigation` |
| Forms | `Form`, `TextInput`, `TextArea`, `Button` |
| Cards | `Tile`, `ClickableTile` |
| Lists | Custom with `Tile` wrapper |
| Notifications | `InlineNotification`, `ToastNotification` |
| Tabs | `Tabs`, `TabList`, `Tab`, `TabPanels` |
| Loading | `InlineLoading`, `Loading`, `SkeletonText` |
| Icons | `@carbon/icons-react` (e.g., `FavoriteFilled`) |
| Pagination | `Pagination` |
| Modal | `Modal`, `ComposedModal` |

## File Structure Recommendations

```
App/Client/src/
├── api/
│   ├── auth.ts (✅ exists)
│   ├── articles.ts (new)
│   ├── comments.ts (new)
│   ├── profiles.ts (new)
│   ├── tags.ts (new)
│   └── client.ts (✅ exists)
├── components/
│   ├── layout/
│   │   ├── AppHeader.tsx (new)
│   │   ├── MainLayout.tsx (new)
│   │   └── Footer.tsx (new)
│   ├── articles/
│   │   ├── ArticlePreview.tsx (new)
│   │   ├── ArticleList.tsx (new)
│   │   ├── ArticleMeta.tsx (new)
│   │   └── TagList.tsx (new)
│   ├── comments/
│   │   ├── CommentCard.tsx (new)
│   │   ├── CommentForm.tsx (new)
│   │   └── CommentList.tsx (new)
│   ├── profile/
│   │   ├── ProfileCard.tsx (new)
│   │   └── FollowButton.tsx (new)
│   └── ProtectedRoute.tsx (✅ exists)
├── pages/
│   ├── HomePage.tsx (✅ exists - needs enhancement)
│   ├── LoginPage.tsx (✅ exists)
│   ├── RegisterPage.tsx (✅ exists)
│   ├── ProfilePage.tsx (✅ exists - needs enhancement)
│   ├── SettingsPage.tsx (new)
│   ├── EditorPage.tsx (new)
│   └── ArticlePage.tsx (new)
├── hooks/
│   ├── useAuth.ts (✅ exists)
│   ├── useArticles.ts (new)
│   ├── useComments.ts (new)
│   └── useProfile.ts (new)
├── types/
│   ├── user.ts (✅ exists)
│   ├── article.ts (new)
│   ├── comment.ts (new)
│   └── profile.ts (new)
└── context/
    └── AuthContext.tsx (✅ exists)
```

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Carbon component limitations | Medium | Low | Use custom CSS when needed, ensure Carbon props support use cases |
| API response format mismatch | Low | Medium | Validate against RealWorld spec, add integration tests |
| State management complexity | Medium | Medium | Consider React Query for server state if Context becomes unwieldy |
| Performance with large article lists | Medium | Medium | Implement pagination, virtual scrolling if needed |
| Mobile responsive issues | Low | Medium | Test on multiple devices throughout development |
| Accessibility gaps | Medium | Low | Regular a11y audits, follow Carbon guidelines |

## Success Criteria

The frontend replication is considered complete when:

1. ✅ All pages from RealWorld spec are implemented
2. ✅ All user flows function correctly
3. ✅ All API endpoints are integrated
4. ✅ Responsive design works on mobile/tablet/desktop
5. ✅ Accessibility audit passes
6. ✅ Test coverage >80%
7. ✅ All Carbon components are properly used
8. ✅ Authentication and authorization work correctly
9. ✅ Error handling is consistent and user-friendly
10. ✅ UI matches Carbon Design System guidelines

## Estimated Timeline

| Phase | Estimated Effort | Priority |
|-------|-----------------|----------|
| Phase 1: Core Infrastructure | 12-17 hours | High |
| Phase 2: Article Features | 29-38 hours | High |
| Phase 3: Comments & Social | 18-24 hours | Medium |
| Phase 4: Polish & Optimization | 18-26 hours | Low |
| **Total** | **77-105 hours** | |

*Note: This is a rough estimate for a single developer. Actual time may vary based on experience with React, Carbon, and the RealWorld spec.*

## Next Steps

1. **Create GitHub Issues:** Break down each section of Phase 1-4 into individual issues
2. **Setup Project Board:** Organize issues into columns (To Do, In Progress, Review, Done)
3. **Prioritization Meeting:** Review with stakeholders to confirm priorities
4. **Sprint Planning:** Group issues into 1-2 week sprints
5. **Begin Phase 1:** Start with Header/Navigation component

## References

- [RealWorld Spec](https://github.com/gothinkster/realworld)
- [RealWorld API Spec](https://docs.realworld.show/specifications/backend/endpoints/)
- [Carbon Design System](https://carbondesignsystem.com/)
- [Carbon React Components](https://react.carbondesignsystem.com/)
- [Carbon Icons](https://carbondesignsystem.com/guidelines/icons/library/)

---

**Document Version:** 1.0  
**Last Updated:** 2025-11-10  
**Author:** GitHub Copilot Agent  
**Status:** Planning Phase
