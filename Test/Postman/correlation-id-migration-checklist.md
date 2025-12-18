# Correlation ID Manual Migration Checklist

## Overview
This checklist tracks the manual addition of correlation ID support to each request in all Postman collections.

Each request needs:
1. Pre-request script to generate correlation ID: `pm.collectionVariables.set('correlationId', pm.variables.replaceIn('{{$randomUUID}}'));`
2. Header added: `X-Correlation-Id` with value `{{correlationId}}`
3. Test script changes: Replace `pm.test(` with `pm.testWithCorrelation(` for all tests

**Total Requests:** 138

---

## Conduit.Article.postman_collection.json (56 requests)

### Article Folder
- [x] Register Articles User - setup
- [x] Register Articles User 2 - setup
- [x] Login and Remember Token - setup
- [x] Login and Remember Token 2 - setup
- [x] Follow Profile
- [x] Create Article
- [x] Create Article without tags
- [x] Create Article - validation - missing required fields
- [x] Create Article - validation - required fields empty
- [x] Create Article - validation - invalid tag (comma)
- [x] Create Article - validation - invalid tag (empty)
- [x] Create Article - validation - duplicate slug
- [x] Create Article - error - unauthenticated
- [x] Get Tags
- [x] Get Article
- [x] Get Article - following author
- [x] Get Article - unauthenticated
- [x] Get Article - error - not found
- [x] Favorite Article
- [x] Favorite Article - already favorited
- [x] Favorite Article - error - unauthenticated
- [x] Favorite Article - error - not found
- [x] Unfavorite Article
- [x] Unfavorite Article - not already favorited
- [x] Unfavorite Article - error - unauthenticated
- [x] Unfavorite Article - error - not found
- [x] All Articles
- [x] All Articles - limit 2
- [x] All Articles - offset 2
- [x] Articles by Author
- [x] Articles by Author - limit 2
- [x] Articles by Author - not found
- [x] Articles Favorited by Username
- [x] Articles Favorited by Username - limit 2
- [x] Articles Favorited by Username - not found
- [x] Articles by Tag
- [x] Articles by Tag - limit 2
- [x] Articles by Tag - not found
- [x] Update Article
- [x] Update Article - validation - provided fields blank
- [x] Update Article - validation - duplicate slug
- [x] Update Article - error - unauthenticated
- [x] Update Article - error - not found
- [x] Update Article - error - not author
- [x] Delete Article
- [x] Delete Article - error - unauthenticated
- [x] Delete Article - error - not found
- [x] Delete Article - error - not author
- [x] Create Comment
- [x] Create Comment - validation - missing required fields
- [x] Create Comment - validation - required fields blank
- [x] Create Comment - error - unauthenticated
- [x] Get Comments
- [x] Get Comments - unauthenticated
- [x] Delete Comment
- [x] Delete Comment - error - unauthenticated
- [x] Delete Comment - error - not found
- [x] Delete Comment - error - not author

---

## Conduit.ArticlesEmpty.postman_collection.json (5 requests)

### ArticlesEmpty Folder
- [x] All Articles
- [x] Articles by Author
- [x] Articles Favorited by Username
- [x] Articles by Tag
- [x] Get Tags

---

## Conduit.Auth.postman_collection.json (18 requests)

### Auth Folder
- [x] Register
- [x] Register (second user)
- [x] Register - validation - duplicate email
- [x] Register - validation - not a valid email
- [x] Register - validation - missing required fields
- [x] Register - validation - required fields blank
- [x] Login
- [x] Login Failed - error - user not found
- [x] Login Failed - error - incorrect password
- [x] Login and Remember Token
- [x] Current User
- [x] Current User - error - unauthenticated, missing authorization header
- [x] Current User - error - unauthenticated, invalid token
- [x] Update User
- [x] Update User - validation - provided fields blank
- [x] Update User - validation - duplicate email
- [x] Update User - validation - duplicate username
- [x] Update User - error - unauthenticated

---

## Conduit.FeedAndArticles.postman_collection.json (42 requests)

### FeedAndArticles Folder
- [x] Register Feed User - setup
- [x] Register Feed User 2 - setup
- [x] Register Feed User 3 - setup
- [x] Register Feed User 4 - setup
- [x] Login and Remember Token - setup
- [x] Login and Remember Token 2 - setup
- [x] Login and Remember Token 3 - setup
- [x] Login and Remember Token 4 - setup
- [x] Follow Profile - user 2
- [x] Follow Profile - user 3
- [x] Create Article - user 1
- [x] Create Article - user 2
- [x] Create Article - user 3
- [x] Create Article - user 2_1
- [x] Create Article - user 4
- [x] Favorite Article
- [x] Favorite Article 2
- [x] Favorite Article 3
- [x] Get Feed
- [x] Get Feed - limit 2
- [x] Get Feed - offset 1
- [x] Get Feed - error - unauthenticated
- [x] All Articles
- [x] All Articles - limit 1
- [x] All Articles - offset 1
- [x] All Articles - limit 1 offset 2
- [x] All Articles - filter by author
- [x] All Articles - filter by author - limit 1
- [x] All Articles - filter by favorited
- [x] All Articles - filter by favorited - limit 1
- [x] All Articles - filter by tag
- [x] All Articles - filter by tag - limit 1
- [x] All Articles - filter by author and tag
- [x] All Articles - filter by author and favorited
- [x] All Articles - filter by tag and favorited
- [x] All Articles - filter by author, tag and favorited
- [x] All Articles - unauthenticated
- [x] All Articles - filter by author - unauthenticated
- [x] All Articles - filter by favorited - unauthenticated
- [x] All Articles - filter by tag - unauthenticated
- [x] All Articles - filter by author and tag - unauthenticated
- [x] All Articles - filter by author and favorited - unauthenticated

---

## Conduit.Profiles.postman_collection.json (17 requests)

### Profiles Folder
- [x] Register
- [x] Login and Remember Token
- [x] Register (second user)
- [x] Register Celeb
- [x] Follow Profile - error - unauthenticated
- [x] Follow Profile
- [x] Follow Profile - already following
- [x] Follow Profile - validation - user doesn't exist
- [x] Profile - authenticated - following
- [x] Profile - authenticated - not following
- [x] Profile - error - 404 not found
- [x] Profile - validation - invalid username
- [x] Profile - unauthenticated
- [x] Unfollow Profile - error - unauthenticated
- [x] Unfollow Profile
- [x] Unfollow Profile - error - not already following
- [x] Profile - authenticated - unfollowed

---

## Progress Summary
- **Total:** 138/138 completed (100%) ✅
- **Article:** 56/56 completed ✅
- **ArticlesEmpty:** 5/5 completed ✅
- **Auth:** 18/18 completed ✅
- **FeedAndArticles:** 42/42 completed ✅
- **Profiles:** 17/17 completed ✅

