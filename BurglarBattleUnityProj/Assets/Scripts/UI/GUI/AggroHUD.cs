using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AggroHUD : MonoBehaviour
{
    [SerializeField] private List<Transform> _aggroList = new();
    [SerializeField] private EnemyIndicator _enemyIndicator;

    private const short NUMBER_OF_INDICATORS = 10;

    private Transform _playerTransform;
    private readonly List<EnemyIndicator> _enemyIndicators = new(NUMBER_OF_INDICATORS);

    private delegate IEnumerator IndicatorUpdate(short index);
    private IndicatorUpdate _indicatorUpdateFunc;
    private Coroutine[] _indicatorUpdateCoroutines = new Coroutine[NUMBER_OF_INDICATORS];

    private void Start()
    {
        _indicatorUpdateFunc = UpdateIndicator;
        _playerTransform = gameObject.transform.parent.transform;

        InitIndicators();
    }

    public void SetAggro(bool value, Transform guard)
    {
        if (value)
        {
            AddGuard(guard);
        }
        else
        {
            RemoveGuard(guard);
        }
    }

    private void AddGuard(Transform guard)
    {
        if (_aggroList.Contains(guard)) return;
        _aggroList.Add(guard);
        StartUpdateIndicator((short)(_aggroList.Count - 1));
    }

    private void RemoveGuard(Transform guard)
    {
        if (!_aggroList.Contains(guard)) return;
        _aggroList.Remove(guard);
        StopUpdateIndicator((short)(_aggroList.Count));
    }

    private void InitIndicators()
    {
        for (short i = 0; i < NUMBER_OF_INDICATORS; ++i)
        {
            _enemyIndicators.Add(Instantiate(_enemyIndicator,gameObject.transform));
            _enemyIndicators[i].ToggleIndicator(false);
        }
    }

    private IEnumerator UpdateIndicator(short index)
    {
        while (true)
        {
            yield return null;
            _enemyIndicators[index].SetRotation(GetSignedAngleToGuard(index));
        }
        yield break;
    }

    private void StartUpdateIndicator(short index)
    {
        _enemyIndicators[index].ToggleIndicator(true);
        _indicatorUpdateCoroutines[index] = StartCoroutine(_indicatorUpdateFunc(index));
    }

    private void StopUpdateIndicator(short index)
    {
        _enemyIndicators[index].ToggleIndicator(false);
        if (_indicatorUpdateCoroutines[index] != null)
        {
            StopCoroutine(_indicatorUpdateCoroutines[index]);
        }
        _indicatorUpdateCoroutines[index] = null;
    }

    private float GetSignedAngleToGuard(short index)
    {
        return Vector3.SignedAngle(_playerTransform.forward,
                                (_aggroList[index].transform.position - _playerTransform.position).normalized,
                                Vector3.up);
    }

    public void ResetHud()
    {
        for (short i = 0; i < _enemyIndicators.Count; ++i)
        {
            _enemyIndicators[i].SetRotation(0);
            _enemyIndicators[i].ToggleIndicator(false);

            StopUpdateIndicator(i);
        }
        
        _aggroList.Clear();
    }
}
