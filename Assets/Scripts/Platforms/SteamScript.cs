using UnityEngine;
using System.Collections;
#if !UNITY_WEBGL
using Steamworks;
#endif
public class SteamScript : MonoBehaviour
{
#if !UNITY_WEBGL
	void Start()
	{
		if (SteamManager.Initialized)
		{
			string name = SteamFriends.GetPersonaName();
			Debug.Log(name);
		}
	}
	protected Callback<GameOverlayActivated_t> m_GameOverlayActivated;

	private void OnEnable()
	{
		if (SteamManager.Initialized)
		{
			m_GameOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
		}
	}

	private void OnGameOverlayActivated(GameOverlayActivated_t pCallback)
	{
		if (pCallback.m_bActive != 0)
		{
			Debug.Log("Steam Overlay has been activated");
		}
		else
		{
			Debug.Log("Steam Overlay has been closed");
		}
	}
#endif
}