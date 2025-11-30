using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using PathologicalGames;
using Sirenix.OdinInspector;
//using System.Linq;

[System.Serializable]
public class FishActionStruct
{
    public string Description;
    [TabGroup( "Effect" )]
    public EFishActionEffectType EffectType = EFishActionEffectType.SwimType;
    [TabGroup( "Effect" )]
    public float EffectVal = -1;
    [TabGroup( "Effect" )]
    public float EffectVal2 = -1;
    [TabGroup( "Effect" )]
    public float ActionTotalTime = -1;    // tot time until action end
    [TabGroup( "Effect" )]
    public int WaitActionID = -1;
    [TabGroup( "Effect" )]
    public int WaitTotalTime = -1;
    [TabGroup( "Effect" )]
    public EFishSwimType EffFishSwimType = EFishSwimType.NONE;
    [TabGroup( "Condition" )]
    public EFishActionConditionType ConditionType = EFishActionConditionType.FishPercent;
    [TabGroup( "Condition" )]
    public float ConditionVal = -1;     // Fish percentage threshold to activate action
    [TabGroup( "Loop" )]
    public int TimesApplied = 0;               // looping counter
    [TabGroup( "Loop" )]
    public int TotalTimesApplied = 1;          // total loop amount

    public static void ResetAll( Unit un )
    {
        for( int i = 0; i < un.Control.FishAction.Count; i++ ) 
            un.Control.FishAction[ i ].Reset();
    }
    public void Reset()
    {
        EffFishSwimType = EFishSwimType.NONE;
        EffectVal = -1;
        EffectVal2 = -1;
        ConditionVal = -1;
        ConditionType = EFishActionConditionType.NONE;
        ActionTotalTime = -1;
        TimesApplied = 0;
        TotalTimesApplied = 1;
        WaitActionID = -1;
        WaitTotalTime = -1;    
    }
    public void Save()
    {
        GS.W.Write( ( int ) EffectType );
        GS.W.Write( EffectVal );
        GS.W.Write( EffectVal2 );
        GS.W.Write( ActionTotalTime );
        GS.W.Write( WaitActionID );
        GS.W.Write( WaitTotalTime );
        GS.W.Write( ( int ) ConditionType );
        GS.W.Write( WaitTotalTime );
        GS.W.Write( ConditionVal );
        GS.W.Write( TimesApplied );
        GS.W.Write( TotalTimesApplied );
    }
    public void Load()
    {
        EffectType = ( EFishActionEffectType ) GS.R.ReadInt32();
        EffectVal = GS.R.ReadSingle();
        EffectVal2 = GS.R.ReadSingle();
        ActionTotalTime = GS.R.ReadSingle();
        WaitActionID = GS.R.ReadInt32();
        WaitTotalTime = GS.R.ReadInt32();
        ConditionType = ( EFishActionConditionType ) GS.R.ReadInt32();
        WaitTotalTime = GS.R.ReadInt32();
        ConditionVal = GS.R.ReadSingle();
        TimesApplied = GS.R.ReadInt32();
        TotalTimesApplied = GS.R.ReadInt32();
    }
}

public class PondInfo
{
    public Vector2 TilePos;
    public float OverFishRecordTime;
    public int PondID;
    public int OverFishAffectedUnitsRecordCount;
    public float OverFishBonusRecord;
    public float OverFishCummulativeBonus;
}

public partial class Map : MonoBehaviour
{
    #region Variables
    [TabGroup( "Fish" )]
    public EFishingPhase FishingMode = EFishingPhase.NO_FISHING;
    [TabGroup( "Fish" )]
    public float FishingTimerCount = 0, FishingLineTimerCount = 4, FishingLineBreakTimerCount = -1, GlobalFishingBonusRecord = 0, OverFishTimer = 0, OverFishBonus = 0;
    [TabGroup( "Fish" )]
    public int OverFishCount = 0, OverFishAffectedUnitsCount = 0;
    [TabGroup( "Fish" )]
    public bool PerfectFishing = true;
    [TabGroup( "Fish" )]
    public List<FishingObject> HookList;
    [TabGroup( "Fish" )]
    public FishingObject MainHook = null;
    [TabGroup( "Fish" )]
    public float FishFinishingTotalTime = 3;
    [TabGroup( "Fish" )]
    public Vector2 MinHook, MaxHook, InitialHookPosition;
    [TabGroup( "Fish" )]
    public int CurrentContinuousPondId = -1;
    [TabGroup( "Fish" )]
    public int NumActiveHooks = 0;
    [TabGroup( "Fish" )]
    public int NumFishCaught = 0;
    [TabGroup( "Fish" )]
    public string FishCaughtList = "";
    [TabGroup( "Fish" )]
    public bool SkipTime = false;
    [TabGroup( "Fish" )]
    public List<PondInfo> PondList = new List<PondInfo>();
    [TabGroup( "Fish" )]
    public PondInfo CurPond;
    [TabGroup( "Fish" )]
    public bool RecordBroken = false;
    [TabGroup( "Fish" )]
    public Unit CurrentFishingPole = null;
    [TabGroup( "Fish" )]
    public tk2dSprite PoleEndHelper;  // pole ending position to connect to the line
    [TabGroup( "Fish" )]
    public LineRenderer FishingLineChunk;
    [TabGroup( "Fish" )]
    public Tack PoleStart, PoleEnd;
    [TabGroup( "Fish" )]
    public TackRope FishingLine;
    [TabGroup( "Fish" )]
    public float IdleInputTimer = 0;   
    #endregion

    public void UpdateFishing()
    {
        UpdateAllTimeFishingStuff();
        UpdateFlyingObjectsAnimation();
        if( IsPaused() ) return;
        UpdateFishLure();
        if( FishingMode == EFishingPhase.NO_FISHING ) return;
        SkipTime = false;

        switch( FishingMode )                                                                    // Main fishing update function
        {
            case EFishingPhase.INTRO:
            UpdateFishingIntro();
            break;
            case EFishingPhase.FISHING:
            UpdateFishingAction();
            break;
            case EFishingPhase.FINISH:
            UpdateFishingFinishing();
            break;
        }

        UpdateFishingHooks();
        UpdateFishStuff();
        if( SkipTime == false )
            FishingTimerCount -= Time.deltaTime;
    }
    public void UpdateAllTimeFishingStuff()
    {
        if( Manager.I.GameType != EGameType.CUBES ) return;
        if( FishingLineBreakTimerCount >= 0 )
            FishingLineBreakTimerCount += Time.deltaTime;
        if( FishingLineBreakTimerCount > 15 )
            FishingLine.transform.parent.gameObject.SetActive( false );
        UpdateFishingLineAnimation();                                                               // Updates sishing line animation

        Water.UpdateFishingMessages();
    }

    private void UpdateFishingLineAnimation()
    {
        if( FishingLine )
        {
            Color c1 = new Color( 1, 1, 1, 1 );
            Color c2 = new Color( 1, 1, 1, .4f );
            if( FishingLineChunk == null )
                FishingLineChunk = FishingLine.transform.GetComponentInChildren<LineRenderer>();

            if( FishingMode == EFishingPhase.NO_FISHING )
                PoleStart.transform.position = G.Hero.Graphic.transform.position;
            else
            {
                if( FishingLineChunk )
                    FishingLineChunk.SetColors( c1, c2 );
                FishingLineChunk.SetWidth( 0.04f, 0.04f );
                PoleStart.transform.position = PoleEndHelper.transform.position;
                if( FishingLineTimerCount > 3 )
                {
                    FishingLineTimerCount = 0;
                    FishingLine.AutoCalculateAmountOfNodes = true;
                    float fact = 15; if( Util.Chance( 50 ) ) fact *= -1;
                    iTween.PunchRotation( G.Hero.Graphic, new Vector3( 0, 0, fact ), .8f );
                }
                PoleEnd.transform.position = MainHook.transform.position;
            }
            FishingLineTimerCount += Time.deltaTime;
        }
    }
    public void UpdateFlyingObjectsAnimation()
    {
        if( Manager.I.GameType != EGameType.CUBES ) return;
        if( Map.I.TurnFrameCount == 1 )
            G.HS.GlobalVaultPoints = 0;
        for( int u = RM.HeroSector.Fly.Count - 1; u >= 0; u-- )                                // Updates Fishing objects animation
        if ( RM.HeroSector.Fly[ u ].TileID == ETileType.FISH ||
             RM.HeroSector.Fly[ u ].TileID == ETileType.FOG ||
             RM.HeroSector.Fly[ u ].TileID == ETileType.MINE )
             RM.HeroSector.Fly[ u ].UpdateAnimation();
        Water.ForceUpdatePoleText = false;
        Mine.OldZoom = Map.I.ZoomMode;
        if( Map.I.TurnFrameCount == 3 )
            Mine.UpdateChiselText = false;
    }

    public void UpdateFishLure()
    {
        if( Map.I.RM.RMD.FishCornTargetRadius > 0 )
        if( Item.GetNum( ItemType.Res_BirdCorn ) > 0 )
        {
            G.Hero.Control.FrontalFlyingTargetDist = Map.I.RM.RMD.FishCornTargetRadius +
            Controller.FrontalTargetManeuverDist;
            float ds = G.Hero.Control.FrontalFlyingTargetDist;
            if( Util.IsDiagonal( G.Hero.Dir ) ) ds = ds * 1.414213f;
            Map.I.HeroTargetSprite.transform.localPosition = new Vector3( 0, ds, -5 );
            Map.I.HeroTargetSprite.gameObject.SetActive( true ); 
        }
        else
            Map.I.HeroTargetSprite.gameObject.SetActive( false );
    }

    public void UpdateFishStuff()
    {
        BabyData.OverArrowDir = EDirection.NONE;
        Map.I.HeroTargetSprite.gameObject.SetActive( false );
        for( int u = RM.HeroSector.Fly.Count - 1; u >= 0; u-- )                                // Updates Fishing Line Positions
        if ( RM.HeroSector.Fly[ u ].TileID == ETileType.FISH )
        {
            Unit un = RM.HeroSector.Fly[ u ];
            if( un.Body.FishCaught )
            if( un.Control.FishingLine )
                {
                    un.Control.FishingLine.SetPosition( 0, PoleEndHelper.transform.position );
                    un.Control.FishingLine.SetPosition( 1, un.Spr.transform.position );
                }        

            if( un.Activated == false )
            {
                un.Control.RespawnTimeCount += Time.deltaTime;                                         // Respawn bonuses
                float totTime = un.Control.BaseTotalRespawnTime;

                if( ( un.Control.RespawnCount < un.Control.BaseTotalRespawnAmount &&
                      un.Control.RespawnTimeCount >= totTime ) || 
                      FishingMode == EFishingPhase.FINISH )
                {
                    un.Activate( true );
                    un.Graphic.gameObject.SetActive( true );
                    un.Control.RespawnCount++;
                    MasterAudio.PlaySound3DAtVector3( "Item Collect", G.Hero.Pos );
                }
            }
        }
    }

