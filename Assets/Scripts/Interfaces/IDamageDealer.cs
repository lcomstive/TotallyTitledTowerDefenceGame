/// <summary>
/// Implementations of this class can deal damage,
/// also keep track of kill count
/// </summary>
public interface IDamageDealer
{
	public float Damage { get; }
	public int KillCount { get; set; }
}
