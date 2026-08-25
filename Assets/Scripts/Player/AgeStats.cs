using UnityEngine;

public class AgeStats
{
    // Цифровые характеристики
    public float JumpForce { get; }    
    public float Smoothing { get; }
    public float MaxSpeed { get; }
    public int JumpLimit { get; }

    // Визуальные и физические ассеты
    public Sprite VisualSprite { get; }
    public Vector2 ColliderSize { get; }
    public Vector2 ColliderOffset { get; }

    // Конструктор для заполнения данных в коде
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
