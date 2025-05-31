namespace FOCS.Application.DTOs.AdminServiceDTO
{
    public class MenuItemAdminServiceDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Images { get; set; }
        public double BasePrice { get; set; }
        public bool IsAvailable { get; set; }

        public Guid MenuCategoryId { get; set; }
        public Guid StoreId { get; set; }
    }
}
