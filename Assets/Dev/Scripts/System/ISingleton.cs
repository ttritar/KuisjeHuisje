using UnityEngine;

public class ISingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] private bool _dontDestroyOnLoad = true;
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance != null) 
                return _instance;

            _instance = FindAnyObjectByType<T>();
            if (_instance != null)
                return _instance;

            var obj = new GameObject(typeof(T).Name);
            _instance = obj.AddComponent<T>();
            return _instance;
        }
    }

    // START
    //--------------------------------------------------
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            if (_dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}