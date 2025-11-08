using eCommerce.Web.Models.VNPay;
using eCommerce.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Web.Controllers
{
    public class VnPayController : Controller
    {
        private readonly IVnPayService _vnPayService;

        public VnPayController(IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        [HttpPost]
        public IActionResult CreatePayment(VnPayRequestModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            return Redirect(url);
        }
    }
}