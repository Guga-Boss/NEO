using UnityEngine;
using System.Collections;
//using tk2dRuntime.TileMap;

public class MiniMap : MonoBehaviour {
    //public tk2dTileMap MMTilemap;
    //public tk2dSprite MMHero;

	// Use this for initialization
	public void Init ()
    {
        return;
    //    for ( int l = 0; l < 2;  l++)
    //    for ( int y = 0; y < Map.I.Tilemap.height; y++)
    //    for ( int x = 0; x < Map.I.Tilemap.width;  x++) 
    //    {
    //    int tile = Map.I.Tilemap.GetTile( x, y, l ); 
    //    if( tile != -1 ) MMTilemap.SetTile ( x, y, 0, tile ); 
    //    }

    //    MMTilemap.Build();
    //    gameObject.SetActive( false );

    //ChangeLayersRecursively( MMTilemap.Layers[0].gameObject, "UI");
	}

	public void UpdateIt() {
		return;
	
        //if( Input.GetKeyDown( KeyCode.Space ) )
        //    gameObject.SetActive(!gameObject.activeSelf );

        //Vector3 pos = MMTilemap.GetTilePosition( (int) Map.I.Hero.Pos.x, (int) Map.I.Hero.Pos.y );
        //pos.z = -5;
        //MMHero.transform.position = pos;
        //MMHero.transform.Rotate( 0, 0, 180.0f * Time.deltaTime);
	}

	public void ChangeLayersRecursively( GameObject inst, string name) {
		if(inst == null)
			return;
		
		foreach (Transform child in inst.transform) {
			child.gameObject.layer = LayerMask.NameToLayer(name);
			ChangeLayersRecursively(child.gameObject, name);
		}
	}
}
