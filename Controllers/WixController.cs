using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using WixInstallation;
using WixInstallation.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

[ApiController]
[Route("api/Wix")]
public class WixController : ControllerBase
{
    private readonly string _wixPublicKeyPem;

    public WixController(IConfiguration configuration)
    {
        _wixPublicKeyPem = configuration["Wix:WebhookPublicKey"];
    }

    [HttpPost("register")]
    public async Task<IActionResult> HandleInstallation()
    {
        string jwtToken;
        using (var render = new StreamReader(Request.Body))
        {
            jwtToken = await render.ReadToEndAsync();
        }

        if (string.IsNullOrEmpty(jwtToken))
        {
            return BadRequest("Empty Payload");
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = GetRsaSecurityKey(_wixPublicKeyPem)
            };

            var principal = tokenHandler.ValidateToken(jwtToken, validationParameters, out SecurityToken validatedToken);

            var jwt = (JwtSecurityToken)validatedToken;

            var rawData = jwt.Payload["data"];
            JsonElement dataElement;

            if (rawData is string jsonString)
            {
                dataElement = JsonDocument.Parse(jsonString).RootElement;
            }
            else if (rawData is JsonElement jsonElement)
            {
                dataElement = jsonElement;
            }
            else
            {
                return BadRequest();
            }

            var instanceId = dataElement.GetProperty("instanceId").GetString();
            string eventType = dataElement.GetProperty("eventType").GetString();
            //string webhookId = dataElement.GetProperty("webhookId").GetString();

            string innerJson = dataElement.GetProperty("data").GetString();
            using var innerDoc = JsonDocument.Parse(innerJson);
            var inner = innerDoc.RootElement;
            string appId = inner.GetProperty("appId").GetString();
            string originInstanceId = inner.GetProperty("originInstanceId").GetString();

            string identityJson = dataElement.GetProperty("identity").GetString();
            using var identityDoc = JsonDocument.Parse(identityJson);
            var identity = identityDoc.RootElement;
            string identityType = identity.GetProperty("identityType").GetString();
            string wixUserId = identity.GetProperty("wixUserId").GetString();

            long issuedAt = jwt.Payload.TryGetValue("iat", out var iat)
    ? Convert.ToInt64(iat)
    : 0;

            long expiresAt = jwt.Payload.TryGetValue("exp", out var exp)
                ? Convert.ToInt64(exp)
                : 0;

            DateTime issuedAtUtc = DateTimeOffset.FromUnixTimeSeconds(issuedAt).UtcDateTime;
            DateTime expiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(expiresAt).UtcDateTime;


            var evt = new WixAppInstalledEvent
            {
                InstanceId = instanceId,
                EventType = eventType,
                //WebhookId = webhookId,
                AppId = appId,
                OriginInstanceId = originInstanceId,
                IdentityType = identityType,
                WixUserId = wixUserId,
                IssuedAtUtc = issuedAtUtc,
                ExpiresAtUtc = expiresAtUtc
            };

        }
        catch (Exception ex)
        {
        }
        return Ok();
    }

    private SecurityKey GetRsaSecurityKey(string wixPublicKeyPem)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(wixPublicKeyPem.ToCharArray());
        return new RsaSecurityKey(rsa);
    }
}

public class WixAppInstalledEvent
{
    public string InstanceId { get; set; }
    public string EventType { get; set; }
    public string WebhookId { get; set; }

    public string AppId { get; set; }
    public string OriginInstanceId { get; set; }

    public string IdentityType { get; set; }
    public string WixUserId { get; set; }

    public DateTime IssuedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
}
