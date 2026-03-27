# Conduit API — Complete Specification Reference

> **Single source of truth for the Conduit (RealWorld) API.**
> Do NOT fetch external URLs — everything you need is here.

## Overview

Conduit is a social blogging platform (Medium.com clone). **This is NOT the standard gothinkster RealWorld API.** Key differences:
- Registration: `POST /identity/register` returns 204 (not a user object)
- Login: `POST /identity/login?useCookies=false` returns `{ accessToken }`
- Auth header: `Bearer` prefix (not `Token`)
- Most endpoints require authentication (including profiles, article listing, tags)
- Username defaults to user's email address
- Validation errors return HTTP 400 (not 422)
- Comment IDs are GUIDs (not integers)
- Second user: `POST /identity/invite` (requires ADMIN role)

## Base URL & Conventions

- **Base URL:** `http://localhost:{PORT}` — all routes under `/api/` prefix
- **Content-Type:** `application/json`
- **Required Headers:** `X-Requested-With: XMLHttpRequest`, `x-correlation-id: {UUID}`
- **Postman `{{APIURL}}`** already includes `/api`, so test URLs like `{{APIURL}}/articles` → `/api/articles`

---

## Response Shapes

### User `{ "user": UserResponse }`
```typescript
interface UserResponse {
  email: string;        // User's email
  username: string;     // Defaults to email on registration
  bio: string;          // Default: "I work at statefarm"
  image: string | null; // Default: null
  roles: string[];      // ["ADMIN", "USER"] or ["USER"]
}
```

### Profile `{ "profile": ProfileResponse }`
```typescript
interface ProfileResponse {
  username: string;
  bio: string;
  image: string | null;
  following: boolean;   // Whether authenticated user follows this profile
}
```

### Article `{ "article": ArticleResponse }` or `{ "articles": ArticleResponse[], "articlesCount": number }`
```typescript
interface ArticleResponse {
  slug: string;           // Auto-generated from title (kebab-case)
  title: string;
  description: string;
  body: string;           // ONLY in single-article responses, EXCLUDED from lists/feed
  tagList: string[];      // Ordered array, may be empty
  createdAt: string;      // ISO 8601
  updatedAt: string;      // ISO 8601
  favorited: boolean;     // Whether authenticated user favorited this
  favoritesCount: number; // Integer (not float), total unique users
  author: ProfileResponse;
}
```

### Comment `{ "comment": CommentResponse }` or `{ "comments": CommentResponse[] }`
```typescript
interface CommentResponse {
  id: string;           // GUID format
  body: string;
  createdAt: string;    // ISO 8601
  updatedAt: string;    // ISO 8601
  author: ProfileResponse;
}
```

### Login Response
```json
{ "tokenType": "Bearer", "accessToken": "eyJ...", "expiresIn": 3600, "refreshToken": "CfDJ8..." }
```

### Error Formats (tests handle BOTH)
```javascript
// Format 1 (Identity style): { "errors": { "DuplicateEmail": ["message"] } }
// Format 2 (FastEndpoints):  { "errors": [{ "name": "field", "reason": "message" }] }
// Test matching: key.toLowerCase().includes(fieldName) OR err.name === fieldName
```

| Status | When |
|--------|------|
| 400 | Validation failure, duplicates, business rule violation |
| 401 | Missing/invalid auth token |
| 403 | Authenticated but not authorized |
| 404 | Resource doesn't exist |
| 422 | Some validation cases (e.g., comments on non-existent article) |

---

## Request DTOs

```typescript
interface RegisterRequest { email: string; password: string; }
interface LoginRequest { email: string; password: string; }
interface UpdateUserRequest { user: { email?: string; username?: string; password?: string; bio?: string; image?: string; } }
interface CreateArticleRequest { article: { title: string; description: string; body: string; tagList?: string[]; } }
interface CreateCommentRequest { comment: { body: string; } }
```

---

## Endpoints — Identity

