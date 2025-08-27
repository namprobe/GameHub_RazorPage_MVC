# VNPay Integration Guide

## Overview
VNPayHelper provides a complete integration with VNPay payment gateway for processing online payments in the GameHub application.

## Configuration

### 1. AppSettings Configuration
Add the following configuration to your `appsettings.json`:

```json
{
  "VNPay": {
    "TmnCode": "YOUR_TMN_CODE",
    "HashSecret": "YOUR_HASH_SECRET", 
    "ReturnUrl": "https://yourdomain.com/vnpay/return",
    "PaymentUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "TimeOut": 900,
    "CardNumber": "9704198526191432198",
    "CardOwner": "NGUYEN VAN A", 
    "CardExpiry": "07/15",
    "CardBank": "NCB",
    "OTP": "123456"
  }
}
```

### 2. Service Registration
Register VNPayHelper in your DI container:

```csharp
services.AddScoped<IVNPayHelper, VNPayHelper>();
services.AddHttpContextAccessor();
```

## Usage Examples

### 1. Create Payment URL
```csharp
public class PaymentController : Controller
{
    private readonly IVNPayHelper _vnPayHelper;
    
    public PaymentController(IVNPayHelper vnPayHelper)
    {
        _vnPayHelper = vnPayHelper;
    }
    
    [HttpPost("create-payment")]
    public IActionResult CreatePayment(int orderId, decimal amount)
    {
        try
        {
            string paymentUrl = _vnPayHelper.CreatePaymentUrl(orderId, amount);
            return Ok(new { PaymentUrl = paymentUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }
}
```

### 2. Handle Payment Return
```csharp
[HttpGet("vnpay/return")]
public IActionResult PaymentReturn()
{
    try
    {
        // Verify payment response
        bool isValid = _vnPayHelper.VerifyPaymentResponse(Request.Query);
        
        if (!isValid)
        {
            return BadRequest("Invalid payment response");
        }
        
        // Get payment result
        var (isSuccess, message, transactionId) = _vnPayHelper.GetPaymentResult(Request.Query);
        
        if (isSuccess)
        {
            // Update order status to paid
            // Redirect to success page
            return RedirectToAction("PaymentSuccess", new { transactionId });
        }
        else
        {
            // Redirect to failure page
            return RedirectToAction("PaymentFailed", new { message });
        }
    }
    catch (Exception ex)
    {
        return BadRequest("Payment processing error");
    }
}
```

## Payment Flow

### 1. Payment Request
1. User initiates payment
2. System creates payment URL with VNPay parameters
3. User is redirected to VNPay payment page
4. User completes payment on VNPay

### 2. Payment Response
1. VNPay redirects user back to ReturnUrl
2. System verifies payment response signature
3. System processes payment result
4. User sees success/failure page

## VNPay Parameters

### Required Parameters for Payment Request:
- `vnp_Amount`: Amount in smallest currency unit (cents)
- `vnp_Command`: Always "pay"
- `vnp_CreateDate`: Current timestamp (yyyyMMddHHmmss)
- `vnp_CurrCode`: Currency code (VND)
- `vnp_IpAddr`: Client IP address
- `vnp_Locale`: Language (vn)
- `vnp_OrderInfo`: Order description
- `vnp_OrderType`: Order type (other)
- `vnp_ReturnUrl`: Return URL after payment
- `vnp_TmnCode`: Merchant code
- `vnp_TxnRef`: Transaction reference (order ID)
- `vnp_Version`: API version (2.1.0)
- `vnp_SecureHash`: HMAC-SHA512 signature

### Response Parameters:
- `vnp_ResponseCode`: Response code (00 = success)
- `vnp_TransactionStatus`: Transaction status (00 = success)
- `vnp_TransactionNo`: VNPay transaction number
- `vnp_BankCode`: Bank code
- `vnp_BankTranNo`: Bank transaction number
- `vnp_PayDate`: Payment date
- `vnp_SecureHash`: Response signature

## Security Features

### 1. HMAC-SHA512 Signature
- All requests and responses are signed with HMAC-SHA512
- Prevents tampering and ensures data integrity
- Uses HashSecret from configuration

### 2. Parameter Validation
- Validates all required parameters
- Checks for null/empty values
- Ensures proper data types

### 3. Response Verification
- Verifies response signature
- Validates transaction status
- Checks response codes

## Error Handling

### Common Error Codes:
- `00`: Success
- `01`: Invalid signature
- `02`: Invalid order info
- `03`: Invalid amount
- `04`: Invalid currency
- `05`: Invalid transaction reference
- `06`: Invalid merchant code
- `07`: Invalid IP address
- `08`: Invalid return URL
- `09`: Invalid command
- `10`: Invalid version
- `11`: Invalid locale
- `12`: Invalid order type
- `13`: Invalid create date
- `14`: Invalid timeout
- `15`: Invalid card type
- `16`: Invalid card number
- `17`: Invalid card owner
- `18`: Invalid card expiry
- `19`: Invalid OTP
- `20`: Invalid bank code
- `21`: Invalid bank tran no
- `22`: Invalid pay date
- `23`: Invalid transaction no
- `24`: Invalid transaction status
- `25`: Invalid response code

## Testing

### Sandbox Environment:
- Use sandbox URL: `https://sandbox.vnpayment.vn/paymentv2/vpcpay.html`
- Test with provided card details
- Use test TMN code and HashSecret

### Test Card Details:
- Card Number: 9704198526191432198
- Card Owner: NGUYEN VAN A
- Card Expiry: 07/15
- Bank: NCB
- OTP: 123456

## Production Deployment

### 1. Update Configuration:
- Change PaymentUrl to production URL
- Update TMN code and HashSecret
- Set proper ReturnUrl

### 2. Security Considerations:
- Store HashSecret securely
- Use HTTPS for all URLs
- Validate all input parameters
- Log payment activities
- Implement proper error handling

## Troubleshooting

### Common Issues:

1. **Invalid Signature Error**
   - Check HashSecret configuration
   - Verify parameter ordering
   - Ensure all required parameters are present

2. **Payment Timeout**
   - Check TimeOut configuration
   - Verify network connectivity
   - Check VNPay service status

3. **Invalid Amount Error**
   - Ensure amount is in smallest currency unit
   - Check decimal precision
   - Verify currency code

4. **Return URL Issues**
   - Ensure ReturnUrl is accessible
   - Check URL encoding
   - Verify HTTPS requirement

## Support

For VNPay technical support:
- Email: hotrovnpay@vnpay.vn
- Phone: 1900.5555.77
- Website: https://vnpay.vn
