using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mail;
using BinLord.Models;

namespace BinLord.Services;

public record NotificationResult(bool Attempted, bool Success, string? Error);

public record NotificationOutcome(NotificationResult Email, NotificationResult Ntfy);

/// <summary>
/// Sends a reminder out over whichever channels are enabled in <see cref="AppSettings"/>.
/// </summary>
public class NotificationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IHttpClientFactory httpClientFactory, ILogger<NotificationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<NotificationOutcome> SendAsync(AppSettings settings, string title, string body, string? ntfyTags = null)
    {
        var email = await TrySendAsync(settings.EmailNotificationsEnabled, () => SendEmailAsync(settings, title, body), "email");
        var ntfy = await TrySendAsync(settings.NtfyEnabled, () => SendNtfyAsync(settings, title, body, ntfyTags), "ntfy");
        return new NotificationOutcome(email, ntfy);
    }

    private async Task<NotificationResult> TrySendAsync(bool enabled, Func<Task> send, string channel)
    {
        if (!enabled)
        {
            return new NotificationResult(Attempted: false, Success: false, Error: null);
        }

        try
        {
            await send();
            return new NotificationResult(Attempted: true, Success: true, Error: null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send {Channel} notification.", channel);
            return new NotificationResult(Attempted: true, Success: false, Error: ex.Message);
        }
    }

    private static Task SendEmailAsync(AppSettings settings, string title, string body)
    {
        if (string.IsNullOrWhiteSpace(settings.SmtpHost) || string.IsNullOrWhiteSpace(settings.EmailFrom) || string.IsNullOrWhiteSpace(settings.EmailTo))
        {
            throw new InvalidOperationException("Email notifications are enabled but not fully configured.");
        }

        using var client = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
        {
            EnableSsl = settings.SmtpUseSsl,
        };

        if (!string.IsNullOrWhiteSpace(settings.SmtpUsername))
        {
            client.Credentials = new NetworkCredential(settings.SmtpUsername, settings.SmtpPassword);
        }

        using var message = new MailMessage(settings.EmailFrom, settings.EmailTo, title, body);
        return client.SendMailAsync(message);
    }

    private async Task SendNtfyAsync(AppSettings settings, string title, string body, string? tags)
    {
        if (string.IsNullOrWhiteSpace(settings.NtfyServerUrl) || string.IsNullOrWhiteSpace(settings.NtfyTopic))
        {
            throw new InvalidOperationException("ntfy notifications are enabled but not fully configured.");
        }

        var client = _httpClientFactory.CreateClient(nameof(NotificationService));

        // Publish via ntfy's JSON API (POST to the server root with the topic
        // in the body) rather than the header-based form, so the title and
        // message can contain any UTF-8 text without header-encoding issues.
        using var request = new HttpRequestMessage(HttpMethod.Post, settings.NtfyServerUrl.TrimEnd('/') + "/")
        {
            Content = JsonContent.Create(new
            {
                topic = settings.NtfyTopic,
                title,
                message = body,
                tags = string.IsNullOrWhiteSpace(tags) ? null : new[] { tags },
            }),
        };

        if (!string.IsNullOrWhiteSpace(settings.NtfyToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.NtfyToken);
        }

        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
