# Solution notes

## How to run

```bash
cp .env.example .env
docker compose up -d --build
```

Open http://localhost:4200 and drop a CSV from `samples/`.

See [README.md](README.md) for the full command table and local (non-Compose) workflow.

## Design decisions

- **Producer → queue → consumer**: the API parses CSV and publishes one message per valid row; only the Worker inserts into PostgreSQL.
- **Light layering**: Domain / Application / Infrastructure / Api / Worker with inward dependencies. A simple `Book` entity and ports — no value-object ceremony, MediatR, or domain events — kept the take-home focused on the messaging split.
- **EF Core migrations** create the `library` table on startup (with retry). `db/init.sql` remains as an optional reference.
- **Angular + Tailwind + signals** for the portal; poll after upload because persistence is asynchronous.
- **xUnit** for domain and application tests with fakes at the ports.

## Assumptions

- CSV has a header row (`name,author,genre`); incomplete rows are skipped; no de-duplication.
- Small files only; browser `application/octet-stream` uploads are accepted when the filename ends with `.csv`.
- Local Compose uses demo credentials from `.env.example`.

## With more time

- Idempotent consumers / dead-letter queue
- Upload progress and import job status API
- AuthN for anything beyond local demo
- Stronger integration tests against Compose
