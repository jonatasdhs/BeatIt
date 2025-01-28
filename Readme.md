# BeatIt - Game Backlog Management API

## **Introduction**
BeatIt is a .NET-based API designed to help users manage their gaming backlog and completed games. Users can:
- Add games to their backlog.
- Mark games as completed.
- View games from their backlog and completed lists.
- Fetch game data from the IGDB API.

This project is built using ASP.NET Core and integrates external data from IGDB to enrich the game database.

---

## **Features**
### 1. **User Management**
- Register new users.
- Update user details.
- Soft delete.
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
- **Container**: Docker
- **Testing**: xUnit, Moq

---

## **Setup and Installation**

### **Prerequisites**
- Docker and Docker Compose installed
- IGDB Client ID and Secret

### **Installation Steps**
1. Clone the repository:
   ```bash
   git clone https://github.com/jonatasdhs/BeatIt.git
   cd BeatIt
   ```

2. Copy the `.env.example` file in the root directory and rename it to `.env`. Then, add the following environment variables to the `.env` file:
   ```env
   CONNECTIONSTRINGS__DEFAULTCONNECTION=YourDatabaseConnectionString
   REDIS__CONNECTIONSTRING=redis:6379
   IGDB_CLIENT_ID=YourIGDBClientID
   IGDB_SECRET_KEY=YourIGDBSecretKey
   ```

3. Build and run the application using Docker Compose:
   ```bash
   docker-compose up --build
   ```

4. Access the API at:
   - Local: `http://localhost:5000`
   - Swagger UI: `http://localhost:5000/swagger`

---

## **Testing**
This project includes automated tests to ensure the correctness of critical funcionalities.

### **Testing Frameworks**
- **xUnit**: Main testing framework for .NET.
- **Moq**: Mocking framework for dependency testing.

### **Running Tests**
1. Build the Solution:
```bash
   dotnet build
```

2. Run the tests:
```bash
   dotnet test
```

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

- `POST /api/backlog/{gameId}`
  - Add a game to the backlog.

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
      "difficulty": 3,
      "rating": 8,
      "notes": "Ótimo jogo com uma narrativa envolvente.",
      "finishedDate": "2024-10-01T14:00:00",
      "timeToComplete": "20:45:00",
      "platform": "PC",
      "startDate": "2024-10-01T14:00:00"
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
- `RedisConfigurations__Endpoint`: Redis connection string.
- `IgdbConfiguration__CLIENT_ID`: IGDB API client ID.
- `IgdbConfiguration__SECRET_KEY`: IGDB API secret key.

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
└──