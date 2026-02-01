CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FullName] nvarchar(100) NULL,
    [ProfilePictureUrl] nvarchar(max) NULL,
    [Vung] nvarchar(100) NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Brands] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Brands] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Categories] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Reviews] (
    [ReviewId] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [UserId] nvarchar(max) NULL,
    [OrderId] int NULL,
    [UserName] nvarchar(256) NULL,
    [Rating] int NOT NULL,
    [Comment] nvarchar(2000) NOT NULL,
    [CreatedDate] datetime2 NOT NULL,
    [Status] nvarchar(32) NOT NULL,
    CONSTRAINT [PK_Reviews] PRIMARY KEY ([ReviewId])
);
GO


CREATE TABLE [UserPushSubscriptions] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [Endpoint] nvarchar(max) NOT NULL,
    [P256dh] nvarchar(255) NOT NULL,
    [Auth] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_UserPushSubscriptions] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Vouchers] (
    [Id] int NOT NULL IDENTITY,
    [Code] nvarchar(50) NOT NULL,
    [Description] nvarchar(200) NOT NULL,
    [DiscountPercent] decimal(18,2) NULL,
    [DiscountAmount] decimal(18,2) NULL,
    [MinOrderValue] decimal(18,2) NOT NULL,
    [MaxDiscountAmount] decimal(18,2) NULL,
    [MaxUsage] int NOT NULL,
    [UsedCount] int NOT NULL,
    [MaxUsagePerUser] int NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [ExpiryDate] datetime2 NOT NULL,
    [IsActive] bit NOT NULL,
    [Vung] nvarchar(200) NULL,
    CONSTRAINT [PK_Vouchers] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(128) NOT NULL,
    [ProviderKey] nvarchar(128) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(128) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [LoyaltyPoints] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [TotalPoints] int NOT NULL,
    [LifetimePoints] int NOT NULL,
    [MembershipTier] nvarchar(max) NOT NULL,
    [LastUpdated] datetime2 NOT NULL,
    CONSTRAINT [PK_LoyaltyPoints] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LoyaltyPoints_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Orders] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [PaymentMethod] nvarchar(32) NOT NULL DEFAULT N'COD',
    [PaymentStatus] nvarchar(32) NOT NULL DEFAULT N'Pending',
    [Status] nvarchar(32) NOT NULL DEFAULT N'Pending',
    [PaidAt] datetime2 NULL,
    [CardHolderName] nvarchar(128) NULL,
    [CardLast4] nvarchar(8) NULL,
    [ShippingName] nvarchar(max) NOT NULL,
    [ShippingAddress] nvarchar(max) NOT NULL,
    [ShippingMethod] nvarchar(max) NOT NULL,
    [ShippingCountry] nvarchar(max) NOT NULL,
    [ShippingProvince] nvarchar(max) NOT NULL,
    [SubTotal] decimal(18,2) NOT NULL,
    [Discount] decimal(18,2) NOT NULL,
    [ShippingFee] decimal(18,2) NOT NULL,
    [VoucherCode] nvarchar(max) NULL,
    [Total] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Orders_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id])
);
GO


CREATE TABLE [Products] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [StockQuantity] int NOT NULL,
    [MinStockLevel] int NOT NULL,
    [ImageUrl] nvarchar(max) NOT NULL,
    [CategoryId] int NOT NULL,
    [BrandId] int NOT NULL,
    [Color] nvarchar(50) NOT NULL,
    [Size] nvarchar(50) NOT NULL,
    [IsNew] bit NOT NULL,
    [IsOnSale] bit NOT NULL,
    [OldPrice] decimal(18,2) NULL,
    [DiscountPercent] int NOT NULL,
    [AverageRating] float NOT NULL,
    [ReviewCount] int NOT NULL,
    [Specifications] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Products_Brands_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [Brands] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [PointTransactions] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [OrderId] int NULL,
    [Points] int NOT NULL,
    [Type] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_PointTransactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PointTransactions_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PointTransactions_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE SET NULL
);
GO


CREATE TABLE [UserVouchers] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [VoucherId] int NOT NULL,
    [CollectedDate] datetime2 NOT NULL,
    [IsUsed] bit NOT NULL,
    [UsedDate] datetime2 NULL,
    [OrderId] int NULL,
    CONSTRAINT [PK_UserVouchers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_UserVouchers_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserVouchers_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]),
    CONSTRAINT [FK_UserVouchers_Vouchers_VoucherId] FOREIGN KEY ([VoucherId]) REFERENCES [Vouchers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Messages] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [SenderId] nvarchar(max) NOT NULL,
    [SenderName] nvarchar(max) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [ImageUrl] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IsFromSeller] bit NOT NULL,
    CONSTRAINT [PK_Messages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Messages_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [OrderItems] (
    [Id] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [ProductId] int NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [Quantity] int NOT NULL,
    CONSTRAINT [PK_OrderItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrderItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [StockHistories] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [Type] nvarchar(20) NOT NULL,
    [Quantity] int NOT NULL,
    [BeforeQuantity] int NOT NULL,
    [AfterQuantity] int NOT NULL,
    [Reason] nvarchar(500) NOT NULL,
    [OrderId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(256) NOT NULL,
    CONSTRAINT [PK_StockHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StockHistories_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]),
    CONSTRAINT [FK_StockHistories_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);
GO


CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO


CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO


CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO


CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO


CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO


CREATE UNIQUE INDEX [IX_LoyaltyPoints_UserId] ON [LoyaltyPoints] ([UserId]);
GO


CREATE INDEX [IX_Messages_ProductId] ON [Messages] ([ProductId]);
GO


CREATE INDEX [IX_OrderItems_OrderId] ON [OrderItems] ([OrderId]);
GO


CREATE INDEX [IX_OrderItems_ProductId] ON [OrderItems] ([ProductId]);
GO


CREATE INDEX [IX_Orders_UserId] ON [Orders] ([UserId]);
GO


CREATE INDEX [IX_PointTransactions_OrderId] ON [PointTransactions] ([OrderId]);
GO


CREATE INDEX [IX_PointTransactions_UserId] ON [PointTransactions] ([UserId]);
GO


CREATE INDEX [IX_Products_BrandId] ON [Products] ([BrandId]);
GO


CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId]);
GO


CREATE INDEX [IX_StockHistories_OrderId] ON [StockHistories] ([OrderId]);
GO


CREATE INDEX [IX_StockHistories_ProductId] ON [StockHistories] ([ProductId]);
GO


CREATE INDEX [IX_UserVouchers_OrderId] ON [UserVouchers] ([OrderId]);
GO


CREATE INDEX [IX_UserVouchers_UserId] ON [UserVouchers] ([UserId]);
GO


CREATE INDEX [IX_UserVouchers_VoucherId] ON [UserVouchers] ([VoucherId]);
GO


