using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
	public static T instance { get; private set; }

	protected virtual void Awake()
	{
		if (instance == null)
			instance = this as T;
		else
		{
			string typeName = typeof(T).Name;
			Debug.LogWarning($"More than one instance of {typeName} found!!");

			Destroy(this.gameObject);

			return;
		}
	}
}
