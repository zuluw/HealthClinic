# InnoClinic

### Internship Project

## 🛠 Tech Stack
- **Backend:** .NET 10, ASP.NET Core
- **Databases:** MS SQL Server (EF Core), MongoDB
- **Logging:** Serilog
- **DevOps:** Docker, Docker Compose

## 🚀 How to Run
1. Clone the repository.
2. Create a `.env` file in the root directory (use `.env.example` as a template).
3. Open a terminal in the root directory.
4. Run the command:
   ```bash
   docker-compose up -d --build```

## 🧪 API Testing (Postman)
The collections in the `/postman` folder are designed to work as a sequence. Follow these steps to initialize all required variables:

1. **Import**: Import the JSON collection files into Postman.
2. **Global Variables**: variables are pre-configured in the **Collection Variables**.
3. **Step 1: Authentication (Auth API)**
   - Run the **Login** request. 
   - This will automatically save data to the collection variables.
4. **Step 2: Initialize Data (Offices API)**
   - Run the **Get Offices** (or similar GET) request.
   - This will automatically save data to the collection variables.
5. **Ready to Test**: Now all other requests (Create, Update, Delete) will have the necessary IDs and Tokens pre-filled.

> **Tip**: Check the **Variables** tab of the collection to see the current values of `first_office_id`, `access_token`, etc.

> **Note**: If you changed the default ports in `docker-compose.yml`, update the corresponding variables in the **Variables** tab of the imported collection.

## Services
- **Auth API:** [http://localhost:8080/swagger](http://localhost:8080/swagger)
- **Offices API:** [http://localhost:8082/swagger](http://localhost:8082/swagger)
- **Mongo UI:** [http://localhost:8081](http://localhost:8081)

---
*Copyright (c) 2026 Alexandr Mikhalkov*