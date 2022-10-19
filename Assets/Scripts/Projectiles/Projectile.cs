using UnityEngine;

public abstract class Projectile : MonoBehaviour, IProjectile
{
	public Elements Element { get; set; }

	public float ElementTime { get; set; }

	public BuildableInfo Shooter { get; set; }

	private void OnCollisionEnter(Collision collision)
	{
		Destroy(gameObject);
		Hit(collision.gameObject);
	}

	protected abstract void Hit(GameObject other);
}
