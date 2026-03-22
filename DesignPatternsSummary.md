# Design Patterns Summary - eCommerce Project

**Ngày quét:** 22/03/2026 (Sau khi implement Visitor Pattern)

**Tổng số pattern tìm được:** 20

---

## 📊 Thống kê nhanh

| Loại | Số lượng | Patterns |
|------|---------|----------|
| **Creational** | 4 | Builder, Factory Method, Simple Factory, Singleton |
| **Structural** | 7 | Adapter, Decorator, Facade, Proxy, Repository, Composite, Flyweight |
| **Behavioral** | 9 | Chain of Responsibility, Command, Iterator, Observer, State, Strategy, Template Method, Mediator, Visitor |
| **TOTAL** | **20** | ۞ |

---



## Creational Patterns

### Pattern: Builder

**Mô tả ngắn:**
Xây dựng object phức tạp từng bước thông qua các method chaining. Dùng để tạo `CheckoutRequest` với nhiều tham số theo trình tự rõ ràng.

**Các file liên quan:**

- eCommerce.Web/Builders/CheckoutRequestBuilder.cs

**Vai trò các class:**

- `CheckoutRequestBuilder`: Builder class - cung cấp các method Set* để thiết lập từng thuộc tính của CheckoutRequest, trả về `this` để cho phép chaining

---

### Pattern: Factory Method (Abstract Factory)

**Mô tả ngắn:**
Tạo các loại discount (percentage, fixed amount) qua các phương thức factory được định nghĩa trong abstract creator. Cho phép subclass quyết định class nào sẽ được tạo.

**Các file liên quan:**

- eCommerce.Application/DesignPatterns/FactoryMethod/DiscountCreator.cs
- eCommerce.Application/DesignPatterns/FactoryMethod/PercentageDiscountCreator.cs
- eCommerce.Application/DesignPatterns/FactoryMethod/FixedDiscountCreator.cs
- eCommerce.Application/DesignPatterns/FactoryMethod/IDiscountCalculator.cs
- eCommerce.Application/DesignPatterns/FactoryMethod/PercentageDiscount.cs
- eCommerce.Application/DesignPatterns/FactoryMethod/FixedAmountDiscount.cs
- eCommerce.Application/DesignPatterns/FactoryMethod/ExampleUsage.cs

**Vai trò các class:**

- `DiscountCreator`: Abstract creator - định nghĩa abstract method `CreateDiscount()`
- `PercentageDiscountCreator`: Concrete creator - tạo PercentageDiscount object
- `FixedDiscountCreator`: Concrete creator - tạo FixedAmountDiscount object
- `IDiscountCalculator`: Product interface
- `PercentageDiscount`: Concrete product
- `FixedAmountDiscount`: Concrete product

---

### Pattern: Simple Factory

**Mô tả ngắn:**
Tạo các phương thức vận chuyển (Standard, Express, Overnight) dựa vào tên loại được truyền vào. Logic tạo object được đóng gói trong static factory method.

**Các file liên quan:**

- eCommerce.Application/DesignPatterns/SimpleFactory/ShippingFactory.cs
- eCommerce.Application/DesignPatterns/SimpleFactory/IShippingMethod.cs
- eCommerce.Application/DesignPatterns/SimpleFactory/StandardShipping.cs
- eCommerce.Application/DesignPatterns/SimpleFactory/ExpressShipping.cs
- eCommerce.Application/DesignPatterns/SimpleFactory/OvernightShipping.cs
- eCommerce.Application/DesignPatterns/SimpleFactory/ExampleUsage.cs

**Vai trò các class:**

- `ShippingFactory`: Factory class - cung cấp static method `CreateShippingMethod(type)` để tạo shipping object
- `IShippingMethod`: Product interface
- `StandardShipping`, `ExpressShipping`, `OvernightShipping`: Concrete products

---

### Pattern: Singleton

**Mô tả ngắn:**
Đảm bảo chỉ có một instance duy nhất của logger trong toàn bộ ứng dụng. Dùng `Lazy<T>` đảm bảo thread-safe lazy initialization.

**Các file liên quan:**

- eCommerce.Application/DesignPatterns/Singleton/AppLoggerSingleton.cs
- eCommerce.Application/DesignPatterns/DesignPatternDemo.cs

**Vai trò các class:**

- `AppLoggerSingleton`: Singleton class - cung cấp property static `Instance` duy nhất, constructor private, sử dụng `Lazy<T>` để khởi tạo an toàn

