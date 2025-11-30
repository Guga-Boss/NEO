using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Reflection;

[CustomEditor( typeof( Artifact ) )]
public class ArtifactEditor : EditorWindow
{
    int SelLevel = -1;
    int SelDungeon = -1;
    GameObject SelArtifact = null;
    Vector2 scrollPosition = Vector2.zero;

    // Add menu item named "My Window" to the Window menu
    [MenuItem( "Window/Artifact Editor" )]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow( typeof( ArtifactEditor ) );
    }

    [MenuItem( "Helper/Select Helper _F7" )]
    public static void SelectHelper()
    {
        Selection.activeGameObject = GameObject.Find( "Helper" );
    }

    [MenuItem( "Helper/Update Data" )]
    public static void UpdateData()
    {
        GameObject ob = GameObject.Find( "Farm" );
        Farm f = ob.GetComponent<Farm>();
        f.UpdateListsCallBack();
    }

    //[MenuItem( "Helper/Select Images" )]
    //public static void SelectImages()
    //{  
    //    Type pt= Assembly.GetAssembly( typeof( Editor ) ).GetType( "UnityEditor.SceneAsset" );
    //    EditorWindow.GetWindow( pt ).Show();
               
    //}

    [MenuItem( "Window/Delete Saves" )]
    public static void DeleteSaves()
    {
        GameObject go = GameObject.Find( "----------------Game Manager---------" );
        Manager.I = go.GetComponent<Manager>();
        string file = Manager.I.GetProfileFolder();
        System.IO.Directory.Delete( file, true );
    }

    void OnHierarchyChange() //Called whenever the scene hierarchy has changed.
    {
        //Debug.Log( "Sync" );
    }
    void OnSelectionChange()
    {
        //Debug.Log( "Sync" );
        Repaint();
    }

    public void DrawLevelSelector()
    {
        Color defaulcol = GUI.color;

        //GUILayout.Space( 20 );
        //if( GUILayout.Button( "Create Table", GUILayout.Width( 100 ), GUILayout.Height( 40 ) ) )
        //{
        //    GameObject.Find("Rand Table").GetComponent<RandValues>().InitRandTable();
        //}

        //GUILayout.Space( 20 );
        //if( GUILayout.Button( "Translate", GUILayout.Width( 100 ), GUILayout.Height( 40 ) ) )
        //{
        //    LanguageEditor ed = new LanguageEditor();
        //    string gDocsURL = EditorPrefs.GetString( PlayerSettings.productName + "gDocs" );

        //    gDocsURL = gDocsURL.Trim();
        //    if( gDocsURL.Contains( "&output" ) )
        //    {
        //        //OLD FORMAT
        //        if( !gDocsURL.Contains( "&single=" ) )
        //            gDocsURL += "&single=true";
        //        if( !gDocsURL.Contains( "&gid=" ) )
        //            gDocsURL += "&gid=0";
        //        if( !gDocsURL.Contains( "&output=csv" ) )
        //            gDocsURL += "&output=csv";
        //        if( gDocsURL.Contains( "&output=html" ) )
        //            gDocsURL = gDocsURL.Replace( "&output=html", "&output=csv" );
        //    }
        //    if( !gDocsURL.Contains( ".google.com" ) )
        //    {
        //        EditorUtility.DisplayDialog( "Error", "You have entered an incorrect spreadsheet URL. Please read the manuals instructions (See readme.txt)", "OK" );
        //    }
        //    else
        //    {
        //        EditorPrefs.SetString( PlayerSettings.productName + "gDocs", gDocsURL );
        //        ed.LoadSettings( gDocsURL );
        //    }
        //}

        EditorGUILayout.BeginVertical();
        GUILayout.Space( 20 );
        int oldLevel = SelLevel;

        bool dungeonChosen = false, levelChosen = false; 

		for( int l = 0; l < Quest.I.LevelList.Length; l++ )
		{
			if( SelLevel != l &&
			   GUILayout.Button( "Level " + ( l + 1 ), GUILayout.Width( 100 ), GUILayout.Height( 20 ) ) )
			{
				SelLevel = l;
                levelChosen = true;
                SelDungeon = -1;
			}
			else
				if( SelLevel == l )
				{
					GUI.color = Color.yellow;
					if( GUILayout.Button( "Level " + ( l + 1 ), GUILayout.Width( 100 ), GUILayout.Height( 20 ) ) )
					{
						SelLevel = l;
                        levelChosen = true;
                        SelDungeon = -1;
					}
				}
            GUI.color = defaulcol;
        }


        EditorGUILayout.EndVertical();
        GUILayout.Space( 20 );     
       
        EditorGUILayout.BeginVertical();
        GUILayout.Space( 20 );
        int oldDungeon = SelDungeon;

        for( int l = 0; l < Quest.I.LabList.Length; l++ )
        {
            if( SelDungeon != l &&
               GUILayout.Button( "Lab " + ( l + 1 ), GUILayout.Width( 100 ), GUILayout.Height( 20 ) ) )
            {
                SelDungeon = l;
                dungeonChosen = true;
                SelLevel = -1;
            }
            else
                if( SelDungeon == l )
                {
                    GUI.color = Color.yellow;
                    if( GUILayout.Button( "Lab " + ( l + 1 ), GUILayout.Width( 100 ), GUILayout.Height( 20 ) ) )
                    {
                        SelDungeon = l;
                        dungeonChosen = true;
                        SelLevel = -1;
                    }
                }
            GUI.color = defaulcol;
        }
        
        if( oldLevel != SelLevel || dungeonChosen  )
        {
            Quest.I = GameObject.Find( "Quest" ).GetComponent<Quest>();
            for( int i = 0; i < Quest.I.LevelList.Length; i++ ) Quest.I.LevelList[ i ].gameObject.SetActive( false );
        }

        if( oldLevel != SelLevel )
            if( SelLevel != -1 )
            {
                Selection.activeGameObject = Quest.I.LevelList[ SelLevel ].Tilemap.gameObject;
                Quest.I.LevelList[ SelLevel ].gameObject.SetActive( true );
                Quest.CurrentLevel = SelLevel;
                Manager.I.StartingLevel = SelLevel;
                GameObject.Find( "Game Manager" ).GetComponent<Manager>().StartingLevel = SelLevel;
            }
        

        if( oldDungeon != SelDungeon || levelChosen)
        {
            Quest.I = GameObject.Find( "Quest" ).GetComponent<Quest>();
            for( int i = 0; i < Quest.I.LabList.Length; i++ ) Quest.I.LabList[ i ].gameObject.SetActive( false );
        }

        if( oldDungeon != SelDungeon )
            if( SelDungeon != -1 )
            {
                Selection.activeGameObject = Quest.I.LabList[ SelDungeon ].Tilemap.gameObject;
                Quest.I.LabList[ SelDungeon ].gameObject.SetActive( true );
                Quest.CurrentDungeon = SelDungeon;
            }




        EditorGUILayout.EndVertical();
    }

    //public void Update()
    //{
    ////    //Debug.Log( "CurrentLevel " + Quest.CurrentLevel );
    //    UI.I = GameObject.Find( "UI" ).GetComponent<UI>(); // this code resizes all icons sizes
    //    for( int i = 0; i < UI.I.PerkList.Count; i++ )
    //    {
    //        UI.I.PerkList[ i ].Label.transform.localPosition = new Vector3( 31.9f, -27.1f, 0 );
    //        UI.I.PerkList[ i ].Label.width = 135;
    //        UI.I.PerkList[ i ].Label.height = 41;
    //        UI.I.PerkList[ i ].Label.fontSize = 36;
    //        UI.I.PerkList[ i ].Label.pivot = UIWidget.Pivot.Right;
    //        UI.I.PerkList[ i ].Label.text = "Info";
    //        for( int x = 0; x < UI.I.PerkList[ i ].Sprite.Length; x++ )
    //            if( UI.I.PerkList[ i ].Sprite[ x ] != null )
    //                UI.I.PerkList[ i ].Sprite[ x ].width = UI.I.PerkList[ i ].Sprite[ x ].height = 70;
    //        //Debug.Log( "id " + i );

    //        UIButton bt = UI.I.PerkList[ i ].gameObject.GetComponent<UIButton>();

    //        if( bt )
    //        {
    //            bt.hoverSprite = "Perk Background";
    //            bt.pressedSprite = "Perk Background";
    //        }

    //        UISprite hh = UI.I.PerkList[ i ].gameObject.GetComponent<UISprite>();

    //        hh.width = hh.height = 88;


    //    }


        ////if( Input.GetKeyDown( KeyCode.Space ) )
        //{
        //    GameObject[] go = GameObject.FindGameObjectsWithTag( "Artifact" );

        //    Debug.Log(go.Length);

        //    for( int i = 0; i < go.Length; i++ )
        //    {
        //        Artifact ar = go[i].GetComponent<Artifact>();

        //        if( ar )
        //        {
        //        //Debug.Log( go[i] + "  " + ar.BelongingLevel );

        //            if( ar.PrefabName == "ImproveMemory" ) ar.Category = Artifact.ECategory.SECONDARY;
        //            if( ar.PrefabName == "ImproveScout" ) ar.Category = Artifact.ECategory.SECONDARY;
        //            if( ar.PrefabName == "ImproveToolbox" ) ar.Category = Artifact.ECategory.SECONDARY;
        //            if( ar.PrefabName == "Star_1" ) ar.Category = Artifact.ECategory.SECONDARY;
        //            if( ar.PrefabName == "Star_2" ) ar.Category = Artifact.ECategory.SECONDARY;
        //            if( ar.PrefabName == "Star_3" ) ar.Category = Artifact.ECategory.SECONDARY;
        //        }

        //    }
        //}
    //}

    public void OnGUI()
    {
        if( Application.isPlaying ) return;

        if( UpdateUnitWindow() ) return;
        DrawLevelSelector();
        UpdatePerkWindow();
    }

    public bool UpdateUnitWindow()
    {
        if( Selection.activeGameObject == null ) return false;
        Unit un = Selection.activeGameObject.GetComponent<Unit>();
        if( un == null ) return false;
        if( un.gameObject.tag != "Monster" ) return false;
        if( un.Body == null ) return false;

        GUILayout.Space( 20 );
        un.Body.Level = ( int ) EditorGUILayout.IntSlider( "Level", ( int ) un.Body.Level, 1, 100 );
        un.LevelTxt.text = "" + un.Body.Level;
        PrefabUtility.DisconnectPrefabInstance( un.gameObject );
        SceneView.RepaintAll();
        return true;
    }

    public bool DrawArtifactEditor( GameObject obj, bool bEnemy )
    {
        if( obj == null ) return false;
        return true;
    }

    public void SetArtifact( Artifact art )
    {
        foreach( GameObject ob in Selection.objects )
        {
            Artifact artl = ob.GetComponent<Artifact>();
            if( artl )
            {
                artl.Copy( art );
                //artl.TargetUnitName = art.transform.GetComponentInChildren<tk2dTextMesh>();
            }
        }
    }
      

    public void UpdatePerkWindow()
    {
        Color defaulcol = GUI.color;

        if( Selection.activeGameObject == null ) return;
        if( Selection.activeGameObject.tag != "Artifact" ) return;
        SelArtifact = Selection.activeGameObject;

        GUILayout.Space( 10 );
        if( GUILayout.Button( SelArtifact.name, GUILayout.Width( 100 ), GUILayout.Height( 40 ) ) ) { }
        Artifact ar = SelArtifact.GetComponent<Artifact>();
        if( ar == null ) return;
        GUILayout.Space( 10 );

        //ar.Trigger.Type = ( EPerkType ) EditorGUILayout.EnumPopup(new GUIContent("Perk Type", ""), ar.Trigger.Type );

		if( PrefabUtility.GetPrefabParent( ar.gameObject ) == null && PrefabUtility.GetPrefabObject( ar.gameObject ) != null )	{}
		else
        PrefabUtility.DisconnectPrefabInstance( ar.gameObject );

        EArtifactDataBase art = EArtifactDataBase.ARTIFACT;

        art = ( EArtifactDataBase ) EditorGUILayout.EnumPopup( new GUIContent( "Load Artifact", "Load Artifact" ), art );

        if( art != EArtifactDataBase.ARTIFACT )
        {
            GameObject res = ( GameObject ) Resources.Load( "Artifacts/" + art );

            if( res )
            {
                Debug.Log( "Artifacts/" + art + " Loaded." );
                SetArtifact( res.GetComponent<Artifact>() );
            }
            else Debug.Log( "Artifacts/" + art + " Could not be found." );
        }
                
        GUILayout.Space( 10 );
        ar.Multiplier = (Artifact.EMultiplier) EditorGUILayout.EnumPopup( new GUIContent( "Multiplier", "x1" ), ar.Multiplier );

        GUILayout.Space( 10 );
        ar.TargetHero = ( EHeroID ) EditorGUILayout.EnumPopup( new GUIContent( "Target Hero", "Target Hero" ), ar.TargetHero );

        if( ar.TargetUnitName != null )              // updates target hero text mesh
		{
			if( ar.TargetHero == EHeroID.ALL_HEROES )
				ar.TargetUnitName.text = "All";

			if( ar.TargetHero == EHeroID.CURRENT_HERO )
				ar.TargetUnitName.text = "Active";

			if( ( int ) ar.TargetHero >= ( int ) EHeroID.KADE && ( int ) ar.TargetHero <= ( int ) EHeroID.HERO_4 )
				ar.TargetUnitName.text = HeroData.I.HeroNameList[ ( int ) ar.TargetHero ];

            if( ar.Multiplier != Artifact.EMultiplier.x1 )
            ar.TargetUnitName.text += " " + ar.Multiplier.ToString();
		}

        GUILayout.Space( 10 );
        ar.LifeTime = ( Artifact.EArtifactLifeTime ) EditorGUILayout.EnumPopup( new GUIContent( "Lifetime", "Lifetime" ), ar.LifeTime );

        GUILayout.Space( 10 );
        ar.Trigger.ConditionVarID = ( ETriggerVarID ) EditorGUILayout.EnumPopup( new GUIContent( "Condition Variable", "Condition Variable" ), ar.Trigger.ConditionVarID );

        if( ar.Trigger.ConditionVarID != ETriggerVarID.NONE )
        {
            ar.Trigger.ConditionOperator = ( ETriggerCondOperator ) EditorGUILayout.EnumPopup( new GUIContent( "Condition Operator", "Condition Operator" ), ar.Trigger.ConditionOperator );
            ar.Trigger.ConditionVal1 = EditorGUILayout.Slider( "Condition Value", ar.Trigger.ConditionVal1, -1000, 1000 );
        }
        GUILayout.Space( 10 );

        ar.Trigger.EffectVarID = ( ETriggerVarID ) EditorGUILayout.EnumPopup( new GUIContent( "Effect Variable", "Effect Variable" ), ar.Trigger.EffectVarID );
        if( ar.Trigger.EffectVarID != ETriggerVarID.NONE )
        {
            ar.Trigger.EffectOperator = ( ETriggerEffOperator ) EditorGUILayout.EnumPopup( new GUIContent( "Effect Operator", "Effect Operator" ), ar.Trigger.EffectOperator );

            ar.Trigger.EffectVarID1 = ( ETriggerVarID ) EditorGUILayout.EnumPopup( new GUIContent( "Effect Var ID", "Effect Var ID" ), ar.Trigger.EffectVarID1 );
            if( ar.Trigger.EffectVarID1 == ETriggerVarID.NONE )
                ar.Trigger.EffectVal1 = EditorGUILayout.Slider( "Effect Value 1", ar.Trigger.EffectVal1, -1000, 1000 );

            ar.Trigger.EffectOperator2 = ( ETriggerEffOperator ) EditorGUILayout.EnumPopup( new GUIContent( "Effect Operator 2", "Effect Operator 2" ), ar.Trigger.EffectOperator2 );
            if( ar.Trigger.EffectOperator2 != ETriggerEffOperator.NONE )
            {
                ar.Trigger.EffectVarID2 = ( ETriggerVarID ) EditorGUILayout.EnumPopup( new GUIContent( "Effect Var ID 2", "Effect Var ID 2" ), ar.Trigger.EffectVarID2 );
                if( ar.Trigger.EffectVarID2 == ETriggerVarID.NONE )
                    ar.Trigger.EffectVal2 = EditorGUILayout.Slider( "Effect Value 2", ar.Trigger.EffectVal2, -1000, 1000 );

                ar.Trigger.EffectOperator3 = ( ETriggerEffOperator ) EditorGUILayout.EnumPopup( new GUIContent( "Effect Operator 3", "Effect Operator 3" ), ar.Trigger.EffectOperator3 );
                if( ar.Trigger.EffectOperator3 != ETriggerEffOperator.NONE )
                {
                    ar.Trigger.EffectVarID3 = ( ETriggerVarID ) EditorGUILayout.EnumPopup( new GUIContent( "Effect Var ID 3", "Effect Var ID 3" ), ar.Trigger.EffectVarID3 );
                    if( ar.Trigger.EffectVarID2 == ETriggerVarID.NONE )
                        ar.Trigger.EffectVal3 = EditorGUILayout.Slider( "Effect Value 3", ar.Trigger.EffectVal3, -1000, 1000 );
                }
            }
        }

        GUILayout.Space( 40 );
        tk2dSprite targetComp = ar.Sprite;
        if( targetComp != null )
        {
            var editor = Editor.CreateEditor( targetComp );
            editor.OnInspectorGUI();
        }
    }
}


