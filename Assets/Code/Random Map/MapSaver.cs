using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; 
#if UNITY_EDITOR
using UnityEditor;
#endif
using Sirenix.OdinInspector;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;

public enum EMapTemplate
{
    __CHOOSE__ = -1, Empty, Temp, The_Pond_1, New_1, New_2, Template_1, Template_2, Template_3, Template_4, Template_5
}

public enum EOri2Template
{
    NONE = -1, ORI_1, ORI_2, ORI_3, ORI_4, ORI_5, ORI_6, ORI_7, ORI_8, ORI_9, ORI_10, ORI_11, ORI_12, ORI_13, ORI_14, ORI_15, ORI_16, ORI_17, ORI_18, ORI_19, ORI_20,
    ORI_21, ORI_22, ORI_23, ORI_24, ORI_25, ORI_26, ORI_27, ORI_28, ORI_29, ORI_30, ORI_31, ORI_32, ORI_33, ORI_34, ORI_35, ORI_36, ORI_37, ORI_38, ORI_39, ORI_40, ORI_41, ORI_42, ORI_43, ORI_44, ORI_45, ORI_46, ORI_47, ORI_48
}
public enum EOri3Template
{
    NONE = -1, ORI_1, ORI_2, ORI_3, ORI_4, ORI_5, ORI_6, ORI_7, ORI_8, ORI_9, ORI_10, ORI_11, ORI_12, ORI_13, ORI_14, ORI_15, ORI_16, ORI_17, ORI_18, ORI_19, ORI_20
}

public enum EOrientatorType
{
    NONE = -1, ORI_0 = 134, ORI_1 = 135, ORI_2 = 136, ORI_3 = 262, ORI_4 = 264, ORI_5 = 390, ORI_6 = 391, ORI_7 = 392, ORI_8 = 512,
    ORI_9, ORI_10, ORI_11, ORI_12, ORI_13, ORI_14, ORI_15, ORI_16, ORI_17, ORI_18, ORI_19, ORI_20, ORI_21   
}

public class MapSaver : MonoBehaviour
{
    public static MapSaver I;
    //[HideInInspector]
    public CubeData CubeData;
    [HideInInspector]
    public string LastLoadedFile;
    [HideInInspector]
    public Vector2 Size = new Vector2( 20, 20 );
    [HideInInspector]
    public Vector2 Source;
    [HideInInspector]
    public tk2dTileMap Tilemap;
    public static bool Converting = false;

    [HorizontalGroup( "Split", 0.5f )]
    [Button( "Load", ButtonSizes.Large ), GUIColor( 0, 1f, 0 )]
    public void Button1()
    {
        Load();
        MapQuestHelper hp = GetRM().RMList[ CurrentAdventure ].GetComponent<MapQuestHelper>();
        CurrentCube = MapName;
#if UNITY_EDITOR
        hp.UpdateExternaEditorInfo();
#endif
    }

    [HorizontalGroup( "Split", 0.5f )]
    [Button( "Save", ButtonSizes.Large ), GUIColor( 1, 0.2f, 0 )]
    public void Button2()
    {
        Save();
        MapQuestHelper hp = GetRM().RMList[ CurrentAdventure ].GetComponent<MapQuestHelper>();
#if UNITY_EDITOR
        hp.UpdateExternaEditorInfo();
#endif
    }
    
    [Button( "World Map...", ButtonSizes.Large ), GUIColor( 1f, 0.52f, 0.1f )]
    public void Button9()
    {
        EditQuestDataCallBack();
    }

    [HorizontalGroup( "Split", 0.1f )]
    [Button( "MOD", ButtonSizes.Large ), GUIColor( 1f, 1f, 0 )]
    public void ButtonMod()
    {
#if UNITY_EDITOR  
        RandomMapData rm = GetRM().RMList[ CurrentAdventure ];
        if( rm )
            Selection.activeGameObject = rm.GetComponentInChildren<Mod>().gameObject;
#endif
    }

    [BoxGroup( "1", false, false, 1 )]
    public string FolderName, SubFolder;
    [BoxGroup( "1", false, false, 1 )]
    public string MapName;
    [BoxGroup( "1", false, false, 1 )]
    public string CurrentCube = "";

    [HideInInspector]
    public Sector CurrentSector;

