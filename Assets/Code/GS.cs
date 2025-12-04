using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Sirenix.OdinInspector;
using DarkTonic.MasterAudio;
using PathologicalGames;
using System.Runtime.InteropServices;

public class GS : MonoBehaviour
{
    public static GS I;
    public static BinaryWriter W;
    public static BinaryReader R;
    public static List<Unit> ga2l = new List<Unit>();
    public static bool IsLoading, IsSaving;
    public static bool CubeLoaded;
    public static Unit LastSavedCube, LastLoadedCube; 
    public static List<Unit> SaveStepUnitList = new List<Unit>();
    public static List<Vector2> SaveStepList = new List<Vector2>();
    public static bool RestartAvailable;
    public static bool ForceLoad;
    public static bool CustomSaveExists;
    public static int  LoadFrameCount = 0;   // Frame count since last load
    public static float CurrentButcherCoins = 0;
    public void Start()
    {
        I = this;
    }
    public static void UpdateCubeSaving()
    {
        LoadFrameCount++;
        if( Input.GetKeyUp( KeyCode.R ) || ForceLoad )                      // Load cube by pressing R
        if( GS.RestartAvailable == false )
        if( CustomSaveExists == false )
        if( SaveStepList.Count > 0 )
        {
            GS.I.LoadCube( LastSavedCube );                                 // Load it
            GS.RestartAvailable = true;
        }
        if( Manager.I.GugaVersion )
        {
            if( Input.GetKeyDown( KeyCode.F10 ) )                                      
            {
                GS.I.SaveCube();                                            // My debug Save
                CustomSaveExists = true;
                LastSavedCube = null;
            }
            if( Input.GetKeyDown( KeyCode.F11 ) || ForceLoad ||
              ( Input.GetKeyUp( KeyCode.R ) && LastSavedCube == null ) )
            {
                string file = Manager.I.GetProfileFolder() +
                "Cube Save/Statistics My.NEO";
                if( File.Exists( file ) )
                    GS.I.LoadCube();                                        // My debug Load
            }
        }

        UpdateActiveSaveSwitching();                                        // Changes active saved file according to mouse click
        
        if( Map.I.AdvanceTurn == false ) return;

        Unit save = Map.I.GetUnit( ETileType.SAVEGAME, G.Hero.Pos );

        if( save )
        if( SaveStepList.Contains( save.Pos ) == false )                    // Cannot save more than once
        //if( Map.I.RM.HeroSector.Cleared == false )
        if( G.Hero.Control.PathFinding.Path == null   || 
            G.Hero.Control.PathFinding.Path.Count <= 0 )
        {
            save.Body.EffectList[ 0 ].gameObject.SetActive( true );                         // activates effects
            save.Body.EffectList[ 1 ].gameObject.SetActive( true );
            save.Body.EffectList[ 2 ].gameObject.SetActive( true );  
            UI.I.EnableOverlay( new Color( 0, 0, 1, .75f ), 1.5f );                         // Activates Overlay effect
            string nm = " " + ( int ) G.Hero.Pos.x + " " + ( int ) G.Hero.Pos.y;
            GS.I.SaveCube( save );
            LastSavedCube = save;
            CustomSaveExists = false;
        }
    }
    public static void UpdateActiveSaveSwitching()
    {
        if( Input.GetMouseButtonUp( 0 ) == false ) return;
        Unit save = Map.I.GetUnit( ETileType.SAVEGAME, new Vector2( Map.I.Mtx, Map.I.Mty ) );

        if( save )
        if( SaveStepUnitList.Contains( save ) == true )                                    // Switches current activated save spot
        {
            LastSavedCube = save;
            MasterAudio.PlaySound3DAtVector3( "Percussion", save.Pos );
            GS.I.LoadCube( LastSavedCube );
        }
    }
    public void LoadCube( Unit save = null )
    {
        //if( Map.I.RM.HeroSector.Cleared ) return;
        if( Map.I.FishingMode != EFishingPhase.NO_FISHING ) return;

        InitLoading();                                                                     // Init loading

        string nm = " My";
        if( save ) nm = " " + save.Pos.x + " " + save.Pos.y;

        Manager.I.Inventory.Load( nm );                                                    // Load Inventory

        Statistics.Load( nm );                                                             // Load Statistics

        Sector.Load( nm );                                                                 // Load Sector

        string file = Manager.I.GetProfileFolder() + "Cube Save/Cube" + nm + ".NEO";       // Provides filename
        Sector s = Map.I.RM.HeroSector;   

        using( R = new BinaryReader( File.Open( file, FileMode.Open ) ) )
        {
            IsLoading = true;
            int SaveVersion = GS.R.ReadInt32();                                            // Load Version      

            string unique = GS.R.ReadString();                                             // Load Unique Player ID     TODO: check error     

            Map.I.Load();                                                                  // Load Map

            for( int y = ( int ) s.Area.yMin - 1; y < s.Area.yMax + 1; y++ )
            for( int x = ( int ) s.Area.xMin - 1; x < s.Area.xMax + 1; x++ )
            {
                int tile = GS.R.ReadInt32();                                               // Load Gaia
                if( SalvableGaia( tile ) )                   
                    LoadSalvableGaia( y, x, tile );

                if( Map.I.Unit[ x, y ] )                                                   // Clean monsters
                    Map.I.Unit[ x, y ].Kill();

                if( Map.I.Gaia2[ x, y ] )                                                  // Clean gaia 2
                    Map.I.Gaia2[ x, y ].Kill();
            }

            for( int i = G.HS.Fly.Count - 1; i >= 0; i-- )                                 // Clean Flying units
            {
                G.HS.Fly[ i ].Kill();
            }

            Unit.LoadHeader();                                           // Load Hero header
            G.Hero.Load();                                               // Load Hero main

            int sz = GS.R.ReadInt32();                                   // Load Monsters Array size
            s.MoveOrder = new List<Unit>();

            for( int i = 0; i < sz; i++ )                                // Load Monsters header
                Unit.LoadHeader();

            for( int i = 0; i < s.MoveOrder.Count; i++ )                 // Load Monsters main
                s.MoveOrder[ i ].Load();

            sz = GS.R.ReadInt32();                                       // Load Flying Units Array size
            Map.I.RM.HeroSector.Fly = new List<Unit>();

            for( int i = 0; i < sz; i++ )                                // Load Flying Units header
                Unit.LoadHeader( true );

            for( int i = 0; i < G.HS.Fly.Count; i++ )                    // Load Flying Units main
                G.HS.Fly[ i ].Load();

            sz = GS.R.ReadInt32();                                       // Load Gaia 2 Array size
            ga2l = new List<Unit>();

            for( int i = 0; i < sz; i++ )                                // Load Gaia 2 header
                Unit.LoadHeader();

            for( int i = 0; i < ga2l.Count; i++ )                        // Load Gaia 2 main
                ga2l[ i ].Load();

            Artifact.Load( nm );                                         // Load Artifact

            Secret.LoadTemp( nm );                                       // Load temp Secret

            Secret.InitializeSecrets( true );                            // Initialize Secrets

            CubeLoaded = true;
            LastLoadedCube = save;
            LoadFrameCount = 0;   
            GS.ForceLoad = false;
            ResourceIndicator.UpdateGrid = true;
            string add = "\nNext Step will Charge Indicated Resource...";
            if( save && save.Body.StackAmount <= 0 ) 
                add = "\nNext Step will Unpause it.";
            UI.I.SetBigMessage( "Game Loaded!" + add, 
            Color.yellow, 99f, 4.7f, 135.8f, 65 );                              // Bigtext message

            UI.I.UpdateInfoPanel( true );                                       // Update Perks panel

            R.Close();

            if( s.GoalConquered )
                RandomMapGoal.LoadAll( nm );                                    // Load Goals Data: only loads goals if a BP or recipe has been given as prize. otherwise its not needed. goals loading is slow 

            UpdateReferencesAfterLoad();                                        // Update References After Load

            MasterAudio.PlaySound3DAtVector3( "Click 2", G.Hero.Pos );          // Sound FX
            IsLoading = false;

            Map.I.UpdateTilemap = true;
            //Map.I.UpdateTransLayerTilemap( true );
        }
    }