//		int SelEn = -1;
//		for(int i = 0; i < 10; i++)
//		if( obj.name == "Map Enemy " + i ) { SelEn = i; break; } else if( i == 9 ) return;
//
//		if( GUILayout.Button("Edit Map", GUILayout.Height(30)))
//		{
//			obj.GetComponent<MapEnemy>().TilemapObj.SetActive( true );
//			Selection.activeGameObject = obj.GetComponent<MapEnemy>().TilemapObj;
//		}
//		
//		//scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true,  GUILayout.Width(300),  GUILayout.Height(800)); 
//		int cont = 0;
//		int nextfree = -1;
//		GUILayout.Space(10);
//		
//		Color defaulcol = GUI.color;
//		GUI.color = defaulcol;
//		GUILayout.Space(35);
//		
//		EditorGUILayout.BeginHorizontal();
//		for( int i = 1; i < 30; i++ ) 
//		{
//			if ( obj.transform.FindChild("Enemy Artifact " + i ) ) 
//			{
//				cont++;	
//				if( SelArt != i )
//					if( GUILayout.Button("Artifact " + i, GUILayout.Width(100), GUILayout.Height(20) ) ) SelArt = i;
//				
//				if( SelArt == i ) {
//					GUI.color = Color.yellow;
//					if( GUILayout.Button("Artifact " + i, GUILayout.Width(100), GUILayout.Height(20) ) ) SelArt = i; }
//				GUI.color = defaulcol;
//			}
//			else if( nextfree == -1 ) nextfree = i;
//			if( cont == 4 || cont == 8 || cont == 12 || cont == 16 )
//			{ EditorGUILayout.EndHorizontal(); EditorGUILayout.BeginHorizontal(); }
//		}
//		
//		EditorGUILayout.EndHorizontal();  		
//		GUILayout.Space(20);
//		if( GUILayout.Button("Add") ) 
//		{
//			GameObject prefab = (GameObject)Resources.Load ("Enemy Artifact");  		
//			GameObject instance = (GameObject)GameObject.Instantiate (prefab);
//			instance.transform.parent = obj.transform;
//			instance.name = "Enemy Artifact " + nextfree;
//		}
//		
//		if( SelArt != -1)
//		{
//			GUILayout.TextField("Selected: Artifact: " + SelArt, 33);
//			GameObject art = (GameObject)obj.transform.FindChild("Enemy Artifact " + SelArt ).gameObject;
//			if( GUILayout.Button("Delete") ) 
//			{
//				DestroyImmediate( art );
//				SelArt = -1;
//			} 	
//			GUILayout.Space(30);
//			DrawArtifactEditor( art, true );
//		}
//		else
//			GUILayout.TextField("Selected: Artifact: NONE", 25);
//		//GUI.EndScrollView();
//if( obj == null ) return;
//GUILayout.Space(20);
//int cont = 0;
//int nextfree = -1;
//EditorGUILayout.BeginHorizontal();