    [ButtonGroup( "2", 2 )]
    [Button( "Previous Cube", ButtonSizes.Large ), GUIColor( 1, 1f, 0 )]
    public void Button3()
    {
        PrevMapCallBack();
    }

    [ButtonGroup( "2", 2 )]
    [Button( "Next Cube", ButtonSizes.Large ), GUIColor( 1, 1f, 0 )]
    public void Button4()
    {
        NextMapCallBack();
    }

    [BoxGroup( "3", false, false, 3 )]
    public int CurrentAdventure = 0;
    [BoxGroup( "3", false, false, 3 )]
    public string CurrentAdventureName = "";

    [ShowIf( "Advanced" )]
    [ButtonGroup( "4", 4 )]
    [Button( "Delete Cube", ButtonSizes.Medium ), GUIColor( 1, 0.2f, 0 )]
    public void Button5()
    {
        DeleteMapCallBack();
    }

    [ShowIf( "Advanced" )]
    [ButtonGroup( "4", 4 )]
    [Button( "Add Fog", ButtonSizes.Small ), GUIColor( 1, 1, 0 )]
    public void ButtonFog()
    {
        AddFogCallBack();
    }

    [ShowIf( "Advanced" )]
    [ButtonGroup( "10", 10 )]
    [Button( "Move UP", ButtonSizes.Medium ), GUIColor( 0.4f, 0.4f, .8f )]
    public void ButtonMoveUp()
    {
        MoveUpCallBack();
    }

    [ShowIf( "Advanced" )]
    [ButtonGroup( "10", 10 )]
    [Button( "Move DN", ButtonSizes.Medium ), GUIColor( 0.4f, 0.4f, .8f )]
    public void ButtonMoveDown()
    {
        MoveDownCallBack();
    }
  

    [ButtonGroup( "4", 6 )]
    [Button( "Test Quest", ButtonSizes.Gigantic ), GUIColor( 0, 1f, 0 )]
    public void Button6()
    {
         TestQuestCallBack();
    }

    [BoxGroup( "5", false, false, 5 )]
    public bool Advanced = false;

    [ShowIf( "Advanced" )]
    [BoxGroup( "5", false, false, 5 )]
    public float FogChance = 20;
    [ShowIf( "Advanced" )]
    [BoxGroup( "5", false, false, 5 )]
    public bool RemoveFog = false;
    [ShowIf( "Advanced" )]
    [BoxGroup( "5", false, false, 5 )]
    public bool FogOnWater = false;
    [ShowIf( "Advanced" )]
    [BoxGroup( "5", false, false, 5 )]
    public bool AutoDecorLayer = false;

    [HideInInspector]
    public EMapTemplate MapTemplate = EMapTemplate.__CHOOSE__;

    [ButtonGroup( "6", 6 )]
    [Button( "Previous Quest", ButtonSizes.Large ), GUIColor( 1, 1f, 0 )]
    public void Button7()
    {
        PrevAdventureCallBack();
    }

    [ButtonGroup( "6", 6 )]
    [Button( "Next Quest", ButtonSizes.Large ), GUIColor( 1, 1f, 0 )]
    public void Button8()
    {
        NextAdventureCallBack();
    }

    [BoxGroup( "7", false, false, 7 )]
    [Header( "Write Message Here!" )]
    [TextArea( 1, 20 )]
    public string Message = "";
    
    [BoxGroup( "7", false, false, 7 )]
    [Header( "Write Script Here!" )]
    [TextArea( 1, 20 )]
    public string Script = "";
    [HideInInspector]
    public string LastMapSavedFilename;
    [HideInInspector]
    public EModType AutoMod = EModType.NONE;
    [HideInInspector]
    public EOrientatorType AutoOri = EOrientatorType.NONE;
    [HideInInspector]
    public EOri2Template AutoOri2 = EOri2Template.NONE;
    [HideInInspector]
    public EOri3Template AutoOri3 = EOri3Template.NONE;

	void Start () 
    {
        I = this;
        Source = new Vector2();
	}
	
    public static MapSaver Get()
    {
        GameObject ms = GameObject.Find( "Areas Template Tilemap" );
        if( ms ) return ms.GetComponent<MapSaver>(); ;
        return null;
    }

