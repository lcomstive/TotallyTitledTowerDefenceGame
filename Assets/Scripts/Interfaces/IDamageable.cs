/// <summary>
/// Hold health points that can be removed by outside sources (e.g. <see cref="IDamageDealer"/>)
/// </summary>
public interface IDamageable
{
	public float Health { get; }
	public float MaxHealth { get; }

	public void ApplyDamage(float damage);
	public void ApplyDamage(IDamageDealer dealer);

	public delegate void OnDamaged(float amount, IDamageDealer dealer);
	public event OnDamaged Damaged;

	public delegate void OnDestroyed(IDamageDealer dealer);
	public event OnDestroyed Destroyed;
}
