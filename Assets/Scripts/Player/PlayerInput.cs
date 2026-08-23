using UnityEngine.InputSystem;

public static class PlayerInput
{
    public static bool JumpPressed { get; private set; }
    public static bool RKeyPressed { get; private set; }
    public static bool DKeyPressed { get; private set; }
    public static bool AKeyPressed { get; private set; }
    public static bool EKeyPressed { get; private set; }
    public static bool EKeyHeld { get; private set; }

    public static void GatherInput()
    {
        if (Keyboard.current == null) return;

        JumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
        RKeyPressed = Keyboard.current.rKey.wasPressedThisFrame;
        DKeyPressed = Keyboard.current.dKey.wasPressedThisFrame;
        AKeyPressed = Keyboard.current.aKey.wasPressedThisFrame;
        EKeyPressed = Keyboard.current.eKey.wasPressedThisFrame;
        EKeyHeld = Keyboard.current.eKey.isPressed;
    }
}
