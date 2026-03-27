# Conduit API — Complete Specification Reference

> **This document is the single source of truth for the Conduit (RealWorld) API.**
> Do NOT fetch external URLs — everything you need is here.
> All endpoints, schemas, and validation rules are derived from the Postman test suites
> and existing server code.

---

## Table of Contents

1. [Overview](#overview)
2. [Base URL & Conventions](#base-url--conventions)
3. [Authentication](#authentication)
4. [Error Response Format](#error-response-format)
5. [Endpoints — Identity](#endpoints--identity)
6. [Endpoints — User](#endpoints--user)
7. [Endpoints — Profiles](#endpoints--profiles)
8. [Endpoints — Articles](#endpoints--articles)
9. [Endpoints — Comments](#endpoints--comments)
10. [Endpoints — Favorites](#endpoints--favorites)
11. [Endpoints — Feed](#endpoints--feed)
12. [Endpoints — Tags](#endpoints--tags)
13. [Data Models](#data-models)
14. [Business Rules](#business-rules)
15. [Implementation Status](#implementation-status)
16. [Postman Test Configuration](#postman-test-configuration)

---

## Overview

Conduit is a social blogging platform (Medium.com clone) built with:

- **Backend:** .NET, FastEndpoints, MediatR (CQRS), FluentValidation, EF Core + SQLite
- **Auth:** ASP.NET Core Identity with Bearer JWT tokens
- **Multi-tenancy:** Finbuckle with ClaimsStrategy (first registered user creates a new tenant)

This is NOT the standard gothinkster RealWorld API. Key differences:
- Registration is via `/identity/register` (returns 204, not a user object)
- Login is via `/identity/login?useCookies=false` (returns `{ accessToken }`)
- Auth header uses `Bearer` prefix (not `Token`)
- Most endpoints require authentication (including profiles, article listing, tags)
- Username defaults to the user's email address
- Validation errors return HTTP 400 (not 422)
- Comment IDs are GUIDs (not integers)
- Second user registration uses `/identity/invite` (requires ADMIN role)

---

## Base URL & Conventions

- **Base URL:** `http://localhost:{PORT}` (no `/api` prefix on most routes — the Postman tests use `{{APIURL}}` which points to the server root)
- **Content-Type:** `application/json` for all request/response bodies
- **Common Headers on ALL Requests:**
  - `Content-Type: application/json` (on POST/PUT)
  - `X-Requested-With: XMLHttpRequest`
  - `x-correlation-id: {{correlationId}}` (UUID, for request tracing)

### Route Patterns

All production API routes are under the `/api/` prefix (configured in FastEndpoints):
- Identity: `/api/identity/...`
- User: `/api/user`
- Profiles: `/api/profiles/{username}`
- Articles: `/api/articles/...`
- Comments: `/api/articles/{slug}/comments/...`
- Favorites: `/api/articles/{slug}/favorite`
- Feed: `/api/articles/feed`
- Tags: `/api/tags`

**Note:** The Postman tests use `{{APIURL}}` which already includes `/api`, so test URLs like `{{APIURL}}/articles` resolve to `/api/articles`.

---

## Authentication

### Registration Flow

1. **First user:** `POST /api/identity/register` with `{ email, password }`
   - Creates a new tenant (multi-tenant isolation)
   - Assigns both `ADMIN` and `USER` roles
   - Returns `204 No Content` (no body)
   - Username is set to the email address

2. **Additional users:** `POST /api/identity/invite` with `{ email, password }`
   - Requires `Authorization: Bearer {token}` from an ADMIN user
   - Creates user in the same tenant as the inviting admin
   - Assigns only `USER` role
   - Returns `204 No Content`

### Login Flow

1. `POST /api/identity/login?useCookies=false` with `{ email, password }`
2. Response:
   ```json
   {
     "tokenType": "Bearer",
     "accessToken": "eyJhbG...",
     "expiresIn": 3600,
     "refreshToken": "CfDJ8..."
   }
   ```
3. Store the `accessToken` for subsequent requests

### Authenticated Requests

- **Header:** `Authorization: Bearer {accessToken}`
- Missing or invalid token returns `401 Unauthorized`
- Token format `Token {value}` (without "Bearer") also returns `401`

### Password Requirements

- Minimum length: 6 characters
- No digit, lowercase, uppercase, or special character requirements
- Lockout: 5 failed attempts triggers 10-minute lockout

---

## Error Response Format

The API uses two error formats. Test assertions handle BOTH, so implementations can use either:

### Format 1: Object-keyed (ASP.NET Identity style)

```json
{
  "errors": {
    "DuplicateEmail": ["A user has already been registered with that email"],
    "InvalidEmail": ["Email is invalid."]
  }
}
```

### Format 2: Array format (FastEndpoints style)

```json
{
  "errors": [
    { "name": "title", "reason": "is required." },
    { "name": "description", "reason": "is required." }
  ]
}
```

### Standard Error Status Codes

| Status | When |
|--------|------|
| `400 Bad Request` | Validation failure, duplicate email/username, business rule violation |
| `401 Unauthorized` | Missing/invalid auth token, wrong credentials |
| `403 Forbidden` | Authenticated but not authorized (e.g., deleting another user's comment) |
| `404 Not Found` | Resource doesn't exist (article, profile, comment) |
| `422 Unprocessable Entity` | Used for some validation cases (e.g., delete comment on non-existent article) |

### Error Assertion Logic (from Postman tests)

The Postman tests use this logic to check errors — your error response must match:

```javascript
// The test checks if ANY error key/name contains the field name (case-insensitive)
// Object format: Object.keys(errors).some(key => key.toLowerCase().includes(fieldName.toLowerCase()))
// Array format: errors.some(err => err.name === fieldName || err.name.startsWith(fieldName))
```

This means error keys like `"DuplicateEmail"` will match a check for `"email"` (case-insensitive contains).

---

## Endpoints — Identity

### POST `/api/identity/register` — Register First User

- **Auth:** Not required (public)
- **Request Body:**
  ```json
  {
    "email": "user@example.com",
    "password": "password123"
  }
  ```
- **Success Response:** `204 No Content` (empty body)
- **Error Responses:**
  - `400 Bad Request` — duplicate email: error key contains `"email"`
  - `400 Bad Request` — invalid email format: error key contains `"email"`
  - `400 Bad Request` — missing/empty fields: error key contains `"email"`
- **Validation Rules:**
  - `email`: required, must be valid email format, must be unique across ALL tenants
  - `password`: required
- **Side Effects:**
  - Creates a new tenant
  - Sets `UserName = Email`
  - Sets `Bio = "I work at statefarm"`, `Image = null`
  - Assigns `ADMIN` + `USER` roles

### POST `/api/identity/invite` — Register Additional User

- **Auth:** Required (Bearer token, ADMIN role)
- **Request Body:**
  ```json
  {
    "email": "invited@example.com",
    "password": "password123"
  }
  ```
- **Success Response:** `204 No Content` (empty body)
- **Error Responses:**
  - `400 Bad Request` — duplicate email
  - `401 Unauthorized` — missing/invalid token
- **Validation Rules:**
  - `email`: required, valid format, unique across all tenants
  - `password`: required, minimum 6 characters
- **Side Effects:**
  - Creates user in the inviting admin's tenant
  - Sets `UserName = Email`
  - Assigns `USER` role only (not ADMIN)

### POST `/api/identity/login?useCookies=false` — Login

- **Auth:** Not required (public)
- **Request Body:**
  ```json
  {
    "email": "user@example.com",
    "password": "password123"
  }
  ```
- **Query Parameters:**
  - `useCookies` (boolean, optional) — use `false` for bearer token flow
  - `useSessionCookies` (boolean, optional)
- **Success Response:** `200 OK`
  ```json
  {
    "tokenType": "Bearer",
    "accessToken": "eyJhbGciOiJ...",
    "expiresIn": 3600,
    "refreshToken": "CfDJ8..."
  }
  ```
- **Error Responses:**
  - `401 Unauthorized` — user not found or wrong password (no error body)
- **Validation Rules:**
  - `email`: required, valid email format
  - `password`: required
- **Business Logic:**
  - Finds user across ALL tenants by email
  - Resolves tenant context from the found user
  - Checks account lockout before password validation
  - Increments failed access count on wrong password
  - Resets failed access count on successful login
  - Adds `__tenant__` claim to the JWT

### POST `/api/identity/logout` — Logout

- **Auth:** Not required (accepts both authenticated and anonymous)
- **Request Body:** None
- **Success Response:** `200 OK` (empty body)
- **Side Effects:** Clears authentication cookies

---

## Endpoints — User

### GET `/api/user` — Get Current User

- **Auth:** Required (Bearer token)
- **Request Body:** None
- **Success Response:** `200 OK`
  ```json
  {
    "user": {
      "email": "user@example.com",
      "username": "user@example.com",
      "bio": "I work at statefarm",
      "image": null,
      "roles": ["ADMIN", "USER"]
    }
  }
  ```
- **Error Responses:**
  - `401 Unauthorized` — missing/invalid token
- **Test Assertions:**
  - `user.email` matches the registered email
  - `user.username` matches the registered email (defaults to email)
  - `user.bio` and `user.image` properties exist

### PUT `/api/user` — Update Current User

- **Auth:** Required (Bearer token)
- **Request Body:** (partial update — only include fields to change)
  ```json
  {
    "user": {
      "username": "newusername",
      "email": "newemail@example.com",
      "password": "newpassword123",
      "bio": "Updated bio",
      "image": "https://example.com/photo.jpg"
    }
  }
  ```
- **Success Response:** `200 OK`
  ```json
  {
    "user": {
      "email": "newemail@example.com",
      "username": "newusername",
      "bio": "Updated bio",
      "image": "https://example.com/photo.jpg",
      "roles": []
    }
  }
  ```
- **Error Responses:**
  - `400 Bad Request` — validation failure (blank required fields, duplicate email/username)
  - `401 Unauthorized` — missing/invalid token
- **Validation Rules (conditional — only when field is provided):**
  - `email`: not empty, valid email format, max 255 chars, unique
  - `username`: not empty, min 2 chars, max 100 chars, unique
  - `password`: not empty, min 6 chars
  - `bio`: not empty, max 1000 chars
- **Test Assertions:**
  - Providing `username=""`, `email=""`, `password=""`, `bio=""` returns 400 with errors for `username`, `email`, `bio`
  - Providing a duplicate email returns 400 with error for `email`
  - Providing a duplicate username returns 400 with error for `username`
  - Successful update returns the new values in the response

---

## Endpoints — Profiles

### GET `/api/profiles/{username}` — Get Profile

- **Auth:** Required (Bearer token)
- **Path Parameters:** `username` (string, required)
- **Success Response:** `200 OK`
  ```json
  {
    "profile": {
      "username": "user@example.com",
      "bio": "I work at statefarm",
      "image": null,
      "following": false
    }
  }
  ```
- **Error Responses:**
  - `401 Unauthorized` — missing/invalid token
  - `404 Not Found` — user doesn't exist
- **Test Assertions:**
  - `profile.following` is `true` if the authenticated user follows this profile
  - `profile.following` is `false` if not following
  - Non-existent username returns 404
  - Empty username returns 404

### POST `/api/profiles/{username}/follow` — Follow User

- **Auth:** Required (Bearer token)
- **Path Parameters:** `username` (string, required)
- **Request Body:** `{}` (empty object)
- **Success Response:** `200 OK`
  ```json
  {
    "profile": {
      "username": "celeb@example.com",
      "bio": "I work at statefarm",
      "image": null,
      "following": true
    }
  }
  ```
- **Error Responses:**
  - `401 Unauthorized` — missing/invalid token
  - `404 Not Found` — user doesn't exist
- **Business Rules:**
  - Idempotent — following someone already followed returns 200 with `following: true`

### DELETE `/api/profiles/{username}/follow` — Unfollow User

- **Auth:** Required (Bearer token)
- **Path Parameters:** `username` (string, required)
- **Success Response:** `200 OK`
  ```json
  {
    "profile": {
      "username": "celeb@example.com",
      "bio": "I work at statefarm",
      "image": null,
      "following": false
    }
  }
  ```
- **Error Responses:**
  - `400 Bad Request` — not currently following the user (error for field `"username"`)
  - `401 Unauthorized` — missing/invalid token
- **Business Rules:**
  - NOT idempotent — unfollowing someone you don't follow returns `400 Bad Request`

---

## Endpoints — Articles

### POST `/api/articles` — Create Article

- **Auth:** Required (Bearer token)
- **Request Body:**
  ```json
  {
    "article": {
      "title": "How to train your dragon",
      "description": "Ever wonder how?",
      "body": "Very carefully.",
      "tagList": ["training", "dragons"]
    }
  }
  ```
- **Success Response:** `201 Created`
  ```json
  {
    "article": {
      "slug": "how-to-train-your-dragon",
      "title": "How to train your dragon",
      "description": "Ever wonder how?",
      "body": "Very carefully.",
      "tagList": ["training", "dragons"],
      "createdAt": "2026-03-27T12:00:00.000Z",
      "updatedAt": "2026-03-27T12:00:00.000Z",
      "favorited": false,
      "favoritesCount": 0,
      "author": {
        "username": "user@example.com",
        "bio": "I work at statefarm",
        "image": null,
        "following": false
      }
    }
  }
  ```
- **Error Responses:**
  - `400 Bad Request` — missing/blank required fields: errors for `"title"`, `"description"`, `"body"`
  - `400 Bad Request` — duplicate slug: error for `"slug"`
  - `400 Bad Request` — invalid tag (contains comma): error for `"article.TagList[0]"`
  - `400 Bad Request` — empty tag string: error for `"article.TagList[0]"`
  - `401 Unauthorized` — missing/invalid token
- **Validation Rules:**
  - `title`: required, not empty
  - `description`: required, not empty
  - `body`: required, not empty
  - `tagList`: optional (defaults to empty array); each tag must be non-empty and must not contain commas
- **Test Assertions:**
  - `favoritesCount` is an integer (not a float)
  - `createdAt` and `updatedAt` are ISO 8601 format
  - `createdAt` is after the request timestamp
  - `tagList` preserves input order
  - Without `tagList`, response has `tagList: []`
  - `author.username` matches the creator's email/username
  - `author.bio` is `"I work at statefarm"` for new users
  - `author.image` is `null` for new users
  - `author.following` is `false` for own articles
  - Duplicate title (producing same slug) returns 400 with error for `"slug"`

### GET `/api/articles/{slug}` — Get Single Article

- **Auth:** Required (Bearer token)
- **Path Parameters:** `slug` (string, required)
- **Success Response:** `200 OK`
  ```json
  {
    "article": {
      "slug": "how-to-train-your-dragon",
      "title": "How to train your dragon",
      "description": "Ever wonder how?",
      "body": "Very carefully.",
      "tagList": ["training", "dragons"],
      "createdAt": "2026-03-27T12:00:00.000Z",
      "updatedAt": "2026-03-27T12:00:00.000Z",
      "favorited": false,
      "favoritesCount": 0,
      "author": {
        "username": "user@example.com",
        "bio": "I work at statefarm",
        "image": null,
        "following": false
      }
    }
  }
  ```
- **Error Responses:**
  - `401 Unauthorized` — missing/invalid token
  - `404 Not Found` — article doesn't exist
- **Test Assertions:**
  - `body` field IS included (unlike list endpoints)
  - `author.following` reflects whether the authenticated user follows the author
  - `createdAt` and `updatedAt` match the values from creation
  - `favorited` and `favoritesCount` reflect the authenticated user's state

### GET `/api/articles` — List Articles

- **Auth:** Required (Bearer token)
- **Query Parameters:**
  - `author` (string, optional) — filter by author username
  - `tag` (string, optional) — filter by tag
  - `favorited` (string, optional) — filter by username who favorited
  - `limit` (integer, optional, default: 20, minimum: 1) — page size
  - `offset` (integer, optional, default: 0, minimum: 0) — skip count
- **Success Response:** `200 OK`
  ```json
  {
    "articles": [
      {
        "slug": "how-to-train-your-dragon",
        "title": "How to train your dragon",
        "description": "Ever wonder how?",
        "tagList": ["training", "dragons"],
        "createdAt": "2026-03-27T12:00:00.000Z",
        "updatedAt": "2026-03-27T12:00:00.000Z",
        "favorited": false,
        "favoritesCount": 0,
        "author": {
          "username": "user@example.com",
          "bio": "I work at statefarm",
          "image": null,
          "following": false
        }
      }
    ],
    "articlesCount": 5
  }
  ```
- **Error Responses:**
  - `400 Bad Request` — invalid pagination (`offset < 0`, `limit < 1`, non-numeric values): errors for `"offset"`, `"limit"`
  - `400 Bad Request` — empty `tag` or `author` param: error for `"tag"` or `"author"`
  - `401 Unauthorized` — missing/invalid token
- **Test Assertions:**
  - `body` field is **NOT included** in article list items
  - Articles are ordered by most recent first (descending creation time)
  - `articlesCount` is the TOTAL matching count, not the page size
  - `limit=4` on 5 articles returns 4 articles but `articlesCount: 5`
  - `offset=1` skips the first article
  - `author=username` filters to only that author's articles
  - `tag=tagname` filters to articles with that tag
  - `favorited=username` filters to articles favorited by that user
  - `favorited` field in each article reflects the authenticated user's state
  - `author.following` reflects the authenticated user's follow state
  - Non-existent author returns `articlesCount: 0`
  - Non-existent tag returns `articlesCount: 0`
  - Non-existent favorited user returns `articlesCount: 0`

### PUT `/api/articles/{slug}` — Update Article

> **Note:** This endpoint is part of the standard RealWorld spec but is NOT tested by the current Postman test suites. Implement if the project scope requires it.

- **Auth:** Required (Bearer token, must be article author)
- **Path Parameters:** `slug` (string, required)
- **Request Body:** (partial update)
  ```json
  {
    "article": {
      "title": "Updated title",
      "description": "Updated description",
      "body": "Updated body"
    }
  }
  ```
- **Success Response:** `200 OK` with full article object
- **Error Responses:**
  - `401 Unauthorized` — missing/invalid token
  - `403 Forbidden` — not the article author
  - `404 Not Found` — article doesn't exist

### DELETE `/api/articles/{slug}` — Delete Article

> **Note:** This endpoint is part of the standard RealWorld spec but is NOT tested by the current Postman test suites. Implement if the project scope requires it.

- **Auth:** Required (Bearer token, must be article author)
- **Path Parameters:** `slug` (string, required)
- **Success Response:** `204 No Content`
- **Error Responses:**
  - `401 Unauthorized` — missing/invalid token
  - `403 Forbidden` — not the article author
  - `404 Not Found` — article doesn't exist

---

## Endpoints — Comments

### POST `/api/articles/{slug}/comments` — Create Comment

- **Auth:** Required (Bearer token)
- **Path Parameters:** `slug` (string, required)
- **Request Body:**
  ```json
  {
    "comment": {
      "body": "Is that so?"
    }
  }
  ```
- **Success Response:** `201 Created`
  ```json
  {
    "comment": {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "body": "Is that so?",
      "createdAt": "2026-03-27T12:00:00.000Z",
      "updatedAt": "2026-03-27T12:00:00.000Z",
      "author": {
        "username": "commenter@example.com",
        "bio": "I work at statefarm",
        "image": null,
        "following": false
      }
    }
  }
  ```
- **Error Responses:**
  - `400 Bad Request` — missing/blank body: error for `"body"`
  - `401 Unauthorized` — missing/invalid token
  - `404 Not Found` — article doesn't exist
- **Test Assertions:**
  - `comment.id` exists (GUID format)
  - `comment.createdAt` is ISO 8601, after request timestamp
  - `comment.author.following` reflects the authenticated user's follow state

### GET `/api/articles/{slug}/comments` — Get Comments

- **Auth:** Required (Bearer token)
- **Path Parameters:** `slug` (string, required)
- **Success Response:** `200 OK`
  ```json
  {
    "comments": [
      {
        "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        "body": "Is that so?",
        "createdAt": "2026-03-27T12:00:00.000Z",
        "updatedAt": "2026-03-27T12:00:00.000Z",
        "author": {
          "username": "commenter@example.com",
          "bio": "I work at statefarm",
          "image": null,
          "following": false
        }
      }
    ]
  }
  ```
- **Error Responses:**
  - `401 Unauthorized` — missing/invalid token
  - `422 Unprocessable Entity` — article doesn't exist
- **Test Assertions:**
  - Comments are returned in chronological order (oldest first)
  - Deleted comments are not returned
  - Empty article returns `comments: []`
  - `author.following` reflects the authenticated user's follow state

### DELETE `/api/articles/{slug}/comments/{id}` — Delete Comment

- **Auth:** Required (Bearer token, must be comment author)
- **Path Parameters:**
  - `slug` (string, required)
  - `id` (GUID, required)
- **Success Response:** `204 No Content`
- **Error Responses:**
  - `400 Bad Request` — invalid comment ID format (e.g., `"abc"`): error for `"id"`
  - `401 Unauthorized` — missing/invalid token
  - `403 Forbidden` — not the comment author
  - `404 Not Found` — comment doesn't exist (valid GUID but not found)
  - `422 Unprocessable Entity` — article doesn't exist
- **Test Assertions:**
  - Only the comment author can delete (other users get 403)
  - Deleting one comment leaves others intact
  - Invalid GUID format returns 400 with error for `"id"`
  - Non-existent comment (valid GUID `00000000-0000-0000-0000-000000000000`) returns 404
  - Non-existent article returns 422

---

## Endpoints — Favorites

### POST `/api/articles/{slug}/favorite` — Favorite Article

- **Auth:** Required (Bearer token)
- **Path Parameters:** `slug` (string, required)
- **Request Body:** `{}` (empty object)
- **Success Response:** `200 OK`
  ```json
  {
    "article": {
      "slug": "how-to-train-your-dragon",
      "title": "How to train your dragon",
      "description": "Ever wonder how?",
      "body": "Very carefully.",
      "tagList": ["training", "dragons"],
      "createdAt": "2026-03-27T12:00:00.000Z",
      "updatedAt": "2026-03-27T12:00:00.000Z",
      "favorited": true,
      "favoritesCount": 1,
      "author": {
        "username": "user@example.com",
        "bio": "I work at statefarm",
        "image": null,
        "following": false
      }
    }
  }
  ```
- **Error Responses:**
  - `401 Unauthorized` — missing/invalid token
  - `404 Not Found` — article doesn't exist
- **Business Rules:**
  - Idempotent — favoriting an already-favorited article returns the same state (count doesn't increase)
  - Multiple users can favorite the same article (count increments per unique user)
- **Test Assertions:**
  - After favoriting: `favorited: true`, `favoritesCount` increments
  - Favoriting twice by same user keeps `favoritesCount` the same
  - Returns full article object (includes `body`)

### DELETE `/api/articles/{slug}/favorite` — Unfavorite Article

- **Auth:** Required (Bearer token)
- **Path Parameters:** `slug` (string, required)
- **Success Response:** `200 OK` (full article object with updated `favorited` and `favoritesCount`)
- **Error Responses:**
  - `401 Unauthorized` — missing/invalid token
  - `404 Not Found` — article doesn't exist
- **Test Assertions:**
  - After unfavoriting: `favorited: false`, `favoritesCount` decrements
  - Unfavoriting an article you haven't favorited returns `favorited: false`, `favoritesCount: 0`

---

## Endpoints — Feed

### GET `/api/articles/feed` — Get Feed

- **Auth:** Required (Bearer token)
- **Query Parameters:**
  - `limit` (integer, optional, default: 20, minimum: 1) — page size
  - `offset` (integer, optional, default: 0, minimum: 0) — skip count
- **Success Response:** `200 OK`
  ```json
  {
    "articles": [
      {
        "slug": "article-by-followed-user",
        "title": "...",
        "description": "...",
        "tagList": ["..."],
        "createdAt": "...",
        "updatedAt": "...",
        "favorited": false,
        "favoritesCount": 0,
        "author": {
          "username": "followed@example.com",
          "bio": "I work at statefarm",
          "image": null,
          "following": true
        }
      }
    ],
    "articlesCount": 3
  }
  ```
- **Error Responses:**
  - `400 Bad Request` — invalid pagination (`offset < 0`, `limit < 1`): errors for `"limit"`, `"offset"`
  - `401 Unauthorized` — missing/invalid token
- **Business Rules:**
  - Returns articles ONLY from users the authenticated user follows
  - Does NOT include the authenticated user's own articles
  - Does NOT include articles from unfollowed users
- **Test Assertions:**
  - `body` field is NOT included in feed article items
  - Articles ordered most recent first
  - `articlesCount` is TOTAL feed count (not page count)
  - `author.following` is `true` for all feed articles
  - `limit=2` returns at most 2 articles
  - `offset=1` skips the first article
  - `offset` beyond feed length returns empty `articles: []` with correct `articlesCount`
  - Empty feed (no followed users have articles) returns `{ articles: [], articlesCount: 0 }`

---

## Endpoints — Tags

### GET `/api/tags` — Get All Tags

- **Auth:** Required (Bearer token)
- **Success Response:** `200 OK`
  ```json
  {
    "tags": ["training", "dragons"]
  }
  ```
- **Test Assertions:**
  - Returns all tags from all articles in the current tenant
  - Tags are strings
  - When no articles exist, returns `tags: []` (empty array)

---

## Data Models

### User (response object)

```typescript
interface UserResponse {
  email: string;       // User's email
  username: string;    // Defaults to email on registration
  bio: string;         // Default: "I work at statefarm"
  image: string | null; // Default: null
  roles: string[];     // e.g., ["ADMIN", "USER"] or ["USER"]
}

// Envelope: { "user": UserResponse }
```

### Profile (response object)

```typescript
interface ProfileResponse {
  username: string;
  bio: string;
  image: string | null;
  following: boolean;  // Whether the authenticated user follows this profile
}

// Envelope: { "profile": ProfileResponse }
```

### Article (single response)

```typescript
interface ArticleResponse {
  slug: string;          // Auto-generated from title (kebab-case)
  title: string;
  description: string;
  body: string;          // Only in single article responses, NOT in lists
  tagList: string[];     // Ordered array, may be empty
  createdAt: string;     // ISO 8601 datetime
  updatedAt: string;     // ISO 8601 datetime
  favorited: boolean;    // Whether the authenticated user favorited this
  favoritesCount: number; // Integer, total favorites across all users
  author: ProfileResponse;
}

// Single: { "article": ArticleResponse }
// List:   { "articles": ArticleResponse[], "articlesCount": number }
// In list responses, `body` field is EXCLUDED from each article
```

### Comment (response object)

```typescript
interface CommentResponse {
  id: string;          // GUID format (e.g., "a1b2c3d4-e5f6-7890-abcd-ef1234567890")
  body: string;
  createdAt: string;   // ISO 8601 datetime
  updatedAt: string;   // ISO 8601 datetime
  author: ProfileResponse;
}

// Single: { "comment": CommentResponse }
// List:   { "comments": CommentResponse[] }
```

### ApplicationUser (database entity)

```csharp
public class ApplicationUser : IdentityUser<Guid>
{
  // Field constraints
  public const int EmailMaxLength = 255;
  public const int UsernameMinLength = 2;
  public const int UsernameMaxLength = 100;
  public const int PasswordMinLength = 6;
  public const int BioMaxLength = 1000;
  public const int ImageUrlMaxLength = 500;

  public string Bio { get; set; } = "I work at statefarm";
  public string? Image { get; set; }
}
```

### Request DTOs

```typescript
// Registration / Invite
interface RegisterRequest {
  email: string;    // Required
  password: string; // Required
}

// Login
interface LoginRequest {
  email: string;    // Required
  password: string; // Required
}

// Update User
interface UpdateUserRequest {
  user: {
    email?: string;    // Optional, must be valid if provided
    username?: string; // Optional, min 2, max 100 chars
    password?: string; // Optional, min 6 chars
    bio?: string;      // Optional, max 1000 chars
    image?: string;    // Optional, max 500 chars
  }
}

// Create Article
interface CreateArticleRequest {
  article: {
    title: string;       // Required
    description: string; // Required
    body: string;        // Required
    tagList?: string[];  // Optional, each tag: non-empty, no commas
  }
}

// Create Comment
interface CreateCommentRequest {
  comment: {
    body: string;  // Required
  }
}
```

---

## Business Rules

### User Defaults
- Username is set to the email address on registration
- Default bio: `"I work at statefarm"`
- Default image: `null`

### Multi-Tenancy
- First registered user creates a new tenant and gets ADMIN + USER roles
- Subsequent users are invited by ADMIN users into the same tenant
- All data is isolated per tenant (query filters via Finbuckle)

### Slug Generation Rules
- Convert title to lowercase
- Remove all characters except: letters, digits, whitespace, hyphens, underscores
- Regex: `[^a-z0-9\s\-_]`
- Replace whitespace sequences with a single hyphen
- Trim leading/trailing hyphens
- Duplicate titles that produce the same slug are REJECTED (400 error for field `"slug"`)
- Examples:
  - `"How to train your dragon"` → `"how-to-train-your-dragon"`
  - `"How to train your dragon_2"` → `"how-to-train-your-dragon_2"`
  - `"Côte d'Azur!"` → `"cte-dazur"`

### Tag Rules
- Tags are stored as ordered arrays
- Tag order from creation input is preserved
- Individual tags must be non-empty strings
- Individual tags must NOT contain commas
- Articles without `tagList` get an empty array `[]`

### Article Ordering
- Articles are always ordered by creation time, most recent first (descending)

### Pagination
- Default `limit`: 20
- Minimum `limit`: 1
- Minimum `offset`: 0
- Non-numeric `limit`/`offset` values are rejected (400)
- `articlesCount` always reflects the TOTAL matching count, regardless of pagination

### Feed Rules
- Feed returns articles ONLY from users the authenticated user follows
- Feed excludes the authenticated user's own articles
- Feed excludes articles from unfollowed users
- Empty feed returns `{ articles: [], articlesCount: 0 }`

### Following Rules
- Follow is idempotent (following someone already followed returns 200 OK)
- Unfollow is NOT idempotent (unfollowing someone not followed returns 400)
- Following state is reflected in profile responses and article author objects

### Favorite Rules
- Favorite is idempotent (favoriting twice doesn't increase count)
- Unfavoriting something not favorited returns the article with `favorited: false`
- `favoritesCount` reflects the total number of unique users who favorited

### Comment Rules
- Only the comment author can delete their comment (403 for others)
- Comment IDs are GUIDs
- Deleting one comment doesn't affect other comments on the same article

### Authorization Rules
| Resource | Who Can Modify/Delete |
|----------|-----------------------|
| Comment | Only the comment author |
| Article | Only the article author (for update/delete, if implemented) |
| User Profile | Only the authenticated user (themselves) |
| Follows | Any authenticated user |
| Favorites | Any authenticated user |

---

## Implementation Status

### Already Built (in the starter template)

These endpoints exist in the server code and do NOT need to be implemented:

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

These endpoints are tested by Postman but do NOT exist yet:

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

### Needs Database Entities

The following entities need to be created (they don't exist yet):

- **Article**: slug, title, description, body, createdAt, updatedAt, authorId (FK to ApplicationUser)
- **Tag**: name (unique), many-to-many with Article
- **Comment**: id (GUID), body, createdAt, updatedAt, articleId (FK), authorId (FK)
- **ArticleFavorite**: userId (FK), articleId (FK) — join table
- **UserFollowing**: followerId (FK), followedId (FK) — join table (needed for follow/unfollow and feed)

### Existing Profile Endpoint Needs Enhancement

The current `GET /api/profiles/{username}` endpoint returns `ProfileDto` which does NOT include a `following` field. The Postman tests expect `following` in the profile response. This endpoint needs to be updated to:
1. Accept the current user context
2. Query the UserFollowing table
3. Include `following: boolean` in the response

---

## Postman Test Configuration

### Collections

| Collection File | Tests |
|-----------------|-------|
| `Conduit.Auth.postman_collection.json` | Registration, login, get user, update user |
| `Conduit.Article.postman_collection.json` | Article CRUD, tags, favorites, comments |
| `Conduit.ArticlesEmpty.postman_collection.json` | Empty state: articles, tags |
| `Conduit.Profiles.postman_collection.json` | Profile get, follow, unfollow |
| `Conduit.FeedAndArticles.postman_collection.json` | Feed, article listing with filters |

### Environment Variables

| Variable | Description |
|----------|-------------|
| `APIURL` | Base API URL (e.g., `http://localhost:5000/api`) |
| `EMAIL` | Primary test user email |
| `PASSWORD` | Test user password |

### Docker Compose Files

Each collection has a corresponding `docker-compose.{Name}.yml` in `Test/Postman/`:
- `docker-compose.Auth.yml`
- `docker-compose.Article.yml`
- `docker-compose.ArticlesEmpty.yml`
- `docker-compose.Profiles.yml`
- `docker-compose.FeedAndArticles.yml`

### Test Execution Order

Tests within each collection run sequentially (setup creates users and tokens, then tests use them). Collections are independent of each other — each registers its own users.

### Test Isolation Pattern

Each collection:
1. Registers fresh users with random email prefixes
2. Logs in to get bearer tokens
3. Stores tokens in Postman globals
4. Runs tests using those tokens
5. Uses `x-correlation-id` headers for request tracing

---

## Endpoint Summary Table

| Method | Route | Auth | Status Codes | Postman Tested |
|--------|-------|------|--------------|----------------|
| POST | `/api/identity/register` | No | 204, 400 | Yes |
| POST | `/api/identity/invite` | Bearer+ADMIN | 204, 400, 401 | Yes |
| POST | `/api/identity/login?useCookies=false` | No | 200, 401 | Yes |
| POST | `/api/identity/logout` | No | 200 | No |
| GET | `/api/user` | Bearer | 200, 401 | Yes |
| PUT | `/api/user` | Bearer | 200, 400, 401 | Yes |
| GET | `/api/profiles/{username}` | Bearer | 200, 401, 404 | Yes |
| POST | `/api/profiles/{username}/follow` | Bearer | 200, 401, 404 | Yes |
| DELETE | `/api/profiles/{username}/follow` | Bearer | 200, 400, 401 | Yes |
| POST | `/api/articles` | Bearer | 201, 400, 401 | Yes |
| GET | `/api/articles/{slug}` | Bearer | 200, 401, 404 | Yes |
| GET | `/api/articles` | Bearer | 200, 400, 401 | Yes |
| GET | `/api/articles/feed` | Bearer | 200, 400, 401 | Yes |
| PUT | `/api/articles/{slug}` | Bearer | 200, 401, 403, 404 | No |
| DELETE | `/api/articles/{slug}` | Bearer | 204, 401, 403, 404 | No |
| POST | `/api/articles/{slug}/favorite` | Bearer | 200, 401, 404 | Yes |
| DELETE | `/api/articles/{slug}/favorite` | Bearer | 200, 401, 404 | Yes |
| POST | `/api/articles/{slug}/comments` | Bearer | 201, 400, 401, 404 | Yes |
| GET | `/api/articles/{slug}/comments` | Bearer | 200, 401, 422 | Yes |
| DELETE | `/api/articles/{slug}/comments/{id}` | Bearer | 204, 400, 401, 403, 404, 422 | Yes |
| GET | `/api/tags` | Bearer | 200 | Yes |
| GET | `/api/users` | Bearer | 200, 401 | No |
