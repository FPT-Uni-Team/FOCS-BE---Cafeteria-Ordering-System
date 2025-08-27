USE [SEP490FOCS]
GO
INSERT [dbo].[Brands] ([Id], [Name], [DefaultTaxRate], [IsDelete], [IsActive], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'6bf5e5d5-aa17-4a47-a80b-1fea3e3802e2', N'Nguyễn Hồng Phúc', 0.04, 0, 1, CAST(N'2025-08-11T15:05:46.0074193' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', NULL, NULL)
GO
INSERT [dbo].[Brands] ([Id], [Name], [DefaultTaxRate], [IsDelete], [IsActive], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'a196bf01-5c5e-480f-b496-233ddaeea011', N'Nguyễn Hồng Phúc', 0.1, 0, 1, CAST(N'2025-08-11T15:03:49.1008221' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', NULL, NULL)
GO
INSERT [dbo].[Brands] ([Id], [Name], [DefaultTaxRate], [IsDelete], [IsActive], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'383127ac-d56e-48b7-94fa-35fd1da2e79d', N'The Coffee House', 1, 0, 1, CAST(N'2025-08-26T22:19:40.0699961' AS DateTime2), N'502076ae-0fd0-4c1c-bd57-8c3a7dc480ac', NULL, NULL)
GO
INSERT [dbo].[Brands] ([Id], [Name], [DefaultTaxRate], [IsDelete], [IsActive], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'6a35b974-2738-4038-aa28-4375f81a436c', N'a', 1, 0, 1, CAST(N'2025-08-11T15:06:01.9834927' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', NULL, NULL)
GO
INSERT [dbo].[Brands] ([Id], [Name], [DefaultTaxRate], [IsDelete], [IsActive], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'557eb2ea-94fa-440b-a2d6-438df22f9326', N'Nguyễn Hồng Phúc', 1, 1, 1, CAST(N'2025-08-11T15:00:40.5310471' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', CAST(N'2025-08-11T15:03:42.6091610' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7')
GO
INSERT [dbo].[Brands] ([Id], [Name], [DefaultTaxRate], [IsDelete], [IsActive], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'550e8400-e29b-41d4-a716-446655440001', N'mixue', 0.1, 0, 1, CAST(N'2025-08-11T15:00:40.0000000' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', NULL, NULL)
GO
INSERT [dbo].[Brands] ([Id], [Name], [DefaultTaxRate], [IsDelete], [IsActive], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'adb8e965-7127-4d6a-97dd-78466720279b', N'The Coffee House', 1, 0, 1, CAST(N'2025-08-12T07:57:59.3146315' AS DateTime2), N'6d109a17-2a7d-4c61-99ef-b110e02a0563', NULL, NULL)
GO
INSERT [dbo].[Brands] ([Id], [Name], [DefaultTaxRate], [IsDelete], [IsActive], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'47393468-3135-42e8-9a3b-eb01d8683c6c', N'Constrast', 1, 0, 1, CAST(N'2025-08-12T07:58:13.0948871' AS DateTime2), N'6d109a17-2a7d-4c61-99ef-b110e02a0563', NULL, NULL)
GO
INSERT [dbo].[Stores] ([Id], [Name], [Address], [PhoneNumber], [CustomTaxRate], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [BrandId]) VALUES (N'7ab65347-49d6-42dd-a8b7-3021bccc6ad3', N'The Coffee House - CG', N'Nguyễn Văn Huyên - Cầu Giấy', N'0923312332', 1, 1, 0, CAST(N'2025-08-12T07:59:09.9225185' AS DateTime2), N'6d109a17-2a7d-4c61-99ef-b110e02a0563', NULL, NULL, N'adb8e965-7127-4d6a-97dd-78466720279b')
GO
INSERT [dbo].[Stores] ([Id], [Name], [Address], [PhoneNumber], [CustomTaxRate], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [BrandId]) VALUES (N'c9f036ed-515e-4a6a-8b04-3294e34ddb2d', N'The Coffee House - HL', N'Đại học FPT - Hòa Lạc', N'06253254189', 1, 1, 0, CAST(N'2025-08-12T07:59:35.1867716' AS DateTime2), N'6d109a17-2a7d-4c61-99ef-b110e02a0563', NULL, NULL, N'adb8e965-7127-4d6a-97dd-78466720279b')
GO
INSERT [dbo].[Stores] ([Id], [Name], [Address], [PhoneNumber], [CustomTaxRate], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [BrandId]) VALUES (N'550e8400-e29b-41d4-a716-446655440000', N'The Coffee House', N'Dương Đình Nghệ', N'0938491222', 1, 1, 0, NULL, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', NULL, NULL, N'550e8400-e29b-41d4-a716-446655440001')
GO
INSERT [dbo].[Stores] ([Id], [Name], [Address], [PhoneNumber], [CustomTaxRate], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [BrandId]) VALUES (N'1bda923c-97c9-42a7-b9a9-73f0e602db27', N'Nguyễn Hồng Phúc', N'Đồng Tiến - Khoái Châu - Hưng Yên', N'0966393025', 0.12, 1, 0, CAST(N'2025-08-11T15:04:16.3272553' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', CAST(N'2025-08-11T15:04:42.1321828' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'550e8400-e29b-41d4-a716-446655440001')
GO
INSERT [dbo].[Stores] ([Id], [Name], [Address], [PhoneNumber], [CustomTaxRate], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [BrandId]) VALUES (N'84859782-c27b-470e-883b-77e07df13a57', N'The Coffee House ', N'Cầu Giấy', N'0974767405', 1, 1, 0, CAST(N'2025-08-26T22:20:35.2712320' AS DateTime2), N'502076ae-0fd0-4c1c-bd57-8c3a7dc480ac', NULL, NULL, N'383127ac-d56e-48b7-94fa-35fd1da2e79d')
GO
INSERT [dbo].[Stores] ([Id], [Name], [Address], [PhoneNumber], [CustomTaxRate], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [BrandId]) VALUES (N'05f17a65-bf51-4a8d-a08b-961f1e0c708a', N'Nguyễn Hồng Phúc', N'Đồng Tiến - Khoái Châu - Hưng Yên', N'0966393025', 1, 1, 1, CAST(N'2025-08-11T15:00:54.6234613' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', CAST(N'2025-08-11T15:01:29.8397484' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'557eb2ea-94fa-440b-a2d6-438df22f9326')
GO
INSERT [dbo].[MenuItems] ([Id], [Name], [Description], [BasePrice], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [IsActive]) VALUES (N'bc067dd1-a55d-464d-9df1-0ea74b0e503c', N'Phở sốt vang', N'Phở sốt vang', 35000, 1, 0, CAST(N'2025-08-12T10:13:55.0103511' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'550e8400-e29b-41d4-a716-446655440000', 0)
GO
INSERT [dbo].[MenuItems] ([Id], [Name], [Description], [BasePrice], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [IsActive]) VALUES (N'dc098afc-eb8b-4757-82e1-3db5d2d413db', N'tra sua 1', N'abcd', 1.212, 1, 1, CAST(N'2025-08-11T08:03:42.9609606' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', CAST(N'2025-08-12T07:29:42.9281977' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'550e8400-e29b-41d4-a716-446655440000', 0)
GO
INSERT [dbo].[MenuItems] ([Id], [Name], [Description], [BasePrice], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [IsActive]) VALUES (N'7e2994b3-3aee-4dbf-8744-41c95fa8f73d', N'Cơm rang thập cẩm', N'Cơm rang thập cẩm', 35000, 1, 0, CAST(N'2025-08-12T10:10:40.1852083' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', CAST(N'2025-08-12T10:27:02.0603464' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'550e8400-e29b-41d4-a716-446655440000', 0)
GO
INSERT [dbo].[MenuItems] ([Id], [Name], [Description], [BasePrice], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [IsActive]) VALUES (N'80185741-1f1c-40af-9eeb-80718c7b9d2c', N'Phở bò nóng hổi', N'Phở bò', 35000, 1, 0, CAST(N'2025-08-12T07:59:05.6423870' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', CAST(N'2025-08-12T08:17:39.0273624' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'550e8400-e29b-41d4-a716-446655440000', 0)
GO
INSERT [dbo].[MenuItems] ([Id], [Name], [Description], [BasePrice], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [IsActive]) VALUES (N'8927310a-664e-46e6-965f-81e0209a96a0', N'Cơm rang đùi gà', N'Cơm rang đùi gà', 35000, 1, 0, CAST(N'2025-08-12T10:11:59.0106868' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'550e8400-e29b-41d4-a716-446655440000', 0)
GO
INSERT [dbo].[MenuItems] ([Id], [Name], [Description], [BasePrice], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [IsActive]) VALUES (N'059c4212-3f4d-4ec9-96e2-848d7224ca74', N'Phở trộn', N'Phở trộn', 35000, 1, 0, CAST(N'2025-08-12T10:08:36.3273816' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'550e8400-e29b-41d4-a716-446655440000', 0)
GO
INSERT [dbo].[MenuItems] ([Id], [Name], [Description], [BasePrice], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [IsActive]) VALUES (N'f6e20025-2858-47eb-96ef-9d49d0d49b1c', N'Phở bò', N'phở', 35000, 1, 1, CAST(N'2025-08-11T04:17:59.0380044' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', CAST(N'2025-08-12T07:29:32.0617142' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'550e8400-e29b-41d4-a716-446655440000', 0)
GO
INSERT [dbo].[MenuItems] ([Id], [Name], [Description], [BasePrice], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [IsActive]) VALUES (N'222b05d1-da3e-464e-909f-ac2190e1d946', N'Matcha Latte', N'Matcha ', 45000, 1, 0, CAST(N'2025-08-12T08:13:39.6169572' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'550e8400-e29b-41d4-a716-446655440000', 0)
GO
INSERT [dbo].[MenuItems] ([Id], [Name], [Description], [BasePrice], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [IsActive]) VALUES (N'324cba2b-6943-4e84-acef-b3cb01ac21b3', N'Trà sữa Matcha', N'Trà sữa Matcha', 30000, 1, 0, CAST(N'2025-08-12T10:21:42.5534466' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'550e8400-e29b-41d4-a716-446655440000', 0)
GO
INSERT [dbo].[MenuItems] ([Id], [Name], [Description], [BasePrice], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [IsActive]) VALUES (N'bb3726e9-fddb-406c-b137-c7942b3b92d1', N'Trà sữa Socola', N'Trà sữa Socola', 30000, 1, 0, CAST(N'2025-08-12T10:23:10.5714818' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'550e8400-e29b-41d4-a716-446655440000', 0)
GO
INSERT [dbo].[MenuItems] ([Id], [Name], [Description], [BasePrice], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [IsActive]) VALUES (N'1fdde566-2f30-4f4a-814f-c8cd6cfb40d1', N'Phở gà', N'Phở gà', 35000, 1, 0, CAST(N'2025-08-12T08:14:25.4852816' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'550e8400-e29b-41d4-a716-446655440000', 0)
GO
INSERT [dbo].[VariantGroups] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'd832d8a4-e27d-44bb-beb3-2f20d692a60d', N'Đá', NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[VariantGroups] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'7cfcac39-7163-48c6-bb0a-3c15155b1d34', N'Sữa', NULL, N'7ab65347-49d6-42dd-a8b7-3021bccc6ad3', NULL, NULL)
GO
INSERT [dbo].[VariantGroups] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'4403be4d-3222-41c9-a368-3e181015cf07', N'Trân Trâu', NULL, N'7ab65347-49d6-42dd-a8b7-3021bccc6ad3', NULL, NULL)
GO
INSERT [dbo].[VariantGroups] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'6cc41540-dbaa-491c-bf72-6b81d7351494', N'Đường', NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[VariantGroups] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'f7bbcf3b-5a76-412c-81d4-8396b47b0cc1', N'Size', NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[VariantGroups] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'3c0dfb72-4243-4fb2-a6aa-a19a48e54528', N'Size', NULL, N'550E8400-E29B-41D4-A716-446655440000', NULL, NULL)
GO
INSERT [dbo].[VariantGroups] ([Id], [Name], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'bb0f2634-bc0c-443f-a38f-b9a77b9c9048', N'Topping', NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemVariantGroups] ([Id], [MenuItemId], [VariantGroupId], [MinSelect], [MaxSelect], [IsRequired]) VALUES (N'1b399499-4c80-4db2-85e8-141397b092e7', N'bb3726e9-fddb-406c-b137-c7942b3b92d1', N'3c0dfb72-4243-4fb2-a6aa-a19a48e54528', 1, 1, 1)
GO
INSERT [dbo].[MenuItemVariantGroups] ([Id], [MenuItemId], [VariantGroupId], [MinSelect], [MaxSelect], [IsRequired]) VALUES (N'e8043471-a19f-4535-89ea-3f9ef08b2db7', N'1fdde566-2f30-4f4a-814f-c8cd6cfb40d1', N'3c0dfb72-4243-4fb2-a6aa-a19a48e54528', 1, 1, 1)
GO
INSERT [dbo].[MenuItemVariantGroups] ([Id], [MenuItemId], [VariantGroupId], [MinSelect], [MaxSelect], [IsRequired]) VALUES (N'b5be44dc-1c72-4ece-89b1-43ddca2944c5', N'bb3726e9-fddb-406c-b137-c7942b3b92d1', N'bb0f2634-bc0c-443f-a38f-b9a77b9c9048', 0, 10, 0)
GO
INSERT [dbo].[MenuItemVariantGroups] ([Id], [MenuItemId], [VariantGroupId], [MinSelect], [MaxSelect], [IsRequired]) VALUES (N'ed8f6464-7bfe-42fb-895d-45998d3dc830', N'dc098afc-eb8b-4757-82e1-3db5d2d413db', N'3c0dfb72-4243-4fb2-a6aa-a19a48e54528', 0, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroups] ([Id], [MenuItemId], [VariantGroupId], [MinSelect], [MaxSelect], [IsRequired]) VALUES (N'9fb321b0-2bdc-4cd0-8a3f-6d22ccfca532', N'324cba2b-6943-4e84-acef-b3cb01ac21b3', N'6cc41540-dbaa-491c-bf72-6b81d7351494', 1, 1, 1)
GO
INSERT [dbo].[MenuItemVariantGroups] ([Id], [MenuItemId], [VariantGroupId], [MinSelect], [MaxSelect], [IsRequired]) VALUES (N'cbaf1be6-7489-48de-bd15-71def3b665a1', N'f6e20025-2858-47eb-96ef-9d49d0d49b1c', N'3c0dfb72-4243-4fb2-a6aa-a19a48e54528', 1, 2, 1)
GO
INSERT [dbo].[MenuItemVariantGroups] ([Id], [MenuItemId], [VariantGroupId], [MinSelect], [MaxSelect], [IsRequired]) VALUES (N'6dc2745d-126b-4219-a39f-80144dcbe447', N'bb3726e9-fddb-406c-b137-c7942b3b92d1', N'6cc41540-dbaa-491c-bf72-6b81d7351494', 1, 1, 1)
GO
INSERT [dbo].[MenuItemVariantGroups] ([Id], [MenuItemId], [VariantGroupId], [MinSelect], [MaxSelect], [IsRequired]) VALUES (N'f7c74697-45dc-451c-9339-80cc2fe3d042', N'7e2994b3-3aee-4dbf-8744-41c95fa8f73d', N'3c0dfb72-4243-4fb2-a6aa-a19a48e54528', 1, 1, 0)
GO
INSERT [dbo].[MenuItemVariantGroups] ([Id], [MenuItemId], [VariantGroupId], [MinSelect], [MaxSelect], [IsRequired]) VALUES (N'1daee17e-5c2d-4488-8568-849a5ace7530', N'324cba2b-6943-4e84-acef-b3cb01ac21b3', N'bb0f2634-bc0c-443f-a38f-b9a77b9c9048', 0, 10, 0)
GO
INSERT [dbo].[MenuItemVariantGroups] ([Id], [MenuItemId], [VariantGroupId], [MinSelect], [MaxSelect], [IsRequired]) VALUES (N'a8b1439b-c014-46d3-a0a8-a45623c102c1', N'bc067dd1-a55d-464d-9df1-0ea74b0e503c', N'3c0dfb72-4243-4fb2-a6aa-a19a48e54528', 1, 1, 1)
GO
INSERT [dbo].[MenuItemVariantGroups] ([Id], [MenuItemId], [VariantGroupId], [MinSelect], [MaxSelect], [IsRequired]) VALUES (N'b1a7deff-5091-439f-ad6a-a67740173310', N'324cba2b-6943-4e84-acef-b3cb01ac21b3', N'3c0dfb72-4243-4fb2-a6aa-a19a48e54528', 1, 1, 1)
GO
INSERT [dbo].[MenuItemVariantGroups] ([Id], [MenuItemId], [VariantGroupId], [MinSelect], [MaxSelect], [IsRequired]) VALUES (N'7943d1da-38db-47e8-8713-b282a995b535', N'324cba2b-6943-4e84-acef-b3cb01ac21b3', N'd832d8a4-e27d-44bb-beb3-2f20d692a60d', 1, 1, 1)
GO
INSERT [dbo].[MenuItemVariantGroups] ([Id], [MenuItemId], [VariantGroupId], [MinSelect], [MaxSelect], [IsRequired]) VALUES (N'08c55537-2fa0-43c5-9481-b77e3ce5cf30', N'059c4212-3f4d-4ec9-96e2-848d7224ca74', N'3c0dfb72-4243-4fb2-a6aa-a19a48e54528', 1, 1, 1)
GO
INSERT [dbo].[MenuItemVariantGroups] ([Id], [MenuItemId], [VariantGroupId], [MinSelect], [MaxSelect], [IsRequired]) VALUES (N'cfc0367f-5148-4776-829e-ca85a5ac464a', N'222b05d1-da3e-464e-909f-ac2190e1d946', N'7cfcac39-7163-48c6-bb0a-3c15155b1d34', 1, 2, 1)
GO
INSERT [dbo].[MenuItemVariantGroups] ([Id], [MenuItemId], [VariantGroupId], [MinSelect], [MaxSelect], [IsRequired]) VALUES (N'a68a2fd4-eacf-49be-b109-cc7f916b9f66', N'80185741-1f1c-40af-9eeb-80718c7b9d2c', N'3c0dfb72-4243-4fb2-a6aa-a19a48e54528', 0, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroups] ([Id], [MenuItemId], [VariantGroupId], [MinSelect], [MaxSelect], [IsRequired]) VALUES (N'e2f3f78e-8c1f-4c99-a2f5-d318f1e75def', N'8927310a-664e-46e6-965f-81e0209a96a0', N'3c0dfb72-4243-4fb2-a6aa-a19a48e54528', 0, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroups] ([Id], [MenuItemId], [VariantGroupId], [MinSelect], [MaxSelect], [IsRequired]) VALUES (N'bdedbe06-6aa7-4a0d-ae67-fcd5456b4d24', N'bb3726e9-fddb-406c-b137-c7942b3b92d1', N'd832d8a4-e27d-44bb-beb3-2f20d692a60d', 1, 1, 1)
GO
INSERT [dbo].[Promotions] ([Id], [Title], [Description], [PromotionType], [DiscountValue], [StartDate], [EndDate], [IsDeleted], [IsActive], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [AcceptForItems], [StoreId], [CanApplyCombine], [MaxDiscountValue], [PromotionScope], [CountUsed], [MaxUsage], [MaxUsagePerUser], [MinimumItemQuantity], [MinimumOrderAmount]) VALUES (N'fd4bb84e-2257-40b6-94ae-522ec00cebea', N'Opening Promotion', NULL, 0, 10, CAST(N'2025-08-13T00:00:00.0000000' AS DateTime2), CAST(N'2025-08-19T23:59:00.0000000' AS DateTime2), 0, 1, CAST(N'2025-08-12T07:21:48.1120043' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', NULL, NULL, N'[]', N'550e8400-e29b-41d4-a716-446655440000', 0, 100, 0, 0, 100, NULL, 1, 100)
GO
INSERT [dbo].[Promotions] ([Id], [Title], [Description], [PromotionType], [DiscountValue], [StartDate], [EndDate], [IsDeleted], [IsActive], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [AcceptForItems], [StoreId], [CanApplyCombine], [MaxDiscountValue], [PromotionScope], [CountUsed], [MaxUsage], [MaxUsagePerUser], [MinimumItemQuantity], [MinimumOrderAmount]) VALUES (N'd3fe1d43-8880-4e6f-927d-c8c1b18837fb', N'Buy X Get Y', NULL, 4, NULL, CAST(N'2025-08-28T17:00:00.0000000' AS DateTime2), CAST(N'2025-08-31T17:00:00.0000000' AS DateTime2), 0, 1, CAST(N'2025-08-27T02:08:06.1099826' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', NULL, NULL, N'[]', N'550e8400-e29b-41d4-a716-446655440000', 0, NULL, 0, 0, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Promotions] ([Id], [Title], [Description], [PromotionType], [DiscountValue], [StartDate], [EndDate], [IsDeleted], [IsActive], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [AcceptForItems], [StoreId], [CanApplyCombine], [MaxDiscountValue], [PromotionScope], [CountUsed], [MaxUsage], [MaxUsagePerUser], [MinimumItemQuantity], [MinimumOrderAmount]) VALUES (N'00188e81-a60c-44e4-8a4c-f7f361ed0f05', N'a', NULL, 1, 1, CAST(N'2025-09-01T17:00:00.0000000' AS DateTime2), CAST(N'2025-09-02T17:00:00.0000000' AS DateTime2), 1, 1, CAST(N'2025-08-27T02:27:08.4242882' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', CAST(N'2025-08-27T02:27:12.3555397' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'[]', N'550e8400-e29b-41d4-a716-446655440000', 0, NULL, 0, 0, 1, NULL, 1, 1)
GO
INSERT [dbo].[Promotions] ([Id], [Title], [Description], [PromotionType], [DiscountValue], [StartDate], [EndDate], [IsDeleted], [IsActive], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [AcceptForItems], [StoreId], [CanApplyCombine], [MaxDiscountValue], [PromotionScope], [CountUsed], [MaxUsage], [MaxUsagePerUser], [MinimumItemQuantity], [MinimumOrderAmount]) VALUES (N'9dac07b8-a94f-450e-9b44-fe0069a9ba88', N'Fix Amount Promotion', NULL, 1, 10, CAST(N'2025-08-28T03:00:00.0000000' AS DateTime2), CAST(N'2025-08-31T03:00:00.0000000' AS DateTime2), 0, 1, CAST(N'2025-08-27T02:03:17.3127421' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', CAST(N'2025-08-27T02:06:20.1568392' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'[]', N'550e8400-e29b-41d4-a716-446655440000', 0, NULL, 0, NULL, 100, NULL, 1, 10)
GO
INSERT [dbo].[Coupons] ([Id], [Code], [Description], [DiscountType], [Value], [StartDate], [EndDate], [MaxUsage], [CountUsed], [MaxUsagePerUser], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [PromotionId], [AcceptForItems], [MinimumItemQuantity], [MinimumOrderAmount], [CouponType]) VALUES (N'60d690ec-95d4-4730-8df8-09c10a39e8aa', N'KV47JHBF', N'a', 1, 10, CAST(N'2025-08-11T16:20:00.0000000' AS DateTime2), CAST(N'2025-08-12T10:00:00.0000000' AS DateTime2), 10000, 0, 100000, 1, 0, CAST(N'2025-08-11T16:18:41.0115949' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', CAST(N'2025-08-11T17:02:03.7879559' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'550e8400-e29b-41d4-a716-446655440000', NULL, N'[]', NULL, NULL, 1)
GO
INSERT [dbo].[Coupons] ([Id], [Code], [Description], [DiscountType], [Value], [StartDate], [EndDate], [MaxUsage], [CountUsed], [MaxUsagePerUser], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [PromotionId], [AcceptForItems], [MinimumItemQuantity], [MinimumOrderAmount], [CouponType]) VALUES (N'c06743e4-dc9d-4ab6-a9ee-236c8b3b0141', N'BUYXPRO', N'BUYXPRO', 1, 11, CAST(N'2025-08-27T19:19:00.0000000' AS DateTime2), CAST(N'2025-08-30T10:00:00.0000000' AS DateTime2), 100, 0, NULL, 1, 0, CAST(N'2025-08-27T02:19:00.4352784' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', CAST(N'2025-08-27T02:20:20.8727963' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'550e8400-e29b-41d4-a716-446655440000', N'd3fe1d43-8880-4e6f-927d-c8c1b18837fb', N'[]', NULL, NULL, 1)
GO
INSERT [dbo].[Coupons] ([Id], [Code], [Description], [DiscountType], [Value], [StartDate], [EndDate], [MaxUsage], [CountUsed], [MaxUsagePerUser], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [PromotionId], [AcceptForItems], [MinimumItemQuantity], [MinimumOrderAmount], [CouponType]) VALUES (N'ae878da9-1205-4a6d-98f3-2aad98b868cf', N'FIXPRO', N'FIXPRO', 1, 15, CAST(N'2025-08-27T02:12:00.0000000' AS DateTime2), CAST(N'2025-08-30T17:00:00.0000000' AS DateTime2), 100, 0, NULL, 1, 0, CAST(N'2025-08-27T02:11:54.8395360' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', NULL, NULL, N'550e8400-e29b-41d4-a716-446655440000', N'9dac07b8-a94f-450e-9b44-fe0069a9ba88', N'[]', NULL, NULL, 1)
GO
INSERT [dbo].[Coupons] ([Id], [Code], [Description], [DiscountType], [Value], [StartDate], [EndDate], [MaxUsage], [CountUsed], [MaxUsagePerUser], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [PromotionId], [AcceptForItems], [MinimumItemQuantity], [MinimumOrderAmount], [CouponType]) VALUES (N'7e6cd39f-2997-4c27-bbe9-447a9f897314', N'PERPRO', N'PERPRO', 0, 15, CAST(N'2025-08-27T02:11:00.0000000' AS DateTime2), CAST(N'2025-08-31T17:00:00.0000000' AS DateTime2), 100, 0, NULL, 1, 0, CAST(N'2025-08-27T02:11:07.2925497' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', NULL, NULL, N'550e8400-e29b-41d4-a716-446655440000', N'fd4bb84e-2257-40b6-94ae-522ec00cebea', N'[]', NULL, NULL, 1)
GO
INSERT [dbo].[Coupons] ([Id], [Code], [Description], [DiscountType], [Value], [StartDate], [EndDate], [MaxUsage], [CountUsed], [MaxUsagePerUser], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [PromotionId], [AcceptForItems], [MinimumItemQuantity], [MinimumOrderAmount], [CouponType]) VALUES (N'e948a1b9-fca1-43c3-a44d-4512c47449bf', N'0G7ATCRR', N'Percent', 0, 15, CAST(N'2025-08-27T02:10:00.0000000' AS DateTime2), CAST(N'2025-08-31T17:00:00.0000000' AS DateTime2), 100, 0, NULL, 1, 0, CAST(N'2025-08-27T02:10:12.7184623' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', NULL, NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, N'[]', NULL, NULL, 0)
GO
INSERT [dbo].[Coupons] ([Id], [Code], [Description], [DiscountType], [Value], [StartDate], [EndDate], [MaxUsage], [CountUsed], [MaxUsagePerUser], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [PromotionId], [AcceptForItems], [MinimumItemQuantity], [MinimumOrderAmount], [CouponType]) VALUES (N'ad133097-2fa5-4de8-a6c6-f054ea3f467a', N'KFSQAQSM', N'abc', 0, 10, CAST(N'2025-08-11T17:05:00.0000000' AS DateTime2), CAST(N'2025-08-12T10:08:00.0000000' AS DateTime2), 1000000, 0, 100000, 1, 0, CAST(N'2025-08-11T17:05:00.1293137' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', CAST(N'2025-08-11T17:12:52.2233874' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'550e8400-e29b-41d4-a716-446655440000', NULL, N'[]', NULL, NULL, 1)
GO
INSERT [dbo].[Coupons] ([Id], [Code], [Description], [DiscountType], [Value], [StartDate], [EndDate], [MaxUsage], [CountUsed], [MaxUsagePerUser], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [PromotionId], [AcceptForItems], [MinimumItemQuantity], [MinimumOrderAmount], [CouponType]) VALUES (N'af7ca855-6202-4592-8e76-feb9c89a1523', N'13QTR1P5', N'Opening Coupon', 1, 10, CAST(N'2025-08-14T00:00:00.0000000' AS DateTime2), CAST(N'2025-08-30T23:59:00.0000000' AS DateTime2), 1000, 0, NULL, 1, 0, CAST(N'2025-08-12T07:55:17.9358109' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', NULL, NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, N'[]', NULL, 50, 0)
GO
INSERT [dbo].[OrderWraps] ([Id], [OrderWrapStatus], [StoreId], [Code], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'f578acc1-5f70-4583-bf52-f9e48c453f79', 0, N'550e8400-e29b-41d4-a716-446655440000', N'1', NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Tables] ([Id], [TableNumber], [QrCode], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [Status], [QrVersion]) VALUES (N'c5bf3dc3-fbe0-409e-b860-094d13fa422f', 2, N'https://res.cloudinary.com/dpi0x4iy6/image/upload/v1754926105/stores/550e8400-e29b-41d4-a716-446655440000/tables/table_c5bf3dc3-fbe0-409e-b860-094d13fa422f_2d6eadf3-3451-44c0-8f3c-4b9224e376ba.png', 1, 0, CAST(N'2025-08-11T15:28:24.7447220' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', CAST(N'2025-08-11T18:09:04.2549035' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'550e8400-e29b-41d4-a716-446655440000', 0, 1)
GO
INSERT [dbo].[Tables] ([Id], [TableNumber], [QrCode], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [Status], [QrVersion]) VALUES (N'01b28322-8b1c-4288-b4af-0f924fd99bcb', 1, N'https://res.cloudinary.com/dpi0x4iy6/image/upload/v1756247249/stores/550e8400-e29b-41d4-a716-446655440000/tables/table_01b28322-8b1c-4288-b4af-0f924fd99bcb_077ddacb-6bd9-4dab-8593-3006ed9c5406.png', 1, 0, CAST(N'2025-08-26T22:27:29.0551628' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', CAST(N'2025-08-27T02:24:28.1522258' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'550e8400-e29b-41d4-a716-446655440000', 0, 1)
GO
INSERT [dbo].[Tables] ([Id], [TableNumber], [QrCode], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [Status], [QrVersion]) VALUES (N'94690db9-5c86-4fcc-b485-1ee69b3875c0', 1, N'href', 1, 1, NULL, NULL, CAST(N'2025-08-27T02:24:12.6204560' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'550e8400-e29b-41d4-a716-446655440000', 0, 1)
GO
INSERT [dbo].[Tables] ([Id], [TableNumber], [QrCode], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [Status], [QrVersion]) VALUES (N'7ab69da3-b8cc-4892-85f5-81dad1e96864', 3, N'https://res.cloudinary.com/dpi0x4iy6/image/upload/v1754986951/stores/550e8400-e29b-41d4-a716-446655440000/tables/table_7ab69da3-b8cc-4892-85f5-81dad1e96864_76bbd0e7-d20e-4f41-ae7f-252148ba6bac.png', 1, 0, CAST(N'2025-08-12T08:22:30.8746370' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', CAST(N'2025-08-12T08:22:32.3197336' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'550e8400-e29b-41d4-a716-446655440000', 0, 1)
GO
INSERT [dbo].[Orders] ([Id], [OrderCode], [OrderStatus], [OrderType], [PaymentStatus], [SubTotalAmout], [TaxAmount], [DiscountAmount], [TotalAmount], [CustomerNote], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [CouponId], [OrderWrapId], [UserId], [TableId], [IsDeleted], [RemainingTime]) VALUES (N'80308e3f-8096-490c-8c7e-29f7ec5a53ba', 2482, 0, 0, 0, 0, 12, 0, 12, N'', CAST(N'2025-08-11T17:06:09.5175437' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', CAST(N'2025-08-11T17:06:09.5175438' AS DateTime2), NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'94690db9-5c86-4fcc-b485-1ee69b3875c0', 0, CAST(N'00:04:00' AS Time))
GO
INSERT [dbo].[Orders] ([Id], [OrderCode], [OrderStatus], [OrderType], [PaymentStatus], [SubTotalAmout], [TaxAmount], [DiscountAmount], [TotalAmount], [CustomerNote], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [CouponId], [OrderWrapId], [UserId], [TableId], [IsDeleted], [RemainingTime]) VALUES (N'1c94b057-f449-4e29-b9fb-3584974a9d7e', 9180, 0, 0, 0, 5035, 12, 0, 5047, N'', CAST(N'2025-08-11T18:10:43.8856037' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', CAST(N'2025-08-11T18:10:43.8856037' AS DateTime2), NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'94690db9-5c86-4fcc-b485-1ee69b3875c0', 0, CAST(N'00:04:00' AS Time))
GO
INSERT [dbo].[Orders] ([Id], [OrderCode], [OrderStatus], [OrderType], [PaymentStatus], [SubTotalAmout], [TaxAmount], [DiscountAmount], [TotalAmount], [CustomerNote], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [CouponId], [OrderWrapId], [UserId], [TableId], [IsDeleted], [RemainingTime]) VALUES (N'9ac757b9-f19e-4386-9fed-364c603e3ec5', 7098, 0, 0, 0, 5035, 12, 0, 5047, N'', CAST(N'2025-08-12T04:12:31.2641948' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', CAST(N'2025-08-12T04:12:31.2641949' AS DateTime2), NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'94690db9-5c86-4fcc-b485-1ee69b3875c0', 0, CAST(N'00:04:00' AS Time))
GO
INSERT [dbo].[Orders] ([Id], [OrderCode], [OrderStatus], [OrderType], [PaymentStatus], [SubTotalAmout], [TaxAmount], [DiscountAmount], [TotalAmount], [CustomerNote], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [CouponId], [OrderWrapId], [UserId], [TableId], [IsDeleted], [RemainingTime]) VALUES (N'2f00dede-4ae6-41a7-948e-3beb5d6080b6', 8311, 0, 0, 0, 5035, 12, 0, 5047, N'', CAST(N'2025-08-11T16:09:40.7826316' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', CAST(N'2025-08-11T16:09:40.7826316' AS DateTime2), NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'c5bf3dc3-fbe0-409e-b860-094d13fa422f', 0, CAST(N'00:04:00' AS Time))
GO
INSERT [dbo].[Orders] ([Id], [OrderCode], [OrderStatus], [OrderType], [PaymentStatus], [SubTotalAmout], [TaxAmount], [DiscountAmount], [TotalAmount], [CustomerNote], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [CouponId], [OrderWrapId], [UserId], [TableId], [IsDeleted], [RemainingTime]) VALUES (N'550e8400-e29b-41d4-a716-446655440022', 1231, 0, 1, 1, 1000, 100, 12, 121312, N'note', CAST(N'2025-11-11T00:00:00.0000000' AS DateTime2), N'2025-11-11 00:00:00.0000000', CAST(N'2025-11-11T00:00:00.0000000' AS DateTime2), NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'00000000-0000-0000-0000-000000000000', NULL, 1, NULL)
GO
INSERT [dbo].[Orders] ([Id], [OrderCode], [OrderStatus], [OrderType], [PaymentStatus], [SubTotalAmout], [TaxAmount], [DiscountAmount], [TotalAmount], [CustomerNote], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [CouponId], [OrderWrapId], [UserId], [TableId], [IsDeleted], [RemainingTime]) VALUES (N'8d44f8ee-16ea-4e14-8f73-4492384f0347', 6072, 1, 0, 1, 5035, 12, 0, 5047, N'', CAST(N'2025-08-11T09:20:22.7464411' AS DateTime2), N'f3c3b4e8-2b7f-4f47-b6a1-0ec2b7a5f8a1', CAST(N'2025-08-11T09:20:22.7464421' AS DateTime2), NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'f3c3b4e8-2b7f-4f47-b6a1-0ec2b7a5f8a1', N'94690db9-5c86-4fcc-b485-1ee69b3875c0', 0, CAST(N'00:02:00' AS Time))
GO
INSERT [dbo].[Orders] ([Id], [OrderCode], [OrderStatus], [OrderType], [PaymentStatus], [SubTotalAmout], [TaxAmount], [DiscountAmount], [TotalAmount], [CustomerNote], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [CouponId], [OrderWrapId], [UserId], [TableId], [IsDeleted], [RemainingTime]) VALUES (N'8bb4aafd-ef52-46c9-8d13-646ebd6e7a63', 3281, 0, 0, 0, 5035, 12, 0, 5047, N'', CAST(N'2025-08-11T15:47:48.1852689' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', CAST(N'2025-08-11T15:47:48.1852697' AS DateTime2), NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'c5bf3dc3-fbe0-409e-b860-094d13fa422f', 0, CAST(N'00:04:00' AS Time))
GO
INSERT [dbo].[Orders] ([Id], [OrderCode], [OrderStatus], [OrderType], [PaymentStatus], [SubTotalAmout], [TaxAmount], [DiscountAmount], [TotalAmount], [CustomerNote], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [CouponId], [OrderWrapId], [UserId], [TableId], [IsDeleted], [RemainingTime]) VALUES (N'28b45ab6-8721-4cdf-b722-6574feffaefa', 3233, 0, 0, 0, 5035, 12, 0, 5047, N'', CAST(N'2025-08-11T04:26:18.2769203' AS DateTime2), N'f3c3b4e8-2b7f-4f47-b6a1-0ec2b7a5f8a1', CAST(N'2025-08-11T04:26:18.2769222' AS DateTime2), NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'f3c3b4e8-2b7f-4f47-b6a1-0ec2b7a5f8a1', N'94690db9-5c86-4fcc-b485-1ee69b3875c0', 0, NULL)
GO
INSERT [dbo].[Orders] ([Id], [OrderCode], [OrderStatus], [OrderType], [PaymentStatus], [SubTotalAmout], [TaxAmount], [DiscountAmount], [TotalAmount], [CustomerNote], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [CouponId], [OrderWrapId], [UserId], [TableId], [IsDeleted], [RemainingTime]) VALUES (N'f41bf68d-0e24-495c-bb38-65b487181554', 3702, 0, 0, 0, 5035, 12, 0, 5047, N'', CAST(N'2025-08-11T15:51:56.3991680' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', CAST(N'2025-08-11T15:51:56.3991684' AS DateTime2), NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'c5bf3dc3-fbe0-409e-b860-094d13fa422f', 0, CAST(N'00:04:00' AS Time))
GO
INSERT [dbo].[Orders] ([Id], [OrderCode], [OrderStatus], [OrderType], [PaymentStatus], [SubTotalAmout], [TaxAmount], [DiscountAmount], [TotalAmount], [CustomerNote], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [CouponId], [OrderWrapId], [UserId], [TableId], [IsDeleted], [RemainingTime]) VALUES (N'ba1d2088-2292-4750-a4b6-74dd7269b594', 7102, 0, 0, 0, 40290, 12, 0, 40302, N'', CAST(N'2025-08-12T10:28:05.1253733' AS DateTime2), N'f3c3b4e8-2b7f-4f47-b6a1-0ec2b7a5f8a1', CAST(N'2025-08-12T10:28:05.1253735' AS DateTime2), NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'f3c3b4e8-2b7f-4f47-b6a1-0ec2b7a5f8a1', N'94690db9-5c86-4fcc-b485-1ee69b3875c0', 0, CAST(N'01:34:00' AS Time))
GO
INSERT [dbo].[Orders] ([Id], [OrderCode], [OrderStatus], [OrderType], [PaymentStatus], [SubTotalAmout], [TaxAmount], [DiscountAmount], [TotalAmount], [CustomerNote], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [CouponId], [OrderWrapId], [UserId], [TableId], [IsDeleted], [RemainingTime]) VALUES (N'a1e920ec-fc8e-47f3-bc09-877c9bd0eb36', 4471, 0, 0, 0, 5001.212, 12, 0, 5013.212, N'', CAST(N'2025-08-11T16:04:09.3863161' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', CAST(N'2025-08-11T16:04:09.3863165' AS DateTime2), NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'c5bf3dc3-fbe0-409e-b860-094d13fa422f', 0, CAST(N'00:07:00' AS Time))
GO
INSERT [dbo].[Orders] ([Id], [OrderCode], [OrderStatus], [OrderType], [PaymentStatus], [SubTotalAmout], [TaxAmount], [DiscountAmount], [TotalAmount], [CustomerNote], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [CouponId], [OrderWrapId], [UserId], [TableId], [IsDeleted], [RemainingTime]) VALUES (N'6acdb96e-01de-49ff-b1be-920d3ae477b7', 4409, 0, 0, 0, 0, 12, 0, 12, N'', CAST(N'2025-08-11T17:34:06.4567991' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', CAST(N'2025-08-11T17:34:06.4567991' AS DateTime2), NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'94690db9-5c86-4fcc-b485-1ee69b3875c0', 0, CAST(N'00:04:00' AS Time))
GO
INSERT [dbo].[Orders] ([Id], [OrderCode], [OrderStatus], [OrderType], [PaymentStatus], [SubTotalAmout], [TaxAmount], [DiscountAmount], [TotalAmount], [CustomerNote], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [CouponId], [OrderWrapId], [UserId], [TableId], [IsDeleted], [RemainingTime]) VALUES (N'8f767744-fada-49da-aa73-9879aa1b73aa', 9173, 0, 0, 0, 5035, 12, 0, 5047, N'', CAST(N'2025-08-11T18:25:31.6529479' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', CAST(N'2025-08-11T18:25:31.6529480' AS DateTime2), NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'94690db9-5c86-4fcc-b485-1ee69b3875c0', 0, CAST(N'00:04:00' AS Time))
GO
INSERT [dbo].[Orders] ([Id], [OrderCode], [OrderStatus], [OrderType], [PaymentStatus], [SubTotalAmout], [TaxAmount], [DiscountAmount], [TotalAmount], [CustomerNote], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [CouponId], [OrderWrapId], [UserId], [TableId], [IsDeleted], [RemainingTime]) VALUES (N'a4759ca1-1e50-45ed-a296-98eaada13401', 7014, 0, 0, 0, 0, 12, 0, 12, N'', CAST(N'2025-08-12T13:14:20.9196046' AS DateTime2), N'f3c3b4e8-2b7f-4f47-b6a1-0ec2b7a5f8a1', CAST(N'2025-08-12T13:14:20.9196049' AS DateTime2), NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'f3c3b4e8-2b7f-4f47-b6a1-0ec2b7a5f8a1', N'94690db9-5c86-4fcc-b485-1ee69b3875c0', 0, CAST(N'01:32:00' AS Time))
GO
INSERT [dbo].[Orders] ([Id], [OrderCode], [OrderStatus], [OrderType], [PaymentStatus], [SubTotalAmout], [TaxAmount], [DiscountAmount], [TotalAmount], [CustomerNote], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [CouponId], [OrderWrapId], [UserId], [TableId], [IsDeleted], [RemainingTime]) VALUES (N'7689629d-dc1e-48a8-bf38-a5c0c08c4d87', 5999, 0, 0, 0, 0, 12, 0, 12, N'', CAST(N'2025-08-11T17:48:07.5063360' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', CAST(N'2025-08-11T17:48:07.5063361' AS DateTime2), NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'94690db9-5c86-4fcc-b485-1ee69b3875c0', 0, CAST(N'00:04:00' AS Time))
GO
INSERT [dbo].[Orders] ([Id], [OrderCode], [OrderStatus], [OrderType], [PaymentStatus], [SubTotalAmout], [TaxAmount], [DiscountAmount], [TotalAmount], [CustomerNote], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [CouponId], [OrderWrapId], [UserId], [TableId], [IsDeleted], [RemainingTime]) VALUES (N'03e80548-400e-4018-a666-bdfeb13d7400', 4764, 0, 0, 0, 0, 12, 0, 12, N'', CAST(N'2025-08-11T17:42:32.6333597' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', CAST(N'2025-08-11T17:42:32.6333598' AS DateTime2), NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'94690db9-5c86-4fcc-b485-1ee69b3875c0', 0, CAST(N'00:04:00' AS Time))
GO
INSERT [dbo].[Orders] ([Id], [OrderCode], [OrderStatus], [OrderType], [PaymentStatus], [SubTotalAmout], [TaxAmount], [DiscountAmount], [TotalAmount], [CustomerNote], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId], [CouponId], [OrderWrapId], [UserId], [TableId], [IsDeleted], [RemainingTime]) VALUES (N'40aff19f-5fd3-4ca3-a3ea-f60866fb7ec6', 7744, 1, 0, 1, 100000, 12, 0, 100012, N'', CAST(N'2025-08-12T14:17:59.2466505' AS DateTime2), N'f3c3b4e8-2b7f-4f47-b6a1-0ec2b7a5f8a1', CAST(N'2025-08-12T14:17:59.2466510' AS DateTime2), NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, N'f578acc1-5f70-4583-bf52-f9e48c453f79', N'f3c3b4e8-2b7f-4f47-b6a1-0ec2b7a5f8a1', N'94690db9-5c86-4fcc-b485-1ee69b3875c0', 0, CAST(N'01:49:00' AS Time))
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'6e299aaf-6250-4ed8-899b-08ddd88db885', N'L', 5000, 1, 0, NULL, N'550e8400-e29b-41d4-a716-446655440000', CAST(N'2025-08-12T10:01:41.2570282' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'9f59c3be-ea8c-4748-6a91-08ddd9718f10', N'M', 0, 1, 0, NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'3c0dfb72-4243-4fb2-a6aa-a19a48e54528', NULL)
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'5a0e1987-311f-4ba7-6a92-08ddd9718f10', N'Ex', 10000, 1, 0, NULL, N'550e8400-e29b-41d4-a716-446655440000', CAST(N'2025-08-12T08:08:46.2703891' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', N'3c0dfb72-4243-4fb2-a6aa-a19a48e54528', NULL)
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'ff67f466-8846-485a-e393-08ddd9775747', N'Trân Trâu Trắng', 5000, 1, 0, NULL, N'7ab65347-49d6-42dd-a8b7-3021bccc6ad3', NULL, NULL, N'4403be4d-3222-41c9-a368-3e181015cf07', NULL)
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'd418cd64-1701-462e-e394-08ddd9775747', N'Trân Trâu Đen', 5000, 1, 0, NULL, N'7ab65347-49d6-42dd-a8b7-3021bccc6ad3', CAST(N'2025-08-12T08:08:14.0032354' AS DateTime2), N'7ab65347-49d6-42dd-a8b7-3021bccc6ad3', N'4403be4d-3222-41c9-a368-3e181015cf07', NULL)
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'a8a53c25-e9ef-4cf4-e395-08ddd9775747', N'Sữa bò', 10000, 1, 0, NULL, N'7ab65347-49d6-42dd-a8b7-3021bccc6ad3', NULL, NULL, N'7cfcac39-7163-48c6-bb0a-3c15155b1d34', NULL)
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'4a7294ce-515d-46fe-e396-08ddd9775747', N'Sữa tươi', 20000, 1, 0, NULL, N'7ab65347-49d6-42dd-a8b7-3021bccc6ad3', NULL, NULL, N'7cfcac39-7163-48c6-bb0a-3c15155b1d34', NULL)
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'063b0da7-32c3-4342-e397-08ddd9775747', N'100% Đường', 0, 1, 0, NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'6cc41540-dbaa-491c-bf72-6b81d7351494', NULL)
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'1e8606f1-8f20-4dfb-e398-08ddd9775747', N'70% Đường', 0, 1, 0, NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'6cc41540-dbaa-491c-bf72-6b81d7351494', NULL)
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'7c3e501c-9dc6-4fd2-e399-08ddd9775747', N'50% Đường', 0, 1, 0, NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'6cc41540-dbaa-491c-bf72-6b81d7351494', NULL)
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'0381461f-07de-4599-e39a-08ddd9775747', N'30% Đường', 0, 1, 0, NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'6cc41540-dbaa-491c-bf72-6b81d7351494', NULL)
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'9c520794-2da2-4368-e39b-08ddd9775747', N'0% Đường', 0, 1, 0, NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'6cc41540-dbaa-491c-bf72-6b81d7351494', NULL)
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'e632e6f5-af4f-4a91-e39c-08ddd9775747', N'100% Đá', 0, 1, 0, NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'd832d8a4-e27d-44bb-beb3-2f20d692a60d', NULL)
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'b90b7d26-de84-4469-e39d-08ddd9775747', N'70% Đá', 0, 1, 0, NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'd832d8a4-e27d-44bb-beb3-2f20d692a60d', NULL)
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'87b50046-4e82-4359-e39e-08ddd9775747', N'50% Đá', 0, 1, 0, NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'd832d8a4-e27d-44bb-beb3-2f20d692a60d', NULL)
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'423aa0a3-fa73-4312-e39f-08ddd9775747', N'30% Đá', 0, 1, 0, NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'd832d8a4-e27d-44bb-beb3-2f20d692a60d', NULL)
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'faeefa32-2fb1-48bc-e3a0-08ddd9775747', N'0% Đá', 0, 1, 0, NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'd832d8a4-e27d-44bb-beb3-2f20d692a60d', NULL)
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'57cc8b1e-8abe-4b6e-e3a1-08ddd9775747', N'Pudding', 5000, 1, 0, NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'bb0f2634-bc0c-443f-a38f-b9a77b9c9048', NULL)
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'50080ef0-098c-4068-e3a2-08ddd9775747', N'Thạch', 5000, 1, 0, NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'bb0f2634-bc0c-443f-a38f-b9a77b9c9048', NULL)
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'8d4d9d62-e8da-470e-e3a3-08ddd9775747', N'Trân Châu', 5000, 1, 0, NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'bb0f2634-bc0c-443f-a38f-b9a77b9c9048', NULL)
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'5ac70e0a-9cbc-4933-e3a4-08ddd9775747', N'Kem Cheese', 5000, 1, 0, NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, N'bb0f2634-bc0c-443f-a38f-b9a77b9c9048', NULL)
GO
INSERT [dbo].[MenuItemVariants] ([Id], [Name], [Price], [IsAvailable], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [VariantGroupId], [OrderDetailId]) VALUES (N'83704130-b5f4-46e6-0989-08dde4ef5362', N'L', 5000, 1, 0, NULL, N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'70d6999f-7f8d-444e-9f02-02b69710869e', N'f7c74697-45dc-451c-9339-80cc2fe3d042', N'9f59c3be-ea8c-4748-6a91-08ddd9718f10', 1, 1, 10, 3)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'6f6bcc8f-dbed-4322-90f4-0abb66d41bc8', N'08c55537-2fa0-43c5-9481-b77e3ce5cf30', N'6e299aaf-6250-4ed8-899b-08ddd88db885', 1, 1, 10, 3)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'ec27cec6-7416-4930-b76e-0c90f3719e0f', N'9fb321b0-2bdc-4cd0-8a3f-6d22ccfca532', N'7c3e501c-9dc6-4fd2-e399-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'659bcfcc-8e40-42a9-bbbb-0e7fae1bcd1c', N'9fb321b0-2bdc-4cd0-8a3f-6d22ccfca532', N'063b0da7-32c3-4342-e397-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'036d631c-d003-49e9-9363-1349e86e5b4d', N'bdedbe06-6aa7-4a0d-ae67-fcd5456b4d24', N'faeefa32-2fb1-48bc-e3a0-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'48afd96c-8ee0-4d09-9457-137850ba4bac', N'e2f3f78e-8c1f-4c99-a2f5-d318f1e75def', N'6e299aaf-6250-4ed8-899b-08ddd88db885', 1, 1, 10, 3)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'b2d8326c-e240-4c4f-9e3e-2824b8c52db9', N'b5be44dc-1c72-4ece-89b1-43ddca2944c5', N'57cc8b1e-8abe-4b6e-e3a1-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'c9170798-94a4-48a2-8c62-3222fad0e951', N'e8043471-a19f-4535-89ea-3f9ef08b2db7', N'5a0e1987-311f-4ba7-6a92-08ddd9718f10', 1, 1, 15, 2)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'f693dd05-8223-4c2d-aaf7-3ae96a8bf8f9', N'1daee17e-5c2d-4488-8568-849a5ace7530', N'50080ef0-098c-4068-e3a2-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'5587c2fe-48b8-4cf2-a255-46e95a097507', N'b1a7deff-5091-439f-ad6a-a67740173310', N'5a0e1987-311f-4ba7-6a92-08ddd9718f10', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'1206f7fd-eb1c-456d-834e-4b5f98a78e8b', N'9fb321b0-2bdc-4cd0-8a3f-6d22ccfca532', N'9c520794-2da2-4368-e39b-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'27ba6be8-a6ec-45b1-befa-4e61a38adf0c', N'1b399499-4c80-4db2-85e8-141397b092e7', N'5a0e1987-311f-4ba7-6a92-08ddd9718f10', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'03c19968-02b6-40a1-a555-58c1a4ebdeef', N'6dc2745d-126b-4219-a39f-80144dcbe447', N'0381461f-07de-4599-e39a-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'bff6189e-26a9-48e6-85b9-59e6f148eae4', N'1daee17e-5c2d-4488-8568-849a5ace7530', N'57cc8b1e-8abe-4b6e-e3a1-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'31851526-d70a-4d88-944d-5da94b8466d2', N'bdedbe06-6aa7-4a0d-ae67-fcd5456b4d24', N'87b50046-4e82-4359-e39e-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'bbe6e60a-0ad1-4bfc-a7bf-61cff798a78e', N'a8b1439b-c014-46d3-a0a8-a45623c102c1', N'6e299aaf-6250-4ed8-899b-08ddd88db885', 1, 1, 10, 3)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'408ff570-60e3-4b09-85a1-61d5db764d1a', N'e2f3f78e-8c1f-4c99-a2f5-d318f1e75def', N'9f59c3be-ea8c-4748-6a91-08ddd9718f10', 1, 1, 10, 3)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'117ca5ae-71a0-4243-b851-644ffe885c88', N'1daee17e-5c2d-4488-8568-849a5ace7530', N'5ac70e0a-9cbc-4933-e3a4-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'afe0a683-53df-44be-a7ef-6685d4b0b875', N'b5be44dc-1c72-4ece-89b1-43ddca2944c5', N'50080ef0-098c-4068-e3a2-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'c68f3c09-ead5-4309-badf-6b74fbefa222', N'a68a2fd4-eacf-49be-b109-cc7f916b9f66', N'9f59c3be-ea8c-4748-6a91-08ddd9718f10', 1, 1, 10, 3)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'246040c5-2a6d-42c0-a1ec-79f57c1e28a3', N'b5be44dc-1c72-4ece-89b1-43ddca2944c5', N'8d4d9d62-e8da-470e-e3a3-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'40a8fc98-9918-4962-bdb7-80403fdf6a7a', N'08c55537-2fa0-43c5-9481-b77e3ce5cf30', N'5a0e1987-311f-4ba7-6a92-08ddd9718f10', 1, 1, 15, 3)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'12c34012-085f-426e-a9f7-8146778691b6', N'a8b1439b-c014-46d3-a0a8-a45623c102c1', N'9f59c3be-ea8c-4748-6a91-08ddd9718f10', 1, 1, 10, 3)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'b9af8ac8-83be-4160-986b-844d1bc5463f', N'6dc2745d-126b-4219-a39f-80144dcbe447', N'063b0da7-32c3-4342-e397-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'261cbb74-1578-4322-a246-84a1ee1090d6', N'bdedbe06-6aa7-4a0d-ae67-fcd5456b4d24', N'b90b7d26-de84-4469-e39d-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'38f1393c-12ec-4875-a97c-877b628dc306', N'6dc2745d-126b-4219-a39f-80144dcbe447', N'1e8606f1-8f20-4dfb-e398-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'39097074-55ed-4fac-8829-8e9078854349', N'a68a2fd4-eacf-49be-b109-cc7f916b9f66', N'6e299aaf-6250-4ed8-899b-08ddd88db885', 1, 1, 10, 3)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'364eeb0d-9aa6-4205-ac9c-923678eed123', N'1b399499-4c80-4db2-85e8-141397b092e7', N'6e299aaf-6250-4ed8-899b-08ddd88db885', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'feedab3f-1f98-41e3-b1e3-946c64520522', N'f7c74697-45dc-451c-9339-80cc2fe3d042', N'6e299aaf-6250-4ed8-899b-08ddd88db885', 1, 1, 10, 3)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'c45a78ed-49e8-4230-8ca4-946d6ad4b0e6', N'e2f3f78e-8c1f-4c99-a2f5-d318f1e75def', N'5a0e1987-311f-4ba7-6a92-08ddd9718f10', 1, 1, 10, 3)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'f7f6313f-57dc-4ca5-8ead-97c2d0d582e1', N'bdedbe06-6aa7-4a0d-ae67-fcd5456b4d24', N'e632e6f5-af4f-4a91-e39c-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'59f9b27b-8f2b-418d-9ce4-97e8320301cd', N'f7c74697-45dc-451c-9339-80cc2fe3d042', N'5a0e1987-311f-4ba7-6a92-08ddd9718f10', 1, 1, 10, 3)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'30431f26-ae84-48da-a13d-9c109cd5da7d', N'7943d1da-38db-47e8-8713-b282a995b535', N'423aa0a3-fa73-4312-e39f-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'94730eed-3060-4494-adb9-9d36c5d9b1fe', N'9fb321b0-2bdc-4cd0-8a3f-6d22ccfca532', N'1e8606f1-8f20-4dfb-e398-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'8e77c393-5147-4054-92ad-abae6dfa3e6a', N'e8043471-a19f-4535-89ea-3f9ef08b2db7', N'9f59c3be-ea8c-4748-6a91-08ddd9718f10', 1, 1, 10, 3)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'eae8610f-6f84-4d12-a5a1-adff015ff658', N'7943d1da-38db-47e8-8713-b282a995b535', N'b90b7d26-de84-4469-e39d-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'e869a604-b842-41ea-8f0b-ba98b81f2b7d', N'a68a2fd4-eacf-49be-b109-cc7f916b9f66', N'5a0e1987-311f-4ba7-6a92-08ddd9718f10', 1, 1, 15, 2)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'5e2e367d-3b55-406c-8618-c1dd1155686a', N'bdedbe06-6aa7-4a0d-ae67-fcd5456b4d24', N'423aa0a3-fa73-4312-e39f-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'ae1affc5-9370-49fa-aa53-c4eb641bb2c9', N'08c55537-2fa0-43c5-9481-b77e3ce5cf30', N'9f59c3be-ea8c-4748-6a91-08ddd9718f10', 1, 1, 10, 3)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'087322c3-4475-4e83-86d0-c5863a6995a0', N'ed8f6464-7bfe-42fb-895d-45998d3dc830', N'6e299aaf-6250-4ed8-899b-08ddd88db885', 1, 1, 5, 1)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'622bd5b7-bf3f-4d76-8872-cbe78657f1a7', N'cfc0367f-5148-4776-829e-ca85a5ac464a', N'4a7294ce-515d-46fe-e396-08ddd9775747', 1, 1, 1, 1)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'd7e26bb1-23c9-4e42-b8e6-cc074d74cb23', N'cbaf1be6-7489-48de-bd15-71def3b665a1', N'6e299aaf-6250-4ed8-899b-08ddd88db885', 1, 1, 2, 1)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'838998f4-6a0a-49bb-adff-cdf8797a81bf', N'7943d1da-38db-47e8-8713-b282a995b535', N'87b50046-4e82-4359-e39e-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'54d37c3a-3fc6-48ee-9deb-d2a26925fdc1', N'6dc2745d-126b-4219-a39f-80144dcbe447', N'7c3e501c-9dc6-4fd2-e399-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'a2aa45e6-1098-49eb-b3eb-d332f2b65e87', N'b1a7deff-5091-439f-ad6a-a67740173310', N'9f59c3be-ea8c-4748-6a91-08ddd9718f10', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'c7b1c13c-70a9-43ed-b0bb-d54a201f9d68', N'1daee17e-5c2d-4488-8568-849a5ace7530', N'8d4d9d62-e8da-470e-e3a3-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'78d04c38-c721-4e8d-8643-e4277f08e3cc', N'7943d1da-38db-47e8-8713-b282a995b535', N'faeefa32-2fb1-48bc-e3a0-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'cdbc53bb-0113-4fea-b795-e604c0830005', N'7943d1da-38db-47e8-8713-b282a995b535', N'e632e6f5-af4f-4a91-e39c-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'738562b8-f00d-4453-beb8-e70e39f265a5', N'6dc2745d-126b-4219-a39f-80144dcbe447', N'9c520794-2da2-4368-e39b-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'a1b6c0c6-6514-4bcd-acda-e8d447a332db', N'9fb321b0-2bdc-4cd0-8a3f-6d22ccfca532', N'0381461f-07de-4599-e39a-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'b8317e3e-ca05-478f-8284-f5712e1ded40', N'1b399499-4c80-4db2-85e8-141397b092e7', N'9f59c3be-ea8c-4748-6a91-08ddd9718f10', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'e08c9242-bbc1-4620-819b-f6896ca6e2d3', N'b5be44dc-1c72-4ece-89b1-43ddca2944c5', N'5ac70e0a-9cbc-4933-e3a4-08ddd9775747', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'ad6e399d-bb92-4fc3-9ef3-f6f0844e7092', N'a8b1439b-c014-46d3-a0a8-a45623c102c1', N'5a0e1987-311f-4ba7-6a92-08ddd9718f10', 1, 1, 15, 3)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'60467dc5-281b-421a-a240-fc1f82a48034', N'cfc0367f-5148-4776-829e-ca85a5ac464a', N'a8a53c25-e9ef-4cf4-e395-08ddd9775747', 1, 1, 1, 1)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'b7801aca-196b-462d-bc43-fcac3b1666e4', N'b1a7deff-5091-439f-ad6a-a67740173310', N'6e299aaf-6250-4ed8-899b-08ddd88db885', 1, 1, 0, 0)
GO
INSERT [dbo].[MenuItemVariantGroupItems] ([Id], [MenuItemVariantGroupId], [MenuItemVariantId], [IsActive], [IsAvailable], [PrepPerTime], [QuantityPerTime]) VALUES (N'2c4dffa2-537f-4a32-b723-ff09c1b346d9', N'e8043471-a19f-4535-89ea-3f9ef08b2db7', N'6e299aaf-6250-4ed8-899b-08ddd88db885', 1, 1, 10, 3)
GO
INSERT [dbo].[Workshifts] ([Id], [StoreId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [WorkDate]) VALUES (N'a3c73891-5c8e-406e-823d-a4cfe7da4ee4', N'550e8400-e29b-41d4-a716-446655440000', CAST(N'2025-08-11T05:06:06.7689794' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, CAST(N'2025-08-12T00:00:00.0000000' AS DateTime2))
GO
INSERT [dbo].[WorkshiftSchedules] ([Id], [StoreId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [EndTime], [IsActive], [Name], [StartTime], [WorkshiftId]) VALUES (N'778fcff9-2f1a-4a2e-af86-80bd6181d106', N'550e8400-e29b-41d4-a716-446655440000', CAST(N'2025-08-11T05:07:07.9275789' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, CAST(N'15:09:00' AS Time), 1, N'2 Nguyễn', CAST(N'14:08:00' AS Time), N'a3c73891-5c8e-406e-823d-a4cfe7da4ee4')
GO
INSERT [dbo].[WorkshiftSchedules] ([Id], [StoreId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [EndTime], [IsActive], [Name], [StartTime], [WorkshiftId]) VALUES (N'2195cb2a-0515-4ddc-8bac-f1e7386033ab', N'550e8400-e29b-41d4-a716-446655440000', CAST(N'2025-08-11T05:06:06.8216663' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL, CAST(N'14:06:00' AS Time), 1, N'Hồng Phúc Nguyễn', CAST(N'12:05:00' AS Time), N'a3c73891-5c8e-406e-823d-a4cfe7da4ee4')
GO
INSERT [dbo].[PromotionItemConditions] ([Id], [PromotionId], [BuyItemId], [BuyQuantity], [GetItemId], [GetQuantity]) VALUES (N'd5726d5c-4e24-4e63-ac82-6754f9443e58', N'd3fe1d43-8880-4e6f-927d-c8c1b18837fb', N'324cba2b-6943-4e84-acef-b3cb01ac21b3', 1, N'bb3726e9-fddb-406c-b137-c7942b3b92d1', 1)
GO
INSERT [dbo].[MenuCategories] ([Id], [Name], [Description], [SortOrder], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId]) VALUES (N'f4ee3653-e66a-4832-8414-1b25c67e6950', N'Cơm rang', N'', 0, 1, 0, NULL, NULL, NULL, NULL, N'550e8400-e29b-41d4-a716-446655440000')
GO
INSERT [dbo].[MenuCategories] ([Id], [Name], [Description], [SortOrder], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId]) VALUES (N'7b40e47a-26fd-4533-a080-254cf1f9268f', N'Phở', N'', 0, 1, 0, NULL, NULL, NULL, NULL, N'550e8400-e29b-41d4-a716-446655440000')
GO
INSERT [dbo].[MenuCategories] ([Id], [Name], [Description], [SortOrder], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId]) VALUES (N'8856fab4-35ff-4ada-8801-4796647afead', N'Món ăn kèm', N'', 0, 1, 0, NULL, NULL, NULL, NULL, N'550e8400-e29b-41d4-a716-446655440000')
GO
INSERT [dbo].[MenuCategories] ([Id], [Name], [Description], [SortOrder], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId]) VALUES (N'e517ef68-9f75-4a05-adef-48aad209b5a4', N'Món chính', N'Món chính', 0, 1, 0, NULL, NULL, NULL, NULL, N'550e8400-e29b-41d4-a716-446655440000')
GO
INSERT [dbo].[MenuCategories] ([Id], [Name], [Description], [SortOrder], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId]) VALUES (N'75cfa08c-f49b-46e7-8325-6476f1e4146b', N'Đồ nóng', N'Đồ uống ấm, nóng ', 0, 1, 0, NULL, NULL, NULL, NULL, N'7ab65347-49d6-42dd-a8b7-3021bccc6ad3')
GO
INSERT [dbo].[MenuCategories] ([Id], [Name], [Description], [SortOrder], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId]) VALUES (N'0a0d7f0c-f050-46b2-8c1b-91e8dd233d03', N'Đồ lạnh', N'Đồ lạnh - mát', 0, 1, 0, NULL, NULL, NULL, NULL, N'7ab65347-49d6-42dd-a8b7-3021bccc6ad3')
GO
INSERT [dbo].[MenuCategories] ([Id], [Name], [Description], [SortOrder], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId]) VALUES (N'e70c8fdf-0a20-4b98-be49-93dc693debe3', N'Món chính', N'Món chính', 0, 1, 0, NULL, NULL, NULL, NULL, N'550e8400-e29b-41d4-a716-446655440000')
GO
INSERT [dbo].[MenuCategories] ([Id], [Name], [Description], [SortOrder], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId]) VALUES (N'056f3587-1a79-4319-82c9-a98f83d3dbb0', N'Món khô', N'Món khô', 0, 1, 0, NULL, NULL, NULL, NULL, N'550e8400-e29b-41d4-a716-446655440000')
GO
INSERT [dbo].[MenuCategories] ([Id], [Name], [Description], [SortOrder], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId]) VALUES (N'8649777c-05cb-4a30-acca-c15d76dc257b', N'Trà sữa', N'Trà sữa', 0, 1, 0, NULL, NULL, NULL, NULL, N'550e8400-e29b-41d4-a716-446655440000')
GO
INSERT [dbo].[MenuCategories] ([Id], [Name], [Description], [SortOrder], [IsActive], [IsDeleted], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [StoreId]) VALUES (N'c4f38982-6e41-4b61-9169-f37c02f70262', N'Món nước', N'Bún, phở, ...', 0, 1, 0, NULL, NULL, NULL, NULL, N'550e8400-e29b-41d4-a716-446655440000')
GO
INSERT [dbo].[StoreSettings] ([Id], [OpenTime], [CloseTime], [Currency], [PaymentConfig], [LogoUrl], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsActive], [IsDeleted], [StoreId], [IsSelfService], [discountStrategy], [AllowCombinePromotionAndCoupon], [SpendingRate], [PayOSApiKey], [PayOSChecksumKey], [PayOSClientId]) VALUES (N'550e8400-e29b-41d4-a716-446655440003', CAST(N'07:00:00' AS Time), CAST(N'23:00:00' AS Time), N'VND', 1, N'url.com', NULL, NULL, CAST(N'2025-08-12T08:54:35.2978497' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', 1, 0, N'550e8400-e29b-41d4-a716-446655440000', 1, 0, 1, 2, N'CfDJ8AP-oRfx4X9NsA08qQZ58JsiuyiRy5RIMCy1ub0PeHxuchBL05VyhNu3Z8FHxvGXEXEKfuaNvTNdQlE76eKK6Nx5dSupaySrGM_h3A3fu4FbeB_pwa6ig4azFqGoWjxBLgrrwFZ-qxrwpyDS2KeHaPHUi5jhgfoX99qIp3Kz8qVU', N'CfDJ8AP-oRfx4X9NsA08qQZ58JsQXY17IueCnKACsYZoky7qTEc_iV5s8NNGmMrvPC-hCmipPJ5iCcEaznQ1jyWolexdk7_x4S5dO_3Yfs61ZXxGlPLhfSXGZzQBfLcWIfdRnpHvd1N42Vg26yM8jTXx2CysY90h-eN7uxspWZlgt8QrvQsiQSl-j_0iI-KNo39itwXUlVLuCUcf4mtlS-95WlM', N'CfDJ8AP-oRfx4X9NsA08qQZ58Ju9UwIvL5DdZ9iddrzYzGHpHkuzjk4fn22PZyB3AZHk1WQZrTiSGdaJHXFLTgCXeo1rf6a7bgakxOrrsID5cEIxknas1tA8t_qCTqaAYsobNb_QhagnINFSLGkVE19JwxDLus17o0-YFEZXasvLPRKs')
GO
INSERT [dbo].[StoreSettings] ([Id], [OpenTime], [CloseTime], [Currency], [PaymentConfig], [LogoUrl], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsActive], [IsDeleted], [StoreId], [IsSelfService], [discountStrategy], [AllowCombinePromotionAndCoupon], [SpendingRate], [PayOSApiKey], [PayOSChecksumKey], [PayOSClientId]) VALUES (N'5124e04d-4730-4181-aeba-58d8b5191cb4', CAST(N'00:00:00' AS Time), CAST(N'00:00:00' AS Time), N'VND', 0, N'', CAST(N'2025-08-26T22:20:35.3632251' AS DateTime2), N'502076ae-0fd0-4c1c-bd57-8c3a7dc480ac', CAST(N'2025-08-26T22:20:35.3633594' AS DateTime2), N'502076ae-0fd0-4c1c-bd57-8c3a7dc480ac', 1, 0, N'84859782-c27b-470e-883b-77e07df13a57', 0, 0, 1, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[StoreSettings] ([Id], [OpenTime], [CloseTime], [Currency], [PaymentConfig], [LogoUrl], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsActive], [IsDeleted], [StoreId], [IsSelfService], [discountStrategy], [AllowCombinePromotionAndCoupon], [SpendingRate], [PayOSApiKey], [PayOSChecksumKey], [PayOSClientId]) VALUES (N'a3e6ab8b-8e66-45f4-b8e7-5ebaf36291f7', CAST(N'00:00:00' AS Time), CAST(N'00:00:00' AS Time), N'VND', 0, N'', CAST(N'2025-08-11T15:04:16.3333510' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', CAST(N'2025-08-11T15:04:16.3333514' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', 1, 0, N'1bda923c-97c9-42a7-b9a9-73f0e602db27', 0, 0, 1, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[StoreSettings] ([Id], [OpenTime], [CloseTime], [Currency], [PaymentConfig], [LogoUrl], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsActive], [IsDeleted], [StoreId], [IsSelfService], [discountStrategy], [AllowCombinePromotionAndCoupon], [SpendingRate], [PayOSApiKey], [PayOSChecksumKey], [PayOSClientId]) VALUES (N'6b2dfe81-523d-404c-b7b6-6f4ec7f9dce4', CAST(N'00:00:00' AS Time), CAST(N'00:00:00' AS Time), N'VND', 0, N'', CAST(N'2025-08-12T07:59:09.9322712' AS DateTime2), N'6d109a17-2a7d-4c61-99ef-b110e02a0563', CAST(N'2025-08-12T07:59:09.9323895' AS DateTime2), N'6d109a17-2a7d-4c61-99ef-b110e02a0563', 1, 0, N'7ab65347-49d6-42dd-a8b7-3021bccc6ad3', 0, 0, 1, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[StoreSettings] ([Id], [OpenTime], [CloseTime], [Currency], [PaymentConfig], [LogoUrl], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsActive], [IsDeleted], [StoreId], [IsSelfService], [discountStrategy], [AllowCombinePromotionAndCoupon], [SpendingRate], [PayOSApiKey], [PayOSChecksumKey], [PayOSClientId]) VALUES (N'802dbf25-cd78-455e-9c10-e02dafbe358c', CAST(N'00:00:00' AS Time), CAST(N'00:00:00' AS Time), N'VND', 0, N'', CAST(N'2025-08-12T07:59:35.1924521' AS DateTime2), N'6d109a17-2a7d-4c61-99ef-b110e02a0563', CAST(N'2025-08-12T07:59:35.1924531' AS DateTime2), N'6d109a17-2a7d-4c61-99ef-b110e02a0563', 1, 0, N'c9f036ed-515e-4a6a-8b04-3294e34ddb2d', 0, 0, 1, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[StoreSettings] ([Id], [OpenTime], [CloseTime], [Currency], [PaymentConfig], [LogoUrl], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsActive], [IsDeleted], [StoreId], [IsSelfService], [discountStrategy], [AllowCombinePromotionAndCoupon], [SpendingRate], [PayOSApiKey], [PayOSChecksumKey], [PayOSClientId]) VALUES (N'7a516b8d-e43c-426f-ab8f-e893bd722221', CAST(N'00:00:00' AS Time), CAST(N'00:00:00' AS Time), N'VND', 0, N'', CAST(N'2025-08-11T15:00:54.7196174' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', CAST(N'2025-08-11T15:00:54.7197282' AS DateTime2), N'09f0de91-0237-4062-8bea-46ca64b7f7a7', 1, 0, N'05f17a65-bf51-4a8d-a08b-961f1e0c708a', 0, 0, 1, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[UserStores] ([Id], [UserId], [StoreId], [JoinDate], [Status], [BlockReason]) VALUES (N'cfac7b2d-ade8-4127-9264-03fe3fe36f93', N'f24b9caa-f1af-4823-876e-b5f5083cca4d', N'550e8400-e29b-41d4-a716-446655440000', CAST(N'2025-08-11T05:03:48.8670900' AS DateTime2), 0, NULL)
GO
INSERT [dbo].[UserStores] ([Id], [UserId], [StoreId], [JoinDate], [Status], [BlockReason]) VALUES (N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'550e8400-e29b-41d4-a716-446655440000', CAST(N'2025-08-11T04:17:59.0380044' AS DateTime2), 0, NULL)
GO
INSERT [dbo].[UserStores] ([Id], [UserId], [StoreId], [JoinDate], [Status], [BlockReason]) VALUES (N'2d498855-f774-4183-adeb-70746516d0cb', N'861ea4af-d875-4dc0-bbd6-184e701b3d1b', N'550e8400-e29b-41d4-a716-446655440000', CAST(N'2025-08-11T05:05:26.1303314' AS DateTime2), 0, NULL)
GO
INSERT [dbo].[UserStores] ([Id], [UserId], [StoreId], [JoinDate], [Status], [BlockReason]) VALUES (N'b57118f6-bfa1-4798-828b-9d314e89e044', N'452b6ccc-f348-4a6b-96ac-6d277b3c91be', N'550e8400-e29b-41d4-a716-446655440000', CAST(N'2025-08-11T08:58:01.4381361' AS DateTime2), 0, NULL)
GO
INSERT [dbo].[UserStores] ([Id], [UserId], [StoreId], [JoinDate], [Status], [BlockReason]) VALUES (N'7b7ceafd-27ca-41f4-bd29-bd3d598eb5bd', N'879e9619-5179-4f62-a353-43e1bedab1bd', N'550e8400-e29b-41d4-a716-446655440000', CAST(N'2025-08-12T08:30:04.0807586' AS DateTime2), 0, NULL)
GO
INSERT [dbo].[UserStores] ([Id], [UserId], [StoreId], [JoinDate], [Status], [BlockReason]) VALUES (N'c57fa491-8a30-4537-94ee-d8b492a4e38e', N'0b753faf-c12a-48c3-8ff6-33089c4b4a2c', N'550e8400-e29b-41d4-a716-446655440000', CAST(N'2025-08-12T08:32:37.9169418' AS DateTime2), 0, NULL)
GO
INSERT [dbo].[Roles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'123dc5a4-2ce6-4497-9c3a-f614752c28fa', N'User', N'USER', N'123dc5a4-2ce6-4497-9c3a-f614752c28fa')
GO
INSERT [dbo].[Roles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'35e0de77-c3a0-4a6e-ae26-d7fa0721ed62', N'Manager', N'MANAGER', N'35e0de77-c3a0-4a6e-ae26-d7fa0721ed62')
GO
INSERT [dbo].[Roles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'5961f48a-d700-4807-bc3d-4fa9d229e9fb', N'KitchenStaff', N'KITCHENSTAFF', N'5961f48a-d700-4807-bc3d-4fa9d229e9fb')
GO
INSERT [dbo].[Roles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'6a25d16f-9e41-431a-86e3-5e7e1620746b', N'Admin', N'ADMIN', N'6a25d16f-9e41-431a-86e3-5e7e1620746b')
GO
INSERT [dbo].[Roles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp]) VALUES (N'bc722046-068e-47ca-b280-c9088e3ebc1f', N'Staff', N'STAFF', N'bc722046-068e-47ca-b280-c9088e3ebc1f')
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsActive], [IsDeleted], [FOCSPoint]) VALUES (N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'Phúc', N'Nguyễn', N'phuc021203', N'PHUC021203', N'phuc021203@gmail.com', N'PHUC021203@GMAIL.COM', 1, N'AQAAAAIAAYagAAAAEAmueae6gaES8apZP3udztRkmoFNrY8G571+oDqDiK9hk0pVG8vp0w+7qJmcBJ8tiw==', N'ZG3VKOJBHYUKJFIXSKTAUMA4SXSMGSH2', N'46a01934-a917-4f86-9c2e-67db691a6bbc', N'0966393025', 0, 0, NULL, 1, 0, CAST(N'2025-08-10T16:47:43.4181497' AS DateTime2), N'System', CAST(N'2025-08-11T08:31:00.1221222' AS DateTime2), N'System', 0, 0, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsActive], [IsDeleted], [FOCSPoint]) VALUES (N'0b753faf-c12a-48c3-8ff6-33089c4b4a2c', N'Phúc 2', N'Nguyễn', N'phuc2', N'PHUC2', N'phuc2@staff.com', N'PHUC2@STAFF.COM', 0, N'AQAAAAIAAYagAAAAEI2/uMfyhygcVceERiclyx/Bob45khjqbPlb6nwhzJ3tDWExwbnFd+K4D2jJvMCU3w==', N'LZWJWCJVBKP5T5WIO4677GSMPBSUKVSS', N'42290786-3da1-4d0a-9927-c07b1fab0876', N'0912345678', 0, 0, NULL, 1, 0, CAST(N'2025-08-12T08:32:37.9116173' AS DateTime2), N'System', CAST(N'2025-08-12T08:33:31.8623315' AS DateTime2), N'System', 1, 0, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsActive], [IsDeleted], [FOCSPoint]) VALUES (N'452b6ccc-f348-4a6b-96ac-6d277b3c91be', N'Hồng Phúc', N'Nguyễn', N'ditexaj683', N'DITEXAJ683', N'ditexaj683@cronack.com', N'DITEXAJ683@CRONACK.COM', 1, N'AQAAAAIAAYagAAAAEJN9uTUIqbImH+BNbF0dJc+qqOfgwBhZlYiREhv1iI+IJHctTJrVBd9cicXnIUYTEQ==', N'ZY6RQM44XCG6DOCRHDKO6TJ22OCT46DB', N'9b1e5bfb-713b-4f4e-942e-99cd896a6212', N'0966393025', 0, 0, NULL, 1, 0, CAST(N'2025-08-11T08:58:01.4189630' AS DateTime2), N'System', CAST(N'2025-08-11T08:58:23.0312483' AS DateTime2), N'System', 0, 0, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsActive], [IsDeleted], [FOCSPoint]) VALUES (N'502076ae-0fd0-4c1c-bd57-8c3a7dc480ac', N'Hoàng', N'Sơn', N'hson512475', N'HSON512475', N'hson512475@gmail.com', N'HSON512475@GMAIL.COM', 1, N'AQAAAAIAAYagAAAAEHdUYKaPgKMkuSBrwW21LgL6dcqqwQQXYfcNaS2ib2MWao6Bik9JMym2m8qIcQCzgQ==', N'IXPTIYFNDH2ZTP2VRQPFNEOW4ESNPNX2', N'f6fc31da-f2fc-469f-a051-f669f497a98c', N'0974767405', 0, 0, NULL, 1, 0, CAST(N'2025-08-26T22:18:00.3495656' AS DateTime2), N'System', CAST(N'2025-08-26T22:19:14.8923089' AS DateTime2), N'System', 0, 0, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsActive], [IsDeleted], [FOCSPoint]) VALUES (N'5469a36d-c4fa-4a53-84c9-0da853d0cf55', N'string', N'string', N'Tomido2742', N'TOMIDO2742', N'Tomido2742@bizmud.com', N'TOMIDO2742@BIZMUD.COM', 0, N'AQAAAAIAAYagAAAAELZABiYxvGBB3ChbOlhkWE+3Fi1C6wEqlPsRAbIcOE82fL/45/y02Z2yf1JH5eda5A==', N'OJYDJIBJ6T3RNCELUPGO72AOY47ZXYDM', N'c77e2977-b5a2-4801-a9ea-c3262b52f853', N'30794399789', 0, 0, NULL, 1, 0, CAST(N'2025-08-10T16:54:43.7671040' AS DateTime2), N'System', CAST(N'2025-08-10T16:54:43.7809519' AS DateTime2), N'System', 0, 0, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsActive], [IsDeleted], [FOCSPoint]) VALUES (N'6d109a17-2a7d-4c61-99ef-b110e02a0563', N'Sơn', N'Hoàng Xuân', N'hson512475', N'HSON512475', N'hson512475@gmail.com', N'HSON512475@GMAIL.COM', 1, N'AQAAAAIAAYagAAAAEBqk5A4qv7ly09IPfxyGSW4nIzoEX6HHlUfvHmYFjXkkDgU65sQe9u/CTZHlOCYnRg==', N'NYEE2CX7MEAQEIX6PIHLPIZ4TLUZ6KOH', N'6b427c4c-e065-43e4-8b19-335089ca3850', N'0974767405', 0, 0, NULL, 1, 0, CAST(N'2025-08-12T07:56:49.4655070' AS DateTime2), N'System', CAST(N'2025-08-12T07:57:15.0590328' AS DateTime2), N'System', 0, 0, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsActive], [IsDeleted], [FOCSPoint]) VALUES (N'861ea4af-d875-4dc0-bbd6-184e701b3d1b', N'2', N'Nguyễn', N'2', N'2', N'2@gmail.com', N'2@GMAIL.COM', 0, N'AQAAAAIAAYagAAAAEBoLsj+XuO2RQcpcLNdzn+3ySbRvGDr/TZd8reZ9L7jvhYMSZu58pndPEtJsbgIzYg==', N'UZ5TC2WZICZ3GGHDC7ISCOIALMXFSVZW', N'75ed3af8-7b22-4519-aec1-16c1b366a92d', N'0966393025', 0, 0, NULL, 1, 0, CAST(N'2025-08-11T05:05:26.1231765' AS DateTime2), N'System', CAST(N'2025-08-12T07:57:43.5017555' AS DateTime2), N'System', 0, 1, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsActive], [IsDeleted], [FOCSPoint]) VALUES (N'879e9619-5179-4f62-a353-43e1bedab1bd', N'Phúc', N'Nguyễn', N'phuc', N'PHUC', N'phuc@staff.com', N'PHUC@STAFF.COM', 0, N'AQAAAAIAAYagAAAAEJ3eOdurw1w6DXUAc6bhBFrmzRc9kflvGxVNJPFxiHxO24apQJavIrP3JIB1YB5eVQ==', N'4H6SBNZU3UUXWMW77YTX23MV5HTPEE57', N'b47ef862-0f1e-4726-bf24-f43c2b586f3a', N'0912345678', 0, 0, NULL, 1, 0, CAST(N'2025-08-12T08:30:04.0638413' AS DateTime2), N'System', CAST(N'2025-08-12T08:31:15.3629859' AS DateTime2), N'System', 1, 0, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsActive], [IsDeleted], [FOCSPoint]) VALUES (N'a263440a-b083-4d44-ab36-53c0118318b2', N'Son', N'Hoang Xuan', N'admin', N'admin', N'admin@focs.site', N'admin', 1, N'AQAAAAEAACcQAAAAEFuQ5MY9SJW/Y/kM+vXeLBwypCD6D1wEnlUIZqLC1dGyMi0LyR3Qwrh1AJeZ+rmnJQ==
', N'KGHJBCEESEAC5JI6FWB3PDVRIPEUPRV1', N'7dba96fc-c27a-4bed-9476-fbe0a7e52542', N'0928374922', 1, 0, NULL, 1, 0, CAST(N'2025-11-08T00:00:00.0000000' AS DateTime2), N'System', CAST(N'2025-11-08T00:00:00.0000000' AS DateTime2), N'System', 1, 0, 10)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsActive], [IsDeleted], [FOCSPoint]) VALUES (N'a263440a-b083-4d44-ab36-53c0118318b6', N'Hoàng', N'Xuân Sơn', N'mh512475', N'MH512475', N'mh512475@gmail.com', N'MH512475@GMAIL.COM', 0, N'AQAAAAIAAYagAAAAEJpk1iU2PdMxNd4zeDo0WU22o1rlRyQg+nPW/XWVcMsdB3BWAFL+fuM/1aiEG4Ii1w==', N'5UXRWGDP76I6B3S4JR7W56VYZ5CY7VP4', N'd6b264ad-24f6-4afa-9b09-87a3282cdb23', N'0546213487', 0, 0, NULL, 1, 0, CAST(N'2025-08-10T16:46:36.1187156' AS DateTime2), N'System', CAST(N'2025-08-10T16:46:36.1313970' AS DateTime2), N'System', 0, 0, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsActive], [IsDeleted], [FOCSPoint]) VALUES (N'f24b9caa-f1af-4823-876e-b5f5083cca4d', N'Hồng Phúc', N'Nguyễn', N'phucstaff', N'PHUCSTAFF', N'phucstaff@gmail.com', N'PHUCSTAFF@GMAIL.COM', 0, N'AQAAAAIAAYagAAAAEKKEH3zEPLRwLamSjUme0w8HPGvhg5srPSCNwMO50TR2PrByD/POxVFTgrCLSrvHyg==', N'M5GTVVARVKRB32NN3C54AKZPOMGM2L4O', N'025b390f-83f5-4f20-87e0-46a6291f5d0c', N'0966393025', 0, 0, NULL, 1, 0, CAST(N'2025-08-11T05:03:48.7116847' AS DateTime2), N'System', CAST(N'2025-08-12T07:56:39.5920848' AS DateTime2), N'System', 0, 1, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [IsActive], [IsDeleted], [FOCSPoint]) VALUES (N'f7cbbe6f-aca7-45f4-8b7b-d7581f1e00d5', N'Chi', N'Phan', N'chiphhs171224', N'CHIPHHS171224', N'chiphhs171224@fpt.edu.vn', N'CHIPHHS171224@FPT.EDU.VN', 1, N'AQAAAAIAAYagAAAAEA7XCw8HbwiioQljOrJvkDlDmUwedj1+pRST29j+dqRbKBMYrOL067WnvJ+9KRq9mQ==', N'TBEMM5ZAZPSFBZW7EZG77MCXN7CSGEG7', N'0686cce4-6d9a-4015-8ac3-2ccc026f42a6', N'0121345689', 0, 0, NULL, 1, 0, CAST(N'2025-08-11T15:14:28.1980249' AS DateTime2), N'System', CAST(N'2025-08-11T15:14:55.6208511' AS DateTime2), N'System', 0, 0, NULL)
GO
INSERT [dbo].[UserRoles] ([UserId], [RoleId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'6a25d16f-9e41-431a-86e3-5e7e1620746b', CAST(N'2025-08-10T16:47:43.4323121' AS DateTime2), N'System', CAST(N'2025-08-10T16:47:43.4323121' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRoles] ([UserId], [RoleId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'0b753faf-c12a-48c3-8ff6-33089c4b4a2c', N'bc722046-068e-47ca-b280-c9088e3ebc1f', CAST(N'2025-08-12T08:32:41.8074083' AS DateTime2), N'System', CAST(N'2025-08-12T08:32:41.8074083' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRoles] ([UserId], [RoleId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'452b6ccc-f348-4a6b-96ac-6d277b3c91be', N'123dc5a4-2ce6-4497-9c3a-f614752c28fa', CAST(N'2025-08-11T08:58:01.4308158' AS DateTime2), N'System', CAST(N'2025-08-11T08:58:01.4308158' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRoles] ([UserId], [RoleId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'502076ae-0fd0-4c1c-bd57-8c3a7dc480ac', N'6a25d16f-9e41-431a-86e3-5e7e1620746b', CAST(N'2025-08-26T22:18:00.6095537' AS DateTime2), N'System', CAST(N'2025-08-26T22:18:00.6095537' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRoles] ([UserId], [RoleId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'5469a36d-c4fa-4a53-84c9-0da853d0cf55', N'6a25d16f-9e41-431a-86e3-5e7e1620746b', CAST(N'2025-08-10T16:54:43.7808987' AS DateTime2), N'System', CAST(N'2025-08-10T16:54:43.7808987' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRoles] ([UserId], [RoleId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'6d109a17-2a7d-4c61-99ef-b110e02a0563', N'6a25d16f-9e41-431a-86e3-5e7e1620746b', CAST(N'2025-08-12T07:56:49.4786338' AS DateTime2), N'System', CAST(N'2025-08-12T07:56:49.4786338' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRoles] ([UserId], [RoleId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'861ea4af-d875-4dc0-bbd6-184e701b3d1b', N'bc722046-068e-47ca-b280-c9088e3ebc1f', CAST(N'2025-08-11T05:05:35.5100704' AS DateTime2), N'System', CAST(N'2025-08-11T05:05:35.5100704' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRoles] ([UserId], [RoleId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'879e9619-5179-4f62-a353-43e1bedab1bd', N'bc722046-068e-47ca-b280-c9088e3ebc1f', CAST(N'2025-08-12T08:30:18.3969400' AS DateTime2), N'System', CAST(N'2025-08-12T08:30:18.3969400' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRoles] ([UserId], [RoleId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'a263440a-b083-4d44-ab36-53c0118318b2', N'6a25d16f-9e41-431a-86e3-5e7e1620746b', CAST(N'0001-01-01T00:00:00.0000000' AS DateTime2), NULL, CAST(N'0001-01-01T00:00:00.0000000' AS DateTime2), NULL)
GO
INSERT [dbo].[UserRoles] ([UserId], [RoleId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'a263440a-b083-4d44-ab36-53c0118318b6', N'6a25d16f-9e41-431a-86e3-5e7e1620746b', CAST(N'2025-08-10T16:46:36.1313687' AS DateTime2), N'System', CAST(N'2025-08-10T16:46:36.1313687' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRoles] ([UserId], [RoleId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'f24b9caa-f1af-4823-876e-b5f5083cca4d', N'bc722046-068e-47ca-b280-c9088e3ebc1f', CAST(N'2025-08-11T05:04:01.4267700' AS DateTime2), N'System', CAST(N'2025-08-11T05:04:01.4267700' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRoles] ([UserId], [RoleId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'f7cbbe6f-aca7-45f4-8b7b-d7581f1e00d5', N'6a25d16f-9e41-431a-86e3-5e7e1620746b', CAST(N'2025-08-11T15:14:28.3209416' AS DateTime2), N'System', CAST(N'2025-08-11T15:14:28.3209416' AS DateTime2), N'System')
GO
INSERT [dbo].[MenuItemCategories] ([Id], [MenuItemId], [CategoryId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'f54cee95-9469-43eb-bc85-12366ae89427', N'1fdde566-2f30-4f4a-814f-c8cd6cfb40d1', N'e70c8fdf-0a20-4b98-be49-93dc693debe3', CAST(N'2025-08-12T08:14:25.4914489' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemCategories] ([Id], [MenuItemId], [CategoryId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'4f8f872b-5910-412c-aea5-1f2a8cf66cf1', N'324cba2b-6943-4e84-acef-b3cb01ac21b3', N'8649777c-05cb-4a30-acca-c15d76dc257b', CAST(N'2025-08-12T10:21:42.5620902' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemCategories] ([Id], [MenuItemId], [CategoryId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'76222ad5-c330-4af3-be8a-1f3ebda52171', N'059c4212-3f4d-4ec9-96e2-848d7224ca74', N'e70c8fdf-0a20-4b98-be49-93dc693debe3', CAST(N'2025-08-12T10:08:36.3350110' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemCategories] ([Id], [MenuItemId], [CategoryId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'3e444903-618e-482c-993c-2c8b513afe8f', N'80185741-1f1c-40af-9eeb-80718c7b9d2c', N'e70c8fdf-0a20-4b98-be49-93dc693debe3', CAST(N'2025-08-12T07:59:05.6718985' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemCategories] ([Id], [MenuItemId], [CategoryId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'23af1624-ddf2-40c0-83b6-2e1813e802e5', N'059c4212-3f4d-4ec9-96e2-848d7224ca74', N'7b40e47a-26fd-4533-a080-254cf1f9268f', CAST(N'2025-08-12T10:08:36.3352828' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemCategories] ([Id], [MenuItemId], [CategoryId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'04025bc9-e8ee-4173-a14b-4456971dced6', N'bc067dd1-a55d-464d-9df1-0ea74b0e503c', N'e70c8fdf-0a20-4b98-be49-93dc693debe3', CAST(N'2025-08-12T10:13:55.0181029' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemCategories] ([Id], [MenuItemId], [CategoryId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'bcf23cdc-7cb7-4106-9e4b-487cc8880f21', N'dc098afc-eb8b-4757-82e1-3db5d2d413db', N'e70c8fdf-0a20-4b98-be49-93dc693debe3', CAST(N'2025-08-11T08:03:43.0110994' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemCategories] ([Id], [MenuItemId], [CategoryId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'e736a965-f352-44d1-87dc-6629daafe7b9', N'7e2994b3-3aee-4dbf-8744-41c95fa8f73d', N'056f3587-1a79-4319-82c9-a98f83d3dbb0', CAST(N'2025-08-12T10:10:40.1927556' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemCategories] ([Id], [MenuItemId], [CategoryId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'14835f2b-2928-4994-84d2-7436b91f6a57', N'7e2994b3-3aee-4dbf-8744-41c95fa8f73d', N'f4ee3653-e66a-4832-8414-1b25c67e6950', CAST(N'2025-08-12T10:10:40.1922043' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemCategories] ([Id], [MenuItemId], [CategoryId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'66c4ad38-0c55-4240-a605-76dee089e24a', N'f6e20025-2858-47eb-96ef-9d49d0d49b1c', N'e70c8fdf-0a20-4b98-be49-93dc693debe3', CAST(N'2025-08-11T04:17:59.1446146' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemCategories] ([Id], [MenuItemId], [CategoryId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'7bc05629-3e2d-4594-b478-775f7e2a5941', N'80185741-1f1c-40af-9eeb-80718c7b9d2c', N'c4f38982-6e41-4b61-9169-f37c02f70262', CAST(N'2025-08-12T07:59:05.6773825' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemCategories] ([Id], [MenuItemId], [CategoryId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'c275b8a5-0f93-40b7-9ff2-7c1409956b69', N'8927310a-664e-46e6-965f-81e0209a96a0', N'f4ee3653-e66a-4832-8414-1b25c67e6950', CAST(N'2025-08-12T10:11:59.0170635' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemCategories] ([Id], [MenuItemId], [CategoryId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'f6f5afcf-e66b-4e7d-8330-8b301f90bfa2', N'8927310a-664e-46e6-965f-81e0209a96a0', N'056f3587-1a79-4319-82c9-a98f83d3dbb0', CAST(N'2025-08-12T10:11:59.0178625' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemCategories] ([Id], [MenuItemId], [CategoryId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'1d9b5148-62ad-4338-8530-8e1cba2fbb2a', N'1fdde566-2f30-4f4a-814f-c8cd6cfb40d1', N'c4f38982-6e41-4b61-9169-f37c02f70262', CAST(N'2025-08-12T08:14:25.4918341' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemCategories] ([Id], [MenuItemId], [CategoryId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'b7432634-fde5-4702-97a5-8ebed1750294', N'bc067dd1-a55d-464d-9df1-0ea74b0e503c', N'7b40e47a-26fd-4533-a080-254cf1f9268f', CAST(N'2025-08-12T10:13:55.0178411' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemCategories] ([Id], [MenuItemId], [CategoryId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'f8143017-0818-410e-8aba-916221d37c86', N'8927310a-664e-46e6-965f-81e0209a96a0', N'e70c8fdf-0a20-4b98-be49-93dc693debe3', CAST(N'2025-08-12T10:11:59.0173403' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemCategories] ([Id], [MenuItemId], [CategoryId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'c9de0df5-3e84-4133-b357-a0c53ddaad42', N'059c4212-3f4d-4ec9-96e2-848d7224ca74', N'056f3587-1a79-4319-82c9-a98f83d3dbb0', CAST(N'2025-08-12T10:08:36.3352397' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemCategories] ([Id], [MenuItemId], [CategoryId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'0813e213-8abf-4dbb-a686-c0ab86375b33', N'7e2994b3-3aee-4dbf-8744-41c95fa8f73d', N'e70c8fdf-0a20-4b98-be49-93dc693debe3', CAST(N'2025-08-12T10:10:40.1927202' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemCategories] ([Id], [MenuItemId], [CategoryId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'c49be1e9-4555-4ec7-b49e-d07b1aa89f0b', N'bb3726e9-fddb-406c-b137-c7942b3b92d1', N'8649777c-05cb-4a30-acca-c15d76dc257b', CAST(N'2025-08-12T10:23:10.5787035' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemCategories] ([Id], [MenuItemId], [CategoryId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'af75f293-5935-4be0-94a6-e7753925ca8f', N'222b05d1-da3e-464e-909f-ac2190e1d946', N'0a0d7f0c-f050-46b2-8c1b-91e8dd233d03', CAST(N'2025-08-12T08:13:39.6461952' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[UserRefreshTokens] ON 
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (1, N'502076ae-0fd0-4c1c-bd57-8c3a7dc480ac', N'+2MKURUZPccTjlOf5F91co+gu6Zl0hYx7OX7VYBSPpw=', CAST(N'2025-09-02T22:19:24.5014958' AS DateTime2), 0, CAST(N'2025-08-26T22:19:24.5732738' AS DateTime2), N'System', CAST(N'2025-08-26T22:19:24.5732738' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (2, N'502076ae-0fd0-4c1c-bd57-8c3a7dc480ac', N'CF2kBpaX0RCylH8ttFVBxM2PphJKdkNc7T16sgH0lKM=', CAST(N'2025-09-02T22:19:24.7296434' AS DateTime2), 0, CAST(N'2025-08-26T22:19:24.7309947' AS DateTime2), N'System', CAST(N'2025-08-26T22:19:24.7309947' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (3, N'502076ae-0fd0-4c1c-bd57-8c3a7dc480ac', N'rwewkwBUASdnxo+MAp6H50Dij7iZRcNywupTinNFndw=', CAST(N'2025-09-02T22:22:49.3406303' AS DateTime2), 0, CAST(N'2025-08-26T22:22:49.3420052' AS DateTime2), N'System', CAST(N'2025-08-26T22:22:49.3420052' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (4, N'502076ae-0fd0-4c1c-bd57-8c3a7dc480ac', N'uZCCnhhb1bdPWfppKCoSuEpO9hBN3ACLFfR9+Zec/Cg=', CAST(N'2025-09-02T22:22:49.4762423' AS DateTime2), 0, CAST(N'2025-08-26T22:22:49.4766625' AS DateTime2), N'System', CAST(N'2025-08-26T22:22:49.4766625' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (5, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'frzBelq9F6N8cwTv/NjFmB2D3H4sHymDvyy2+IeDTNQ=', CAST(N'2025-08-18T08:30:23.4839110' AS DateTime2), 0, CAST(N'2025-08-11T08:30:23.4853566' AS DateTime2), N'System', CAST(N'2025-08-11T08:30:23.4853566' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (6, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'fNfvPPnJIkCOw8XyuCNKBEPkELfO68G5P6HMTq4lWfc=', CAST(N'2025-08-18T08:53:15.8066335' AS DateTime2), 0, CAST(N'2025-08-11T08:53:15.8083847' AS DateTime2), N'System', CAST(N'2025-08-11T08:53:15.8083847' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (7, N'452b6ccc-f348-4a6b-96ac-6d277b3c91be', N'UxK4vbCA6tjhpqOjulU9QktUxAsu/GFOzgjmGkhW0iQ=', CAST(N'2025-08-18T08:58:40.5049002' AS DateTime2), 0, CAST(N'2025-08-11T08:58:40.5060372' AS DateTime2), N'System', CAST(N'2025-08-11T08:58:40.5060372' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (8, N'452b6ccc-f348-4a6b-96ac-6d277b3c91be', N'VaDXHQ3zAailF5tpOa0VOazFBopYFl6ek3WXAOykIEQ=', CAST(N'2025-08-18T08:59:24.5054043' AS DateTime2), 0, CAST(N'2025-08-11T08:59:24.5059258' AS DateTime2), N'System', CAST(N'2025-08-11T08:59:24.5059258' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (9, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'FBNqHevlFo6S/iINTOCy5Xr3DBh3wB2YNgCe605P+OU=', CAST(N'2025-08-18T10:00:01.3297006' AS DateTime2), 0, CAST(N'2025-08-11T10:00:01.3308753' AS DateTime2), N'System', CAST(N'2025-08-11T10:00:01.3308753' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (10, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'dFMmwrZ1+lMjSipkE+GnqWW5do0yB89OsGVJDV5cjVQ=', CAST(N'2025-08-18T14:54:38.3160077' AS DateTime2), 0, CAST(N'2025-08-11T14:54:38.4496130' AS DateTime2), N'System', CAST(N'2025-08-11T14:54:38.4496130' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (11, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'EX7+IClgOSeQCqKOqoqryCZZ0aSHkxVht05Fhmp+5GI=', CAST(N'2025-08-18T15:00:05.3171197' AS DateTime2), 0, CAST(N'2025-08-11T15:00:05.4675188' AS DateTime2), N'System', CAST(N'2025-08-11T15:00:05.4675188' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (12, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'QY3Ho15X5lCexF1GPiyrQTxUm37er/fYrRF4pRFsDb4=', CAST(N'2025-08-18T15:20:29.9257720' AS DateTime2), 0, CAST(N'2025-08-11T15:20:30.0476340' AS DateTime2), N'System', CAST(N'2025-08-11T15:20:30.0476340' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (13, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'KuNctEoMG9CNx/BsAQTyxBN/8N3Rv9DAEa1P+PEZat0=', CAST(N'2025-08-18T15:27:58.7367010' AS DateTime2), 0, CAST(N'2025-08-11T15:27:58.8674176' AS DateTime2), N'System', CAST(N'2025-08-11T15:27:58.8674176' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (14, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'gkNvcf0KLBV2mI8OW/DOh4dZq6kuLlgRi4mNqP7QlhU=', CAST(N'2025-08-18T15:32:10.4823518' AS DateTime2), 0, CAST(N'2025-08-11T15:32:10.4852688' AS DateTime2), N'System', CAST(N'2025-08-11T15:32:10.4852688' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (15, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'3RpzJ6joouhss4UHIOwjU7jW6PzdMVun2Xc7vw1UJEM=', CAST(N'2025-08-18T17:40:51.9366830' AS DateTime2), 0, CAST(N'2025-08-11T17:40:51.9380658' AS DateTime2), N'System', CAST(N'2025-08-11T17:40:51.9380658' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (16, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'E2CNCoztRFF0syHRyBkBK5uAu+kvB1nKgzzUDfU/Rzw=', CAST(N'2025-08-19T03:28:22.6950164' AS DateTime2), 0, CAST(N'2025-08-12T03:28:22.6966127' AS DateTime2), N'System', CAST(N'2025-08-12T03:28:22.6966127' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (17, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'tXi5MELWHAk8xPbeRVGoieTiCwGPl8lyElUEFuxrpoI=', CAST(N'2025-08-19T03:29:13.6164421' AS DateTime2), 0, CAST(N'2025-08-12T03:29:13.6172368' AS DateTime2), N'System', CAST(N'2025-08-12T03:29:13.6172368' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (18, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'4LwUVXvx/+I2CxJqYRrSrsQCsg8SzubSTmNgJcAW2LI=', CAST(N'2025-08-19T03:54:05.9036356' AS DateTime2), 0, CAST(N'2025-08-12T03:54:05.9046594' AS DateTime2), N'System', CAST(N'2025-08-12T03:54:05.9046594' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (19, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'Qvz7/P4+w/DQt6PTE9Ly7PiG/PjUY2qbG+aTH7Ee7Ok=', CAST(N'2025-08-19T04:07:32.0644653' AS DateTime2), 0, CAST(N'2025-08-12T04:07:32.0655160' AS DateTime2), N'System', CAST(N'2025-08-12T04:07:32.0655160' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (20, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'37OrXSGxp1E2L4V933VtnfFM2p/gzAa74OaB1vKG2vI=', CAST(N'2025-08-19T04:53:02.3036546' AS DateTime2), 0, CAST(N'2025-08-12T04:53:02.3049419' AS DateTime2), N'System', CAST(N'2025-08-12T04:53:02.3049419' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (21, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'+O02pVX+H8GgyftvqTlAVgvtLRuDWxxN11cEfaf0iuc=', CAST(N'2025-08-19T06:26:25.1423096' AS DateTime2), 0, CAST(N'2025-08-12T06:26:25.1437508' AS DateTime2), N'System', CAST(N'2025-08-12T06:26:25.1437508' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (22, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'kiWG3LqLI1Q5ve2PVOyKFwpsHUnybAFDsuRXmaH/OZs=', CAST(N'2025-08-19T07:16:24.1191760' AS DateTime2), 0, CAST(N'2025-08-12T07:16:24.2418959' AS DateTime2), N'System', CAST(N'2025-08-12T07:16:24.2418959' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (23, N'6d109a17-2a7d-4c61-99ef-b110e02a0563', N'lbLOw/5OiFoY2v54xHJGUlxRw6sYVN+Feh6bcAwW6HU=', CAST(N'2025-08-19T07:57:28.8959454' AS DateTime2), 0, CAST(N'2025-08-12T07:57:28.8972112' AS DateTime2), N'System', CAST(N'2025-08-12T07:57:28.8972112' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (24, N'6d109a17-2a7d-4c61-99ef-b110e02a0563', N'FAzgczVbJK73mrVSb07uoS2XdHZQbZ9jxoPsIw4z4eQ=', CAST(N'2025-08-19T07:59:50.0965730' AS DateTime2), 0, CAST(N'2025-08-12T07:59:50.0971423' AS DateTime2), N'System', CAST(N'2025-08-12T07:59:50.0971423' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (25, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'i08F7MC+xuTZXBZbWgRBuT2pspMTFPKgRMs6JpCHxTs=', CAST(N'2025-08-19T08:16:03.7913786' AS DateTime2), 0, CAST(N'2025-08-12T08:16:03.8725772' AS DateTime2), N'System', CAST(N'2025-08-12T08:16:03.8725772' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (26, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'V3L1/ykoQzyx8UdUMjxNwJLTB/MREJ0gB8cFAIn0ZUo=', CAST(N'2025-08-19T08:16:04.2394103' AS DateTime2), 0, CAST(N'2025-08-12T08:16:04.2401586' AS DateTime2), N'System', CAST(N'2025-08-12T08:16:04.2401586' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (27, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'CLy0XbCgqyemxj53/yPCs14OmxRgS2dwZHEb8IEhJdQ=', CAST(N'2025-08-19T08:37:38.7581595' AS DateTime2), 0, CAST(N'2025-08-12T08:37:38.7593712' AS DateTime2), N'System', CAST(N'2025-08-12T08:37:38.7593712' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (28, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'XEnRpOHbdiBU1tPAuZrNhWsA/eDy7X37XBlSHCH3eoQ=', CAST(N'2025-08-19T08:37:38.9097399' AS DateTime2), 0, CAST(N'2025-08-12T08:37:38.9101028' AS DateTime2), N'System', CAST(N'2025-08-12T08:37:38.9101028' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (29, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'VTL+3RQCyzmtwprjRHAsr8OXK8a1V0K5socW7dHCaY8=', CAST(N'2025-08-19T08:46:11.5515722' AS DateTime2), 0, CAST(N'2025-08-12T08:46:11.5526837' AS DateTime2), N'System', CAST(N'2025-08-12T08:46:11.5526837' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (30, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'T5zPgqzNEJ2giYFN7xbLKW4NRa7Gw1/DC+h0cthxO4E=', CAST(N'2025-08-19T08:46:11.7214555' AS DateTime2), 0, CAST(N'2025-08-12T08:46:11.7218011' AS DateTime2), N'System', CAST(N'2025-08-12T08:46:11.7218011' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (31, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'DtvFljKxrKmvO0gr7oCYb1Z/VONBooJZK2/YGpd3irE=', CAST(N'2025-08-19T09:39:08.8693619' AS DateTime2), 0, CAST(N'2025-08-12T09:39:08.8707496' AS DateTime2), N'System', CAST(N'2025-08-12T09:39:08.8707496' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (32, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'1dOAIVy5pJUcHIJuVz/etOVrxmYJCDJ8+GT12mvqH6I=', CAST(N'2025-08-19T09:39:09.0251862' AS DateTime2), 0, CAST(N'2025-08-12T09:39:09.0256125' AS DateTime2), N'System', CAST(N'2025-08-12T09:39:09.0256125' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (33, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'ntGr2RkR6HgkybcCVQO7Bs14GMDzcUSfLuEBoME/1ME=', CAST(N'2025-08-19T09:39:27.3298103' AS DateTime2), 0, CAST(N'2025-08-12T09:39:27.3302302' AS DateTime2), N'System', CAST(N'2025-08-12T09:39:27.3302302' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (34, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'WS7zZ0w7xKne2WhubL3qGcP7+LBk2pYD6sjKJwZwL+Y=', CAST(N'2025-08-19T09:39:27.4780613' AS DateTime2), 0, CAST(N'2025-08-12T09:39:27.4788372' AS DateTime2), N'System', CAST(N'2025-08-12T09:39:27.4788372' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (35, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'gug4pevHUXT+2HGJC6431oC4SIsLFf5g3VOCmmWrljo=', CAST(N'2025-08-19T12:34:03.4926562' AS DateTime2), 0, CAST(N'2025-08-12T12:34:03.4937578' AS DateTime2), N'System', CAST(N'2025-08-12T12:34:03.4937578' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (36, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'36nmaO2HtnCkY2wwmnCjuxsFKlH0UCmNIOTYkIr0mGk=', CAST(N'2025-08-19T12:34:03.6688625' AS DateTime2), 0, CAST(N'2025-08-12T12:34:03.6691886' AS DateTime2), N'System', CAST(N'2025-08-12T12:34:03.6691886' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (37, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'c4K1y6/lPRzlOGOwf0V6/vd4zSnzxrYCsADorTT3Nto=', CAST(N'2025-08-19T12:55:48.1892284' AS DateTime2), 0, CAST(N'2025-08-12T12:55:48.1902029' AS DateTime2), N'System', CAST(N'2025-08-12T12:55:48.1902029' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (38, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'ykuAVnY+GZQ47MLI1UVvaYiCUHsQUDAmxBQS19kjaaI=', CAST(N'2025-08-19T12:55:48.3412736' AS DateTime2), 0, CAST(N'2025-08-12T12:55:48.3415776' AS DateTime2), N'System', CAST(N'2025-08-12T12:55:48.3415776' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (39, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'gwO/AqODOeoJ6lMKzNsotmU1rztM6SYAdX7cqgfLN4c=', CAST(N'2025-08-19T13:11:46.3840535' AS DateTime2), 0, CAST(N'2025-08-12T13:11:46.3851327' AS DateTime2), N'System', CAST(N'2025-08-12T13:11:46.3851327' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (40, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'jUhiDLBKjsL2dwW8pHbfxdXQscuRmIEU5QS6onFrXHk=', CAST(N'2025-08-19T13:11:46.5351270' AS DateTime2), 0, CAST(N'2025-08-12T13:11:46.5353701' AS DateTime2), N'System', CAST(N'2025-08-12T13:11:46.5353701' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (41, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'3b72t6diymnbfLsk6gVWp+axtWBI6qt5pMi0vPlaoTM=', CAST(N'2025-09-03T02:01:11.5242831' AS DateTime2), 0, CAST(N'2025-08-27T02:01:11.5251849' AS DateTime2), N'System', CAST(N'2025-08-27T02:01:11.5251849' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (42, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'/nyN/h3lizCJ1dDH8xi39bSrDjKrdxBGua4mGjk6gYY=', CAST(N'2025-09-03T02:01:11.6562818' AS DateTime2), 0, CAST(N'2025-08-27T02:01:11.6566976' AS DateTime2), N'System', CAST(N'2025-08-27T02:01:11.6566976' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (43, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'+DTCyuzv8Ejl2lxfnUKIh9zZgm9YnmryLjJeOxlJMsY=', CAST(N'2025-09-03T02:37:27.2571308' AS DateTime2), 0, CAST(N'2025-08-27T02:37:27.2581161' AS DateTime2), N'System', CAST(N'2025-08-27T02:37:27.2581161' AS DateTime2), N'System')
GO
INSERT [dbo].[UserRefreshTokens] ([Id], [UserId], [Token], [ExpirationDate], [IsRevoked], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (44, N'09f0de91-0237-4062-8bea-46ca64b7f7a7', N'Z9eAiNCAa81lbb3f7EZ+l6tYiM3WESrd3iPneyacHFk=', CAST(N'2025-09-03T02:37:27.3910391' AS DateTime2), 0, CAST(N'2025-08-27T02:37:27.3913262' AS DateTime2), N'System', CAST(N'2025-08-27T02:37:27.3913262' AS DateTime2), N'System')
GO
SET IDENTITY_INSERT [dbo].[UserRefreshTokens] OFF
GO
INSERT [dbo].[Feedbacks] ([Id], [Rating], [Comment], [UserId], [OrderId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy], [Images], [IsPublic], [StoreId]) VALUES (N'5469a36d-c4fa-4a53-84c9-0da853d0cf55', 5, N'a', N'5469a36d-c4fa-4a53-84c9-0da853d0cf55', N'550e8400-e29b-41d4-a716-446655440022', CAST(N'2025-11-11T00:00:00.0000000' AS DateTime2), N'5469a36d-c4fa-4a53-84c9-0da853d0cf55', CAST(N'2025-08-11T08:52:58.2499064' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', N'[]', 1, N'550e8400-e29b-41d4-a716-446655440000')
GO
INSERT [dbo].[MenuItemImages] ([Id], [Url], [IsMain], [MenuItemId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'6a650287-a461-4509-ba0d-04ab5bcb6c76', N'https://res.cloudinary.com/dpi0x4iy6/image/upload/v1754993662/stores/550e8400-e29b-41d4-a716-446655440000/menu-items/bc067dd1-a55d-464d-9df1-0ea74b0e503c/684b40ff-6b10-40ba-aa34-c636c33f1cc1.jpg', 0, N'bc067dd1-a55d-464d-9df1-0ea74b0e503c', CAST(N'2025-08-12T10:14:23.6507469' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemImages] ([Id], [Url], [IsMain], [MenuItemId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'c0dc78b0-2ead-4426-89d1-1f08a8e53ef0', N'https://res.cloudinary.com/dpi0x4iy6/image/upload/v1754885881/stores/550E8400-E29B-41D4-A716-446655440000/menu-items/f6e20025-2858-47eb-96ef-9d49d0d49b1c/7585375b-4f36-4656-8b8c-9e9c17d4da0d.webp', 0, N'f6e20025-2858-47eb-96ef-9d49d0d49b1c', CAST(N'2025-08-11T04:18:02.2717737' AS DateTime2), N'550E8400-E29B-41D4-A716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemImages] ([Id], [Url], [IsMain], [MenuItemId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'56a241f7-1fbf-4007-961d-7f62e0c009f6', N'https://res.cloudinary.com/dpi0x4iy6/image/upload/v1754993338/stores/550e8400-e29b-41d4-a716-446655440000/menu-items/059c4212-3f4d-4ec9-96e2-848d7224ca74/88e9d769-ac38-4fce-92de-4f1d32ed4ab6.jpg', 0, N'059c4212-3f4d-4ec9-96e2-848d7224ca74', CAST(N'2025-08-12T10:08:59.3521527' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemImages] ([Id], [Url], [IsMain], [MenuItemId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'0ca1c79f-bf10-4967-a798-93493a850098', N'https://res.cloudinary.com/dpi0x4iy6/image/upload/v1754993447/stores/550e8400-e29b-41d4-a716-446655440000/menu-items/7e2994b3-3aee-4dbf-8744-41c95fa8f73d/9f8a1da6-87ba-46b4-8bab-4b5ab8536d2a.jpg', 0, N'7e2994b3-3aee-4dbf-8744-41c95fa8f73d', CAST(N'2025-08-12T10:10:47.9518742' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemImages] ([Id], [Url], [IsMain], [MenuItemId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'fa2bc76f-d189-477e-a720-a6f2a3f50837', N'https://res.cloudinary.com/dpi0x4iy6/image/upload/v1754986421/stores/7ab65347-49d6-42dd-a8b7-3021bccc6ad3/menu-items/222b05d1-da3e-464e-909f-ac2190e1d946/cd2b3e2f-eb47-41bf-9183-3e9cdab5fd5b.jpg', 0, N'222b05d1-da3e-464e-909f-ac2190e1d946', CAST(N'2025-08-12T08:13:42.8213747' AS DateTime2), N'7ab65347-49d6-42dd-a8b7-3021bccc6ad3', NULL, NULL)
GO
INSERT [dbo].[MenuItemImages] ([Id], [Url], [IsMain], [MenuItemId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'24182d1b-5e03-44ee-8506-ba1a91f3e386', N'https://res.cloudinary.com/dpi0x4iy6/image/upload/v1754993530/stores/550e8400-e29b-41d4-a716-446655440000/menu-items/8927310a-664e-46e6-965f-81e0209a96a0/70a7b5e4-aaa6-4dbb-8784-d4fa1b11f47a.jpg', 0, N'8927310a-664e-46e6-965f-81e0209a96a0', CAST(N'2025-08-12T10:12:11.3375911' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemImages] ([Id], [Url], [IsMain], [MenuItemId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'1a765b8a-beec-4b0f-8e75-d668cf5d4369', N'https://res.cloudinary.com/dpi0x4iy6/image/upload/v1754994105/stores/550e8400-e29b-41d4-a716-446655440000/menu-items/324cba2b-6943-4e84-acef-b3cb01ac21b3/9640cb24-2048-42e3-83ef-b2f7c473b30b.jpg', 0, N'324cba2b-6943-4e84-acef-b3cb01ac21b3', CAST(N'2025-08-12T10:21:46.3598584' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[MenuItemImages] ([Id], [Url], [IsMain], [MenuItemId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'69a11271-76da-4bca-84ef-fbccd76e06b0', N'https://res.cloudinary.com/dpi0x4iy6/image/upload/v1754986423/stores/7ab65347-49d6-42dd-a8b7-3021bccc6ad3/menu-items/222b05d1-da3e-464e-909f-ac2190e1d946/f26b29ba-bef6-4572-8e5d-2e816911af73.webp', 0, N'222b05d1-da3e-464e-909f-ac2190e1d946', CAST(N'2025-08-12T08:13:44.1007374' AS DateTime2), N'7ab65347-49d6-42dd-a8b7-3021bccc6ad3', NULL, NULL)
GO
INSERT [dbo].[MenuItemImages] ([Id], [Url], [IsMain], [MenuItemId], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'011dfc33-4f9e-4b6e-a89e-ff48a1890cf1', N'https://res.cloudinary.com/dpi0x4iy6/image/upload/v1754985551/stores/550e8400-e29b-41d4-a716-446655440000/menu-items/80185741-1f1c-40af-9eeb-80718c7b9d2c/9883b78e-7eea-4477-8bf6-c79746e59d6c.jpg', 0, N'80185741-1f1c-40af-9eeb-80718c7b9d2c', CAST(N'2025-08-12T07:59:12.7823681' AS DateTime2), N'550e8400-e29b-41d4-a716-446655440000', NULL, NULL)
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250514080317_InitialDB', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250514094619_AddBaseAttributes', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250515074941_update_refreshdb', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250515082439_InitRole', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250525154849_refresh', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250527153658_init_orderdb', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250528125057_second_update_order', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250528150128_update_store', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250603023624_update_cart', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250603142909_seed_new_role', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250607083351_update_promotion', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250607083526_update_promotion_2', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250607094110_update_store_setting', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250607145053_update_role', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250608082605_add_store_to_promotion', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250609004516_update_coupon_model', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250609024350_update_coupon_field', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250609065232_update_item_link_promotion', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250612043911_update_user_store_table', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250613070721_update_user', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250613115228_update_store_user', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250614043233_update_user_unique_key', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250614064808_update_store_table', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250615160158_update_user_order', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250616083648_update_order', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250616085949_update_order_1', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250619074019_update_promotion_apply', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250619085842_update_promotion_apply_1', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250621142227_update_promotion_attribute', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250621162557_update_promotion_attribute_1', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250623023024_update_menu_cate', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250623032615_update_variant_group', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250623040506_update_variant_v2', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250623042000_update_variant_v', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250624012700_update_variant_v1', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250625151823_update_variant_v4', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250629073105_update_image_product', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250703173230_update_variant_group_item', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250704073309_setup_loyal_points', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250704073457_setup_focs_points', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250708145552_AddQRVersionToTable', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250708160401_add_menuitem_active', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250712132039_AddCouponType', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250717080520_update_order_entity', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250719065438_update_loyaity', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250723042304_update_mobile_token_v1', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250724064757_update_payment', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250729075529_update_feedback', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250804153023_update_wrap_order', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250805165838_update_workshift', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250807053029_update_workshift_1', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250807053434_update_workshift_2', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250807060547_update_workshift_4', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250810151016_update_new_version', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250811042325_update_cou_entity', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250811091656_update_order_remaining_time', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250818174403_update_coupon_usage', N'9.0.5')
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250824134152_update_order_Details', N'9.0.5')
GO
INSERT [dbo].[PaymentAccounts] ([Id], [StoreId], [BankName], [BankCode], [AccountNumber], [AccountName], [IsActive], [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy]) VALUES (N'b3dd81df-c7bf-443b-aad6-fadeb2831059', N'84859782-c27b-470e-883b-77e07df13a57', N'Ngân hàng TMCP Quân đội', N'MB', N'9890197038869', N'HOANG XUAN SON', 1, CAST(N'2025-08-26T22:20:35.5752225' AS DateTime2), NULL, NULL, NULL)
GO
