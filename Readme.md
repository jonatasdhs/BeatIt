# BeatIt - Game Backlog Management API

## **Overview**
BeatIt is a .NET-based API designed to help users manage their gaming backlog and completed games. Users can:
- Add games to their backlog.
- Mark games as completed.
- View games from their backlog and completed lists.
- Fetch game data from the IGDB API.

This project is built using ASP.NET Core and integrates external data from IGDB to enrich the user's game database.

---

## **Features**
### 1. **User Management**
- Register new users.
- Update user details.
- Authenticate users with secure credentials.

### 2. **Game Management**
- Fetch game details from the IGDB API.
- Add games to the local database if not already present.

### 3. **Backlog Management**
- Add games to the user's backlog.
- Remove games from the backlog.
- Retrieve all games in the backlog.

### 4. **Completed Games**
- Mark games as completed.
- Retrieve all completed games for a user.

---

## **Technologies Used**
- **Framework**: ASP.NET Core
- **Database**: SQL Server (via Entity Framework Core)
- **External API**: IGDB API
- **Caching**: Redis
- **Dependency Injection**: Built-in ASP.NET Core DI
- **HTTP Client**: Used for IGDB API interactions

---

## **Setup and Installation**

### **Prerequisites**
- .NET SDK 7.0 or later
- SQL Server
- Redis
- IGDB Client ID and Secret
- A compatible IDE (e.g., Visual Studio, JetBrains Rider, or Visual Studio Code)

### **Installation Steps**
1. Clone the repository:
   ```bash
   git clone https://github.com/jonatasdhs/BeatIt.git
   cd BeatIt
   ```

2. Set up the database:
   - Update the `ConnectionStrings` section in `appsettings.json` with your SQL Server connection details.
   - Run migrations to create the database schema:
     ```bash
     dotnet ef database update
     ```

3. Configure Redis:
   - Ensure Redis is running and accessible.
   - Update the Redis connection string in `appsettings.json`.

4. Set up the IGDB API:
   - Add your `IGDB_CLIENT_ID` and `IGDB_SECRET_KEY` as environment variables.

5. Run the application:
   ```bash
   dotnet run
   ```

6. Access the API at:
   - Local: `http://localhost:5000`
   - Swagger UI: `http://localhost:5000/swagger`

---

## **API Endpoints**

### **Authentication**
- `POST /api/auth/register`
  - Register a new user.
  - **Body**:
    ```json
    {
      "name": "John Doe",
      "email": "johndoe@example.com",
      "password": "yourpassword"
    }
    ```

- `POST /api/auth/login`
  - Authenticate a user.
  - **Body**:
    ```json
    {
      "email": "johndoe@example.com",
      "password": "yourpassword"
    }
    ```

### **Backlog**
- `GET /api/backlog`
  - Retrieve all games in the user's backlog.

- `POST /api/backlog`
  - Add a game to the backlog.
  - **Body**:
    ```json
    {
      "gameId": 123
    }
    ```

- `DELETE /api/backlog/{gameId}`
  - Remove a game from the backlog.

### **Completed Games**
- `GET /api/completed`
  - Retrieve all completed games.

- `POST /api/completed`
  - Mark a game as completed.
  - **Body**:
    ```json
    {
      "gameId": 123
    }
    ```

### **Games**
- `GET /api/games`
  - Retrieve all games in the database.

- `GET /api/games/{id}`
  - Retrieve a game by ID.

---

## **Environment Variables**
The application requires the following environment variables:
- `ConnectionStrings__DefaultConnection`: Database connection string.
- `Redis__ConnectionString`: Redis connection string.
- `IGDB_CLIENT_ID`: IGDB API client ID.
- `IGDB_SECRET_KEY`: IGDB API secret key.

---

## **Project Structure**

```
BeatIt
├── Controllers      // API Controllers
├── Models           // Entity models and DTOs
├── Repositories     // Data access logic
├── Services         // Business logic and external API integrations
├── DataContext      // Entity Framework DbContext
├── Migrations       // Database migrations
├── Program.cs       // Entry point
└── appsettings.json // Configuration file
```

---

## **Future Improvements**
- Implement unit and integration tests to improve reliability.
- Add pagination for game retrieval endpoints.
- Enhance error handling with a global exception handler.
- Improve caching for high-demand data from IGDB.
- Add user-specific preferences and settings.

---

## **Contact**
Maintained by [Jônatas Daniel Hora Silveira](https://github.com/jonatasdhs). Feel free to open an issue or submit a pull request if you'd like to contribute!

