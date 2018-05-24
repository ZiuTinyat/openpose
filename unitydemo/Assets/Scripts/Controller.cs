using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {

    // Singleton
    private static Controller instance = null;

    private PlayMode mode = PlayMode.Default;

    // Interface
    public static PlayMode Mode { get { try { return instance.mode; } catch { return PlayMode.Default; } } set { try { instance.mode = value; } catch { } } }
    public static void StartPlay(PlayMode mode = PlayMode.Default)
    {
        Mode = mode;
        instance.LoadMain();
    }

    public void SetModeStream()
    {
        mode = PlayMode.Stream;        
    }
    
    private void Awake()
    {
        instance = this;
    }

    private void LoadMain()
    {
        DontDestroyOnLoad(gameObject);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }
}

public enum PlayMode
{
    Default,
    Stream,
    FileJson,
    FileFbx,
    FileBvh
}
