using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Level : MonoBehaviour {

	public GameObject ArtifactFolder, AreaFolder;
	public List<Artifact> ArtifactList;
    public List<Area>AreaList;
	public tk2dTileMap Tilemap;
	// Use this for initialization
	public void Reset () 
    {
        for( int i = 0; i < ArtifactList.Count; i++ ) Destroy( ArtifactList[ i ].gameObject );
        ArtifactList = new List<Artifact>();
        for( int i = 0; i < AreaList.Count; i++ ) Destroy( AreaList[ i ].gameObject );
        AreaList = new List<Area>();
	}	
}
