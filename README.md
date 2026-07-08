# Learning Management System (LMS)

This repository contains the source code for a comprehensive Learning Management System. The project is split into a frontend application (built with Angular) and a backend API (built with .NET 10).

---

## 🛠 Prerequisites

Before setting up the project, ensure you have the following installed on your machine:

### Backend Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) (v14 or higher recommended)
- [Redis](https://redis.io/download) (Required for distributed caching)
- Entity Framework Core CLI (optional but recommended for migrations): 
  `dotnet tool install --global dotnet-ef`

### Frontend Prerequisites
- [Node.js](https://nodejs.org/) (v18.x or higher)
- npm (v11+)
- [Angular CLI](https://angular.io/cli) (v21.2+)
  `npm install -g @angular/cli@21`

---

## 🚀 Setup & Installation

### 1. Backend Setup (LMSApi)

The backend is a .NET 10 Web API project using PostgreSQL and Entity Framework Core.

1. **Navigate to the API folder:**
   ```bash
   cd LMSApi/LMSApi.API
   ```

2. **Configure Database Connection:**
   Open `appsettings.Development.json` (or `appsettings.json`) and update the connection strings for PostgreSQL and Redis according to your local setup (refer `appsettings.json` for format):
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Database=LmsDb;Username=postgres;Password=your_password",
     "Redis": "localhost:6379"
   }
   ```

3. **Start Redis via Docker (Optional):**
   If you don't have Redis installed locally, you can easily spin it up using Docker:
   ```bash
   docker run --name lms-redis -p 6379:6379 -d redis
   ```

4. **Apply Database Migrations:**
   This will create the necessary tables and PostgreSQL functions used by the application.
   ```bash
   dotnet ef migrations add migrationname --project LMSApi.DALLibrary --startup-project LMSApi.API --context LMSApi.DALLibrary.Contexts.LMSDbContext
   dotnet ef database update --project LMSApi.DALLibrary --startup-project LMSApi.API --context LMSApi.DALLibrary.Contexts.LMSDbContext
   ```
   *(Note: If you have manual SQL scripts like `docs/routines.sql`, execute them against your PostgreSQL database.)*

5. **Run the API:**
   ```bash
   dotnet run --project LMSApi.API
   ```
   The backend API will start running (typically on `https://localhost:5029` or similar).

---

### 2. Frontend Setup (lms-app)

The frontend is an Angular 21 Single Page Application (SPA).

1. **Navigate to the frontend folder:**
   ```bash
   cd lms-app
   ```

2. **Install Dependencies:**
   ```bash
   npm install
   ```

3. **Configure API Endpoint (Optional):**
   By default, the Angular app expects the API to run on the configured environment URL. Update `src/environments/environment.ts` if your backend is running on a different port.

4. **Run the Development Server:**
   ```bash
   npm start
   ```
   or
   ```bash
   ng serve
   ```
   The application will be accessible at `http://localhost:4200/`.

---

## 📚 Project Structure

- **`LMSApi/`**: Backend API solution (.NET 10, C#).
- **`lms-app/`**: Frontend web application (Angular 21, TypeScript, TailwindCSS).
- **`docs/`**: Documentation and raw SQL scripts (`routines.sql`).

---

## 📦 External Libraries (Backend)

The .NET 10 backend (`LMSApi`) utilizes several external libraries (NuGet packages) for various features:

- **AutoMapper**: Simplifies object-to-object mapping (e.g., mapping entity models to DTOs).
- **CloudinaryDotNet**: Integration with Cloudinary for managing and serving user uploads like images and videos.
- **Hangfire**: Provides background job processing (used for sending emails, processing long-running tasks, etc.) with PostgreSQL storage support (`Hangfire.PostgreSql`).
- **StackExchange.Redis**: Facilitates distributed caching to improve application performance and reduce database load.
- **PdfSharpCore**: Used for generating PDF files programmatically (e.g., generating certificates).
- **Razorpay**: Payment gateway integration to handle course purchases and transactions.
- **SixLabors.ImageSharp / SixLabors.Fonts**: Cross-platform libraries for image manipulation and drawing (useful for dynamic graphic generation or image resizing).
- **ClosedXML**: Simplifies creating, reading, and modifying Excel spreadsheets for data export features.
- **System.IdentityModel.Tokens.Jwt / Microsoft.AspNetCore.Authentication.JwtBearer**: Handles generation and validation of JSON Web Tokens (JWT) for secure authentication and authorization.
- **Entity Framework Core / Npgsql**: Provides Object-Relational Mapping (ORM) capabilities and PostgreSQL database provider support (`Npgsql.EntityFrameworkCore.PostgreSQL`).
- **Asp.Versioning.Mvc**: Manages API versioning.
- **Serilog**: Enables structured logging to the console and files.
- **Swashbuckle.AspNetCore**: Automatically generates Swagger UI and OpenAPI specifications for interactive API documentation.

---

## 📦 External Libraries (Frontend)

The Angular 21 frontend (`lms-app`) relies on the following key external npm packages:

- **@microsoft/signalr**: Used for enabling real-time web functionality (e.g., live notifications or real-time course updates).
- **marked**: A markdown parser and compiler used for rendering rich text content, such as course descriptions or lesson notes.
- **pdfjs-dist**: A web standards-based platform for parsing and rendering PDFs directly in the browser (ideal for displaying course materials or certificates).
- **rxjs**: Reactive Extensions Library for JavaScript, heavily used within Angular for handling asynchronous operations and data streams.
- **tailwindcss / @tailwindcss/postcss**: A utility-first CSS framework for rapidly building custom, responsive UI designs without leaving the HTML.
- **vitest / jsdom**: A fast, modern testing framework and headless browser environment used for running unit tests.
- **prettier**: An opinionated code formatter to maintain consistent code style across the frontend project.
