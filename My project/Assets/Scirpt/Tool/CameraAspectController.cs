using UnityEngine;

/// <summary>
/// 고정 화면 비율 유지 (레터박스 / 필러박스).
///
/// [설정 방법]
/// 1. MainScene, GameScene 각각의 Main Camera에 이 컴포넌트 추가
/// 2. Target Aspect Width / Height 로 목표 비율 설정 (기본 9:16 세로)
/// 3. Camera Background Color를 검정으로 설정하면 레터박스 자연스럽게 표현
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraAspectController : MonoBehaviour
{
    [SerializeField] private float targetAspectWidth = 9f;
    [SerializeField] private float targetAspectHeight = 16f;

    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    private void Start()
    {
        Apply();
    }

    private void Apply()
    {
        float targetAspect = targetAspectWidth / targetAspectHeight;
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (Mathf.Approximately(scaleHeight, 1f))
        {
            _cam.rect = new Rect(0f, 0f, 1f, 1f);
            return;
        }

        if (scaleHeight < 1f)
        {
            // 화면이 목표보다 가로가 좁음 → 위아래 레터박스
            _cam.rect = new Rect(0f, (1f - scaleHeight) / 2f, 1f, scaleHeight);
        }
        else
        {
            // 화면이 목표보다 가로가 넓음 → 좌우 필러박스
            float scaleWidth = 1f / scaleHeight;
            _cam.rect = new Rect((1f - scaleWidth) / 2f, 0f, scaleWidth, 1f);
        }
    }
}
