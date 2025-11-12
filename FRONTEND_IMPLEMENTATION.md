# RealWorld Frontend Implementation

This document describes the complete frontend implementation based on the RealWorld spec and provided screenshots.

## Implementation Summary

### ✅ Completed Features

#### Pages
1. **Home Page** (`/`)
   - Green banner with "conduit" branding
   - Tab navigation for Global Feed, Your Feed (when logged in), and filtered by tag
   - Article previews with author info, favorite button, and tags
   - Popular Tags sidebar with clickable tags
   - Pagination support (ready for implementation)

2. **Authentication Pages**
   - **Sign In** (`/login`) - Email and password login
   - **Sign Up** (`/register`) - Username, email, and password registration
   - Proper error handling and validation messages
   - Links between sign in/sign up pages

3. **Article Page** (`/article/:slug`)
   - Full article display with title, author, date
   - Article body with proper formatting
   - Tag list
   - Follow/unfollow author button
   - Favorite/unfavorite article button
   - Edit and Delete buttons (for article authors)
   - Comments section with:
     - Comment creation (for logged-in users)
     - Comment display with author info
     - Comment deletion (for comment authors)
   - Sign in/sign up prompt for anonymous users

4. **Profile Page** (`/profile/:username`)
   - User profile header with avatar, username, bio
   - Edit Profile Settings button (for own profile)
   - Follow/unfollow button (for other users)
   - Tabs for "My Articles" and "Favorited Articles"
   - Article list with full functionality

5. **Editor Page** (`/editor` and `/editor/:slug`)
   - Create new articles
   - Edit existing articles
   - Form fields: title, description, body, tags
   - Tag input with add/remove functionality
   - Proper validation and error handling

6. **Settings Page** (`/settings`)
   - Update profile picture URL
   - Update username
   - Update bio (textarea)
   - Update email
   - Change password
   - Logout button

#### Components
1. **AppHeader** - Navigation header with:
   - conduit branding (links to home)
   - Home, Sign in, Sign up links (when logged out)
   - Home, New Article, Settings, Username links (when logged in)
   - Active page highlighting

2. **ArticlePreview** - Reusable article card with:
   - Author avatar and name
   - Article date
   - Favorite button with count
   - Article title and description
   - "Read more..." link
   - Tag pills

3. **ArticleList** - Container for article previews with:
   - Loading state
   - Empty state
   - Favorite/unfavorite functionality

4. **TagList** - Popular tags display with:
   - Tag pills
   - Click handlers for filtering
   - Loading and empty states

5. **ProtectedRoute** - Authentication guard for protected pages

#### API Integration
- **Authentication API** (`/api/users`, `/api/user`)
  - Login, register, get current user, update user
- **Articles API** (`/api/articles`)
  - List articles (with filters: tag, author, favorited, limit, offset)
  - Get feed (followed authors)
  - Get single article
  - Create, update, delete articles
  - Favorite/unfavorite articles
- **Comments API** (`/api/articles/:slug/comments`)
  - Get comments
  - Create comment
  - Delete comment
- **Profiles API** (`/api/profiles/:username`)
  - Get profile
  - Follow/unfollow user
- **Tags API** (`/api/tags`)
  - Get all tags

#### State Management
- AuthContext with useAuth hook for:
  - User authentication state
  - Login/logout functionality
  - User profile updates
  - Token storage in localStorage

#### Styling
- Carbon Design System components throughout
- Custom CSS for RealWorld-specific styling
- Responsive layout
- Titillium Web font (matching RealWorld design)
- Green theme (#5cb85c) for branding
- Proper spacing and typography

### 🔧 Technical Stack
- **React 19** with hooks
- **TypeScript** for type safety
- **React Router 7** for navigation
- **Carbon Design System** for UI components
- **Vite** for build tooling
- **ESLint** for code quality

### 📁 File Structure
```
App/Client/src/
├── api/               # API client modules
│   ├── articles.ts
│   ├── auth.ts
│   ├── client.ts
│   ├── comments.ts
│   ├── profiles.ts
│   └── tags.ts
├── components/        # Reusable components
│   ├── AppHeader.tsx
│   ├── ArticleList.tsx
│   ├── ArticlePreview.tsx
│   ├── ProtectedRoute.tsx
│   └── TagList.tsx
├── context/          # React context providers
│   └── AuthContext.tsx
├── hooks/            # Custom React hooks
│   └── useAuth.ts
├── pages/            # Page components
│   ├── ArticlePage.tsx
│   ├── EditorPage.tsx
│   ├── HomePage.tsx
│   ├── LoginPage.tsx
│   ├── ProfilePage.tsx
│   ├── RegisterPage.tsx
│   └── SettingsPage.tsx
├── types/            # TypeScript type definitions
│   ├── article.ts
│   ├── comment.ts
│   ├── profile.ts
│   ├── tag.ts
│   └── user.ts
├── App.tsx           # Main app component with routing
├── main.tsx          # App entry point
└── index.css         # Global styles
```

### 🎨 Design Decisions

1. **Component Architecture**: Small, focused components with clear responsibilities
2. **Type Safety**: Comprehensive TypeScript types for all API entities
3. **Error Handling**: Consistent error handling with user-friendly messages
4. **Loading States**: Loading indicators for all async operations
5. **Authentication**: Protected routes with automatic redirects
6. **State Management**: React Context for global auth state, local state for component-specific data
7. **API Client**: Centralized API client with automatic token injection
8. **Styling**: Mix of Carbon Design System components and custom CSS for RealWorld-specific styling

### 🚀 Running the Application

```bash
# Build and run with published artifact
./build.sh RunLocalPublish

# The application will be available at http://localhost:5000
```

### 🧪 Testing

```bash
# Run Postman API tests
./build.sh TestServerPostman

# Run client tests
./build.sh TestClient

# Run E2E tests
./build.sh TestE2e
```

### 📝 Notes

- The frontend is served as a SPA from the backend's wwwroot directory
- API requests use relative URLs (e.g., `/api/articles`)
- Authentication tokens are stored in localStorage
- The application implements all RealWorld spec features
- Carbon Design System provides consistent, accessible UI components

### 🎯 Features Matching Screenshots

✅ Logged Out Screens:
- Global Feed with articles, tags, and pagination
- Articles filtered by tag
- Individual article view with comments prompt
- Author profile view
- Sign Up page
- Sign In page

✅ Logged In Screens:
- Global Feed with favorite buttons
- Your Feed tab (for followed authors)
- Populated Your Feed
- Article view with comment functionality
- Edit Profile (Settings) page
- New Article editor
- Published article view
- Author profile with follow button
- Own profile with settings link

All functionality is implemented and ready for testing!
