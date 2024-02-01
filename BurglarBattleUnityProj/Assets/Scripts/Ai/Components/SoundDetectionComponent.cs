using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SoundDetectionComponent : AiComponent
{
    SoundDetectionComponent() : base(AiComponentType.SOUND_DETECTION) { }

    private NavMeshAgent _agent;
    private CommunicationComponent _communication;

    [Header("the closer this to 0 is\nthe less sound is lost from the thickness of the walls")]
    public float _soundDampeningConst;

    [Header("How good the hearing of this enemy is\nExample: if the sound is 8 units big\n then if this is = 1 the guard will hear it from 8 units distance\nLower than 1 worst, higher than 1 better")]
    public float _hearingStrength = 1;


    private void Start() 
    { 
        _agent = GetComponent<NavMeshAgent>();
        _communication = GetComponent<CommunicationComponent>();
    }
    
    /// <summary>
    /// This sound uses the thicnkness of the walls to determin if it heard somthing
    /// </summary>
    /// <param name="soundOrigin">The location of the sound</param>
    /// <param name="soundDist">how loud the sound was</param>
    /// <param name="relay"></param>
    public void ThicknesBasedSoundDetection(Vector3 soundOrigin, float soundDist, bool relay) 
    {

        Dictionary<Collider, Vector3> collisions = new Dictionary<Collider, Vector3>();

        RaycastHit[] hitsToThePoint;
        hitsToThePoint = Physics.RaycastAll(this.transform.position, soundOrigin - this.transform.position, Vector3.Distance(this.transform.position, soundOrigin));

        if (hitsToThePoint.Length > 0)
        {
            foreach (var hit in hitsToThePoint)
            {
                collisions.Add(hit.collider, hit.point);
            }
        }

        RaycastHit[] hitsComingBack;
        hitsComingBack = Physics.RaycastAll(soundOrigin, this.transform.position - soundOrigin, Vector3.Distance(this.transform.position, soundOrigin));


        var distToSound = Vector3.Distance(soundOrigin, this.transform.position);
        var perc = distToSound / soundDist;
        var soundLossValue = perc;


        if (hitsComingBack.Length > 0)
        {
            foreach (var hit in hitsComingBack)
            {
                if (collisions.ContainsKey(hit.collider))
                {
                    soundLossValue += Vector3.Distance(collisions[hit.collider], hit.point) * _soundDampeningConst;
                }
            }
        }


        if (soundLossValue > _hearingStrength) 
        {
            //coulndt hear
            ////Debug.Log($"<color=red>the guard did not hear this due to sound loss being {soundLossValue} and the allowed hearing is {_hearingStrength}</color>");
           
        }
        else 
        {
            ////Debug.Log($"<color=green>the guard did hear as the sound loss is {soundLossValue} and the allowed hearing is {_hearingStrength}</color>");
            DistanceSoundDetection(soundOrigin, soundDist);
            // did hear the sound 
        }
    }






    /// <summary>
    /// This method uses the distance of the navmesh path created between the sound origin and the agent, This takes into account corners and stuff like that 
    /// </summary>
    /// <param name="soundOrigin">The location of the sound</param>
    /// <param name="soundDist">How loud the sound was</param>
    public void DistanceSoundDetection(Vector3 soundOrigin, float soundDist)
    {
        NavMeshPath soundPath = new NavMeshPath();

        _agent.CalculatePath(soundOrigin, soundPath);

        if (soundPath.status == NavMeshPathStatus.PathInvalid) 
        {
            //this is called if the path to the sound is not valid
            return;
        }

        float dist = 0;
        for (int i = 0; i < soundPath.corners.Length; i++)
        {
            if (i == soundPath.corners.Length - 1) { break; }
            dist += Vector3.Distance(soundPath.corners[i], soundPath.corners[i + 1]);
        }

        float soundLossVal = (dist / soundDist);
        
        if (soundLossVal > _hearingStrength)
        {
            //the sound was heard but the distance to the sound it self is just too great this is where the communication component comes in and relays to another guard who might have not heard the sound via the first function 
            //but its close enough
            ////Debug.Log($"<color=red>the guard did not hear this due to sound loss being {soundLossVal} and the allowed hearing is {_hearingStrength}, the soundDist is {soundDist}</color>");
        }
        else
        {
            ////Debug.Log($"<color=green>the guard did hear as the sound loss is {soundLossVal} and the allowed hearing is {_hearingStrength}, the soundDist is {soundDist}</color>");
            //did hear it and its going to that point
            _communication.ReceiveAlertLocation(soundOrigin);
            //if (transform.GetComponent<NavMeshMovementComponent>())
            //    transform.GetComponent<NavMeshMovementComponent>().SetNewDestination(_agent, soundOrigin);
        }
    }
}
