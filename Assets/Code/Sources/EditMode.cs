using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

[ExecuteInEditMode]
public class EditMode : MonoBehaviour
{
#if UNITY_EDITOR
    void OnEnable()
    {
        EditorApplication.playmodeStateChanged += StateChange; 
    }
 
    void StateChange(){
     if (EditorApplication.isPlayingOrWillChangePlaymode && EditorApplication.isPlaying)
     {
         EditorApplication.ExecuteMenuItem( "Window/Update Lists" );
     }
 }
#endif

#if UNITY_EDITOR
    void Update()
    {
        if( UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode )
        {
            //this.enabled = false;
            //if(!Application.isPlaying )
            //    EditorApplication.ExecuteMenuItem( "Helper/Select Helper" );
        }
        else
        {
        }
    }

#endif
}