# SheSecure 🛡️

SheSecure is a comprehensive, microservices-based platform designed to prioritize and ensure the physical safety, mental well-being, and overall security of employees in the modern workplace. It acts as a digital safety net and an advanced HR support system, empowering employees to seek help discreetly while giving organizations the tools to respond quickly and effectively.

---

## 🌟 Key Features

### Personal Safety & Emergency Response
- **SOS Alerts (Panic Button):** Instantly notify the security team and HR in the event of an immediate threat or danger.
- **Safe Check-ins (Safe Reach):** Employees traveling for work or commuting late can log their safe arrival to keep the organization informed.

### Mental Health & Wellness
- **Mood Tracking:** Log daily moods to maintain a pulse on personal well-being.
- **Wellness Checks:** Discreetly request wellness checks from HR or counseling teams when feeling overwhelmed.

### Incident Reporting & Case Management
- **Secure Complaint Portal:** A private space to report workplace issues or HR grievances with the ability to upload supporting evidence (images, documents).
- **HR Resolution Workflow:** A dedicated tracking system for HR to review, investigate, and update the status of complaints securely.

### Leadership & Analytics Dashboard
- **Command Center:** Real-time dashboards for HR and leadership to monitor organizational safety.
- **Key Metrics:** Track active complaints, SOS alert volume, and overall wellness trends to make data-driven decisions.

---

## 🏗️ Technical Architecture

SheSecure is built on a scalable **Microservices Architecture** using **.NET Core / ASP.NET Core** and **Entity Framework Core**.

### Microservices
1. **`SheSecure.Web`**: The frontend MVC web application serving as the primary user interface for employees, HR, and Admins.
2. **`SheSecure.AuthService`**: Manages identity, secure logins, role-based access control (RBAC), and JWT token issuance.
3. **`SheSecure.ComplaintService`**: Handles the lifecycle of workplace complaints and secure file uploads.
4. **`SheSecure.Safety&WellnessService`**: The core engine for SOS alerts, Safe Reach check-ins, and mood tracking.
5. **`SheSecure.NotificationService`**: A background messaging hub responsible for sending automated emails and alerts.
6. **`SheSecure.DashboardService`**: Aggregates data across services to populate analytical dashboards for employees and leadership.

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) (or the relevant version used in your environment)
- SQL Server (or your configured database provider)
- Visual Studio 2022 or VS Code

### Installation & Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/your-username/SheSecure.git
   cd SheSecure
   ```

2. **Configure Database Connection Strings:**
   Update the `appsettings.json` file in each microservice (ComplaintService, SafetyService, etc.) to point to your local SQL Server instance.

3. **Run Entity Framework Migrations:**
   Navigate to the respective service directories (e.g., `SheSecure.ComplaintService`, `SheSecure.Safety&WellnessService`) and apply database migrations:
   ```bash
   dotnet ef database update
   ```

4. **Run the Application:**
   You can run all microservices and the Web project simultaneously by configuring your IDE to start multiple projects, or run them individually via the CLI:
   ```bash
   dotnet run --project SheSecure.Web
   dotnet run --project SheSecure.AuthService
   # repeat for other services...
   ```

---

## 🤝 Contributing
Contributions, issues, and feature requests are welcome! Feel free to check the [issues page](https://github.com/your-username/SheSecure/issues).

## 📝 License
This project is licensed under the MIT License - see the LICENSE file for details.