### POST `/api/identity/register` — Register First User
- **Auth:** None | **Response:** 204 No Content
- **Body:** `{ email, password }`
- **Errors:** 400 if duplicate/invalid email (error key contains `"email"`)
- **Validation:** email required + valid format + unique across ALL tenants; password required
- **Side effects:** Creates new tenant, sets `UserName = Email`, `Bio = "I work at statefarm"`, `Image = null`, assigns ADMIN + USER roles

### POST `/api/identity/invite` — Register Additional User
- **Auth:** Bearer (ADMIN) | **Response:** 204 No Content
- **Body:** `{ email, password }`
- **Errors:** 400 duplicate email, 401 missing/invalid token
- **Validation:** email required + valid + unique across all tenants; password required + min 6 chars
- **Side effects:** Creates user in inviting admin's tenant, `UserName = Email`, USER role only

### POST `/api/identity/login?useCookies=false` — Login
- **Auth:** None | **Response:** 200 with login response shape (see above)
- **Body:** `{ email, password }`
- **Errors:** 401 wrong credentials (no error body)
- **Logic:** Finds user across ALL tenants, checks lockout (5 attempts → 10 min), adds `__tenant__` claim to JWT

---

## Endpoints — User

### GET `/api/user` — Get Current User
- **Auth:** Bearer | **Response:** 200 `{ user: UserResponse }`
- **Errors:** 401
- **Assertions:** `user.username` matches registered email, `user.bio` and `user.image` exist

### PUT `/api/user` — Update Current User
- **Auth:** Bearer | **Response:** 200 `{ user: UserResponse }`
- **Body:** `{ user: { email?, username?, password?, bio?, image? } }` (partial update)
- **Errors:** 400 validation/duplicates, 401
- **Validation (only when field provided):**
  - email: not empty, valid format, max 255, unique
  - username: not empty, min 2, max 100, unique
  - password: not empty, min 6
  - bio: not empty, max 1000
- **Assertions:** `username=""`, `email=""`, `password=""`, `bio=""` → 400 with errors for each; duplicate email/username → 400

---

## Endpoints — Profiles

