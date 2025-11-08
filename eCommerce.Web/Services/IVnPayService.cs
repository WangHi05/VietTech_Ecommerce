using eCommerce.Web.Models.VNPay;
using Microsoft.AspNetCore.Http;

namespace eCommerce.Web.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(VnPayRequestModel model, HttpContext context);
        VnPayResponseModel PaymentExecute(IQueryCollection collections);
    }
}