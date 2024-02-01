// Author: William Whitehouse (WSWhitehouse)

using System;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// This interface should be used on scripts that the player should interact with.
/// The interface requires a Collider to be present on the same GameObject as the
/// component, this is so the <see cref="PlayerInteraction"/> script can access the
/// interface. All of the Interaction functions pass through the Player Interaction
/// script that invoked the interaction.
/// </summary>
public interface IInteractable
{
    /// <summary> The max distance an object can be interacted with. </summary>
    public const float MAX_INTERACTABLE_DISTANCE = 20.0f;

    /// <summary> The default distance an object can be interacted with. </summary>
    public const float DEFAULT_INTERACTABLE_DISTANCE = 3.0f;

    /// <summary>
    /// This must returns the mesh renderers attached to the object that are apart
    /// of the interaction. The Interaction System will apply a glow effect to any
    /// components returned in the span.
    ///
    /// This function *MUST ALWAYS* return the same mesh renderers everytime it's
    /// called otherwise the PlayerInteraction script won't be able to manage the
    /// glow effect properly.
    ///
    /// Using a `Span` here to avoid allocating unnecessary memory. Use `.AsSpan()`
    /// on a list/array to be able to return one. Spans also allow returning a
    /// segment of a list/array if needed. 
    /// </summary>
    /// <returns>Span of Mesh Renderers to apply glow effect.</returns>
    public Span<MeshRenderer> GetInteractionMeshRenderers();

    /// <summary>
    /// An optional implementable function that allows an Interactable to communicate
    /// if it can be interacted with by the player or not. By default this always
    /// returns true - therefore always interactable.
    /// </summary>
    /// <returns>True when the player can interact; false otherwise.</returns>
    public bool CanInteract() { return true; }

    /// <summary>
    /// An optional implementable function that allows an interactable to set its
    /// interaction distance. When unimplemented this returns the default value
    /// (<see cref="DEFAULT_INTERACTABLE_DISTANCE"/>).
    /// </summary>
    /// <returns>The distance this object can be interacted with.</returns>
    public float GetInteractionDistance() { return DEFAULT_INTERACTABLE_DISTANCE; }

    /// <summary>
    /// Invoked when the player interacts with this object.
    /// </summary>
    public void OnInteract(PlayerInteraction playerInteraction) { }

    /// <summary>
    /// Invoked when the player holds the interact button on this object.
    /// </summary>
    public void OnInteractHoldStarted(PlayerInteraction playerInteraction) { }

    /// <summary>
    /// Invoked when the player stops holding the interact button.
    /// </summary>
    public void OnInteractHoldEnded(PlayerInteraction playerInteraction) { }

    /// <summary>
    /// Invoked when the player hovers over this object. This is NOT invoked when
    /// the player presses a button.
    /// </summary>
    public void OnInteractHoverStarted(PlayerInteraction playerInteraction) { }

    /// <summary>
    /// Invoked when the player stops hovering over this object. 
    /// </summary>
    public void OnInteractHoverEnded(PlayerInteraction playerInteraction) { }

}
