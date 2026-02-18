# InnoClinic

### Internship Project

## 🛠 Tech Stack
- **Backend:** .NET 10, ASP.NET Core
- **Databases:** 
  - **MS SQL Server / Azure SQL:** Auth API, Profiles API (Entity Framework Core)
  - **MongoDB:** Offices API
- **Architecture:** Clean Architecture, Microservices
- **Logging:** Serilog
- **DevOps:** Docker, Docker Compose

## 🚀 How to Run
1. Clone the repository.
2. Create a `.env` file in the root directory (use `.env.example` as a template).
3. Open a terminal in the root directory.
4. Run the command:
   ```bash
   docker-compose up -d --build
   ```
> **Note**: Migrations for SQL services (Auth, Profiles) will be applied automatically on startup.*
   
## 🧪 API Testing (Postman)
1. **Import**: Import the JSON collection files from the `/postman` folder into Postman.
2. **Configure & Run**:
   - Select the desired collection and click **Run collection**.
   - In the **Runner** tab, configure your execution settings (select specific requests, adjust the order, or set delays).
   - Click the **Run [Collection Name]** button to start the automated flow.
3. **Automation Logic**:
   - All essential data, such as access tokens and newly created resource IDs, are automatically captured and stored in **Collection Variables** during execution.
   - Subsequent requests (Read, Update, Delete) automatically retrieve these values, ensuring a seamless testing process.

> **Note**: If you prefer to execute requests individually, ensure that authentication and resource creation steps are performed first to populate the necessary collection variables.


## 🔗 Service Map
| Service | URL | Database |
| :--- | :--- | :--- |
| **Auth API** | [http://localhost:8080/swagger](http://localhost:8080/swagger/index.html) | MS SQL |
| **Offices API** | [http://localhost:8082/swagger](http://localhost:8082/swagger/index.html) | MongoDB |
| **Profiles API** | [http://localhost:8083/swagger](http://localhost:8083/swagger/index.html) | MS SQL |
| **Mongo UI** | [http://localhost:8081](http://localhost:8081) | - |

---
*Copyright (c) 2026 Alexandr Mikhalkov*