    public bool Load( bool showErrorMsg = true )
    {
        string nm = MapName;
        tk2dTileMap tm = null;
        string sub = "";
        if( Application.isPlaying )
        {
            RandomMapData rm = Map.I.RM.RMList[ Map.I.RM.CurrentAdventure ];
            if( Map.I.RM.SD.MapTemplates != null && Map.I.RM.SD.MapTemplates.Count > 0 )
            {
                int id = UnityEngine.Random.Range( 0, Map.I.RM.SD.MapTemplates.Count );
                nm = rm.QuestHelper.SubFolder + "/" + rm.QuestHelper.Signature + "/" + 
                     Map.I.RM.SD.MapTemplates[ id ].name;
                FolderName = "";
                tm = Map.I.RM.AreasTM;
            }
            else
            {
                string folder = rm.QuestHelper.SubFolder + "/" + rm.QuestHelper.Signature;
                nm = folder + "/Cube " + CurrentSector.Number;
                FolderName = "";
                tm = Map.I.RM.AreasTM;
            }
        }

        string fn = FolderName;
        if( MapTemplate != EMapTemplate.__CHOOSE__ )                                                              // Template
        {
            sub = "";
            fn = "Default Templates";
            nm = "" + MapTemplate;
        }

        if( tm == null )
        {
            GameObject go = GameObject.Find( "Areas Template Tilemap" );
            tm = go.GetComponent<tk2dTileMap>();
        }
        string file = Application.dataPath + "/Resources/Map Templates/" + sub + fn + "/" + nm + ".NEO";
        LastMapSavedFilename = file;
        bool res = LoadMap( file, ref tm );
        if( !res )
        {
            if( MapTemplate != EMapTemplate.__CHOOSE__ ) return false;
            if( showErrorMsg )
            {
                Debug.LogError( "Could not load: " + file );
                Application.Quit();
            }
            return false;
        }
        else
            Debug.Log( "Map Loaded! " + file );
        LastLoadedFile = file;
        MapTemplate = EMapTemplate.__CHOOSE__;

        if( Application.isPlaying == false )
        {
            RandomMapData rm = GetRM().RMList[ CurrentAdventure ];
            SubFolder = rm.QuestHelper.SubFolder;
        }
        return true;
    }
    public void Save()
    {
        GameObject ob = GameObject.Find( "Farm" );
        Farm f = ob.GetComponent<Farm>();
        //f.UpdateListsCallBack();
        GameObject go = GameObject.Find("Areas Template Tilemap");
        tk2dTileMap tm = go.GetComponent<tk2dTileMap>();
        RandomMapData rm = GetRM().RMList[ CurrentAdventure ];
        string nm = MapName;
        string fn = FolderName;

        if( MapTemplate != EMapTemplate.__CHOOSE__ )                                                              // Template
        {
            fn = "Default Templates";
            nm = "" + MapTemplate;
        }

        string file = Application.dataPath + "/Resources/Map Templates/" + 
        fn + "/" + nm + ".NEO";
        LastMapSavedFilename = file;
        SaveMap( file, ref tm );
        LastMapSavedFilename = file;
        Debug.Log("Map Saved! " + file );

        if( Converting == false )
            CopyToRelease( nm, fn, file );                                                                        // Copy to PC version    
        if( Application.isPlaying == false )
            SubFolder = rm.QuestHelper.SubFolder;
    }
    private void CopyToRelease(string nm, string fn, string file )
    {
        if( Converting ) return;
        string buildPath = "C:/Users/alien/Desktop/NEO Release/NEO_Data/Resources/Map Templates/" +
        fn + "/" + nm + ".NEO";
        string buildFolder = System.IO.Path.GetDirectoryName( buildPath );
        if( !System.IO.Directory.Exists( buildFolder ) )
            System.IO.Directory.CreateDirectory( buildFolder );
        System.IO.File.Copy( file, buildPath, true );
        Debug.Log( "Map Copied to Build! " + buildPath );
    }

