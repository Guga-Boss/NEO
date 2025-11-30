using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using PathologicalGames;
using System.Linq;

public partial class Map : MonoBehaviour
{
    public void UpdateNoHeroLOSFire( bool force = false )
    {
        if( AdvanceTurn )                                                                    // No LOS to Hero wood placement
        if( CurrentArea != -1 )
        if( G.Hero.Body.FireMasterLevel >= 7 )
        {
            List<Unit> fplist = new List<Unit>();
            for( int y = ( int ) Map.I.P2.y; y <= Map.I.P1.y; y++ )
            for( int x = ( int ) Map.I.P1.x; x <= Map.I.P2.x; x++ )
            if ( AreaID[ x, y ] == CurrentArea )
            if ( Gaia2 [ x, y ] )
            if ( Gaia2 [ x, y ].TileID == ETileType.FIRE )            
                {
                    bool los = HasLOS( Hero.Pos, new Vector2( x, y ) );
                    
                    if( los ) return;
                    else
                    {
                        fplist.Add( Gaia2[ x, y ] );
                    }     
                }

            for( int i = 0; i < fplist.Count; i++ )
            {
                fplist[ i ].Body.WoodAdded[ 11 ] = true;
                CheckFireIgnition( ( int ) fplist[ i ].Pos.x, ( int ) fplist[ i ].Pos.y );
            }
        }
    }

    public void UpdateFire()
    {
        if( AdvanceTurn == false ) return;
        UpdateNoHeroLOSFire();
        Sector hs = Map.I.RM.HeroSector;
        int range = 1;
        if( Hero.Body.FireSpreadLevel >= 1 )
        { 
            List<Vector2> pires = new List<Vector2>();                                                                // Make a list of all burning pires      
            for( int y = ( int ) hs.Area.yMin - 1; y < hs.Area.yMax + 1; y++ )
            for( int x = ( int ) hs.Area.xMin - 1; x < hs.Area.xMax + 1; x++ )
                {
                    Unit un = null;
                    if( Gaia2[ x, y ] )
                    if( Gaia2[ x, y ].TileID == ETileType.FIRE )
                        un = Gaia2[ x, y ];
                    
                    if( un == null )
                    if( Unit[ x, y ] )
                    if( Unit[ x, y ].TileID == ETileType.BARRICADE )
                    if( Unit[ x, y ].Activated )
                        un = Unit[ x, y ];

                    if( un && un.Body.FireIsOn ) pires.Add( new Vector2( x, y ) );
                }

            if( CurrentArea != -1 || BarricadeDestroyedInTheTurn == false )
            for( int i = 0; i < pires.Count; i++ )                                                                    // Fire spreads
            for( int y = ( int ) pires[ i ].y - range; y <= pires[ i ].y + range; y++ )
            for( int x = ( int ) pires[ i ].x - range; x <= pires[ i ].x + range; x++ )
            if ( PtOnMap( Tilemap, new Vector2( x, y ) ) )
                {
                    if( CurrentArea != -1 && GetPosArea( new Vector2( x, y ) ) == -1 ) {} else
                    if( Unit[ x, y ] && Unit[ x, y ].TileID == ETileType.BARRICADE &&                                 // Firespread check for Barricade Burning
                        Unit[ x, y ].Variation > Hero.Body.FireSpreadLevel - 1 ) {} else
                        LightFire( x, y, true, true );
                }
        }
    }      

    public void PlaceWood( int x, int y, bool byhero = true )
    {
        for( int yy = y - 1; yy <= y + 1; yy++ )
        for( int xx = x - 1; xx <= x + 1; xx++ )
        if(  Gaia2[ xx, yy ] )
        if(  Gaia2[ xx, yy ].TileID == ETileType.FIRE )
        if(  Gaia2[ xx, yy ].Body.FireIsOn == false )
        if(  xx != x || yy != y )
        {
            for( int dr = 0; dr < 8; dr++ )
            {
                Vector2 tg = new Vector2( xx, yy ) + Manager.I.U.DirCord[ dr ];
                Unit ga = Gaia2[ xx, yy ];

                bool add = true;
                if( CurrentArea == -1 )                                                                      // Outarea firewood placement
                {
                    if( G.Hero.Pos == tg )
                    if( ga.Body.WoodAdded[ dr ] ) add = false;
                }

                if( add && tg == new Vector2( x, y ) )                                                       // Add firewood
                {
                    Map.I.CountRecordTime = true;
                    if( ga.Body.WoodAdded[ dr ] == false )
                    {
                        Map.I.CreateExplosionFX( new Vector2( x, y ), "Smoke Cloud", "Place Log" );          // Smoke Cloud FX
                        iTween.ShakePosition( ga.Graphic, new Vector3( .1f, .1f, 0 ), 0.15f );
                    }
                    ga.Body.WoodAdded[ dr ] = add;                                                           // Regular Wood Placing
                }
                CheckFireIgnition( xx, yy );                                                                 // Checks fire ignition
            }
        }
    }
    
