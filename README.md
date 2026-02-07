# InnoClinic

## Internship Project

## Tech Stack
- **Backend:** .NET 10, ASP.NET Core
- **Databases:** MS SQL Server (EF Core), MongoDB
- **Logging:** Serilog
- **DevOps:** Docker, Docker Compose

## How to Run
1. Clone the repository.
2. Create a `.env` file in the root directory (use `.env.example` as a template).
3. Open a terminal in the root directory.
4. Run the command:
   ```bash
   docker-compose up -d --build

## API Testing
Postman collections are located in the `/postman` folder. 
1. Import collections into Postman.
2. Create an environment with `base_url_auth` (localhost:8080) and `base_url_offices` (localhost:8082).
3. Run the **Login** request first to populate access tokens.

## Services
- **Auth API:** http://localhost:8080/swagger
- **Offices API:** http://localhost:8082/swagger
- **Mongo UI:** http://localhost:8081
```

---
*Copyright (c) 2026 Alexandr Mikhalkov*