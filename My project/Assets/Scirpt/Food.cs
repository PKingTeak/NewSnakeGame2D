using UnityEngine;

public enum FoodType
{
    fire,
    grass,
    ice,
    smoke,
    lava,
    poison,
    electric
}

public class Food : MonoBehaviour
{
    public FoodType foodType; // �ν����Ϳ��� �����ϰų� ���� �� ���� �ο�
    public SpriteRenderer spriteRenderer;

    public Sprite[] foodSprite;
    // ������ �ð������� �ݿ��ϱ� ���� �޼���
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