    public void UpdateHeroFireWoodPlacement()
    {
        if( Hero.Body.FireMasterLevel >= 1 )
        {
            PlaceWood( ( int ) G.Hero.Pos.x, ( int ) G.Hero.Pos.y );                                                     // Hero firewood placement
            if( Hero.Body.FireMasterLevel >= 12 )
            if( CurrentArea != -1 )
            {
                Vector2 tg = Hero.GetFront();                                                                       // Hero Frontal Wood Placement
                if( Hero.CanMoveFromTo( false, Hero.Pos, tg, G.Hero ) )
                if( GetPosArea( tg ) != -1 )
                    PlaceWood( ( int ) tg.x, ( int ) tg.y );
            }
        }
    }

    public void UpdateFireIgnition( bool force = false )
     {
         if( force == false )
         if( AdvanceTurn == false ) return;

         if( CurrentArea != -1 )
         if( AreaCleared )
         {
             if( Hero.Body.FireMasterLevel >= 10 &&
                 CurArea.AreaClearedTurnCount <= 2 ) {}                                                                   // Two Extra Turns for fire ignition after clearing area
             else return;
         }

         UpdateHeroFireWoodPlacement();       
     }

     public bool CheckFireIgnition( int x, int y, int extraWood = 0 )
     {
         if( Gaia2[ x, y ] )
         if( Gaia2[ x, y ].TileID == ETileType.FIRE )
         if( Gaia2[ x, y ].Body.FireIsOn == false )
         {
             int wcont = 0;
             for( int dr = 0; dr < Gaia2[ x, y ].Body.WoodList.Length; dr++ )                       // counts logs around firepit
                 if( Gaia2[ x, y ].Body.WoodAdded[ dr ] ) wcont++;

             float pits = Map.I.RM.HeroSector.FireIgnitionCount;
             int times = 0;
             int woodneeded = Map.I.RM.RMD.BaseFireWoodNeeded -
                     ( int ) G.Hero.Body.FireWoodNeeded - times;
             if( woodneeded < 1 ) woodneeded = 1;                                                   // min wood == 1

             woodneeded -= extraWood;                                                               // extra wood provided from other sources like torch

             if( wcont >= woodneeded )                                                              // check condition
             {
                 bool res = LightFire( x, y, true, false, true );                                   // Light the firepit

                 if( res )
                 {
                     ForceUpdateLOSFire = true;                                                     // update LOS fire
                 }

                 return res;
             }
         }
         return false;
     }

     public bool LightFire( int x, int y, bool fireOn, bool barricade, bool addPoints = false )
    {
        if( PtOnMap( Tilemap, new Vector2( x, y ) ) == false ) return false;

        Unit un = null;
        if( Gaia2[ x, y ] )
        if( Gaia2[ x, y ].TileID == ETileType.FIRE ) un = Gaia2[ x, y ];

        if( barricade )
        if( un == null )
        if( Unit[ x, y ] )
        if( Unit[ x, y ].TileID == ETileType.BARRICADE )
        if( Unit[ x, y ].Activated )
            un = Unit[ x, y ];

        if( un )
        if( fireOn != un.Body.FireIsOn || GS.IsLoading )
        {
            un.Body.FireIsOn = fireOn;                                                           // update variable fire is on   

            LightSource2D.Create( CubeData.I.FirepitBase, un.Pos );                              // create light source

            Util.SetActiveRecursively( un.Body.EffectList[ 0 ].gameObject, fireOn );             // Activate Fx
            if( fireOn )
            if( GS.IsLoading == false )
            {
                if( addPoints )
                    Item.AddItem( ItemType.Res_Fire_Lits, +1 );                                  // Add fire points
                Map.I.CreateExplosionFX( new Vector2( x, y ), "Fire Explosion", "" );            // Explosion FX

                MasterAudio.PlaySound3DAtVector3( "Fire Ignite", un.Pos );
                if( GetPosArea( new Vector2( x, y ) ) == -1 )
                    Statistics.AddStats( Statistics.EStatsType.BONFIRESLIT, 1 );

                for( int i = 0; i < 8; i++ )
                {
                    Vector2 tg = un.Pos + Manager.I.U.DirCord[ i ];
                    int inv = Manager.I.U.InvDir[ i ];
                    Unit vine = Map.GFU( ETileType.VINES, tg );
                    if( vine )
                    if( vine.MeleeAttack.SpeedTimeCounter == -1 )
                    if( vine.Body.FireIsOn == false )
                    if( vine.Body.PoleSpriteList[ inv ].gameObject.activeSelf )
                    {                                                                             // Ignite vine
                        vine.MeleeAttack.SpeedTimeCounter = vine.Body.VineBurnTime;
                        vine.Body.VineFireSpreadID = vine.Control.VineColorList[ inv ];
                        vine.Body.FireSourcePos = un.Pos;
                    }
                }
            }
            return true;
        }
        return false;
    }