    public ItemType GetFishResID( EFishType t )
    {
        if( t == EFishType.FISH_1 ) return ItemType.Res_Fish_Yellow;
        if( t == EFishType.FISH_2 ) return ItemType.Res_Fish_Red;
        if( t == EFishType.FISH_3 ) return ItemType.Res_Fish_Blue;
        if( t == EFishType.FISH_CRAB ) return ItemType.Res_Fish_Crab;
        if( t == EFishType.FISH_MANTA ) return ItemType.Res_Fish_Manta_Ray;
        if( t == EFishType.FISH_SNAKE ) return ItemType.Res_Fish_Water_Snake;
        if( t == EFishType.FISH_BROWN ) return ItemType.Res_Fish_Brown;
        if( t == EFishType.FISH_FROG ) return ItemType.Res_Fish_Frog;
        return ItemType.NONE;
    }
    public void UpdateFishingFinishing()
    {
        if( FishingTimerCount <= 0 )
        {
            FishingMode = EFishingPhase.NO_FISHING;                                                       // Set no fishing mode
            UI.I.SetBigMessage( "", Color.yellow, .1f );
            UI.I.BigMessageTextTimeCounter = .1f;
                                
            CurrentFishingPole.Activate( false );                                                         // Deactivate Pole
            CurrentFishingPole.Spr.gameObject.SetActive( true );            
            CurrentFishingPole.LevelTxt.gameObject.SetActive( true );
            MasterAudio.StopAllOfSound( "Water Flow" );
            UI.I.SelectedPerk = UI.I.LastClickedPerk;

            int count = 0;
            for( int u = RM.HeroSector.Fly.Count - 1; u >= 0; u-- )
            if ( RM.HeroSector.Fly[ u ].TileID == ETileType.FISH )
            if ( RM.HeroSector.Fly[ u ].Body.FishCaught )                                         // Fish caught bonus giving
            {
                Unit fl = RM.HeroSector.Fly[ u ];
                int amt = 1 + G.HS.FishingExtraPrize + Water.TempFishingExtraPrize;                      // Extra Prize Pole Bonus
                Item.AddItem( GetFishResID( fl.Body.FishType ), amt );

                amt = G.HS.FishingReturnCatch + Water.TempFishingReturnCatch;  

                Water.SpawnFishByMod( fl, amt, fl.ModID, Water.HookDropPosition );                        // Return Catch Pole Bonus

                fl.Body.FishCaught = false;
                PoolManager.Pools[ "Pool" ].Despawn( fl.Control.FishingLine.transform );                  // Despawn fishing line

                fl.Kill();                                                                                // kill fish
                count++;
            }
      
            for( int u = RM.HeroSector.Fly.Count - 1; u >= 0; u-- )
            if( RM.HeroSector.Fly[ u ].TileID == ETileType.FISH )
            if( RM.HeroSector.Fly[ u ].Body.FishCaught == false )
            {
                Unit fl = RM.HeroSector.Fly[ u ];
                if( fl.Body.Hp > 0 )
                {
                    float add = G.HS.FishAddPercent + Water.TempFishAddPercent;                          // Add Percent Pole Bonus
                    fl.Body.Hp += Util.Percent( add, fl.Body.Hp );

                    float mult = G.HS.FishMultiplyPercent + Water.TempFishMultiplyPercent;               // Multiply Percent Pole Bonus
                    if( mult > 0 ) fl.Body.Hp *= mult;

                    float per = G.HS.FishIncreasePercent + Water.TempFishIncreasePercent;                // Increase fish Percent Pole Bonus
                    if( per > 0 ) fl.Body.Hp += Util.Percent( per, fl.Body.Hp );
                }
            }

            MasterAudio.StopAllOfSound( "Fishing Reel" );
            if( count >= 1 )
                MasterAudio.PlaySound3DAtVector3( "Item Collect", G.Hero.Pos );                           // Sound Fx
            else
                MasterAudio.PlaySound3DAtVector3( "Error 2", G.Hero.Pos );
            BreakTheLine( false );

            Water.CheckCn( EPoleBonusCnType.HOOK_REMAIN_AROUND_LAND, null );                              // Keep hook around land tile hook bonus check
            Water.CheckCn( EPoleBonusCnType.HAVE_X_FISH_IN_INVENTORY, null );                             // Have x fish in inventory hook bonus check
            Water.CheckCn( EPoleBonusCnType.FINISH_FISHING, null );                                       // Finish Fishing hook bonus check

            if( Water.PoleBonusGiven == false )
                CurrentFishingPole.Water.PoleBonusFailed = true;                                          // Pole Bonus failed
            Water.ResetVars();
            CurrentFishingPole = null;
            Water.ForceUpdatePoleText = true;
            return;
        }
        UI.I.SetBigMessage( "Pulling Hook: " + FishingTimerCount.ToString( "0.0" )
            + " s.", Color.yellow, -1, -1, 850 );

        FishingLine.transform.parent.gameObject.SetActive( true );
        if( NumFishCaught >= 1 )
        if( FishingTimerCount > .2f )
            FishingLine.transform.parent.gameObject.SetActive( false );
    }

    public void UpdateFishingAction()
    {
        //if( cInput.GetKey( "Battle" ) )
        //{
        //    Message.GreenMessage( "Fishing Skipped..." );    
        //    FishingTimerCount = 0;                                                                        // Fishing Session Skip
        //}

        if( NumActiveHooks == 0 )
        {
            Message.RedMessage( "All Hooks were Lost..." );
            BreakTheLine( true );
        }

        if( FishingTimerCount <= 0 )
        {
            for( int h = 0; h < HookList.Count; h++ )
            if ( HookList[ h ].gameObject.activeSelf )
                 HookList[ h ].gameObject.SetActive( false );

            UpdateOverFishBonus( true );
            FishingMode = EFishingPhase.FINISH;
            FishingTimerCount = FishFinishingTotalTime;

            int frogcount = 0;
            for( int u = 0; u < RM.HeroSector.Fly.Count; u++ )                                                       // Any Frog Caught?
            {
                Unit fs = RM.HeroSector.Fly[ u ];
                if( fs.TileID == ETileType.FISH )
                {
                    if( fs.Body.FishType == EFishType.FISH_FROG )
                    {
                        fs.Control.RespawnTimeCount = 999;
                        if( fs.Body.IsFish && fs.Body.FishCaught == false )
                        if( ContinuousPondID[ ( int ) fs.Pos.x, ( int ) fs.Pos.y ] == CurrentContinuousPondId )
                        if( PullHook( fs, true ) )
                            frogcount++;
                    }
                    if( fs.Water.MarkedFish == 1 )
                        fs.Water.MarkedFish = 0;
                }
            }

            int redcount = 0;
            for( int u = 0; u < RM.HeroSector.Fly.Count; u++ )
            {
                Unit un = RM.HeroSector.Fly[ u ];
                if( un.TileID == ETileType.FISH )
                {
                    if( frogcount > 0 )
                    {
                        un.Body.Hp = 0;
                        Message.RedMessage( "Frog Caught! All Fish Lost!" );
                    }
                    un.Control.RespawnTimeCount = 999;
                    if( un.Body.IsFish && un.Body.FishCaught == false )
                    if( ContinuousPondID[ ( int ) un.Pos.x, ( int ) un.Pos.y ] == CurrentContinuousPondId )
                        {
                            bool res = PullHook( un, true );
                            if( res && un.Body.FishType == EFishType.FISH_SNAKE ) redcount++;
                        }

                    un.Control.OverFishTimeCount = 0;
                }
            }

            if( redcount > 0 ) GiveInventoryFishBonus( redcount );                                          // Red Fish Inventory Bonus

            if( NumFishCaught > 0 )
                Message.GreenMessage( "Fishing Results:\n" + FishCaughtList );
            else
            {
                FishingTimerCount = 0;
                Message.RedMessage( "No Fish this time..." );
            }

            Water.CheckCn( EPoleBonusCnType.SIMULTANEOUS_FISH_CATCH, null );                                // Simultaneous Catch fish condition

            MasterAudio.PlaySound3DAtVector3( "Fishing Reel", G.Hero.transform.position );                  // Sound FX
        }

        string msg = "Fishing: " + FishingTimerCount.ToString( "0.0" ) + "s";                               // Updates big message text

        float tot = GetTotRecord();
        if( tot > 0 )
            msg += " (" + tot.ToString( "0." ) + "%)";
        if( Water.PoleBonusText != "" )
            msg += "\n" + Water.PoleBonusText;
        if( Water.PoleBonusEffText != "" )
            msg += "\n" + Water.PoleBonusEffText;

        UI.I.SetBigMessage( msg, Color.yellow, -1, -1, 850 );                                               // Set text
        Water.FishingTime += Time.deltaTime;
    }

    public void BreakTheLine( bool fx )
    {
        FishingLine.ObjectB = null;
        //FishingLine.CutNode( FishingLine.m_nodes[ FishingLine.m_nodes.Count - 1 ], true );       
        FishingTimerCount = 0;
        FishingLineBreakTimerCount = 0;
        if( fx )
            MasterAudio.PlaySound3DAtVector3( "Whip Crack", G.Hero.Pos );
    }

    public float GetTotRecord()
    {
        float val = CurPond.OverFishCummulativeBonus;
        float amt = Util.Percent( Map.I.RM.RMD.OverFishCumulativeBonusFactor, val );
        float flowerbn = Item.GetNum( ItemType.Res_Water_Flower ) * RM.RMD.FishingBonusPerFlower;
        amt += flowerbn;
        return CurPond.OverFishBonusRecord + amt;
    }

    public void GiveInventoryFishBonus( int redcount )
    {
        ItemType it = ItemType.Sardine_Rarity_1;
        if( redcount == 2 ) it = ItemType.Trout_Rarity_2;
        if( redcount == 3 ) it = ItemType.Bass_Rarity_3;
        if( redcount == 4 ) it = ItemType.Cod_Rarity_6;
        if( redcount == 5 ) it = ItemType.StoneCat_Rarity_5;
        if( redcount == 6 ) it = ItemType.SwordFish_Rarity_6;
        Item.AddItem( it, 1 );
        string nm = G.GIT( it ).GetName();
        Message.GreenMessage( "Fish of rarity " + redcount + "\nAdded to the inventory:\n" + nm );
    }

    public bool PullHook( Unit un, bool ending )
    {
        float chance = un.Body.Hp;                                                                // Fish caught or not calculation

        if( ending == false && un.Body.FishType == EFishType.FISH_SNAKE )                         // no red fish from bonus hook pull
            chance = 0;

        if( un.Md && un.Md.FishCatchType == EFishCatchType.ABSOLUTE )                             // Absoulute Fishing type
        {
            chance = 0;
            if( un.Body.Hp >= 100 ) chance = 100;
        }

        if( Util.Chance( chance ) )
        {
            if( G.HS.CatchEnabled == false || Water.TempCatchEnabled == false )                   // Fish Catch Disabled Pole bonus
            {
                Message.CreateMessage( ETileType.NONE, "Fish Catch Disabled!", 
                un.Pos, Color.red );
                return false;
            }

            int res = ( int ) GetFishResID( un.Body.FishType );
            FishCaughtList += G.GIT( res ).GetName() +" Caught.\n";        // Fish Caught
            un.Body.FishCaught = true;
            un.Control.FishCaughtTimeCount = 0;
            un.Control.FishCatchPosition = un.transform.position;
            if( FishingMode == EFishingPhase.FINISH )
                un.Control.FishCaughtTimeCount = -1;
            un.Control.SwimmingDepht = 0;
            un.RightText.gameObject.SetActive( false );
            Transform tr = PoolManager.Pools[ "Pool" ].Spawn( "Fishing Line" );
            un.Control.FishingLine = tr.GetComponent<LineRenderer>();
            MasterAudio.PlaySound3DAtVector3( "Fishing Reel", G.Hero.transform.position );
            NumFishCaught++;
            Water.SimultaneousCatchFish++;
            if( Map.I.RM.HeroSector.CatchModList.Contains( un.ModID ) == false )
                Map.I.RM.HeroSector.CatchModList.Add( un.ModID );
            return true;
        }
        FishingLineBreakTimerCount = 0;
        return false;
    }

    public void UpdateFishingIntro()
    {
        if( FishingTimerCount <= 0 )
        {
            FishingMode = EFishingPhase.FISHING;
            FishingTimerCount = HeroData.I.GetVal( EHeroDataVal.FISHING_TIME );
            MasterAudio.StopAllOfSound( "Fishing Reel" );
            MasterAudio.PlaySound3DAtVector3( "Water Splash", G.Hero.transform.position );
            Util.PlayParticleFX( "Water Splash", MainHook.transform.position );  
            OverFishTimer = 0;
            OverFishAffectedUnitsCount = 0;
            OverFishBonus = 0;
            RecordBroken = false;
            for( int u = 0; u < RM.HeroSector.Fly.Count; u++ )
            if ( RM.HeroSector.Fly[ u ].TileID == ETileType.FISH )
                 RM.HeroSector.Fly[ u ].Control.OverFishTimeCount = 0;
            Water.CheckCn( EPoleBonusCnType.THROW_HOOK, null );                                  // Throw the hook hook bonus check
        }
        UI.I.SetBigMessage( "Preparing Bait: " + FishingTimerCount.ToString( "0.0" ) + " s.", Color.yellow, -1, - 1, 850 );
    }

