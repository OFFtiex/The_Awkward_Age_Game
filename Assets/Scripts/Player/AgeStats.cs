using UnityEngine;

public class AgeStats
{
    public float MaxSpeed  { get; }
    public float JumpForce { get; }   
    public int   JumpLimit { get; }
    public float Smoothing { get; }

    public Sprite VisualSprite    { get; }
    public Vector2 ColliderSize   { get; }
    public Vector2 ColliderOffset { get; }

    public AgeStats(float speed, float jumpForce, int jumpLimit, float smoothing, Sprite sprite, Vector2 colSize, Vector2 colOffset)
    {
        MaxSpeed = speed;
        JumpForce = jumpForce;
        JumpLimit = jumpLimit;
        Smoothing = smoothing;

        VisualSprite = sprite;
        ColliderSize = colSize;
        ColliderOffset = colOffset;
    }
}
