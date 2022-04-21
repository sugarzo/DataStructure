using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
	public static T Instance;

	protected virtual void Awake()
	{
		if (Instance == null)
		{
			Instance = this as T;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			if (Instance != this)
			{
				Destroy(gameObject);
			}
		}
	}
}