    ELayerType[] layers = new ELayerType[]
        {
            ELayerType.TERRAIN,
            ELayerType.GAIA,
            ELayerType.GAIA2,
            ELayerType.MONSTER,
            ELayerType.MODIFIER,
            ELayerType.DECOR,
            ELayerType.AREAS,
            ELayerType.DECOR2,
            ELayerType.RAFT
        };
    public void SaveMap( string file, ref tk2dTileMap tm )
    {
        Directory.CreateDirectory( Path.GetDirectoryName( file ) );                                     // Create folder if not exists
        using( var ms = new MemoryStream() )                                                            // Open MemoryStream
        using( var writer = new BinaryWriter( ms ) )                                                    // Create BinaryWriter
        {
            int SaveVersion = 1;
            writer.Write( SaveVersion );                                                               // Save Version

            GS.W = writer;                                                                             // Set GS.W para usar SVector2
            GS.SVector2( Size );                                                                       // Save Map size
            writer.Write( layers.Length );                                                             // Save layer count

            int totalTiles = ( int ) Size.x * ( int ) Size.y * layers.Length;
            int[] tileBuffer = new int[ totalTiles ];                                                  // Buffer único

            int index = 0;
            for( int y = 0; y < ( int ) Size.y; y++ )
            for( int x = 0; x < ( int ) Size.x; x++ )
            foreach( var layer in layers )
                 tileBuffer[ index++ ] = tm.GetTile( x, y, ( int ) layer );                            // Save Layers into buffer

            for( int i = 0; i < tileBuffer.Length; i++ )
                GS.W.Write( tileBuffer[ i ] );                                                         // Write buffer to stream

            GS.W.Write( Message );                                                                     // Save Message
            GS.W.Write( Script );                                                                      // Save Script
            CubeData.Save();                                                                           // Save Cube Data

            // Criptografar e gravar no disco
            byte[] encrypted = AESCrypto.Encrypt( ms.ToArray() );
            File.WriteAllBytes( file, encrypted );                                                     // Save encrypted bytes
        }
    }

    public bool LoadMap( string file, ref tk2dTileMap tm )
    {
        if( tm == null )                                                                               // Checar se tm é null
        {
            Debug.Log( "tm == null" );
            return false;
        }

        if( File.Exists( file ) == false ) return false;                                               // Checar se arquivo existe

        // Ler bytes criptografados e descriptografar
        byte[] encrypted = File.ReadAllBytes( file );                                                  // Ler bytes
        byte[] decrypted = AESCrypto.Decrypt( encrypted );                                             // Descriptografar bytes

        using( var ms = new MemoryStream( decrypted ) )                                                // Abrir MemoryStream com dados descriptografados
        using( var reader = new BinaryReader( ms ) )                                                   // Criar BinaryReader
        {
            GS.R = reader;                                                                            // Set GS.R

            int SaveVersion = reader.ReadInt32();                                                     // Load Version
            Size = GS.LVector2();                                                                     // Load Map size

            int layerCount = reader.ReadInt32();                                                      // Load Layer Count

            int totalTiles = ( int ) Size.x * ( int ) Size.y * layers.Length;
            int[] tileBuffer = new int[ totalTiles ];                                                 // Buffer único para leitura

            for( int i = 0; i < totalTiles; i++ )
                tileBuffer[ i ] = reader.ReadInt32();                                                 // Read tiles into buffer

            int index = 0;
            for( int y = 0; y < ( int ) Size.y; y++ )
            for( int x = 0; x < ( int ) Size.x; x++ )
            foreach( var layer in layers )
                 tm.SetTile( x, y, ( int ) layer, tileBuffer[ index++ ] );                           // Set tiles from buffer

            tm.Build();                                                                              // Build map

            Message = reader.ReadString();                                                           // Load Message
            Script = reader.ReadString();                                                            // Load Script

            CubeData.Load();                                                                         // Load Cube Data

            return true;
        }
    }

    #if UNITY_EDITOR
    #region Convert // Use this function if you want to convert all cubes at once
    //int larguraAntiga = 16;
    //int larguraNova = 128;
    //int conv( int indiceAntigo )
    //{
    //    int linha = indiceAntigo / larguraAntiga;
    //    int coluna = indiceAntigo % larguraAntiga;

