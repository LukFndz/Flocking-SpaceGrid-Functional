using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    public static List<Food> allFoods = new List<Food>();

    private void Start()
    {
        allFoods.Add(this);
    }
}
