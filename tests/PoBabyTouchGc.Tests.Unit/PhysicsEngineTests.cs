using Xunit;
using PoBabyTouchGc.Client.Services;

namespace PoBabyTouchGc.Tests.Unit;

/// <summary>
/// Unit tests for physics engines
/// Addresses Priority 9: Unit Test Coverage for Game Logic
/// </summary>
public class PhysicsEngineTests
{
    [Fact]
    public void StandardPhysicsEngine_IsOverlapping_TouchingCircles_ReturnsTrue()
    {
        // Arrange
        var engine = new StandardPhysicsEngine();
        var circle1 = new GameCircle { X = 100, Y = 100, Radius = 50 };
        var circle2 = new GameCircle { X = 140, Y = 100, Radius = 50 }; // 40 pixels apart, radii = 100 total

        // Act
        var isOverlapping = engine.IsOverlapping(circle1, circle2);

        // Assert
        Assert.True(isOverlapping);
    }

    [Fact]
    public void StandardPhysicsEngine_IsOverlapping_SeparateCircles_ReturnsFalse()
    {
        // Arrange
        var engine = new StandardPhysicsEngine();
        var circle1 = new GameCircle { X = 100, Y = 100, Radius = 50 };
        var circle2 = new GameCircle { X = 300, Y = 300, Radius = 50 }; // Far apart

        // Act
        var isOverlapping = engine.IsOverlapping(circle1, circle2);

        // Assert
        Assert.False(isOverlapping);
    }

    [Fact]
    public void BabyModePhysicsEngine_ClampVelocity_ExceedsMaxSpeed_ClampsCorrectly()
    {
        // Arrange
        var engine = new BabyModePhysicsEngine();
        var circles = new List<GameCircle>
        {
            new GameCircle 
            { 
                Id = 1, 
                X = 500, 
                Y = 500, 
                Radius = 50, 
                VelocityX = 5.0f,  // High velocity
                VelocityY = 5.0f,  // High velocity
                IsVisible = true 
            }
        };

        // Act
        engine.UpdateCirclePhysics(circles, 1000, 1000, 1.0f);

        // Assert
        var circle = circles[0];
        var speed = Math.Sqrt(circle.VelocityX * circle.VelocityX + circle.VelocityY * circle.VelocityY);
        Assert.True(speed <= engine.MaxSpeed * 1.01); // Allow small floating point error
    }

    [Fact]
    public void StandardPhysicsEngine_BoundaryCollision_BouncesOffLeftWall()
    {
        // Arrange
        var engine = new StandardPhysicsEngine();
        var circles = new List<GameCircle>
        {
            new GameCircle 
            { 
                Id = 1, 
                X = 10,      // Near left edge
                Y = 500, 
                Radius = 50, 
                VelocityX = -1.0f,  // Moving left
                VelocityY = 0, 
                IsVisible = true 
            }
        };

        // Act
        engine.UpdateCirclePhysics(circles, 1000, 1000, 1.0f);

        // Assert
        var circle = circles[0];
        Assert.True(circle.VelocityX > 0); // Should bounce right
        Assert.Equal(circle.Radius, circle.X); // Should be repositioned to boundary
    }

    [Fact]
    public void StandardPhysicsEngine_UpdatePosition_MovesCircle()
    {
        // Arrange
        var engine = new StandardPhysicsEngine();
        var originalX = 500.0;
        var circles = new List<GameCircle>
        {
            new GameCircle 
            { 
                Id = 1, 
                X = originalX, 
                Y = 500, 
                Radius = 50, 
                VelocityX = 2.0f, 
                VelocityY = 0, 
                IsVisible = true 
            }
        };

        // Act
        engine.UpdateCirclePhysics(circles, 1000, 1000, 1.0f);

        // Assert
        Assert.True(circles[0].X > originalX);
    }

    [Fact]
    public void BabyModePhysicsEngine_MaxSpeed_IsLowerThanStandard()
    {
        // Arrange
        var standardEngine = new StandardPhysicsEngine();
        var babyEngine = new BabyModePhysicsEngine();

        // Assert
        Assert.True(babyEngine.MaxSpeed < standardEngine.MaxSpeed);
    }

    [Fact]
    public void CircleManager_InitializeCircles_CreatesNonOverlappingCircles()
    {
        // Arrange
        var engine = new StandardPhysicsEngine();
        var manager = new CircleManager(engine);
        var personTypes = new[] { "mommy", "daddy", "baby" };

        // Act
        var circles = manager.InitializeCircles(5, 1000, 1000, 50, personTypes);

        // Assert
        Assert.Equal(5, circles.Count);
        
        // Check no overlaps
        for (int i = 0; i < circles.Count; i++)
        {
            for (int j = i + 1; j < circles.Count; j++)
            {
                Assert.False(engine.IsOverlapping(circles[i], circles[j]));
            }
        }
    }

    [Fact]
    public void CircleManager_RespawnCircle_FindsNonOverlappingPosition()
    {
        // Arrange
        var engine = new StandardPhysicsEngine();
        var manager = new CircleManager(engine);
        var circles = new List<GameCircle>
        {
            new GameCircle { Id = 1, X = 500, Y = 500, Radius = 50, IsVisible = true },
            new GameCircle { Id = 2, X = 700, Y = 700, Radius = 50, IsVisible = true }
        };
        var circleToRespawn = new GameCircle { Id = 3, Radius = 50 };

        // Act
        var success = manager.RespawnCircle(circleToRespawn, circles, 1000, 1000);

        // Assert
        Assert.True(success);
        Assert.True(circleToRespawn.IsVisible);
        Assert.False(circleToRespawn.IsHit);
        
        // Should not overlap with existing circles
        Assert.False(engine.IsOverlapping(circleToRespawn, circles[0]));
        Assert.False(engine.IsOverlapping(circleToRespawn, circles[1]));
    }

    [Fact]
    public void CircleManager_RespawnCircle_AreaTooSmall_ReturnsFalse()
    {
        // Arrange
        var engine = new StandardPhysicsEngine();
        var manager = new CircleManager(engine);
        var circles = new List<GameCircle>();
        var circle = new GameCircle { Id = 1, Radius = 50 };

        // Act - Area too small (50x50) for radius 50
        var success = manager.RespawnCircle(circle, circles, 50, 50);

        // Assert
        Assert.False(success);
    }
}

