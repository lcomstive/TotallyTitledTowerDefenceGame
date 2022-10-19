using UnityEngine;
using GameAnalyticsSDK;

[RequireComponent(typeof(GameAnalytics))]
public class AnalyticsInit : MonoBehaviour, IGameAnalyticsATTListener
{
	private void Start()
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer)
			GameAnalytics.RequestTrackingAuthorization(this);
		else
			GameAnalytics.Initialize();
	}

	public void GameAnalyticsATTListenerDenied()		=> GameAnalytics.Initialize();
	public void GameAnalyticsATTListenerAuthorized()	=> GameAnalytics.Initialize();
	public void GameAnalyticsATTListenerRestricted()	=> GameAnalytics.Initialize();
	public void GameAnalyticsATTListenerNotDetermined() => GameAnalytics.Initialize();
}
