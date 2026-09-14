# 🏥 Reaya Clinic Management System

<div align="center">

![Reaya Clinic](https://img.shields.io/badge/Reaya-Clinic-blue?style=for-the-badge)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-purple?style=for-the-badge)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2019-red?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

**A comprehensive web-based healthcare management system built with modern technologies**

[Features](#-key-features) • [Tech Stack](#-technology-stack) • [Installation](#-installation) • [Usage](#-usage) • [Contributing](#-contributing)

</div>

---

## 📋 Table of Contents

- [About The Project](#-about-the-project)
- [Key Features](#-key-features)
- [Technology Stack](#-technology-stack)
- [Project Structure](#-project-structure)
- [Installation](#-installation)
- [Configuration](#-configuration)
- [Usage](#-usage)
- [User Roles](#-user-roles--permissions)
- [Contributing](#-contributing)
- [License](#-license)
- [Contact](#-contact)

---

## 🌟 About The Project

**Reaya Clinic Management System** is a comprehensive web-based healthcare management solution built with ASP.NET Core MVC and Entity Framework Core. The system streamlines clinic operations by providing role-based access for different user types including Administrators, Receptionists, Doctors, and Patients.

This project aims to modernize healthcare management by combining robust technology with intuitive user experience to deliver efficient and secure clinic operations.

---

## ✨ Key Features

### 🔐 User Roles & Permissions
- **Admin**: Full system administration including user management, doctor/patient records, and appointment oversight
- **Receptionist**: Patient registration, appointment scheduling, and basic record management
- **Doctor**: View assigned patients, manage medical records, and update appointment statuses
- **Patient**: Book appointments, view personal medical history, and manage profile information

### 🏥 Core Modules
- **Patient Management**: Complete patient profiles with medical history, contact information, and appointment tracking
- **Doctor Management**: Doctor profiles, specializations, consultation fees, and availability management
- **Appointment System**: Real-time booking with status tracking (Pending, Confirmed, Completed, Cancelled)
- **Medical Records**: Secure medical record creation and management for doctors
- **User Authentication**: Integrated ASP.NET Identity with role-based access control

### 🎨 User Experience
- **Patient Portal**: Easy appointment booking, medical history access, and profile management
- **Doctor Dashboard**: Efficient patient management, medical record creation, and appointment tracking
- **Admin/Receptionist Interface**: Comprehensive clinic management with search, filtering, and reporting capabilities

---

## 🛠 Technology Stack

### Backend
- **ASP.NET Core MVC** - Web framework
- **C#** - Programming language
- **Entity Framework Core** - ORM for database operations
- **SQL Server** - Database management system

### Frontend
- **Razor Views** - Server-side rendering
- **Bootstrap 5** - CSS framework
- **Bootstrap Icons** - Icon library
- **Custom CSS** - Responsive styling

### Authentication & Security
- **ASP.NET Identity** - User authentication and authorization
- **Role-based Authorization** - Access control
- **Data Validation** - Input validation and security
- **Secure File Handling** - Image upload and management

---

## 📁 Project Structure

```
ReayaClinic/
├── Reaya/                          # Main application project
│   ├── Controllers/               # MVC Controllers
│   │   ├── HomeController.cs
│   │   ├── PatientsController.cs
│   │   ├── DoctorsController.cs
│   │   ├── AppointmentsController.cs
│   │   └── MedicalRecordsController.cs
│   ├── Models/                    # Data models
│   │   ├── Patient.cs
│   │   ├── Doctor.cs
│   │   ├── Appointment.cs
│   │   ├── MedicalRecord.cs
│   │   └── ApplicationUser.cs
│   ├── Views/                     # Razor Views
│   │   ├── Home/
│   │   ├── Patients/
│   │   ├── Doctors/
│   │   ├── Appointments/
│   │   ├── PatientPortal/
│   │   ├── DoctorDashboard/
│   │   └── Shared/
│   ├── Data/                      # Database context
│   │   └── AppDbContext.cs
│   ├── wwwroot/                   # Static files
│   │   ├── css/
│   │   ├── js/
│   │   ├── images/
│   │   └── lib/                   # External libraries
│   └── Areas/                     # Areas (Identity)
│       └── Identity/
├── docs/                          # Documentation
│   └── frontend-architecture.md
└── README.md                      # This file
```

---

## 🚀 Installation

### Prerequisites

Before you begin, ensure you have the following installed:
- **.NET 8.0 SDK** or later
- **SQL Server** (LocalDB or full SQL Server)
- **Visual Studio 2022** or **VS Code**
- **Git** (for cloning the repository)

### Step 1: Clone the Repository

```bash
git clone https://github.com/AbdelrahmanAboud/Reaya-Clinic.git
cd Reaya-Clinic
```

### Step 2: Restore Dependencies

```bash
dotnet restore
```

### Step 3: Configure Database Connection

Update the connection string in `Reaya/Data/AppDbContext.cs`:

```csharp
optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=Reaya;Trusted_Connection=True;TrustServerCertificate=True;");
```

### Step 4: Apply Database Migrations

```bash
dotnet ef database update
```

### Step 5: Run the Application

```bash
dotnet run
```

The application will be available at `https://localhost:5001` or `http://localhost:5000`

---

## ⚙️ Configuration

### Database Setup

1. **SQL Server Configuration**: Ensure SQL Server is running and accessible
2. **Connection String**: Update the connection string in `AppDbContext.cs` if needed
3. **Migrations**: Run migrations to create the database schema

### Default Roles

The system includes the following roles:
- **Admin**: Full system access
- **Receptionist**: Patient and appointment management
- **Doctor**: Medical records and patient management
- **Patient**: Personal appointments and medical history

### Initial Admin Account

Create an admin account through the registration process or use database seeding for initial setup.

---

## 📖 Usage

### For Patients

1. **Register**: Create a new patient account
2. **Book Appointment**: Select doctor, date, and time
3. **View History**: Access medical records and appointment history
4. **Manage Profile**: Update personal information

### For Doctors

1. **Login**: Access doctor dashboard
2. **View Patients**: See assigned patients and their information
3. **Manage Records**: Create and update medical records
4. **Update Status**: Change appointment statuses

### For Receptionists

1. **Register Patients**: Add new patients to the system
2. **Schedule Appointments**: Book appointments for patients
3. **Manage Records**: Update patient information
4. **Generate Reports**: View clinic statistics

### For Admins

1. **User Management**: Create and manage user accounts
2. **Role Assignment**: Assign roles to users
3. **System Configuration**: Configure system settings
4. **Oversight**: Monitor all system activities

---

## 👥 User Roles & Permissions

| Resource | Admin | Receptionist | Doctor | Patient |
|----------|-------|--------------|--------|---------|
| Doctors | Full CRUD | View and basic edit | View own profile | View only |
| Patients | Full CRUD | Add, view, edit | View assigned patients | View own profile |
| Appointments | Full CRUD | Add, view, edit, cancel | View assigned and update status | Create and view own |
| Medical Records | Authorized view | No access by default | Create and edit assigned | View own |

---

## 🤝 Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

### How to Contribute

1. **Fork the Project**
2. **Create your Feature Branch** (`git checkout -b feature/AmazingFeature`)
3. **Commit your Changes** (`git commit -m 'Add some AmazingFeature'`)
4. **Push to the Branch** (`git push origin feature/AmazingFeature`)
5. **Open a Pull Request**

### Development Guidelines

- Follow the existing code style and conventions
- Write clean, commented code
- Test your changes thoroughly
- Update documentation as needed

---

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

---

## 👨‍💻 Author

**Abdelrahman Aboud Mohamed Aboud**

- **University**: Helwan University
- **Faculty**: Engineering
- **Department**: Computer Engineering
- **Year**: Second Year
- **Submitted to**: Dr. Mohamed Abdellatif

---

## 📞 Contact

- **GitHub**: [@AbdelrahmanAboud](https://github.com/AbdelrahmanAboud)
- **Project Link**: [https://github.com/AbdelrahmanAboud/Reaya-Clinic](https://github.com/AbdelrahmanAboud/Reaya-Clinic)

---

## 🙏 Acknowledgments

- **Dr. Mohamed Abdellatif** - Project supervisor and guidance
- **Helwan University** - Academic institution
- **ASP.NET Core Team** - Amazing framework and documentation
- **Bootstrap Team** - Excellent UI framework

---

<div align="center">

**Built with ❤️ for modern healthcare management**

⭐ **Star this project if you find it helpful!**

</div>