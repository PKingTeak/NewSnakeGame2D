using System.Collections.Generic;
using UnityEngine;

public class FoodData
{
    [SerializeField] private string id;
    [SerializeField] private int enumindex;
    
    [SerializeField] private string unlockedBySlime;


    public string Id=>id;
    public int EnumIndex => enumindex;

    public string UnlockedBySlime => unlockedBySlime;

}