---

## Structural Patterns

### Pattern: Adapter

**Mô tả ngắn:**
Adapt giao diện của Twilio SMS service (TwilioSmsService) để phù hợp với giao diện `ISmsService` mà hệ thống mong đợi.

**Các file liên quan:**

- eCommerce.Application/DesignPatterns/Adapter/ISmsService.cs
- eCommerce.Application/DesignPatterns/Adapter/TwilioSmsAdapter.cs
- eCommerce.Application/DesignPatterns/Adapter/TwilioSmsService.cs
- eCommerce.Application/DesignPatterns/Adapter/ExampleUsage.cs

**Vai trò các class:**

- `ISmsService`: Target interface - giao diện mà client mong đợi
- `TwilioSmsAdapter`: Adapter class - wrap TwilioSmsService để phù hợp với ISmsService
- `TwilioSmsService`: Adaptee - giao diện gốc từ bên thứ ba

---

### Pattern: Decorator

**Mô tả ngắn:**
Tính giá cuối cùng của đơn hàng bằng cách "dán" các decorator lên nhau: giá cơ bản → thêm voucher → thêm shipping fee → thêm điểm tích lũy. Mỗi decorator thêm thuộc tính hay hành vi vào object gốc.

**Các file liên quan:**

- eCommerce.Application/Decorators/PriceCalculators/IPriceCalculator.cs
- eCommerce.Application/Decorators/PriceCalculators/BasePriceCalculator.cs
- eCommerce.Application/Decorators/PriceCalculators/LoyaltyDecorator.cs
- eCommerce.Application/Decorators/PriceCalculators/ShippingDecorator.cs
- eCommerce.Application/Decorators/PriceCalculators/VoucherDecorator.cs

**Vai trò các class:**

- `IPriceCalculator`: Component interface
- `BasePriceCalculator`: Concrete component - trả về giá cơ bản
- `LoyaltyDecorator`: Decorator - thêm discount từ điểm tích lũy
- `ShippingDecorator`: Decorator - thêm phí vận chuyển
- `VoucherDecorator`: Decorator - thêm discount từ voucher

---

### Pattern: Facade

**Mô tả ngfrankfurtọn:**
Cung cấp giao diện đơn giản `ICheckoutFacade` để bọc quy trình checkout phức tạp gồm 5 subsystem: OrderService, StockService, LoyaltyService, CartService, AppDbContext.

**Các file liên quan:**

- eCommerce.Web/Services/ICheckoutFacade.cs
- eCommerce.Web/Services/CheckoutFacade.cs
- eCommerce.Web/Pages/Checkout.cshtml.cs (sử dụng)
- eCommerce.Web/Program.cs (đăng ký DI)

**Vai trò các class:**

- `ICheckoutFacade`: Facade interface - định nghĩa method `PlaceOrderAsync()`
- `CheckoutFacade`: Concrete facade - điều phối giữa 5 subsystem: kiểm tra tồn kho, tạo đơn hàng, cập nhật điểm, xử lý voucher, xóa giỏ hàng

---

### Pattern: Proxy

**Mô tả ngắn:**
ProductServiceProxy intercept request tới ProductService, kiểm tra cache trước. Nếu dữ liệu có trong cache thì trả về cache; nếu không thì gọi service thật và lưu vào cache.

**Các file liên quan:**

- eCommerce.Application/Services/ProductServiceProxy.cs
- eCommerce.Application/Services/ProductService.cs (service thật)
- eCommerce.Application/Services/IProductService.cs (interface)
- eCommerce.Web/Program.cs (đăng ký DI - proxy thay thế service thật)

**Vai trò các class:**

- `IProductService`: Interface chung
- `ProductService`: Real service - xử lý logic thật
- `ProductServiceProxy`: Proxy - bọc ProductService, thêm cache logic

---

### Pattern: Repository

**Mô tả ngắn:**
Cung cấp giao diện uniform để truy cập dữ liệu từ database. Tách biệt logic data access khỏi business logic, dễ dàng mock khi test và thay thế database backend.

**Các file liên quan:**

- eCommerce.Core/Interfaces/IProductRepository.cs
- eCommerce.Infrastructure/Data/ProductRepository.cs
- eCommerce.Core/Interfaces/IOrderRepository.cs
- eCommerce.Infrastructure/Data/OrderRepository.cs
- eCommerce.Core/Interfaces/ICategoryRepository.cs
- eCommerce.Infrastructure/Data/CategoryRepository.cs
- eCommerce.Core/Interfaces/IBrandRepository.cs
- eCommerce.Infrastructure/Data/BrandRepository.cs

