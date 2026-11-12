-- ==========================================================
-- SCRIPT TẠO CƠ SỞ DỮ LIỆU RECIPER (CHO THÀNH VIÊN TRONG NHÓM)
-- Hướng dẫn: Mở SQL Server Management Studio (SSMS) -> Mở file này -> Bấm Execute (F5)
-- ==========================================================

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'Reciper')
BEGIN
    CREATE DATABASE [Reciper];
END
GO

USE [Reciper];
GO

-- 1. BẢNG CATEGORIES (Danh mục món ăn)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'categories')
BEGIN
    CREATE TABLE [dbo].[categories] (
        [id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [name] NVARCHAR(100) NOT NULL
    );
END
GO

-- 2. BẢNG USERS (Người dùng & Quản trị viên)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'users')
BEGIN
    CREATE TABLE [dbo].[users] (
        [id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [username] VARCHAR(50) NOT NULL UNIQUE,
        [email] VARCHAR(150) NOT NULL UNIQUE,
        [password_hash] VARCHAR(255) NOT NULL,
        [full_name] NVARCHAR(100) NULL,
        [role] VARCHAR(20) NOT NULL DEFAULT 'User',
        [is_active] BIT NOT NULL DEFAULT 1,
        [created_at] DATETIME2(7) NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 3. BẢNG INGREDIENTS (Từ điển nguyên liệu)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ingredients')
BEGIN
    CREATE TABLE [dbo].[ingredients] (
        [id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [name] NVARCHAR(100) NOT NULL UNIQUE
    );
END
GO

-- 4. BẢNG RECIPES (Công thức món ăn)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'recipes')
BEGIN
    CREATE TABLE [dbo].[recipes] (
        [id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [category_id] INT NULL FOREIGN KEY REFERENCES [dbo].[categories]([id]),
        [title] NVARCHAR(200) NOT NULL,
        [description] NVARCHAR(MAX) NULL,
        [servings] TINYINT NOT NULL DEFAULT 2,
        [prep_minutes] SMALLINT NOT NULL DEFAULT 15,
        [cook_minutes] SMALLINT NOT NULL DEFAULT 30,
        [difficulty] NVARCHAR(20) NOT NULL DEFAULT N'Trung bình',
        [image_url] NVARCHAR(255) NULL,
        [user_id] INT NULL FOREIGN KEY REFERENCES [dbo].[users]([id]),
        [status] VARCHAR(20) NOT NULL DEFAULT 'published',
        [created_at] DATETIME2(7) NOT NULL DEFAULT GETDATE()
    );
END
GO

-- 5. BẢNG RECIPE_INGREDIENTS (Nguyên liệu của từng công thức)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'recipe_ingredients')
BEGIN
    CREATE TABLE [dbo].[recipe_ingredients] (
        [recipe_id] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[recipes]([id]) ON DELETE CASCADE,
        [ingredient_id] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[ingredients]([id]),
        [quantity] DECIMAL(8,2) NULL,
        [unit] NVARCHAR(30) NULL,
        [note] NVARCHAR(150) NULL,
        PRIMARY KEY ([recipe_id], [ingredient_id])
    );
END
GO

-- 6. BẢNG STEPS (Các bước chế biến)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'steps')
BEGIN
    CREATE TABLE [dbo].[steps] (
        [id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [recipe_id] INT NOT NULL FOREIGN KEY REFERENCES [dbo].[recipes]([id]) ON DELETE CASCADE,
        [step_number] TINYINT NOT NULL,
        [instruction] NVARCHAR(MAX) NOT NULL
    );
END
GO

-- ==========================================================
-- DỮ LIỆU KHỞI TẠO MẪU
-- ==========================================================

-- Tạo tài khoản Admin mặc định (admin / 123456)
IF NOT EXISTS (SELECT 1 FROM [dbo].[users] WHERE [username] = 'admin')
BEGIN
    INSERT INTO [dbo].[users] ([username], [email], [password_hash], [full_name], [role], [is_active], [created_at])
    VALUES ('admin', 'admin@reciper.com', '123456', N'Quản trị viên', 'Admin', 1, GETDATE());
END
GO

-- Tạo các danh mục mẫu
IF NOT EXISTS (SELECT 1 FROM [dbo].[categories])
BEGIN
    INSERT INTO [dbo].[categories] ([name]) VALUES 
    (N'Món Bún'),
    (N'Món Phở'),
    (N'Món Cơm'),
    (N'Món Canh / Súp'),
    (N'Món Xào'),
    (N'Món Nướng'),
    (N'Món Tráng Miệng');
END
GO

