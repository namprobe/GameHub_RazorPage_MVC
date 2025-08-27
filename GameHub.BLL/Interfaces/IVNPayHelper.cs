using Microsoft.AspNetCore.Http;

namespace GameHub.BLL.Interfaces;

public interface IVNPayHelper
{
    string CreatePaymentUrl(int orderId, decimal amount);
    bool VerifyPaymentResponse(IQueryCollection queryParams);
}