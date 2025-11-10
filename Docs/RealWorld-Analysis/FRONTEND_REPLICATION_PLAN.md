# RealWorld Frontend Replication Plan with Carbon Design System

## Executive Summary

This document provides a comprehensive plan for replicating the RealWorld demo frontend (https://demo.realworld.show) using the Carbon Design System in the james-s-tayler/realworld-vibe-coded repository. The analysis includes:

- Complete page inventory and user flows
- UI component catalog and Carbon Design System mappings
- Feature parity matrix
- Backend API endpoint mappings
- Implementation roadmap with prioritized tasks

## Table of Contents

1. [Pages Overview](#pages-overview)
2. [User Flows](#user-flows)
3. [UI Components Catalog](#ui-components-catalog)
4. [Carbon Design System Component Mapping](#carbon-design-system-component-mapping)
5. [Feature Parity Matrix](#feature-parity-matrix)
6. [Backend API Endpoints](#backend-api-endpoints)
7. [Implementation Roadmap](#implementation-roadmap)
8. [Technical Considerations](#technical-considerations)

---

## Pages Overview

Based on the RealWorld specification and demo site exploration, the application consists of the following pages:

### 1. **Home Page** (`/` or `#/`)
- **Purpose**: Landing page displaying articles and tags
- **States**: 
  - Anonymous: Shows "Your Feed" and "Global Feed" tabs (Global Feed active)
  - Authenticated: Shows "Your Feed" (personalized) and "Global Feed" tabs
- **Key Features**:
  - Banner with tagline
  - Article feed with pagination
  - Popular tags sidebar
  - Filter by tag functionality
  - Feed toggle (Your Feed vs Global Feed)

### 2. **Sign In Page** (`#/login`)
- **Purpose**: User authentication
- **Features**:
  - Email input field
  - Password input field
  - Sign in button
  - Link to sign up page ("Need an account?")
  - Error display for invalid credentials

### 3. **Sign Up Page** (`#/register`)
- **Purpose**: New user registration
- **Features**:
  - Username input field
  - Email input field
  - Password input field
  - Sign up button
  - Link to sign in page ("Have an account?")
  - Error display for validation issues

### 4. **Settings Page** (`#/settings`)
- **Purpose**: User profile and account management
- **Features**:
  - Profile image URL input
  - Username input
  - Bio textarea
  - Email input
  - Password input (change password)
  - Update settings button
  - Logout button

### 5. **Article Page** (`#/article/:slug`)
- **Purpose**: Display full article content
- **Features**:
  - Article title, author, date
  - Article body (markdown rendered)
  - Author profile card
  - Favorite button with count
  - Follow author button
  - Edit/Delete buttons (for article author)
  - Comments section
  - Add comment form (authenticated users)
  - Delete comment button (comment author)

### 6. **Editor Page** (`#/editor` and `#/editor/:slug`)
- **Purpose**: Create or edit articles
- **Features**:
  - Article title input
  - Description input
  - Body textarea (markdown)
  - Tags input (comma-separated or tag pills)
  - Publish article button
  - Preview (optional)

### 7. **Profile Page** (`#/profile/:username`)
- **Purpose**: Display user profile and their articles
- **Features**:
  - User avatar
  - Username
  - Bio
  - Follow/Unfollow button
  - Edit profile button (own profile only)
  - Tabs:
    - "My Articles" - articles authored by user
    - "Favorited Articles" - articles favorited by user
  - Article list with pagination

---

## User Flows

### Flow 1: Anonymous User Browsing
```
1. Land on Home Page
2. View Global Feed articles
3. Click on tag to filter articles
4. Click on article to read full content
5. Prompted to sign in to interact (favorite, comment, follow)
```

### Flow 2: User Registration
```
1. Click "Sign up" in navigation
2. Fill in username, email, password
3. Submit form
4. Automatically logged in
5. Redirected to Home Page (authenticated)
```

### Flow 3: User Login
```
1. Click "Sign in" in navigation
2. Fill in email and password
3. Submit form
4. Redirected to Home Page (authenticated)
```

### Flow 4: Authenticated Article Browsing
```
1. View "Your Feed" tab (followed users' articles)
2. Switch to "Global Feed"
3. Click article to read
4. Favorite/unfavorite article
5. Follow/unfollow author
6. Add comment
7. Delete own comment
```

### Flow 5: Article Creation
```
1. Click "New Article" in navigation (authenticated)
2. Fill in title, description, body, tags
3. Publish article
4. Redirected to article page
```

### Flow 6: Article Editing
```
1. View own article
2. Click "Edit Article" button
3. Modify fields
4. Update article
5. Redirected to article page
```

### Flow 7: Article Deletion
```
1. View own article
2. Click "Delete Article" button
3. Confirm deletion (optional)
4. Redirected to Home Page
```

### Flow 8: Profile Management
```
1. Click "Settings" in navigation
2. Update profile information
3. Submit changes
4. Profile updated
```

### Flow 9: Viewing User Profile
```
1. Click on author name or avatar
2. View profile page
3. See user's articles
4. Follow/unfollow user
5. Switch to "Favorited Articles" tab
```

---

## UI Components Catalog

### Navigation Components
1. **Header/Navigation Bar**
   - Logo/Brand link
   - Navigation links (Home, Sign in, Sign up) - anonymous
   - Navigation links (Home, New Article, Settings, Profile) - authenticated
   - Active link highlighting

2. **Footer**
   - Logo/Brand link
   - Copyright text
   - External link to RealWorld project

### Layout Components
3. **Page Container**
   - Main content wrapper
   - Responsive grid layout

4. **Hero Banner**
   - Large heading
   - Tagline/subtitle

### Article Components
5. **Article Preview Card**
   - Author avatar
   - Author name (link)
   - Article date
   - Favorite button with count
   - Article title (link)
   - Article description
   - Tags list
   - "Read more..." link

6. **Article Full View**
   - Article metadata bar (author, date, actions)
   - Article title
   - Article body (markdown rendered)
   - Tags list

7. **Article Metadata**
   - Author info (avatar, name, date)
   - Action buttons (Favorite, Follow, Edit, Delete)

### Form Components
8. **Text Input**
   - Single-line text input
   - Placeholder text
   - Validation states
   - Error messages

9. **Textarea**
   - Multi-line text input
   - Auto-resize (optional)

10. **Password Input**
    - Masked input
    - Show/hide toggle (optional)

11. **Button**
    - Primary button
    - Secondary button
    - Outline button
    - Disabled state
    - Loading state (optional)

12. **Form Container**
    - Form wrapper
    - Error list display

### List Components
13. **Tag List**
    - Tag pills/chips
    - Clickable tags
    - Loading state

14. **Article List**
    - List of article preview cards
    - Loading state
    - Empty state

15. **Comment List**
    - Individual comment cards
    - Author info
    - Comment body
    - Delete button (for comment author)

### Interactive Components
16. **Tabs**
    - Tab navigation
    - Active tab indicator
    - Tab panels

17. **Pagination**
    - Page numbers
    - Previous/Next buttons
    - Current page indicator
    - Disabled states

18. **Follow Button**
    - Follow/Unfollow states
    - Icon + text
    - Loading state

19. **Favorite Button**
    - Favorite/Unfavorite states
    - Heart icon
    - Count display
    - Loading state

### User Components
20. **User Profile Card**
    - Avatar
    - Username
    - Bio
    - Follow button
    - Edit profile button (conditional)

21. **Comment Card**
    - Author avatar (small)
    - Author name
    - Comment date
    - Comment body
    - Delete button (conditional)

22. **Comment Form**
    - Avatar
    - Textarea
    - Submit button

### Feedback Components
23. **Loading Indicator**
    - Spinner
    - "Loading..." text

24. **Error Display**
    - Error message list
    - Individual error messages

25. **Empty State**
    - "No articles..." message
    - Contextual messaging

---

## Carbon Design System Component Mapping

This section maps each RealWorld UI component to its Carbon Design System equivalent.

| RealWorld Component | Carbon Component | Notes |
|---------------------|------------------|-------|
| **Header/Navigation Bar** | `Header`, `HeaderName`, `HeaderNavigation`, `HeaderMenuItem` | Use Carbon's header components for top navigation |
| **Footer** | Custom div with `Link` | Carbon doesn't have a specific footer component; use custom layout |
| **Page Container** | `Grid`, `Column` | Use Carbon Grid for responsive layouts |
| **Hero Banner** | Custom div with Carbon typography classes | Create custom banner with `h1` and `p` using Carbon type styles |
| **Article Preview Card** | `Tile`, `ClickableTile` | Use Tile component as base for article cards |
| **Article Full View** | Custom layout with `Tile` | Combine multiple Carbon components |
| **Article Metadata** | Custom component with `Button`, `Tag` | Build custom metadata bar |
| **Text Input** | `TextInput` | Direct mapping to Carbon TextInput |
| **Textarea** | `TextArea` | Direct mapping to Carbon TextArea |
| **Password Input** | `PasswordInput` | Carbon has dedicated password input with toggle |
| **Button** | `Button` | Use Carbon Button with various kinds (primary, secondary, ghost) |
| **Form Container** | `Form`, `FormGroup`, `Stack` | Use Carbon Form components |
| **Tag List** | `Tag` | Use Carbon Tag component for displaying tags |
| **Article List** | Custom list with `Tile` components | Map articles to ClickableTile components |
| **Comment List** | `Accordion` or custom list with `Tile` | Comments can use tiles or accordion |
| **Tabs** | `Tabs`, `TabList`, `Tab`, `TabPanels`, `TabPanel` | Direct mapping to Carbon Tabs |
| **Pagination** | `Pagination` | Direct mapping to Carbon Pagination |
| **Follow Button** | `Button` with custom logic | Use Button with icon and toggle logic |
| **Favorite Button** | `Button` with custom logic | Use Button with heart icon and counter |
| **User Profile Card** | Custom component with `Tile`, `Button` | Combine Carbon components |
| **Comment Card** | `Tile` or custom card | Use Tile as base |
| **Comment Form** | `TextArea`, `Button` | Combine form components |
| **Loading Indicator** | `Loading` or `InlineLoading` | Use Carbon Loading component |
| **Error Display** | `InlineNotification` or `ToastNotification` | Use notification components |
| **Empty State** | Custom with typography | Build custom empty state with Carbon typography |
| **Avatar Image** | Custom `img` with Carbon styling | Carbon doesn't have avatar component; use styled img |
| **Link** | `Link` | Direct mapping to Carbon Link |

### Additional Carbon Components to Leverage

- **Modal**: For confirmations (delete article, etc.)
- **Breadcrumb**: Optional, for navigation hierarchy
- **SkeletonText** / **SkeletonPlaceholder**: For loading states
- **ContentSwitcher**: Alternative to tabs in some cases
- **IconButton**: For compact actions
- **OverflowMenu**: For additional actions dropdown
- **Search**: For article search functionality (future feature)
- **ComboBox**: For tag input with suggestions

---

## Feature Parity Matrix

| Feature | RealWorld Demo | Current Implementation | Carbon Components Needed | Priority |
|---------|----------------|------------------------|--------------------------|----------|
| **Authentication** | ✅ | ✅ Partial (Login, Register, Profile pages exist) | TextInput, PasswordInput, Button, Form | High |
| **Home Page** | ✅ | ✅ Partial (HomePage exists) | Tabs, Tile, Pagination, Tag, Loading | High |
| **Article List** | ✅ | ⚠️ Needs styling | Tile/ClickableTile, Tag, Button, Pagination | High |
| **Article View** | ✅ | ❌ | Tile, Button, Tag, TextArea | High |
| **Article Editor** | ✅ | ❌ | TextInput, TextArea, Tag input, Button, Form | Medium |
| **Comments** | ✅ | ❌ | Tile, TextArea, Button | Medium |
| **Tags/Topics** | ✅ | ⚠️ Needs styling | Tag, ClickableTile | Medium |
| **Follow User** | ✅ | ❌ | Button with custom logic | Low |
| **Favorite Article** | ✅ | ❌ | Button with custom logic | Low |
| **User Profile** | ✅ | ✅ Partial (ProfilePage exists) | Tile, Button, Tabs, Pagination | Medium |
| **Settings** | ✅ | ❌ | Form, TextInput, TextArea, Button, PasswordInput | Medium |
| **Pagination** | ✅ | ❌ | Pagination | High |
| **Responsive Layout** | ✅ | ⚠️ Partial | Grid, Column | High |
| **Error Handling** | ✅ | ⚠️ Partial | InlineNotification | Medium |
| **Loading States** | ✅ | ⚠️ Partial | Loading, InlineLoading, SkeletonText | Medium |

**Legend:**
- ✅ Complete
- ⚠️ Partial / Needs improvement
- ❌ Not implemented

---

## Backend API Endpoints

Based on RealWorld specification and network analysis:

### Authentication Endpoints
| Endpoint | Method | Purpose | Used By |
|----------|--------|---------|---------|
| `/api/users/login` | POST | User login | Sign In Page |
| `/api/users` | POST | User registration | Sign Up Page |
| `/api/user` | GET | Get current user | App initialization, auth check |
| `/api/user` | PUT | Update user | Settings Page |

### Articles Endpoints
| Endpoint | Method | Purpose | Used By |
|----------|--------|---------|---------|
| `/api/articles` | GET | List articles (global feed) | Home Page (Global Feed) |
| `/api/articles?tag=:tag` | GET | Filter articles by tag | Home Page (tag filter) |
| `/api/articles?author=:author` | GET | Filter articles by author | Profile Page (My Articles) |
| `/api/articles?favorited=:username` | GET | Get favorited articles | Profile Page (Favorited) |
| `/api/articles/feed` | GET | Get personalized feed | Home Page (Your Feed) |
| `/api/articles/:slug` | GET | Get single article | Article Page |
| `/api/articles` | POST | Create article | Editor Page (new) |
| `/api/articles/:slug` | PUT | Update article | Editor Page (edit) |
| `/api/articles/:slug` | DELETE | Delete article | Article Page (delete action) |
| `/api/articles/:slug/favorite` | POST | Favorite article | Article interactions |
| `/api/articles/:slug/favorite` | DELETE | Unfavorite article | Article interactions |

### Comments Endpoints
| Endpoint | Method | Purpose | Used By |
|----------|--------|---------|---------|
| `/api/articles/:slug/comments` | GET | Get article comments | Article Page |
| `/api/articles/:slug/comments` | POST | Add comment | Article Page (comment form) |
| `/api/articles/:slug/comments/:id` | DELETE | Delete comment | Article Page (delete comment) |

### Profiles Endpoints
| Endpoint | Method | Purpose | Used By |
|----------|--------|---------|---------|
| `/api/profiles/:username` | GET | Get user profile | Profile Page |
| `/api/profiles/:username/follow` | POST | Follow user | Profile interactions |
| `/api/profiles/:username/follow` | DELETE | Unfollow user | Profile interactions |

### Tags Endpoint
| Endpoint | Method | Purpose | Used By |
|----------|--------|---------|---------|
| `/api/tags` | GET | Get popular tags | Home Page (tags sidebar) |

**Pagination Parameters** (for list endpoints):
- `limit`: Number of items per page (default: 10)
- `offset`: Starting position (default: 0)

**Authentication**:
- JWT token passed in `Authorization` header: `Token {jwt}`
- Required for: create/update/delete operations, personalized feed, follow/unfollow

---

## Implementation Roadmap

### Phase 1: Foundation & Infrastructure (Week 1-2)
**Goal**: Set up Carbon Design System and core layout

#### Tasks:
- [ ] Install and configure Carbon Design System React components
  - `npm install @carbon/react` (already done based on package.json)
  - Configure Carbon themes
  - Set up global styles and Carbon type system
- [ ] Create layout components
  - [ ] Header component with Carbon Header components
  - [ ] Footer component
  - [ ] Page container with Carbon Grid
  - [ ] Responsive layout utilities
- [ ] Set up routing structure (React Router)
  - [ ] Define all routes
  - [ ] Protected route wrapper
  - [ ] Navigation guards
- [ ] Create reusable base components
  - [ ] LoadingSpinner component (using Carbon Loading)
  - [ ] ErrorDisplay component (using InlineNotification)
  - [ ] EmptyState component

**Acceptance Criteria**:
- Carbon Design System is properly integrated
- Navigation works across all pages
- Layout is responsive
- Loading and error states display correctly

---

### Phase 2: Authentication & Home Page (Week 3-4)
**Goal**: Complete authentication flow and home page with Carbon components

#### Tasks:
- [ ] Refactor Login page with Carbon components
  - [ ] Replace inputs with Carbon TextInput and PasswordInput
  - [ ] Style form with Carbon Form components
  - [ ] Add Carbon Button
  - [ ] Implement error display with InlineNotification
- [ ] Refactor Register page with Carbon components
  - [ ] Update all form fields to Carbon components
  - [ ] Add validation and error handling
- [ ] Refactor Home page with Carbon components
  - [ ] Implement feed tabs using Carbon Tabs
  - [ ] Create ArticlePreview component with Carbon Tile
  - [ ] Add tags sidebar with Carbon Tag components
  - [ ] Implement pagination with Carbon Pagination
  - [ ] Add loading states with Carbon Loading
- [ ] Implement tag filtering
  - [ ] Click handler for tags
  - [ ] Update article list based on selected tag
  - [ ] Visual feedback for active tag

**Acceptance Criteria**:
- Users can register and login successfully
- Home page displays article feed with proper Carbon styling
- Tab switching works (Your Feed / Global Feed)
- Tag filtering works
- Pagination works
- Loading states display correctly

---

### Phase 3: Article Viewing & Creation (Week 5-6)
**Goal**: Enable users to view, create, and edit articles

#### Tasks:
- [ ] Create Article page
  - [ ] Article header with metadata (using Carbon Tile, Button)
  - [ ] Article body (markdown rendering)
  - [ ] Tags display (Carbon Tag)
  - [ ] Favorite button (Carbon Button with icon)
  - [ ] Follow author button
  - [ ] Edit/Delete buttons (conditional for author)
- [ ] Create Editor page
  - [ ] Title input (Carbon TextInput)
  - [ ] Description input (Carbon TextInput)
  - [ ] Body textarea (Carbon TextArea)
  - [ ] Tags input (custom with Carbon Tag or ComboBox)
  - [ ] Publish button (Carbon Button)
  - [ ] Form validation
  - [ ] Load article data for editing
- [ ] Implement article CRUD operations
  - [ ] Create article API integration
  - [ ] Update article API integration
  - [ ] Delete article API integration
  - [ ] Markdown rendering library integration

**Acceptance Criteria**:
- Users can view full article content
- Users can create new articles
- Authors can edit their own articles
- Authors can delete their own articles
- Markdown is rendered correctly
- All actions use Carbon components

---

### Phase 4: Comments & Social Features (Week 7)
**Goal**: Add commenting and social interaction features

#### Tasks:
- [ ] Create Comments section component
  - [ ] Comment list (using Carbon Tile)
  - [ ] Comment card component
  - [ ] Delete comment button (conditional)
  - [ ] Comment form (Carbon TextArea, Button)
  - [ ] Loading states
- [ ] Implement comment CRUD operations
  - [ ] Get comments API integration
  - [ ] Add comment API integration
  - [ ] Delete comment API integration
- [ ] Implement Favorite feature
  - [ ] Favorite/unfavorite API integration
  - [ ] Update UI state on favorite toggle
  - [ ] Update count display
- [ ] Implement Follow feature
  - [ ] Follow/unfollow API integration
  - [ ] Update button state
  - [ ] Update "Your Feed" after following users

**Acceptance Criteria**:
- Users can view comments on articles
- Authenticated users can add comments
- Comment authors can delete their comments
- Users can favorite/unfavorite articles
- Users can follow/unfollow other users
- All features use Carbon components

---

### Phase 5: Profile & Settings (Week 8)
**Goal**: Complete user profile management features

#### Tasks:
- [ ] Refactor Profile page with Carbon components
  - [ ] User profile card (using Carbon Tile)
  - [ ] Tabs for "My Articles" and "Favorited" (Carbon Tabs)
  - [ ] Article list with pagination
  - [ ] Follow/unfollow button
  - [ ] Edit profile link (conditional)
- [ ] Create Settings page
  - [ ] Profile image URL input (Carbon TextInput)
  - [ ] Username input (Carbon TextInput, disabled if not editable)
  - [ ] Bio textarea (Carbon TextArea)
  - [ ] Email input (Carbon TextInput)
  - [ ] Password input (Carbon PasswordInput)
  - [ ] Update button (Carbon Button)
  - [ ] Logout button (Carbon Button, kind="danger")
  - [ ] Form validation and error handling
- [ ] Implement profile update API integration
  - [ ] Update user settings
  - [ ] Handle validation errors
  - [ ] Update global user state

**Acceptance Criteria**:
- Profile page displays user info and articles
- Tab switching works (My Articles / Favorited)
- Users can view other profiles
- Users can edit their own profile via Settings
- Logout functionality works
- All pages use Carbon components

---

### Phase 6: Polish & Enhancement (Week 9)
**Goal**: Add finishing touches and improve user experience

#### Tasks:
- [ ] Implement skeleton loading states
  - [ ] Use Carbon SkeletonText for text content
  - [ ] Use Carbon SkeletonPlaceholder for cards
  - [ ] Article list loading skeleton
  - [ ] Article page loading skeleton
- [ ] Add empty states throughout
  - [ ] "No articles yet" for empty feeds
  - [ ] "No comments yet" for articles without comments
  - [ ] "No favorited articles" for empty favorites
- [ ] Improve error handling
  - [ ] Better error messages
  - [ ] Toast notifications for success actions (using ToastNotification)
  - [ ] Form field validation feedback
- [ ] Accessibility improvements
  - [ ] Ensure all Carbon components have proper ARIA labels
  - [ ] Keyboard navigation
  - [ ] Focus management
  - [ ] Screen reader support
- [ ] Responsive design refinement
  - [ ] Test on mobile, tablet, desktop
  - [ ] Adjust Carbon Grid breakpoints
  - [ ] Touch-friendly interactions
- [ ] Performance optimization
  - [ ] Code splitting
  - [ ] Lazy loading
  - [ ] API caching
  - [ ] Debouncing for search/filter

**Acceptance Criteria**:
- All pages have loading states
- Empty states are informative
- Error handling is robust
- Application is accessible
- Application is responsive
- Performance is optimized

---

### Phase 7: Testing & Documentation (Week 10)
**Goal**: Ensure quality and maintainability

#### Tasks:
- [ ] Write component tests
  - [ ] Unit tests for all components
  - [ ] Integration tests for pages
  - [ ] Test Carbon component integrations
- [ ] Write E2E tests
  - [ ] User flows testing
  - [ ] Critical path testing
  - [ ] Cross-browser testing
- [ ] Create documentation
  - [ ] Component usage guide
  - [ ] Carbon Design System integration guide
  - [ ] Development setup instructions
  - [ ] Contribution guidelines
- [ ] Code review and refactoring
  - [ ] Review all Carbon component usage
  - [ ] Ensure consistent patterns
  - [ ] Remove technical debt

**Acceptance Criteria**:
- Test coverage > 80%
- E2E tests pass
- Documentation is complete
- Code quality is high

---

## Technical Considerations

### Carbon Design System Integration

#### Installation & Setup
```bash
npm install @carbon/react
# Carbon is already installed based on package.json
```

#### Theme Configuration
```javascript
// In main.tsx or App.tsx
import '@carbon/react/scss/styles.scss';
// Or use specific theme:
// import '@carbon/react/scss/themes/white.scss';
// import '@carbon/react/scss/themes/g10.scss';
// import '@carbon/react/scss/themes/g90.scss';
// import '@carbon/react/scss/themes/g100.scss';
```

#### Component Import Pattern
```javascript
// Import specific components to optimize bundle size
import { Button, TextInput, Form } from '@carbon/react';

// Or import from specific paths
import Button from '@carbon/react/lib/components/Button';
```

### State Management

**Recommendation**: Use React Context API (already in place) for:
- Authentication state
- Current user
- Global UI state (loading, errors)

**Future consideration**: If app complexity grows, consider Redux or Zustand

### Routing

**Current**: React Router is already installed
**Strategy**:
- Use React Router v7 features (already in package.json)
- Implement protected routes
- Handle 404s
- Preserve hash routing (`#/`) for RealWorld compatibility

### API Integration

**Current**: API client exists in `src/api/client.ts`
**Enhancements needed**:
- Add interceptors for JWT token
- Handle 401 responses (redirect to login)
- Implement retry logic
- Add request cancellation
- Type safety with TypeScript interfaces

### Markdown Rendering

**Recommendation**: Use a library like `react-markdown` or `marked`
```bash
npm install react-markdown
```

### Form Validation

**Strategy**:
- Leverage Carbon Form component built-in validation
- Add custom validation logic where needed
- Display errors using Carbon InlineNotification or form field errors

### Performance

**Considerations**:
- Lazy load routes with React.lazy()
- Implement virtual scrolling for long lists (if needed)
- Optimize images (avatars, article images)
- Use React.memo for expensive components
- Debounce API calls

### Testing

**Current**: Vitest is configured
**Strategy**:
- Write tests for all new components
- Test Carbon component integrations
- Ensure accessibility compliance
- Use React Testing Library best practices

### Accessibility

**Carbon Design System Benefits**:
- All Carbon components are accessible by default
- WCAG 2.1 AA compliance
- Built-in keyboard navigation
- Screen reader support

**Additional Requirements**:
- Add proper alt text for images
- Ensure proper heading hierarchy
- Test with screen readers
- Ensure color contrast meets standards

### Browser Support

**Carbon Design System** supports:
- Modern browsers (Chrome, Firefox, Safari, Edge)
- IE11 with polyfills (if needed)

**Testing matrix**:
- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)
- Mobile browsers (iOS Safari, Chrome Mobile)

---

## Current Implementation Status

Based on exploration of `/App/Client/src`:

### ✅ Already Implemented
- React + Vite + TypeScript setup
- Carbon Design System installed (`@carbon/react@^1.92.1`)
- React Router 7
- Authentication context (`AuthContext.tsx`)
- Protected routes (`ProtectedRoute.tsx`)
- API client (`api/client.ts`, `api/auth.ts`)
- Basic pages:
  - HomePage (`pages/HomePage.tsx`)
  - LoginPage (`pages/LoginPage.tsx`)
  - RegisterPage (`pages/RegisterPage.tsx`)
  - ProfilePage (`pages/ProfilePage.tsx`)
- Test setup with Vitest and React Testing Library

### ⚠️ Needs Carbon Integration
All existing pages need to be refactored to use Carbon Design System components instead of native HTML elements.

### ❌ Not Yet Implemented
- Article page
- Editor page
- Settings page
- Comments system
- Favorite functionality
- Follow functionality
- Tag filtering
- Pagination
- Full Carbon component integration

---

## Success Criteria

The frontend replication will be considered complete when:

1. ✅ All pages from RealWorld spec are implemented
2. ✅ All Carbon Design System components are properly integrated
3. ✅ All user flows work as expected
4. ✅ All API endpoints are integrated
5. ✅ Authentication flow is complete
6. ✅ Responsive design works on mobile, tablet, and desktop
7. ✅ Accessibility standards are met (WCAG 2.1 AA)
8. ✅ Test coverage > 80%
9. ✅ E2E tests pass
10. ✅ Documentation is complete

---

## Appendix A: Carbon Design System Resources

- **Main Website**: https://carbondesignsystem.com/
- **React Storybook**: https://react.carbondesignsystem.com/
- **GitHub**: https://github.com/carbon-design-system/carbon
- **NPM Package**: https://www.npmjs.com/package/@carbon/react
- **Getting Started**: https://carbondesignsystem.com/developing/frameworks/react/

---

## Appendix B: RealWorld Resources

- **Main Repository**: https://github.com/gothinkster/realworld
- **Demo Site**: https://demo.realworld.show
- **API Spec**: https://docs.realworld.show/specifications/backend/endpoints/
- **Features**: https://docs.realworld.show/implementation-creation/features/

---

## Appendix C: Screenshots from Demo Site

Screenshots from the RealWorld demo site exploration are available in `/Docs/RealWorld-Analysis/`:

1. `01-home-page-logged-out.png` - Home page (anonymous user)
2. `02-sign-up-page.png` - Sign up page
3. `03-sign-in-page.png` - Sign in page

Additional screenshots can be taken as needed during implementation.

---

**Document Version**: 1.0  
**Last Updated**: 2025-11-10  
**Author**: GitHub Copilot Agent  
**Status**: Ready for Implementation
