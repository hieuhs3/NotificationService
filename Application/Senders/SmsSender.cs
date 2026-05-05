using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationService.Domain.Events;
using NotificationService.Domain.Interfaces;

namespace NotificationService.Application.Senders;

public class SmsSender : INotificationSender
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsSender> _logger;

    public SmsSender(HttpClient httpClient, IConfiguration configuration, ILogger<SmsSender> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(NotificationEvent e)
    {
        if (e.Channel == "SMS" || e.Channel == "All")
        {
            var providerUrl = _configuration["SmsSettings:ProviderUrl"];
            var apiKey = _configuration["SmsSettings:ApiKey"];
            var fromNumber = _configuration["SmsSettings:FromNumber"] ?? "SynapseApp";

            if (string.IsNullOrEmpty(providerUrl))
            {
                _logger.LogWarning("SMS Provider URL is not configured. Mocking SMS dispatch for Event {EventId}.", e.EventId);
                return;
            }

            // In a real scenario, the recipient phone number would be fetched using e.UserId
            var toNumber = "+1234567890"; // Mock number

            var payload = new
            {
                from = fromNumber,
                to = toNumber,
                text = e.Message
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Assuming a generic API key header (e.g., Authorization: Bearer {apiKey} or x-api-key)
            if (!string.IsNullOrEmpty(apiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            }

            try
            {
                var response = await _httpClient.PostAsync(providerUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("SMS sent successfully to {ToNumber} for Event {EventId}", toNumber, e.EventId);
                }
                else
                {
                    _logger.LogError("Failed to send SMS to {ToNumber} for Event {EventId}. Status: {StatusCode}", toNumber, e.EventId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending SMS to {ToNumber} for Event {EventId}", toNumber, e.EventId);
            }
        }
    }
}
