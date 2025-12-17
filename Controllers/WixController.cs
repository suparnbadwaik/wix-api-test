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
        using(var render = new StreamReader(Request.Body))
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

    //[HttpGet("wixget")]
    //public async Task<IActionResult> Get()
    //{
    //    return await Task.Run(() => Ok("Wix Controller Works Fine"));
    //}

    //[HttpGet]
    //public IActionResult ReceiveWixGet([FromQuery] string token)
    //{

    //    if (string.IsNullOrEmpty(token))
    //        return BadRequest("Token is missing.");

    //    // Decode if URL-encoded
    //    var decodedToken = Uri.UnescapeDataString(token);

    //    var handler = new JwtSecurityTokenHandler();
    //    JwtSecurityToken jwtToken;

    //    try
    //    {
    //        jwtToken = handler.ReadJwtToken(decodedToken);
    //    }
    //    catch (Exception ex)
    //    {
    //        return BadRequest($"Invalid token format: {ex.Message}");
    //    }

    //    var instanceId = jwtToken.Claims.FirstOrDefault(c => c.Type == "instanceId")?.Value;
    //    return Ok(new { InstanceId = instanceId });
    //}

    ////[HttpPost("webhook/app-installed")]
    //public async Task<IActionResult> ReceiveWixRequest()
    //{

    //    string requestBody;
    //    using (var reader = new StreamReader(Request.Body))
    //    {
    //        requestBody = await reader.ReadToEndAsync();
    //    }

    //    WixInstanceData instanceData = new WixInstanceData();

    //    if (IsJwt(requestBody))
    //    {
    //        var handler = new JwtSecurityTokenHandler();
    //        var jwtToken = handler.ReadJwtToken(requestBody);

    //        instanceData.InstanceId = jwtToken.Claims.FirstOrDefault(c => c.Type == "instanceId")?.Value;
    //        instanceData.AppDefId = jwtToken.Claims.FirstOrDefault(c => c.Type == "appDefId")?.Value;
    //        instanceData.Permissions = jwtToken.Claims.FirstOrDefault(c => c.Type == "permissions")?.Value;
    //        instanceData.SiteOwnerId = jwtToken.Claims.FirstOrDefault(c => c.Type == "siteOwnerId")?.Value;
    //        instanceData.SiteId = jwtToken.Claims.FirstOrDefault(c => c.Type == "siteId")?.Value;
    //        instanceData.UserId = jwtToken.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
    //        instanceData.SignDate = jwtToken.Claims.FirstOrDefault(c => c.Type == "signDate")?.Value;
    //        instanceData.ExpirationDate = jwtToken.Claims.FirstOrDefault(c => c.Type == "expirationDate")?.Value;
    //        instanceData.IpAndPort = jwtToken.Claims.FirstOrDefault(c => c.Type == "ipAndPort")?.Value;
    //        instanceData.VendorProductId = jwtToken.Claims.FirstOrDefault(c => c.Type == "vendorProductId")?.Value;
    //        instanceData.MetaSiteId = jwtToken.Claims.FirstOrDefault(c => c.Type == "metaSiteId")?.Value;

    //        // Collect any additional claims
    //        var knownClaims = new[] { "instanceId", "appDefId", "permissions", "siteOwnerId", "siteId", "uid", "signDate", "expirationDate", "ipAndPort", "vendorProductId", "metaSiteId" };
    //        instanceData.AdditionalClaims = jwtToken.Claims
    //            .Where(c => !knownClaims.Contains(c.Type))
    //            .ToDictionary(c => c.Type, c => (object)c.Value);

    //        return Ok(instanceData);
    //    }
    //    else
    //    {
    //        try
    //        {
    //            var jsonObj = JsonSerializer.Deserialize<JsonElement>(requestBody);

    //            instanceData.InstanceId = jsonObj.GetPropertyOrNull("instanceId");
    //            instanceData.AppDefId = jsonObj.GetPropertyOrNull("appDefId");
    //            instanceData.Permissions = jsonObj.GetPropertyOrNull("permissions");
    //            instanceData.SiteOwnerId = jsonObj.GetPropertyOrNull("siteOwnerId");
    //            instanceData.SiteId = jsonObj.GetPropertyOrNull("siteId");
    //            instanceData.UserId = jsonObj.GetPropertyOrNull("uid");
    //            instanceData.SignDate = jsonObj.GetPropertyOrNull("signDate");
    //            instanceData.ExpirationDate = jsonObj.GetPropertyOrNull("expirationDate");
    //            instanceData.IpAndPort = jsonObj.GetPropertyOrNull("ipAndPort");
    //            instanceData.VendorProductId = jsonObj.GetPropertyOrNull("vendorProductId");
    //            instanceData.MetaSiteId = jsonObj.GetPropertyOrNull("metaSiteId");

    //            // Collect additional properties
    //            var knownProps = new[] { "instanceId", "appDefId", "permissions", "siteOwnerId", "siteId", "uid", "signDate", "expirationDate", "ipAndPort", "vendorProductId", "metaSiteId" };
    //            instanceData.AdditionalClaims = jsonObj.EnumerateObject()
    //                                            .Where(p => !knownProps.Contains(p.Name))
    //                                            .Select(p => new KeyValuePair<string, object>(p.Name,
    //                                                p.Value.ValueKind switch
    //                                                {
    //                                                    JsonValueKind.String => p.Value.GetString()!,
    //                                                    JsonValueKind.Number => TryGetNumber(p.Value)!,
    //                                                    JsonValueKind.True => true,
    //                                                    JsonValueKind.False => false,
    //                                                    _ => p.Value.GetRawText()
    //                                                }))
    //                                            .Where(kvp => kvp.Value != null)
    //                                            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);


    //            // Helper method for numbers
    //            static object? TryGetNumber(JsonElement value)
    //            {
    //                if (value.TryGetInt64(out var l)) return l;
    //                if (value.TryGetDouble(out var d)) return d;
    //                return value.GetRawText();
    //            }



    //            return Ok(instanceData);
    //        }
    //        catch
    //        {
    //            return BadRequest("Unsupported request format.");
    //        }
    //    }
    //}

    //private bool IsJwt(string token)
    //{
    //    var parts = token.Split('.');
    //    return parts.Length == 3;
    //}

    //[HttpPost]
    //public async Task<IActionResult> HandleWebhook()
    //{
    //    var body = await new StreamReader(Request.Body).ReadToEndAsync();

    //    try
    //    {
    //        await _wixService.ProcessWebhookAsync(body);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine("Webhook error: " + ex.Message);
    //        return StatusCode(500, "Webhook error: " + ex.Message);
    //    }

    //    return Ok();
    //}
}

//// Extension method for safe property extraction
//public static class JsonElementExtensions
//{
//    public static string? GetPropertyOrNull(this JsonElement element, string propertyName)
//    {
//        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var prop))
//            return prop.GetString();
//        return null;
//    }
//}
