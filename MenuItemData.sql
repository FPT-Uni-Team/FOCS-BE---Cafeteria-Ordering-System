-- Sample data for restaurant menu system with variants using GUIDs
-- Brands
INSERT INTO Brands (Id, [Name], DefaultTaxRate, IsDelete, IsActive, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) VALUES
('c9f6a50e-ac67-9012-edaf-5e6f7a8b9c0d', 'Gourmet Eats', 7.50, 0, 1, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin')
-- Stores
INSERT INTO Stores (Id, [Name], [Address], PhoneNumber, CustomTaxRate, IsActive, IsDeleted, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, BrandId) VALUES
('550e8400-e29b-41d4-a716-446655440000', 'Downtown Location', '123 Main Street, City Center', '555-0123', 8.25, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', 'c9f6a50e-ac67-9012-edaf-5e6f7a8b9c0d');

-- Tables
INSERT INTO Tables (Id, TableNumber, QRCode, IsActive, IsDeleted, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, StoreId) VALUES
('d0a7b61f-bd78-0123-feba-6f7a8b9c0d1e', 1, 'QR_T01_ABC123', 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '550e8400-e29b-41d4-a716-446655440000'),
('e1b8c72a-ce89-1234-acfb-7a8b9c0d1e2f', 2, 'QR_T02_DEF456', 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '550e8400-e29b-41d4-a716-446655440000'),
('f2c9d83b-df90-2345-badc-8b9c0d1e2f3a', 3, 'QR_T03_GHI789', 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '550e8400-e29b-41d4-a716-446655440000'),
('a3d0e94c-ea01-3456-cbed-9c0d1e2f3a4b', 4, 'QR_T04_JKL012', 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '550e8400-e29b-41d4-a716-446655440000'),
('b4e1f05d-fb12-4567-dcfe-0d1e2f3a4b5c', 5, 'QR_T05_MNO345', 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '550e8400-e29b-41d4-a716-446655440000');
-- MenuCategories
INSERT INTO MenuCategories (Id, [Name], [Description], SortOrder, IsActive, IsDeleted, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, StoreId) VALUES
('f47ac10b-58cc-4372-a567-0e02b2c3d479', 'Beverages', 'Hot and cold drinks', 1, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '550e8400-e29b-41d4-a716-446655440000'),
('581fa8c2-8e4d-4e3a-9b5c-6d2e3f4a5b67', 'Pizza', 'Handcrafted pizzas with fresh ingredients', 2, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '550e8400-e29b-41d4-a716-446655440000'),
('7c9e6679-7425-40de-944b-e07cc1c5d9f2', 'Burgers', 'Gourmet burgers and sandwiches', 3, 1, 0, '2024-01-15 08:00:00', 'admin','2024-01-15 08:00:00', 'admin', '550e8400-e29b-41d4-a716-446655440000'),
('9b5d3e2f-1a4b-4c8d-9e6f-3a1b2c4d5e6f', 'Desserts', 'Sweet treats and ice cream', 4, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '550e8400-e29b-41d4-a716-446655440000');

-- MenuItems
INSERT INTO MenuItems (Id, [Name], [Description], [Images], BasePrice, IsAvailable, IsDeleted, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, MenuCategoryId, StoreId) VALUES
('c4ca4238-a0b9-2382-0d4c-5b6d7e8f9a0b', 'Specialty Coffee', 'Premium coffee blends from around the world', 'coffee.jpg', 4.50, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', '550e8400-e29b-41d4-a716-446655440000'),
('c81e728d-9d4c-0e92-1d5c-6d2e3f4a5b67', 'Fresh Juice', 'Freshly squeezed fruit juices', 'juice.jpg', 3.95, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', 'f47ac10b-58cc-4372-a567-0e02b2c3d479', '550e8400-e29b-41d4-a716-446655440000'),
('eccbc87e-4b5c-2322-2d8c-7e8f9a0b1c2d', 'Margherita Pizza', 'Classic pizza with tomato sauce, mozzarella, and fresh basil', 'margherita.jpg', 12.99, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '581fa8c2-8e4d-4e3a-9b5c-6d2e3f4a5b67', '550e8400-e29b-41d4-a716-446655440000'),
('a87ff679-a2f3-2342-3d9c-8f9a0b1c2d3e', 'Pepperoni Pizza', 'Traditional pepperoni pizza with mozzarella cheese', 'pepperoni.jpg', 14.99, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '581fa8c2-8e4d-4e3a-9b5c-6d2e3f4a5b67', '550e8400-e29b-41d4-a716-446655440000'),
('e4da3b7f-b234-3452-4eac-9a0b1c2d3e4f', 'Classic Burger', 'Beef patty with lettuce, tomato, onion, and special sauce', 'classic_burger.jpg', 9.99, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '7c9e6679-7425-40de-944b-e07cc1c5d9f2', '550e8400-e29b-41d4-a716-446655440000'),
('c9f0f895-fb34-4562-5fbd-0b1c2d3e4f5a', 'Chicken Burger', 'Grilled chicken breast with avocado and mayo', 'chicken_burger.jpg', 10.99, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '7c9e6679-7425-40de-944b-e07cc1c5d9f2', '550e8400-e29b-41d4-a716-446655440000'),
('d3d94468-2a34-5672-6fce-1c2d3e4f5a6b', 'Ice Cream Sundae', 'Vanilla ice cream with your choice of toppings', 'sundae.jpg', 5.99, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '9b5d3e2f-1a4b-4c8d-9e6f-3a1b2c4d5e6f', '550e8400-e29b-41d4-a716-446655440000'),
('2a5ed9b3-3b45-6782-7adf-2d3e4f5a6b7c', 'Chocolate Cake', 'Rich chocolate cake with layers of chocolate ganache', 'chocolate_cake.jpg', 6.99, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '9b5d3e2f-1a4b-4c8d-9e6f-3a1b2c4d5e6f', '550e8400-e29b-41d4-a716-446655440000');

-- VariantGroups
INSERT INTO VariantGroups (Id, [Name], CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, MenuItemId) VALUES
('8e296a06-7c45-7892-8bae-3e4f5a6b7c8d', 'Size', '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', 'c4ca4238-a0b9-2382-0d4c-5b6d7e8f9a0b'), -- Coffee sizes
('4a8d5b1f-9d56-8902-9cbf-4f5a6b7c8d9e', 'Milk Type', '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', 'c4ca4238-a0b9-2382-0d4c-5b6d7e8f9a0b'), -- Coffee milk options
('6b9e6c2a-0e67-9012-adcf-5a6b7c8d9e0f', 'Fruit Type', '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', 'c81e728d-9d4c-0e92-1d5c-6d2e3f4a5b67'), -- Juice flavors
('7c0f7d3b-1f78-0123-beda-6b7c8d9e0f1a', 'Size', '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', 'c81e728d-9d4c-0e92-1d5c-6d2e3f4a5b67'), -- Juice sizes
('8d1a8e4c-2a89-1234-cfeb-7c8d9e0f1a2b', 'Size', '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', 'eccbc87e-4b5c-2322-2d8c-7e8f9a0b1c2d'), -- Pizza sizes
('9e2b9f5d-3b90-2345-dafc-8d9e0f1a2b3c', 'Crust Type', '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', 'eccbc87e-4b5c-2322-2d8c-7e8f9a0b1c2d'), -- Pizza crust
('0f3caf6e-4c01-3456-ebfd-9e0f1a2b3c4d', 'Size', '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', 'a87ff679-a2f3-2342-3d9c-8f9a0b1c2d3e'), -- Pizza sizes
('1a4db07f-5d12-4567-fdce-0f1a2b3c4d5e', 'Crust Type', '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', 'a87ff679-a2f3-2342-3d9c-8f9a0b1c2d3e'), -- Pizza crust
('2b5ec18a-6e23-5678-aedf-1a2b3c4d5e6f', 'Patty Type', '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', 'e4da3b7f-b234-3452-4eac-9a0b1c2d3e4f'), -- Burger patty
('3c6fd29b-7f34-6789-bfea-2b3c4d5e6f7a', 'Side Option', '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', 'e4da3b7f-b234-3452-4eac-9a0b1c2d3e4f'), -- Burger sides
('4d7ae39c-8a45-7890-cafb-3c4d5e6f7a8b', 'Preparation', '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', 'c9f0f895-fb34-4562-5fbd-0b1c2d3e4f5a'), -- Chicken prep
('5e8bf49d-9b56-8901-dbfc-4d5e6f7a8b9c', 'Side Option', '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', 'c9f0f895-fb34-4562-5fbd-0b1c2d3e4f5a'), -- Chicken burger sides
('6f9ca50e-ac67-9012-ecfd-5e6f7a8b9c0d', 'Ice Cream Flavor', '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', 'd3d94468-2a34-5672-6fce-1c2d3e4f5a6b'), -- Sundae flavors
('7a0db61f-bd78-0123-fdae-6f7a8b9c0d1e', 'Toppings', '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', 'd3d94468-2a34-5672-6fce-1c2d3e4f5a6b'), -- Sundae toppings
('8b1ec72a-ce89-1234-aebf-7a8b9c0d1e2f', 'Slice Size', '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '2a5ed9b3-3b45-6782-7adf-2d3e4f5a6b7c'), -- Cake portions
('9c2fd83b-df90-2345-bfac-8b9c0d1e2f3a', 'Add-ons', '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '2a5ed9b3-3b45-6782-7adf-2d3e4f5a6b7c'); -- Cake add-ons

-- MenuItemVariants
INSERT INTO MenuItemVariants (Id, [Name], Price, PrepPerTime, QuantityPerTime, IsAvailable, IsDeleted, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, VariantGroupId) VALUES
-- Coffee Size Variants (Group 1)
('a3d0e94c-ea01-3456-cafd-9c0d1e2f3a4b', 'Small (8oz)', 0.00, 2, 50, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '8e296a06-7c45-7892-8bae-3e4f5a6b7c8d'),
('b4e1f05d-fb12-4567-dbae-0d1e2f3a4b5c', 'Medium (12oz)', 1.00, 2, 40, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '8e296a06-7c45-7892-8bae-3e4f5a6b7c8d'),
('c5f2a16e-ac23-5678-ecdf-1e2f3a4b5c6d', 'Large (16oz)', 2.00, 3, 30, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '8e296a06-7c45-7892-8bae-3e4f5a6b7c8d'),

-- Coffee Milk Type Variants (Group 2)
('d6a3b27f-bd34-6789-fdea-2f3a4b5c6d7e', 'Regular Milk', 0.00, 0, 100, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '4a8d5b1f-9d56-8902-9cbf-4f5a6b7c8d9e'),
('e7b4c38a-ce45-7890-aefb-3a4b5c6d7e8f', 'Almond Milk', 0.75, 0, 80, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '4a8d5b1f-9d56-8902-9cbf-4f5a6b7c8d9e'),
('f8c5d49b-df56-8901-bafc-4b5c6d7e8f9a', 'Oat Milk', 0.75, 0, 60, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '4a8d5b1f-9d56-8902-9cbf-4f5a6b7c8d9e'),
('a9d6e50c-ea67-9012-cbfd-5c6d7e8f9a0b', 'Soy Milk', 0.50, 0, 70, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '4a8d5b1f-9d56-8902-9cbf-4f5a6b7c8d9e'),

-- Juice Fruit Type Variants (Group 3)
('b1e7f61d-fb78-0123-dcfe-6d7e8f9a0b1c', 'Orange', 0.00, 3, 25, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '6b9e6c2a-0e67-9012-adcf-5a6b7c8d9e0f'),
('c2f8a72e-ac89-1234-edaf-7e8f9a0b1c2d', 'Apple', 0.50, 3, 30, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '6b9e6c2a-0e67-9012-adcf-5a6b7c8d9e0f'),
('d3a9b83f-bd90-2345-feba-8f9a0b1c2d3e', 'Pineapple', 1.00, 4, 20, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '6b9e6c2a-0e67-9012-adcf-5a6b7c8d9e0f'),
('e4b0c94a-ce01-3456-afcb-9a0b1c2d3e4f', 'Mixed Berry', 1.50, 5, 15, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '6b9e6c2a-0e67-9012-adcf-5a6b7c8d9e0f'),

-- Juice Size Variants (Group 4)
('f5c1d05b-df12-4567-badc-0b1c2d3e4f5a', 'Small (8oz)', 0.00, 0, 50, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '7c0f7d3b-1f78-0123-beda-6b7c8d9e0f1a'),
('a6d2e16c-ea23-5678-cbed-1c2d3e4f5a6b', 'Medium (12oz)', 1.50, 0, 40, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '7c0f7d3b-1f78-0123-beda-6b7c8d9e0f1a'),
('b7e3f27d-fb34-6789-dcfe-2d3e4f5a6b7c', 'Large (16oz)', 2.50, 0, 30, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '7c0f7d3b-1f78-0123-beda-6b7c8d9e0f1a'),

-- Margherita Pizza Size Variants (Group 5)
('c8f4a38e-ac45-7890-edaf-3e4f5a6b7c8d', 'Personal (8")', 0.00, 12, 8, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '8d1a8e4c-2a89-1234-cfeb-7c8d9e0f1a2b'),
('d9a5b49f-bd56-8901-feba-4f5a6b7c8d9e', 'Medium (12")', 4.00, 15, 6, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '8d1a8e4c-2a89-1234-cfeb-7c8d9e0f1a2b'),
('e0b6c50a-ce67-9012-afcb-5a6b7c8d9e0f', 'Large (16")', 8.00, 18, 4, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '8d1a8e4c-2a89-1234-cfeb-7c8d9e0f1a2b'),

-- Margherita Pizza Crust Variants (Group 6)
('f1c7d61b-df78-0123-badc-6b7c8d9e0f1a', 'Thin Crust', 0.00, 0, 20, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '9e2b9f5d-3b90-2345-dafc-8d9e0f1a2b3c'),
('a2d8e72c-ea89-1234-cbed-7c8d9e0f1a2b', 'Thick Crust', 1.50, 2, 15, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '9e2b9f5d-3b90-2345-dafc-8d9e0f1a2b3c'),
('b3e9f83d-fb90-2345-dcfe-8d9e0f1a2b3c', 'Stuffed Crust', 3.00, 3, 10, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '9e2b9f5d-3b90-2345-dafc-8d9e0f1a2b3c'),

-- Pepperoni Pizza Size Variants (Group 7)
('c4f0a94e-ac01-3456-edaf-9e0f1a2b3c4d', 'Personal (8")', 0.00, 12, 8, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '0f3caf6e-4c01-3456-ebfd-9e0f1a2b3c4d'),
('d5a1b05f-bd12-4567-feba-0f1a2b3c4d5e', 'Medium (12")', 4.00, 15, 6, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '0f3caf6e-4c01-3456-ebfd-9e0f1a2b3c4d'),
('e6b2c16a-ce23-5678-acfb-1a2b3c4d5e6f', 'Large (16")', 8.00, 18, 4, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '0f3caf6e-4c01-3456-ebfd-9e0f1a2b3c4d'),

-- Pepperoni Pizza Crust Variants (Group 8)
('f7c3d27b-df34-6789-badc-2b3c4d5e6f7a', 'Thin Crust', 0.00, 0, 20, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '1a4db07f-5d12-4567-fdce-0f1a2b3c4d5e'),
('a8d4e38c-ea45-7890-cbed-3c4d5e6f7a8b', 'Thick Crust', 1.50, 2, 15, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '1a4db07f-5d12-4567-fdce-0f1a2b3c4d5e'),
('b9e5f49d-fb56-8901-dcfe-4d5e6f7a8b9c', 'Stuffed Crust', 3.00, 3, 10, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '1a4db07f-5d12-4567-fdce-0f1a2b3c4d5e'),

-- Classic Burger Patty Type Variants (Group 9)
('c0f6a50e-ac67-9012-edaf-5e6f7a8b9c0d', 'Regular Beef', 0.00, 8, 12, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '2b5ec18a-6e23-5678-aedf-1a2b3c4d5e6f'),
('d1a7b61f-bd78-0123-feba-6f7a8b9c0d1e', 'Angus Beef', 3.00, 8, 10, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '2b5ec18a-6e23-5678-aedf-1a2b3c4d5e6f'),
('e2b8c72a-ce89-1234-acfb-7a8b9c0d1e2f', 'Wagyu Beef', 8.00, 10, 5, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '2b5ec18a-6e23-5678-aedf-1a2b3c4d5e6f'),
('f3c9d83b-df90-2345-badc-8b9c0d1e2f3a', 'Plant-Based', 2.00, 6, 15, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '2b5ec18a-6e23-5678-aedf-1a2b3c4d5e6f'),

-- Classic Burger Side Options (Group 10)
('a4d0e94c-ea01-3456-cbed-9c0d1e2f3a4b', 'Regular Fries', 2.50, 4, 25, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '3c6fd29b-7f34-6789-bfea-2b3c4d5e6f7a'),
('b5e1f05d-fb12-4567-dcfe-0d1e2f3a4b5c', 'Sweet Potato Fries', 3.50, 5, 20, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '3c6fd29b-7f34-6789-bfea-2b3c4d5e6f7a'),
('c6f2a16e-ac23-5678-edaf-1e2f3a4b5c6d', 'Onion Rings', 3.00, 6, 18, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '3c6fd29b-7f34-6789-bfea-2b3c4d5e6f7a'),
('d7a3b27f-bd34-6789-feba-2f3a4b5c6d7e', 'Side Salad', 4.00, 3, 30, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '3c6fd29b-7f34-6789-bfea-2b3c4d5e6f7a'),

-- Chicken Burger Preparation (Group 11)
('e8b4c38a-ce45-7890-acfb-3a4b5c6d7e8f', 'Grilled', 0.00, 10, 12, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '4d7ae39c-8a45-7890-cafb-3c4d5e6f7a8b'),
('f9c5d49b-df56-8901-badc-4b5c6d7e8f9a', 'Fried', 1.00, 8, 15, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '4d7ae39c-8a45-7890-cafb-3c4d5e6f7a8b'),
('a0d6e50c-ea67-9012-cbed-5c6d7e8f9a0b', 'Blackened', 1.50, 12, 10, 1, 0, '2024-01-15 08:00:00', 'admin', '2024-01-15 08:00:00', 'admin', '4d7ae39c-8a45-7890-cafb-3c4d5e6f7a8b'),
