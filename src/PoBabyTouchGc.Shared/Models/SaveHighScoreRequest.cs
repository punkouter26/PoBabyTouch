namespace PoBabyTouchGc.Shared.Models
{
    /// <summary>
    /// Request model for saving high scores
    /// </summary>
    public class SaveHighScoreRequest
    {
        public string PlayerInitials { get; set; } = string.Empty;
        public int Score { get; set; }
        public string? GameMode { get; set; } = "Default";
    }
}