**Vai trò các class:**

- `IProductRepository`, `IOrderRepository`, `ICategoryRepository`, `IBrandRepository`: Repository interfaces - định nghĩa contract CRUD
- `ProductRepository`, `OrderRepository`, `CategoryRepository`, `BrandRepository`: Concrete repositories - triển khai access dữ liệu qua EF Core

---

## Behavioral Patterns

### Pattern: Chain of Responsibility

**Mô tả ngắn:**
Tạo chuỗi các handler để validation đơn hàng: validation dữ liệu → kiểm tra tồn kho → kiểm tra thanh toán. Mỗi handler quyết định xử lý hay chuyển sang handler tiếp theo.

**Các file liên quan:**

- eCommerce.Application/DesignPatterns/ChainOfResponsibility/OrderHandler.cs
- eCommerce.Application/DesignPatterns/ChainOfResponsibility/OrderValidationHandler.cs
- eCommerce.Application/DesignPatterns/ChainOfResponsibility/StockCheckHandler.cs
- eCommerce.Application/DesignPatterns/ChainOfResponsibility/PaymentCheckHandler.cs
- eCommerce.Application/DesignPatterns/DesignPatternDemo.cs (demo)

**Vai trò các class:**

- `OrderHandler`: Abstract handler - định nghĩa interface `Handle()`, chứa reference đến `_next` handler
- `OrderValidationHandler`: Concrete handler - validation dữ liệu order
- `StockCheckHandler`: Concrete handler - kiểm tra tồn kho
- `PaymentCheckHandler`: Concrete handler - kiểm tra trạng thái thanh toán (cuối chuỗi)

---

### Pattern: Command

**Mô tả ngắn:**
Đóng gói yêu cầu tạo đơn hàng thành object Command. Cho phép hệ thống queue command, retry, audit log, hoặc undo/redo trong tương lai.

**Các file liên quan:**

- eCommerce.Application/DesignPatterns/Command/ICommand.cs
- eCommerce.Application/DesignPatterns/Command/CreateOrderCommand.cs
- eCommerce.Application/DesignPatterns/Command/CommandInvoker.cs
- eCommerce.Application/DesignPatterns/DesignPatternDemo.cs (demo)

**Vai trò các class:**

- `ICommand`: Command interface - định nghĩa method `Execute()`
- `CreateOrderCommand`: Concrete command - đóng gói request tạo đơn hàng
- `CommandInvoker`: Invoker - thực thi command(s) từ client

---

### Pattern: Iterator

**Mô tả ngắn:**
Cung cấp cách duyệt qua danh sách Order mà không cần tiết lộ cấu trúc nội bộ. OrderCollection có thể tạo ra các loại iterator khác nhau (toàn bộ, theo status, theo payment status, theo date range).

**Các file liên quan:**

- eCommerce.Application/DesignPatterns/Iterator/IIterator.cs
- eCommerce.Application/DesignPatterns/Iterator/IAggregate.cs
- eCommerce.Application/DesignPatterns/Iterator/OrderCollection.cs
- eCommerce.Application/DesignPatterns/Iterator/OrderIterator.cs
- eCommerce.Application/DesignPatterns/Iterator/OrderStatusIterator.cs
- eCommerce.Application/DesignPatterns/Iterator/OrderPaymentStatusIterator.cs
- eCommerce.Application/DesignPatterns/Iterator/OrderDateRangeIterator.cs
- eCommerce.Web/Controllers/OrdersController.cs (sử dụng)

**Vai trò các class:**

- `IIterator<T>`: Iterator interface - định nghĩa `Current`, `MoveNext()`, `Reset()`
- `IAggregate<T>`: Aggregate interface - định nghĩa `CreateIterator()`
- `OrderCollection`: Concrete aggregate - chứa danh sách Order, tạo các loại iterator
- `OrderIterator`, `OrderStatusIterator`, `OrderPaymentStatusIterator`, `OrderDateRangeIterator`: Concrete iterators - duyệt order theo cách khác nhau

---

### Pattern: Observer

**Mô tả ngắn:**
OrderService làm Subject, khi trạng thái thanh toán hoặc status của đơn hàng thay đổi, nó sẽ thông báo cho các Observer (tích điểm, gửi thông báo khách hàng). Lkiết couple giữa OrderService và các business logic phụ.

