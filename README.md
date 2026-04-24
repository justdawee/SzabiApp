# SzabiApp

A modern, full-stack Leave Management System (Szabadságkezelő alkalmazás) designed to streamline the process of requesting and approving time off within an organization.

![Full Stack](https://img.shields.io/badge/Architecture-Full--Stack-blue)
![.NET 10](https://img.shields.io/badge/.NET-10.0-512bd4)
![Vue 3](https://img.shields.io/badge/Vue.js-3.5-4fc08d)
![PostgreSQL](https://img.shields.io/badge/Database-PostgreSQL-336791)
![Docker](https://img.shields.io/badge/Docker-Supported-2496ed)

## 🚀 Overview

SzabiApp provides a seamless interface for employees to track their leave allowances and submit requests, while giving managers and admins the tools they need to review applications, manage company holidays, and oversee work schedules.

### Key Features

-   **🔐 Authentication:** Secure JWT-based authentication with role-based access control (Admin, Manager, User).
-   **📅 Leave Management:** Comprehensive system for submitting, reviewing, and tracking leave requests.
-   **📊 Leave Allowances:** Automated tracking of remaining leave days per user.
-   **🗓️ Holiday Management:** Global company holiday calendar integration.
-   **🕒 Work Schedules:** Flexible work schedule management for different departments or users.
-   **📱 Responsive Design:** Modern UI built with Vue 3 and Tailwind CSS 4, fully responsive for mobile and desktop.

---

## 🛠️ Tech Stack

### Backend
-   **.NET 10 Web API**: Latest performance and features.
-   **Entity Framework Core**: For robust data access.
-   **PostgreSQL**: Reliable relational database.
-   **NodaTime**: Precise date and time handling.
-   **Riok.Mapperly**: High-performance source-generated mapping.
-   **FluentValidation**: Clean and expressive validation logic.
-   **Scalar**: Modern API documentation (OpenAPI/Swagger).

### Frontend
-   **Vue 3 (Composition API)**: Reactive and modular UI.
-   **TypeScript**: Type safety across the application.
-   **Vite**: Ultra-fast build tool and dev server.
-   **Pinia**: Intuitive state management.
-   **Tailwind CSS 4**: Next-generation utility-first styling.
-   **Lucide Vue**: Beautiful and consistent iconography.

---

## 🚦 Getting Started

### Prerequisites
-   [Docker Desktop](https://www.docker.com/products/docker-desktop/)
-   [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (for local development)
-   [Node.js (v20+)](https://nodejs.org/) (for local development)

### Quick Start with Docker

The easiest way to get the entire stack running is using Docker Compose:

1.  Clone the repository:
    ```bash
    git clone https://github.com/justdawee/SzabiApp.git
    cd SzabiApp
    ```

2.  Copy the example environment file and adjust if necessary:
    ```bash
    cp .env.example .env
    ```

3.  Build and start the containers:
    ```bash
    docker-compose up -d --build
    ```

4.  Access the applications:
    -   **Frontend:** [http://localhost:80](http://localhost:80)
    -   **Backend API:** [http://localhost:5062](http://localhost:5062)
    -   **API Documentation:** [http://localhost:5062/scalar/v1](http://localhost:5062/scalar/v1)

---

## 📂 Project Structure

```text
SzabiApp/
├── SzabiApp.Backend/     # .NET 10 Web API
├── SzabiApp.Frontend/    # Vue 3 Single Page Application
├── docker-compose.yml    # Orchestration for DB, API, and Web
└── .env                  # Environment configuration
```

## 📝 License

This project is licensed under the MIT License.
