using UnityEngine;

public class LockOrientation : MonoBehaviour
{
    void Start()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
#if UNITY_ANDROID
		Screen.SetResolution(1280, 720, true);
		Application.targetFrameRate = 60;
#endif

    }
}
