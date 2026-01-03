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
            
            // Nếu chưa có instance, tạo mới
            if (instance == null)
            {
                GameObject go = new GameObject("MobileDetector");
                instance = go.AddComponent<MobileDetector>();
                DontDestroyOnLoad(go);
            }
            
            // Phát hiện mobile
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
        // Cách 1: Kiểm tra platform native (Android, iOS)
        if (Application.isMobilePlatform)
        {
            return true;
        }
        
        // Cách 2: WebGL - dùng JavaScript để phát hiện
        #if UNITY_WEBGL && !UNITY_EDITOR
        return IsMobileWebGL();
        #else
        // Trong Editor, có thể dùng screen size để test
        // Hoặc dùng build target để ước tính
        #if UNITY_EDITOR
        if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
        {
            // Trong editor với WebGL build target, dùng screen size
            return Screen.width < 1024 || Screen.height < 768;
        }
        #endif
        return false;
        #endif
    }
    
    #if UNITY_WEBGL && !UNITY_EDITOR
    // Gọi JavaScript function để phát hiện mobile
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
            // Fallback: dùng screen size nếu JavaScript không hoạt động
            return Screen.width < 1024 || Screen.height < 768;
        }
    }
    #endif
    
    // Reset cache (có thể gọi khi cần detect lại)
    public static void ResetCache()
    {
        cachedIsMobile = null;
    }
}

