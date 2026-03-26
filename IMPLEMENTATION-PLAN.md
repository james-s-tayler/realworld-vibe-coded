# Implementation Plan — RealWorld Conduit API

> **Read this at the start of every session.** Follow the story order exactly.
> Each story is designed to fit in a single context window and builds only on completed dependencies.
> See `SPEC-REFERENCE.md` for detailed endpoint specs, request/response shapes, and validation rules.

---

## Feature Dependency DAG

```
Tags (standalone)          Auth (already built)
    |                          |
    |                    +-----+----------+
    |                    |     |          |
    |                    v     v          v
    |              Profiles  User    Follow/Unfollow
    |              (built)  (built)       |
    |                                     |
    |         +---------------------------+
    |         |                           |
    |         v                           |
    |    Articles CRUD <------------------+
    |         |                           |
    |         |                           |
    +---------+                           |
    |         |                           |
    |    +----+-----+-----------+         |
    |    |          |           |         |
    |    v          v           v         |
    |  Comments  Favorites  Article List  |
    |                  |       |          |
    |                  |       |          |
    |                  v       v          v
    |               Feed (articles from followed users)
    |
    v
  Tags endpoint (returns tags from articles)
```

### Legend
- **Already built**: Auth (register, invite, login, logout), Get User, Update User, Get Profile
- **Needs building**: Everything else below

### Dependency Summary

| Feature | Depends On |
|---------|-----------|
| Database Entities (Article, Tag, Comment, Favorites, Following) | Auth (existing) |
| Follow / Unfollow | Auth, DB Entities (UserFollowing) |
| Profile Enhancement (`following` field) | Follow/Unfollow |
| Articles CRUD (create, get single) | Auth, DB Entities (Article, Tag) |
| Tags endpoint | Articles (reads tags from existing articles) |
| Articles Empty State | Tags endpoint, Articles List |
| Article List + Filters | Articles CRUD |
| Comments | Articles CRUD |
| Favorites | Articles CRUD |
| Feed | Follow/Unfollow, Articles CRUD, Article List |
| Full Integration (FeedAndArticles) | Feed, Article List, Favorites, Follow |

---

## Story Order

### Story 1: Database Entities & Migrations (S)

**What:** Create all missing EF Core entities and the migration.

**Entities to create:**
- `Article` — slug, title, description, body, createdAt, updatedAt, authorId (FK -> ApplicationUser)
- `Tag` — name (unique), many-to-many with Article (join table `ArticleTag`)
- `Comment` — id (GUID), body, createdAt, updatedAt, articleId (FK -> Article), authorId (FK -> ApplicationUser)
- `ArticleFavorite` — userId (FK), articleId (FK), composite PK
- `UserFollowing` — followerId (FK), followedId (FK), composite PK

**Location:** `App/Server/src/Server.Core/` for entities, `App/Server/src/Server.Infrastructure/` for EF config

**Tests:** `./build.sh BuildServer` (compiles without errors)

**Done when:** Solution builds, migration applies cleanly, database schema includes all new tables.

---

### Story 2: Follow / Unfollow + Profile Enhancement (M)

**What:** Implement follow/unfollow endpoints and update the existing Profile GET to include the `following` field.

**Endpoints:**
- `POST /api/profiles/{username}/follow` — follow a user
- `DELETE /api/profiles/{username}/follow` — unfollow a user
- `GET /api/profiles/{username}` — enhance to return `following: boolean`

**Depends on:** Story 1 (UserFollowing entity)

**Key business rules:**
- Follow is idempotent (re-following returns 200 OK)
- Unfollow is NOT idempotent (unfollowing when not following returns 400)
- `following` field must appear in all profile responses and article `author` objects

**Tests:** `./build.sh TestServerPostmanProfiles`

**Done when:** All Postman Profiles collection tests pass.

---

### Story 3: Create Article + Get Single Article + Tags (M)

**What:** Implement article creation, single article retrieval, and the tags endpoint.