    public void UpdateFishingHooks()
    {
        Vector3 vec = GetInputVector( false );
        if( Map.I.FreeCamMode ) vec = Vector3.zero;
        if( Map.I.InvalidateInputTimer > 0 ) vec = new Vector3( 0, 0, 0 );   
        float basespeed = HeroData.I.GetVal( EHeroDataVal.FISHING_HOOK_SPEED );
        float speed = basespeed;
        Vector3 vecn = vec;
        vecn.Normalize();
        Vector3 add = vecn * speed;
        // Buoy move Speed
        EDirection movedir = Util.GetTargetUnitDir( Vector2.zero, add );
        Vector3 addnotnormalized = add;
        MinHook = MainHook.transform.position;
        MaxHook = MainHook.transform.position;
        OverFishCount = 0;
        FishingObject.UpdateHarpoonShot();                                                                      // Updates Token based harpoon shooting

        if( vec != Vector3.zero )
        {
            MasterAudio.PlaySound3DAtVector3( "Water Flow", MainHook.transform.position );                 // Water flow hook moving Sound FX
            IdleInputTimer = 0;
        }
        else
        {
            IdleInputTimer += Time.deltaTime;
            if( IdleInputTimer > .5f )
                MasterAudio.StopAllOfSound( "Water Flow" );                                                     // Stop Sound FX
        }

        NumActiveHooks = 0;
        Water.SimultaneousAttackedFish = 0;
        for( int h = 0; h < HookList.Count; h++ )
        if ( HookList[ h ].gameObject.activeSelf )
        {
            NumActiveHooks++;
            FishingObject hk = HookList[ h ];
            hk.Dest = Vector3.zero;

            hk.UpdateHookMerging( h );                                                                         // Hook Merging

            hk.Sprite.color = Color.white;                                                                     // Hook Color

            if( HookList[ h ] != MainHook )
                hk.Sprite.color = new Color( .8f, .6f, .6f, 1f );

            hk.Sprite.spriteId = 384;
            hk.Sprite.transform.localScale = new Vector3( 1, 1, 1 );                                           // Hook Size sprite setting
            if( hk.HookType == 2 )
                hk.Sprite.transform.localScale = new Vector3( .75f, .75f, 1 );
            if( hk.HookType == 3 )
                hk.Sprite.transform.localScale = new Vector3( .5f, .5f, 1 );

            if( hk.transform.position.x < MinHook.x ) MinHook.x = hk.transform.position.x;                     // Min and Max hook pos for camera placement
            if( hk.transform.position.y < MinHook.y ) MinHook.y = hk.transform.position.y;
            if( hk.transform.position.x > MaxHook.x ) MaxHook.x = hk.transform.position.x;
            if( hk.transform.position.y > MaxHook.y ) MaxHook.y = hk.transform.position.y;


            Vector3 old = hk.transform.position;
            int x, y;
            Vector3 tgg = hk.transform.position;
            tgg += addnotnormalized * Time.deltaTime;

            Tilemap.GetTileAtPosition( tgg, out x, out y );

            bool tileChanged = false;
            if( hk.TilePos != new Vector2( x, y ) )
            {
                tileChanged = true;
                hk.TileDistanceTravelled = 0;
            }  

            hk.OldTilePos = hk.TilePos;
            hk.TilePos = new Vector2( x, y );
            hk.OrbHitTimeCounter -= Time.deltaTime;

            if( tileChanged )
                Physics2D.gravity = new Vector2( Random.Range( -3, +3 ), Random.Range( -3, +3 ) );                  // Alter gravity for animation purposes

            Unit item = GetUnit( ETileType.ITEM, hk.TilePos );
            if( item )
                G.Hero.Control.UpdateResourceCollecting( hk.TilePos, false );                                       // Hook Collects water item    

            Water.UpdateOneTimePerHookBonusCheck( hk );                                                             // Once per frame hook bonus update

            Unit arrow = Map.I.GetUnit( ETileType.ARROW, new Vector2( x, y ) );                                     // for forced move purposes
            if( arrow == null || arrow.Pos != hk.ArrowCheckedPos )                     
            hk.ArrowCheckedPos = new Vector2( -1, -1 );

            bool intTile = false;
            speed = basespeed;
            List<Unit> ol = FUnit[ ( int ) hk.TilePos.x, ( int ) hk.TilePos.y ];         
            if( ol != null )
            for( int i = 0; i < ol.Count; i++ )
            if ( ol[ i ].TileID == ETileType.FISH )
                {
                    if( ol[ i ].Body.FishType == EFishType.FAST_TILE )                                                // fast tile stepping
                        speed = 4;

                    if( ol[ i ].Body.FishType == EFishType.INT_TILE )                                                 // Int tile stepping
                    {
                        intTile = true;
                        UpdateIntTile( hk, x, y, ol[ i ].gameObject );       
                    }
                    else 
                    { 
                        hk.EnterIntTileTimerCount = 0;
                    }

                    if( tileChanged )
                    {
                        if( ol[ i ].Body.FishType == EFishType.CLOSED_WATER_TRAP )                                    // Water trap stepping
                            SetWaterObject( ol[ i ], EFishType.OPEN_WATER_TRAP );
                        else
                        if( ol[ i ].Body.FishType == EFishType.OPEN_WATER_TRAP )
                            SetWaterObject( ol[ i ], EFishType.CLOSED_WATER_TRAP );  
                    }
                }

            add = vecn * speed;

            if( intTile == false )
            { 
                hk.UpdateForcedMovement( ref add, x, y, vec, vecn );                                         // Forced Movement 
                Vector3 tg = add;                                                                            // Apply hook movement
                hk.Delta = tg;
            } 

            bool blockhook = false;
            bool res = false;
            if( intTile == false )
                res = DoesTileDestroyBuoy( new Vector2( -1, -1 ), x, y, 
                movedir, ref blockhook, tileChanged, hk );                                                   // Destroy hook if land stepping
            if( res )
            {
                hk.Deactivate();
            }

            if( intTile == false )
            if( blockhook )                                                                                   // Slide Hook if obstacle found 
            {
                if( vec.x != 0 && vec.y != 0 )
                {
                    Vector2 vecH = new Vector2( vec.x, 0 );
                    Vector3 addH = vecH * (speed*1);
                    addH.Normalize();
                    Vector3 hor = old;
                    hor += addH * Time.deltaTime;                    
                    bool horiz = IsPosFree( hk.OldTilePos, hor );

                    Vector2 vecV = new Vector2( 0, vec.y );
                    Vector3 addV = vecV * (speed*1);
                    addV.Normalize();
                    Vector3 ver = old;
                    ver += addV * Time.deltaTime;
                    bool vert = IsPosFree( hk.OldTilePos, ver );

                    if( horiz && !vert ) hk.Dest = hor;  else
                    if( vert && !horiz ) hk.Dest = ver;  else
                    {
                        hk.Dest = old;
                        hk.TilePos = hk.OldTilePos;
                    }
                }
                else
                if( hk.ForcedMoveDir == Vector3.zero )
                {
                    hk.Dest = old;
                    hk.TilePos = hk.OldTilePos;                    
                }
            }

            string msg = "";
            float percent = 0;
            Unit oldClosest = hk.ClosestFish;
            hk.ClosestFish = null;
            hk.ClosestFishDistance = 1000;
            hk.OldTargetFishList.AddRange( hk.TargetFishList );
            hk.TargetFishList = new List<Unit>();
            List<Unit> fl = GetFlyingInArea( new Vector2( x, y ), new Vector2( 3, 3 ) );                    // Is hook over water object?
            if ( FishingMode == EFishingPhase.FISHING )
            for( int u = fl.Count - 1; u >= 0; u-- )
            if ( fl[ u ].TileID == ETileType.FISH )
                {
                    float oldhp = fl[ u ].Body.Hp;
                    float dist = Vector2.Distance( hk.transform.position, fl[ u ].transform.position );     // Distance calculation from fisho to hook
                    float max = HeroData.I.GetVal( EHeroDataVal.FISHING_HOOK_RADIUS );

                    if( dist <= max )
                    {
                        UpdateHookOverObject( h, fl[ u ], ref msg, dist, max, ref percent );                // Updates Hook ove water object
                    }

                    fl[ u ].Water.IgnoreAttackTime -= Time.deltaTime;                                       // ignore attack timer decrement
                }

            if( msg != "" )
                Message.CreateMessage( ETileType.NONE, msg, hk.TilePos, Color.green );
            Tilemap.GetTileAtPosition( hk.transform.position, out x, out y );
            hk.TilePos = new Vector2( x, y );
            hk.Text.color = Color.white;
            hk.Text.gameObject.SetActive( false );                                                     
            if( h == 0 )
            if( OverFishAffectedUnitsCount >= 1 )
                hk.Text.gameObject.SetActive( true );
            if( RecordBroken )
                hk.Text.color = Color.yellow;

            if( OverFishBonus > 0 )
                hk.Text.text = "" + OverFishBonus.ToString( "0." ) + "%" +                                    // Updates Hook Text
                " x" + OverFishAffectedUnitsCount;
            else hk.Text.text = "";

            if( hk.ClosestFish == null && oldClosest != null )
            {
                Water.CheckCn( EPoleBonusCnType.HOOK_LEAVE_AT_EXACT_HP, oldClosest );
                Water.CheckCn( EPoleBonusCnType.HOOK_LEAVE_AT_RANGE_HP, oldClosest );
                Water.CheckCn( EPoleBonusCnType.HOOK_LEAVE_AT_HP_HIGHER_THAN, oldClosest );
                Water.CheckCn( EPoleBonusCnType.HOOK_LEAVE_AT_HP_SMALLER_THAN, oldClosest );
                MasterAudio.StopAllOfSound( "Water Flow" );
            }

            if( G.HS.MarkingEnabled == true ||
                Water.TempMarkingEnabled == true ) 
            if( hk.ClosestFish != null )
            {
                float amt = Time.deltaTime;
                percent += 50;
                amt = Util.Percent( percent, amt );
                float oldtm = hk.ClosestFish.Control.OverFishTimeCount;
                hk.ClosestFish.Control.OverFishTimeCount += amt;                                              // Only Closest fish gets bonus depending on hook distance

                if( hk.ClosestFish.Control.OverFishTimeCount >=
                    hk.ClosestFish.Control.OverFishSecondsNeededPerUnit )
                    if( oldtm < hk.ClosestFish.Control.OverFishSecondsNeededPerUnit )                         // Sound FX for Fish ready
                        MasterAudio.PlaySound3DAtVector3( "Click 2", G.Hero.Pos );
            }

            float dist2 = Vector3.Distance( hk.OldPosition, hk.transform.position );                         // Calculates total distance travelled by the hook in the tile
            if( hk.OldPosition.x != -1 )
            {
                hk.TileDistanceTravelled += dist2;
                Water.TempHookMoveDistance += dist2;
                if( Map.I.CurrentFishingPole.Water.PoleBonusCnType ==
                    EPoleBonusCnType.MOVE_HOOK_X_METERS )
                    Water.CheckCn( EPoleBonusCnType.MOVE_HOOK_X_METERS, null );                               // move hook x meters pole bonus condition check
            }
            hk.OldPosition = hk.transform.position;
        }

        Water.UpdateOneTimePerFrameBonusCheck();                                                              // Updates one time per frame bonus check
        UpdateOverFishBonus();                                                                                // Updates Over Fish Bonus    
        UpdateMarkedFish();
        Water.CheckCn( EPoleBonusCnType.SIMULTANEOUS_FISH_ATTACKED, null );                                   // Simultaneous attacked fish condition
    }

