using System.ComponentModel.DataAnnotations;

namespace PoBabyTouchGc.Api.Models;

/// <summary>
/// Request model for client-side log messages sent to the server
/// </summary>
public class ClientLogRequest
{
    /// <summary>
    /// The log message content
    /// </summary>
    [Required]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Log level (Debug, Information, Warning, Error, Critical)
    /// </summary>
    [Required]
    public string Level { get; set; } = "Information";

    /// <summary>
    /// Timestamp when the log was generated on the client
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Category/source of the log message
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Client session identifier
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// User identifier (if available)
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Current game mode when log was generated
    /// </summary>
    public string? GameMode { get; set; }

    /// <summary>
    /// Additional structured data related to the log entry
    /// </summary>
    public Dictionary<string, object>? Data { get; set; }
}
