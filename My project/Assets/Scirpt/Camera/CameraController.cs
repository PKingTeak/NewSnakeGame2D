using UnityEngine;

/// <summary>
/// 메인 카메라에 부착하여 강제로 16:9 비율을 유지하고 남는 공간을 레터박스(검은 여백)로 만듭니다.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("기준 해상도")]
    [Tooltip("기준이 되는 가로 해상도")]
    public float targetWidth = 1920f;
    [Tooltip("기준이 되는 세로 해상도")]
    public float targetHeight = 1080f;

    void Awake()
    {
        SetLetterBox();
    }

    void SetLetterBox()
    {
        Camera cam = GetComponent<Camera>();
        
        // 목표하는 화면 비율 (예: 16:9 = 1.777...)
        float targetAspect = targetWidth / targetHeight;
        // 현재 실행 중인 기기의 화면 비율
        float currentAspect = (float)Screen.width / (float)Screen.height;

        // 현재 화면 비율과 목표 비율의 차이 계산
        float scaleHeight = currentAspect / targetAspect;

        Rect rect = cam.rect;

        if (scaleHeight < 1.0f)
        {
            // 기기 화면이 목표 비율보다 세로로 더 긴 경우 (예: 세로 방향 게임)
            // 위아래에 검은색 레터박스가 생겨야 함
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else
        {
            // 기기 화면이 목표 비율보다 가로로 더 긴 경우 (예: Z플립, S10 등 가로 모드)
            // 좌우에 검은색 필러박스가 생겨야 함
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        cam.rect = rect;
    }
}