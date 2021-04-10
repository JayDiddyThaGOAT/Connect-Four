using UnityEngine;
using UnityEngine.SceneManagement;

public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                var objs = FindObjectsOfType(typeof(T)) as T[];
                if (objs.Length > 0)
                    _instance = objs[0];
                if (objs.Length > 1)
                    Debug.LogError("There is more than one " + typeof(T).Name + " in the scene.");
                
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.hideFlags = HideFlags.HideAndDontSave;
                    _instance = obj.AddComponent<T>();
                }
            }

            return _instance;
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
}

public class SingletonPersistent<T> : MonoBehaviour where T : Component
{
    private static T _instance;

    public static T Instance {get; private set;}

    public virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public virtual void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode){}
}
