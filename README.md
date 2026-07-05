# SoftUni MVC

An ASP.NET Core MVC application built on the SoftUni sample database. It provides full CRUD
(Create, Read, Update, Delete) management screens for towns, addresses, departments,
employees, projects, and employee-project assignments, using Entity Framework Core against
a MySQL database.

## Tech stack

- ASP.NET Core MVC (.NET 10)
- Entity Framework Core 9 (`Microsoft.EntityFrameworkCore.Design`, `.Tools`)
- Pomelo EF Core MySQL provider
- MySQL database (schema/data in `softuni.sql`)

## Project structure

```
soft-uni-mvc/
├── Controllers/          MVC controllers (one per entity: Addresses, Departments,
│                         Employees, EmployeeProjects, Projects, Towns, Home)
├── Models/                Entity classes (Address, Department, Employee,
│                         EmployeeProject, Project, Town, ErrorViewModel)
├── Data/
│   └── SoftUniContext.cs  EF Core DbContext and entity configuration
├── Views/                 Razor views, one folder per controller
│   └── Shared/            Shared layout, error page, validation partials
├── wwwroot/                Static assets (css, js, lib, favicon)
├── Properties/
│   └── launchSettings.json  Local run profiles (http/https)
├── softuni.sql             MySQL dump to create and seed the `softuni` database
├── appsettings.json         Configuration incl. the DB connection string
├── appsettings.Development.json
└── soft-uni-mvc.csproj
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A running MySQL server (locally or remote)

## Setup

1. **Create and seed the database**

   Import the provided SQL dump into your MySQL server:

   ```bash
   mysql -u root -p < softuni.sql
   ```

   This creates the `softuni` database and populates it with sample data.

2. **Configure the connection string**

   Create an `appsettings.Development.json` file in the project root and fill it in with your
   MySQL credentials (this file is git-ignored, so your credentials never get committed):

   ```json
   {
   "ConnectionStrings": {
      "SoftUniContext": "Server=localhost;Database=softuni;User=root;Password=yourpassword;"
   },
   "Logging": {
      "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
      }
   }
   }
   ```

3. **Restore dependencies**

   ```bash
   dotnet restore
   ```

## Running the project

```bash
dotnet run
```

By default the app listens on `http://localhost:5206` (and `https://localhost:7110` for the
`https` profile) — see `Properties/launchSettings.json`. Open the printed URL in your
browser; the app will launch there automatically in development.

## Development

- Build: `dotnet build`
- Run with the HTTPS profile: `dotnet run --launch-profile https`
