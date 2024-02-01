// Author: Zack Collins

using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Mathematics;

/// <summary>
/// Static wrapper for common lerping coroutine functions, for use in C# scripts.
/// For example usage look at <see cref="IdolPedestal"/>.
/// </summary>
public static class Lerp 
{
    private const MethodImplOptions INLINE = MethodImplOptions.AggressiveInlining;

    // delegate prototypes
    public delegate IEnumerator VectorDel(Transform obj, float3 start, float3 end, float duration, LerpModifierDel modifierFunc = null);
    public delegate IEnumerator QuatDel(Transform obj, quaternion start, quaternion end, float duration, LerpModifierDel modifierFunc = null);
    public delegate float LerpModifierDel(float t);

    // NOTE(Zack): these are to be called in place of the functions below, as they're pre-allocated function pointers to the same functions
    public static VectorDel       ToPositionFunc;
    public static VectorDel       ToPositionLocalFunc;
    public static QuatDel         ToRotationFunc;
    public static QuatDel         ToRotationLocalFunc;
    public static LerpModifierDel DefaultLerpModifierFunc;

    static Lerp()
    {
        // NOTE(Zack): pre-allocating delegates to remove as many runtime allocations as possible
        ToPositionFunc          = ToPosition;
        ToPositionLocalFunc     = ToPositionLocal;
        ToRotationFunc          = ToRotation;
        ToRotationLocalFunc     = ToRotationLocal;
        DefaultLerpModifierFunc = DefaultLerpModifier;
    }

    [MethodImpl(INLINE)]
    private static float DefaultLerpModifier(float t) => t;

    [MethodImpl(INLINE)]
    private static IEnumerator ToPosition(Transform obj, float3 start, float3 end, float duration, LerpModifierDel modifierFunc = null)
    {
        // HACK(Zack): this is only because we cannot set this as the default value, as C# complains about it not being a compile time constant
        if (modifierFunc == null) modifierFunc = DefaultLerpModifierFunc;

        obj.position = start;

        float elapsed = float.Epsilon;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = modifierFunc(t); // NOTE(Zack): we're not doing a null check as we assign a value above if it is null
            obj.position = math.lerp(start, end, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.position = end;
        yield break;
    }

    [MethodImpl(INLINE)]
    private static IEnumerator ToPositionLocal(Transform obj, float3 start, float3 end, float duration, LerpModifierDel modifierFunc = null)
    {
        // HACK(Zack): this is only because we cannot set this as the default value, as C# complains about it not being a compile time constant
        if (modifierFunc == null) modifierFunc = DefaultLerpModifierFunc;

        obj.localPosition = start;

        float elapsed = float.Epsilon;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = modifierFunc(t); // NOTE(Zack): we're not doing a null check as we assign a value above if it is null

            obj.localPosition = math.lerp(start, end, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.localPosition = end;
        yield break;
    }

    [MethodImpl(INLINE)]
    private static IEnumerator ToRotation(Transform obj, quaternion start, quaternion end, float duration, LerpModifierDel modifierFunc = null)
    {
        // HACK(Zack): this is only because we cannot set this as the default value, as C# complains about it not being a compile time constant
        if (modifierFunc == null) modifierFunc = DefaultLerpModifierFunc;

        obj.rotation = start;

        float elapsed = float.Epsilon;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = modifierFunc(t); // NOTE(Zack): we're not doing a null check as we assign a value above if it is null

            obj.rotation = math.nlerp(start, end, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.rotation = end;
        yield break;
    }

    [MethodImpl(INLINE)]
    private static IEnumerator ToRotationLocal(Transform obj, quaternion start, quaternion end, float duration, LerpModifierDel modifierFunc = null)
    {
        // HACK(Zack): this is only because we cannot set this as the default value, as C# complains about it not being a compile time constant
        if (modifierFunc == null) modifierFunc = DefaultLerpModifierFunc;

        obj.localRotation = start;

        float elapsed = float.Epsilon;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = modifierFunc(t); // NOTE(Zack): we're not doing a null check as we assign a value above if it is null

            obj.localRotation = math.nlerp(start, end, t);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.localRotation = end;
        yield break;
    }
}
