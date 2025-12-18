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
- [ ] Register Articles User - setup
- [ ] Register Articles User 2 - setup
- [ ] Login and Remember Token - setup
- [ ] Login and Remember Token 2 - setup
- [ ] Follow Profile
- [ ] Create Article
- [ ] Create Article without tags
- [ ] Create Article - validation - missing required fields
- [ ] Create Article - validation - required fields empty
- [ ] Create Article - validation - invalid tag (comma)
- [ ] Create Article - validation - invalid tag (empty)
- [ ] Create Article - validation - duplicate slug
- [ ] Create Article - error - unauthenticated
- [ ] Get Tags
- [ ] Get Article
- [ ] Get Article - following author
- [ ] Get Article - unauthenticated
- [ ] Get Article - error - not found
- [ ] Favorite Article
- [ ] Favorite Article - already favorited
- [ ] Favorite Article - error - unauthenticated
- [ ] Favorite Article - error - not found
- [ ] Unfavorite Article
- [ ] Unfavorite Article - not already favorited
- [ ] Unfavorite Article - error - unauthenticated
- [ ] Unfavorite Article - error - not found
- [ ] All Articles
- [ ] All Articles - limit 2
- [ ] All Articles - offset 2
- [ ] Articles by Author
- [ ] Articles by Author - limit 2
- [ ] Articles by Author - not found
- [ ] Articles Favorited by Username
- [ ] Articles Favorited by Username - limit 2
- [ ] Articles Favorited by Username - not found
- [ ] Articles by Tag
- [ ] Articles by Tag - limit 2
- [ ] Articles by Tag - not found
- [ ] Update Article
- [ ] Update Article - validation - provided fields blank
- [ ] Update Article - validation - duplicate slug
- [ ] Update Article - error - unauthenticated
- [ ] Update Article - error - not found
- [ ] Update Article - error - not author
- [ ] Delete Article
- [ ] Delete Article - error - unauthenticated
- [ ] Delete Article - error - not found
- [ ] Delete Article - error - not author
- [ ] Create Comment
- [ ] Create Comment - validation - missing required fields
- [ ] Create Comment - validation - required fields blank
- [ ] Create Comment - error - unauthenticated
- [ ] Get Comments
- [ ] Get Comments - unauthenticated
- [ ] Delete Comment
- [ ] Delete Comment - error - unauthenticated
- [ ] Delete Comment - error - not found
- [ ] Delete Comment - error - not author

---

## Conduit.ArticlesEmpty.postman_collection.json (5 requests)

### ArticlesEmpty Folder
- [ ] All Articles
- [ ] Articles by Author
- [ ] Articles Favorited by Username
- [ ] Articles by Tag
- [ ] Get Tags

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
- [ ] Register Feed User - setup
- [ ] Register Feed User 2 - setup
- [ ] Register Feed User 3 - setup
- [ ] Register Feed User 4 - setup
- [ ] Login and Remember Token - setup
- [ ] Login and Remember Token 2 - setup
- [ ] Login and Remember Token 3 - setup
- [ ] Login and Remember Token 4 - setup
- [ ] Follow Profile - user 2
- [ ] Follow Profile - user 3
- [ ] Create Article - user 1
- [ ] Create Article - user 2
- [ ] Create Article - user 3
- [ ] Create Article - user 2_1
- [ ] Create Article - user 4
- [ ] Favorite Article
- [ ] Favorite Article 2
- [ ] Favorite Article 3
- [ ] Get Feed
- [ ] Get Feed - limit 2
- [ ] Get Feed - offset 1
- [ ] Get Feed - error - unauthenticated
- [ ] All Articles
- [ ] All Articles - limit 1
- [ ] All Articles - offset 1
- [ ] All Articles - limit 1 offset 2
- [ ] All Articles - filter by author
- [ ] All Articles - filter by author - limit 1
- [ ] All Articles - filter by favorited
- [ ] All Articles - filter by favorited - limit 1
- [ ] All Articles - filter by tag
- [ ] All Articles - filter by tag - limit 1
- [ ] All Articles - filter by author and tag
- [ ] All Articles - filter by author and favorited
- [ ] All Articles - filter by tag and favorited
- [ ] All Articles - filter by author, tag and favorited
- [ ] All Articles - unauthenticated
- [ ] All Articles - filter by author - unauthenticated
- [ ] All Articles - filter by favorited - unauthenticated
- [ ] All Articles - filter by tag - unauthenticated
- [ ] All Articles - filter by author and tag - unauthenticated
- [ ] All Articles - filter by author and favorited - unauthenticated

---

## Conduit.Profiles.postman_collection.json (17 requests)

### Profiles Folder
- [ ] Register
- [ ] Login and Remember Token
- [ ] Register (second user)
- [ ] Register Celeb
- [ ] Follow Profile - error - unauthenticated
- [ ] Follow Profile
- [ ] Follow Profile - already following
- [ ] Follow Profile - validation - user doesn't exist
- [ ] Profile - authenticated - following
- [ ] Profile - authenticated - not following
- [ ] Profile - error - 404 not found
- [ ] Profile - validation - invalid username
- [ ] Profile - unauthenticated
- [ ] Unfollow Profile - error - unauthenticated
- [ ] Unfollow Profile
- [ ] Unfollow Profile - error - not already following
- [ ] Profile - authenticated - unfollowed

---

## Progress Summary
- **Total:** 18/138 completed (13%)
- **Article:** 0/56 completed
- **ArticlesEmpty:** 0/5 completed
- **Auth:** 18/18 completed ✅
- **FeedAndArticles:** 0/42 completed
- **Profiles:** 0/17 completed

