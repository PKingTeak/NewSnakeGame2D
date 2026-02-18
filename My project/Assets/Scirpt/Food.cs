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

    public Sprite[] foodSprite;
    // 색상을 시각적으로 반영하기 위한 메서드
    public void SetType(FoodType type)
    {
        foodType = type;
        spriteRenderer.color = Color.white;

        if (foodSprite.Length > 0)
        {
            spriteRenderer.sprite = foodSprite[(int)type];
        }
    }
}