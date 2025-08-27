# VnPayLibrary Utility

## Overview
VnPayLibrary is a utility class that provides VNPay payment integration functionality for ASP.NET Core applications. This library has been updated from .NET Framework to be compatible with .NET Core/5+.

## Key Features

### ✅ **ASP.NET Core Compatibility**
- Updated from `HttpContext.Current` to `IHttpContextAccessor`
- Proper dependency injection support
- Nullable reference types support

### ✅ **Payment URL Generation**
- Creates VNPay payment URLs with proper parameters
- Automatic HMAC-SHA512 signature generation
- Parameter sorting and URL encoding

### ✅ **Response Verification**
- Validates VNPay payment responses
- Signature verification for security
- Response data parsing

### ✅ **IP Address Detection**
- Supports multiple IP detection methods
- Handles proxy and load balancer scenarios
- Fallback to default IP if detection fails

## Migration from .NET Framework

### Before (.NET Framework):
```csharp
// Old way - using HttpContext.Current
string ipAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
```

### After (.NET Core):
```csharp
// New way - using IHttpContextAccessor
public class VnPayLibrary
{
    private readonly IHttpContextAccessor? _httpContextAccessor;
    
    public VnPayLibrary(IHttpContextAccessor? httpContextAccessor = null)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public string GetClientIpAddress()
    {
        return Utils.GetIpAddress(_httpContextAccessor?.HttpContext);
    }
}
```

## Usage Examples

### 1. Basic Setup
```csharp
// Register in DI container
services.AddHttpContextAccessor();
services.AddScoped<VnPayLibrary>();

// Or use directly
var vnPayLibrary = new VnPayLibrary(httpContextAccessor);
```

### 2. Create Payment URL
```csharp
var vnPay = new VnPayLibrary(httpContextAccessor);

// Add required parameters
vnPay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
vnPay.AddRequestData("vnp_Command", "pay");
vnPay.AddRequestData("vnp_TmnCode", "YOUR_TMN_CODE");
vnPay.AddRequestData("vnp_Amount", "1806000"); // Amount in cents
vnPay.AddRequestData("vnp_CurrCode", "VND");
vnPay.AddRequestData("vnp_TxnRef", "12345");
vnPay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang :12345");
vnPay.AddRequestData("vnp_OrderType", "other");
vnPay.AddRequestData("vnp_Locale", "vn");
vnPay.AddRequestData("vnp_ReturnUrl", "https://yourdomain.com/vnpay/return");
vnPay.AddRequestData("vnp_IpAddr", vnPay.GetClientIpAddress());
vnPay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));

// Create payment URL
string paymentUrl = vnPay.CreateRequestUrl("https://sandbox.vnpayment.vn/paymentv2/vpcpay.html", "YOUR_HASH_SECRET");
```

### 3. Verify Payment Response
```csharp
// Add response data from query parameters
foreach (var param in Request.Query)
{
    vnPay.AddResponseData(param.Key, param.Value.ToString());
}

// Get secure hash from response
string inputHash = vnPay.GetResponseData("vnp_SecureHash");

// Validate signature
bool isValid = vnPay.ValidateSignature(inputHash, "YOUR_HASH_SECRET");

if (isValid)
{
    // Process successful payment
    string responseCode = vnPay.GetResponseData("vnp_ResponseCode");
    string transactionStatus = vnPay.GetResponseData("vnp_TransactionStatus");
    
    if (responseCode == "00" && transactionStatus == "00")
    {
        // Payment successful
    }
}
```

## IP Address Detection

The library supports multiple methods for detecting client IP addresses:

### 1. X-Forwarded-For Header
```csharp
// For proxy/load balancer scenarios
httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
```

### 2. X-Real-IP Header
```csharp
// Alternative real IP header
httpContext.Request.Headers["X-Real-IP"].FirstOrDefault()
```

### 3. Remote IP Address
```csharp
// Direct connection IP
httpContext.Connection.RemoteIpAddress?.ToString()
```

### 4. Fallback
```csharp
// Default IP if detection fails
"127.0.0.1"
```

## Security Features

### ✅ **HMAC-SHA512 Signature**
- All requests and responses are signed
- Prevents tampering and ensures integrity
- Uses HashSecret for signature generation

### ✅ **Parameter Validation**
- Validates all required parameters
- Checks for null/empty values
- Ensures proper data types

### ✅ **Response Verification**
- Verifies response signatures
- Validates transaction status
- Checks response codes

## Error Handling

### Common Error Scenarios:
1. **Missing HttpContext**: Falls back to default IP
2. **Invalid Parameters**: Returns empty string for missing data
3. **Signature Mismatch**: Returns false for validation
4. **Network Issues**: Graceful degradation

### Best Practices:
```csharp
try
{
    var vnPay = new VnPayLibrary(httpContextAccessor);
    string paymentUrl = vnPay.CreateRequestUrl(baseUrl, hashSecret);
    return paymentUrl;
}
catch (Exception ex)
{
    // Log error and return fallback
    _logger.LogError(ex, "Error creating VNPay URL");
    return null;
}
```

## Configuration

### Required Settings:
```json
{
  "VNPay": {
    "TmnCode": "YOUR_TMN_CODE",
    "HashSecret": "YOUR_HASH_SECRET",
    "ReturnUrl": "https://yourdomain.com/vnpay/return",
    "PaymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"
  }
}
```

### Service Registration:
```csharp
// Program.cs or Startup.cs
services.AddHttpContextAccessor();
services.AddScoped<VnPayLibrary>();
```

## Testing

### Sandbox Environment:
- Use sandbox URL for testing
- Test with provided card details
- Verify signature generation

### Test Card Details:
- Card Number: 9704198526191432198
- Card Owner: NGUYEN VAN A
- Card Expiry: 07/15
- Bank: NCB
- OTP: 123456

## Troubleshooting

### Common Issues:

1. **HttpContext.Current Error**
   - Solution: Use IHttpContextAccessor instead
   - Register HttpContextAccessor in DI container

2. **IP Address Detection Issues**
   - Check proxy/load balancer configuration
   - Verify X-Forwarded-For headers
   - Use fallback IP if needed

3. **Signature Validation Failures**
   - Verify HashSecret configuration
   - Check parameter ordering
   - Ensure all required parameters are present

4. **Null Reference Exceptions**
   - Add null checks for HttpContext
   - Use nullable reference types
   - Provide fallback values

## Performance Considerations

### ✅ **Optimizations:**
- Reuse VnPayLibrary instances
- Clear data between requests
- Use proper async/await patterns
- Minimize string allocations

### ✅ **Memory Management:**
- Clear request/response data after use
- Dispose of resources properly
- Use StringBuilder for large strings

## Support

For VNPay technical support:
- Email: hotrovnpay@vnpay.vn
- Phone: 1900.5555.77
- Website: https://vnpay.vn

## License

This library is provided as-is for educational and development purposes.
