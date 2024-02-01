// Author: Zack Collins

using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;

public static class Profile
{
    [Conditional("UNITY_EDITOR")]
    public static void Start(string name)
    {
        Profiler.BeginSample(name);
    }

    [Conditional("UNITY_EDITOR")]
    public static void End()
    {
        Profiler.EndSample();
    }
}

public struct ScopedProfile : IDisposable
{
    public ScopedProfile(string name)
    {
#if UNITY_EDITOR
        Profile.Start(name);
#endif
    }

    public void Dispose()
    {
#if UNITY_EDITOR
        Profile.End();
#endif
    }
}

