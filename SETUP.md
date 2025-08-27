# GameHub Setup Guide

This guide will walk you through setting up the GameHub application on your local development environment.

## Prerequisites

### Required Software
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (Express, Developer, or LocalDB)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [Git](https://git-scm.com/)

### Optional Tools
- [SQL Server Management Studio (SSMS)](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- [Azure Data Studio](https://azure.microsoft.com/en-us/products/data-studio/)

## Step-by-Step Setup

### 1. Clone the Repository
```bash
git clone https://github.com/namprobe/GameHub_RazorPage_MVC.git
cd GameHub_NguyenHoaiNam_SE161728_3W
```

### 2. Setup SQL Server Database

#### Option A: Using SQL Server LocalDB (Recommended for Development)
LocalDB is included with Visual Studio and .NET SDK.

1. Verify LocalDB installation:
```bash
sqllocaldb info
```

2. Start LocalDB (if not running):
```bash
sqllocaldb start mssqllocaldb
```

3. Update connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GameHub;Trusted_Connection=true;TrustServerCertificate=True;"
  }
}
```

#### Option B: Using SQL Server Express/Developer
1. Install SQL Server Express or Developer edition
2. Update connection string with your SQL Server instance details:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=GameHub;User Id=sa;Password=YourPassword;TrustServerCertificate=True;"
  }
}
```

### 3. Configure Application Settings

1. Copy the example configuration:
```bash
cp GameHub.WebApp/appsettings.example.json GameHub.WebApp/appsettings.json
```

2. Update `appsettings.json` with your values:

#### Required Settings
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "YOUR_DATABASE_CONNECTION_STRING"
  },
  "BaseUrl": "http://localhost:5045",
  "AdminUser": {
    "Email": "admin@gamehub.com",
    "DefaultPassword": "YourSecurePassword123!",
    "Username": "Administrator"
  }
}
```

#### VNPay Configuration (Optional for Payment Testing)
```json
{
  "VNPay": {
    "TmnCode": "YOUR_VNPAY_TMN_CODE",
    "HashSecret": "YOUR_VNPAY_HASH_SECRET",
    "ReturnUrl": "http://localhost:5045/vnpay/return"
  }
}
```

### 4. Setup VNPay Sandbox (Optional)

If you want to test payment functionality:

1. Register at [VNPay Sandbox](https://sandbox.vnpayment.vn/)
2. Get your credentials:
   - TMN Code
   - Hash Secret
3. Update the VNPay section in `appsettings.json`

#### Setting up HTTPS with ngrok for VNPay Return URL

VNPay requires HTTPS for return URLs. Since localhost doesn't have SSL by default, use ngrok to create a secure tunnel:

1. **Install ngrok**:
   - Download from [ngrok.com](https://ngrok.com/)
   - Extract and add to your PATH
   - Sign up for a free account and get your auth token

2. **Setup ngrok**:
   ```bash
   # Authenticate ngrok with your token
   ngrok authtoken YOUR_AUTH_TOKEN
   
   # Start your GameHub application first
   dotnet run --urls="http://localhost:5045"
   
   # In another terminal, create HTTPS tunnel
   ngrok http 5045
   ```

3. **Configure VNPay with ngrok URL**:
   ```json
   {
     "BaseUrl": "https://your-ngrok-subdomain.ngrok-free.app",
     "VNPay": {
       "TmnCode": "YOUR_VNPAY_TMN_CODE",
       "HashSecret": "YOUR_VNPAY_HASH_SECRET",
       "ReturnUrl": "https://your-ngrok-subdomain.ngrok-free.app/vnpay/return"
     }
   }
   ```

4. **Test Payment Flow**:
   - Access your app via the ngrok HTTPS URL
   - Add games to cart and proceed to checkout
   - Complete payment on VNPay sandbox
   - VNPay will successfully redirect back to your local app

**Note**: ngrok URLs change on each restart unless you have a paid plan. Update `appsettings.json` with the new URL each time you restart ngrok.

**For detailed ngrok setup and troubleshooting**, see [VNPay Testing Guide](VNPAY_TESTING_GUIDE.md).

### 5. Install Dependencies and Run

```bash
# Navigate to the web application directory
cd GameHub.WebApp

# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build

# Run the application
dotnet run
```

The application will:
- Automatically apply database migrations
- Seed initial data (if `DataSeeding.EnableSeeding` is true)
- Start on `https://localhost:7191` and `http://localhost:5045`

### 6. Verify Installation

1. **Open your browser** and navigate to `http://localhost:5045`
2. **Login as Admin**:
   - Email: `admin@gamehub.com`
   - Password: As configured in `AdminUser.DefaultPassword`
3. **Test Player Registration**:
   - Click "Register" to create a player account
   - Browse games and test cart functionality

## Development Environment Setup

### Visual Studio 2022
1. Open `GameHub_NguyenHoaiNam_SE161728_3W.sln`
2. Set `GameHub.WebApp` as startup project
3. Press F5 to run with debugging

### VS Code
1. Open the project folder in VS Code
2. Install recommended extensions:
   - C# Dev Kit
   - .NET Core Tools
3. Use Terminal to run `dotnet run`

## Database Operations

### Manual Migration Commands
If you need to run migrations manually:

```bash
# Add new migration
dotnet ef migrations add MigrationName --project GameHub.DAL --startup-project GameHub.WebApp

# Update database
dotnet ef database update --project GameHub.DAL --startup-project GameHub.WebApp

# Drop database (for development reset)
dotnet ef database drop --project GameHub.DAL --startup-project GameHub.WebApp
```

### Reset Database and Data
To completely reset the database:

1. Stop the application
2. Drop the database:
```bash
dotnet ef database drop --project GameHub.DAL --startup-project GameHub.WebApp
```
3. Run the application again (migrations will be applied automatically)

## Common Issues and Solutions

### Issue: Database Connection Failed
**Solution**: 
- Verify SQL Server is running
- Check connection string format
- Ensure database user has proper permissions

### Issue: VNPay Payment Not Working
**Solution**:
- Verify VNPay credentials in `appsettings.json`
- Check return URL matches your local development URL
- Ensure you're using sandbox environment
- **For local testing**: Use ngrok to create HTTPS tunnel and update return URL

### Issue: VNPay Return URL Not Working
**Solution**:
- Ensure you're using HTTPS (via ngrok for local development)
- Verify return URL in VNPay configuration matches your ngrok URL exactly
- Check that your application is running when VNPay tries to return
- Update `BaseUrl` in appsettings.json to match your ngrok URL

### Issue: ngrok Connection Failed
**Solution**:
- Verify ngrok is installed and authenticated
- Check that your application is running on the specified port
- Ensure no firewall is blocking ngrok
- Try restarting ngrok tunnel

### Issue: Migrations Not Applied
**Solution**:
- Check if `DataSeeding.EnableSeeding` is true
- Manually run migration commands
- Verify connection string is correct

### Issue: Admin User Not Created
**Solution**:
- Check `AdminUser` configuration in `appsettings.json`
- Ensure `DataSeeding.EnableSeeding` is true
- Reset database to trigger seeding again

## Production Deployment Considerations

### Security
- Change all default passwords
- Use strong JWT secret keys
- Set up HTTPS with valid certificates
- Disable data seeding in production

### Database
- Use a production SQL Server instance
- Set up proper backup strategies
- Configure connection pooling
- Monitor performance

### VNPay
- Switch to production VNPay credentials
- Update return URLs to production domains
- Test payment flows thoroughly

## Getting Help

- Check the main [README.md](README.md) for general information
- Review the [VNPay Integration Guide](VNPAY_INTEGRATION_GUIDE.md)
- See [VNPay Testing Guide](VNPAY_TESTING_GUIDE.md) for ngrok setup and troubleshooting
- Contact the development team for specific issues

## Next Steps

After successful setup:
1. Explore the admin panel for game management
2. Test the player registration and cart functionality
3. Try the VNPay payment integration
4. Review the codebase architecture
5. Start customizing for your needs
