using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;



public class TKUtil : MonoBehaviour 
{
    public static void Settile( int x, int y, ETileType tile, bool warning = true )
    {
        if( x < 0 || x > Quest.I.CurLevel.Tilemap.width - 1 ||
            y < 0 || y > Quest.I.CurLevel.Tilemap.height - 1 )
        {
            if( warning )
                Debug.LogError( "Settile: out of bounds " + x + " " + y );
            else return;
        }
        int layer = ( int ) Map.GetTileLayer( tile );
        Quest.I.CurLevel.Tilemap.SetTile( x, y, layer, ( int ) tile );
    }

    public static void CreateBlankMap( ref tk2dTileMap tm )
    {
        for( int i = 0; i < Quest.I.Dungeon.AreaList.Count; i++ )
            Destroy( Quest.I.Dungeon.AreaList[ i ].gameObject );
        Quest.I.Dungeon.AreaList.Clear();

        for( int i = 0; i < Quest.I.Dungeon.ArtifactList.Count; i++ )
            Destroy( Quest.I.Dungeon.ArtifactList[ i ].gameObject );
        Quest.I.Dungeon.ArtifactList.Clear();

        for( int y = 0; y < tm.height; y++ )
            for( int x = 0; x < tm.width; x++ )
            {
                tm.SetTile( x, y, ( int ) ELayerType.TERRAIN, ( int ) ETileType.NONE );
                tm.SetTile( x, y, ( int ) ELayerType.GAIA, ( int ) ETileType.NONE );
                tm.SetTile( x, y, ( int ) ELayerType.GAIA2, ( int ) ETileType.NONE );
                tm.SetTile( x, y, ( int ) ELayerType.MONSTER, ( int ) ETileType.NONE );
                tm.SetTile( x, y, ( int ) ELayerType.MODIFIER, ( int ) ETileType.NONE );
                tm.SetTile( x, y, ( int ) ELayerType.DECOR, ( int ) ETileType.NONE );
                tm.SetTile( x, y, ( int ) ELayerType.AREAS, ( int ) ETileType.NONE );
                tm.SetTile( x, y, ( int ) ELayerType.DECOR2, ( int ) ETileType.NONE );
                tm.SetTile( x, y, ( int ) ELayerType.RAFT, ( int ) ETileType.NONE );
            }
    }

    public static void Save( string file, ref tk2dTileMap tm )
    {
        file = Manager.I.GetProfileFolder() + file;

        TF.ActivateFieldList( "Farm" );                                                      // Activates Farm Tagged Field List

        using( MemoryStream ms = new MemoryStream() )
        using( BinaryWriter writer = new BinaryWriter( ms ) )                                // Open Memory Stream
        {
            GS.W = writer;                                                                   // Assign BinaryWriter to GS.W for TF

            if( G.Farm.UniqueID == "" )                                                      // Sorts The player Unique ID in the first time
                Security.SortUniqueID();

            int Version = Security.SaveHeader( 1 );                                          // Save Header Defining Current Save Version

            List<Vector2> ga2posl = new List<Vector2>();                                     // init lists
            List<int> ga2typel = new List<int>();
            List<Vector2> mnposl = new List<Vector2>();
            List<int> mntypel = new List<int>();
            List<int> mnvaril = new List<int>();
            List<int> gaiaTileIds = new List<int>();                                         // Init a list to store all Gaia TileIDs

            TF.Save( "MapSize", new Vector2( tm.width, tm.height ) );                       // Save map size

            TF.Save( "TileListSize", G.Farm.Tl.Count );                                     // Save tile list size

            TF.Save( "FarmLimit", G.Farm.FarmLimit );                                       // Save Farm limit

            TF.Save( "WaterTiles", G.Farm.WaterTiles );                                     // Save Water tiles

            for( int tid = 0; tid < G.Farm.Tl.Count; tid++ )                                // Warning: saving only farm area         
            {
                int x = G.Farm.Tl[ tid ].x;
                int y = G.Farm.Tl[ tid ].y;
                {
                    int ga = -1;
                    if( Map.I.Gaia[ x, y ] )                                                 // If there is a Gaia
                        ga = ( int ) Map.I.Gaia[ x, y ].TileID;                              // Get its TileID
                    gaiaTileIds.Add( ga );                                                   // Add to the list

                    if( Map.I.Unit[ x, y ] )
                    {
                        mnposl.Add( new Vector2( x, y ) );                                   // Fill monster array
                        mntypel.Add( ( int ) Map.I.Unit[ x, y ].TileID );
                        mnvaril.Add( ( int ) Map.I.Unit[ x, y ].Variation );
                    }

                    if( Map.I.Gaia2[ x, y ] )
                    {
                        ga2posl.Add( new Vector2( x, y ) );                                  // Fill gaia2 array
                        ga2typel.Add( ( int ) Map.I.Gaia2[ x, y ].TileID );
                    }
                }
            }

            TF.Save( "GaiaTileIDs", gaiaTileIds );                                         // Save entire list at once

            TF.Save( "MonsterType", mntypel );                                              // Save monster Type

            TF.Save( "MonsterPosition", mnposl );                                           // Save monster position

            TF.Save( "MonsterVariation", mnvaril );                                         // Save monster Variation

            TF.Save( "Gaia2Type", ga2typel );                                               // Save Gaia2 type

            TF.Save( "Gaia2Position", ga2posl );                                            // Save Gaia2 position                        

            GS.W.Flush();                                                                   // Flush the writer

            Security.FinalizeSave( ms, file );                                              // Finalize save
        }                                                                                   // using closes the stream automatically
    }
    
