using System.Security.Claims;
using eCommerce.Application.Services;
using eCommerce.Core.Entities;
using eCommerce.Web.Pages;
using eCommerce.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Moq;

namespace eCommerce.Web.Tests;

public class CheckoutModelTests
{
    [Fact]
    public async Task OnPostAsync_WithValidCard_ShowsOtpRedirect()
    {
        var cartItems = new List<CartItem>
        {
            new()
            {
                ProductId = 1,
                Name = "Laptop",
                Price = 15_000_000m,
                Quantity = 1
            }
        };

        var cartMock = new Mock<ICartService>();
        cartMock.Setup(c => c.GetCartAsync()).ReturnsAsync(cartItems);
        cartMock.Setup(c => c.GetAppliedVoucherAsync()).ReturnsAsync((string?)null);
        cartMock.Setup(c => c.GetShippingAsync()).ReturnsAsync(30_000m);

        Order? capturedOrder = null;
        var orderMock = new Mock<IOrderService>();
        orderMock
            .Setup(o => o.PlaceOrderAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) =>
            {
                capturedOrder = o;
                return 123;
            });

        var configMock = new Mock<IConfiguration>();

        var model = CreateCheckoutModel(cartMock.Object, orderMock.Object, configMock.Object);
        model.PaymentMethod = "card";
        model.ShippingName = "Nguyen Van A";
        model.ShippingAddress = "123 Le Loi";
        model.Country = "Vietnam";
        model.Province = "Ho Chi Minh";
        model.CardName = "Nguyen Van A";
        model.CardNumber = "4111 1111 1111 1111";
        model.CardExpiry = "12/30";
        model.CardCvc = "123";

        var result = await model.OnPostAsync();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Payment/CardOtp", redirect.PageName);
        Assert.Equal(123, redirect.RouteValues!["orderId"]);

        Assert.NotNull(capturedOrder);
        Assert.Equal("Card", capturedOrder!.PaymentMethod);
        Assert.Equal("Pending", capturedOrder.PaymentStatus);
        Assert.Equal("Pending", capturedOrder.Status);
        Assert.Equal("Nguyen Van A", capturedOrder.CardHolderName);
        Assert.Equal("1111", capturedOrder.CardLast4);
        Assert.Equal(cartItems.Count, capturedOrder.Items.Count);
        orderMock.Verify(o => o.PlaceOrderAsync(It.IsAny<Order>()), Times.Once);
        cartMock.Verify(c => c.ClearCartAsync(), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WithInvalidCard_ReturnsPageWithErrors()
    {
        var cartItems = new List<CartItem>
        {
            new()
            {
                ProductId = 1,
                Name = "Laptop",
                Price = 15_000_000m,
                Quantity = 1
            }
        };

        var cartMock = new Mock<ICartService>();
        cartMock.Setup(c => c.GetCartAsync()).ReturnsAsync(cartItems);
        cartMock.Setup(c => c.GetAppliedVoucherAsync()).ReturnsAsync((string?)null);
        cartMock.Setup(c => c.GetShippingAsync()).ReturnsAsync(30_000m);

        var orderMock = new Mock<IOrderService>(MockBehavior.Strict);
        var configMock = new Mock<IConfiguration>();

        var model = CreateCheckoutModel(cartMock.Object, orderMock.Object, configMock.Object);
        model.PaymentMethod = "card";
        model.ShippingName = "Nguyen Van A";
        model.ShippingAddress = "123 Le Loi";
        model.Country = "Vietnam";
        model.Province = "Ho Chi Minh";
        model.CardName = string.Empty;
        model.CardNumber = "123";
        model.CardExpiry = "01/20";
        model.CardCvc = "99";

        var result = await model.OnPostAsync();

        Assert.IsType<PageResult>(result);
        Assert.False(model.ModelState.IsValid);
        Assert.True(model.ModelState.ContainsKey(nameof(model.CardName)));
        Assert.True(model.ModelState.ContainsKey(nameof(model.CardNumber)));
        Assert.True(model.ModelState.ContainsKey(nameof(model.CardExpiry)));
        Assert.True(model.ModelState.ContainsKey(nameof(model.CardCvc)));

        orderMock.Verify(o => o.PlaceOrderAsync(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task OnPostAsync_WithCod_RedirectsToSuccessPageAndClearsCart()
    {
        var cartItems = new List<CartItem>
        {
            new()
            {
                ProductId = 42,
                Name = "Tai nghe",
                Price = 500_000m,
                Quantity = 2
            }
        };

        var cartMock = new Mock<ICartService>();
        cartMock.Setup(c => c.GetCartAsync()).ReturnsAsync(cartItems);
        cartMock.Setup(c => c.GetAppliedVoucherAsync()).ReturnsAsync((string?)null);
        cartMock.Setup(c => c.GetShippingAsync()).ReturnsAsync(0m);
        cartMock.Setup(c => c.ClearCartAsync()).Returns(Task.CompletedTask);

        Order? capturedOrder = null;
        var orderMock = new Mock<IOrderService>();
        orderMock
            .Setup(o => o.PlaceOrderAsync(It.IsAny<Order>()))
            .ReturnsAsync((Order o) =>
            {
                capturedOrder = o;
                return 777;
            });

        var model = CreateCheckoutModel(cartMock.Object, orderMock.Object, Mock.Of<IConfiguration>());
        model.PaymentMethod = "cod";
        model.ShippingName = "Tran Thi B";
        model.ShippingAddress = "456 Hai Ba Trung";
        model.Country = "Vietnam";
        model.Province = "Ha Noi";

        var result = await model.OnPostAsync();

        var redirect = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("/Payment/Result", redirect.PageName);
        Assert.Equal(777, redirect.RouteValues!["orderId"]);
        Assert.Equal(true, redirect.RouteValues!["success"]);
        Assert.Equal("cod", redirect.RouteValues!["method"]);

        Assert.NotNull(capturedOrder);
        Assert.Equal("COD", capturedOrder!.PaymentMethod);
        Assert.Equal("AwaitingPayment", capturedOrder.PaymentStatus);
        Assert.Equal("Pending", capturedOrder.Status);
        Assert.Equal(cartItems.Count, capturedOrder.Items.Count);

        orderMock.Verify(o => o.PlaceOrderAsync(It.IsAny<Order>()), Times.Once);
        cartMock.Verify(c => c.ClearCartAsync(), Times.Once);
    }

    private static CheckoutModel CreateCheckoutModel(ICartService cart, IOrderService order, IConfiguration config)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-1")
        }, "TestAuth"));

        var modelState = new ModelStateDictionary();
        var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
        var pageContext = new PageContext(actionContext)
        {
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), modelState)
        };

        var model = new CheckoutModel(cart, order, config)
        {
            PageContext = pageContext,
            TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>())
        };

        return model;
    }
}