**Các file liên quan:**

- eCommerce.Core/Interfaces/IOrderSubject.cs
- eCommerce.Core/Interfaces/IOrderObserver.cs
- eCommerce.Application/Services/OrderService.cs (Subject)
- eCommerce.Application/Observers/LoyaltyOrderObserver.cs
- eCommerce.Application/Observers/CustomerNotificationObserver.cs
- eCommerce.Web/Program.cs (đăng ký Observer vào Subject)

**Vai trò các class:**

- `IOrderSubject`: Subject interface - `Attach()`, `Detach()`, `Notify...()`
- `IOrderObserver`: Observer interface - `OrderPaymentStatusChangedAsync()`, `OrderStatusChangedAsync()`
- `OrderService`: Concrete subject - implements IOrderSubject, manage list of observers, notify them khi state change
- `LoyaltyOrderObserver`: Concrete observer - tích điểm khi đơn được thanh toán
- `CustomerNotificationObserver`: Concrete observer - gửi thông báo cho khách hàng

---

### Pattern: State

**Mô tả ngắn:**
OrderContext quản lý state hiện tại của đơn hàng (Pending, Confirmed, Shipping, Completed, Canceled). Mỗi state quyết định action nào có thể làm được (transition sang state khác hay báo lỗi).

**Các file liên quan:**

- eCommerce.Application/States/OrderStates/IOrderState.cs
- eCommerce.Application/States/OrderStates/OrderContext.cs
- eCommerce.Application/States/OrderStates/PendingState.cs
- eCommerce.Application/States/OrderStates/ConfirmedState.cs
- eCommerce.Application/States/OrderStates/ShippingState.cs
- eCommerce.Application/States/OrderStates/CompletedState.cs
- eCommerce.Application/States/OrderStates/CanceledState.cs

**Vai trò các class:**

- `IOrderState`: State interface - `Confirm()`, `Ship()`, `Complete()`, `Cancel()`, property `StatusName`
- `OrderContext`: Context - chứa Order object, quản lý state hiện tại, delegate action cho state, cung cấp `TransitionTo()`
- `PendingState`, `ConfirmedState`, `ShippingState`, `CompletedState`, `CanceledState`: Concrete states - implement các action hợp lệ cho mỗi state

---

### Pattern: Strategy

**Mô tả ngắn:**
Cung cấp các chiến lược thanh toán khác nhau (COD, VNPay) có thể được chọn runtime. Client không cần biết logic chi tiết của mỗi strategy, chỉ gọi `ExecutePaymentAsync()`.

**Các file liên quan:**

- eCommerce.Application/Strategies/Payment/IPaymentStrategy.cs
- eCommerce.Application/Strategies/Payment/CodPaymentStrategy.cs
- eCommerce.Application/Strategies/Payment/VnPayPaymentStrategy.cs
- eCommerce.Web/Pages/Checkout.cshtml.cs (chọn strategy runtime)
- eCommerce.Web/Program.cs (đăng ký strategies)

**Vai trò các class:**

- `IPaymentStrategy`: Strategy interface - `ExecutePaymentAsync()`, property `ProviderName`
- `CodPaymentStrategy`: Concrete strategy - xử lý COD (Cash on Delivery)
- `VnPayPaymentStrategy`: Concrete strategy - xử lý VNPay gateway
- CheckoutModel: Context - chọn đúng strategy dựa vào PaymentMethod của user

---

### Pattern: Template Method

**Mô tả ngắn:**
Định nghĩa bộ khung cố định cho quy trình xử lý đơn hàng hoặc gửi thông báo. Các bước chính được xác định, nhưng subclass có thể override các bước cụ thể.

**Các file liên quan:**

- eCommerce.Web/Controllers/OrderProcessorTemplateMethod.cs
- eCommerce.Web/Controllers/OrdersController.cs (extend)
- eCommerce.Web/Services/Notifications/NotificationSenderTemplateMethod.cs
- eCommerce.Web/Services/Notifications/EmailNotificationSender.cs (extend)
- eCommerce.Web/Services/Notifications/PushNotificationSender.cs (extend)

**Vai trò các class:**

- `OrderProcessorTemplateMethod`: Abstract class - định nghĩa template method `PlaceOrderAsync()` với step: ValidateOrder → CheckStock → ProcessPayment → SendConfirmation
- `OrdersController`: Concrete class - override `ProcessPaymentAsync()` và `SendConfirmationAsync()`
- `NotificationSenderTemplateMethod`: Abstract class - định nghĩa template method `SendAsync()` với step: ValidateRecipient → BuildMessage → SendCore → OnSent
- `EmailNotificationSender`, `PushNotificationSender`: Concrete classes - override `BuildMessage()` và `SendCoreAsync()`