    public static bool Load( string file, ref tk2dTileMap tm )
    {
        file = Manager.I.GetProfileFolder() + file;
        if( ES2.Exists( file ) == false ) return false;

        Farm.RevealedTileList = new List<Vector2>();
        Map.I.Farm.ChooserMonsterList = new List<Vector2>();
        Map.I.Farm.PushMonsterList = new List<Vector2>();
        Map.I.Farm.KillerMonsterList = new List<Vector2>();
        Map.I.Farm.KickableMonsterList = new List<Vector2>();
        Map.I.Farm.SwapMonsterList = new List<Vector2>();
        Map.I.Farm.BlockerMonsterList = new List<Vector2>();
        Map.I.Farm.GrabMonsterList = new List<Vector2>();
        Map.I.Farm.HoneyCombList = new List<Vector2>();

        TF.ActivateFieldList( "Farm" );                                                      // Activates Farm Tagged Field List

        byte[] fileData = File.ReadAllBytes( file );                                         // Read full file
        byte[] content = Security.CheckLoad( fileData );                                     // Validate HMAC and get clean content

        using( GS.R = new BinaryReader( new MemoryStream( content ) ) )                      // Use MemoryStream for TF
        {
            int SaveVersion = Security.LoadHeader();                                         // Load Header

            G.Farm.FarmSize = 0;

            Vector2 size = TF.Load<Vector2>( "MapSize" );                                    // Load map size via Tagged

            int tlsz = TF.Load<int>( "TileListSize" );                                       // Load tile list size via Tagged

            Vector2 fl = TF.Load<Vector2>( "FarmLimit" );                                    // Load Farm limit via Tagged

            G.Farm.WaterTiles = TF.Load<int>( "WaterTiles" );                                // Load water tiles via Tagged

            List<int> gaiaTileIds = TF.Load<List<int>>( "GaiaTileIDs" );                     // Load Gaia TileIDs
            List<int> mntypel = TF.Load<List<int>>( "MonsterType" );                         // Load Monster Types
            List<Vector2> mnposl = TF.Load<List<Vector2>>( "MonsterPosition" );              // Load Monster Positions
            List<int> mnvaril = TF.Load<List<int>>( "MonsterVariation" );                    // Load Monster Variations
            List<int> ga2typel = TF.Load<List<int>>( "Gaia2Type" );                          // Load Gaia2 Types
            List<Vector2> ga2posl = TF.Load<List<Vector2>>( "Gaia2Position" );               // Load Gaia2 Positions            

            for( int tid = 0; tid < G.Farm.Tl.Count; tid++ )                                 // Warning: loading only farm area
            {
                int x = G.Farm.Tl[ tid ].x;
                int y = G.Farm.Tl[ tid ].y;

                int tile = gaiaTileIds[ tid ];                                                // Load Gaia TileID from list
                tm.SetTile( x, y, ( int ) ELayerType.GAIA, tile );

                if( tile == ( int ) ETileType.FOREST )                                        // Add decoration to farm
                {
                    tm.SetTile( x, y, ( int ) ELayerType.DECOR2, 2569 );
                }
                else 
                if( tile != ( int ) ETileType.WATER )                                         // Farm size calculation
                {
                    G.Farm.FarmSize++;
                    tm.SetTile( x, y, ( int ) ELayerType.DECOR2, -1 );                        // clear decor tile after chopped
                }
                tm.SetTile( x, y, ( int ) ELayerType.GAIA2, -1 );                             // Reset GAIA2
                tm.SetTile( x, y, ( int ) ELayerType.MONSTER, -1 );                           // Reset MONSTER
            }

            for( int i = 0; i < mntypel.Count; i++ )                                          // Load Monsters
            {
                Vector2 p = mnposl[ i ];
                int type = mntypel[ i ];
                int vari = mnvaril[ i ];

                InitFarmMonsters( p, type, vari );                                            // Initializes Farm monsters
            }

            for( int i = 0; i < ga2typel.Count; i++ )                                         // Load Gaia2
            {
                Vector2 p = ga2posl[ i ];
                int type = ga2typel[ i ];
                tm.SetTile( ( int ) p.x, ( int ) p.y, ( int ) ELayerType.GAIA2, type );       // Set Gaia2 tile
            }

            tm.Build();                                                                       // Build tilemap
        }
        return true;                                                                          // using closes the stream automatically
    }

