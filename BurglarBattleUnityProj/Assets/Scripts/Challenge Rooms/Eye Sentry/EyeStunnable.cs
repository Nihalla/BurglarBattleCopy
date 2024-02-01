using System;
using UnityEngine;

public class EyeStunnable : MonoBehaviour
{
    [SerializeField] private float _minimumImpactForce;
    [SerializeField] private GameObject _stunObjectPrefab;

    private EyeSentry _eyeScript;
    private Collider _collider;
    private IThrowableObject _stunObjectScript;

    private void Awake()
    {
        _eyeScript = GetComponentInParent<EyeSentry>();
        _collider = GetComponent<Collider>();

        /*
        // REVIEW(Zack): if we're going to be doing a GetComponent on an GameObject set from the inspector like this,
        // we should prefer to just expose the component type in the inspector instead. This way we can guarantee that,
        // we have gotten a reference to the correct component.
        // e.g.
        // [SerializeField] private IThrowableObject _stunObjectScript;
        _stunObjectScript = _stunObjectPrefab.GetComponent<IThrowableObject>();

        // REVIEW(Zack): if we're following the suggestion from the above comment we should prefer to use;
        // Debug.Assert(_stunObjectScript != null, "Comment ...", this);
        // So that in release builds the check for it the component is compiled out, but during development
        // we give an error and complain that stuff isn't setup correctly.
        if (_stunObjectScript == null)
        {
            throw new Exception("Stun Object does not contain script of type IThrowableObject.");
        }
        */
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > _minimumImpactForce)
        {
            IThrowableObject thrown_object = collision.gameObject.GetComponent<IThrowableObject>();

            if (thrown_object != null && thrown_object.GetType() == _stunObjectScript.GetType())
            {
                thrown_object?.OnObjectHit(_collider);
                _eyeScript.Stun();
            }
        }
    }
}
