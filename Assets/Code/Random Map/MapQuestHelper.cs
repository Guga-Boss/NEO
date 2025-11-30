using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Runtime.InteropServices.ComTypes;
using Sirenix.OdinInspector.Demos;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class MapQuestHelper: MonoBehaviour
{
#if UNITY_EDITOR

    [HorizontalGroup( "Split", 0.3f )]
    [Button( "Edit Cube...", ButtonSizes.Large ), GUIColor( 1f, 0.52f, 0.1f )]
    public void Button1()
    {
        EditTileMapCallBack( );
    }

    [HorizontalGroup( "Split", 0.3f )]
    [Button( "Test...", ButtonSizes.Large ), GUIColor( 0, 1, 0 )]
    public void Button2()
    {
        TestMap( );
    }
    [HorizontalGroup( "Split", 0.1f )]
    [Button( "GOAL", ButtonSizes.Large ), GUIColor( 1f, 1f, 0 )]
    public void ButtonGoal()
    {
        RandomMapData rm = GetComponent<RandomMapData>( );
        Selection.activeGameObject = rm.GoalList[ 0 ].gameObject;
    }

    [HorizontalGroup( "Split", 0.1f )]
    [Button( "MOD", ButtonSizes.Large ), GUIColor( 1f, 1f, 0 )]
    public void ButtonMod()
    {
        RandomMapData rm = GetComponent<RandomMapData>( );
        Selection.activeGameObject = rm.GetComponentInChildren<Mod>( ).gameObject;
    }

    [HorizontalGroup( "Split", 0.1f )]
    [Button( "GO", ButtonSizes.Large ), GUIColor( 0, 1, 1 )]
    public void ButtonGo()
    {
        Helper.I = GameObject.Find( "Helper" ).GetComponent<Helper>( );
        Selection.activeGameObject = Helper.I.gameObject;
    }


    /// <summary>
    /// ///////////////////////////////////////////////////////////////////////////////
    /// </summary>

    [HorizontalGroup( "Split 2", 0.1f )]
    [Button( "Down", ButtonSizes.Small ), GUIColor( 1, 0, 1 )]
    public void ButtonCopyToResource()
    {
        UpdateData();
        RandomMapData rm = GetComponent<RandomMapData>( );
        int id = UpdateCurrentAdventure( );
        CopyToResource( id );
        UpdateData();
    }
    public void CopyToResource( int adv )
    {
        Helper.I = GameObject.Find( "Helper" ).GetComponent<Helper>( );
        Map.I = GameObject.Find( "----------------Map" ).GetComponent<Map>();
        RandomMapData rm = Map.I.RM.RMList[ adv ];

        SectorDefinition[] sl = rm.gameObject.GetComponentsInChildren<SectorDefinition>( );
        Map.I.RM.RMList[ adv ].SDList = new SectorDefinition[ sl.Length ];
        GameObject go = MoveDown( "Images", rm.ImagesFolder, rm );
        if( go )
        {
            rm.ImagesFolder = go; ;
        }

        for( int i = 0; i < sl.Length; i++ )
        {
            go = MoveDown( "Sector " + ( i + 1 ), sl[ i ].gameObject, rm );
            SectorDefinition sd = go.gameObject.GetComponent<SectorDefinition>();
            Map.I.RM.RMList[ adv ].SDList[ i ] = sd;
        }

        for( int g = 0; g < rm.GoalList.Length; g++ )                                          // saves blueprint ID since unity doesnt keep reference to objects in prefabs
        {
            RandomMapGoal gl = rm.GoalList[ g ];
            gl.TargetBluePrintID = "";
            for( int i = 0; i < Map.I.Farm.BluePrintList.Count; i++ )
            {
                if( gl.TargetBluePrint )
                if( gl.TargetBluePrint == Map.I.Farm.BluePrintList[ i ] )
                    gl.TargetBluePrintID = Map.I.Farm.BluePrintList[ i ].UniqueID;
            }

            gl.TargetRecipeBuildingID = "";
            gl.TargetRecipeID = "";

            if( gl.TargetRecipe )
            {
                for( int b = 0; b < Map.I.Farm.BuildingList.Count; b++ )
                {
                    var bld = Map.I.Farm.BuildingList[ b ];
                    if( bld == null ) continue;

                    for( int r = 0; r < bld.RecipeList.Count; r++ )
                    {
                        var rec = bld.RecipeList[ r ];
                        if( rec == null ) continue;

                        if( gl.TargetRecipe == rec )
                        {
                            gl.TargetRecipeBuildingID = bld.UniqueID;
                            gl.TargetRecipeID = rec.UniqueID;                              
                        }
                    }
                }
            }
        }

        go = MoveDown( "Goals", rm.GoalsFolder, rm );
        if( go )
        {
            RandomMapGoal[] rmg = go.gameObject.GetComponentsInChildren<RandomMapGoal>();
            rm.GoalsFolder = go;
            Map.I.RM.RMList[ adv ].GoalList = rmg;
            RandomMapGoal.RestoreGoalPrefabReferences( rm );
        }

        go = MoveDown( "Adventure Upgrade Info List", rm.UpgradeListFolder, rm );
        if( go )
        {
            rm.UpgradeListFolder = go;
            AdventureUpgradeInfo[] rmg = go.gameObject.GetComponentsInChildren<AdventureUpgradeInfo>();
            Map.I.RM.RMList[ adv ].AdventureUpgradeInfoList = rmg;
        }

        go = MoveDown( "Tech Tree", rm.TechListFolder, rm );
        if( go )
        {
            rm.TechListFolder = go;
        }

        rm.IsUp = false;
        go = MoveDown( "Quest Data Backup", rm.gameObject, rm, false );                                    // Saves a Backup for Safety
    }
    private GameObject MoveDown( string nm, GameObject folder, RandomMapData rm, bool destroy = true )
    {
        string path = "Assets/Resources/Map Templates/" + rm.QuestHelper.SubFolder;
        string dt = "/" + rm.QuestHelper.Signature + "/DATA/";
        System.IO.Directory.CreateDirectory( path + dt );

        if( folder )
        {
            GameObject go = PrefabUtility.CreatePrefab( path + dt + nm + ".prefab",
            folder, ReplacePrefabOptions.ReplaceNameBased );
            if( go ) 
            {
                if( destroy ) DestroyImmediate( folder ); 
                return go; 
            }
        }
        return null;
    }

    [HorizontalGroup( "Split 2", 0.1f )]
    [Button( "Up", ButtonSizes.Small ), GUIColor( 1, 0, 1 )]
    public void ButtonCopyToHierarchy()
    {
        UpdateData( );
        RandomMapData rm = GetComponent<RandomMapData>( );
        int id = UpdateCurrentAdventure( );
        CopyToHierarchy( id );
        UpdateData();
    }

    public void CopyToHierarchy( int adv )
    {
        Helper.I = GameObject.Find( "Helper" ).GetComponent<Helper>( );
        Map.I = GameObject.Find( "----------------Map" ).GetComponent<Map>();
        RandomMapData rm = Map.I.RM.RMList[ adv ];

        GameObject go = MoveUp( "Images", rm );
        if( go )
        {
            rm.ImagesFolder = go;
        }

        List<SectorDefinition> sd = new List<SectorDefinition>();
        for( int i = 0; i < 5; i++ )
        {
            go = MoveUp( "Sector " + ( i + 1 ), rm );
            if( go == null ) break;
            sd.Add( go.GetComponent<SectorDefinition>() );
        }
        rm.SDList = sd.ToArray();

        go = MoveUp( "Goals", rm );
        if( go )
        {
            rm.GoalsFolder = go;
            RandomMapGoal[] rmg = Map.I.RM.RMList[ adv ].gameObject.GetComponentsInChildren<RandomMapGoal>( );
            Map.I.RM.RMList[ adv ].GoalList = rmg;
            RandomMapGoal.RestoreGoalPrefabReferences( rm );
        }

        go = MoveUp( "Adventure Upgrade Info List", rm );
        if( go )
        {
            rm.UpgradeListFolder = go;
            AdventureUpgradeInfo[] rmg = Map.I.RM.RMList[ adv ].gameObject.GetComponentsInChildren<AdventureUpgradeInfo>( );
            Map.I.RM.RMList[ adv ].AdventureUpgradeInfoList = rmg;
        }

        go = MoveUp( "Tech Tree", rm );
        if( go )
        {
            rm.TechListFolder = go;
        }
        rm.IsUp = true;
    }

    private GameObject MoveUp( string nm, RandomMapData rm )
    {
        string path = "Map Templates/" + rm.QuestHelper.SubFolder;
        string dt = "/" + rm.QuestHelper.Signature + "/DATA/";

        UnityEngine.Object obj = Resources.Load( path + dt + nm ); // Load generic Object first
        if( obj != null && obj is GameObject ) // Check type safely
        {
            GameObject go = GameObject.Instantiate( obj as GameObject ); // Instantiate safely
            go.transform.parent = rm.transform; // Set parent
            go.name = nm;

            string app = Application.dataPath + "/Resources/";
            FileUtil.DeleteFileOrDirectory( app + path + dt + nm + ".prefab" ); // delete old prefab
            AssetDatabase.Refresh(); // Refresh editor

            return go;
        }

        return null; // return null if load fails
    }

    private static void UpdateData()
    {
        GameObject ob = GameObject.Find( "Farm" );
        Farm f = ob.GetComponent<Farm>( );
        f.UpdateListsCallBack( );
    }

    [HorizontalGroup( "Split 2", 0.2f )]
    [Button( "All Down", ButtonSizes.Small ), GUIColor( 1, 0, 1 )]
    public void ButtonCopyAllToResource()
    {
        Debug.Log( "Disabled." );
        return;
        UpdateData();
        Debug.Log( "Start All Down: Total: " + Map.I.RM.RMList.Count );
        for( int i = 0; i < Map.I.RM.RMList.Count; i++ )
            CopyToResource( i );
        UpdateData();
        Debug.Log( "All Down!" );
    }

    [HorizontalGroup( "Split 2", 0.2f )]
    [Button( "All Up", ButtonSizes.Small ), GUIColor( 1, 0, 1 )]
    public void ButtonCopyAllToHierarchy()
    {
        Debug.Log( "Disabled." );
        return;
        UpdateData();
        Debug.Log( "Start All Up: Total: " + Map.I.RM.RMList.Count );
        for( int i = 0; i < Map.I.RM.RMList.Count; i++ )
            CopyToHierarchy( i );
        UpdateData();
        Debug.Log( "All Up!" );
    }
#endif

    [HideInInspector]
    public RandomMapData MapData;
    [HideInInspector]
    public tk2dSprite MonsterSprite, NestSprite;
    [HideInInspector]
    public tk2dSlicedSprite BackgroundSprite;
    [HideInInspector]
    public tk2dTextMesh QuestNameMesh;
    [HideInInspector]
    public QuestPanel QuestPanel;
    public string QuestName;
    public string Signature = "";
    public string SubFolder = "";
    public ETileType NavigationMapIcon = ETileType.SCARAB;


    public void EditTileMapCallBack()
    {
        GameObject ob = GameObject.Find( "Farm" );
        Farm f = ob.GetComponent<Farm>( );
        //f.UpdateListsCallBack();
        GameObject go = GameObject.Find( "----------------Navigation Map" );
        NavigationMap nm = go.GetComponent<NavigationMap>( );
        RandomMapData rm = GetComponent<RandomMapData>( );

        MapSaver ms = MapSaver.Get( );
        ms.FolderName = rm.QuestHelper.SubFolder + "/" + Signature;
        ms.CurrentAdventure = rm.QuestID;
        ms.CurrentAdventureName = rm.QuestHelper.QuestName;
#if UNITY_EDITOR
        Selection.activeGameObject = nm.AreasTilemap;
        var scene_view = UnityEditor.SceneView.lastActiveSceneView;
        scene_view.pivot = nm.AreasTilemap.transform.position + new Vector3( 15, 15, 0 );
        nm.AreasTilemap.transform.parent = rm.gameObject.transform;
#endif

        bool res = MapSaver.Get( ).Load( false );

        if( res == false )
        {
            ms.MapName = "Cube 1";
            ms.CurrentCube = "Cube 1";
            MapSaver.Get( ).Load( );
        }
        UpdateExternaEditorInfo( );
    }

    public void UpdateExternaEditorInfo()
    {
        RandomMapData rm = GetComponent<RandomMapData>( );
        Helper.I = GameObject.Find( "Helper" ).GetComponent<Helper>( );
        Helper.I.ModDescriptionList = Helper.I.GetModDescriptionText( );


        Helper.I.ModDescriptionList += Helper.I.GetOriListText();
        MapSaver ms = MapSaver.Get( );
        string str = "" + ms.LastMapSavedFilename + "\n" +
        rm.QuestID + "\n" +                                                  // Saves information for the external editor
        ms.FolderName + "\n" +
        ms.MapName + "\n" +
        ms.CurrentAdventureName + "\n\n" + Helper.I.ModDescriptionList;

        Util.WriteToDesktop( str, "Last Edited Cube.txt" );
    }

    public void TestMap()
    {
        MapSaver ms = GameObject.Find( "Areas Template Tilemap" ).GetComponent<MapSaver>( );
        Helper.I = GameObject.Find( "Helper" ).GetComponent<Helper>( );
        RandomMapData rm = GetComponent<RandomMapData>( );
        Helper.I.StartingAdventure = rm.QuestID;
#if UNITY_EDITOR
        EditorApplication.ExecuteMenuItem( "Edit/Play" );
#endif
    }

    public int UpdateCurrentAdventure()
    {
        int id = -1;
#if UNITY_EDITOR  
        Map.I = GameObject.Find( "----------------Map" ).GetComponent<Map>( );
        string nm = "";
        for( int i = 0; i < Map.I.RM.RMList.Count; i++ )
        {
            if( Map.I.RM.RMList[ i ].name == Selection.activeGameObject.name )
            { nm = Map.I.RM.RMList[ i ].name; id = i; }
        }
        MapSaver ms = MapSaver.Get( );
        ms.CurrentAdventure = id;
        ms.CurrentAdventureName = nm;
#endif
        return id;
    }
}