    public static void InitFarmMonsters( Vector2 tg, int tile, int variation )
    {
        if( tile != ( int ) ETileType.PLAGUE_MONSTER ) return;

        if( variation == ( int ) ItemType.Plague_Monster_Cloner )                 // Blocking Monster
        {
            Map.I.Farm.ChooserMonsterList.Add( tg );
        }
        if( variation == ( int ) ItemType.Plague_Monster_Spawner )                // Pushable Monster 
        {
            Map.I.Farm.PushMonsterList.Add( tg );
        }
        if( variation == ( int ) ItemType.Plague_Monster_Slayer )                 // Killer Monster 
        {
            Map.I.Farm.KillerMonsterList.Add( tg );
        }
        if( variation == ( int ) ItemType.Plague_Monster_Kickable )               // Kickable Monster 
        {
            Map.I.Farm.KickableMonsterList.Add( tg );
        }
        if( variation == ( int ) ItemType.Plague_Monster_Swap )                   // Swap Monster 
        {
            Map.I.Farm.SwapMonsterList.Add( tg );
        }
        if( variation == ( int ) ItemType.Plague_Monster_Blocker )                // Blocker Monster 
        {
            Map.I.Farm.BlockerMonsterList.Add( tg );
        }
        if( variation == ( int ) ItemType.Plague_Monster_Grabber )                // Grab Monster 
        {
            Map.I.Farm.GrabMonsterList.Add( tg );
        }
        if( variation == ( int ) ItemType.HoneyComb )                             // HoneyComb
        {
            Map.I.Farm.HoneyCombList.Add( tg );
        }
    }
    public static bool LoadFogOfWar( string file, ref tk2dTileMap tm )
    {
        return false; // Farm Fog of war removed 
        //file = Manager.I.GetProfileFolder() + file + ".NEO";
        //if( ES2.Exists( file ) == false ) return false;

        //List<Vector2> rev = new List<Vector2>();                                                        // revealed tiles
        //rev = ES2.LoadList<Vector2>( file + "?tag=Revealed " );
        for( int i = 0; i < Farm.RevealedTileList.Count; i++ )
        {
            Vector2 p = Farm.RevealedTileList[ i ];
            Map.I.Revealed[ ( int ) p.x, ( int ) p.y ] = true;
        }

        //Map.I.ResetFogOfWar();
        Farm.RevealedTileList = new List<Vector2>();
        return true;
    }

    public static void PrepareSoil( Vector2 tg )
    {
        SetTile( tg, ETileType.MUD );
    }
    public static void ClearForest( Vector2 tg )
    {
        ClearLayer( tg, ELayerType.GAIA );
    }

    public static void DigRiver( Vector2 tg )
    {
        SetTile( tg, ETileType.WATER );
    }

    public static void BuildRoad( Vector2 tg )
    {
        SetTile( tg, ETileType.ROAD );
    }
    
    public static void SetTile( Vector2 tg, ETileType tile )
    {
        ELayerType layer = Map.GetTileLayer( tile );
        Quest.I.CurLevel.Tilemap.SetTile( ( int ) tg.x, ( int ) tg.y, ( int ) layer, ( int ) tile );
        Map.I.SetTile( ( int ) tg.x, ( int ) tg.y, layer, tile, true );

        for( int y = ( int ) tg.y - 1; y <= tg.y + 1; y++ )
        for( int x = ( int ) tg.x - 1; x <= tg.x + 1; x++ )
             Map.I.TransTilemapUpdateList.Add( new VI( x, y ) );
        Map.I.UpdateTilemap = true;
    }

