-- 1-N ENTITY: Category
create table [Category] (
    [Id] int identity(1,1) not null,
    [Name] nvarchar(256) not null,
    [Description] nvarchar(max) not null,
    constraint [PK_Category] primary key clustered ([Id] asc)
)
go

-- PRIMARY ENTITY: Product
create table [Product] (
    [Id] int identity(1,1) not null,
    [Title] nvarchar(256) not null,
    [Description] nvarchar(max) not null,
    [Price] decimal(18,2) not null,
    [ImageUrl] nvarchar(512) null,
    [CategoryId] int not null,
    [CreatedAt] datetime2(7) not null,
    [DeletedAt] datetime2(7) null,
    constraint [PK_Product] primary key clustered ([Id] asc),
    constraint [FK_Product_Category] foreign key ([CategoryId]) 
        references [Category]([Id])
)
go

alter table [Product]
    add constraint [DF_Product_CreatedAt] default (getutcdate()) for [CreatedAt]
go

-- M-N ENTITY: Country
create table [Country] (
    [Id] int identity(1,1) not null,
    [Name] nvarchar(256) not null,
    [Code] nvarchar(10) not null,
    constraint [PK_Country] primary key clustered ([Id] asc)
)
go

-- M-N: ProductCountry
create table [ProductCountry] (
    [ProductId] int not null,
    [CountryId] int not null,
    constraint [PK_ProductCountry] primary key clustered ([ProductId], [CountryId]),
    constraint [FK_ProductCountry_Product] foreign key ([ProductId]) 
        references [Product]([Id]) on delete cascade,
    constraint [FK_ProductCountry_Country] foreign key ([CountryId]) 
        references [Country]([Id]) on delete cascade
)
go

-- USER 
create table [User] (
    [Id] int identity(1,1) not null,
    [Username] nvarchar(50) not null,
    [Email] nvarchar(256) not null,
    [PwdHash] nvarchar(256) not null,
    [PwdSalt] nvarchar(256) not null,
    [FirstName] nvarchar(256) not null,
    [LastName] nvarchar(256) not null,
    [Phone] nvarchar(50) null,
    [IsAdmin] bit not null,
    [CreatedAt] datetime2(7) not null,
    [Address] nvarchar(200) null,
    constraint [PK_User] primary key clustered ([Id] asc),
    constraint [UQ_User_Username] unique ([Username]),
    constraint [UQ_User_Email] unique ([Email])
)
go

alter table [User]
    add constraint [DF_User_IsAdmin] default (0) for [IsAdmin]
go

alter table [User]
    add constraint [DF_User_CreatedAt] default (getutcdate()) for [CreatedAt]
go

-- USER M-N: Order
create table [Order] (
    [Id] int identity(1,1) not null,
    [UserId] int not null,
    [OrderDate] datetime2(7) not null,
    [TotalAmount] decimal(18,2) not null,
    [Status] nvarchar(50) not null,
    constraint [PK_Order] primary key clustered ([Id] asc),
    constraint [FK_Order_User] foreign key ([UserId]) references [User]([Id])
)
go

alter table [Order]
    add constraint [DF_Order_OrderDate] default (getutcdate()) for [OrderDate]
go

alter table [Order]
    add constraint [DF_Order_Status] default ('Pending') for [Status]
go

-- OrderItem
create table [OrderItem] (
    [Id] int identity(1,1) not null,
    [OrderId] int not null,
    [ProductId] int not null,
    [Quantity] int not null,
    [PriceAtOrder] decimal(18,2) not null,
    constraint [PK_OrderItem] primary key clustered ([Id] asc),
    constraint [FK_OrderItem_Order] foreign key ([OrderId]) references [Order]([Id]) on delete cascade,
    constraint [FK_OrderItem_Product] foreign key ([ProductId]) references [Product]([Id])
)


-- LOG
create table [Log] (
    [Id] int identity(1,1) not null,
    [Timestamp] datetime2(7) not null,
    [Level] int not null,
    [Message] nvarchar(1024) not null,
    [ErrorDetails] nvarchar(max) null,
    constraint [PK_Log] primary key clustered ([Id] asc)
)
go

alter table [Log]
    add constraint [DF_Log_Timestamp] default (getutcdate()) for [Timestamp]
go

--ALTER TABLE PRODUCT 
alter table [Product]
    add [Stock] int not null default 0
go

---------------------------------
-- SEED FOR RWA eCommerce project
---------------------------------


------------
--CATEGORIES
------------
set identity_insert [dbo].[Category] ON
go

insert into [dbo].[Category] ([Id], [Name], [Description])
values
(1, N'Elektornika', N'Elektronièki ureðaji'),
(2, N'Odjeæa', N'Muška i ženska odjeæa'),
(3, N'Knjige', N'Tiskane knjige'),
(4, N'Sport', N'Sportksa oprema'),
(5, N'Kuæanski aparati', N'Mali i veliki kuæanski aparati')
go

