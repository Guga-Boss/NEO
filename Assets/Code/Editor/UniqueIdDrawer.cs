using UnityEngine;
using UnityEditor;
using System;

[CustomEditor( typeof( UniqueId ) )]
public class UniqueIdInspector : Editor
{
    private UniqueId id;

    // assign UniqueId instance to this inspector
    void OnEnable()
    {
        id = ( UniqueId ) target;

        // generate new guid when you create a new game object
        if( id.gameObjectID == 0 ) id.gameObjectID = UnityEngine.Random.Range( int.MinValue, int.MaxValue );

        // generate new guid if guid already exists
        else
        {
            UniqueId[] objects = Array.ConvertAll( GameObject.FindObjectsOfType( typeof( UniqueId ) ), x => x as UniqueId );
            int idCount = 0;
            for( int i = 0; i < objects.Length; i++ )
            {
                if( id.gameObjectID == objects[ i ].gameObjectID )
                    idCount++;
            }
            if( idCount > 1 ) id.gameObjectID = UnityEngine.Random.Range( int.MinValue, int.MaxValue );
        }
    }
}