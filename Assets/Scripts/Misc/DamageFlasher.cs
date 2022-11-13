using UnityEngine;
using System.Threading.Tasks;

public class DamageFlasher : MonoBehaviour
{
	[SerializeField]
	private AnimationCurve m_FlashCurve = new AnimationCurve(new Keyframe[]
	{
        // Default: flash twice, ending after 0.8 seconds
        new Keyframe(0.0f, 0.0f),
		new Keyframe(0.2f, 1.0f),
		new Keyframe(0.4f, 0.0f),
		new Keyframe(0.6f, 1.0f),
		new Keyframe(0.8f, 0.0f)
	});

	[SerializeField]
	private Color m_FlashColor = Color.white;

	// Cache local components
	private IDamageable m_Damageable;
	private MeshRenderer[] m_Renderers;
	private Color[] m_OriginalColours;

	private void Start()
	{
		m_Damageable = GetComponent<IDamageable>();
		m_Damageable.Damaged += OnDamaged;

		m_Renderers = GetComponentsInChildren<MeshRenderer>(true);
		m_OriginalColours = new Color[m_Renderers.Length];
		for (int i = 0; i < m_OriginalColours.Length; i++)
			m_OriginalColours[i] = m_Renderers[i].material.color;
	}

	private void OnDestroy() => m_Damageable.Damaged -= OnDamaged;

	private void OnDamaged(float amount, IDamageDealer dealer) => Flash();

	public async void Flash()
	{
		float time = 0.0f;
		float desiredTime = m_FlashCurve.keys[^1].time;
		while (time < desiredTime && m_Renderers.Length > 0 && m_Renderers[0] != null)
		{
			for (int i = m_Renderers.Length - 1; i >= 0; i--)
			{
				if (m_Renderers[i])
					m_Renderers[i].material.color = Color.Lerp(m_OriginalColours[i], m_FlashColor, m_FlashCurve.Evaluate(time));
			}

			float deltaTime = Time.deltaTime;
			await Task.Delay((int)(deltaTime * 1000));
			time += deltaTime;
		}

		for (int i = 0; i < m_Renderers.Length; i++)
			if (m_Renderers[i])
				m_Renderers[i].material.color = m_OriginalColours[i];
	}
}