### GET `/api/profiles/{username}` — Get Profile
- **Auth:** Bearer | **Response:** 200 `{ profile: ProfileResponse }`
- **Errors:** 401, 404 (user doesn't exist or empty username)
- **Assertions:** `profile.following` reflects authenticated user's follow state

### POST `/api/profiles/{username}/follow` — Follow User
- **Auth:** Bearer | **Response:** 200 `{ profile: ProfileResponse }` with `following: true`
- **Body:** `{}` | **Errors:** 401, 404
- **Idempotent:** following someone already followed → 200 OK

### DELETE `/api/profiles/{username}/follow` — Unfollow User
- **Auth:** Bearer | **Response:** 200 `{ profile: ProfileResponse }` with `following: false`
- **Errors:** 400 not currently following (error for `"username"`), 401
- **NOT idempotent:** unfollowing someone you don't follow → 400

---

## Endpoints — Articles

### POST `/api/articles` — Create Article
- **Auth:** Bearer | **Response:** 201 `{ article: ArticleResponse }`
- **Body:** `{ article: { title, description, body, tagList? } }`
- **Errors:** 400 missing/blank fields (errors for `"title"`, `"description"`, `"body"`), 400 duplicate slug (error for `"slug"`), 400 invalid tag with comma (error for `"article.TagList[0]"`), 400 empty tag (error for `"article.TagList[0]"`), 401
- **Validation:** title/description/body required + non-empty; each tag non-empty + no commas
- **Assertions:** `favoritesCount` is integer; `createdAt`/`updatedAt` ISO 8601 and after request time; `tagList` preserves input order; without `tagList` → `tagList: []`; `author.following` is false for own articles

### GET `/api/articles/{slug}` — Get Single Article
- **Auth:** Bearer | **Response:** 200 `{ article: ArticleResponse }`
- **Errors:** 401, 404
- **Assertions:** `body` IS included; `author.following` reflects authenticated user's state

### GET `/api/articles` — List Articles
- **Auth:** Bearer | **Response:** 200 `{ articles: ArticleResponse[], articlesCount: number }`
- **Query params:** `author`, `tag`, `favorited` (username), `limit` (default 20, min 1), `offset` (default 0, min 0)
- **Errors:** 400 invalid pagination (errors for `"offset"`, `"limit"`), 400 empty `tag`/`author` param, 401
- **Assertions:**
  - `body` field NOT included in list items
  - Ordered by most recent first (descending creation time)
  - `articlesCount` is TOTAL matching count, not page size
  - `limit=4` on 5 articles → 4 articles but `articlesCount: 5`
  - `offset=1` skips first article
  - Filters: `author=username`, `tag=tagname`, `favorited=username`
  - `favorited` field reflects authenticated user's state
  - `author.following` reflects authenticated user's follow state
  - Non-existent author/tag/favorited user → `articlesCount: 0`

### PUT `/api/articles/{slug}` — Update Article *(not Postman-tested)*
- **Auth:** Bearer (article author) | **Response:** 200 full article
- **Errors:** 401, 403, 404

### DELETE `/api/articles/{slug}` — Delete Article *(not Postman-tested)*
- **Auth:** Bearer (article author) | **Response:** 204
- **Errors:** 401, 403, 404

---

## Endpoints — Comments

### POST `/api/articles/{slug}/comments` — Create Comment
- **Auth:** Bearer | **Response:** 201 `{ comment: CommentResponse }`
- **Body:** `{ comment: { body } }`
- **Errors:** 400 missing/blank body (error for `"body"`), 401, 404 article not found
- **Assertions:** `comment.id` exists (GUID); `createdAt` ISO 8601 after request time; `author.following` reflects follow state

### GET `/api/articles/{slug}/comments` — Get Comments
- **Auth:** Bearer | **Response:** 200 `{ comments: CommentResponse[] }`
- **Errors:** 401, 422 article doesn't exist
- **Assertions:** Chronological order (oldest first); deleted comments not returned; empty article → `comments: []`

### DELETE `/api/articles/{slug}/comments/{id}` — Delete Comment
- **Auth:** Bearer (comment author) | **Response:** 204
- **Errors:** 400 invalid GUID format (error for `"id"`), 401, 403 not comment author, 404 comment not found (valid GUID), 422 article doesn't exist
- **Assertions:** Only comment author can delete (others get 403); deleting one leaves others intact

---

## Endpoints — Favorites

### POST `/api/articles/{slug}/favorite` — Favorite Article
- **Auth:** Bearer | **Response:** 200 `{ article: ArticleResponse }` with `favorited: true`
- **Body:** `{}` | **Errors:** 401, 404
- **Idempotent:** favoriting twice → same `favoritesCount` (no double-count)
- **Assertions:** Returns full article (includes `body`); multiple users can favorite (count increments per unique user)

### DELETE `/api/articles/{slug}/favorite` — Unfavorite Article
- **Auth:** Bearer | **Response:** 200 `{ article: ArticleResponse }` with `favorited: false`
- **Errors:** 401, 404
- **Assertions:** Unfavoriting something not favorited → `favorited: false`, `favoritesCount: 0`

---

## Endpoints — Feed

### GET `/api/articles/feed` — Get Feed
- **Auth:** Bearer | **Response:** 200 `{ articles: ArticleResponse[], articlesCount: number }`
- **Query params:** `limit` (default 20, min 1), `offset` (default 0, min 0)
- **Errors:** 400 invalid pagination (errors for `"limit"`, `"offset"`), 401
- **Rules:** Returns articles ONLY from followed users. Excludes own articles. Excludes unfollowed users.
- **Assertions:**
  - `body` NOT included in feed items
  - Most recent first
  - `articlesCount` is TOTAL feed count
  - `author.following` is true for all feed articles
  - `limit=2` → at most 2 articles
  - `offset` beyond feed → empty `articles: []` with correct `articlesCount`
  - Empty feed → `{ articles: [], articlesCount: 0 }`

---

## Endpoints — Tags

### GET `/api/tags` — Get All Tags
- **Auth:** Bearer | **Response:** 200 `{ tags: string[] }`
- **Assertions:** Returns all tags from current tenant's articles; no articles → `tags: []`

---

## Database Entities (need to be created)

```csharp
// ApplicationUser already exists with these constraints:
// EmailMaxLength=255, UsernameMin=2, UsernameMax=100, PasswordMin=6, BioMax=1000, ImageUrlMax=500
// Default Bio="I work at statefarm", Image=null

// Need to create:
// Article: slug, title, description, body, createdAt, updatedAt, authorId (FK)
// Tag: name (unique), many-to-many with Article
// Comment: id (GUID), body, createdAt, updatedAt, articleId (FK), authorId (FK)
// ArticleFavorite: userId (FK), articleId (FK) — join table
// UserFollowing: followerId (FK), followedId (FK) — join table
```

## Business Rules

**Slug generation:** lowercase → remove `[^a-z0-9\s\-_]` → whitespace→hyphens → trim hyphens. Duplicate slugs rejected (400 error for `"slug"`).

**Tags:** Ordered arrays, order preserved from input. Non-empty, no commas. No tagList → `[]`.

**Pagination:** Default limit=20, min limit=1, min offset=0. Non-numeric values → 400. `articlesCount` always TOTAL matching count.

**Multi-tenancy:** First user creates tenant + gets ADMIN+USER. Subsequent users invited by ADMIN into same tenant. All data tenant-isolated (Finbuckle query filters).

**Authorization:** Comments: only author can delete (403 for others). Articles: only author can update/delete. User profile: only self.

---

## Implementation Status

### Already Built
| Endpoint | Location |
|----------|----------|
| `POST /api/identity/register` | `Server.Web/Identity/Register/` |
| `POST /api/identity/invite` | `Server.Web/Identity/Invite/` |
| `POST /api/identity/login` | `Server.Web/Identity/Login/` |
| `POST /api/identity/logout` | `Server.Web/Identity/Logout/` |
| `GET /api/user` | `Server.Web/Users/GetCurrent/` |
| `PUT /api/user` | `Server.Web/Users/Update/` |
| `GET /api/profiles/{username}` | `Server.Web/Profiles/Get/` |
| `GET /api/users` | `Server.Web/Users/List/` |

### Needs to Be Built
| Endpoint | Postman Collection |
|----------|-------------------|
| `POST /api/profiles/{username}/follow` | Conduit.Profiles |
| `DELETE /api/profiles/{username}/follow` | Conduit.Profiles |
| `POST /api/articles` | Conduit.Article |
| `GET /api/articles/{slug}` | Conduit.Article |
| `GET /api/articles` | Conduit.FeedAndArticles, Conduit.ArticlesEmpty |
| `GET /api/articles/feed` | Conduit.FeedAndArticles |
| `POST /api/articles/{slug}/favorite` | Conduit.Article |
| `DELETE /api/articles/{slug}/favorite` | Conduit.Article |
| `POST /api/articles/{slug}/comments` | Conduit.Article |
| `GET /api/articles/{slug}/comments` | Conduit.Article |
| `DELETE /api/articles/{slug}/comments/{id}` | Conduit.Article |
| `GET /api/tags` | Conduit.Article, Conduit.ArticlesEmpty |

### Existing Profile Endpoint Needs Enhancement
The current `GET /api/profiles/{username}` does NOT include a `following` field. The Postman tests expect it. Update to query UserFollowing table and include `following: boolean`.

## Postman Collections
| Collection | Tests |
|------------|-------|
| `Conduit.Auth` | Registration, login, get user, update user |
| `Conduit.Article` | Article CRUD, tags, favorites, comments |
| `Conduit.ArticlesEmpty` | Empty state: articles, tags |
| `Conduit.Profiles` | Profile get, follow, unfollow |
| `Conduit.FeedAndArticles` | Feed, article listing with filters |

Each collection is independent — registers its own users with random email prefixes. Tests run sequentially within each collection.