     public void CheckVineIgnition( Unit un )
    {
        for( int i = 0; i < 8; i++ )
        {
            Vector2 tg = un.Pos + Manager.I.U.DirCord[ i ];
            int inv = Manager.I.U.InvDir[ i ];
            Unit source = Map.GFU( ETileType.VINES, tg );                                           // Finds a source aroud vine to ignite it
            if( source == null )
                source = Map.I.GetUnit( ETileType.FIRE, tg );

            if( source )
            if( source.Body.FireIsOn == true )
            if( un.Body.PoleSpriteList[ i ].gameObject.activeSelf )
            if( source.TileID ==  ETileType.FIRE ||
                source.Body.PoleSpriteList[ inv ].gameObject.activeSelf )
            {                                                                                       // Ignite vine
                un.MeleeAttack.SpeedTimeCounter = source.Body.VineBurnTime;
                if( source.TileID != ETileType.FIRE )
                    un.Body.VineFireSpreadID = source.Body.VineFireSpreadID;                        // new was: un.Body.VineFireSpreadID = source.Control.VineColorList[ i ];
                un.Body.FireSourcePos = source.Pos;
            }
        }
    } 

     public void LightVine( Unit vine )
     {
         vine.Body.EffectList[ 0 ].gameObject.SetActive( true );                                     // Ignites the  vine
         vine.Body.FireIsOn = true;
         vine.MeleeAttack.SpeedTimeCounter = -1;
         MasterAudio.PlaySound3DAtVector3( "Fire Ignite", vine.Pos );
         EDirection dr = Util.GetTargetUnitDir( vine.Body.FireSourcePos, vine.Pos );
         bool ok2 = false;
         for( int i = 3; i >= 0; i-- )
         {
             int id = ( int ) Util.RotateDir( ( int ) dr, i );                                       // Try to ignite vines frome both directions
             Vector2 tg = vine.Pos + Manager.I.U.DirCord[ id ];
             bool ok1 = TryToLightTheVine( vine, id, tg, dr );

             if( i > 0 )
             {
                 id = ( int ) Util.RotateDir( ( int ) dr, -i );                                      // other direction
                 tg = vine.Pos + Manager.I.U.DirCord[ id ];
                 ok2 = TryToLightTheVine( vine, id, tg, dr ); 
             }

             if( ok1 || ok2 ) return;                                                                // fire ok, return
         }
     }

     private bool TryToLightTheVine( Unit srcvine, int id, Vector2 tg, EDirection dr )
     {
         Unit fire = Map.I.GetUnit( ETileType.FIRE, tg );
         if( fire && fire.Body.FireIsOn == false )
         if( srcvine.Body.PoleSpriteList[ id ].gameObject.activeSelf )                                  // vine ignite other firepits
             {
                 LightFire( ( int ) tg.x, ( int ) tg.y, true, false );
             }

         Unit orb = Map.I.GetUnit( ETileType.ORB, tg );
         if( orb && orb.Body.FireIsOn == false )                                                        // vine strike the orb
         if( srcvine.Body.PoleSpriteList[ id ].gameObject.activeSelf )
             {
                 orb.StrikeTheOrb( srcvine );
             }

         Unit tgvn = Map.GFU( ETileType.VINES, tg );
         int inv = Manager.I.U.InvDir[ ( int ) Util.GetTargetUnitDir( srcvine.Pos, tg ) ];
         if( tgvn )
         if( tgvn.MeleeAttack.SpeedTimeCounter == -1 )
         //if( tgvn.Body.FireIsOn == false )
         if( tgvn.Body.PoleSpriteList[ inv ].gameObject.activeSelf )
         if( tgvn.Control.VineColorList[ inv ] == srcvine.Body.VineFireSpreadID )
         {
             if( tgvn.Body.FireIsOn )
             if( tgvn.Body.VineFireSpreadID == srcvine.Body.VineFireSpreadID )                            // Do not allow infinite loop vine burning
                 return true;
             tgvn.MeleeAttack.SpeedTimeCounter = tgvn.Body.VineBurnTime;                                  // vine start burning
             tgvn.Body.VineFireSpreadID = srcvine.Control.VineColorList[ id ];
             tgvn.Body.FireSourcePos = srcvine.Pos;
             return true;
         }
         return false;
     }