//		for( int i = 1; i < 30; i++ ) 
//		{
//			if ( dad.transform.FindChild("Perk " + i ) ) 
//			{
//				cont++;	
//				if( SelPerk != i )
//					if( GUILayout.Button("Perk " + i, GUILayout.Width(66), GUILayout.Height(20) ) ) SelPerk = i;
//				
//				if( SelPerk == i ) {
//					GUI.color = Color.yellow;
//					if( GUILayout.Button("Perk " + i, GUILayout.Width(66), GUILayout.Height(20) ) ) SelPerk = i; }
//				GUI.color = defaulcol;
//			}
//			else if( nextfree == -1 ) nextfree = i;
//			if( cont == 4 || cont == 8 || cont == 12 || cont == 16 )
//			{ EditorGUILayout.EndHorizontal(); EditorGUILayout.BeginHorizontal(); }
//		}
//EditorGUILayout.EndHorizontal();
//GUILayout.Space(20);

//		if( GUILayout.Button("Add") ) 
//		{
//			GameObject prefab = (GameObject)Resources.Load ("Perk");  		
//			GameObject instance = (GameObject)GameObject.Instantiate (prefab);
//			instance.transform.parent = dad.transform;
//			instance.name = "Perk " + nextfree;
//
//			if( dad.name == "Unit Perks" ) 
//			{
//				Unit ui = dad.transform.parent.gameObject.GetComponent<Unit>();
//				ui.UpdatePerkList( dad );
//			}
//			if( dad.name == "Enemy Artifact Perks" || dad.name == "Artifact Perks" )
//			{
//				Artifact ar = dad.transform.parent.gameObject.GetComponent<Artifact>();
//				ar.UpdatePerkList( dad );
//			}
//		}

