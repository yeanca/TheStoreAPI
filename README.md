# TheStore API

TheStore API is a production-grade **C# Web API** for managing a fashion store. It provides endpoints for handling products, orders, and users, and comes with a pre-configured database for testing.

---

## 🚀 Setup Instructions

### 1. Clone the Repository
```bash
git clone https://github.com/yeanca/TheStoreAPI.git
cd thestore-api
```

### 2. Restore NuGet Packages
- Open the solution in **Visual Studio**.  
- Restore all required NuGet packages.

### 3. Set Up the Database
- Open **SQL Server Management Studio (SSMS)**.  
- Run the provided `TheStoreDB.sql` script file.  
- This will create and populate a database named **TheStoreDB** with all necessary tables and dummy data.

### 4. Configure the Connection String
- Open `appsettings.json` in the main project.  
- Update the `DefaultConnection` string to point to **TheStoreDB** local database.

### 5. Run the Project
- Start the project in Visual Studio.  
- The API will launch and automatically open **Swagger UI** in your browser.
- You can use Postman to test the API.

---

## ✅ Assumptions

Before running the project, ensure you have:

- **.NET SDK**: .NET 8 SDK (or a later LTS version) installed.  
- **SQL Server**: Access to a local instance of Microsoft SQL Server.  
- **API Client**: Postman or the built-in Swagger UI for testing endpoints.

---

## 🛠 Tech Stack

- **Language:** C#  
- **Framework:** .NET 8 with ASP.NET Core  
- **Database:** Microsoft SQL Server  
- **ORM:** Entity Framework Core (EF Core)  
- **Authentication:** JWT (for anonymous user identification)

---

## 📐 Architecture & Features

- **Clean Architecture**:  
  Uses a folder structure (`Infrastructure`, `Controllers`) to separate concerns and keep business logic independent of data and API layers.  

- **Scalability**:  
  Database schema is designed to prevent overselling with concurrent requests (using transactions).  
  Flexible design allows adding new product types easily with tables for **colors, materials, and attributes**.  

- **Robustness**:  
  Includes structured logging and extensive error handling to provide clear developer and user messages when issues occur.  

---

## 📖 API Documentation

Once the project is running, the API documentation will be available via **Swagger UI** at:  

```
https://localhost:44360/
```

---

## 🤝 Contributing

1. Fork the repository  
2. Create a new branch (`feature/your-feature`)  
3. Commit your changes  
4. Push the branch  
5. Open a Pull Request  