    //    int indiceNovo = linha * larguraNova + coluna;
    //    return indiceNovo;
    //}   
    [ButtonGroup( "6", 6 )]
    [Button( "Convert", ButtonSizes.Large ), GUIColor( 1, 0f, 0 )]
    public void ButtonConvert()
    {
        ConvertAllCubes();
    }
    public void ConvertAllCubes()
    {
        bool result = EditorUtility.DisplayDialog( "Prosseguir?", "Deseja realmente continuar?", "Yes", "No" );
        if( result == false ) return;
        result = EditorUtility.DisplayDialog( "Mesmo?", "Cuidado!!!!", "Yes", "No" );
        if( result == false ) return;
        Converting = true;
        MapSaver ms = GameObject.Find( "Areas Template Tilemap" ).
        GetComponent<MapSaver>();
        Map.I = GameObject.Find( "----------------Map" ).GetComponent<Map>();
        RandomMap rm = GameObject.Find( "----------------Random Map----------------" ).
        GetComponent<RandomMap>();
        GameObject go = GameObject.Find( "Areas Template Tilemap" );
        tk2dTileMap tm = go.GetComponent<tk2dTileMap>();
        string oldm = MapName;
        string oldfolder = FolderName;
        for( int rmid = 0; rmid < rm.RMList.Count; rmid++ )                                                                                       // Check for Duplicated Goal ID
        {
            string folder = Application.dataPath + "/Resources/Map Templates/" + rm.RMList[ rmid ].QuestHelper.SubFolder + "/" + rm.RMList[ rmid ].QuestHelper.Signature + "/";
            string[] fl = ES2.GetFiles( folder, "*.NEO" );
            List<string> list = fl.ToList();

            CubeData.available = rm.RMList[ rmid ].Available;
            CubeData.nm = rm.RMList[ rmid ].name;

            var sortedWords = from w in list orderby w select w;
            sortedWords = sortedWords.OrderBy( x => x.Length );

            for( int i = 0; i < list.Count; i++ )
            {
                MapName = list[ i ];
                //MapName = MapName.Replace( ".bytes", "" );
                FolderName = rm.RMList[ rmid ].QuestHelper.SubFolder + "/" + rm.RMList[ rmid ].QuestHelper.Signature;
                Debug.Log( "Converting Cube: " + list[ i ] + "    " + folder + "  " + rm.RMList[ rmid ].name );
                string file = Application.dataPath + "/Resources/Map Templates/" + FolderName + "/" + MapName;
                LoadMap( file, ref tm );
                SaveMap( file, ref tm );
            }
        }
        MapName = oldm;
        FolderName = oldfolder;
        Converting = false;
    }    
    #endregion
    #endif

    public string[] GetFiles()
    {
        string folder = Application.dataPath + "/Resources/Map Templates/" + FolderName + "/";
        string[ ] fl = ES2.GetFiles( folder, "*.NEO" );
        List<string> list = fl.ToList();

        var sortedWords = from w in list orderby w select w;
        sortedWords = sortedWords.OrderBy( x => x.Length );
        
        //Debug.Log("The sorted list of words:");
        //foreach( var w in sortedWords )
        //{
        //    Debug.Log( w );
        //}
        return sortedWords.ToArray();
    }

    public void PrevMapCallBack()
    {
        string[] files = GetFiles();
        if( CurrentCube != "" )
        {
            for( int i = 0; i < files.Length; i++ )
            if( files[ i ] == CurrentCube + ".NEO" )
            {
                if( i == 0 )
                {
                    CurrentCube = files[ files.Length - 1 ];
                }
                else
                    CurrentCube = files[ i - 1 ];
            }
        }
        else
            CurrentCube = "Cube 1";

        CurrentCube = CurrentCube.Replace( ".NEO", "" ); 
        MapName = CurrentCube;
        CurrentAdventureName = GetRM().RMList[ ( int ) CurrentAdventure ].name;
        MapQuestHelper hp = GetRM().RMList[ CurrentAdventure ].GetComponent<MapQuestHelper>();
        FolderName = hp.SubFolder + "/" + hp.Signature;
        Load();
        hp.UpdateExternaEditorInfo();
    }

    public void NextMapCallBack()
    {
        string[] files = GetFiles();
        if( CurrentCube != "" )
        {
            for( int i = 0; i < files.Length; i++ )
                if( files[ i ] == CurrentCube + ".NEO" )
                {
                    if( i == files.Length - 1 )
                    {
                        CurrentCube = files[ 0 ];
                    }
                    else
                        CurrentCube = files[ i + 1 ];
                }
        }
        else
            CurrentCube = "Cube 1";

        CurrentCube = CurrentCube.Replace( ".NEO", "" ); 
        MapName = CurrentCube;
        CurrentAdventureName = GetRM().RMList[ ( int ) CurrentAdventure ].name;
        MapQuestHelper hp = GetRM().RMList[ CurrentAdventure ].GetComponent<MapQuestHelper>();
        FolderName = hp.SubFolder + "/" + hp.Signature;   
        Load();
        hp.UpdateExternaEditorInfo();
    }

