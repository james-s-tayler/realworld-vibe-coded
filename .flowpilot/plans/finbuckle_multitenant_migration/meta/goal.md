## Goal

You are updating an existing **ASP.NET Core monolith** that uses:

- **ASP.NET Identity** for authentication
- **Cookie auth** for the SPA
- **Bearer tokens** for the API
- **EF Core** for data access

Currently, multiple users can register accounts independently and **all users can see all data**.

We want to transition this system towards **single-database, organization-based multi-tenancy** using **Finbuckle.Multitenant**.
The nature of the platform is shifting to an internal company social network.

---

## Domain & Terminology

- **Tenant = Organization**
- Each **Organization** has one or more **ApplicationUsers**.
- Each **ApplicationUser belongs to exactly one Organization**.
- The **Organization Owner** is an ApplicationUser with a special `Owner` role for that Organization.

---

## Functional Requirements

Implement multi-tenancy with these rules:

1. The **tenant is an Organization**.
2. When a new user self-registers, they are:
    - Creating a **new Organization**.
    - Becoming the **Owner** of that Organization.
3. The **Owner** can add additional users for their Organization via an **admin screen**.
4. Users can **only see and manipulate data belonging to their own Organization**.
5. A user can **only belong to one Organization**.
6. Only users in the `Owner` role for their organization can:
    - Access the **admin screen**.
    - Manage users in their Organization.

---

## Multi-Tenancy Architecture Requirements

Implement multi-tenancy as follows:

1. **Single Database**
    - All tenants share the **same physical database**.
    - Multi-tenancy is logical, enforced at the application layer.

2. **Tenant Column / Filters**
    - All domain entities that should be tenant-scoped must have a `TenantId` column.
    - EF Core should apply **global query filters** so queries are automatically filtered by the current tenant.

3. **Finbuckle.Multitenant**
    - Use **Finbuckle.Multitenant** for tenant resolution and per-tenant EF behavior.
    - Use **`ClaimStrategy`** as the primary tenant resolver.
    - The tenant identifier claim should be : `TenantId`.

4. **Auth Integration**
    - When a user signs in (cookie or issuing bearer tokens):
        - Their identity must include a claim with the **TenantId of their Organization**.
    - The `ClaimStrategy` must read this claim and set the current tenant.
    - All subsequent EF queries must be scoped to that tenant via Finbuckle + query filters.

---

## Out of Scope

Do **not** implement the following:

- Users belonging to multiple Organizations.
- Mechanism to switch Organizations.
- Per-tenant connection strings or separate databases.
- Per-tenant IdPs / authentication schemes.
- Tenant selection via route, subdomain, or header (we only use ClaimStrategy for now).

Do **not** worry about needing to migrate or maintain compatibility with existing data. This is a pre-production application with no users.

---