    public bool GetKey( string key, bool down )
    {
        if( down )
            return cInput.GetKeyDown( key );
        else
            return cInput.GetKey( key );
    }
    public Vector3 GetInputVector( bool down )
    {
        float fact = 1f;
        Vector3 vec = new Vector3( 0, 0, 0 ); 
        if( GetKey( "Move NE", down ) ) vec += new Vector3(  fact, fact,  0 ); else                  // Keyboard input for hook movement
        if( GetKey( "Move SE", down ) ) vec += new Vector3( +fact, -fact, 0 ); else
        if( GetKey( "Move SW", down ) ) vec += new Vector3( -fact, -fact, 0 ); else
        if( GetKey( "Move NW", down ) ) vec += new Vector3( -fact, +fact, 0 );
        if( vec == Vector3.zero )
        {
            if( GetKey( "Move N", down ) ) vec += new Vector3( 0, fact,  0 );
            if( GetKey( "Move S", down ) ) vec += new Vector3( 0, -fact, 0 );
            if( GetKey( "Move E", down ) ) vec += new Vector3( +fact, 0, 0 );
            if( GetKey( "Move W", down ) ) vec += new Vector3( -fact, 0, 0 );
        }

        if( down )
        if( vec != Vector3.zero )
            Controller.InputVectorList.Add( vec );
        return vec;
    }
    private void UpdateMarkedFish()
    {
        if( FishingMode != EFishingPhase.FISHING ) return;
        if( G.HS.MarkingEnabled == false )
        if( Water.TempMarkingEnabled == false ) return;

        List<Unit> fl = Map.I.RM.HeroSector.Fly;
        FishingObject hk = MainHook;
        for( int u = fl.Count - 1; u >= 0; u-- )
            if( fl[ u ].TileID == ETileType.FISH )
            {
                Unit fs = fl[ u ];
                if( hk.TargetFishList.Contains( fs ) == false )
                if( hk.OldTargetFishList.Contains( fs ) == true )
                {
                    if( OverFishAffectedUnitsCount < 2 )
                    if( fs.Water.MarkedFish == 1 )
                        fs.Water.MarkedFish = 0;                                      // Marks the fish temporarilly
                }

                if( hk.TargetFishList.Contains( fs ) == false )
                if( OverFishAffectedUnitsCount >= 2 )                   
                {
                    if( hk.ClosestFish.Water.MarkedFish == 1 )
                    if( fs.Water.MarkedFish == 1 )
                        fs.Water.MarkedFish = 2;                                     // Marks the fish forever  
                }
            }

        hk.OldTargetFishList = new List<Unit>();
    }
    public void UpdateOverFishBonus( bool finish = false )
    {
        if( FishingMode != EFishingPhase.FISHING ) return;
        if( G.HS.MarkingEnabled == false )
        if( Water.TempMarkingEnabled == false ) return;
        if( OverFishCount > 0 && finish == false )
            {
                OverFishTimer += Time.deltaTime;                                                    // Over Fish Timer increment 
            }
        else
            {
                if( OverFishTimer > 0 )                                                             // Hook leave fish
                {
                    MasterAudio.PlaySound3DAtVector3( "Water Splash 2",
                    G.Hero.transform.position );
                    RecordBroken = false;
                    if( finish == false )
                    {
                        for( int u = 0; u < RM.HeroSector.Fly.Count; u++ )                  // Reset all fish time counter
                        if ( RM.HeroSector.Fly[ u ].TileID == ETileType.FISH )
                        {
                            Unit fs = RM.HeroSector.Fly[ u ];                               // voids temp marked fish
                            if( fs.Water.MarkedFish == 1 ) 
                                fs.Water.MarkedFish = 0;
                            fs.Control.OverFishTimeCount = 0;
                        }
                        PerfectFishing = false;                                                     // voids perfect run
                        MasterAudio.PlaySound3DAtVector3( "Error 2", G.Hero.Pos, .4f );
                    }
                    CurPond.OverFishCummulativeBonus += OverFishBonus;
                }
                if( finish == false )
                    OverFishTimer = 0;
            }

            OverFishAffectedUnitsCount = 0;
            Water.MarkedFishCount = 0;
            for( int u = 0; u < RM.HeroSector.Fly.Count; u++ )                                 // Counts the number of affected fish
            if ( RM.HeroSector.Fly[ u ].TileID == ETileType.FISH )
            {
                Unit fs = RM.HeroSector.Fly[ u ];
                if( fs.Control.OverFishTimeCount > 0 )
                {
                    if( fs.Control.OverFishTimeCount >=
                        fs.Control.OverFishSecondsNeededPerUnit )
                    {
                        if( fs.Water.MarkedFish == 0 )                                                 // Marks the fish temporarilly
                            fs.Water.MarkedFish = 1;
                        OverFishAffectedUnitsCount++;
                    }
                }
                if( fs.Water.MarkedFish == 2 )
                    Water.MarkedFishCount++;
            }

            if( OverFishAffectedUnitsCount > CurPond.OverFishAffectedUnitsRecordCount )                // affected units record
                CurPond.OverFishAffectedUnitsRecordCount = OverFishAffectedUnitsCount;

            if( OverFishTimer > CurPond.OverFishRecordTime )                                           // over fish time record
                CurPond.OverFishRecordTime = OverFishTimer;

            OverFishBonus = OverFishTimer * Map.I.RM.RMD.OverFishBonusPerSecond;                       // Bonus Calculation

            float infla = OverFishTimer * Map.I.RM.RMD.OverFishBonusInflationPerSecond;                // Inflation Over Time
            OverFishBonus += Util.Percent( infla, OverFishBonus );
            int num = OverFishAffectedUnitsCount - 1;
            float rate = Util.CompoundInterest( Map.I.RM.RMD.OverFishBaseBonusPerUnit,                 // Inflation per extra fish units
            num, Map.I.RM.RMD.OverFishInflationPerUnit );
            OverFishBonus += Util.Percent( rate, OverFishBonus );

            string perf = "";
            float pb = 0;
            float tottime = HeroData.I.GetVal( EHeroDataVal.FISHING_TIME );                            // Bonus for keeping hook over fish all the time
            if( finish )    
            if( OverFishTimer >= tottime )
            {
                pb = Util.Percent( Map.I.RM.RMD.PerfectFishingBonus, OverFishBonus );
                OverFishBonus += pb;
                CurPond.OverFishCummulativeBonus += pb;
                perf = " PERFECT!";
            }

            if( OverFishBonus > CurPond.OverFishBonusRecord )                                          // Over Fish Bonus Record
            {
                CurPond.OverFishBonusRecord = OverFishBonus;
                RecordBroken = true;
            }

            float tot = GetTotRecord();
            if( tot > GlobalFishingBonusRecord )                                                       // Global record stats update for goal achievement
            {
                GlobalFishingBonusRecord = tot;
                LevelStats.FishingBonusReached = tot;
            }

            float cumf = Map.I.RM.RMD.OverFishCumulativeBonusFactor;
            float cum = Util.Percent( cumf, CurPond.OverFishCummulativeBonus );
            //string pbt = "" + pb.ToString( "0.0" ) + "%";
            //if( PerfectFishing == false ) pbt = "FAIL!";

            //string txt =                                                                 // Turn Info Text for fishing records
            //"Total Fishing Bonus: " + tot.ToString( "0.0" ) + "%";

            //float flowerbn = Item.GetNum( ItemType.Res_Water_Flower ) *
            //            RM.RMD.FishingBonusPerFlower;
            //if( flowerbn > 0 )
            //    txt += "\nFlower Bonus: " + flowerbn.ToString( "0.0" ) + "%";

            //if( Map.I.RM.RMD.OverFishBonusPerSecond > 0 )
            //    txt +=
            //    "\nSingle Run Bonus Record: " + CurPond.OverFishBonusRecord.ToString( "0.0" ) + "%" +
            //    "\nCummulative Bonus: " + cum.ToString( "0.0" ) + "%  (" + cumf + "%)" +
            //    "\nOver Fish Record Time: " + CurPond.OverFishRecordTime.ToString( "0.0" ) + "s" +
            //    "\nMost Fish Targeted: " + CurPond.OverFishAffectedUnitsRecordCount +
            //    "\nPerfect Fishing Bonus: " + pbt + " (" + Map.I.RM.RMD.PerfectFishingBonus + "%)";
            //UI.I.SetTurnInfoText(  txt  + perf, 2, Color.white );
    }

    public bool UpdateIntTile( FishingObject hk, int x, int y, GameObject intTile )
    {
        if( hk.EnterIntTileTimerCount  == 0 ) 
            MasterAudio.PlaySound3DAtVector3( "Plug in", hk.TilePos, .3f );
        hk.EnterIntTileTimerCount += Time.deltaTime;
        float fact = 1f;
        hk.transform.position = new Vector3( x, y, hk.transform.position.z );
        Vector3 ivec = Vector3.zero;
        if( cInput.GetKeyDown( "Move NE" ) ) ivec += new Vector3( fact, fact,   0 ); else            // Keyboard input for Int Tile movement
        if( cInput.GetKeyDown( "Move SE" ) ) ivec += new Vector3( +fact, -fact, 0 ); else
        if( cInput.GetKeyDown( "Move SW" ) ) ivec += new Vector3( -fact, -fact, 0 ); else
        if( cInput.GetKeyDown( "Move NW" ) ) ivec += new Vector3( -fact, +fact, 0 );
        if( ivec == Vector3.zero )
           {
            if( cInput.GetKeyDown( "Move N" ) ) ivec += new Vector3( 0, fact, 0  ); else
            if( cInput.GetKeyDown( "Move S" ) ) ivec += new Vector3( 0, -fact, 0 ); else
            if( cInput.GetKeyDown( "Move E" ) ) ivec += new Vector3( +fact, 0    ); else
            if( cInput.GetKeyDown( "Move W" ) ) ivec += new Vector3( -fact, 0, 0 );
           }
        EDirection dr = Util.GetTargetUnitDir( Vector2.zero, hk.ForcedMoveDir );
        Unit arrow = Map.I.GetUnit( ETileType.ARROW, hk.TilePos );                                   // Charges Harpoon direction over arrow
        if( arrow != null && hk.ForcedMoveDir != Vector3.zero )
        {
            hk.CastHarpoon( arrow.Dir );                                                             // Cast Harpoon
        }
        if( ivec == Vector3.zero ) return false;
        int xx, yy;
        Vector3 old = hk.transform.position;
        Vector3 dest = hk.transform.position + ivec;
        Tilemap.GetTileAtPosition( dest, out xx, out yy );
        bool block = false;
        DoesTileDestroyBuoy( hk.TilePos, xx, yy, EDirection.NONE, ref block, true, hk );
        if( hk.TilePos != new Vector2( xx, yy ) )                                                    // To avoid the int tile jumping bug
        if( hk.EnterIntTileTimerCount < .05f ) 
            return false;

        bool arrowb = CheckArrowBlockFromTo( hk.TilePos, new Vector2( xx, yy ), G.Hero, true );       // Arrow Block move?     

        if( hk.ForcedMoveDir != Vector3.zero )
        {
            EDirection dr2 = Util.GetTargetUnitDir( Vector2.zero, ivec * -1 );
            if( Map.IsWall( new Vector2( xx, yy ) ) || arrowb )                                       // Change harpoon direction over int tile 
            {
                hk.CastHarpoon( dr2 );
                iTween.PunchPosition( hk.transform.gameObject, ivec * .4f, .3f );
                return false;
            }
        }

        if( Map.IsWall( new Vector2( xx, yy ) ) )                                                    // wall found
        {
            return false;
        }

        if( arrowb ) block = true;

        if( block == false )
        {
            hk.transform.position = dest;                                                            // Apply position change
            EDirection dr3 = Util.GetTargetUnitDir( Vector2.zero, ivec );
            if( GetFish( new Vector2( xx, yy ), EFishType.INT_TILE ) == null )
            if( Util.IsDiagonal( dr3 ) )
                hk.Dest = dest;                                                                      // When diagonally exiting int tile, harpoon should be moved to the center of the tile to avoid collision bug. other cases hook should be placed in the tangent     
        }

        if( hk.TilePos != new Vector2( xx, yy ) )                                                    
        {
            Unit fs = Map.I.GetFish( new Vector2( xx, yy ), EFishType.INT_TILE );
            if( fs && fs.Body.FishType == EFishType.INT_TILE )
            {
                iTween.PunchScale( intTile, new Vector3( .3f,.3f, 0 ), 2 );
                MasterAudio.PlaySound3DAtVector3( "Plug in", hk.TilePos, 0.3f );
            }
            else
            {
                hk.transform.position = old + ( ivec * .6f );
                MasterAudio.PlaySound3DAtVector3( "Plug out", hk.TilePos, .3f );
                MasterAudio.PlaySound3DAtVector3( "Water Splash", hk.TilePos );
                hk.EnterIntTileTimerCount = 0;
                EDirection dr2 = Util.GetTargetUnitDir( Vector2.zero, ivec );                       // Int tile only stops forced move if exiting dir is against forced move dir
                if( dr == Util.GetInvDir( dr2 ) ||
                    dr == Util.GetInvDir( Map.I.RotateDir( dr2, +1 ) ) ||
                    dr == Util.GetInvDir( Map.I.RotateDir( dr2, -1 ) ) )
                {
                    hk.transform.position = old;                                  
                }
            }
        }
        return true;
    }

