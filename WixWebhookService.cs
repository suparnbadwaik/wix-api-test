using System.Text.Json;
using WixInstallation.Models;
using System.Security.Cryptography;

namespace WixInstallation
{
    public class WixWebhookService
    {
        // Same public key as Node code
        private const string PUBLIC_KEY = @"
-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAvsg4rfjQafrnfCH/VmHr
YVj/6KzUKxNpmIGyh/qMetp/izTkXYqAGccd8smOByX7IvdVRUprkxVNG94o/PCe
koc9wZrGGLRQUuIK3L3YZjryP6CpNnywEhwEr8wK+0rcub7VRe4Rnbz5zS60QBrm
AxW7FvHkwfrsDnP07szCjMBu5ZhkM8R9/CUaQrnZHIJTXwLxKFtlncWIpG5gF9zv
0xdoNO0R01wYWPBuj6LyfNmYRFh95/1Q5EuhzqwlaPgZIcEepB5GMZXbafwa3h7J
CfKcpCL0czON1AZ/Q42iBh3MNONjIoWXBxkKmpfcqd5FL5Xv6MmWMKpwHUI6FGOD
SwIDAQAB
-----END PUBLIC KEY-----";

        public async Task ProcessWebhookAsync(string rawBody)
        {
            var payload = JsonSerializer.Deserialize<WixWebhookEnvelope>(rawBody);

            if (payload == null)
                throw new Exception("Invalid JSON");

            // Route event (similar to onAppInstanceRemoved in Node)
            switch (payload.EventType)
            {
                case "APP_INSTANCE_REMOVED":
                    OnAppInstanceRemoved(payload);
                    break;

                case "APP_INSTALLED":
                    OnAppInstalled(payload);
                    break;

                default:
                    Console.WriteLine("Unhandled event: " + payload.EventType);
                    break;
            }
        }

        private void OnAppInstanceRemoved(WixWebhookEnvelope envelope)
        {
            Console.WriteLine("onAppInstanceRemoved invoked:");
            Console.WriteLine($"Instance ID: {envelope.Data.InstanceId}");
        }

        private void OnAppInstalled(WixWebhookEnvelope envelope)
        {
            Console.WriteLine("onAppInstalled invoked:");
            Console.WriteLine($"Installed on Site: {envelope.Data.SiteId}");
            Console.WriteLine($"Instance: {envelope.Data.InstanceId}");
        }
    }
}