    private static void LoadSalvableGaia( int y, int x, int tile )
    {
        Map.I.Gaia[ x, y ].Kill();
        Unit prefabUnit = Map.I.GetUnitPrefab( ( ETileType ) tile );           // Create Trap objects
        Map.I.CreateUnit( prefabUnit, new Vector2( x, y ), ELayerType.GAIA );
        Map.I.AddTrans( new VI( x, y ) );                                      // add trans tile to be updated                                                              
        Map.I.ClearTransTile( x, y, 0 );                                       // clear back trans tiles
        Map.I.ClearTransTile( x, y, 1 );
        Map.I.ClearTransTile( x, y, 2 );

        if( Map.I.Gaia[ x, y ].TileID == ETileType.ROOMDOOR ||                 // enables colliders
            Map.I.Gaia[ x, y ].TileID == ETileType.CLOSEDDOOR )
            Map.I.Gaia[ x, y ].Collider.enabled = true;
        else
            if( Map.I.Gaia[ x, y ].TileID == ETileType.OPENDOOR ||             // disable colliders
                Map.I.Gaia[ x, y ].TileID == ETileType.OPENROOMDOOR )
                Map.I.Gaia[ x, y ].Collider.enabled = false;
    }
    private void InitLoading()
    {
        CurrentButcherCoins = Item.GetNum( ItemType.Res_Butcher_Coin );
        Map.I.RM.ModedUnitlList = new List<Unit>();
        UI.I.EnableOverlay( new Color( 0, 0, 1, 1f ), 1.5f );                              // Activates Overlay effect
        Map.I.RestPos = new List<Vector2>();
        Map.I.RestID = new List<int>();
        if( Map.I.Lights.lights.Count > 1 )
            Map.I.Lights.lights.RemoveRange( 1, Map.I.Lights.lights.Count - 1 );           // Remove Lights

        RemoveFixedSpells();                                                               // Remove and despawn hero fixed spells
    }
    public static void RemoveFixedSpells()
    {
        for( int i = G.Hero.Body.Sp.Count - 1; i >= 0; i-- )
        {
            if( i < 8 ) return;
            Spell s = G.Hero.Body.Sp[ i ];
            Item.AddItem( s.ItemType, -1 );
            if( PoolManager.Pools[ "Pool" ].IsSpawned( s.transform ) )
            {
                s.transform.parent = PoolManager.Pools[ "Pool" ].gameObject.transform;
                PoolManager.Pools[ "Pool" ].Despawn( s.transform );
            }
            G.Hero.Body.Sp.RemoveAt( i );
            G.Hero.Attacks.RemoveAt( i );
        }
    }

