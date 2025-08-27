"# GameHub - Game Registration Platform

GameHub is a comprehensive game registration platform built with ASP.NET Core 8.0 using MVC Razor Pages architecture. The platform enables users to browse, purchase, and manage game registrations with integrated VNPay payment processing.

## 🚀 Features

### For Players
- **User Registration & Authentication**: Secure account creation and login with JWT tokens
- **Game Browsing**: Browse games by categories, developers with search and filtering
- **Shopping Cart**: Add games to cart, manage cart items
- **Game Purchase**: Secure payment processing via VNPay integration
- **Registration Management**: View purchase history and game registrations
- **Real-time Updates**: SignalR integration for live notifications

### For Administrators
- **Game Management**: CRUD operations for games, categories, and developers
- **User Management**: Manage player accounts and registrations
- **Registration Monitoring**: View and manage all game registrations
- **Dashboard**: Administrative overview and analytics

### Payment Integration
- **VNPay Integration**: Secure payment processing with Vietnam's leading payment gateway
- **Currency Support**: USD to VND conversion with configurable exchange rates
- **Payment Tracking**: Complete payment history and status tracking

## 🏗️ Architecture

The project follows a **3-Layer Architecture** pattern:

```
GameHub_NguyenHoaiNam_SE161728_3W/
├── GameHub.WebApp/          # Presentation Layer (MVC Razor Pages)
├── GameHub.BLL/             # Business Logic Layer
└── GameHub.DAL/             # Data Access Layer
```

### Presentation Layer (`GameHub.WebApp`)
- **ASP.NET Core 8.0 MVC Razor Pages**
- **SignalR Hubs** for real-time communication
- **Authentication & Authorization** with JWT tokens
- **Session Management** for user state
- **Responsive UI** with Bootstrap

### Business Logic Layer (`GameHub.BLL`)
- **Service Pattern** implementation
- **AutoMapper** for object-to-object mapping
- **DTOs** for data transfer
- **Validation Attributes** for business rules
- **Query Builders** for dynamic filtering
- **VNPay Integration** helpers

### Data Access Layer (`GameHub.DAL`)
- **Entity Framework Core** with Code First approach
- **Generic Repository Pattern** with Unit of Work
- **Entity Configurations** and relationships
- **Migration Support** with auto-apply on startup

## 🛠️ Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT Bearer Tokens
- **Payment**: VNPay Integration
- **Real-time**: SignalR
- **Mapping**: AutoMapper
- **UI**: Bootstrap 5, Razor Pages
- **Architecture**: 3-Layer with Repository Pattern

## 📋 Prerequisites

- **.NET 8.0 SDK** or later
- **SQL Server** (LocalDB or full instance)
- **Visual Studio 2022** or **VS Code**
- **VNPay Sandbox Account** (for payment testing)

## 🚀 Quick Start

### 1. Clone the Repository
```bash
git clone https://github.com/namprobe/GameHub_RazorPage_MVC.git
cd GameHub_NguyenHoaiNam_SE161728_3W
```

### 2. Configure Database
Update the connection string in `GameHub.WebApp/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=GameHub;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

### 3. Configure Application Settings
Copy `appsettings.example.json` to `appsettings.json` and update the following:

```json
{
  "BaseUrl": "http://localhost:5045",
  "AdminUser": {
    "Email": "admin@gamehub.com",
    "DefaultPassword": "YourSecurePassword",
    "Username": "Administrator"
  },
  "VNPay": {
    "TmnCode": "YOUR_VNPAY_TMN_CODE",
    "HashSecret": "YOUR_VNPAY_HASH_SECRET",
    "ReturnUrl": "http://localhost:5045/vnpay/return"
  }
}
```

### 4. Run the Application
```bash
# Navigate to the web application directory
cd GameHub.WebApp

# Restore dependencies
dotnet restore

# Run the application (migrations will be applied automatically)
dotnet run
```

The application will be available at `https://localhost:5045` or `http://localhost:5045`.

## 🗄️ Database Schema

The application uses **Code First** approach with automatic migration application on startup. Key entities include:

- **User** - Authentication and user management
- **Player** - Player-specific data
- **Game** - Game catalog
- **GameCategory** - Game categorization
- **Developer** - Game developers
- **Cart & CartItem** - Shopping cart functionality
- **GameRegistration & GameRegistrationDetail** - Purchase records
- **Payment** - Payment tracking

