using eCommerce.Core.Entities;
using eCommerce.Web.Services;
using System.Collections.Generic;

namespace eCommerce.Web.Builders
{
    public class CheckoutRequestBuilder
    {
        private readonly CheckoutRequest _request = new();

        public CheckoutRequestBuilder SetCustomer(string? name, string? address, string? country, string? province)
        {
            _request.ShippingName = name ?? string.Empty;
            _request.ShippingAddress = address ?? string.Empty;
            _request.ShippingCountry = country ?? string.Empty;
            _request.ShippingProvince = province ?? string.Empty;
            return this;
        }

        public CheckoutRequestBuilder SetUser(string? userId, string? userName)
        {
            _request.UserId = userId;
            _request.UserName = userName ?? "Guest";
            return this;
        }

        public CheckoutRequestBuilder SetOrderDetails(List<CartItem> items, string method, string shippingMethod)
        {
            _request.Items = items;
            _request.PaymentMethod = method;
            _request.ShippingMethod = shippingMethod;
            return this;
        }

        public CheckoutRequestBuilder SetPricing(decimal subTotal, decimal discount, decimal pointsDiscount, decimal shippingFee, decimal total)
        {
            _request.SubTotal = subTotal;
            _request.Discount = discount;
            _request.PointsDiscount = pointsDiscount;
            _request.ShippingFee = shippingFee;
            _request.Total = total;
            return this;
        }

        public CheckoutRequestBuilder SetPromotion(string? voucherCode, int pointsToRedeem)
        {
            _request.VoucherCode = voucherCode;
            _request.PointsToRedeem = pointsToRedeem;
            return this;
        }

        public CheckoutRequest Build() => _request;
    }
}