    public bool SalvableGaia( int tile )
    {
        if( tile == ( int ) ETileType.TRAP ||
            tile == ( int ) ETileType.FREETRAP ||
            tile == ( int ) ETileType.PIT ||
            tile == ( int ) ETileType.OPENDOOR ||
            tile == ( int ) ETileType.CLOSEDDOOR ||
            tile == ( int ) ETileType.ROOMDOOR ) return true;
        return false;                                                                      // forest sand snow and water should not be saved
    }

    public void SaveCube( Unit save = null )
    {
        if( Map.I.CubeDeath ) return;                                                      // No Save if dying
        IsSaving = true;
        string nm = " My";
        if( save ) nm = " " + save.Pos.x + " " + save.Pos.y;

        Manager.I.Inventory.Save( nm );                                                    // Save Inventory

        Statistics.Save( nm );                                                             // Save Statistics

        Sector.Save( nm );                                                                 // Save Sector
        
        string file = Manager.I.GetProfileFolder() + "Cube Save/Cube" + nm + ".NEO";       // Provides filename
        Sector s = Map.I.RM.HeroSector;     

        using( W = new BinaryWriter( File.Open( file, FileMode.OpenOrCreate ) ) )
        {
            int SaveVersion = 1;
            GS.W.Write( SaveVersion );                                                     // Save Version

            GS.W.Write( G.Farm.UniqueID );                                                 // save Player Unique ID 

            Map.I.Save();                                                                  // Save Map

            ga2l = new List<Unit>();

            for( int y = ( int ) s.Area.yMin - 1; y < s.Area.yMax + 1; y++ )
            for( int x = ( int ) s.Area.xMin - 1; x < s.Area.xMax + 1; x++ )
                {
                    int tile = -1;
                    Unit ga = Map.I.GetUnit( new Vector2( x, y ), ELayerType.GAIA );      // Save Gaia
                    if( ga ) tile = ( int ) ga.TileID;
                    GS.W.Write( tile );

                    Unit ga2 = Map.I.GetUnit( new Vector2( x, y ), ELayerType.GAIA2 );    // Save Gaia 2
                    if( ga2 ) ga2l.Add( ga2 );
                }

            G.Hero.SaveHeader();                                                     // Save hero header
            G.Hero.Save();                                                           // Save hero main

            GS.W.Write( s.MoveOrder.Count );

            for( int i = 0; i < s.MoveOrder.Count; i++ )                             // Save Monsters header
                s.MoveOrder[ i ].SaveHeader();

            for( int i = 0; i < s.MoveOrder.Count; i++ )                             // Save Monsters main
                s.MoveOrder[ i ].Save();

            GS.W.Write( G.HS.Fly.Count );                                  
            for( int i = 0; i < G.HS.Fly.Count; i++ )                                // Save Flying Units header
                G.HS.Fly[ i ].SaveHeader();

            for( int i = 0; i < G.HS.Fly.Count; i++ )                                // Save Flying Units main
                G.HS.Fly[ i ].Save();

            GS.W.Write( ga2l.Count );
            for( int i = 0; i < ga2l.Count; i++ )                                    // Save Gaia header
                ga2l[ i ].SaveHeader();

            for( int i = 0; i < ga2l.Count; i++ )                                    // Save Gaia main
                ga2l[ i ].Save();

            Artifact.Save( nm );                                                     // Save Artifact

            Secret.SaveTemp( nm );                                                   // Save temp Secret steps

            W.Close();                                                               // Closes the stream

            if( save != null )
            {
                LastSavedCube = save;                                                // Set as last saved cube
                if( GS.SaveStepUnitList.Contains( save ) == false )                  // Adds save to step list
                {
                    GS.SaveStepUnitList.Add( save );
                    GS.SaveStepList.Add( save.Pos );
                }
            }
            MasterAudio.PlaySound3DAtVector3( "Percussion", G.Hero.Pos );            // Sound FX
        }
        IsSaving = false;
    }    
    public void UpdateReferencesAfterLoad()
    {
        G.Hero.Spr.gameObject.SetActive( true );
        Util.SmoothRotate( G.Hero.Spr.transform, G.Hero.Dir, 10000 );                                           // Init hero rotation
        Util.SmoothRotate( G.Hero.Body.Shadow.transform, G.Hero.Dir, 10000 );

        UpdateFatherSonConnection( G.Hero );                                                                    // hero father and son connection

        for( int i = 0; i < G.HS.Fly.Count; i++ )
        if( G.HS.Fly[ i ].TileID == ETileType.MINE )           
        {
            Unit fl = G.HS.Fly[ i ];
            UpdateFatherSonConnection( fl );                                                                   // Father and son connections

            fl.Body.MineLeverActivePuller = Map.GFU( ETileType.MINE, fl.Body.MineLeverActivePullerPos );       // Connect to active puller
            if( fl.Body.MineLeverActivePuller && fl.Body.MineHasLever() )
                fl.Body.MineLeverActivePuller.Body.EffectList[ 2 ].gameObject.SetActive( true );
            if( fl.Body.RopeConnectSon )
                fl.UpdateChainSizes( fl.Body.RopeConnectSon.Pos );                                             // Update chain
            fl.Body.RopeConnectSonPos = new Vector2( -1, -1 );
            fl.Body.MineLeverActivePullerPos = new Vector2( -1, -1 );
            fl.Mine.UpdateText = true;
        }

        RestLine.Create();                                                                                    // Create Resting Lines 

        for( int i = 0; i < 8; i++ )
        if ( G.Hero.Body.Sp[ i ].Type == ESpiderBabyType.TORCH )                                              // Add Torches Light
            LightSource2D.Create( CubeData.I.TorchBase, i );

        if( G.HS.Cleared )
            Item.SetAmt( ItemType.Res_Butcher_Coin, CurrentButcherCoins );                                    // Do not Restore coins to avoid cheating: Once spent, its gone
    }