    public Unit GetFish( Vector2 pt, EFishType type )
    {
        List<Unit> f = Map.I.GF( pt, ETileType.FISH );
        if ( f != null )
        for( int i = 0; i < f.Count; i++ )
        if ( f[ i ].Body.FishType == type ) return f[ i ];
        return null;
    }

    public bool IsPosFree( Vector3 from, Vector3 to )
    {
        int x, y;
        Tilemap.GetTileAtPosition( to, out x, out y );
        bool block = false;
        bool tc = false;
        if( from != new Vector3( x, y, 0 ) ) tc = true;
        DoesTileDestroyBuoy( from, x, y, EDirection.NONE, ref block, tc, null );
        return !block;
    }
    
    public void PickUpWaterObject( Unit un )
    {
        un.Activate( false );
        un.Graphic.gameObject.SetActive( false );
        if( un.Control.RandomizePositionOnRespawn )
            RandomizeWaterObjectPosition( un, false );
        un.Control.RespawnTimeCount = 0;             
    }

    public void UpdateHookOverObject( int hook, Unit un, ref string msg, float dist, float max, ref float percent )
    {
        if( un.Activated == false ) return;
        FishingObject hk = HookList[ hook ];
        int soundFxId = 0;
        if( un.Body.FishType == EFishType.BONUS_TIME )                                                             // Bonus time
        {
            float bn = un.Control.FishingBonusAmount;
            FishingTimerCount += bn;
            PickUpWaterObject( un );
            msg = "+" + bn.ToString( "0." ) + "s";
            soundFxId = 1;
        }
        else
        if( un.Body.FishType == EFishType.BONUS_HOOK_SMALL  ||                                                     // Bonus Hook
            un.Body.FishType == EFishType.BONUS_HOOK_MEDIUM ||
            un.Body.FishType == EFishType.BONUS_HOOK_LARGE )
        {
            int numHooks = 1;
            int size = 1;
            if( un.Body.FishType == EFishType.BONUS_HOOK_MEDIUM ) size = 2;
            if( un.Body.FishType == EFishType.BONUS_HOOK_SMALL ) size = 3;
            if( Water.CreateExtraHook( numHooks, size, un.Pos ) )
            {
                PickUpWaterObject( un );
                msg = "+" + numHooks + " Extra hook" + Util.Plural( numHooks ) + "!";
                soundFxId = 1;
            }
        }
        else
        if( un.Body.FishType == EFishType.HOOK_PULL )                                                                // Hook Pull
        {       
            List<Unit> fl = GetFlyingInArea( un.Pos, new Vector2( 3, 3 ) );
            int count = 0;
            for( int u = fl.Count - 1; u >= 0; u-- )
            if ( fl[ u ].TileID == ETileType.FISH )
                {
                    float fishdist = Vector2.Distance( un.transform.position, fl[ u ].transform.position );
                    float maxfdist = 5;

                    if( fishdist < maxfdist )
                    {
                       bool res = PullHook( fl[ u ], false );
                       if( res ) count++;
                    }
                }
            if( count < 1 ) 
                Message.RedMessage( "No Fish Caught!" );
            PickUpWaterObject( un );
        }
        else
        if( un.Body.FishType == EFishType.BAIT )                                                                    // Bait
        {
            if( PondID == null ) UpdatePondID();
            List<Vector2> tlist = new List<Vector2>();
            int pondId = PondID[ ( int ) un.Pos.x, ( int ) un.Pos.y ];
            for( int dr = 0; dr < 8; dr++ )
            {
                List<Vector2> tl = Util.GetTileLine( un.Pos, ( EDirection ) dr, ETileType.WATER, Sector.TSX );
                for( int j = 0; j < tl.Count -1; j++ )
                {
                    if( PondID[ ( int ) tl[ j ].x,     ( int ) tl[ j  ].y ] !=                                     // remove no continuous pond id
                        PondID[ ( int ) tl[ j + 1 ].x, ( int ) tl[ j + 1 ].y ] )                 
                    {
                        int cut = ( tl.Count - ( j + 1 ) );
                        if( cut < 0 ) cut = 0;
                        tl.RemoveRange( j + 1, cut );
                    }
                }
                if( tl != null )
                {
                    if( tl.Count > 0 ) tlist.AddRange( tl );                                                      // Add direction to the master list
                }
            }

            tlist.RemoveAll( item => item == un.Pos );

            for( int i = 0; i < tlist.Count; i++ )
            {
                List<Unit> ol = FUnit[ ( int ) tlist[ i ].x, ( int ) tlist[ i ].y ];                              // Attracts Fish
                if ( ol != null )
                for( int j = 0; j < ol.Count; j++ )
                if ( ol[ j ].TileID == ETileType.FISH )
                if ( ol[ j ].Body.IsFish )
                {
                    ol[ j ].Control.FlyingTarget = un.Pos;
                    ol[ j ].Control.SwimmingDepht = Random.Range( 0.2f, 0.6f );
                }
            }

            PickUpWaterObject( un );
            msg = "Bait!";
            soundFxId = 1;
        }
        else
        if( un.Body.FishType == EFishType.TIME_SKIP )
        {
            SkipTime = true;
        }
        else
        if( un.Body.FishType == EFishType.BONUS_FISHING_LEVEL )                                    // Bonus Fishing Level
        {
            //HeroData.I.FishingHookExtraLevel++;
            PickUpWaterObject( un );
            msg = "Extra fishing Level!";
            soundFxId = 1;
        }
        else                    
        if( un.Body.FishType == EFishType.FAST_TILE ) {}
        else
        if( un.Body.FishType == EFishType.INT_TILE )  {}
        else
        if( un.Water.IgnoreAttackTime <= 0 )                                                       // Ignore attack if recently spawned
        {
            float perc = 100 - ( dist * 100 / max );
            percent = perc;
            float power = HeroData.I.GetVal( EHeroDataVal.FISHING_HOOK_ATTACK );                     
            bool harpoon = false;

            if( hk != Map.I.MainHook )
            {
                if( hk.HookType == 1 ) power = Util.Percent( 50, power );                          // Secondary Hooks
                if( hk.HookType == 2 ) power = Util.Percent( 25, power );
                if( hk.HookType == 3 ) power = Util.Percent( 10, power );  
            }

            if( un.Md && un.Md.FishCatchType == EFishCatchType.RANDOM )
            {
                if( un.Body.Hp > un.Control.FishRedLimit )                                         // Red Limit
                {
                    power /= 4;
                    float pr = G.HS.RedFishExtraAttack + 
                    Water.TempRedFishExtraAttack;
                    power += Util.Percent( pr, power );                                            // red fish Pole att bonus 
                }
                else
                if( un.Body.Hp < un.Control.FishGreenLimit )                                       // Green Limit
                {
                    power *= 2;
                    float pr = G.HS.GreenFishExtraAttack +
                    Water.TempGreenFishExtraAttack;
                    power += Util.Percent( pr, power );                                           // green fish Pole att bonus 
                }
                else
                {
                    float pr = G.HS.YellowFishExtraAttack + 
                    Water.TempYellowFishExtraAttack;                                              // yellow fish Pole att bonus      
                    power += Util.Percent( pr, power );
                }
            }
            if( hk.ForcedMoveDir != Vector3.zero )                                                // Forced Movement (caused by orb)
            {
                power *= Map.I.RM.RMD.HarpoonBonusAttack;
                float pr = G.HS.HarpoonAttackBonus +
                Water.TempHarpoonAttackBonus;                                                     // Harpoon attack bonus pole bonus
                power += Util.Percent( pr, power );
                harpoon = true;   
            }

            Unit fog = Controller.GetFog( un.Pos );
            if( fog )
                perc += Water.TempFogExtraAttack + G.HS.FogExtraAttack;                           // Fish under fog Bonus Attack

            power = Util.Percent( perc, power );                                                  // apply calculation

            if( perc >= 80 )                                                                      // Bulls eye
            {
                hk.Sprite.color = Color.green;
                power += Util.Percent( 100, power );
                hk.Sprite.spriteId = 386;
            }
            else                                                                                  // Normal power
            {
                hk.Sprite.color = new Color( 
                Util.Percent( 100 - perc, 1 ), Util.Percent( perc, 1 ), 0, 1 );
                hk.Sprite.spriteId = 385;
            }

            if( perc > 0 )
            {
                if( un.Control.OverFishSecondsNeededPerUnit > 0 )                                 // Over fish count increment
                    OverFishCount++;
                float hdist = Vector2.Distance( hk.transform.position, un.transform.position );

                if( hdist < hk.ClosestFishDistance )                                               // Measures fish distance from hook and only give over fish bonus to the closest
                if( un.Body.FishType != EFishType.FISH_FROG )
                {
                    hk.ClosestFishDistance = hdist;
                    hk.ClosestFish = un;
                }
                else                    
                    hk.ForcedMoveDir = Vector3.zero;                                                // Force moving hook stops on frog collision
            }

            power = Util.Percent( un.Control.FishingPowerFactor, power );                           // Fishing Power Factor

            float tot = GetTotRecord();
            power += Util.Percent( tot, power );                                                    // Over Fish Bonus Record

            float damage = power * Time.deltaTime;                                                  // Final damage

            if( harpoon )
            {
                Water.TempHarpoonAttackPercent += damage;
                Water.CheckCn( EPoleBonusCnType.HARPOON_ATTACK_X_PERCENT, un );                     // Condition: Harpoon attack x %
            }
            Water.CheckCn( EPoleBonusCnType.ATTACK_X_VIRGIN_FISH, un );                             // Condition: first Attack number of fish
            Water.CheckCn( EPoleBonusCnType.FIRST_ATTACK_VIRGIN, un );                              // Condition: first Attack Virgin

            un.Body.Hp += damage;                                                                   // Perform attack

            hk.TargetFishList.Add( un );

            un.Control.FAOverFishTimeCount += Time.deltaTime;                                       // Total time for hook over fish
            un.Control.FAOverFishTimeCountPerFA += Time.deltaTime;                                  // Total time for hook over fish
            
            Water.UpdatePoleBonusOnFishAttacked( un );                                              // Updates pole bonus stuff while attacking fish

            //Debug.Log( "power: " + power + " perc " + perc + "%" );
        }

        if( soundFxId == 1 ) 
            MasterAudio.PlaySound3DAtVector3( "Item Collect", G.Hero.Pos );
    }    

