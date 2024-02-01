using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

public class AudioTest : MonoBehaviour
{
    public Audio _audio = null;
    public int playerID = 0;
    
    public AudioSource3D audioSource;
}

#if UNITY_EDITOR
[CustomEditor(typeof(AudioTest))]
public class AudioTestEditor : Editor
{
    private AudioTest Target => (AudioTest)target;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        if (Target._audio == null)  return;
        
        EditorGUILayout.Space();
        
        if (!Application.isPlaying)
        {
            EditorGUILayout.LabelField("Press play to test audio...");
            return;
        }
        
        if (GUILayout.Button("Play World Space"))
        {
            AudioManager.PlayOneShotWorldSpace(Target._audio, Target.transform.position);
        }
        
        if (GUILayout.Button("Play Player Space"))
        {
            AudioManager.PlayPlayerSpace(Target._audio, Target.playerID);
        }
        
        if (GUILayout.Button("Play Screen Space"))
        {
            AudioManager.PlayScreenSpace(Target._audio);
        }
        
        if (GUILayout.Button("Play Audio Source"))
        {
            Target.audioSource.Play();
        }
    }
}
#endif // UNITY_EDITOR