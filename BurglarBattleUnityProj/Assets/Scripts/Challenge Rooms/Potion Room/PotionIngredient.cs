using UnityEngine;
using PotionEffects;

public class PotionIngredient : MonoBehaviour
{
    public IngredientEffect primaryEffect;
    public IngredientEffect secondaryEffect;

    [Header("Required Components")]
    [SerializeField] private PickUpInteractable _pickUpScript;

    private void Awake()
    {
        _pickUpScript = GetComponent<PickUpInteractable>();
    }
}