    public bool DoesTileDestroyBuoy( Vector2 from, int x, int y, EDirection movedir, ref bool block, bool tilechange, FishingObject hk )
    {
        Unit water = GetUnit( ETileType.WATER, new Vector2( x, y ) );
        Unit arrow = GetUnit( ETileType.ARROW, new Vector2( x, y ) );
        Vector3 vec = Vector3.zero;
        if( movedir != EDirection.NONE )        
            movedir = Util.GetInvDir( movedir );
        if( from.x == -1 && movedir != EDirection.NONE )
            from = new Vector2( x, y ) + Manager.I.U.DirCord[ ( int ) movedir ];

        if( water )
        {
            block = false;
            List<Unit> ol = FUnit[ x, y ];                                                                        // Jumper Buoy blocking
            if ( FishingMode != EFishingPhase.NO_FISHING )
            if ( ol != null )
            for( int i = 0; i < ol.Count; i++ )
            if ( ol[ i ].TileID == ETileType.FISH )
            if ( ol[ i ].Body.FishType == EFishType.FISH_FROG ) block = true;

            Unit orb = Map.I.GetUnit( ETileType.ORB, new Vector2( x, y ) );
            if( orb ) block = true;
            Unit raft = Controller.GetRaft( new Vector2( x, y ) );
            if( raft )
            {
                if( G.HS.RaftDestroy || Water.TempRaftDestroy )                                                   // Hook bonus Raft destroy
                {
                    Water.DestroyRaft( raft );
                    return false;
                }

                if( hk )
                    if( G.HS.ImpulsionateEnabled || Water.TempImpulsionateEnabled )                                   // Hook bonus Raft Impulsionate
                {                   
                    vec = GetInputVector( false );
                    Controller.ImpulsionateRaft( hk.TilePos - new Vector2( vec.x, vec.y ), hk.TilePos, false );
                    hk.Dest = hk.transform.position - ( vec * .5f );
                    MasterAudio.PlaySound3DAtVector3( "Bump", hk.TilePos );
                    Map.I.CreateExplosionFX( hk.transform.position );
                    return false;
                }

                block = true;
                Unit monster = GetUnit( new Vector2( x, y ), ELayerType.MONSTER );
                if( monster && monster.TileID == ETileType.BOULDER ) return false;
                if( orb ) return false;
                return true;
            }

            if( hk )
            if( BabyData.UpdateWaterArrowBlock( hk.transform.position, ref block, movedir, x, y ) )                 // water arrow check
            {
                if( block )
                    return false;
            }

            if( arrow )                                                                                            // Over water arrow block
            {
                vec = GetInputVector( false );
                EDirection dr = Util.GetTargetUnitDir( Vector2.zero, -vec );
                if( G.HS.ArrowPush || Water.TempArrowPush )                                                        // Arrow Push pole bonus
                if( dr != EDirection.NONE )
                {
                    Vector2 tg = new Vector2( x, y ) - Manager.I.U.DirCord[ ( int ) dr ];
                    Unit waterto = GetUnit( ETileType.WATER, tg );
                    Unit arrowto = GetUnit( ETileType.ARROW, tg );
                    if( waterto != null && Controller.GetRaft( tg ) == null && arrowto == null )
                    {
                        arrow.Control.ApplyMove( new Vector2( x, y ), tg );                        
                        arrow.Graphic.transform.localPosition = Vector3.zero;
                        if( hk ) Map.I.CreateExplosionFX( hk.transform.position );
                    }
                }
                if( tilechange == false )
                if( movedir != EDirection.NONE )
                {
                    Vector2 to = new Vector2( x, y ) + Manager.I.U.DirCord[ ( int ) Util.GetInvDir( movedir ) ];
                    if( CheckArrowBlockFromTo( new Vector2( x, y ), to,
                        G.Hero, true, 5, ( int ) G.Hero.Control.ArrowOutLevel ) == true )
                        block = true;
                    return false;
                }
                if( CheckArrowBlockFromTo( from, new Vector2( x, y ), G.Hero, true ) == true )
                    block = true;
                if( G.HS.ArrowDestroy || Water.TempArrowDestroy )                                                  // Arrow Destroy pole bonus
                {
                    Map.Kill( arrow );
                    if( hk ) Map.I.CreateExplosionFX( hk.transform.position );
                }
            }
            return false;
        }

        block = true;
        if( IsWall( new Vector2( x, y ) ) )
        {
            return false;
        }

        if( water == null )
        {
            if( arrow )
            {
                if( movedir == EDirection.NONE ) return false;
                else
                {
                    if( CheckArrowBlockFromTo( from, new Vector2( x, y ), G.Hero, true ) == false ) return true;
                }
                return false;
            }
            return true;
        }
        return false;
    }
    
    public void UpdatePondID()
    {
        int id = 1;
        int cid = 1;
        Sector s = RM.HeroSector;
        PondID = new int[ Tilemap.width, Tilemap.height ];
        ContinuousPondID = new int[ Tilemap.width, Tilemap.height ];
        for( int yy = ( int ) s.Area.yMin - 1; yy < s.Area.yMax + 1; yy++ )
        for( int xx = ( int ) s.Area.xMin - 1; xx < s.Area.xMax + 1; xx++ )
        if ( Map.PtOnMap( Tilemap, new Vector2( xx, yy ) ) )
        {
            if( Gaia[ xx, yy ] != null )            
            if( Gaia[ xx, yy ].TileID == ETileType.WATER )
            {
                if( PondID[ xx, yy ] <= 0 )
                    SetPondId( new Vector2( -1, -1 ), new Vector2( xx, yy ), ++id );
                if( ContinuousPondID[ xx, yy ] <= 0 )
                    SetContinuousPondId( new Vector2( -1, -1 ), new Vector2( xx, yy ), ++cid );
            }
        }
    }
    
    public bool IsWater( Vector2 pt )
    {
        if( GetUnit( ETileType.WATER, pt ) ) return true;
        return false;
    }

    public bool ContinuousWater( Vector2 pt1, Vector2 pt2 )
    {
        if( pt1.x == pt2.x ) return true;
        if( pt1.y == pt2.y ) return true;

        Vector2 dif = pt2 - pt1;
        bool ok = false;

        if( dif == new Vector2( 1, 1 ) )
        {
            if( IsWater( pt1 + new Vector2( 0, 1 ) ) == true )
                if( IsWater( pt1 + new Vector2( 1, 0 ) ) == true ) ok = true;
        }
        if( dif == new Vector2( 1, -1 ) )
        {
            if( IsWater( pt1 + new Vector2( 1, 0 ) ) == true )
                if( IsWater( pt1 + new Vector2( 0, -1 ) ) == true ) ok = true;
        }
        if( dif == new Vector2( -1, -1 ) )
        {
            if( IsWater( pt1 + new Vector2( 0, -1 ) ) == true )
                if( IsWater( pt1 + new Vector2( -1, 0 ) ) == true ) ok = true;
        }

        if( dif == new Vector2( -1, 1 ) )
        {
            if( IsWater( pt1 + new Vector2( 0, 1 ) ) == true )
                if( IsWater( pt1 + new Vector2( -1, 0 ) ) == true ) ok = true;
        }
        return ok;
    }

    public bool CanSwimFromTo( Vector2 from, Vector2 to )
    {
        if( IsWater( to ) == false ) return false;
        List<Unit> ol = FUnit[ ( int ) to.x, ( int ) to.y ];                                        // closed water trap blocking
        if ( ol != null )
        for( int i = 0; i < ol.Count; i++ )
        if ( ol[ i ].TileID == ETileType.FISH )
            {
                if( ol[ i ].Body.FishType == EFishType.CLOSED_WATER_TRAP ) return false;
                if( ol[ i ].Body.FishType == EFishType.INT_TILE ) return false;
            }
        if( from.x != -1 )
        if( ContinuousWater( from, to ) == false ) return false;
        Unit raft = Controller.GetRaft( to );
        if( raft ) return false;
        if( Map.I.GetUnit( ETileType.ORB, to ) ) return false;
        if( Map.I.CheckArrowBlockFromTo( from, to, null, true ) ) return false;
        return true;
    }

    public bool SetPondId( Vector2 from, Vector2 pos, int id )
    {
        if( PtOnMap( Tilemap, pos ) == false ) return false;

        if( Gaia[ ( int ) pos.x, ( int ) pos.y ] == null ) return false;
        if( Gaia[ ( int ) pos.x, ( int ) pos.y ].TileID != ETileType.WATER )
            return false;
        if( PondID[ ( int ) pos.x, ( int ) pos.y ] > 0 ) return false;
        Unit arrow = GetUnit( ETileType.ARROW, pos );
        if( arrow ) return false;
        if( CanSwimFromTo( from, pos ) == false ) return false;

        PondID[ ( int ) pos.x, ( int ) pos.y ] = id;
        SetPondId( pos, new Vector2( pos.x - 1, pos.y ), id );
        SetPondId( pos, new Vector2( pos.x + 1, pos.y ), id );
        SetPondId( pos, new Vector2( pos.x, pos.y - 1 ), id );
        SetPondId( pos, new Vector2( pos.x, pos.y + 1 ), id );
        SetPondId( pos, new Vector2( pos.x + 1, pos.y + 1 ), id );
        SetPondId( pos, new Vector2( pos.x + 1, pos.y - 1 ), id );
        SetPondId( pos, new Vector2( pos.x - 1, pos.y - 1 ), id );
        SetPondId( pos, new Vector2( pos.x - 1, pos.y + 1 ), id );
        return true;
    }

    public bool SetContinuousPondId( Vector2 from, Vector2 pos, int id )
    {
        if( PtOnMap( Tilemap, pos ) == false ) return false;

        if( Gaia[ ( int ) pos.x, ( int ) pos.y ] == null ) return false;
        if( Gaia[ ( int ) pos.x, ( int ) pos.y ].TileID != ETileType.WATER )
            return false;
        if( ContinuousPondID[ ( int ) pos.x, ( int ) pos.y ] > 0 ) return false;

        ContinuousPondID[ ( int ) pos.x, ( int ) pos.y ] = id;
        SetContinuousPondId( pos, new Vector2( pos.x - 1, pos.y ), id );
        SetContinuousPondId( pos, new Vector2( pos.x + 1, pos.y ), id );
        SetContinuousPondId( pos, new Vector2( pos.x, pos.y - 1 ), id );
        SetContinuousPondId( pos, new Vector2( pos.x, pos.y + 1 ), id );
        return true;
    }

