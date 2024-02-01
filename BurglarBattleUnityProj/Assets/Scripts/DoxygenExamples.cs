// NOTE(WSWhitehouse): Shouldn't be compiling this script outside the editor...
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is an example class showing how to write comments for the Doxygen documentation
/// generator! This is also the example class used in the Coding Style found on the wiki!
///
/// ALWAYS FOLLOW THE CODING STYLE AND DOCUMENTATION STYLE OUTLINED IN THE WIKI!
///  - https://github.com/GamesTech/CGD-22-23-BurglarBattle/wiki/Coding-Style
///  - https://github.com/GamesTech/CGD-22-23-BurglarBattle/wiki/Technical-Design-Document-(TDD)#inline-code-documentation
/// </summary>
public class DoxygenExamples : MonoBehaviour
{
    // inspector accessible variables
    [Header("Player Settings")]
    [SerializeField] private int _playerHealth;
    [SerializeField] private float _playerSpeed;
    [SerializeField] private LayerMask _hurtLayerMask;
    [Space] // this will create a space between [_hurtLayerMask] & [playerName] in the inspector
    
    
    // public variables
    public string playerName = "Player One";
    
    
    // private variables
    private int _livesLost;
    
    
    // properties
    
    /// <summary>
    /// How many lives this player has lost.
    /// (Probably should avoid placing doc comments on
    /// such trivial types, this is just an example)
    /// </summary>
    public int LivesLost => _livesLost;
  
    private int _HurtLayerMaskValue => _hurtLayerMask.value;


    // static variables
    
    /// <summary>
    /// The current number of players in this game session. Can't be less than 0,
    /// or greater than <see cref="MAX_PLAYER_COUNT"/>
    /// </summary>
    public static int s_currentPlayerCount = 0;

  
    // const variables
    
    /// <summary>
    /// Maximum number of players in the game.
    /// </summary>
    public const int MAX_PLAYER_COUNT = 4;

    
    // events
    
    /// <summary>
    /// Delegate used for game state events - <see cref="DoxygenExamples.gameFinishedEvent"/>
    /// </summary>
    public delegate void GameStateDel();
    
    /// <summary>
    /// Event that is invoked when the game is finished.
    /// </summary>
    public GameStateDel gameFinishedEvent;


    // function ptrs
    
    /// <summary>
    /// A delegate to define the Lerp Player coroutine function (<see cref="DoxygenExamples.LerpPlayer"/>)
    /// See <see cref="DoxygenExamples.LerpPlayerFunc"/> for the corresponding function pointer.
    /// </summary>
    public delegate IEnumerator LerpPlayerDel(float duration, Vector3 endPos);
    
    /// <summary>
    /// Function pointer for pre-allocating the function <see cref="LerpPlayer"/> in the 
    /// <see cref="Awake"/> function, see <see cref="LerpPlayerDel"/> for the delegate.
    /// </summary>
    public LerpPlayerDel LerpPlayerFunc;
    
    
    // Unity Event Functions
    private void Awake()
    {
        _livesLost = 0;
       
       // Assign func ptr
       LerpPlayerFunc = LerpPlayer;
    }
    
    private void Update()
    {
        MovePlayerDown();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // if the object that the player has collided with is not part of a layer that, 
        // is able to hurt the player we return from this function.
        if ((_HurtLayerMaskValue & (1 << other.gameObject.layer)) == 0) return;
        
        
        _playerHealth -= 1;
        _livesLost += 1;

        if (_playerHealth <= 0)
        {
            Vector3 endPosition = new Vector3(0f, 100f, 50f);
            StartCoroutine(LerpPlayerFunc(5.0f, endPosition));
            EndGame();
        }
    }
  
#region MOVEMENT_RELATED_FUNCTIONS
    /// <summary>
    /// Moves the player down by their player speed.
    /// </summary>
    private void MovePlayerDown()
    {
        Vector3 position = this.transform.position;
        position.y -= _playerSpeed * Time.deltaTime;
        this.transform.position = position;
    }

    /// <summary>
    /// Lerps the player from its current position to <paramref name="endPos"/> over
    /// <paramref name="duration"/> (in seconds).
    /// </summary>
    /// <param name="duration">Duration in seconds to get to <paramref name="endPos"/></param>
    /// <param name="endPos">The end position of the lerp.</param>
    /// <returns></returns>
    private IEnumerator LerpPlayer(float duration, Vector3 endPos)
    {
        float timeElapsed = 0.0f;
        Vector3 startValue = transform.position;

        while (timeElapsed < duration)
        {
          transform.position = Vector3.Lerp(startValue, endPos, timeElapsed / duration);
          timeElapsed += Time.deltaTime;
          yield return null;
        }
        
        transform.position = endPos;
    }
#endregion // MOVEMENT_RELATED_FUNCTIONS
     
    /// <summary>
    /// Calls the game finished events (<see cref="gameFinishedEvent"/>) and resets
    /// player values.
    /// </summary>
    private void EndGame()
    {
        _livesLost = 0;
        transform.position = Vector3.zero;
      
        // call an event delegate to signal the end of the game
        gameFinishedEvent?.Invoke();
    }
}
#endif