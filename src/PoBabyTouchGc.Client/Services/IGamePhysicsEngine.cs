namespace PoBabyTouchGc.Client.Services;

/// <summary>
/// Interface for game physics engine
/// Applying Strategy Pattern to allow different physics implementations
/// </summary>
public interface IGamePhysicsEngine
{
    /// <summary>
    /// Update physics for all circles
    /// </summary>
    void UpdateCirclePhysics(
        List<GameCircle> circles, 
        int areaWidth, 
        int areaHeight, 
        float speedMultiplier);

    /// <summary>
    /// Check if two circles are overlapping
    /// </summary>
    bool IsOverlapping(GameCircle a, GameCircle b);

    /// <summary>
    /// Get the maximum allowed speed for this physics mode
    /// </summary>
    float MaxSpeed { get; }
}

/// <summary>
/// Represents a game circle with physics properties
/// </summary>
public class GameCircle
{
    public int Id { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public int Radius { get; set; }
    public float VelocityX { get; set; }
    public float VelocityY { get; set; }
    public bool IsVisible { get; set; }
    public bool IsHit { get; set; }
    public string Person { get; set; } = "mommy";
    public string PersonClass { get; set; } = "mommy";
}