    public void UpdateFishingPoleStepping( Vector2 from, Vector2 tg )
    {
        Unit pole = GFU( ETileType.FISHING_POLE, tg );
        if( pole == null ) 
        { 
            Water.HeroLeftHook = true; 
            return; 
        }

        if( G.HS.AddNextPoleBonusVal != 0 )
        {
            pole.Water.PoleBonusVal1 += G.HS.AddNextPoleBonusVal;                                        // Add bonus to next pole
            Controller.CreateMagicEffect( pole.Pos );
            MasterAudio.PlaySound3DAtVector3( "Click 2", G.Hero.Pos );
            UI.I.SetBigMessage( "", Color.yellow, .1f );
            Message.GreenMessage( "Pole Bonus " +
            G.HS.AddNextPoleBonusVal.ToString( "+0;-#" ) );
            G.HS.AddNextPoleBonusVal = 0;
        }

        if( G.HS.MultiplyNextPoleBonusVal != 0 )
        {
            pole.Water.PoleBonusVal1 *= G.HS.MultiplyNextPoleBonusVal;                                    // multiply next pole bonus
            Controller.CreateMagicEffect( pole.Pos );
            MasterAudio.PlaySound3DAtVector3( "Click 2", G.Hero.Pos );
            UI.I.SetBigMessage( "", Color.yellow, .1f );
            Message.GreenMessage( "Pole Bonus x" + G.HS.MultiplyNextPoleBonusVal );
            G.HS.MultiplyNextPoleBonusVal = 0;
        }

        if( pole.Activated == false )
        {
            if( Water.HeroLeftHook == true )  
            if( Item.GetNum( ItemType.Res_Fishing_Pole ) >= 1 )                                            // reactivates an inactive Pole by using tokens 
            {
                Item.AddItem( ItemType.Res_Fishing_Pole, -1 );
                pole.Activate( true );
                pole.Water.PoleBonusFailed = false;
                pole.Water.PoleBonusActive = true;
                pole.Water.UpdatePoleText( true );
                Controller.CreateMagicEffect( pole.Pos );                                                 // Create Magic effect
                MasterAudio.PlaySound3DAtVector3( "Click 2", G.Hero.Pos );
            }
            return; 
        }
        MainHook = HookList[ 0 ];
        EDirection dr = Util.GetTargetUnitDir( from, tg );
        if( dr == EDirection.NONE ) return;
        BabyData.OverArrowDir = EDirection.NONE;
        if( HeroData.I.GetVal( EHeroDataVal.FISHING_LEVEL ) < 1 )
        {
            Message.RedMessage("You Don't Know how to Fish.\nImprove your Fishing Level first!");
            return;
        }

        UpdatePondID();   
        Vector2 pt = G.Hero.Pos + Manager.I.U.DirCord[ ( int ) dr ] * 3;
        Vector2 tgg = G.Hero.Pos + Manager.I.U.DirCord[ ( int ) dr ];                                       // hero over arrow 
        if( CheckArrowBlockFromTo( G.Hero.Pos, tgg, G.Hero ) == true ) return;        

        int fishingPondId = -1;
        Vector2 pos = new Vector2( -1, -1 );                                   
        for( int d = 1; d < Sector.TSX; d++ )                                                               // Initial Hook pos calculation
        {
            if( d > Map.I.RM.RMD.MaxPoleDistanceForFishing ) return;
            pos = tg + Manager.I.U.DirCord[ ( int ) dr ] * d;
            if( Map.PtOnMap( Map.I.Tilemap, pos ) == false ) return;
            bool block = false;
            DoesTileDestroyBuoy( new Vector2( -1, -1 ), ( int ) pos.x, 
            ( int ) pos.y, dr, ref block, true, null );
            Unit water = GetUnit( ETileType.WATER, pos );
            if( water == null ) return;                                                                     // Bad fishing Position
            if( d == 1 && water == null )
            if( G.Hero.CanMoveFromTo( false, tg, tg + Manager.I.U.DirCord[
              ( int ) dr ], G.Hero ) == false ) return;

            if( d == 1 && d < Sector.TSX - 1 )
            if( CheckArrowBlockFromTo( tg, pos, G.Hero ) == true ) return;                                  // Arrow Block
            if( block == false )
            {
                FishingObject.Add( pos, 1, new Vector2( -1, -1 ) );
                fishingPondId = PondID[ ( int ) pos.x, ( int ) pos.y ];
                CurrentContinuousPondId = ContinuousPondID[ ( int ) pos.x, ( int ) pos.y ];
                break;
            }            
        }
        FishingLine.ObjectB = PoleEnd.gameObject;
        G.Hero.RotateTo( dr );
        CurrentFishingPole = pole;
        pole.Spr.color = new Color( 1, 1, 1, .0f );
        pole.Body.Sprite2.gameObject.SetActive( false );
        Water.ResetVars();
        Water.HeroLeftHook = false;
        Water.SimultaneousCatchFish = 0;
        Water.HookDropPosition = pos;
        Water.PoleBonusText = "";
        Water.ExtraHooksGiven = false;
        NumFishCaught = 0;
        FishCaughtList = "";
        NumActiveHooks = 1;
        FishingTimerCount = 2f;
        FishingLineBreakTimerCount = -1;
        PerfectFishing = true;
        FishingMode = EFishingPhase.INTRO;
        UI.I.SetBigMessage( "Fishing in: ", Color.yellow, 3f, -1, 850 );
        MasterAudio.PlaySound3DAtVector3( "Fishing Reel", G.Hero.transform.position );
        FishingLine.transform.parent.gameObject.SetActive( true );
        FishingLine.AutoCalculateAmountOfNodes = true;
        MinHook = MaxHook = Vector2.zero;
        UI.I.SelectedPerk = EPerkType.FISHING;
        UI.I.UpdateInfoPanel();
        Util.PlayParticleFX( "Water Splash", MainHook.transform.position );  
        
        for( int u = RM.HeroSector.Fly.Count - 1; u >= 0; u-- )
        {
            Unit fl = RM.HeroSector.Fly[ u ];
            if( pole.Control.RandomizePositionOnPoleStep ||                
                fl.Control.RandomizePositionOnPoleStep || 
                fl.Control.RandomizePositionOnRespawn )
            if( fl.TileID == ETileType.FISH )
            {
                RandomizeWaterObjectPosition( fl,                                           // Randomize fish position
                pole.Control.RandomizePositionOnPoleStep );
                fl.Control.RespawnTimeCount = 0;
                fl.Control.RespawnCount = 0;
            }
            if( fl.TileID == ETileType.FISH )
                fl.Water.IgnoreAttackTime = 0;
        }

        int plid = -1;                                                                      // Checks to seee if there's a pond info for
        Sector s = RM.HeroSector;
        for( int yy = ( int ) s.Area.yMin - 1; yy < s.Area.yMax + 1; yy++ )
        for( int xx = ( int ) s.Area.xMin - 1; xx < s.Area.xMax + 1; xx++ )
        if ( Map.PtOnMap( Tilemap, new Vector2( xx, yy ) ) )
        {
            if( Gaia[ xx, yy ] != null )
            if( Gaia[ xx, yy ].TileID == ETileType.WATER )
            for( int i = 0; i < PondList.Count; i++ )
            {
                if( PondList[ i ].TilePos == new Vector2( xx, yy ) )
                    if( fishingPondId == PondList[ i ].PondID )
                        plid = i;
            }

            Unit algae = Map.I.GetUnit( ETileType.ALGAE, new Vector2( xx, yy ) );            // init algae 
            if( algae )
                BabyData.StartFishingInit( algae );
        }

        if( plid == -1 )                                                                     // No Pond info. Create a new one
        {
            PondInfo p = new PondInfo();
            p.TilePos = pos;
            p.PondID = fishingPondId;
            p.OverFishRecordTime = 0;
            p.OverFishBonusRecord = 0;
            p.OverFishCummulativeBonus = 0;
            p.OverFishAffectedUnitsRecordCount = 0;
            PondList.Add( p );
            plid = PondList.Count - 1;
        }

        CurPond = PondList[ plid ];
        for( int h = 0; h < HookList.Count; h++ )                                            // Init Hooks
            HookList[ h ].Init();

        Water.CheckCn( EPoleBonusCnType.STEP_THE_POLE, null );                               // Step the Pole hook bonus check
        Water.CheckCn( EPoleBonusCnType.HAVE_X_FISH_IN_INVENTORY, null );                    // Have x fish in inventory hook bonus check
        Water.CheckCn( EPoleBonusCnType.CONQUER_X_HOOK_BONUS, null );                        // Conquer x bonuses hook bonus check
    }
   
    public void RandomizeWaterObjectPosition( Unit un, bool pole )
    {
        if( pole )
        if( IsNormalFish( un ) == false ) return;
        if( PondID == null ) UpdatePondID();
        int pond = PondID[ ( int ) un.Pos.x, ( int ) un.Pos.y ];
        Unit arrow = GetUnit( ETileType.ARROW, un.Pos );                                    // since arrows delimitate pond id, do no randomize pos if over arrows
        if( arrow ) return;
        Unit raft = Controller.GetRaft( un.Pos );                                           // raft too
        if( raft ) return;
        List<Unit> ol = FUnit[ ( int ) un.Pos.x, ( int ) un.Pos.y ];
        if( ol != null )
        for( int i = 0; i < ol.Count; i++ )
        if( ol[ i ].Body.FishType == EFishType.INT_TILE )                                   // Int tile too
            return;

        Sector s = RM.HeroSector;
        List<Vector2> pl = FishingObject. GetRandomWaterObjPosition( pond ); 

        if( pl != null && pl.Count > 0 )
        {
            int id = Random.Range( 0, pl.Count );
            float amt = .3f;
            Vector3 rand = new Vector3( Random.Range( -amt, amt ), 
                                        Random.Range( -amt, amt ), 0 );
            un.transform.position = pl[ id ];
            un.transform.position += rand;
            un.Control.UpdateFlyingTile();
            un.Control.FlyingTarget = new Vector2( -1, -1 );
        }
    }

    public bool IsNormalFish( Unit un )
    {
        if( un.Body.FishType == EFishType.FISH_1 ) return true;
        if( un.Body.FishType == EFishType.FISH_2 ) return true;
        if( un.Body.FishType == EFishType.FISH_3 ) return true;
        if( un.Body.FishType == EFishType.FISH_CRAB ) return true;
        if( un.Body.FishType == EFishType.FISH_MANTA ) return true;
        if( un.Body.FishType == EFishType.FISH_SNAKE ) return true;
        if( un.Body.FishType == EFishType.FISH_BROWN ) return true;
        if( un.Body.FishType == EFishType.FISH_FROG ) return true;
        return false;
    }

    public void CreateFish( Unit un )
    {
        un.Body.FishType = ( EFishType ) Random.Range( ( int ) EFishType.FISH_1, ( int ) EFishType.FISH_FROG + 1 );
        un.Body.IsFish = true;
        un.Body.Hp = 0;
        un.Body.FishCaught = false;
        un.Control.MinFishSpeed = .5f;
        un.Control.MaxFishSpeed = 2f;
        un.Control.FlyingSpeed = Random.Range( .5f, 2f );
        FishActionStruct.ResetAll( un );
        un.Spr.transform.localPosition = new Vector3( 0, 0, un.Spr.transform.localPosition.z );
        un.Spr.transform.localScale = new Vector3( 1, 1, 1 );
        un.Body.Sprite3.gameObject.SetActive( false );
        InitFishGraphics( un );
    }

