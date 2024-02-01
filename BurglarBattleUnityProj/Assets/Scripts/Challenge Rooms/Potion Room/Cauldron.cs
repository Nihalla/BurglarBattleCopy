using System.Collections.Generic;
using UnityEngine;
using PotionEffects;

public class Cauldron : MonoBehaviour
{
    [SerializeField] private LayerMask _ingredientLayerMask;

    [Header("Mixing Behaviour")]
    private const int REQUIRED_INGREDIENTS = 2;
    private GameObject[] _currentIngredients;
    private IngredientEffect[] _currentEffects;
    private int _ingredientsIndex;
    private int _effectsIndex;

    [Header("Potion Effect Tuning")]
    [SerializeField] private float _basePotionDuration = 30f;
    [Space]
    [SerializeField] private float _minorSpeedBuff = 0.2f;
    [SerializeField] private float _majorSpeedBuff = 0.5f;
    [SerializeField] private float _minorSlowDebuff = -0.2f;
    [SerializeField] private float _majorSlowDebuff = -0.5f;
    [Space]
    [SerializeField] private float _durationUpIncrease = 10f;
    [SerializeField] private float _durationDownDecrease = -10f;

    [Header("Required Components")]
    [SerializeField] private Collider _trigger;
    [SerializeField] private GameObject _pooledPotionObject;
    [SerializeField] private Transform _objectPoolLocation;

    [Header("Sound Effects")]
    [SerializeField] private Audio _bubbleEffect;

    private Potion _potionObjectScript;

    private void Awake()
    {
        _potionObjectScript = _pooledPotionObject.GetComponent<Potion>();
        ResetCauldron();
    }

    private void ResetCauldron()
    {
        _ingredientsIndex = 0;
        _effectsIndex = 0;

        _currentIngredients = new GameObject[REQUIRED_INGREDIENTS];

        for (int i = 0; i < REQUIRED_INGREDIENTS; ++i)
        {
            _currentIngredients[i] = null;
        }

        _currentEffects = new IngredientEffect[REQUIRED_INGREDIENTS * 2];

        for (int i = 0; i < REQUIRED_INGREDIENTS * 2; ++i)
        {
            _currentEffects[i] = IngredientEffect.NONE;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        GameObject other_object = other.gameObject;


        if (other_object.layer == _ingredientLayerMask)
        {
            return;
        }

        if (other_object.TryGetComponent<PotionIngredient>(out PotionIngredient potionIngredient))
        {
            other_object.SetActive(false);
            other_object.transform.position = _objectPoolLocation.position;

            _currentIngredients[_ingredientsIndex] = other_object;
            _ingredientsIndex += 1;

            _currentEffects[_effectsIndex] = potionIngredient.primaryEffect;
            _effectsIndex += 1;
            _currentEffects[_effectsIndex] = potionIngredient.secondaryEffect;
            _effectsIndex += 1;

            if (_ingredientsIndex == REQUIRED_INGREDIENTS)
            {
                GeneratePotion();
                ResetCauldron();
                return;
            }
        }
    }

    private void GeneratePotion()
    {
        float duration = _basePotionDuration;
        float speed_effect = 1f;
        bool stun = false;
        bool confuse = false;
        bool invisibility = false;      

        for (int i = 0; i < REQUIRED_INGREDIENTS * 2; ++i)
        {
            switch (_currentEffects[i])
            {
                case IngredientEffect.MINOR_SPEED:
                    speed_effect += _minorSpeedBuff;
                    break;

                case IngredientEffect.MAJOR_SPEED:
                    speed_effect += _majorSpeedBuff;
                    break;

                case IngredientEffect.MINOR_SLOW:
                    speed_effect += _minorSlowDebuff;
                    break;

                case IngredientEffect.MAJOR_SLOW:
                    speed_effect += _majorSlowDebuff;
                    break;

                case IngredientEffect.STUN:
                    stun = true;
                    break;

                case IngredientEffect.CONFUSION:
                    confuse = true;
                    break;

                case IngredientEffect.INVISIBILITY:
                    invisibility = true;
                    break;

                case IngredientEffect.DURATION_DOWN:
                    duration += _durationDownDecrease;
                    break;

                case IngredientEffect.DURATION_UP:
                    duration += _durationUpIncrease;
                    break;

                default:
                    break;
            }
        }
        AudioManager.PlayScreenSpace(_bubbleEffect);

        _potionObjectScript.SetPotionEffects(speed_effect, duration, stun, confuse, invisibility);
    }
}