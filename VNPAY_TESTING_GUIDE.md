# VNPay Testing Guide with ngrok

This guide explains how to properly test VNPay payment integration using ngrok for HTTPS tunneling.

## Why ngrok is needed

VNPay requires HTTPS URLs for return callbacks. Since local development typically runs on HTTP, we need ngrok to create a secure tunnel to our localhost application.

## Prerequisites

- GameHub application running locally
- VNPay sandbox account with credentials
- ngrok account (free tier is sufficient)

## Step-by-Step Setup

### 1. Install and Setup ngrok

#### Download and Install
1. Go to [ngrok.com](https://ngrok.com/) and create a free account
2. Download ngrok for your operating system
3. Extract the executable to a folder in your PATH

#### Authenticate ngrok
```bash
# Get your auth token from ngrok dashboard
ngrok authtoken YOUR_AUTH_TOKEN_HERE
```

### 2. Start Your Application

```bash
# Navigate to your GameHub project
cd GameHub.WebApp

# Start the application on a specific port
dotnet run --urls="http://localhost:5045"
```

Keep this terminal running.

### 3. Create ngrok Tunnel

Open a new terminal and run:
```bash
# Create HTTPS tunnel to your local app
ngrok http 5045
```

You'll see output like:
```
ngrok by @inconshreveable

Session Status                online
Account                       your-email@example.com
Version                       3.3.0
Region                        United States (us)
Forwarding                    https://abc123.ngrok-free.app -> http://localhost:5045
Forwarding                    http://abc123.ngrok-free.app -> http://localhost:5045

Connections                   ttl     opn     rt1     rt5     p50     p90
                              0       0       0.00    0.00    0.00    0.00
```

**Important**: Copy the HTTPS URL (e.g., `https://abc123.ngrok-free.app`)

### 4. Update appsettings.json

Update your `GameHub.WebApp/appsettings.json` with the ngrok URL:

```json
{
  "BaseUrl": "https://abc123.ngrok-free.app",
  "VNPay": {
    "TmnCode": "YOUR_VNPAY_TMN_CODE",
    "HashSecret": "YOUR_VNPAY_HASH_SECRET",
    "ReturnUrl": "https://abc123.ngrok-free.app/vnpay/return",
    "PaymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "TimeOut": 900
  }
}
```

### 5. Restart Your Application

Since you updated `appsettings.json`, restart your application:
```bash
# Stop the current running app (Ctrl+C)
# Then restart
dotnet run --urls="http://localhost:5045"
```

## Testing Payment Flow

### 1. Access Your App via ngrok
- Open your browser and go to your ngrok HTTPS URL
- Example: `https://abc123.ngrok-free.app`

### 2. Complete Payment Flow
1. Register/Login as a player
2. Browse games and add to cart
3. Go to checkout
4. Click "Pay with VNPay"
5. Complete payment using VNPay sandbox credentials:
   - **Card Number**: 9704198526191432198
   - **Card Owner**: NGUYEN VAN A
   - **Expiry Date**: 07/15
   - **Bank**: NCB
   - **OTP**: 123456

### 3. Verify Return Flow
- After successful payment, VNPay will redirect back to your app
- You should see the success page and registration should be created
- Check the database for the new registration record

## Common Issues and Solutions

### Issue: ngrok URL Changes
**Problem**: ngrok generates new URLs each time you restart it (free tier)

**Solution**: 
- Update `appsettings.json` with the new URL each time
- Consider upgrading to ngrok paid plan for persistent domains

### Issue: "ngrok not found" Error
**Problem**: ngrok executable not in PATH

**Solution**:
```bash
# Add ngrok to PATH or use full path
./ngrok http 5045
# OR move ngrok to a folder in PATH
```

### Issue: VNPay Returns Error
**Problem**: Return URL doesn't match or isn't accessible

**Solution**:
- Ensure ngrok tunnel is running
- Verify return URL in appsettings.json matches ngrok URL exactly
- Check that your app is running when VNPay tries to return

### Issue: SSL Certificate Errors
**Problem**: Browser shows SSL warnings

**Solution**:
- Click "Advanced" → "Proceed" in browser
- ngrok provides valid SSL certificates, warnings are usually safe to ignore for testing

## Best Practices

### 1. Development Workflow
```bash
# Terminal 1: Start your app
dotnet run --urls="http://localhost:5045"

# Terminal 2: Start ngrok
ngrok http 5045

# Update appsettings.json with ngrok URL
# Restart app if needed
```

### 2. Configuration Management
- Keep a backup of your original `appsettings.json`
- Use separate config for ngrok testing
- Don't commit ngrok URLs to git

### 3. Testing Checklist
- [ ] App running on localhost:5045
- [ ] ngrok tunnel active and showing traffic
- [ ] appsettings.json updated with ngrok URL
- [ ] VNPay credentials configured
- [ ] Browser can access app via ngrok HTTPS URL

## ngrok Alternatives

If you can't use ngrok, consider these alternatives:

### 1. localtunnel
```bash
npm install -g localtunnel
lt --port 5045 --subdomain gamehub-test
```

### 2. Cloudflare Tunnel
```bash
cloudflared tunnel --url http://localhost:5045
```

### 3. Using HTTPS in Development
Configure your app to use HTTPS directly:
```bash
dotnet run --urls="https://localhost:7191;http://localhost:5045"
```
But you'll need to configure VNPay to accept localhost SSL certificates.

## Production Deployment

When deploying to production:
- Use a real domain with valid SSL certificate
- Update VNPay configuration with production URLs
- Switch from sandbox to production VNPay environment
- Test thoroughly in production environment

## Monitoring and Debugging

### ngrok Web Interface
Access `http://127.0.0.1:4040` to see:
- Request/response logs
- Tunnel status
- Traffic metrics

### Application Logs
Monitor your application logs for VNPay return processing:
```bash
# View logs in real-time
dotnet run --urls="http://localhost:5045" | grep -i vnpay
```

### VNPay Sandbox Logs
Check your VNPay sandbox account for transaction logs and status updates.

This setup ensures reliable testing of VNPay integration with proper HTTPS support required by VNPay's return URL mechanism.
