using UnityEngine.InputSystem;

public static class PlayerInput
{
    public static bool JumpPressed { get; private set; }
    public static bool RKeyPressed { get; private set; }
    public static bool EKeyPressed { get; private set; }
    public static bool DKeyHeld { get; private set; }
    public static bool AKeyHeld { get; private set; }
    public static bool EKeyHeld { get; private set; }

    public static void GatherInput()
    {
        if (Keyboard.current == null) return;

        JumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
        RKeyPressed = Keyboard.current.rKey.wasPressedThisFrame;
        EKeyPressed = Keyboard.current.eKey.wasPressedThisFrame;
        DKeyHeld = Keyboard.current.dKey.isPressed;
        AKeyHeld = Keyboard.current.aKey.isPressed;
        EKeyHeld = Keyboard.current.eKey.isPressed;
    }
}