**Endpoints:**
- `POST /api/articles` — create article (201 Created)
- `GET /api/articles/{slug}` — get single article (200 OK)
- `GET /api/tags` — list all tags (200 OK)

**Depends on:** Story 1 (Article, Tag entities), Story 2 (profile `following` field in author response)

**Key business rules:**
- Slug auto-generated from title (kebab-case), duplicate slugs rejected (400)
- Tags: each must be non-empty, no commas; order preserved
- `favoritesCount` must be integer, `createdAt`/`updatedAt` ISO 8601
- `author` object includes `following` field
- Tags endpoint returns all tags across all articles in current tenant

**Tests:** None pass in isolation yet — these are prerequisites for Stories 4-7. Validate with `./build.sh BuildServer` (compiles) and manual smoke test.

**Done when:** Create article returns 201 with correct shape, get article returns 200, tags returns tag list.

---

### Story 4: Articles Empty State (S)

**What:** Ensure articles list and tags return correct empty-state responses.

**Endpoints:**
- `GET /api/articles` — returns `{ articles: [], articlesCount: 0 }` when no articles exist
- `GET /api/tags` — returns `{ tags: [] }` when no articles exist

**Depends on:** Story 3 (articles list + tags endpoints exist)

**Tests:** `./build.sh TestServerPostmanArticlesEmpty`

**Done when:** All Postman ArticlesEmpty collection tests pass.

---

### Story 5: Favorites (S)

**What:** Implement favorite and unfavorite endpoints.

**Endpoints:**
- `POST /api/articles/{slug}/favorite` — favorite an article
- `DELETE /api/articles/{slug}/favorite` — unfavorite an article

**Depends on:** Story 3 (Article entity, get single article)