    private static void UpdateFatherSonConnection( Unit fl )
    {
        fl.Body.RopeConnectFather = new List<Unit>();
        fl.Body.RopeConnectSon = Map.GFU( ETileType.MINE, fl.Body.RopeConnectSonPos );                        // Connect to son

        if( fl.Body.RopeConnectSonPos == G.Hero.Pos )                                                         // connect to hero if same pos
            fl.Body.RopeConnectSon = G.Hero;

        for( int f = 0; f < fl.Body.RopeConnectFatherPosList.Count; f++ )                                     // Connect to Father list
        {
            Vector2 pt = fl.Body.RopeConnectFatherPosList[ f ];
            Unit fath = fl.GetConnectableUnit( pt );
            fl.Body.RopeConnectFather.Add( fath );
        }
        fl.Body.RopeConnectFatherPosList = null;
    }

    public static void DeleteCubeSaves( Sector s = null )
    {
        if ( s )
        for( int y = ( int ) s.Area.yMin - 1; y < s.Area.yMax + 1; y++ )           
        for( int x = ( int ) s.Area.xMin - 1; x < s.Area.xMax + 1; x++ )
        {
             Unit save = Map.I.GetUnit( ETileType.SAVEGAME, new Vector2( x, y ) );
             if( save )
             {
                 save.Body.EffectList[ 0 ].gameObject.SetActive( false );            // Deactivate effects
                 save.Body.EffectList[ 1 ].gameObject.SetActive( false );           
                 save.Body.EffectList[ 2 ].gameObject.SetActive( false );         
             }
        }
        GS.SaveStepUnitList = new List<Unit>();                                      // Empty lists
        GS.SaveStepList = new List<Vector2>();
        ForceLoad = false;
        CustomSaveExists = false;
        LastLoadedCube = null;
        LastSavedCube = null;
        string dir = Manager.I.GetProfileFolder() + "Cube Save/";
        string[] files = System.IO.Directory.GetFiles( dir );                        // Get file list

        for( int i = 0; i < files.Length; i++ )
            File.Delete( files[ i ] );                                               // Delete cube files
    }
    public static bool ChargeCubeSavingResource()
    {
        if( CubeLoaded == false ) return true;
        if( LastLoadedCube == null )                                                      // Debug save: do not charge
        {
            CubeLoaded = false;
            UI.I.SetBigMessage( "", Color.yellow, .1f );
            return false; 
        }

        Unit save = Map.I.GetUnit( ETileType.SAVEGAME, G.Hero.Pos );
        if( save == null ) return true;

        ItemType type = ( ItemType ) save.Variation;
        float cost = GetLoadCost( save );
        float num = Item.GetNum( type );

        if( num < cost )
        {
            string msg = "x" + cost.ToString( "0.#" ) + " Needed!";                       // Out of resouces message
            Message.CreateMessage( ETileType.NONE, ( ItemType )
            type, msg, G.Hero.Pos, Color.red );
            MasterAudio.PlaySound3DAtVector3( "Error 2", G.Hero.Pos );                    // Error Sound FX
            return false;
        }

        if( cost > 0 )
        {
            Item.ForceMessage = true;
            Item.IgnoreBuffer = true;
            Item.AddItem( ( ItemType ) type, -cost );                                     // Charges item
            Item.IgnoreBuffer = false;
            string nm = " " + save.Pos.x + " " + save.Pos.y;
            Manager.I.Inventory.Save( nm );                                               // Save Inventory because an item was charged
        }
        
        CubeLoaded = false;
        UI.I.SetBigMessage( "", Color.yellow, .1f );
        return true;
    }
    internal static float GetLoadCost( Unit save )
    {
        float cost = save.Body.StackAmount;
        float pr = 100 - AdventureUpgradeInfo.GetStat( EAdventureUpgradeType.LOAD_COST_DISCOUNT );    // Load Cost Discount Tech
        cost = Util.Percent( pr, cost );
        return cost;
    }
    #region Util
    public static void SVector2( Vector2 v )
    {
        GS.W.Write( v.x );
        GS.W.Write( v.y );
    }
    public static Vector2 LVector2()
    {
        Vector2 res = new Vector2();
        res.x = GS.R.ReadSingle();
        res.y = GS.R.ReadSingle();
        return res;
    }
    public static void SVector3( Vector3 v )
    {
        GS.W.Write( v.x );
        GS.W.Write( v.y );
        GS.W.Write( v.z );
    }
    public static Vector3 LVector3()
    {
        Vector3 res = new Vector3();
        res.x = GS.R.ReadSingle();
        res.y = GS.R.ReadSingle();
        res.z = GS.R.ReadSingle();
        return res;
    }

