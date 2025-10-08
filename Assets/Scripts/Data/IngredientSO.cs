using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient", menuName = "Cooking/Ingredient")]
public class IngredientSO : ScriptableObject
{
    public string ingredientName;
    public Sprite icon;
}
