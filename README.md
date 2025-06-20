# CredWise
A full-featured bank loan management system built with ASP.NET Core MVC, featuring separate portals for customers and administrators.

# CredWise - Bank Loan Management System

A comprehensive, secure, and modern web application built with ASP.NET Core MVC for managing the entire lifecycle of bank loans. This project features a dual-portal system: a user-friendly portal for customers and a powerful dashboard for administrators.

---

## About The Project

CredWise is a full-featured simulation of a loan management platform. It demonstrates best practices in web application architecture, including secure authentication, efficient data handling with Entity Framework Core, and a clean separation of concerns using the MVC and ViewModel patterns.

The system allows customers to register, apply for various loan products, make payments, and view their statements. The admin panel provides a complete overview of the system's health, with tools to manage customers, loan applications, KYC approvals, and loan products.

### Key Features

#### Customer Portal
* **Secure Authentication:** User registration with password hashing (BCrypt) and secure cookie-based login.
* **KYC Management:** A dedicated portal for customers to upload and check the status of their KYC documents.
* **Dynamic Loan Application:** An interactive form where customers can select loan products and see real-time calculations for EMI and total repayment.
* **Comprehensive Dashboard:** An at-a-glance view of active loans, outstanding balances, overall progress, and payment notifications.
* **Loan Statements & History:** Detailed view of all loans and their complete repayment schedules.
* **Payment Processing:** A robust system for making payments that correctly allocates funds to overdue and current installments.

#### Admin Panel
* **Secure Admin Role:** The entire admin section is protected and accessible only to users with the "Admin" role.
* **Analytics Dashboard:** Features dynamic charts showing monthly loan disbursements vs. repayments and key performance indicators (KPIs) like total active loans, pending applications, and overdue accounts.
* **Customer Management:** A master view to see all registered customers and drill down into their detailed profiles and loan histories.
* **KYC Approval System:** An interface for admins to view submitted KYC documents and approve or reject them.
* **Loan Approval Workflow:** A dedicated page for admins to review pending loan applications and approve or reject them. Approving a loan automatically generates its repayment schedule.
* **Loan Product Management:** Full CRUD (Create, Read, Update, Delete) functionality for managing the loan products offered by the bank.

---

## Built With

This project is built using a modern, robust, and scalable technology stack.

**Back-End:**
* **C#**
* **ASP.NET Core MVC**
* **Entity Framework Core (EF Core)**
* **LINQ**
* **BCrypt.Net**

**Database:**
* **SQL Server**

**Front-End:**
* **HTML5 & CSS3**
* **Bootstrap**
* **JavaScript (ES6+)**
* **jQuery**
* **Font Awesome**

---

## Getting Started

To get a local copy up and running, follow these simple steps.

### Prerequisites

* **.NET SDK** (e.g., .NET 8 or later)
* **SQL Server** (Express, Developer, or full version)
* An IDE like **Visual Studio 2022** or **Visual Studio Code**

### Installation

1.  **Clone the repository**
    ```sh
    git clone [https://github.com/YourUsername/CredWise.git](https://github.com/YourUsername/CredWise.git)
    ```
2.  **Configure Database Connection**
    * Open `appsettings.json` in the main project folder.
    * Find the `ConnectionStrings` section and update the `DefaultConnection` string to point to your local SQL Server instance. It should look something like this:
        ```json
        "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CredWiseDB;Trusted_Connection=True;MultipleActiveResultSets=true"
        ```
3.  **Apply Database Migrations**
    * Open a terminal or command prompt in the project's root directory.
    * Run the following command to create the database and all its tables based on the Entity Framework Core models:
        ```sh
        dotnet ef database update
        ```
4.  **Run the Application**
    * You can either press the "Run" button in Visual Studio or use the .NET CLI:
        ```sh
        dotnet run
        ```
    * The application will be available at `https://localhost:xxxx` and `http://localhost:yyyy` (the ports will be specified in the terminal).

---

## Architectural Highlights & Logic

This project was built with a focus on clean architecture and best practices.

* **Secure, Claims-Based Authentication:** User sessions are handled via encrypted, stateless cookies containing user claims (like `CustomerId` and `Role`).
* **ViewModel Pattern:** Used extensively to create a secure barrier between the database models and the UI. This prevents over-posting security vulnerabilities and tailors data specifically for each view.
* **Efficient Data Access with EF Core:**
    * **Eager Loading (`Include`/`ThenInclude`):** Used to prevent the "N+1 query problem" and efficiently load related data in single database queries.
    * **Projections (`Select`):** Queries are optimized to only select the data columns needed by the ViewModel, drastically reducing network traffic from the database.
    * **`IQueryable`:** Used to build complex queries step-by-step, which are then translated into a single, efficient SQL statement for the database to execute.
* **Service-Oriented Logic:** Complex business logic, like the daily check for overdue loans, is encapsulated in a dedicated `LoanUpdateOrchestratorService`, separating it from the controllers.
* **API-Driven Admin Panel:** The admin dashboard is powered by API-style controller actions that return JSON, allowing for a dynamic front-end experience without full page reloads.

---

## License

Distributed under the MIT License. See `LICENSE.txt` for more information.