//GUILayout.TextField("Selected: Perk: " + SelPerk, 33);
//if( SelPerk == -1 ) return;

//if( GUILayout.Button("Sel None") ) { SelPerk = -1; return; }

//if( obj == null ) return;

//Transform tr = dad.transform.FindChild("Perk " + SelPerk );
//if( tr == null ) return;
//GameObject obj = tr.gameObject;

//		if( GUILayout.Button("Delete") ) 
//		{
//			DestroyImmediate( obj );
//			SelPerk = -1;
//
//			if( dad.name == "Unit Perks" ) 
//			   {
//			    Unit ui = dad.transform.parent.gameObject.GetComponent<Unit>();
//			    ui.UpdatePerkList( dad );
//			   }
//			if( dad.name == "Enemy Artifact Perks" || dad.name == "Artifact Perks" )
//			{
//				Artifact ar = dad.transform.parent.gameObject.GetComponent<Artifact>();
//				ar.UpdatePerkList( dad );
//			}
//			return;
//		} 
//				Perk targetComp = obj.GetComponent<Perk>();
//				if (targetComp != null)
//				{
//					var editor = Editor.CreateEditor(targetComp);
//					editor.OnInspectorGUI();            
//				}
//		GUIStyle myStyle = new GUIStyle();		
//		myStyle.fontSize = 30;
//		GUI.Label(new Rect(10,10, 100, 30), obj.name );
//		GUILayout.Space(40);
//

