using UnityEngine;

public enum AiComponentType
{
    NO_TYPE = -1,
    EXAMPLE,
    DETECTION,
    PATROL,
    MOVEMENT,
    SOUND_DETECTION,
    COMMUNICATION,
    CATCH,
    SEARCH
}
/// <summary>
/// Base component of the Ai system
/// </summary>
public abstract class AiComponent : MonoBehaviour
{
    public AiComponent(AiComponentType type) { Type = type; }
    public AiComponentType Type { get; protected set; }
}

//NOTE(Felix): Here is an example of how to set the type of a component, have to do it this way as fields cannot be abstract
public class ExampleComp : AiComponent
{
    public ExampleComp() : base(AiComponentType.EXAMPLE) { }
}
