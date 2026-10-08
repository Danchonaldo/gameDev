using UnityEngine;

// Единая точка чтения клавиатуры для скриптов способностей/уровней.
// Override позволяет подменить ввод (боты, автотесты); в игре он всегда null.
public static class GameInput
{
    public interface ISource
    {
        float Horizontal { get; }
        float Vertical { get; }
        bool GetKey(KeyCode key);
        bool GetKeyDown(KeyCode key);
    }

    public static ISource Override;

    public static float Horizontal => Override != null ? Override.Horizontal : Input.GetAxisRaw("Horizontal");
    public static float Vertical => Override != null ? Override.Vertical : Input.GetAxisRaw("Vertical");

    public static bool GetKey(KeyCode key) => Override != null ? Override.GetKey(key) : Input.GetKey(key);
    public static bool GetKeyDown(KeyCode key) => Override != null ? Override.GetKeyDown(key) : Input.GetKeyDown(key);

    // Вверх в воде: Space / W / стрелка вверх.
    public static bool UpHeld => GetKey(KeyCode.Space) || Vertical > 0.1f;
    public static bool DownHeld => Vertical < -0.1f;
}