    public void SetWaterObject( Unit un, EFishType type )
    {
        un.Body.FishType = type;
        InitFishGraphics( un );
        if( type == EFishType.CLOSED_WATER_TRAP ||
            type == EFishType.OPEN_WATER_TRAP )
        {
            MasterAudio.PlaySound3DAtVector3( "Raft Merge", un.Pos );
            PondID = null;
        }
        if( type == EFishType.CLOSED_WATER_TRAP ) 
            InvalidateFishTargets( un.Pos );
    }
    public void InvalidateFishTargets( Vector2 tg )
    {
        if( ContinuousPondID == null ) return;
        if( PondID == null ) return;
        for( int u = 0; u < RM.HeroSector.Fly.Count; u++ )
        {
            Unit un = RM.HeroSector.Fly[ u ];
            if( un.TileID == ETileType.FISH )
            if( ContinuousPondID[ ( int ) tg.x, ( int ) tg.y ] == CurrentContinuousPondId )
            if( IsInTheSameLine( un.Pos, tg ) )
                {
                    un.Control.FlyingTarget = new Vector2( -1, -1 );
                }
        }
    }
    public void InitFishGraphics( Unit un )
    {
        if( un.TileID != ETileType.FISH ) return;
        un.Activate( true );
        un.Graphic.gameObject.SetActive( true );
        un.Graphic.transform.localPosition = new Vector3( 0, 0, 0 );
        un.gameObject.SetActive( true );
        un.Body.Animator = un.Spr.GetComponent<tk2dSpriteAnimator>();
        un.Body.Sprite2.gameObject.SetActive( false );
        un.Body.Sprite3.gameObject.SetActive( false );
        if( un.Water.GlowingFish )
            un.Body.Sprite3.gameObject.SetActive( true );
        un.Spr.transform.localScale = new Vector3( 1, 1, 1 );
        switch( un.Body.FishType )
        {
            case EFishType.FISH_1:
            un.Body.Animator.Play( "Yellow Fish Swimming" );
            break;
            case EFishType.FISH_2:
            un.Body.Animator.Play( "Red Fish Swimming" );
            break;
            case EFishType.FISH_3:
            un.Body.Animator.Play( "Blue Fish Swimming" );
            break;
            case EFishType.FISH_CRAB:
            un.Body.Animator.Play( "Black Fish Swimming" );
            break;
            case EFishType.FISH_MANTA:
            un.Body.Animator.Play( "Manta Ray 1 Swimming" );
            break;
            case EFishType.FISH_BROWN:
            un.Body.Animator.Play( "Manta Ray 2 Swimming" );
            break;
            case EFishType.FISH_SNAKE:
            un.Body.Animator.Play( "Snake Swimming" );
            break;
            case EFishType.FISH_FROG:
            un.Body.Animator.Play( "Frog Swimming" );
            break;
            case EFishType.FAST_TILE:
            un.Spr.spriteId = 480;
            un.Body.Animator.Stop();
            break;
            case EFishType.BONUS_TIME:
            un.Body.Sprite2.spriteId = 481;
            un.Body.Sprite2.gameObject.SetActive( true );
            un.Body.Animator.Play( "Black Manta Ray Swimming" );
            un.Spr.transform.localScale = new Vector3( 2, 2, 1 );
            break;
            case EFishType.BONUS_HOOK_SMALL:
            un.Body.Sprite2.spriteId = 487;
            un.Body.Sprite2.gameObject.SetActive( true );
            un.Body.Animator.Play( "Black Manta Ray Swimming" );
            un.Spr.transform.localScale = new Vector3( 2, 2, 1 );
            break;
            case EFishType.BONUS_HOOK_MEDIUM:
            un.Body.Sprite2.spriteId = 488;
            un.Body.Sprite2.gameObject.SetActive( true );
            un.Body.Animator.Play( "Black Manta Ray Swimming" );
            un.Spr.transform.localScale = new Vector3( 2, 2, 1 );
            break;
            case EFishType.BONUS_HOOK_LARGE:
            un.Body.Sprite2.spriteId = 489;
            un.Body.Sprite2.gameObject.SetActive( true );
            un.Body.Animator.Play( "Black Manta Ray Swimming" );
            un.Spr.transform.localScale = new Vector3( 2, 2, 1 );
            break;
            case EFishType.CLOSED_WATER_TRAP:
            un.Spr.spriteId = 483;
            un.Body.Animator.Stop();
            break;
            case EFishType.OPEN_WATER_TRAP:
            un.Spr.spriteId = 484;
            un.Body.Animator.Stop();
            break;
            case EFishType.BAIT:
            un.Spr.spriteId = 485;
            un.Body.Animator.Stop();
            break;
            case EFishType.HOOK_PULL:
            un.Spr.spriteId = 486;
            un.Body.Animator.Stop();
            break;
            case EFishType.INT_TILE:
            un.Spr.spriteId = 482;
            un.Body.Animator.Stop();
            break;
            case EFishType.TIME_SKIP:
            un.Body.Sprite2.spriteId = 490;
            un.Body.Sprite2.gameObject.SetActive( true );
            un.Body.Animator.Play( "Black Manta Ray Swimming" );
            un.Spr.transform.localScale = new Vector3( 2, 2, 1 );
            break;
            case EFishType.BONUS_FISHING_LEVEL:
            un.Body.Sprite2.spriteId = 491;
            un.Body.Sprite2.gameObject.SetActive( true );
            un.Body.Animator.Play( "Black Manta Ray Swimming" );
            un.Spr.transform.localScale = new Vector3( 2, 2, 1 );
            break;
        }
        if( un.Body.IsFish )
        {
            un.Spr.transform.eulerAngles = new Vector3( 0, 0, Random.Range( 0, 360 ) );
            un.Body.EffectList[ 0 ].gameObject.SetActive( true );
        }
        else
        {
            un.Spr.transform.eulerAngles = new Vector3( 0, 0, 0 );
            un.Spr.transform.position = new Vector3( un.Spr.transform.position.x, 
            un.Spr.transform.position.y, un.Spr.transform.position.z + .01f );
            un.Body.EffectList[ 0 ].gameObject.SetActive( false );
        }
    }

    public float GetAngleDamageInPercent( Vector3 a1, Vector3 a2 )
    {
        float ang = Mathf.DeltaAngle( a1.z, a2.z );
        if( ang < 0 ) ang *= -1;
        float perc = 100 - ( 100 * ang / 180 );
        //float bonus = Util.Percent( perc, 50 );
        //Debug.Log( "" + ( a1 + "  " + a2 + "   " + ang + "  " + perc + "    " + bonus ) );
        return perc;
    }

    public void UpdateFrontalTarget()
    {
        if( FishingMode != EFishingPhase.NO_FISHING ) return;
        if( Map.I.HeroTargetSprite.gameObject.activeSelf == false ) return;

        int rad = Map.I.RM.RMD.FishCornTargetRadius + Controller.FrontalTargetManeuverDist;
        Vector2 tg = G.Hero.Pos + Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] * rad;
        List<Unit> ol = FUnit[ ( int ) tg.x, ( int ) tg.y ];         
        if ( ol != null )
        for( int i = 0; i < ol.Count; i++ )
        if ( ol[ i ].TileID == ETileType.FISH )
            {
                if( ol[ i ].Body.FishType == EFishType.CLOSED_WATER_TRAP )                                    // Water trap switching
                    SetWaterObject( ol[ i ], EFishType.OPEN_WATER_TRAP );
                else
                if( ol[ i ].Body.FishType == EFishType.OPEN_WATER_TRAP )
                    SetWaterObject( ol[ i ], EFishType.CLOSED_WATER_TRAP );
            }
    }
    public void UpdateFishBehaviour( Unit un )
    {
        if( un.Activated == false ) return;
        if( un.Control.FishAction == null || 
            un.Control.FishAction.Count <= 0 ) return;
        bool deb = false;
        if( un.Control.CurFA >= un.Control.FishAction.Count ) return;
        bool changed = false; 
        FishActionStruct old = un.Control.FishAction[ un.Control.CurFA ]; 
        bool ok = Map.I.CheckFishActionCondition( un, ref changed );                                        // Check Condition                                          
        FishActionStruct f = un.Control.FishAction[ un.Control.CurFA ];

        if( un.Control.CurFA == 0 )                                                                         // Apply the default swimming mode  ID: 0
        {
            un.Control.ActionTimeCounter = 0;
            un.Control.FishActionWaiting = true;
            if( changed )
            {
                ApplyEffect( un, un.Control.CurFA, old );
                un.Control.WaitTimeCounter = 0;
            }
        }

        if( un.Control.FishActionWaiting == false )
        {
            if( ok )
            if( un.Control.ActionTimeCounter >= f.ActionTotalTime )
                {
                    un.Control.WaitTimeCounter = 0;
                    un.Control.ActionTimeCounter = 0;

                    if( f.WaitActionID != -1 )
                    {
                        ok = true;
                        un.Control.FishActionWaiting = true;
                        un.Control.CurFA = f.WaitActionID;
                        ApplyEffect( un, f.WaitActionID, old );                                              // Apply waiting effect once
                        if( deb )
                            Debug.Log( "Started Waiting: ID: " + un.Control.CurFA );
                    }
                }
        }
        else
        {
            if( un.Control.CurFA > 0 )
            if( ( ok && f.TimesApplied <= 0 ) ||                                                        //First time: do not check timer. apply effect immediatelly
                ( f.WaitTotalTime <= 0 ||
                  un.Control.WaitTimeCounter >= f.WaitTotalTime ) )                                     // What defines wait time is not the default #0 but other
            {
                un.Control.WaitTimeCounter = 0;
                un.Control.ActionTimeCounter = 0;

                bool remaining = true;
                if( f.TotalTimesApplied > 0 )                                                             // f.TotalTimesApplied <= 0   infinite times
                if( f.TimesApplied >= f.TotalTimesApplied )
                    remaining = false;

                if( remaining )
                {
                    un.Control.FishActionWaiting = false;
                    ApplyEffect( un, un.Control.CurFA, old );                                            // Apply main effect once
                    if( deb )
                        Debug.Log( "Started Action: ID: " + un.Control.CurFA );
                }
            }
        }

        if( un.Control.FishActionWaiting )
        {
            un.Control.WaitTimeCounter += Time.deltaTime;                                                 // time increment
            un.Control.ActionTimeCounter = 0;
        }
        else
        {
            un.Control.ActionTimeCounter += Time.deltaTime;
            un.Control.WaitTimeCounter = 0;
        }

        if( Input.GetKeyDown( KeyCode.T ) )
            if( deb ) Debug.Log( "ID: " + un.Control.CurFA + " FishSwimType: " +
           un.Control.FishSwimType + "  f.EffFishSwimType: " + f.EffFishSwimType
           + " FAOverFishTimeCountPerFA: " + un.Control.FAOverFishTimeCountPerFA + 
           " un.Control.FishActionWaiting: " + un.Control.FishActionWaiting );
    }

    private void ApplyEffect( Unit un, int id, FishActionStruct old )
    {
        FishActionStruct f = un.Control.FishAction[ id ];
        switch( un.Control.FishAction[ id ].EffectType )
        {
            case EFishActionEffectType.SwimType:
            {
                un.Control.FishSwimType = f.EffFishSwimType;
                if( f.EffFishSwimType == EFishSwimType.FLEE_TO_FARTHEST )
                {
                    Util.PlayParticleFX( "Water Splash", un.transform.position ); 
                    un.Control.FlyingTarget = new Vector2( -1, -1 );                          // force new target searching      
                }
            }
            break;
        }

        if( old.ConditionType == EFishActionConditionType.HitAndRun )
            un.Control.FAOverFishTimeCountPerFA = 0;                                          // Resets over fish time counter for hit and run mode (totozinho) 
        f.TimesApplied++;
    }
    public bool CheckFishActionCondition( Unit un, ref bool changed )
    {
        int old = un.Control.CurFA;
        bool res = false;
        for( int i = 0; i < un.Control.FishAction.Count; i++ )
        {
            FishActionStruct f = un.Control.FishAction[ i ];
            switch( f.ConditionType )
            {
                case EFishActionConditionType.FishPercent:
                if( un.Body.Hp >= f.ConditionVal )
                    res = Check( i, un );
                break;
                case EFishActionConditionType.TotalSecondsOver:
                if( un.Control.FAOverFishTimeCount >= f.ConditionVal )
                    res = Check( i, un );
                break;
                case EFishActionConditionType.HitAndRun:
                if( un.Control.FAOverFishTimeCountPerFA >= f.ConditionVal )
                {
                    res = Check( i, un );
                }
                break;
                case EFishActionConditionType.WhileFishing:
                if( Map.I.FishingMode != EFishingPhase.NO_FISHING )
                {
                    res = Check( i, un );
                }
                break;
                case EFishActionConditionType.WhileNotFishing:
                if( Map.I.FishingMode == EFishingPhase.NO_FISHING )
                {
                    res = Check( i, un );
                }
                break;
            }
        }
        if( un.Control.CurFA != old ) changed = true;
        return res;
    }
    public bool Check( int i, Unit un )
    {
        if( i > un.Control.CurFA )
        {
            if( i > un.Control.CurFA )
                un.Control.CurFA = i;
        }
        return true;
    }
    internal void CreateExplosionFX( Vector2 tg, string type = "Fire Explosion", string snd = "Explosion 2" )
    {
        Transform tr = PoolManager.Pools[ "Pool" ].Spawn( type );                                        // FX
        tr.position = new Vector3( tg.x, tg.y, -6 );
        ParticleSystem pr = tr.gameObject.GetComponent<ParticleSystem>();
        pr.Stop();
        pr.Play();
        if( snd != "" )
            MasterAudio.PlaySound3DAtVector3( snd, tg );
    }
}

