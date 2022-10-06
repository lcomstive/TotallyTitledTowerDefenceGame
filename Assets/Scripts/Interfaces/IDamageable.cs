using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
	public float Damage { get; }
	public int KillCount { get; set; }
}