    public static void ClearLayer( Vector2 tg, ELayerType layer )
    {
        Map.I.ClearTransTilemap( tg );

        Map.I.SetTile( ( int ) tg.x, ( int ) tg.y, layer, ETileType.NONE, true, true );
        Map.I.Tilemap.SetTile( ( int ) tg.x, ( int ) tg.y, ( int ) layer, -1 );

        int rad = 1; 
        for( int y = ( int ) ( tg.y - rad ); y <= tg.y + rad; y++ )
        for( int x = ( int ) ( tg.x - rad ); x <= tg.x + rad; x++ )
        if( Map.PtOnMap( Map.I.Tilemap, new Vector2( x, y ) ) )
        {
            if( Map.I.TransTilemapUpdateList.Contains( new VI( x, y ) ) == false )
                Map.I.TransTilemapUpdateList.Add( new VI( x, y ) );
            }
        int ll = Map.GetTransLayer( ETileType.SAND );
        Map.I.SetTransTile( ( int ) tg.x, ( int ) tg.y, ( int ) ll, -1 );

        if( layer == ELayerType.GAIA  ) Map.I.Gaia [ ( int ) tg.x, ( int ) tg.y ] = null;
        if( layer == ELayerType.GAIA2 ) Map.I.Gaia2[ ( int ) tg.x, ( int ) tg.y ] = null;
    }

    public static int CountTiles( ELayerType layer, ETileType tile )
    {
        int count = 0;
        for( int y = 0; y < Map.I.Tilemap.height; y++ )
        for( int x = 0; x < Map.I.Tilemap.width; x++ )
            {
                if( layer == ELayerType.GAIA  && Map.I.Gaia [ x, y ] == null && tile == ETileType.NONE ) count++;
                if( layer == ELayerType.GAIA2 && Map.I.Gaia2[ x, y ] == null && tile == ETileType.NONE ) count++;

                if( layer == ELayerType.GAIA    && Map.I.Gaia [ x, y ] && Map.I.Gaia [ x, y ].TileID == tile ) count++;
                if( layer == ELayerType.GAIA2   && Map.I.Gaia2[ x, y ] && Map.I.Gaia2[ x, y ].TileID == tile ) count++;
                if( layer == ELayerType.MONSTER && Map.I.Unit [ x, y ] && Map.I.Unit [ x, y ].TileID == tile ) count++;
            }
        return count;
    }

    public static void CreateObjAtRandomPosition( Sector s, ETileType tile, int amount )
    {
        ELayerType layer = Map.GetTileLayer( tile );

            for( int i = 0; i < amount; i++ )
            {
                List<Vector2> tgl = new List<Vector2>();
                for( int yy = ( int ) s.Area.yMin - 1; yy < s.Area.yMax + 1; yy++ )
                for( int xx = ( int ) s.Area.xMin - 1; xx < s.Area.xMax + 1; xx++ )
                if ( Map.I.GetPosArea( new Vector2( xx, yy ) ) != -1 )
                if ( RandomMap.CanCreateUnit( new Vector2( xx, yy ), layer, tile ) )
                    tgl.Add( new Vector2( xx, yy ) );

                if( tgl.Count > 0 )
                {
                    int sr = Random.Range( 0, tgl.Count );
                    Vector2 p = new Vector2( ( int ) tgl[ sr ].x, ( int ) tgl[ sr ].y );
                    CreateObj( p, layer, tile, true );
                }
            }
    }


    public static List<Unit> CreateObjAtRandomPosition( Area ar, ETileType tile, int amount, bool settile )
    {
        ELayerType layer = Map.GetTileLayer( tile );

        List<Unit> obj = new List<Unit>();
        for( int i = 0; i < amount; i++ )
        {
            List<Vector2> tgl = new List<Vector2>();
            for( int yy = ( int ) ar.P2.y; yy <= ar.P1.y; yy++ )
            for( int xx = ( int ) ar.P1.x; xx <= ar.P2.x; xx++ )
            if ( Map.I.GetPosArea( new Vector2( xx, yy ) ) == ar.GlobalID )
            if ( RandomMap.CanCreateUnit( new Vector2( xx, yy ), layer, tile ) )
                 tgl.Add( new Vector2( xx, yy ) );

            if( tgl.Count > 0 )
            {
                int sr = Random.Range( 0, tgl.Count );
                Vector2 p = new Vector2( ( int ) tgl[ sr ].x, ( int ) tgl[ sr ].y );
                Unit un = CreateObj( p, layer, tile, settile );
                obj.Add( un );
            }
        }
        return obj;
    }

    public static Unit CreateObj( Vector2 pos, ELayerType layer, ETileType tile, bool settile )
    {
        if( settile )
            Quest.I.CurLevel.Tilemap.SetTile( ( int ) pos.x, ( int ) pos.y, ( int ) layer, ( int ) tile );

        Map.I.UpdateTileGameObjectCreation( ( int ) pos.x, ( int ) pos.y, layer, tile );
      
        if( tile == ETileType.ORIENTATION || tile == ETileType.ARROW )
        {
            Map.I.Gaia2[ ( int ) pos.x, ( int ) pos.y ].RotateTo( ( EDirection ) Random.Range( 0, 8 ) );
        }

        return Map.I.Gaia2[ ( int ) pos.x, ( int ) pos.y ];
    }    
}
