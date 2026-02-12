using UnityEngine;
using System; // Required for Environment.StackTrace

public class DeactivationDetector : MonoBehaviour
{
    void OnDisable()
    {
        // Only trigger if the object is being forced inactive in the hierarchy
        if( gameObject.activeInHierarchy == false )
        {
            Debug.LogError( "[FIRE FX DEBUG] Object " + gameObject.name +
                           " was FORCED DISABLED by an UNKNOWN CALLER." );

            // This captures the call history!
            Debug.LogError( "STACK TRACE:\n" + Environment.StackTrace );
        }
    }
}