set identity_insert [dbo].[Category] off
go

-------------------------------------------

------------
--COUNTRIES
------------

set identity_insert [dbo].[Country] on
go

insert into [dbo].[Country] ([Id], [Name], [Code])
values
(1, N'Hrvatska', N'HR'),
(2, N'Slovenija', N'SI'),
(3, N'Bosna i Hercegovina', N'BA'),
(4, N'Srbija', N'RS'),
(5, N'Austrija', N'AT'),
(6, N'Italija', N'IT')

set identity_insert [dbo].[Country] off
go

------------------------------------------


-----------
--Products
-----------

set identity_insert [dbo].[Product] on
go

insert into [dbo].[Product] ([Id], [Title], [Description], [Price], [ImageUrl], [CategoryId], [CreatedAt])
values
--Elektornika - catId 1
(1, N'Iphone 200 Ultra Pro Max Min', N'Noviji od najnovijeg sa jos bržim procesorom nego najnoviji iPhone', 2899.99, N'https://miro.medium.com/v2/0*ZGfqtzaq7RZM2INS.', 1, getutcdate()),
(2, N'Šmasung S Ptica', N'Premium Andorid AI AI AI smartphone.', 3499.99, N'https://cdn.mos.cms.futurecdn.net/dKiapuFZTi37bFMrQQcbuG.png', 1, getutcdate()),
(12 , N'Raspberry Pi 5 8GB', N'Pita od 8GB', 119.99, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTc-b4WOrSOkXOHJsDKxryaqSwVk3QgyUZtTw&s', 1, getutcdate()),

--Sport - catId 4
(3, N'Nike Air Jordan 205', N'Jordanice - Like Mike', 349.99, N'https://cdn.sanity.io/images/pu5wtzfc/production/b98e6b8455c57dd4cb99f30eb870bfa458913458-1200x750.jpg/nike-blazer-like-mike-white-university-blue-cz1055-111-release-date.jpg', 4, getutcdate()),
(4, N'Teniski Reket BaboMlat', N'BaboMlat teniski reket. Za ozbiljne reketare.', 160.00, N'https://www.babolat.hr/wp-content/uploads/2023/06/101488-pure_aero_team-370_Mala.jpg', 4, getutcdate()),
(13, N'Nike kopaèke', N'Nike kopaèke za nogomet - osiguravaju golove', 129.99, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTHPIFjYrcxZ2lIc0vWRLejyOICHk56lEABaA&s', 4, getutcdate()),

--Odjeæa - catId 2
(5, N'Rockamark Hoodie majica', N'Majica sa kapuljaèom - hoodie', 49.99, N'https://www.rockmark.hr/wp-content/uploads/2023/12/rugalica.jpg', 2, getutcdate()),
(6, N'Adidas kratka majica', N'Noviji od najnovijeg sa jos bržim procesorom nego najnoviji iPhone', 29.99, N'https://www.buzzsneakers.hr/files/thumbs/files/images/slike-proizvoda/media/CW0/CW0709/images/thumbs_800/CW0709_800_800px.jpg', 2, getutcdate()),
(14, N'Levis 501 traperice', N'Levis 501 klasik traperice', 99.99, N'https://img2.ans-media.com/i/840x1260/SS25-SJM0HC-50J_F1.webp?v=1744780077', 2, getutcdate()),

--Knjige - catId 3
(7, N'Družba Pere Kvržice', N'Knjiga Družba Pere Kvržice - Mato Lovrak', 19.99, N'https://narodna-knjiznica-dugopolje.hr/wp-content/uploads/2016/12/druzba-pere-vrzice-knjiznica-dugopolje.jpg', 3, getutcdate()),
(8, N'Trojica u Trnju', N'Knjiga Trojica u Trnju - Pavao Pavlièiæ', 15.99, N'https://www.vbz.hr/wp-content/uploads/2019/01/trojica_u_trnju_4.izd_.jpg', 3, getutcdate()),
(15, N'Knjiga Harry Potter', N'Nova knjiga najpoznatijeg èarobnjaka na svijetu', 38.99, N'https://cdn.europosters.eu/image/1300/104639.jpg', 3, getutcdate()),

--Kuæanski - catId 5
(9, N'Xiaomi Robotski Usisavaè', N'Xiaomi Robotski Usisavaè - ultra pametan', 299.99, N'https://mi.hr/media/catalog/product/cache/5b6fdfdea6946760ce9671da1d9a2ad6/5/4/54405_xiaomi_robot_vacuum_x20_eu_3_.webp', 5, getutcdate()),
(10, N'Miele perilica za rublje', N'Miele perilica za rublje - pere ko velika', 499.99, N'https://media.miele.com/images/2000021/200002138/20000213855.png', 5, getutcdate()),
(11, N'Vivax perilica posuða', N'Vivax perilica posuða - pere tanjure, èaše i zube', 399.99, N'https://vivax.com/wp-content/uploads/2021/12/DW-45942B-P-Open-Right.png', 5, getutcdate())
go

set identity_insert [dbo].[Product] off
go

-------------------------------------------------------------

----------------
--ProductCountry
----------------

-- (1, N'Hrvatska', N'HR'),
-- (2, N'Slovenija', N'SI'),
-- (3, N'Bosna i Hercegovina', N'BA'),
-- (4, N'Srbija', N'RS'),
-- (5, N'Austrija', N'AT'),
-- (6, N'Italija', N'IT')

insert into [dbo].[ProductCountry] ([ProductId], [CountryId])
values
--Iphone sve države
(1, 1), (1, 2), (1, 3), (1, 4), (1,5), (1,6),
--Samsung sve države
(2, 1), (2, 2), (2, 3), (2, 4), (2,5), (2,6),
--Pita samo HR, SI, BA
(12, 1), (12, 2), (12, 3),

--Jordanice samo AT i IT
(3, 5), (3, 6),
--Reket samo RS
(4, 4),
--Nike kopacke samo HR, RS, IT
(13, 1), (13, 4), (13, 6),

--Hoodica samo HR
(5, 1),
--Adidas kratka majica svugdje
(6, 1), (6, 2), (6, 3), (6, 4), (6,5), (6,6),
--Levis traperice samo BA i AT
(14, 3), (14, 5),

--Knjiga Pere samo HR, SI, BA, RS
(7, 1), (7, 2), (7, 3), (7, 4),
--Knjiga Trnje samo HR i SI
(8, 1), (8, 2),
--Knjiga Potter samo IT
(15, 6),

--Xiaomi usis svugdje
(9, 1), (9, 2), (9, 3), (9, 4), (9,5), (9,6)

--Druga dva ureðaja nigdje
go
-----------------------------------------------------------

-------
--Users seed, napraviti pravu registraciju kroz app ili api
-------

set identity_insert [dbo].[User] on
go

insert into [dbo].[User] ([Id], [Username], [Email], [PwdHash], [PwdSalt], [FirstName], [LastName], [Phone], [IsAdmin], [CreatedAt])
values

(1, N'admin_seed', N'admin_seed@ecommerce.hr',
     N'kS7wP3M+JvC8zN2xR5hQ6tY9uW1eA4bD0fG8jK3mL7nV2',
     N'X9pL2mK5nB8vC1zA4sD7fG0hJ3qW6eR9tY',
     N'Adminko', N'Seed', N'+385911234567', 1, GETUTCDATE()),

(2, N'djuro_seed', N'djuro.djuric@gmail.com',
     N'kS7wP3M+JvC8zN2xR5hQ6tY9uW1eA4bD0fG8jK3mL7nV2',
     N'X9pL2mK5nB8vC1zA4sD7fG0hJ3qW6eR9tY',
     N'Djuro', N'Seed', N'+385981234567', 0, GETUTCDATE()),

(3, N'marko_seed', N'marko.maric@gmail.com',
     N'kS7wP3M+JvC8zN2xR5hQ6tY9uW1eA4bD0fG8jK3mL7nV2',
     N'X9pL2mK5nB8vC1zA4sD7fG0hJ3qW6eR9tY',
     N'Marko', N'Seed', N'+385991234567', 0, GETUTCDATE())

go

set identity_insert [dbo].[User] off
go

------------------------------------------------

----------
--Order
-----------

set identity_insert [dbo].[Order] on
go

insert into [dbo].[Order] ([Id], [UserId], [OrderDate], [TotalAmount], [Status])
values
--Iphone + Jordanice 3249.98
(1, 2, dateadd(day, -5, getutcdate()), 3249.98, N'Pending'),
--Samsung + Reket = 3659.99
(2, 3, dateadd(day, -3, getutcdate()), 3659.99, N'Completed')

set identity_insert [dbo].[Order] off
go

-----------------------------------------------------------------------

----------
--OrderItem
----------

set identity_insert [dbo].[OrderItem] on
go

insert into [dbo].[OrderItem] ([Id], [OrderId], [ProductId], [Quantity], [PriceAtOrder])
values

(1, 1, 1, 1, 2899.99),
(2, 1, 3, 1, 349.99),

(3, 2, 2, 1, 3499.99),
(4, 2, 4, 1, 160.00)

set identity_insert [dbo].[OrderItem] off
go

-----------------------------------------------------

------
--LOG
------

set identity_insert [dbo].[Log] on
go

insert into [dbo].[Log] ([Id], [Timestamp], [Level], [Message], [ErrorDetails])
values
(1, dateadd(minute, -120, getutcdate()), 1, N'Application started', NULL),
(2, dateadd(minute, -100, getutcdate()), 2, N'Database connection established', NULL)


set identity_insert [dbo].[Log] off
go

--STOCK
-- UPDATE STOCK
update [Product] set [Stock] = 10 where [Stock] = 0
go

-- set user to ADMIN
--update [User] set [IsAdmin] = 1 where [Username] = 'ADMINPLACEHODER'

