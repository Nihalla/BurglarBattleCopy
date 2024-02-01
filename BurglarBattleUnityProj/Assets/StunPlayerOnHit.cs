// Team Sisyphean - Beckham Bagley, Charlie Light, Joe Gollin, Louis Phillips, Ryan Sewell, Tom Roberts

using UnityEngine;
using PlayerControllers;

public class StunPlayerOnHit : MonoBehaviour
{
    [SerializeField] private bool _canStun = false;
    private bool _hitPerson;

    [SerializeField] private float _stunTime = 2;

    [SerializeField] private Audio _hitPlayerSound;
    [SerializeField] private Audio _hitWallSound;
    private float _hitTimer;
    private float _hitTimerLimit = 0.5f;
    private bool _canPlaySound;
    private ParticleSystem _system;
    [Tooltip("Rumble settings")]
    [SerializeField]
    private bool _rumbleEnabled = true;
    [SerializeField]

    [Range(0.0f, 1.0f)]
    [Tooltip("The strength of the rumble on left/right side of controller")]
    private float _rumbleLeft =0.5f, _rumbleRight =0.5f;
    [SerializeField][Range(0.0f,1.0f)]
    [Tooltip("The duration of the rumble")]
    private float _rumbleDuration = 0.5f;
    private void Awake()
    {
        if (GetComponentInChildren<ParticleSystem>())
        {
            _system = GetComponentInChildren<ParticleSystem>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // NOTE(Zack): removed boolean check from every if statement in the function, we only need to do it once at the
        // top if we're going to check it in every if statement
        if (!_canStun) return;
        _canStun = false;

        _hitPerson = false;

        // The below code only works for the player tool holder as this blocks the player capsule in some cases, the code below that works if it hits the player capsule directly
        // The alternative to this code is removing the extra collider from the player, but I wasn't sure on the use of the collider so this was not done. 
        if (other.transform.parent !=null)
        {
            if (other.transform.parent.parent != null)
            {
                if (other.transform.parent.parent.TryGetComponent(out FirstPersonController parentPlayer))
                {
                    StunPlayer(other.transform.parent.parent.GetComponent<Collider>(), parentPlayer);
                }
            }
        }

        if (other.gameObject.TryGetComponent(out FirstPersonController player))
        {
            StunPlayer(other, player);
        }

        // NOTE(Zack): we're now checking GuardBase so that every guard type will now be able to be stunned
        if (other.gameObject.TryGetComponent(out GuardBase guard))
        {
            guard.StunGuard(_stunTime);
            _hitPerson = true;
        }

        PlaySound(_hitPerson);
    }

    // Instead of copy-paste the below code to multiple places, use a function.
    // Keep your code D.R.Y. (Don't Repeat Yourself) - Norbert
    private void StunPlayer(Collider c, FirstPersonController fpc)
    {
        InputDevices.Devices[fpc.playerID].RumblePulse(_rumbleLeft, _rumbleRight, _rumbleDuration, this, false);
        c.GetComponent<Loot>().DropLoot(1);
        fpc.StunPlayerForTimer(_stunTime);
        _hitPerson = true;
    }

    private void PlayParticleSystem()
    {
        if (_system)
        {
            _system.Play();
        }
    }

    private void PlaySound(bool personHit)
    {
        PlayParticleSystem();

        if (personHit)
        {
            AudioManager.PlayScreenSpace(_hitPlayerSound);
        }
        else
        {
            AudioManager.PlayScreenSpace(_hitWallSound);
        }
    }

    /// <summary> 
    /// Sets the objects stun property based on the passed in parameter. 
    /// </summary> 
    /// <param name="value"></param> 
    public void SetToStun(bool value)
    {
        _canStun = value;
    }

    /// <summary>
    /// Sets the duration of the stun based on the passed in value.
    /// </summary>
    /// <param name="value"></param>
    public void SetStunTime(float value)
    {
        _stunTime = value;
    }
}
