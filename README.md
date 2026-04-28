# TeamLinks Shortener

Microsserviço em ASP.NET Core: encurta URLs através da API TeamLinks e serve redirecionamentos em `/r/{codigo}`.

**Fluxo:** `POST /api/shorten` chama a TeamLinks (`POST api/links/project/{projectId}`), monta `shortUrl` com `TeamLinks:PublicBaseUrl` + `/r/{shortCode}`; em `GET /r/{code}`, consulta `GET api/links/ref/{code}` e responde com redirect HTTP.

## Pré-requisitos

- [Docker](https://docs.docker.com/get-docker/)
- API TeamLinks (Java/Spring + Postgres) acessível

## Docker

1. No repositório da API TeamLinks, suba o stack (Postgres + Spring).
2. Suba o container
  
### Variáveis de ambiente

Use o arquivo `.env` ou variáveis no ambiente ao iniciar o Compose:

| Variável | Descrição |
|----------|-----------|
| `TEAMLINKS_API_BASE_URL` | URL base da API TeamLinks (ex.: `http://host.docker.internal:8080`) |
| `TEAMLINKS_PROJECT_ID` | ID do projeto em que os links são criados |
| `SHORTENER_PUBLIC_BASE_URL` | URL pública deste serviço — monta o `shortUrl` (ex.: `http://localhost:5006`) |
| `SHORTENER_HOST_PORT` | Porta no host para o encurtador (padrão `5006`) |

## Endpoints

| Método | Caminho | Descrição |
|--------|---------|-----------|
| `GET` | `/` | Texto de ajuda |
| `POST` | `/api/shorten` | JSON `{ "url": "...", "name"?, "description"?, "tagNames"? }` → `{ shortUrl, shortCode, linkId }` |
| `GET` | `/r/{code}` | Redirect para o URL longo (o código é o `shortCode` devolvido pela TeamLinks) |