    public void PrevAdventureCallBack()
    {
        if( CurrentAdventure > 0 )
            CurrentAdventure -= 1;
        else CurrentAdventure = GetRM().RMList.Count - 1;

        CurrentCube = "";
        MapName = "Cube 1";
        CurrentAdventureName = GetRM().RMList[ ( int ) CurrentAdventure ].name;
        MapQuestHelper hp = GetRM().RMList[ CurrentAdventure ].GetComponent<MapQuestHelper>();
        FolderName = hp.SubFolder + "/" + hp.Signature;
        Load();
    }

    public void NextAdventureCallBack()
    {
        if( CurrentAdventure < GetRM().RMList.Count - 1 )
            CurrentAdventure += 1;
        else CurrentAdventure = 0;

        CurrentCube = "";
        MapName = "Cube 1";
        CurrentAdventureName = GetRM().RMList[ ( int ) CurrentAdventure ].name;
        MapQuestHelper hp = GetRM().RMList[ CurrentAdventure ].GetComponent<MapQuestHelper>();
        FolderName = hp.SubFolder + "/" + hp.Signature;
        Load();
    }


    public void TestQuestCallBack()
    {
        #if UNITY_EDITOR
        Helper.I = GameObject.Find( "Helper" ).GetComponent<Helper>();
        Helper.I.StartingAdventure = CurrentAdventure;
        string resultString = Regex.Match( CurrentCube, @"\d+" ).Value;
        if( Helper.I.StartingCube != -1 ) Helper.I.StartingCube = int.Parse( resultString );
        EditorApplication.ExecuteMenuItem( "Edit/Play" );
        RandomMapData rm = GetRM().RMList[ CurrentAdventure ];
        rm.QuestHelper.UpdateExternaEditorInfo();
        #endif 
    }

    public RandomMap GetRM()
    {
        return GameObject.Find( "----------------Random Map----------------" ).
        GetComponent<RandomMap>();
    }

    public void EditQuestDataCallBack()
    {
        #if UNITY_EDITOR
        MapQuestHelper hp = GetRM().RMList[ CurrentAdventure ].GetComponent<MapQuestHelper>();
        Selection.activeGameObject = hp.gameObject;
        var scene_view = UnityEditor.SceneView.lastActiveSceneView;
        scene_view.pivot = hp.transform.position;
        GameObject ob = GameObject.Find( "Farm" );
        Farm f = ob.GetComponent<Farm>();
        //f.UpdateListsCallBack();
        hp.UpdateExternaEditorInfo();
        #endif
    }

    public void DeleteMapCallBack()
    {
        string file = Application.dataPath + "/Resources/Map Templates/" + FolderName + "/" + CurrentCube + ".NEO";
        ES2.Delete( file );
        CurrentCube = "";
        MapName = "Cube 1";
        Debug.Log( "Cube Deleted! " + file );
    }

    public void AddFogCallBack()
    {
        tk2dTileMap tm = GetRM().AreasTM;
        for( int y = 0; y < ( int ) Size.y; y++ )
        for( int x = 0; x < ( int ) Size.x; x++ )
            {
                ETileType raft = ( ETileType ) tm.GetTile( x, y, ( int ) ELayerType.RAFT );
                if( raft == ( ETileType.FOG ) )
                    tm.SetTile( x, y, ( int ) ELayerType.RAFT, ( int ) ETileType.NONE );
            }
        tm.ForceBuild();
        if( RemoveFog ) return;

        for( int y = 0; y < ( int ) Size.y; y++ )
        for( int x = 0; x < ( int ) Size.x; x++ )
            {
                ETileType mn = ( ETileType ) tm.GetTile( x, y, ( int ) ELayerType.MONSTER );
                ETileType raft = ( ETileType ) tm.GetTile( x, y, ( int ) ELayerType.RAFT );
                ETileType ga = ( ETileType ) tm.GetTile( x, y, ( int ) ELayerType.GAIA );
                if( raft == ( ETileType.NONE ) )
                {
                    if( ga == ETileType.FOREST || ( ga == ETileType.WATER && FogOnWater ) || ga == ETileType.PIT )
                    if( mn == ETileType.NONE )
                    if( Util.Chance( FogChance ) )
                        tm.SetTile( x, y, ( int ) ELayerType.RAFT, ( int ) ETileType.FOG );
                }
            }
        tm.ForceBuild();
        Debug.Log( "Fog Added! " );
    }

