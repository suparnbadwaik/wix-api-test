namespace WixInstallation.Models
{
    public class WixInstanceData
    {
        public string? InstanceId { get; set; }
        public string? AppDefId { get; set; }
        public string? Permissions { get; set; }
        public string? SiteOwnerId { get; set; }
        public string? SiteId { get; set; }
        public string? UserId { get; set; }
        public string? SignDate { get; set; }
        public string? ExpirationDate { get; set; }
        public string? IpAndPort { get; set; }
        public string? VendorProductId { get; set; }
        public string? MetaSiteId { get; set; }
        public Dictionary<string, object>? AdditionalClaims { get; set; }
    }
}