//
//		if( GUILayout.Button("Edit Perks", GUILayout.Height(30)))
//		{
//			if( SelArt != -1 )
//			   {
//				if( Selection.activeGameObject.name == "Enemy Artifact Perks")
//				   {  
//					Selection.activeGameObject = Selection.activeGameObject.transform.parent.gameObject;
//					Debug.Log(Selection.activeGameObject);
//
//					Selection.activeGameObject = Selection.activeGameObject.transform.parent.gameObject;
//					Debug.Log(Selection.activeGameObject);
//				   }
//				else
//				   {  
//				      Selection.activeGameObject = obj.transform.FindChild("Enemy Artifact " + SelArt + "/" + "Enemy Artifact Perks").gameObject;
//					  Debug.Log(Selection.activeGameObject);
//				   }
//			   }
//			else
//			   {
//			   if( Selection.activeGameObject.name == "Artifact Perks" ) Selection.activeGameObject = Selection.activeGameObject.transform.parent.gameObject;
//			   else
//			   Selection.activeGameObject = obj.transform.FindChild("Artifact Perks").gameObject;
//			   }
//			return;
//		}
//
//		if( obj == null ) return;
//		if( obj.name == "Artifact Perks" )       { UpdatePerkWindow( obj ); return; } else
//		if( obj.name == "Enemy Artifact Perks" ) { UpdatePerkWindow( obj ); return; } else
//		if( DrawArtifactEditor( obj, false ) ) return;
//		DrawMapEnemyEditor( obj );
//		if( !bEnemy )
//		{
//		    for( int i = 0; i < 10; i++)
//			if ( obj.name == "Artifact " + i ) break; else if( i == 9 ) return false;
//			GUILayout.Space(20);
//		}
//		Artifact mt = obj.GetComponentInChildren<Artifact>();
//		if( mt == null ) return false;
//		scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true,  GUILayout.Width(300),  GUILayout.Height(800)); 
//		GameObject sel = Selection.activeGameObject;
//				
////		Artifact targetComp = obj.GetComponent<Artifact>();
////		if (targetComp != null)
////		{
////			var editor = Editor.CreateEditor(targetComp);
////			editor.OnInspectorGUI();            
////		}
//		
//		mt.Level = (int)EditorGUILayout.IntSlider( "Level", mt.Level, 1, 100 );
//
//		EUnitType old = mt.UnitType;
//		mt.UnitType = ( EUnitType ) EditorGUILayout.EnumPopup(new GUIContent("Unit Type", ""), mt.UnitType ); 
//
//		if( old != mt.UnitType )
//		   {
//			mt.UpdateSprite();
//			SceneView.RepaintAll();   
//		   }
//
//		GUI.EndScrollView();