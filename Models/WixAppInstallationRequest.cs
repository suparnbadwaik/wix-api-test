using System.Text.Json;

namespace WixInstallation.Models
{
    public class WixWebhookEnvelope
    {
        public string EventType { get; set; }
        public WixWebhookData Data { get; set; }
        public WixWebhookMetadata Metadata { get; set; }
    }

    public class WixWebhookData
    {
        public string InstanceId { get; set; }
        public string SiteId { get; set; }
        public string OwnerId { get; set; }
    }

    public class WixWebhookMetadata
    {
        public string InstanceId { get; set; }
        public string EventTime { get; set; }
    }
}