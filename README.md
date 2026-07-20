# 🎓 Learning Management System (LMS)

[![Demo Video](https://img.shields.io/badge/Watch-Demo_Video-blue?style=for-the-badge&logo=googledrive)](https://drive.google.com/file/d/1s8Wf-NXyzByP6uxCdwlBIShec7yB1fYT/view?usp=sharing) &nbsp;&nbsp;&nbsp; **Tech Stack:** [![Tech Stack](https://skillicons.dev/icons?i=angular,dotnet,postgres,docker,azure,python)](https://skillicons.dev)

This repository contains the source code for a comprehensive, enterprise-grade Learning Management System. The project is built with a modern tech stack featuring an Angular frontend, a robust .NET 10 backend API, and a Python microservice, integrating advanced AI capabilities for an enhanced learning experience.

---

## 📋 Table of Contents
1. [Overview](#-overview)
2. [Key Features](#-key-features)
3. [AI Capabilities](#-ai-capabilities)
4. [Technical Highlights](#️-technical-highlights)
5. [Deployment & Infrastructure](#️-deployment--infrastructure)
6. [Application Flow](#-application-flow)
7. [Prerequisites](#-prerequisites)
8. [Setup & Installation](#-setup--installation)
9. [Project Structure](#-project-structure)
10. [Environment Variables](#️-environment-variables)
11. [External Libraries](#-external-libraries)

---

## 🌟 Overview
- Supports **Learners, Instructors, and Administrators**
- Create and manage **Courses, Lessons, Quizzes, and Assignments**
- Supports **Free** and **Premium Courses**
- **Progress Tracking** and Course Completion Monitoring
- Automated **Certificate Generation**
- **Razorpay Integration** for secure course purchases

## ✨ Key Features
- **Course Versioning** – Track and manage course updates
- **AI Tutor** – Ask questions and get lesson-based answers
- **Azure Storage Integration** – Secure storage for videos, PDFs, and resources
- Secure Content Delivery using **Azure SAS URLs**
- **Razorpay Webhook Integration** – Reliable payment status synchronization
- **Automatic Instructor Payouts** – Revenue sharing through Razorpay Route
- **Real-Time Notifications** – Instant updates using SignalR

## 🤖 AI Capabilities
- **AI Tutor (Llama 3.3 70B RAG)**: A context-aware chatbot that helps learners by answering questions based specifically on the lesson content they are currently studying, leveraging Retrieval-Augmented Generation.
- **Smart Transcriptions (Whisper AI)**: Automatically transcribes video lessons and generates smart summaries, making content more accessible and easier to review.

## 🛠️ Technical Highlights
- **ASP.NET Core Web API** & Angular
- **PostgreSQL** & Azure Blob Storage
- **JWT Authentication** & Role-Based Access Control (RBAC)
- **SignalR** Real-Time Notifications
- **Whisper AI** Transcription & Smart Summaries
- **Llama 3.3 70B RAG** Context-Aware Lesson Q&A
- **Hangfire** Background Jobs
- **Serilog** Logging & Monitoring
- **ImageSharp** Image Processing
- **Responsive** User Interface
- **Multi-Tier Architecture**
- **Automated Email Notifications**
- **Assignment & Quiz Deadline Reminders**

## ☁️ Deployment & Infrastructure
- **Containerized & Scalable Architecture** using Docker & Azure Kubernetes Service (AKS)
- **Managed Cloud Services** with PostgreSQL, Redis Cache & Azure Blob Storage
- **Automated CI/CD & Infrastructure Provisioning** using GitHub Actions & Bicep
- **Secure & Reliable Operations** with Azure Key Vault and Cloud-Native Design

---

## Application Flow

```mermaid
flowchart TB
    %% Subgraph 1: User Onboarding & Auth
    subgraph SG_Auth["1. Authentication & Access Control"]
        direction TB
        User(["User Entrypoint"]) --> Reg["Sign Up / Login Request"]
        Reg --> JWT["JWT Token Generation & Claims"]
        JWT --> RoleCheck{"Role-Based Authorization"}
    end

    %% Subgraph 2: Instructor Workflow
    subgraph SG_Instructor["2. Instructor Content Studio"]
        direction TB
        RoleCheck -->|Instructor / Admin| CourseDraft["Create & Version Course Modules"]
        CourseDraft --> UploadMedia["Upload Video & Document Assets"]
        UploadMedia --> AzureBlob[("Azure Blob Storage")]
        CourseDraft --> Publish["Publish Course Catalogue"]
    end

    %% Subgraph 3: Learner Catalog & Payments
    subgraph SG_Payment["3. Enrollment & Payment Pipeline"]
        direction TB
        RoleCheck -->|Learner| Browse["Browse Course Catalogue"]
        Browse --> CheckType{"Course Type?"}
        CheckType -->|Free| DirectEnroll["Direct Enrollment"]
        CheckType -->|Paid| Razorpay["Razorpay Order Initialization"]
        Razorpay --> Webhook["Razorpay Webhook Handler"]
        Webhook -->|Payment Verified| PaidEnroll["Grant Course Access & Sync State"]
    end

    %% Subgraph 4: AI-Powered Learning Hub
    subgraph SG_Learning["4. Interactive AI Learning Engine"]
        direction TB
        DirectEnroll & PaidEnroll --> Stream["Stream Lesson Content"]
        Stream --> SAS["Generate Azure SAS Token URL"]
        Stream --> AITutor["AI Tutor Service (Llama 3.3 70B RAG)"]
        Stream --> AIWhisper["Whisper AI Audio Transcription"]
        AITutor --> ContextQA["Contextual Q&A Response"]
        AIWhisper --> SmartSummary["Lesson Summaries & Indexing"]
    end

    %% Subgraph 5: Progress, Certs & Revenue Share
    subgraph SG_Completion["5. Progress, Certification & Settlement"]
        direction TB
        Stream --> Track["Track Progress & Quiz Submissions"]
        Track -->|Persist State| DB[("PostgreSQL Database")]
        Track --> Reminders["Hangfire Background Job Dispatcher"]
        Track -->|100% Completed| CertGen["Generate PDF Certificate (PdfSharpCore)"]
        PaidEnroll -->|Revenue Distribution| Payouts["Instructor Payout (Razorpay Route)"]
    end

    %% High-contrast explicit styling for maximum readability on Light & Dark themes
    classDef authStyle fill:#1C1C7B,color:#FFFFFF,stroke:#FF8C00,stroke-width:2px,font-weight:bold;
    classDef instructorStyle fill:#1D4ED8,color:#FFFFFF,stroke:#60A5FA,stroke-width:2px,font-weight:bold;
    classDef paymentStyle fill:#D97706,color:#FFFFFF,stroke:#FBBF24,stroke-width:2px,font-weight:bold;
    classDef aiStyle fill:#059669,color:#FFFFFF,stroke:#34D399,stroke-width:2px,font-weight:bold;
    classDef completeStyle fill:#7C3AED,color:#FFFFFF,stroke:#C084FC,stroke-width:2px,font-weight:bold;
    classDef dbStyle fill:#334155,color:#FFFFFF,stroke:#94A3B8,stroke-width:2px,font-weight:bold;

    class User,Reg,JWT,RoleCheck authStyle;
    class CourseDraft,UploadMedia,Publish instructorStyle;
    class Browse,CheckType,DirectEnroll,Razorpay,Webhook,PaidEnroll paymentStyle;
    class Stream,SAS,AITutor,AIWhisper,ContextQA,SmartSummary aiStyle;
    class Track,Reminders,CertGen,Payouts completeStyle;
    class AzureBlob,DB dbStyle;
```

---

### Detailed Step-by-Step Execution Flow

#### 1. User Onboarding & Access Control
- **Authentication**: Users register or log in via the Angular frontend. Passwords are securely hashed and validated against PostgreSQL.
- **RBAC**: JWT tokens are issued containing claims for specific roles (**Learner**, **Instructor**, or **Admin**).

#### 2. Course Authoring & Secure Media Management
- **Draft & Versioning**: Instructors build rich multi-module courses with draft state management and course version tracking.
- **Cloud Media Uploads**: Video lessons, slide decks, and downloadable resources are streamed to **Azure Blob Storage**.
- **Access Protection**: Media files are private by default and accessed strictly through short-lived **Azure Shared Access Signatures (SAS URLs)** generated dynamically per user session.

#### 3. Course Discovery, Checkout & Webhook Sync
- **Catalog Browsing**: Learners search, filter, and preview available free and premium courses.
- **Razorpay Checkout**: Premium course orders initialize a secure Razorpay order ID. Learners complete payment via Cards, UPI, or NetBanking.
- **Reliable Webhooks**: Razorpay webhooks send payment verification events back to the .NET API to asynchronously activate course access, preventing loss of access during connection drops.

#### 4. AI-Powered Personalized Learning Experience
- **Contextual AI Tutor**: Powered by **Llama 3.3 70B RAG** (Retrieval-Augmented Generation), students can ask questions about specific lessons and receive accurate, context-bound answers.
- **Whisper AI Video Processing**: Audio from uploaded video lessons is automatically transcribed and formatted into smart key-point summaries and lesson notes.
- **Real-Time Notifications**: **SignalR** handles live updates for newly published lessons, announcements, and peer interactions.

#### 5. Assessments, Certifications & Instructor Monetization
- **Progress & Quiz Engine**: Lesson completion and quiz scores are recorded in PostgreSQL. **Hangfire** handles background cron jobs to dispatch email reminders for approaching assignment deadlines.
- **Dynamic PDF Certificates**: Upon achieving 100% course completion, an official certificate of completion is dynamically rendered using **PdfSharpCore** and made available for download.
- **Automated Payouts**: Platform earnings are split according to configured revenue shares and disbursed automatically to instructor bank accounts via **Razorpay Route**.
---

## 🛠 Prerequisites

Before setting up the project, ensure you have the following installed on your machine:

### Backend Prerequisites (.NET & AI Engine)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/) (v14 or higher recommended)
- [Redis](https://redis.io/download) (Required for distributed caching)
- [Python 3.10+](https://www.python.org/downloads/) (For the AI Engine)
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
   Open `appsettings.Development.json` (or `appsettings.json`) and update the connection strings for PostgreSQL and Redis according to your local setup (refer to the Environment Variables section below).

3. **Start Redis via Docker (Optional):**
   If you don't have Redis installed locally, you can easily spin it up using Docker:
   ```bash
   docker run --name lms-redis -p 6379:6379 -d redis
   ```

4. **Apply Database Migrations:**
   This will create the necessary tables and PostgreSQL functions used by the application.
   ```bash
   dotnet ef migrations add migrationname --project ../LMSApi.DALLibrary --startup-project . --context LMSApi.DALLibrary.Contexts.LMSDbContext
   dotnet ef database update --project ../LMSApi.DALLibrary --startup-project . --context LMSApi.DALLibrary.Contexts.LMSDbContext
   ```
   *(Note: If you have manual SQL scripts like `docs/routines.sql`, execute them against your PostgreSQL database.)*

5. **Run the API:**
   ```bash
   dotnet run
   ```
   The backend API will start running (typically on `https://localhost:5029` or similar).

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

### 3. AI Engine Setup (ai-engine)

1. **Navigate to the ai-engine folder:**
   ```bash
   cd ai-engine
   ```

2. **Install Python dependencies:**
   ```bash
   pip install -r requirements.txt
   ```

3. **Run the FastAPI server:**
   ```bash
   uvicorn main:app --reload
   ```

---

## 📚 Project Structure

- **`LMSApi/`**: Backend API solution (.NET 10, C#).
  - `LMSApi.API/`: Main API entry point, controllers, and configuration.
  - `LMSApi.BALLibrary/`: Business logic layer, services (e.g., Razorpay payment providers).
  - `LMSApi.DALLibrary/`: Data access layer, Entity Framework contexts, migrations.
- **`lms-app/`**: Frontend web application (Angular 21, TypeScript, TailwindCSS).
  - `src/app/`: Angular components, services, pages, and routing.
- **`ai-engine/`**: Python-based microservice for AI features.
  - `routers/`: API endpoints for AI tutor and transcription services.
- **`azure/`**: Infrastructure as Code (Bicep) and Kubernetes (AKS) deployment manifests.
- **`.github/workflows/`**: CI/CD pipelines for automated build and infrastructure deployment.
- **`docs/`**: Documentation and raw SQL scripts (`routines.sql`).

---

## ⚙️ Environment Variables

### Backend (`LMSApi/LMSApi.API/appsettings.Development.json`)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=LmsDb;Username=postgres;Password=your_password",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Key": "Your_Super_Secret_Key_Here",
    "Issuer": "LMSApi",
    "Audience": "LMSApp"
  },
  "Razorpay": {
    "KeyId": "your_razorpay_key",
    "KeySecret": "your_razorpay_secret"
  },
  "AzureBlob": {
    "ConnectionString": "your_azure_blob_connection_string",
    "ContainerName": "lms-media"
  },
  "AiEngine": {
    "BaseUrl": "http://localhost:8000"
  }
}
```

### Frontend (`lms-app/src/environments/environment.ts`)
```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:5029/api',
  razorpayKey: 'your_razorpay_key'
};
```

---

## 📦 External Libraries

### Backend (.NET)
- **AutoMapper**: Simplifies object-to-object mapping.
- **CloudinaryDotNet**: Integration with Cloudinary for managing user uploads.
- **Hangfire**: Background job processing with PostgreSQL storage support.
- **StackExchange.Redis**: Distributed caching.
- **PdfSharpCore**: Programmatic PDF generation (e.g., certificates).
- **Razorpay**: Payment gateway integration.
- **SixLabors.ImageSharp**: Image manipulation and processing.
- **ClosedXML**: Excel spreadsheet manipulation.
- **System.IdentityModel.Tokens.Jwt**: JWT generation and validation.
- **Entity Framework Core / Npgsql**: ORM and PostgreSQL support.
- **Serilog**: Structured logging.
- **Swashbuckle.AspNetCore**: Swagger UI API documentation.

### Frontend (Angular)
- **@microsoft/signalr**: Real-time web functionality (notifications, updates).
- **marked**: Markdown parser for rich text content rendering.
- **pdfjs-dist**: Web standards-based platform for rendering PDFs in browser.
- **rxjs**: Reactive Extensions Library for JavaScript.
- **tailwindcss / @tailwindcss/postcss**: Utility-first CSS framework.
- **vitest / jsdom**: Fast unit testing framework.
- **prettier**: Code formatting.
