using UnityEngine;

public enum FoodType
{
    Red,
    Green,
    Blue
}

public class Food : MonoBehaviour
{
    public FoodType foodType; // 인스펙터에서 설정하거나 생성 시 랜덤 부여
    public SpriteRenderer spriteRenderer;

    // 색상을 시각적으로 반영하기 위한 메서드
    public void SetType(FoodType type)
    {
        foodType = type;
        switch (type)
        {
            case FoodType.Red:
                spriteRenderer.color = Color.red;
                break;
            case FoodType.Green:
                spriteRenderer.color = Color.green;
                break;
            case FoodType.Blue:
                spriteRenderer.color = Color.blue;
                break;
        }
    }
}