**Key business rules:**
- Favorite is idempotent (favoriting twice doesn't increase count)
- Unfavoriting something not favorited returns article with `favorited: false`, `favoritesCount: 0`
- Returns full article object (includes `body`)
- `favoritesCount` reflects unique users who favorited

**Tests:** Validated as part of `./build.sh TestServerPostmanArticle` (Story 7).

**Done when:** Favorite/unfavorite returns correct article state with updated counts.

---

### Story 6: Comments (M)

**What:** Implement comment CRUD endpoints.

**Endpoints:**
- `POST /api/articles/{slug}/comments` — create comment (201 Created)
- `GET /api/articles/{slug}/comments` — list comments (200 OK)
- `DELETE /api/articles/{slug}/comments/{id}` — delete comment (204 No Content)

**Depends on:** Story 3 (Article entity exists, get single article works)

**Key business rules:**
- Comment IDs are GUIDs
- Only comment author can delete (403 for others)
- Comments ordered chronologically (oldest first)
- Non-existent article -> 404 for create, 422 for list/delete
- Invalid GUID format -> 400, valid GUID not found -> 404

**Tests:** Validated as part of `./build.sh TestServerPostmanArticle` (Story 7).

**Done when:** Create/list/delete comments work with correct auth checks and error responses.

---

### Story 7: Article Collection Integration (L)

**What:** Implement article list with filters, then run the full Article Postman collection which validates articles, tags, favorites, and comments together.

**Endpoints:**
- `GET /api/articles` — list articles with `author`, `tag`, `favorited`, `limit`, `offset` filters

**Depends on:** Stories 3, 5, 6 (articles, favorites, comments all implemented)

**Key business rules:**
- `body` field NOT included in list responses
- Articles ordered most recent first
- `articlesCount` is TOTAL count (not page size)
- Filter by `author`, `tag`, `favorited` username
- Pagination: `limit` (default 20, min 1), `offset` (default 0, min 0)
- Non-existent filter values return `articlesCount: 0`

**Tests:** `./build.sh TestServerPostmanArticle`

**Done when:** All Postman Article collection tests pass (articles, tags, favorites, comments).

---

### Story 8: Auth Collection Validation (S)

**What:** Run the Auth Postman collection to confirm all existing auth/user endpoints still work correctly after adding new entities and migrations.

**Endpoints (already built):**
- `POST /api/identity/register`
- `POST /api/identity/invite`
- `POST /api/identity/login?useCookies=false`
- `GET /api/user`
- `PUT /api/user`

**Depends on:** Story 1 (DB migration doesn't break existing endpoints)

**Note:** This can be run early (after Story 1) as a regression check, but is listed here to confirm no regressions after all features are built.

**Tests:** `./build.sh TestServerPostmanAuth`

**Done when:** All Postman Auth collection tests pass.

---

### Story 9: Feed + Article Listing Integration (L)

**What:** Implement the personalized feed endpoint, then run the FeedAndArticles collection which tests feed, article listing with filters, and following state together.

**Endpoints:**
- `GET /api/articles/feed` — personalized feed (articles from followed users only)

**Depends on:** Stories 2, 7 (follow/unfollow + full article list)

**Key business rules:**
- Returns articles ONLY from users the authenticated user follows
- Excludes the authenticated user's own articles
- Excludes articles from unfollowed users
- `body` NOT included in feed items
- `author.following` is `true` for all feed articles
- Pagination: `limit` (default 20, min 1), `offset` (default 0, min 0)
- Empty feed -> `{ articles: [], articlesCount: 0 }`

**Tests:** `./build.sh TestServerPostmanFeedAndArticles`

**Done when:** All Postman FeedAndArticles collection tests pass.

---

## Execution Summary

| Order | Story | Size | Test Command | Key Deliverable |
|-------|-------|------|-------------|-----------------|
| 1 | DB Entities & Migrations | S | `./build.sh BuildServer` | All entity classes + migration |
| 2 | Follow/Unfollow + Profile | M | `./build.sh TestServerPostmanProfiles` | Follow/unfollow endpoints + `following` field |
| 3 | Create Article + Get + Tags | M | `./build.sh BuildServer` | Article CRUD core + tags endpoint |
| 4 | Articles Empty State | S | `./build.sh TestServerPostmanArticlesEmpty` | Empty list + empty tags responses |
| 5 | Favorites | S | `./build.sh BuildServer` | Favorite/unfavorite endpoints |
| 6 | Comments | M | `./build.sh BuildServer` | Comment create/list/delete |
| 7 | Article Collection | L | `./build.sh TestServerPostmanArticle` | Article list + filters + full integration |
| 8 | Auth Regression | S | `./build.sh TestServerPostmanAuth` | Confirm no regressions |
| 9 | Feed + Integration | L | `./build.sh TestServerPostmanFeedAndArticles` | Feed endpoint + final integration |

### Full Test Pass (all green = done)

```bash
./build.sh TestServerPostmanAuth
./build.sh TestServerPostmanProfiles
./build.sh TestServerPostmanArticlesEmpty
./build.sh TestServerPostmanArticle
./build.sh TestServerPostmanFeedAndArticles
```

---

## Notes for the Agent

1. **Follow this order strictly.** Do not skip ahead — each story's tests depend on earlier stories being complete.
2. **Run the specified test command after each story** to validate before moving on.
3. **Read SPEC-REFERENCE.md** for detailed endpoint specs, request/response shapes, and validation rules.
4. **Existing code patterns:** Look at `App/Server/src/Server.Web/Identity/` and `App/Server/src/Server.Web/Users/` for the established FastEndpoints + MediatR + FluentValidation patterns.
5. **Database entities go in** `App/Server/src/Server.Core/`, EF configurations in `App/Server/src/Server.Infrastructure/`.
6. **Never run `dotnet` directly** — always use `./build.sh <target>`.
7. **Size guide:** S = ~30 min (1 context window), M = ~60 min (1 context window), L = ~90 min (may need 1-2 context windows).
8. **When a test fails**, read the error output carefully — Postman test failures include the specific assertion that failed and the actual vs expected values.
9. **Context overflow?** If running low on context, finish the current story, commit, and start a new session. The next session picks up from where you left off by reading this plan.
