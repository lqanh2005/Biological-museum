using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Script để phát hiện mobile device trong WebGL
/// Sử dụng JavaScript để kiểm tra user agent và screen size
/// </summary>
public class MobileDetector : MonoBehaviour
{
    private static MobileDetector instance;
    private static bool? cachedIsMobile = null;

    public static bool IsMobile
    {
        get
        {
            if (cachedIsMobile.HasValue)
                return cachedIsMobile.Value;


            if (instance == null)
            {
                GameObject go = new GameObject("MobileDetector");
                instance = go.AddComponent<MobileDetector>();
                DontDestroyOnLoad(go);
            }


            cachedIsMobile = DetectMobile();
            return cachedIsMobile.Value;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    static bool DetectMobile()
    {

        if (Application.isMobilePlatform)
        {
            return true;
        }


        #if UNITY_WEBGL && !UNITY_EDITOR
        return IsMobileWebGL();
        #else


        #if UNITY_EDITOR
        if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
        {

            return Screen.width < 1024 || Screen.height < 768;
        }
        #endif
        return false;
        #endif
    }

    #if UNITY_WEBGL && !UNITY_EDITOR

    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern bool IsMobileDevice();

    static bool IsMobileWebGL()
    {
        try
        {
            return IsMobileDevice();
        }
        catch
        {

            return Screen.width < 1024 || Screen.height < 768;
        }
    }
    #endif


    public static void ResetCache()
    {
        cachedIsMobile = null;
    }
}

