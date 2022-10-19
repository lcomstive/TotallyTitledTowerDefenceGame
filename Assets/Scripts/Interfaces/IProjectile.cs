public interface IProjectile
{
	/// <summary>
	/// Element to apply to an <see cref="IModifierHolder"/> if hit
	/// </summary>
	public Elements Element { get; set; }

	/// <summary>
	/// How long to apply <see cref="Element"/>, in seconds
	/// </summary>
	public float ElementTime { get; set; }

	/// <summary>
	/// Information about the entity that spawned this projectile
	/// </summary>
	public BuildableInfo Shooter { get; set; }
}
