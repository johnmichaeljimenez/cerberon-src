namespace Main.Gameplay.Level;

public struct WorldSettings //struct so that it cannot be null
{
	public Vector2 PlayerSpawnPoint;
	public Color AmbientColor;
	public string AmbientSound;
	public Vector2 WorldSize; //intentionally closed-space world

	[JsonProperty]
    public Dictionary<string, object?> Config { get; set; }
}