---

### Pattern: Visitor

**Mô tả ngắn:**
Cho phép định nghĩa các phép toán mới trên các element của cây danh mục (Composite) mà không cần sửa đổi cấu trúc. Tách biệt logic xử lý khỏi cấu trúc dữ liệu. Hỗ trợ các phép toán khác nhau: báo cáo giá trị tồn kho, áp dụng giảm giá, sinh metadata SEO.

**Các file liên quan:**

- eCommerce.Application/Visitors/ICatalogVisitor.cs
- eCommerce.Application/Visitors/PriceReportVisitor.cs (tính giá trị tồn kho)
- eCommerce.Application/Visitors/DiscountApplyVisitor.cs (áp dụng giảm giá)
- eCommerce.Application/Visitors/SeoMetadataVisitor.cs (sinh metadata SEO)
- eCommerce.Application/Visitors/SeoMetadata.cs (record metadata)
- eCommerce.Application/Visitors/CatalogVisitorDemo.cs (ví dụ sử dụng)
- eCommerce.Application/Composites/ICatalogComponent.cs (thêm Accept method)
- eCommerce.Application/Composites/CategoryComposite.cs (implement Accept với đệ quy)
- eCommerce.Application/Composites/ProductLeaf.cs (implement Accept)

**Vai trò các class:**

- `ICatalogVisitor`: Visitor interface - định nghĩa `VisitCategory()` và `VisitProduct()`
- `PriceReportVisitor`: Concrete visitor - tính tổng giá trị tồn kho (Giá × Tồn kho) của từng sản phẩm, sinh báo cáo
- `DiscountApplyVisitor`: Concrete visitor - áp dụng giảm giá vào sản phẩm trong danh mục cụ thể, idempotent (không giảm 2 lần)
- `SeoMetadataVisitor`: Concrete visitor - sinh Slug, MetaTitle, MetaDescription cho danh mục và sản phẩm
- `SeoMetadata`: Record đơn giản lưu trữ metadata SEO
- `CategoryComposite`, `ProductLeaf`: Element - implement method Accept() để chấp nhận visitor
- `ICatalogComponent`: Interface component - thêm method Accept(visitor)

---

| Loại | Số lượng | Patterns |
|------|---------|----------|
| **Creational** | 4 | Builder, Factory Method, Simple Factory, Singleton |
| **Structural** | 6 | Adapter, Decorator, Facade, Proxy, Repository, Composite, Flyweight |
| **Behavioral** | 10 | Chain of Responsibility, Command, Iterator, Observer, State, Strategy, Template Method, Mediator, Visitor |
| **TOTAL** | **20** | - |

---

## Ghi chú

1. **Repository** tính là Structural vì nó tách interface từ implementation (decoupling).
2. **OrderContext** có sử dụng mini **Factory Pattern** bên trong (`GetStateFromName()`) để tạo state object, nhưng pattern chính vẫn là **State**.
3. Một số pattern được triển khai đơn giản trong thư mục `DesignPatterns` dùng cho ví dụ học tập, nhưng cũng được áp dụng thực tế ở các phần khác của project.
4. **DI Container** (Program.cs) sử dụng tư tưởng của **Factory Pattern** để tạo và quản lý các instance.
5. **Visitor Pattern** kết hợp với **Composite Pattern** để cho phép thêm các phép toán mới mà không cần sửa cấu trúc cây:
   - `PriceReportVisitor`: Báo cáo giá trị tồn kho
   - `DiscountApplyVisitor`: Áp dụng giảm giá (idempotent)
   - `SeoMetadataVisitor`: Sinh metadata SEO
6. **Composite Pattern** cung cấp cấu trúc cây phân cấp cho danh mục sản phẩm, cho phép xây dựng catalog lớn với nhiều cấp độ.
7. **Flyweight Pattern** tối ưu hóa bộ nhớ cho các object brand được sử dụng lặp lại nhiều lần.
8. **Mediator Pattern** tập trung hóa logic giao tiếp phức tạp giữa các component trong quy trình checkout.
9. **Ngày cập nhật:** 22/03/2026 (sau khi pull code mới từ GitHub và implement Visitor Pattern)


