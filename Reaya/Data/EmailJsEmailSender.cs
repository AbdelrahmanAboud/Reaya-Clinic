using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Reaya.Data;

public sealed class EmailJsEmailSender : IEmailSender
{
    private const string Endpoint = "https://api.emailjs.com/api/v1.0/email/send";
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public EmailJsEmailSender(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var serviceId = _configuration["EmailJs:ServiceId"];
        var templateId = _configuration["EmailJs:TemplateId"];
        var publicKey = _configuration["EmailJs:PublicKey"];
        var privateKey = _configuration["EmailJs:PrivateKey"];

        if (string.IsNullOrWhiteSpace(serviceId) ||
            string.IsNullOrWhiteSpace(templateId) ||
            string.IsNullOrWhiteSpace(publicKey) ||
            string.IsNullOrWhiteSpace(privateKey))
        {
            throw new InvalidOperationException(
                "EmailJS is not configured. Set EmailJs:ServiceId, EmailJs:TemplateId, EmailJs:PublicKey, and EmailJs:PrivateKey.");
        }

        var request = new EmailJsRequest
        {
            ServiceId = serviceId,
            TemplateId = templateId,
            UserId = publicKey,
            AccessToken = privateKey,
            TemplateParams = new Dictionary<string, object>
            {
                ["to_email"] = email,
                ["subject"] = subject,
                ["html_message"] = htmlMessage
            }
        };

        using var response = await _httpClient.PostAsJsonAsync(Endpoint, request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"EmailJS rejected the email request: {error}");
        }
    }

    private sealed class EmailJsRequest
    {
        [JsonPropertyName("service_id")]
        public string ServiceId { get; init; } = string.Empty;

        [JsonPropertyName("template_id")]
        public string TemplateId { get; init; } = string.Empty;

        [JsonPropertyName("user_id")]
        public string UserId { get; init; } = string.Empty;

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; init; } = string.Empty;

        [JsonPropertyName("template_params")]
        public Dictionary<string, object> TemplateParams { get; init; } = new();
    }
}
