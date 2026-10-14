# Home Library — agent brief

Short, project-specific guidance. Deep standards live in [docs/engineering-charter.md](docs/engineering-charter.md). Architecture: [docs/ard.md](docs/ard.md). Runnable commands: [README.md](README.md).

## Stack

- .NET 10 — ASP.NET Core API + separate Worker
- PostgreSQL, RabbitMQ
- Angular + Tailwind (UI)
- Docker Compose for the full stack
- xUnit for tests

## Ubiquitous language

- **Book** — imported library entry (name, author, genre, import date)
- **Import** — CSV upload that queues book rows for persistence
- **BookImport** — queue message for one CSV row
- **Library** — the `library` table / catalog of imported books

## Boundaries

- API parses CSV and **publishes** to RabbitMQ; it does **not** insert books.
- Worker **consumes** messages and inserts books.
- Domain holds validation rules; Application orchestrates; Infrastructure adapts EF/RabbitMQ/CSV; presentation stays thin.

## Right-sized layering

Prefer a simple `Book` entity and clear ports over value-object ceremony, MediatR, or domain events. Do not add empty folders or unused abstractions.
