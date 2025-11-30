using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using DarkTonic.MasterAudio;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Secret : MonoBehaviour 
{
    public static List<Vector2> SecretPos = new List<Vector2>();
    public static List<int> SecretQuestID = new List<int>();
    public static List<int> SecretCubeNumber = new List<int>();
    public static List<Vector2> FlushSecretList = new List<Vector2>();
    public static void Reset()
    {
        SecretPos = new List<Vector2>();
        SecretQuestID = new List<int>();
        SecretCubeNumber = new List<int>();        
    }
    public static void InitializeSecrets( bool force = false )
    {
        if( G.HS == null ) return;

        if( force == false )
        {
            if( Map.I.AdvanceTurn == true ) return;
            if( G.HS.CubeFrameCount != 6 ) return;
        }

        Load();

        Sector hs = Map.I.RM.HeroSector;
        for( int y = ( int ) hs.Area.yMin - 1; y < hs.Area.yMax + 1; y++ )                       // find all secrets in the cube
        for( int x = ( int ) hs.Area.xMin - 1; x < hs.Area.xMax + 1; x++ )
        {
            Unit sec = Map.I.GetUnit( ETileType.SECRET, new Vector2( x, y ) );
            if( sec )
            {
                Vector2 pos = Helper.I.GetCubeTile( new Vector2( x, y ) );
                int cube = Map.I.RM.HeroSector.Number;
                int sz = SecretPos.Count;
                for( int i = 0; i < sz; i++ )
                if( SecretQuestID[ i ] == Map.I.RM.CurrentAdventure )
                if( SecretCubeNumber[ i ] == cube )
                if( SecretPos[ i ] == pos )
                {
                    Map.Kill( sec );                                                             // destroy secret already stepped
                }

                sec.Graphic.gameObject.SetActive( true );
                sec.Body.EffectList[ 2 ].gameObject.SetActive( true );
            }
        }
    }

    public static void CheckSecretPickup()
    {
        Unit sec = Map.I.GetUnit( ETileType.SECRET, G.Hero.Pos );
        if( sec == null ) return;
        Vector2 pos = Helper.I.GetCubeTile( G.Hero.Pos );                                       // secret found
        FlushSecretList.Add( pos );                                                             // fills a list of temp secrets stepped to increment value after leaving cube
        Controller.CreateMagicEffect( G.Hero.Pos );
        MasterAudio.PlaySound3DAtVector3( "Save Game", G.Hero.Pos );                            // Sound FX
        
        if( sec.Variation != -1 )
            Item.AddItem( ( ItemType ) sec.Variation, sec.Body.StackAmount );

        Map.Kill( sec );
    }

    public static void SaveTemp( string nm = "" )
    {
        string file = Manager.I.GetProfileFolder();
        if( nm != "" ) file += "Cube Save/Secret Temp" + nm + ".NEO";                       // Provides filename

        using( GS.W = new BinaryWriter( File.Open( file, FileMode.OpenOrCreate ) ) )
        {
            int SaveVersion = 1;
            GS.W.Write( SaveVersion );                                                      // Save Version 

            GS.W.Write( FlushSecretList.Count );
            for( int i = 0; i < FlushSecretList.Count; i++ )
                GS.SVector2( FlushSecretList[ i ] );                                       // Save Flush Secrets List

            GS.W.Close();
        }
    }
    public static void LoadTemp( string nm = "" )
    {
        string file = Manager.I.GetProfileFolder();
        if( nm != "" ) file += "Cube Save/Secret Temp" + nm + ".NEO";                      // Provides filename

        if( File.Exists( file ) == false )                                                 // File Do not exists
        {
            FlushSecretList = new List<Vector2>();
            return;
        }

        using( GS.R = new BinaryReader( File.Open( file, FileMode.Open ) ) )
        {
            int SaveVersion = GS.R.ReadInt32();                                           // Load Version

            FlushSecretList = new List<Vector2>();
            int sz = GS.R.ReadInt32();
            for( int i = 0; i < sz; i++ )
                FlushSecretList.Add( GS.LVector2() );                                     // Load Flush Secrets List

            GS.R.Close();
        }
    }

    public static void Save( string nm = "" )
    {
        if( Manager.I.SaveOnEndGame == false ) return;

        string file = Manager.I.GetProfileFolder();
        if( nm != "" ) file += "Cube Save/Secret" + nm + ".NEO";                            // Provides filename
        else file += "/Secret.NEO";

        using( MemoryStream ms = new MemoryStream() )
        using( BinaryWriter writer = new BinaryWriter( ms ) )                               // Open Memory Stream
        {
            GS.W = writer;                                                                  // Assign BinaryWriter to GS.W for TF

            int Version = Security.SaveHeader( 1 );                                         // Save Header Defining Current Save Version

            TF.SaveT( "SecretPos", SecretPos );                                             // Save Stepped Secrets Positions
            TF.SaveT( "SecretCubeNumber", SecretCubeNumber );                               // Save Stepped Secrets Cube Number
            TF.SaveT( "SecretQuestID", SecretQuestID );                                     // Save Stepped Secrets Quest ID

            GS.W.Flush();                                                                   // Flush the writer

            Security.FinalizeSave( ms, file );                                              // Finalize save
        }                                                                                   // using closes the stream automatically
    }

    public static void Load( string nm = "" )
    {
        string file = Manager.I.GetProfileFolder();
        if( nm != "" ) file += "Cube Save/Secret" + nm + ".NEO";                           // Provides filename
        else file += "/Secret.NEO";

        if( File.Exists( file ) == false )                                                 // File does not exist
        {
            Reset();
            return;
        }

        byte[] fileData = File.ReadAllBytes( file );                                       // Read full file
        byte[] content = Security.CheckLoad( fileData );                                   // Validate HMAC and get clean content

        using( GS.R = new BinaryReader( new MemoryStream( content ) ) )                    // Use MemoryStream for TF
        {
            int SaveVersion = Security.LoadHeader();                                       // Load Header

            SecretPos = TF.LoadT<List<Vector2>>( "SecretPos" );                            // Load Stepped Secrets Positions
            SecretCubeNumber = TF.LoadT<List<int>>( "SecretCubeNumber" );                  // Load Stepped Secrets Cube Number            
            SecretQuestID = TF.LoadT<List<int>>( "SecretQuestID" );                        // Load Stepped Secrets Quest ID

            GS.R.Close();                                                                  // Close stream
        }
    }


    internal static void FlushSecrets( Sector hs )
    {
        int cube = hs.Number;
        int quest = Map.I.RM.CurrentAdventure;

        int count = 0;
        for( int i = 0; i < FlushSecretList.Count; i++ )                                                                              // loops through all temp steped secrets list
        {
            Vector2 pos = FlushSecretList[ i ];
            bool exists = false;
            for( int j = 0; j < SecretPos.Count; j++ )
            {
                if( SecretQuestID[ j ] == quest && SecretCubeNumber[ j ] == cube && SecretPos[ j ] == pos )
                {
                    exists = true;                                                                                                   // check if it exists in the master list
                    break;
                }
            }

            if( !exists )
            {
                Message.CreateMessage( ETileType.NONE, ItemType.Genius_Trophy, "Secret Found!",                                      // message
                G.Hero.Pos, Color.green, Util.Chc(), Util.Chc(), 4, count++ );
                SecretPos.Add( pos );
                SecretQuestID.Add( quest );
                SecretCubeNumber.Add( cube );
                Item.AddItem( ItemType.Secrets_Found, +1, Inventory.IType.Inventory, true, quest );                                  // Adds Secret
                G.GIT( ItemType.Secrets_Found ).Count++;
            }
        }

        FlushSecretList = new List<Vector2>();

        Save();                                                                                                                      // save
    }
    internal static void DestroyAllSecrets( Sector hs )
    {
        for( int y = ( int ) hs.Area.yMin - 1; y < hs.Area.yMax + 1; y++ )                       // find all secrets in the cube
        for( int x = ( int ) hs.Area.xMin - 1; x < hs.Area.xMax + 1; x++ )
        {
            Unit sec = Map.I.GetUnit( ETileType.SECRET, new Vector2( x, y ) );
            if( sec ) sec.Kill();
        }
    }

    #if UNITY_EDITOR
    [MenuItem( "Helper/Update Secrets", false, 5 )]
    public static void UpdateSecretData()
    {
        RandomMap rm = GameObject.Find( "----------------Random Map----------------" ).
        GetComponent<RandomMap>();
        MapSaver ms = GameObject.Find( "Areas Template Tilemap" ).GetComponent<MapSaver>();
        tk2dTileMap tm = ms.GetRM().AreasTM;
        string fileold = Application.dataPath + "/Resources/Map Templates/" +
        ms.FolderName + "/" + ms.MapName + ".NEO";
        string oldmn = ms.MapName;
        Map.I = GameObject.Find( "----------------Map" ).GetComponent<Map>();
        Map.I.TotalSecrets = 0;

        for( int rmid = 0; rmid < rm.RMList.Count; rmid++ )
        if( rm.RMList[ rmid ].Available )
        {
            RandomMapData rmd = rm.RMList[ rmid ];
            string folder = Application.dataPath + "/Resources/Map Templates/" + rm.RMList[ rmid ].QuestHelper.SubFolder + "/" + rm.RMList[ rmid ].QuestHelper.Signature;
            rmd.QuestSecrets = 0;
            for( int i = 0; i < rmd.MaxCubes; i++ )
            {
                string nm = "Cube " + ( i + 1 );                
                ms.MapName = nm;
                ms.FolderName = rm.RMList[ rmid ].QuestHelper.SubFolder + "/" + rm.RMList[ rmid ].QuestHelper.Signature;
                string file = folder + "/" + nm + ".NEO";

                ms.LoadMap( file, ref tm );
                tm.ForceBuild();

                for( int y = 0; y < ( int ) tm.height; y++ )
                for( int x = 0; x < ( int ) tm.width; x++ )
                    {
                        int tl = tm.GetTile( x, y, ( int ) ELayerType.GAIA2 );

                        if( tl == ( int ) ETileType.SECRET )
                        {
                            Map.I.TotalSecrets++;
                            rmd.QuestSecrets++;
                            Debug.Log( " Cube: " + nm + "  Folder:  " + folder + " Name: " + rm.RMList[ rmid ].name + " Pos: " + new Vector2( x, y ) );
                        }
                    }
            }
        }
        ms.MapName = oldmn;
        ms.LoadMap( fileold, ref tm );
        Debug.Log( "Total Secrets: " + Map.I.TotalSecrets );
    }
#endif
}
