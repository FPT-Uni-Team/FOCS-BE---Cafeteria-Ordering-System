namespace FOCS.Application.DTOs.AdminServiceDTO
{
    public class MenuItemVariantAdminServiceDTO
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public int PrepPerTime { get; set; }
        public int QuantityPerTime { get; set; }
        public bool IsAvailable { get; set; }
    }
}