## 💳 VNPay Integration

### Setup VNPay Sandbox
1. Register at [VNPay Sandbox](https://sandbox.vnpayment.vn/)
2. Get your `TmnCode` and `HashSecret`
3. Update `appsettings.json` with your credentials

### Local HTTPS Setup with ngrok
For local testing, VNPay requires HTTPS return URLs. Use ngrok to create a secure tunnel:

```bash
# Install and setup ngrok (one-time setup)
ngrok authtoken YOUR_AUTH_TOKEN

# Start your app
dotnet run --urls="http://localhost:5045"

# In another terminal, create HTTPS tunnel
ngrok http 5045
```

Update your `appsettings.json` with the ngrok URL:
```json
{
  "BaseUrl": "https://your-ngrok-subdomain.ngrok-free.app",
  "VNPay": {
    "ReturnUrl": "https://your-ngrok-subdomain.ngrok-free.app/vnpay/return"
  }
}
```

### Test Payment Flow
1. Add games to cart
2. Proceed to checkout
3. Use VNPay sandbox credentials:
   - **Card Number**: 9704198526191432198
   - **Card Owner**: NGUYEN VAN A
   - **Expiry**: 07/15
   - **Bank**: NCB
   - **OTP**: 123456

**For detailed testing setup with ngrok**, see [VNPay Testing Guide](VNPAY_TESTING_GUIDE.md).

## 🔧 Configuration

### Application Settings
Key configuration sections in `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "Your-Secret-Key-Here",
    "Issuer": "GameHubApp",
    "Audience": "GameHubUsers",
    "AccessTokenExpiration": 150
  },
  "Session": {
    "IdleTimeout": 150,
    "CookieName": "GameHub.Session"
  },
  "Currency": {
    "UsdToVndRate": 25000
  },
  "DataSeeding": {
    "EnableSeeding": true
  }
}
```

## 🧪 Testing

### Admin Account
Default admin credentials (configurable in `appsettings.json`):
- **Email**: admin@gamehub.com
- **Password**: As configured in `AdminUser.DefaultPassword`

### Sample Data
The application includes automatic data seeding for:
- Game categories
- Developers
- Sample games
- Admin user account

## 📁 Project Structure

```
GameHub.WebApp/
├── Pages/
│   ├── Admin/           # Admin management pages
│   ├── Player/          # Player-specific pages
│   ├── Shared/          # Layout and shared components
│   └── Auth/            # Authentication pages
├── Configurations/      # Service configurations
├── Extensions/          # Extension methods
└── Helpers/            # Utility helpers

GameHub.BLL/
├── DTOs/               # Data Transfer Objects
├── Implements/         # Service implementations
├── Interfaces/         # Service contracts
├── Mapping/            # AutoMapper profiles
├── Models/             # Business models
├── QueryBuilders/      # Dynamic query builders
├── Validations/        # Custom validation attributes
└── Helpers/            # Business logic helpers

GameHub.DAL/
├── Entities/           # Entity models
├── Context/            # DbContext configuration
├── Implements/         # Repository implementations
├── Interfaces/         # Repository contracts
├── Migrations/         # EF Core migrations
├── Enums/              # Application enumerations
└── Common/             # Shared data access components
```

## 🔒 Security Features

- **JWT Authentication** with configurable expiration
- **Session Management** with timeout protection
- **Password Hashing** with secure algorithms
- **Authorization Policies** for role-based access
- **CSRF Protection** for forms
- **Input Validation** and sanitization

## 🚀 Deployment

### Production Checklist
1. Update `appsettings.json` with production values
2. Set `DataSeeding.EnableSeeding` to `false`
3. Configure production database connection
4. Set up VNPay production credentials
5. Configure HTTPS and security headers
6. Set up logging and monitoring

### Docker Support
The application can be containerized using the included Dockerfile (if present) or by creating one following .NET Core best practices.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is developed for educational purposes as part of PRN222 coursework.

## 👨‍💻 Author

**Nguyen Hoai Nam** - SE161728  
PRN222 Assignment - FPT University

## 📞 Support

For support and questions, please contact:
- Email: nguyenhoainamvt99@gmail.com
- GitHub: https://github.com/namprobe

---

**Note**: This is an educational project developed for PRN222 course requirements. VNPay integration is configured for sandbox testing only." 
