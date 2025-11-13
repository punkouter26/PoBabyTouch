namespace PoBabyTouchGc.Client.Services;

/// <summary>
/// Standard physics engine with realistic collisions and speed increases
/// Applying Strategy Pattern for physics behavior
/// </summary>
public class StandardPhysicsEngine : IGamePhysicsEngine
{
    public float MaxSpeed => 5.0f; // Higher max speed for normal mode

    public bool IsOverlapping(GameCircle a, GameCircle b)
    {
        double dx = a.X - b.X;
        double dy = a.Y - b.Y;
        double distance = Math.Sqrt(dx * dx + dy * dy);
        return distance < (a.Radius + b.Radius);
    }

    public void UpdateCirclePhysics(
        List<GameCircle> circles, 
        int areaWidth, 
        int areaHeight, 
        float speedMultiplier)
    {
        foreach (var circle in circles.Where(c => c.IsVisible))
        {
            // Update position based on velocity
            circle.X += circle.VelocityX * speedMultiplier;
            circle.Y += circle.VelocityY * speedMultiplier;

            // Boundary collision detection and response
            HandleBoundaryCollisions(circle, areaWidth, areaHeight);

            // Circle-to-circle collision detection and response
            HandleCircleCollisions(circle, circles);
        }
    }

    private void HandleBoundaryCollisions(GameCircle circle, int areaWidth, int areaHeight)
    {
        if (circle.X - circle.Radius < 0)
        {
            circle.X = circle.Radius;
            circle.VelocityX = Math.Abs(circle.VelocityX);
        }
        else if (circle.X + circle.Radius > areaWidth)
        {
            circle.X = areaWidth - circle.Radius;
            circle.VelocityX = -Math.Abs(circle.VelocityX);
        }

        if (circle.Y - circle.Radius < 0)
        {
            circle.Y = circle.Radius;
            circle.VelocityY = Math.Abs(circle.VelocityY);
        }
        else if (circle.Y + circle.Radius > areaHeight)
        {
            circle.Y = areaHeight - circle.Radius;
            circle.VelocityY = -Math.Abs(circle.VelocityY);
        }
    }

    private void HandleCircleCollisions(GameCircle circle, List<GameCircle> allCircles)
    {
        foreach (var otherCircle in allCircles.Where(c => c.IsVisible && c.Id != circle.Id))
        {
            if (!IsOverlapping(circle, otherCircle))
                continue;

            // Calculate collision response
            double dx = otherCircle.X - circle.X;
            double dy = otherCircle.Y - circle.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance == 0) continue; // Prevent division by zero

            // Normalized collision vector
            double nx = dx / distance;
            double ny = dy / distance;

            // Calculate relative velocity
            double vx = circle.VelocityX - otherCircle.VelocityX;
            double vy = circle.VelocityY - otherCircle.VelocityY;

            // Calculate relative velocity along collision normal
            double velocityAlongNormal = vx * nx + vy * ny;

            // Only resolve if circles are moving towards each other
            if (velocityAlongNormal > 0)
                continue;

            // Elastic collision response
            double bounce = 1.0; // Fully elastic
            double impulse = -(1 + bounce) * velocityAlongNormal;

            // Apply impulse equally to both circles
            circle.VelocityX -= (float)(impulse * nx);
            circle.VelocityY -= (float)(impulse * ny);
            otherCircle.VelocityX += (float)(impulse * nx);
            otherCircle.VelocityY += (float)(impulse * ny);

            // Move circles apart to prevent sticking
            float overlap = (float)((circle.Radius + otherCircle.Radius) - distance);
            circle.X -= (float)(overlap * 0.5 * nx);
            circle.Y -= (float)(overlap * 0.5 * ny);
            otherCircle.X += (float)(overlap * 0.5 * nx);
            otherCircle.Y += (float)(overlap * 0.5 * ny);
        }
    }
}
