# Engineering charter (reusable)

Copy this file into any repo as `docs/engineering-charter.md`. It is **not** always-on agent context — each repo keeps a short root [`AGENTS.md`](../AGENTS.md) that links here.

Deliver production-quality software from the **first commit**. Do not prototype first and refactor to DDD later. Every feature must respect domain-driven design, strict separation of concerns, and user-centered responsive design across all breakpoints (when the project has a UI).

These rules apply regardless of language or framework. Examples below are shown in Python and C#; apply the same principles to any other stack.

## Contents

- [Architecture (strict dependency rule)](#architecture-strict-dependency-rule)
- [DDD rules (from commit 1)](#ddd-rules-from-commit-1)
- [Backend standards (any language)](#backend-standards-any-language)
- [Architecture and project docs](#architecture-and-project-docs)
- [DevOps best practices](#devops-best-practices)
- [User-centered design](#user-centered-design)
- [Responsive design (first UI commit onward)](#responsive-design-first-ui-commit-onward)
- [Task workflow](#task-workflow)
- [Definition of done](#definition-of-done)

---

## Architecture (strict dependency rule)

Dependencies flow **inward only**:

```
Presentation → Application → Domain
Infrastructure → Application → Domain
```

| Layer              | Responsibility                                                                   |
| ------------------ | -------------------------------------------------------------------------------- |
| **Domain**         | Business rules, entities, value objects, domain services. No I/O, no frameworks. |
| **Application**    | Use cases, orchestration, ports, dependency wiring (composition root).           |
| **Infrastructure** | Database, external APIs, file I/O, third-party SDK adapters.                     |
| **Presentation**   | CLI, API, web/desktop UI. Thin only.                                             |

### Typical layout

```
src/
  domain/           # Pure business logic
  application/      # Use cases + ports + composition root
  infrastructure/   # External adapters
  interfaces/       # CLI, API, UI entry points
  config.py / appsettings.*
```

### Right-size the structure (important)

Use layered boundaries **in code**, not as empty folder theater.

- **Do create** a layer/folder when it has real types, rules, or adapters to put there.
- **Do not create** empty `domain/`, `application/`, `infrastructure/`, or `interfaces/` trees just to match a template.
- **Do not add** empty `__init__.py`, placeholder services, or stub ports "for later."
- **Start simple** for scripts, CRUD apps, or one-file tools: a few modules with clear names may be enough.
- **Grow into layers** when complexity appears: multiple integrations, non-trivial business rules, more than one entry point, or testing pain from mixed concerns.
- **Remove dead paths** after refactors — no duplicate legacy folders sitting next to their replacement.

| Project size                         | Suggested shape                                                  |
| ------------------------------------ | ---------------------------------------------------------------- |
| Simple script / single feature       | `main.py` + `config.py` + 1–2 focused modules                    |
| Small app (1 integration, few rules) | `domain.py`, `services.py`, `adapters.py`, `cli.py`              |
| Multi-integration app                | Full `domain/`, `application/`, `infrastructure/`, `interfaces/` |

Per-repo agent briefs (`AGENTS.md`) stay short and project-specific; keep this charter as the shared deep standards doc.

### Stack equivalents

| Concept          | Python                | C# (.NET)                                |
| ---------------- | --------------------- | ---------------------------------------- |
| Domain           | `src/domain/`         | `MyApp.Domain`                           |
| Application      | `src/application/`    | `MyApp.Application`                      |
| Infrastructure   | `src/infrastructure/` | `MyApp.Infrastructure`                   |
| Presentation     | `src/interfaces/`     | `MyApp.Api` / `MyApp.Web`                |
| Composition root | `bootstrap.py`        | `Program.cs` / `DependencyInjection.cs`  |
| Ports            | `Protocol` / ABC      | `interface`                              |
| Use cases        | `*Service`, handlers  | MediatR commands/queries or app services |

---

## DDD rules (from commit 1)

1. **Ubiquitous language** — name things after the domain (e.g. `Application`, `SyncInbox`), not `DataManager` or `Helper`.
2. **Bounded contexts** — separate modules per context; no god-packages.
3. **Entities** — identity and lifecycle.
4. **Value objects** — immutable, equality by value (e.g. `Money`, `EmailAddress`, `Status`).
5. **Aggregates** — one root; invariants enforced in domain, not SQL/UI.
6. **Domain services** — cross-entity logic that doesn't belong to one entity.
7. **Application services** — one operation per method; orchestrate only.
8. **Repositories (ports)** — persistence only; no business decisions in SQL/ORM.
9. **Anti-corruption layers** — wrap every external API/SDK dependency.
10. **DTOs at boundaries** — map external/DB shapes to domain at the edge.
11. **Domain events** — prefer events for side effects when use cases grow.

### Forbidden

- Business logic in controllers, CLI commands, UI pages, repositories, or ORM entities
- `Utils` / `Helpers` / `Misc` dumping grounds
- Presentation importing Infrastructure directly
- Leaking DB rows or raw JSON into UI

---

## Backend standards (any language)

- **Errors**: domain exceptions → application results → stable HTTP/CLI codes
- **Logging**: structured logs at use-case boundaries; support a verbose/debug flag
- **Config**: typed settings; no secrets in source control
- **Tests**: see "DevOps best practices → Test Driven Development" below
- **README**: document every runnable entry point near the top (see below). README is the **only** command list for the repo — do not maintain a second copy in `context.md` or the ARD.
- **ARD**: architecture source of truth when the app is deployable (see [Architecture and project docs](#architecture-and-project-docs)).

### README — commands first (required for apps with scripts)

When the project exposes CLI commands, npm/pnpm scripts, `make` targets, `dotnet` tools, or similar, the **README must list them in the first section after the title and one-line description** — before install guides, architecture, or long setup prose.

Include:

1. **How to run** — activate venv, `npx`, `dotnet run`, Docker, etc. when non-obvious
2. **Command table** — every script/command, what it does, and important flags
3. **Typical workflow** — 2–4 commands for the happy path
4. **Details later** — move deep usage, architecture, and troubleshooting below

| Stack      | Examples to document                               |
| ---------- | -------------------------------------------------- |
| Python CLI | `myapp auth`, `myapp sync --full`, `myapp -v sync` |
| Node       | `npm run dev`, `npm test`, `npm run build`         |
| .NET       | `dotnet run --project src/Api`, `dotnet test`      |
| Make       | `make up`, `make test`, `make migrate`             |

Update this section whenever a new command or script is added. If the app has no scripts (library-only), skip the table and state that clearly.

Keep `README.md` in sync with the actual CLI/entry-point code — treat a stale command list as a bug. Prefer linking *to* the README from other docs rather than repeating the table.

### C# guidance

- Projects: `*.Domain`, `*.Application`, `*.Infrastructure`, `*.Api`
- MediatR or explicit command/query handlers
- EF Core only in Infrastructure; map entities ↔ domain explicitly
- Thin controllers; no EF entities in API responses

### Python guidance

- `Protocol` ports, Pydantic/dataclasses for DTOs
- Wire everything in `application/bootstrap.py`
- Keep `interfaces/` free of business rules

---

## Architecture and project docs

This charter is the **only** reusable doc to copy between repos. Per-repo files (`AGENTS.md`, README, ARD, runbooks, etc.) are created by agents for that project when they have real content — do not copy them as a pack.

Use layered docs the same way you right-size code layers: create them when they have real content. Per-repo ownership lives in that repo’s root `AGENTS.md` — if a fact appears in two places, delete one copy and leave a link.

### ARD (Architecture Response Document)

For every deployable app (including small ones with hosting or external APIs):

1. Keep these **section headers** even when a section is empty: Context (Scope / Out of Scope), Proposed Approach, Individual Components Roles and Responsibilities, Deployment, Dependencies, Data Flows/APIs, Security Concerns, COGS.
2. Describe **current production**, not a future backend, unless clearly labeled Out of Scope / target.
3. **Diagrams are visual image assets**, not ASCII art or text-only box drawings. Ship real figures beside the ARD (SVG preferred; PNG/JPEG exports of C4 / sequence / deployment diagrams are fine). A short text topology may supplement an image — it must not replace one. Minimum useful set: C4 Context, C4 Containers, Deployment, plus a sequence diagram for each non-trivial API or async flow.
4. **How to view diagrams:** open the ARD with the default VS Code Markdown preview (**Ctrl+Shift+V**). Cursor’s inline Markdown toggle does **not** render SVG/images — do not treat a blank preview there as missing diagrams.
5. **Security Concerns** must describe **current production** (not a wish list):
   - **CIA** — short table or three bullets: what Confidentiality, Integrity, and Availability mean for *this* system and how they are achieved
   - **AuthN / AuthZ** — what exists, or explicit “none / public read-only”
   - **Secrets** — where they live, or “none in the app / none in git”
   - **Transport** — TLS posture (and any in-transit exceptions)
   - **Top risks for this surface** — only threats that apply (OWASP-class or equivalent). Mark true N/A with a one-line reason. Do **not** paste a full Top 10 / framework table unless at least three items meaningfully apply
   - **Threat view** — for each important flow, note assets, what can go wrong, and mitigations (diagram optional). Use a full STRIDE table only when the surface warrants it
   - **Fitness checks** — prefer automated gates (tests, CI, dependency/security scans, metric thresholds) over one-off review theater; document the ones that actually run
   - Right-size: do not invent login, secrets managers, or compliance programs the product does not have
6. When architecture, deployment, external dependencies, **or security/trust boundaries** change, update the ARD (including diagram images and Security Concerns) in the same change as the code.

Skip a full ARD only for true throwaway scripts with no deployable surface.

---

## DevOps best practices

### Test Driven Development

Tests are written **before** the code that satisfies them, not after. No new production code without a failing test first.

**Red → Green → Clean(Refactor) cycle:**

| Stage     | State                                                                                             |
| --------- | ------------------------------------------------------------------------------------------------- |
| **Red**   | Test is written and fails (code doesn't exist yet, or doesn't satisfy it).                        |
| **Green** | Minimum code written to make the test pass. No more.                                              |
| **Clean** | Refactor the code (and the test, if needed) with confidence — the test suite catches regressions. |

Rules:

- Tests are exhaustive — write a test for every scenario you can reasonably think of, including edge cases and failure paths, not just the happy path.
- No cheating: a build/PR is only "done" when every test passes. Don't skip, comment out, or weaken a failing test to make it pass.
- Map test types to the architecture layers already defined above:
  - **Domain** — pure unit tests, no mocks needed (no I/O to fake).
  - **Application** — unit tests with mocked ports (fake repositories/adapters).
  - **Infrastructure** — integration tests against a real (or realistic, e.g. containerized) dependency.
- Regressions should almost always come with a new test reproducing the bug, added before the fix.

Why: higher code and design quality (TDD naturally pushes toward loose coupling and small units, since tightly coupled code is hard to test in isolation), earlier and cheaper defect detection, and fewer regressions over time.

### Continuous Integration

A CI pipeline (build + test, minimum) must exist from the **first commit** — not bolted on later once the project "grows." Even a single-stage pipeline that runs tests on every push is better than none.

Minimum pipeline stages for a brand-new project:

| Stage                  | Purpose                                      |
| ---------------------- | -------------------------------------------- |
| Install/restore        | Fetch dependencies                           |
| Lint / static analysis | Catch style & type errors early              |
| Build                  | Confirm the project compiles/builds          |
| Test                   | Run the automated test suite (see TDD above) |

Add stages as the project grows (security scanning, containerized integration tests, deploy).

Rules:

- Pipeline runs automatically on every push/PR.
- Build must stay green. If it breaks, fixing it is priority 0 — don't build on top of a red build.
- Keep the pipeline fast (rule of thumb: under 5 minutes). Slow pipelines get skipped or ignored.
- No cheating: don't merge with skipped, commented-out, or weakened failing tests just to force a green build.
- Static code analysis is a required pipeline stage, not optional tooling someone runs manually sometimes.
- Trunk/main stays production-ready at all times — code merged there should always be deployable, whether or not it's actually deployed yet.
- Tag releases (git tags / semantic versioning) so any deployed version traces back to an exact commit.

### Trunk Based Development

- All work happens on a single shared branch (`main`/`trunk`). Feature branches, if used, are short-lived (hours, not days) and merged frequently — no long-lived branches.
- Small, frequent changes — merge often rather than batching large diffs.
- Every merge to trunk runs through CI automatically (see above).
- This is what CI actually depends on: a pipeline is only meaningful if there's something to integrate against continuously — branches that live for weeks defeat the point.

Avoids the classic long-lived-branch failure modes: slow release cycles, painful merges, stale code review, reduced collaboration, and code quality decay from long divergence.

---

## User-centered design

Design for **jobs to be done** — identify the 2-5 core tasks the user actually needs, and design every view around one of them.

### UX requirements

- One clear primary action per view
- Empty, loading, and error states for every async flow
- WCAG 2.1 AA: semantic markup, keyboard nav, labels, contrast ≥ 4.5:1
- 44×44px minimum tap targets on mobile/tablet
- Human-readable labels (`Interview scheduled`, not `interview_scheduled`)
- Show data freshness where relevant (`last synced`)

---

## Responsive design (first UI commit onward)

**Mobile-first.** Enhance with `min-width` breakpoints.

| Token     | Min width | Target                    |
| --------- | --------- | ------------------------- |
| mobile    | default   | phones 320–639px          |
| tablet    | 640px     | tablets                   |
| desktop   | 1024px    | laptops                   |
| ultrawide | 1536px    | large/ultra-wide monitors |

### Layout

- **Mobile**: single column, stacked cards, full-width CTAs
- **Tablet**: 2 columns; collapsible filters; tables → cards or priority-column scroll
- **Desktop**: sidebar + main; full data tables
- **Ultra-wide**: centered max-width (~1400–1600px); side panels, not stretched text

### Requirements

- Design tokens for spacing, type, color, breakpoints
- Test at 375px, 768px, 1280px, 1920px, 2560px
- No accidental horizontal scroll on mobile
- Respect `prefers-reduced-motion` and `prefers-color-scheme`

Streamlit (and similarly limited frameworks) is weak for responsive UX; prefer a real frontend (React, Blazor, Razor + modern CSS) when mobile/tablet experience matters.

---

## Task workflow

1. Assess scope — use the smallest structure that fits; don't scaffold unused layers
2. Identify bounded context and ubiquitous language
3. Model domain (entities, value objects, rules)
4. Define ports and application use case only when a boundary is needed
5. Implement infrastructure adapter
6. Wire in composition root
7. Add thin presentation layer
8. Build UI mobile → tablet → desktop → ultrawide (when UI exists)
9. Add tests, logging, docs — **README commands section at top** when the app has scripts/CLI; update **ARD** (including Security Concerns) when architecture, deployment, or trust boundaries change (no duplicate command lists elsewhere)
10. Delete obsolete folders/files after moves or refactors

### Existing repos (refactor mode)

This charter applies to **greenfield and existing repos alike**. Applying it to an existing codebase is a refactor, not a rewrite — treat it with extra care:

1. **Characterize before restructuring.** Get tests around current behavior first (even coarse-grained ones) so the migration is verifiably behavior-preserving, not a leap of faith. This is what the TDD/CI sections above are for — use them as the refactor's safety net.
2. **Decide structural conventions before touching Infrastructure/Presentation code.** Specifically: does this project need Event-Driven Architecture (affects where domain events/ports live), what's the resilience-wrapping convention for external adapters (circuit breaker/retry/timeout), what's the observability/logging shape, and what's the auth/authz baseline. Deciding these upfront avoids touching the same adapters/endpoints/use cases twice.
3. **Migrate incrementally under a green pipeline.** Move one bounded context / module at a time; keep the build and tests passing between steps rather than a big-bang rewrite.
4. **Delete the old shape as you go** (see "Right-size the structure" above) — no legacy folder living next to its replacement once a piece is migrated.

---

## Definition of done

- [ ] CI pipeline exists and is green (build + lint + tests)
- [ ] Domain rules tested
- [ ] Use case behind a port; adapter if needed
- [ ] Presentation layer is thin
- [ ] UI works at all four breakpoints (if UI exists)
- [ ] Empty/loading/error states present
- [ ] Structured logging added
- [ ] README updated if architecture, usage, or **commands/scripts** changed (commands table at top when the app has entry points) — single command list only
- [ ] ARD updated if architecture, deployment, external dependencies, or **security/trust boundaries** changed (when an ARD exists) — Security Concerns still matches the real threat surface
- [ ] No secrets committed
- [ ] No duplicated docs (commands only in README; architecture only in ARD)
