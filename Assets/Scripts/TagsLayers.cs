public static class Tags
{
    public const string Untagged = "Untagged";
    public const string Respawn = "Respawn";
    public const string Finish = "Finish";
    public const string EditorOnly = "EditorOnly";
    public const string MainCamera = "MainCamera";
    public const string Player = "Player";
    public const string GameController = "GameController";
}

public enum Layer
{
    Default = 0,
    TransparentFX = 1,
    Ignore_Raycast = 2,
    Water = 4,
    UI = 5,

    /// <summary>
    ///     View layer, mostly used for player's local viewmodel objects.
    /// </summary>
    View = 8,

    /// <summary>
    ///     Players layer contains all enemy player objects.
    /// </summary>
    Players = 9,
    Post_Processing = 10
}