// Based on the original version (by Dheeraj Karki), adapted to work with the AI system and maintained by Norbert Kupeczki - 19040948

using System.Collections.Generic;
using UnityEngine;

public class CommunicationComponent : AiComponent
{
    public enum MessageType
    {
        NOTHING = 0,
        ALERT = 1,
        STAND_DOWN = 2
    }

    CommunicationComponent() : base(AiComponentType.COMMUNICATION) { }

    [SerializeField] private Vector3 _alertLocation;
    [SerializeField] private MessageType _messageReceived = MessageType.NOTHING;
    [SerializeField] private float _broadcastRadius = 20.0f;
    [SerializeField] private LayerMask _targetLayerMask;
    [SerializeField] private LayerMask _envriomentLayerMask;

    public GuardLevel _communicationLevel { get; private set; }

    private bool GuardsInRange(out List<CommunicationComponent> targets)
    {
        Collider[] guards = Physics.OverlapSphere(transform.position, _broadcastRadius, _targetLayerMask.value);

        if (guards.Length != 0)
        {
            targets = new List<CommunicationComponent>(guards.Length);

            for (int i = 0; i < guards.Length; ++i)
            {
                if (guards[i].gameObject.TryGetComponent(out CommunicationComponent commComp))
                {
                    if (commComp != this && (_communicationLevel == commComp._communicationLevel))
                    {
                        targets.Add(commComp);
                    }
                }               
            }
            return true;
        }

        targets = new List<CommunicationComponent>(0);
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, _broadcastRadius);
    }
#endif

    /// <summary>
    /// Sends a message with type and location data
    /// </summary>
    /// <param name="_messageType"></param>
    /// <param name="_alertLocation"></param>
    /// <returns>True if the reciving AI is valid and updated with new target, False if reciving AI is not valid and wasn't sent an update</returns>

    public bool SendCommunication(MessageType _messageType, Vector3 _alertLocation)
    {
        List<CommunicationComponent> _receivers;
        GuardsInRange(out _receivers);

        if (_receivers != null)
        {
            foreach (CommunicationComponent guard in _receivers)
            {
                if(_messageType == MessageType.ALERT)
                {
                    guard.ReceiveAlertLocation(_alertLocation);
                }
                else if(_messageType == MessageType.STAND_DOWN)
                {
                    guard.StandDown();
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Component stores the new alert location, also changes the stored message type to ALERT.
    /// </summary>
    /// <param name="_newAlertLocation"></param>
    public void ReceiveAlertLocation(Vector3 _newAlertLocation)
    {
        _alertLocation = _newAlertLocation;
        _messageReceived = MessageType.ALERT;
    }

    /// <summary>
    /// Makes the guard stand down
    /// </summary>
    public void StandDown()
    {
        _alertLocation = Vector3.zero;
        _messageReceived = MessageType.STAND_DOWN;
    }

    /// <summary>
    /// Checks for new message in the communication component
    /// </summary>
    /// <returns>Returns true if there is new alert location message received, false if not</returns>
    public MessageType GetMessage()
    {
        return _messageReceived;
    }

    /// <summary>
    /// Gets the stored alert location
    /// </summary>
    /// <returns>Returns the alert location as a Vector3 type data</returns>
    public Vector3 GetAlertLocation()
    {
        return _alertLocation;
    }

    /// <summary>
    /// Clears the received message log and alert data
    /// </summary>
    public void ClearMessage()
    {
        _messageReceived = MessageType.NOTHING;
        _alertLocation = Vector3.zero;
    }

    /// <summary>
    /// Sets the level on which the guard can communicate with other guards.
    /// </summary>
    /// <param name="level"></param>
    public void SetCommunicationLevel(GuardLevel level)
    {
        _communicationLevel = level;
    }

}