    public void MoveUpCallBack()
    {
        string next = "";
        string[ ] files = GetFiles();
        if( CurrentCube != "" )
        {
            for( int i = 0; i < files.Length; i++ )
                if( files[ i ] == CurrentCube + ".NEO" )
                {
                    if( i == files.Length - 1 )
                    {
                        next = files[ 0 ];
                    }
                    else
                        next = files[ i + 1 ];
                }
        }
        else
            next = "Cube 1";

        SwitchMap( CurrentCube + ".NEO", next );
        CurrentCube = next.Replace( ".NEO", "" );
        MapName = CurrentCube;
        CurrentAdventureName = GetRM().RMList[ ( int ) CurrentAdventure ].name;
        MapQuestHelper hp = GetRM().RMList[ CurrentAdventure ].GetComponent<MapQuestHelper>();
        FolderName = hp.SubFolder + "/" + hp.Signature;
        Load();
        hp.UpdateExternaEditorInfo();
    }
    public void MoveDownCallBack()
    {
        string prev = "";
        string[ ] files = GetFiles();
        if( CurrentCube != "" )
        {
            for( int i = 0; i < files.Length; i++ )
                if( files[ i ] == CurrentCube + ".NEO" )
                {
                    if( i == 0 )
                    {
                        prev = files[ files.Length - 1 ];
                    }
                    else
                        prev = files[ i - 1 ];
                }
        }
        else
            prev = "Cube 1";

        SwitchMap( CurrentCube + ".NEO", prev );
        CurrentCube = prev.Replace( ".NEO", "" );
        MapName = CurrentCube;
        CurrentAdventureName = GetRM().RMList[ ( int ) CurrentAdventure ].name;
        MapQuestHelper hp = GetRM().RMList[ CurrentAdventure ].GetComponent<MapQuestHelper>();
        FolderName = hp.SubFolder + "/" + hp.Signature;
        Load();
        hp.UpdateExternaEditorInfo();
    }

    public bool SwitchMap( string map1, string map2 )
    {
        string file1 = Application.dataPath + "/Resources/Map Templates/" + FolderName + "/" + map1;
        string file2 = Application.dataPath + "/Resources/Map Templates/" + FolderName + "/" + map2;
        string temp = Application.dataPath + "/Resources/Map Templates/" + FolderName + "/" + "temp.NEO";
        ES2.Rename( file1, temp );
        ES2.Rename( file2, file1 );
        ES2.Rename( temp, file2 );
        return true;
    }

    internal void UpdateIntroMessage( string msg = "", string title = "" )
    {
        if( msg != "" )
        {
            //rrmsg = msg.Replace("\n", "");
            UI.I.SelectedPerk = EPerkType.NONE;
            UI.I.PerkInfoDescriptionText.text = msg;                                     // Show initial cube message
            UI.I.PerkInfoIconBack.gameObject.SetActive( false );
            UI.I.PerkInfoDescriptionText.color = Color.green;
            UI.I.PerkInfoDescriptionText.transform.localPosition = new Vector3( -160f, -113f, 0f );
            UI.I.PerkInfoDescriptionText.width = 406;
            UI.I.PerkInfoDescriptionText.height = 148;
            UI.I.PerkInfoTitleText.text = title;
        }
        else
        {
            UI.I.SelectedPerk = EPerkType.MOVEMENTLEVEL;
            UI.I.PerkInfoDescriptionText.transform.localPosition = new Vector3( -118, -113f, 0f );
            UI.I.PerkInfoDescriptionText.width = 314;
            UI.I.PerkInfoDescriptionText.height = 148;
            UI.I.PerkInfoDescriptionText.text = msg;
            UI.I.PerkInfoTitleText.text = title;
            UI.I.ForceUpdateUI = true;
        }
    }
}
