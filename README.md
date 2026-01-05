# E-Commerce Platform with AI Integration (ASP.NET Core)

A robust e-commerce solution built with **ASP.NET Core MVC**, featuring a multi-role system, automated product approval workflows, and an AI-powered FAQ assistant integrated with **Google Gemini**.

## Features

* **Role-Based Access Control (RBAC)**:
    * **Admin**: Full control over products, categories, users, and reviews. Dashboard for system overview.
    * **Collaborator**: Can propose new products. Proposals go through an approval workflow.
    * **Registered User**: Can browse products, manage a wishlist, add items to cart, and place orders.
* **AI FAQ Assistant**: Integrated **Gemini AI** that answers user questions based on product descriptions and historical FAQ data.
* **Product Proposal System**: Collaborators submit products which remain in a "Pending" state until an Admin approves or rejects them.
* **Order Management**: Complete flow from shopping cart to order placement and history tracking.
* **Reviews & Ratings**: Users can leave feedback on products, which Admins can moderate.

## Tech Stack

* **Backend**: .NET 9.0 (ASP.NET Core MVC)
* **Database**: SQL Server with Entity Framework Core
* **AI Service**: Google Gemini API (via `Google.GenerativeAI` SDK)
* **Identity**: ASP.NET Core Identity (Roles: Admin, Colab, User)
* **Frontend**: Razor Views, Bootstrap 5, JavaScript

## Getting Started

### Prerequisites

* [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
* [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or LocalDB
* A **Google Gemini API Key** (Get it from [Google AI Studio](https://aistudio.google.com/))

### Installation & Setup

1.  **Clone the repository**:
    ```bash
    git clone https://github.com/ModernityRejecter/Proiect-DAW.git
    cd Proiect-DAW
    ```

2.  **Configure Environment Secrets**:
    Open `appsettings.json` and update the connection string and API key:
    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=Proiect;Trusted_Connection=True;MultipleActiveResultSets=true"
      },
      "Gemini": {
        "ApiKey": "YOUR_GEMINI_API_KEY_HERE"
      }
    }
    ```

3.  **Apply Migrations & Seed Data**:
    The project uses an initializer to create roles and seed initial data (Admin, Categories, Products).
    ```bash
    dotnet ef database update
    ```

4.  **Run the application**:
    ```bash
    dotnet run
    ```

## Default Credentials (Seed Data)

Upon first run, the system creates the following accounts:

| Role | Email | Password |
| :--- | :--- | :--- |
| **Admin** | `admin@test.com` | `Admin1!` |
| **Collaborator** | `colab@test.com` | `Colab1!` |
| **User** | `user@test.com` | `User1!` |

## Project Structure

* `Controllers/`: Logic for Cart, Orders, Product Proposals, and Admin panels.
* `Models/`: Database entities and ViewModels.
* `Data/`: `ApplicationDbContext` and Seed Data logic.
* `Services/`: Contains `GeminiService.cs` for AI interaction.
* `Views/`: UI templates grouped by feature.

## AI Logic (Gemini)

The `GeminiService` follows a strict logic for the FAQ system:
1.  Checks if a similar question was already answered in the **FAQ History**.
2.  Searches **Product Info** for relevant details.
3.  If no information is found, it returns "NU_STIU" to avoid hallucinations.

## 📄 License

This project is open-source and available under the [MIT License](LICENSE).
