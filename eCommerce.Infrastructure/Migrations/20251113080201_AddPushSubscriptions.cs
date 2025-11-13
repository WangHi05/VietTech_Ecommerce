using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace eCommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPushSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF OBJECT_ID(N'[dbo].[PushSubscriptions]') IS NULL
BEGIN
    CREATE TABLE [PushSubscriptions] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(max) NOT NULL,
        [Endpoint] nvarchar(max) NOT NULL,
        [P256DH] nvarchar(max) NOT NULL,
        [Auth] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_PushSubscriptions] PRIMARY KEY ([Id])
    );
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF OBJECT_ID(N'[dbo].[PushSubscriptions]') IS NOT NULL
BEGIN
    DROP TABLE [PushSubscriptions];
END");
        }
    }
}
