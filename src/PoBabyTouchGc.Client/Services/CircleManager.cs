namespace PoBabyTouchGc.Client.Services;

/// <summary>
/// Manages circle lifecycle: initialization, respawning, and updates
/// Applying Service Layer Pattern for circle management logic
/// </summary>
public class CircleManager
{
    private readonly IGamePhysicsEngine _physicsEngine;
    private readonly Random _random = new Random();
    private const int MaxPlacementAttempts = 50;
    private const float BaseSpeed = 0.5f;

    public CircleManager(IGamePhysicsEngine physicsEngine)
    {
        _physicsEngine = physicsEngine;
    }

    /// <summary>
    /// Initialize a collection of non-overlapping circles
    /// </summary>
    public List<GameCircle> InitializeCircles(
        int count, 
        int areaWidth, 
        int areaHeight,
        int circleRadius,
        string[] personTypes)
    {
        var circles = new List<GameCircle>();

        // Validate area is large enough for circles
        if (areaWidth <= 0 || areaHeight <= 0 || circleRadius <= 0)
        {
            return circles; // Return empty list if dimensions are invalid
        }

        if (areaWidth < 2 * circleRadius || areaHeight < 2 * circleRadius)
        {
            return circles; // Return empty list if area is too small
        }

        for (int i = 0; i < count; i++)
        {
            var circle = CreateCircle(i, areaWidth, areaHeight, circleRadius, personTypes);
            
            // Find non-overlapping position
            bool validPosition = false;
            int attempts = 0;

            while (!validPosition && attempts < MaxPlacementAttempts)
            {
                attempts++;
                circle.X = _random.Next(circle.Radius, areaWidth - circle.Radius);
                circle.Y = _random.Next(circle.Radius, areaHeight - circle.Radius);

                validPosition = !circles.Any(c => _physicsEngine.IsOverlapping(circle, c));
            }

            if (validPosition)
            {
                circles.Add(circle);
            }
        }

        return circles;
    }

    /// <summary>
    /// Respawn a circle at a new non-overlapping position
    /// </summary>
    public bool RespawnCircle(
        GameCircle circle, 
        List<GameCircle> allCircles, 
        int areaWidth, 
        int areaHeight)
    {
        // Ensure game area is large enough
        if (areaWidth < 2 * circle.Radius || areaHeight < 2 * circle.Radius)
        {
            return false;
        }

        bool validPosition = false;
        int attempts = 0;

        while (!validPosition && attempts < MaxPlacementAttempts)
        {
            attempts++;

            circle.X = _random.Next(circle.Radius, areaWidth - circle.Radius);
            circle.Y = _random.Next(circle.Radius, areaHeight - circle.Radius);

            // Check for overlaps with other visible circles
            validPosition = !allCircles.Any(c => 
                c.IsVisible && 
                c.Id != circle.Id && 
                _physicsEngine.IsOverlapping(circle, c));
        }

        if (validPosition)
        {
            // Change velocity to make movement varied
            circle.VelocityX = (float)(_random.NextDouble() * 2 - 1) * BaseSpeed;
            circle.VelocityY = (float)(_random.NextDouble() * 2 - 1) * BaseSpeed;
            circle.IsVisible = true;
            circle.IsHit = false;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Update all circles using the physics engine
    /// </summary>
    public void UpdateAllCircles(
        List<GameCircle> circles, 
        int areaWidth, 
        int areaHeight, 
        float speedMultiplier)
    {
        _physicsEngine.UpdateCirclePhysics(circles, areaWidth, areaHeight, speedMultiplier);
    }

    private GameCircle CreateCircle(
        int id, 
        int areaWidth, 
        int areaHeight, 
        int radius,
        string[] personTypes)
    {
        var person = personTypes[_random.Next(personTypes.Length)];
        
        return new GameCircle
        {
            Id = id,
            X = _random.Next(radius, areaWidth - radius),
            Y = _random.Next(radius, areaHeight - radius),
            Radius = radius,
            VelocityX = (float)(_random.NextDouble() * 2 - 1) * BaseSpeed,
            VelocityY = (float)(_random.NextDouble() * 2 - 1) * BaseSpeed,
            IsVisible = true,
            IsHit = false,
            Person = person,
            PersonClass = person
        };
    }
}
