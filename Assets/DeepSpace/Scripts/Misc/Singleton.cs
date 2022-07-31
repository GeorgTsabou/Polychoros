using UnityEngine;

namespace DeepSpace
{
	// This class is originally from http://wiki.unity3d.com/index.php/Singleton and got slightly modified.

	/// <summary>
	/// Inherit from this base class to create a singleton.
	/// e.g. public class MyClassName : Singleton<MyClassName> {}
	/// </summary>
	public class Singleton<T> : MonoBehaviour where T : Singleton<T>
	{
		// Check to see if we're about to be destroyed.
		private static bool m_ShuttingDown = false;
		private static object m_Lock = new object();
		private static T m_Instance;

		/// <summary>
		/// Access singleton instance through this propriety.
		/// </summary>
		public static T Instance
		{
			get
			{
				if (m_ShuttingDown)
				{
					Debug.LogWarning("[Singleton] Instance \"" + typeof(T) + "\" is already destroyed. Returning null.");
					return null;
				}

				lock (m_Lock)
				{
					if (m_Instance == null)
					{
						// Search for existing instance.
						m_Instance = (T)FindObjectOfType(typeof(T));

						// Create new instance if one doesn't already exist.
						if (m_Instance == null)
						{
							Debug.LogWarning("An instance of " + typeof(T).Name + "could not be found. If you want to use this class, create an instance of it in the scene.");
						}
						else
						{
							// This is only called the first time, Instance is called.
							m_Instance.InstanceGotAssigned();
						}
					}

					return m_Instance;
				}
			}
		}

		protected virtual void InstanceGotAssigned()
		{
			// Implement this, if you want to get notified, when the Singleton was used the first time (e.g. for initialization).
		}

		protected virtual void OnApplicationQuit()
		{
			m_ShuttingDown = true;
		}
		
		protected virtual void OnDestroy()
		{
			m_ShuttingDown = true;
		}
	}
}