    public static Quaternion LQuat()
    {
        Quaternion res = new Quaternion();
        res.x = GS.R.ReadSingle();
        res.y = GS.R.ReadSingle();
        res.z = GS.R.ReadSingle();
        res.w = GS.R.ReadSingle();
        return res;
    }

    public static void SQuat( Quaternion q )
    {
        GS.W.Write( q.x );
        GS.W.Write( q.y );
        GS.W.Write( q.z );
        GS.W.Write( q.w );
    }
    public static void STrans( Transform tr )
    {
        SVector3( tr.position );
        SQuat( tr.rotation );
        SVector3( tr.localScale );
    }

    public static void LTrans( Transform tr )
    {
        tr.position = LVector3();
        tr.rotation = LQuat();
        tr.localScale = LVector3();
    }
    public static void SColor( Color color )
    {
        GS.W.Write( color.r );
        GS.W.Write( color.g );
        GS.W.Write( color.b );
        GS.W.Write( color.a );
    } 
    public static Color LColor()
    {
        return new Color(
            GS.R.ReadSingle(),
            GS.R.ReadSingle(),
            GS.R.ReadSingle(),
            GS.R.ReadSingle() );
    }
    public static void SList<T>( List<T> list )
    {
        if( list == null )
        {
            GS.W.Write( 0 );
            return;
        }

        Type t = typeof( T );
        bool isEnum = t.IsEnum;
        if( isEnum ) t = Enum.GetUnderlyingType( t );

        int count = list.Count;
        GS.W.Write( count );

        if( count == 0 ) return;

        if( t == typeof( int ) || t == typeof( float ) || t == typeof( double ) ||
            t == typeof( long ) || t == typeof( short ) )
        {
            int size = Marshal.SizeOf( t );
            Array arr = Array.CreateInstance( t, count );
            for( int i = 0; i < count; i++ )
                arr.SetValue( Convert.ChangeType( list[ i ], t ), i );

            byte[] bytes = new byte[ count * size ];
            Buffer.BlockCopy( arr, 0, bytes, 0, bytes.Length );
            GS.W.Write( bytes );
        }
        else if( t == typeof( bool ) )
        {
            for( int i = 0; i < count; i++ )
                GS.W.Write( ( bool ) ( object ) list[ i ] );
        }
        else if( t == typeof( string ) )
        {
            for( int i = 0; i < count; i++ )
                GS.W.Write( ( string ) ( object ) list[ i ] ?? "" );
        }
        else
        {
            throw new NotSupportedException( "Tipo não suportado em SList: " + typeof( T ) );
        }
    }
    public static List<T> LList<T>()
    {
        Type originalType = typeof( T );
        Type t = originalType;
        bool isEnum = t.IsEnum;
        if( isEnum ) t = Enum.GetUnderlyingType( t );

        int count = GS.R.ReadInt32();
        var list = new List<T>( count );

        if( count == 0 ) return list;

        if( t == typeof( int ) || t == typeof( float ) || t == typeof( double ) ||
            t == typeof( long ) || t == typeof( short ) )
        {
            int size = Marshal.SizeOf( t );
            byte[] bytes = GS.R.ReadBytes( count * size );
            Array arr = Array.CreateInstance( t, count );
            Buffer.BlockCopy( bytes, 0, arr, 0, bytes.Length );

            for( int i = 0; i < count; i++ )
            {
                object val = arr.GetValue( i );
                if( isEnum ) list.Add( ( T ) Enum.ToObject( originalType, val ) );
                else list.Add( ( T ) val );
            }
        }
        else if( t == typeof( bool ) )
        {
            for( int i = 0; i < count; i++ )
                list.Add( ( T ) ( object ) GS.R.ReadBoolean() );
        }
        else if( t == typeof( string ) )
        {
            for( int i = 0; i < count; i++ )
                list.Add( ( T ) ( object ) GS.R.ReadString() );
        }
        else
        {
            throw new NotSupportedException( "Tipo não suportado em LList: " + t );
        }

        return list;
    }
    public static void SV2List( List<Vector2> list )
    {
        GS.W.Write( list.Count ); // número de elementos
        for( int i = 0; i < list.Count; i++ )
        {
            GS.W.Write( list[ i ].x );
            GS.W.Write( list[ i ].y );
        }
    }
    public static List<Vector2> LV2List()
    {
        int count = GS.R.ReadInt32();
        var list = new List<Vector2>( count );
        for( int i = 0; i < count; i++ )
        {
            float x = GS.R.ReadSingle();
            float y = GS.R.ReadSingle();
            list.Add( new Vector2( x, y ) );
        }
        return list;
    }
    #endregion
}