     public void UpdateVineFire( Unit un )
     {
         if( G.Hero.Pos == un.Pos )
         if( un.Body.FireIsOn )
         {
             StartCubeDeath();                                                                       // Hero dies Burnt
             G.Hero.Graphic.transform.position = Vector2.zero;                                            
             G.Hero.Graphic.gameObject.SetActive( false );
             MasterAudio.PlaySound3DAtVector3( "Fire Ignite", G.Hero.Pos );                          // Burn FX
         }

         if( un.MeleeAttack.SpeedTimeCounter != -1 )
         {
             un.MeleeAttack.SpeedTimeCounter -= Time.deltaTime;

             if( un.MeleeAttack.SpeedTimeCounter <= 0 )
             //if( un.Body.FireIsOn == false )
             {
                 LightVine( un );                                                                     // Lights the vine up
             }
         }
     }

   public void UpdateVineExpansion()
   {
       if( G.Hero.Control.OldPos == G.Hero.Pos ) return;
       if( Item.GetNum( ItemType.Res_Vine_Bale ) <= 0 ) return;

       Unit tgvn = Map.GFU( ETileType.VINES, G.Hero.Pos );
       Unit frvn = Map.GFU( ETileType.VINES, G.Hero.Control.OldPos );
       EDirection dr = Util.GetTargetUnitDir( G.Hero.Control.OldPos, G.Hero.Pos );
       int inv = Manager.I.U.InvDir[ ( int ) dr ];

       if( tgvn == null )
       {
           if( CurrentVineSelection != -1 )
           {
               Unit prefab = Map.I.GetUnitPrefab( ETileType.VINES );                             // Creates a new vine
               GameObject go = Map.I.CreateUnit( prefab, G.Hero.Pos );
               Unit un = go.GetComponent<Unit>();
               G.HS.AddFlying( un );
               Map.I.Unit[ ( int ) un.Pos.x, ( int ) un.Pos.y ] = null;

               un.Control.OldPos = un.Pos;
               un.Control.VineList.Add( CurrentVineSelection );
               un.Body.CreatedVine = true;
               un.Body.PoleSpriteList[ ( int ) dr ].color = new Color( 1, 1, 1, -3 );
               un.Body.PoleSpriteList[ inv ].color = new Color( 1, 1, 1, -3 );
               un.Body.CreatedVineList[ inv ] = CurrentVineSelection;
               if( frvn )
                   frvn.Body.CreatedVineList[ ( int ) dr ] = CurrentVineSelection;
               Item.AddItem( ItemType.Res_Vine_Bale, -1 );                                        // Charges resource
           }
       }
       else
       {
           if( frvn )                                                                             // Only connects to another vine
           {
               if( frvn.Body.PoleSpriteList[ ( int ) dr ].gameObject.activeSelf == false )
               if( tgvn.Body.PoleSpriteList[ inv ].gameObject.activeSelf == false )
               {
                   frvn.Body.CreatedVineList[ ( int ) dr ] = CurrentVineSelection;
                   tgvn.Body.CreatedVineList[ inv ] = CurrentVineSelection;
                   frvn.Control.ForceVineLink[ ( int ) dr ] = CurrentVineSelection;
                   tgvn.Control.ForceVineLink[ inv ] = CurrentVineSelection;
                   Item.AddItem( ItemType.Res_Vine_Bale, -1 );                                    // Charges resource
               }
           }
       }
       if( tgvn && tgvn.Control.VineList.Count == 1 )
       {
           CurrentVineSelection = tgvn.Control.VineList[ 0 ];                                    // Sets the current vine selection for expansion
       }
   }
   public Color GetVineColor( int sel, float alpha )
   {
       if( sel == 0 ) return new Color( 1, 0, 0, alpha );
       if( sel == 1 ) return new Color( 0, 1, 0, alpha );
       if( sel == 2 ) return new Color( 1, 1, 0, alpha );
       if( sel == 3 ) return new Color( 0, 0, 1, alpha );
       if( sel == 4 ) return new Color( 1, 0, 1, alpha );
       if( sel == 5 ) return new Color( 0, 1, 1, alpha );
       if( sel == 6 ) return new Color( 0, 0, 0, alpha );
       return Color.gray;
   }
}

