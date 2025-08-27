using System.Security.Cryptography;
using System.Text;
using GameHub.BLL.Interfaces;
using GameHub.BLL.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace GameHub.BLL.Helpers;

public class VNPayHelper : IVNPayHelper
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly VnPayLibrary _vnPayLibrary;
    
    public VNPayHelper(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _httpClient = new HttpClient();
        _httpContextAccessor = httpContextAccessor;
        _vnPayLibrary = new VnPayLibrary(httpContextAccessor);
    }

    public string CreatePaymentUrl(int orderId, decimal amount)
    {
        string baseUrl = _configuration["VNPay:PaymentUrl"] ?? throw new InvalidOperationException("VNPay:PaymentUrl not configured");
        string vnp_HashSecret = _configuration["VNPay:HashSecret"] ?? throw new InvalidOperationException("VNPay:HashSecret not configured");
        string vnp_ReturnUrl = _configuration["VNPay:ReturnUrl"] ?? throw new InvalidOperationException("VNPay:ReturnUrl not configured");
        string vnp_TmnCode = _configuration["VNPay:TmnCode"] ?? throw new InvalidOperationException("VNPay:TmnCode not configured");
        
        // Clear previous data
        _vnPayLibrary.ClearRequestData();
        
        // Add required parameters
        _vnPayLibrary.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
        _vnPayLibrary.AddRequestData("vnp_Command", "pay");
        _vnPayLibrary.AddRequestData("vnp_TmnCode", vnp_TmnCode);
        _vnPayLibrary.AddRequestData("vnp_Amount", (amount * 100).ToString()); // Convert to smallest currency unit
        _vnPayLibrary.AddRequestData("vnp_CurrCode", "VND");
        _vnPayLibrary.AddRequestData("vnp_BankCode", "");
        // VN requires vnp_TxnRef = GameRegistration.Id (orderId here is registration id)
        _vnPayLibrary.AddRequestData("vnp_TxnRef", orderId.ToString());
        _vnPayLibrary.AddRequestData("vnp_OrderInfo", $"Thanh toan dang ky game :{orderId}");
        _vnPayLibrary.AddRequestData("vnp_OrderType", "other");
        _vnPayLibrary.AddRequestData("vnp_Locale", "vn");
        _vnPayLibrary.AddRequestData("vnp_ReturnUrl", vnp_ReturnUrl);
        _vnPayLibrary.AddRequestData("vnp_IpAddr", _vnPayLibrary.GetClientIpAddress());
        _vnPayLibrary.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
        
        // Create payment URL
        return _vnPayLibrary.CreateRequestUrl(baseUrl, vnp_HashSecret);
    }
    
    public (bool IsSuccess, string Message, string TransactionId) GetPaymentResult(IQueryCollection queryParams)
    {
        try
        {
            var responseCode = queryParams["vnp_ResponseCode"].ToString();
            var transactionStatus = queryParams["vnp_TransactionStatus"].ToString();
            var transactionId = queryParams["vnp_TransactionNo"].ToString();
            var orderInfo = queryParams["vnp_OrderInfo"].ToString();
            
            // Check if payment was successful
            bool isSuccess = responseCode == "00" && transactionStatus == "00";
            
            string message = isSuccess 
                ? "Thanh toán thành công" 
                : $"Thanh toán thất bại. Mã lỗi: {responseCode}";
            
            return (isSuccess, message, transactionId);
        }
        catch
        {
            return (false, "Không thể xử lý kết quả thanh toán", string.Empty);
        }
    }

    public bool VerifyPaymentResponse(IQueryCollection queryParams)
    {
        try
        {
            string vnp_HashSecret = _configuration["VNPay:HashSecret"] ?? throw new InvalidOperationException("VNPay:HashSecret not configured");
            
            // Clear previous response data
            _vnPayLibrary.ClearResponseData();
            
            // Add all response parameters
            foreach (var param in queryParams)
            {
                _vnPayLibrary.AddResponseData(param.Key, param.Value.ToString());
            }
            
            // Get the secure hash from response
            string inputHash = _vnPayLibrary.GetResponseData("vnp_SecureHash");
            
            // Validate signature
            return _vnPayLibrary.ValidateSignature(inputHash, vnp_HashSecret);
        }
        catch
        {
            return false;
        }
    }
}