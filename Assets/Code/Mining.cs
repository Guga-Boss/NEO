using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using Sirenix.OdinInspector;
using DigitalRuby.LightningBolt;
using PathologicalGames;

public partial class Controller : MonoBehaviour
{
    public static bool UpdateMining( Vector2 from, Vector2 tg )
    {
        if( Manager.I.GameType != EGameType.CUBES ) return false;
        if( Map.I.CheckArrowBlockFromTo( from, tg, G.Hero ) ) return false;             // Arrow Block
        if( Sector.GetPosSectorType( from ) == Sector.ESectorType.GATES ) 
            return false;

        if( UpdateWheelMine( from, tg ) == true ) return false;                         // Updates Wheel Mine

        Physics2D.gravity = new Vector2( Random.Range( -3, +3 ),                        // Alter gravity for animation purposes
                                         Random.Range( -3, +3 ) );

        if( UpdateAlcovePush( tg, from ) ) return false;                                // Update Mine Alcove Push

        if( UpdateLeverMineBump( from, tg ) ) return false;                             // Update Lever Mine

        if( UpdateSwapperMine( from, tg ) ) return false;                               // Update Swapper Mine

        if( UpdateWedgeMine( tg, from ) ) return false;                                 // Update wedge Mine 

        Unit mine = Map.GFU( ETileType.MINE, tg );                                      // No mine in the target
        if( mine == null )
        {
            UpdateEmptyMineAttack( from, tg );                                          // If theres no mine in the target, add code here
            return false;                                                               // No mine in the target
        }

        if( UpdateGloveMine( mine, from, tg ) ) return false;                           // Updates Glove Mine 

        if( UpdateMineJump( mine, from, tg ) ) return false;                            // Updates Mine Jump vault power

        if( UpdateJumperMine( mine, from, tg ) ) return false;                          // Updates Jumper Mine

        if( Map.IsClimbable( from, false ) )
        if( G.Hero.CheckClimbing( false, from, tg, false ) == false )                   // No mining while climbing  
            return false;

        if( UpdateVaultBump( mine, tg, from ) ) return false;                           // Update Vault Bump

        if( UpdateMagnetMine( mine, tg, from ) ) return false;                          // Update Magnet Mine

        if( UpdateCogMine( mine, tg, from ) ) return true;                              // Update Cog Mine

        if( UpdateRopeMine( mine, tg, from ) ) return false;                            // Update Rope Mine

        if( UpdateDynamiteMine( mine, tg, from ) ) return false;                        // Update Dynamite Mine

        if( UpdateCannonMine( mine, tg, from ) ) return false;                          // Update Cannon Mine

        if( UpdateChiselMine( mine, tg, from ) ) return false;                          // Update Chisel Mine

        if( UpdateArrowMine( mine, tg, from ) ) return true;                            // Update Arrow Mine

        if( CanBeMined( tg, true ) == false )                                           // These cannot be mined
            return false;

        if( G.Hero.Body.MiningLevel < 1 )
        {
            //Message.RedMessage( "Improve Your Mining Level" );                                             // Not enough mining Level
            return false;
        }

        bool res = false;
        bool up = false;
        float price = GetMiningPrice( tg, mine, ref up, from );                                                // Get Mining Price       

        price = GetHammerMinePrice( price, tg, mine, ref up, from );                                         // Hammer mine price: smallest is chosen

        float mp = Item.GetNum( ItemType.Res_Mining_Points );
        if( price > mp )                                                                                     // Not enough mining points
        {
            Message.CreateMessage( ETileType.NONE, ItemType.Res_Mining_Points,
            "Needed!\nNeed: " + price + "\nAvailable: " + ( int ) mp, mine.Pos, Color.red );
            return false;
        }
        if( Mine.SafeMining )
        if( price > 1 || price == 0 )
        {
            string msg = "Safe Mining!\nTry Again!";
            if( price == 0 ) msg = "Destroy?";
            Message.GreenMessage( msg );                                                                     // Safe Mining Msg
            Mine.SafeMining = false;                                                                         // Safe Mining
            return false;
        }
        bool frontface = true;
        float def = 50;
        string snd = "Mining 2";

        float chance = GetMineChance( mine, ref def );                                                         // gets mine chance

        bool sort = Util.Chance( chance );                                                                     // Sorts Mine break

        EDirection destdr = Util.GetTargetUnitDir( from, tg );
        Vector2 desttg = tg + Manager.I.U.DirCord[ ( int ) destdr ];                                           // find Hammer mine position
        Unit hammermine = Map.GFU( ETileType.MINE, desttg +
        Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] );
        Unit basemine = Map.GFU( ETileType.MINE, desttg );

        if( UpdateHoleMine( ref tg, ref mine, sort ) ) return true;                                            // Update Hole mine

        if( sort )                                                                                             // -------------------- Mining Successful!!
        {
            res = false;
            Mine.SafeMining = true;
            Mine.UpdateMiningSuccessfulBonuses( mine, ref price );                                             // Update behind mine bonuses
   
            DestroyMine( ref mine, false );                                                                    // Destroy main mine

            Mine.UpdateVaultCounter( EMineBonusCnType.MINE_X_MINE, mine );                                     // Destroy mined mine
          
            if( mine.Mine.HammerMine && basemine != null )                                                     // Hammer mine destroy mine behind
            {
                if( hammermine )
                if( destdr == EDirection.NONE ) return false;
                if( hammermine && hammermine.Body.MineLeverDir != EDirection.NONE )                
                {
                    mine.Mine.HammerMine = false;
                    Map.I.CreateExplosionFX( hammermine.Pos );                                                 // Explosion FX
                    DestroyMine( ref hammermine, true );                                                       // destroy hammer mine tg
                }
                DestroyMine( ref mine, true );                                                                 // no mine behind, destroy main        
            }
            else
            {
                if( mine.Mine.SpikedMine )                                                                     // Only destroy spike
                {
                    mine.Mine.SpikedMine = false;
                    snd = "Wood Break";
                    FallingAnimation.CreateAnim( 263, 1,
                    new Vector3( mine.Pos.x, mine.Pos.y, -1.98f ), Vector3.zero );
                }
                else
                    if( up )                                                                                  // destroy UP mine
                    {
                        mine.Body.UPMineType = EMineType.NONE;
                    }
                    else
                    {
                        mine.Kill();                                                                          // destroy mine just now to avoid bugs                        
                    }
            }
        }
        else                                                                                      // -----------------Mining Failed
        {
            if( mine.Mine.HammerMine ||                                                           // no frontfacing
                mine.Mine.HoleMine )
                frontface = false;

            AddMineCrackEffect( mine, tg );                                                       // Add mine crack effect

            if( mine.Mine.HammerMine && basemine != null )   
            if( hammermine && hammermine.Body.MineLeverDir != EDirection.NONE )
                {
                    AddMineCrackEffect( hammermine, hammermine.Pos );                             // Add Hammer mine crack effect               
                    Controller.CreateMagicEffect( hammermine.Pos, "Mine Debris" );                // Mine Debris FX
                }
            res = Mine.UpdateMiningFailedBonuses( mine, ref price );                              // Mining failed vault bonuses
            Mine.UpdateVaultCounter( EMineBonusCnType.MISS_X_ATTEMPTS, mine );                    // Fill vault data
            Controller.CreateMagicEffect( G.Hero.Pos, "Mine Debris" );                            // Mine Debris FX
        }

        if( iTween.Count( G.Hero.Spr.gameObject ) <= 0 )                                          // Pickaxe rotation animation
            iTween.PunchRotation( G.Hero.Spr.gameObject, new Vector3( 0, 0, 30 ), .5f );

        EDirection dir = Util.GetTargetUnitDir( from, tg );                                       // Rotate to face mine
        if( frontface )
            G.Hero.RotateTo( dir );
        MasterAudio.PlaySound3DAtVector3( snd, tg );
        Item.AddItem( ItemType.Res_Mining_Points, -price );                                       // Charges MP

        Message.CreateMessage( ETileType.NONE, ItemType.Res_Mining_Points, "" +
        ( -price ).ToString( "+0;-#" ), tg + new Vector2( .3f, -.75f ),
        Color.red, false, false, 2.2f, 0, -1, 50 );                                               // localized msg

        Controller.CreateMagicEffect( mine.Pos, "Mine Debris" );                                  // Mine Debris FX
        Map.I.CreateExplosionFX( tg, "Smoke Cloud", "" );                                         // Smoke Cloud FX
        Vector3 cord = Util.GetTargetUnitVector( from, tg );
        Vector3 amt = new Vector3( cord.x, cord.y, 0 ) * .4f;
        iTween.PunchPosition( mine.Graphic.gameObject, amt, .4f);                                 // punch FX 
        return res;
    }
    private static void UpdateEmptyMineAttack( Vector2 from, Vector2 tg )
    {
        Mine.SafeMining = true;                                                                   // Safe mining
        G.HS.DestroyedMinesInSequence = 0;
        G.HS.MissSameMineInSequence = 0;
    }
    #region Important
    public static int GetMiningPrice( Vector2 tg, Unit mine, ref bool up, Vector2 AttFrom )
    {
        EMineType type = mine.Body.MineType;
        EDirection dir = mine.Dir;
        if( G.Hero.Control.Floor == 2 || G.Hero.Control.StairStepDir == 0 )                        // which mine? up or down
        {
            type = mine.Body.UPMineType;
            dir = mine.Body.UPMineDir;
            up = true;
        }

        Sector s = Map.I.RM.HeroSector;
        int times = 0;
        for( int tm = 0; tm <= 4; tm++ )                                                       // Check mining relative position
        {
            times = tm;
            int dr = ( int ) Util.RotateDir( ( int ) dir, tm );
            Vector2 frontright = mine.Pos + Manager.I.U.DirCord[ dr ];
            dr = ( int ) Util.RotateDir( ( int ) dir, -tm );
            Vector2 frontleft = mine.Pos + Manager.I.U.DirCord[ dr ];
            if( AttFrom == frontright || AttFrom == frontleft ) break;
        }

        int price = 1;                                                                         // Determines mining price according to pos
        if( times == 1 ) price = 3;
        if( times == 2 ) price = 5;
        if( times == 3 ) price = 7;
        if( times == 4 ) price = 10;

        if( type == EMineType.ROUND )                                                          // Round Mine
            price = 1;

        if( type == EMineType.SQUARE )                                                         // Square Mine
        {
            price = 1;
            if( times == 1 || times == 3 ) price = 3;
        }
        if( type == EMineType.DOUBLE )                                                         // Double Mine
        {
            price = 1;
            if( times == 1 || times == 3 ) price = 3;
            if( times == 2 ) price = 5;
        }
        if( type == EMineType.UNITARY )                                                        // Unitary Mine
        {
            price = 1;
            if( times == 1 ) price = 2;
            if( times == 2 ) price = 3;
            if( times == 3 ) price = 4;
            if( times == 4 ) price = 5;
        }

        if( G.HS.FailedInflationPerSide > 0 )                                                    // Mine cost Inflation over mud hs 3 levels
        {
            int side = ( int ) Util.GetTargetUnitDir( mine.Pos, AttFrom );                       // lowest inflation
            price += mine.Body.MineSideHitCount[ side ];
        }

        //int chisel = GetNeighborChisels( tg );                                                 // Chisel discount
        //if( chisel >= 1 ) price -= chisel;
        if( price < 1 ) price = 1;                                                               // Minimum price
        if( type == EMineType.WEDGE_LEFT || type == EMineType.WEDGE_RIGHT )                      // Wedge is Exception, Costs Zero to be destroyed, but requires at least one pickaxe
        {
            price = 0;
        }
        return price;
    }
    public static void DestroyMine( ref Unit m, bool kill = true )
    {
        float amt = m.Body.MiningBonusAmount;                                                   
        if( G.HS.BagExtraBonusItemID != -1 )                                                     // Calculates bag bonus
        if( ( ItemType ) m.Body.MiningPrize == ( ItemType ) G.HS.BagExtraBonusItemID )           // Mining Bag Vault Bonus
            amt += Item.GetNum( ItemType.Res_Mining_Bag ) * G.HS.BagExtraBonusAmount;

        if( amt > 0 )
        {
            Item.IgnoreMessage = true;
            Item.AddItem( m.Body.MiningPrize, amt );                                             // gives mining prize 
            Mine.UpdateAfterResourcePickup( ( ItemType ) m.Body.MiningPrize, amt );              // Update Vault Bonuses after picking up resources 
            MasterAudio.PlaySound3DAtVector3( "Save Game 2", m.Pos );                            // Sound FX  
            Message.CreateMessage( ETileType.NONE, m.Body.MiningPrize, "" +
            ( amt ).ToString( "+0;-#" ), m.Pos + new Vector2( 0.15f, -0.2f ),                    // static message
            Color.green, false, false, 3.5f, 0, -1, 75 );
        }
        if( G.HS.XDestroyedGiveItemID >= 0 )                                                     // Xdestroyed vault bonus
        {
            string nm = G.GIT( G.HS.XDestroyedGiveItemID ).GetName();
            Item.AddItem( ( ItemType ) G.HS.XDestroyedGiveItemID, 1 /
            G.HS.XDestroyedGiveItemAmount );
        }
        
        if ( m.Mine.ChiselMine )
        for( int i = 0; i < 8; i++ )
        {
            Unit mm = Map.GFU( ETileType.MINE, m.Pos + Manager.I.U.DirCord[ i ] );               // Chisel destroyed gives bonus around mine
            if( mm )
            {
                if( mm.Body.MiningPrize != ItemType.NONE )
                if( mm.Body.MiningBonusAmount > 0 )
                    {
                        Item.AddItem( mm.Body.MiningPrize, mm.Body.MiningBonusAmount );
                        mm.Body.MiningPrize = ItemType.NONE;
                        mm.Body.MiningBonusAmount = 0;
                        Controller.CreateMagicEffect( mm.Pos );
                        MasterAudio.PlaySound3DAtVector3( "Save Game 2", mm.Pos ); 
                    }
            }
        }

        Map.I.CreateExplosionFX( m.Pos );                                                        // Explosion FX
        MasterAudio.PlaySound3DAtVector3( "Mine Destroy", m.Pos );                               // Sound FX
        if( kill ) m.Kill();                                                                     // kill it
        G.HS.MineDestroyedCount++;                                                               // Destroyed count increment
    }
    public static float GetMineChance( Unit un, ref float def )
    {
        float chance = un.Body.BaseMiningChance;
        if( G.HS.DefaultMiningChanceItemID >= 0 )
        {
            def = Item.GetNum( ( ItemType ) G.HS.DefaultMiningChanceItemID );                               // Default mining chance defined by vault bonus item ID
            chance = def;
        }
        if( G.HS.DefaultMiningChance >= 0 )
        {                                                                                                   // Default mining chance defined by vault bonus
            def = G.HS.DefaultMiningChance;
            chance = def;
        }
        if( G.HS.FailedMineBonusChance > 0 )                                                                // failed mine bonus chance
        {
            chance += un.Mine.HitCount * G.HS.FailedMineBonusChance;
        }
        chance += un.Body.ExtraMiningChance;
        int chisel = Controller.GetNeighborChisels( un.Pos );                                              
        chance += G.HS.MinesAroundChiselBonusChance * chisel;                                               // Chisel mining chance bonus
        if( un.Body.MineType == EMineType.WEDGE_RIGHT ||
            un.Body.MineType == EMineType.WEDGE_LEFT )                                                      // wedge chance 100%
            chance = 100;        
        return chance;
    }
    private bool IsConectableMine( Unit mine )
    {
        if( mine == null ) return false;
        if( mine.Body.MineType == EMineType.INDESTRUCTIBLE ||
            mine.Body.MineType == EMineType.SPIKE_BALL ||
            mine.Body.MineType == EMineType.HOOK ) return false;
        return true;
    }
    public static bool CheckMineBlock( Vector2 from, Vector2 to, Unit un, bool move, ref bool up )
    {
        EDirection mov = Util.GetTargetUnitDir( from, to );                                            // get relative direction ladder from           
        if( mov == EDirection.NONE ) return false;
        EDirection inv = Util.GetInvDir( mov );
        Unit frbr = Map.GMine( EMineType.BRIDGE, from );
        Unit tobr = Map.GMine( EMineType.BRIDGE, to );
        Unit tola = Map.GMine( EMineType.LADDER, to );
        Unit toforest = Map.I.GetUnit( ETileType.FOREST, to );

        Unit frmine = Map.GFU( ETileType.MINE, from );                                                // from mine
        Unit tomine = Map.GFU( ETileType.MINE, to );                                                  // to mine

        if( G.Hero.Control.Floor == 4 )                                                               // mining disabled from top floor
            AttemptMining = false;

        un.Control.StairStepDir = -1;
        int calc = -1;
        for( int pass = 0; pass < 2; pass++ )                                                         // First pass checks the from position and second checks the destination
        {
            Vector2 tg = from;
            if( pass == 1 ) tg = to;
            Unit la = Map.GFU( ETileType.MINE, tg );
            int outl = -1, inl = -1;                                                                   // in and out levels for bridge or stair
            List<int> all = new List<int>();
            EDirection dir = EDirection.NONE;

            if( la )
            {
                Unit gate = Map.I.GetUnit( ETileType.CLOSEDDOOR, tg );                                 // gate suspended mine 
                if( gate ) return true;
                if( la.Mine.SpikedMine )
                {
                    if( move )                                                                         // Spiked mine
                    if( un.Control.Floor == 2 )
                        Message.RedMessage( "Hit the Mine from the\nGround to Dismantle Spike." );
                    AttemptMining = false;
                    return true;
                }

                up = false;
                EMineType type = la.Body.MineType;

                dir = la.Dir;
                if( la.Body.UPMineType != EMineType.NONE )                                                        // is hero checking up ou down mine?
                { type = la.Body.UPMineType; dir = la.Body.UPMineDir; up = true; }

                if( type == EMineType.LADDER )
                {
                    if( pass == 0 )
                    {
                        all.Add( 0 );
                        if( up == false ||
                          ( tomine != null && ( tomine.Body.UPMineType == EMineType.NONE ||                       // only leave upstair to the 2nd floor without upmine 
                                                tomine.Body.UPMineType == EMineType.BRIDGE ) ) )                  // exception: descend to under bridge               
                        {
                            all.Add( 3 ); all.Add( 4 );
                        }
                    }
                    if( pass == 1 )
                    {
                        if( la.Body.MineType == EMineType.LADDER && un.Control.Floor == 0 ||                    // stair entering
                            la.Body.UPMineType == EMineType.LADDER && un.Control.Floor == 1 ||
                            la.Body.UPMineType == EMineType.LADDER && un.Control.Floor == 2 )
                        {
                            all.Add( 0 ); all.Add( 1 );
                        }
                        if( ( un.Control.Floor == 2 && up == false ) ||
                            ( un.Control.Floor == 4 || un.Control.Floor == 3 ) )
                            all.Add( 4 );                                                                        // no inverse stair climb from ground
                    }
                    outl = 0; inl = 1;
                }
                if( type == EMineType.BRIDGE )
                {
                    outl = 0; inl = 0;
                    all.Add( 0 ); all.Add( 4 );
                    bool add = false;

                    if( frbr )                                                                               // handles bridge FROM crossing
                    if( frbr.Dir == mov || frbr.Dir == inv )
                        add = true;

                    if( tobr )
                    if( tobr.Dir == mov || tobr.Dir == inv )                                                 // handles bridge TO crossing
                        add = true;

                    if( frmine && frmine.Body.UPMineType == EMineType.BRIDGE )                               // handles UPbridge FROM crossing
                    if( frmine.Body.UPMineDir == mov || frmine.Body.UPMineDir == inv )
                        add = true;

                    if( tomine && tomine.Body.UPMineType == EMineType.BRIDGE )                               // handles UPbridge TO crossing
                    if( tomine.Body.UPMineDir == mov || tomine.Body.UPMineDir == inv )
                        add = true;

                    if( frbr == null && tobr == null )
                    if( un.Control.Floor < 4 )
                        add = true;                                                                          // unit below bridge move freely

                    if( un.Control.Floor == 2 && tola != null )                                              // bridge to ladder
                        add = true;

                    if( un.Control.Floor == 4 )
                    if( tomine && tomine.Body.UPMineType == EMineType.LADDER )                               // upbridge to upladder
                        add = true;

                    if( un.UnitType == EUnitType.MONSTER )                                                   // Bridges should not restrict monsters over forest
                    if( Map.I.GetUnit( ETileType.FOREST, tg ) )
                        add = true;

                    if( pass == 1 )
                    {
                        if( un.Control.Floor == 1 || un.Control.Floor == 3 )
                        {
                            if( un.Control.StairStepDir >= 3 ) add = true;                                      // Bridge dont block hero descending to the ground
                            if( un.Control.StairStepDir == 0 ) add = true;                                      // Bridge dont block hero climbing from stair to the Bridge
                        }
                    }

                    if( un.Control.Floor < 1 )                                                               // bridge never blocks unit in the ground
                        add = true;

                    if( add ) { all.Add( 1 ); all.Add( 2 ); all.Add( 3 ); }
                }
            }

            if( la == null )
            if( un.UnitType == EUnitType.HERO )
            if( toforest )                                                                                       // No hero climb over forest
                return true;

            int lev = outl;
            if( pass == 1 ) lev = inl;
            if( lev >= 0 && mov != EDirection.NONE )                                                             // do the in and out calculations
            {
                Vector2 dest = tg + Manager.I.U.DirCord[ ( int ) mov ];
                for( int i = 0; i <= 4; i++ )
                {
                    int dr1 = ( int ) dir + i;
                    if( dr1 >= 8 ) dr1 -= 8;
                    Vector2 dest1 = la.Pos + Manager.I.U.DirCord[ dr1 ];
                    if( dest == dest1 ) { calc = i; break; }                                                     // Calc variable means the relative move direction 0 == to north of relative target bridge or stair dir
                    int dr2 = ( int ) dir - i;
                    if( dr2 < 0 ) dr2 += 8;
                    Vector2 dest2 = la.Pos + Manager.I.U.DirCord[ dr2 ];
                    if( dest == dest2 ) { calc = i; break; }
                }

                //Debug.Log( pass + "     " + calc );

                if( pass == 0 )
                if( la.Body.MineType == EMineType.LADDER ||
                    la.Body.UPMineType == EMineType.LADDER )
                    un.Control.StairStepDir = calc;                                     // set stair step dir

                if( all.Contains( calc ) )
                {
                    if( G.Hero.Control.Floor == 1 )
                    if( la.Body.MineType == EMineType.LADDER )                          // side mining while stepping the ladder
                    if( calc >= 3 )
                    if( CanBeMined( to ) ) return true;
                }
                else
                {
                    return true;
                }
            }
        }
        return false;
    }
    public static bool CanBeMined( Vector2 to, bool msg = false )
    {
        if( AttemptMining == false ) return false;
        Unit mine = Map.GFU( ETileType.MINE, to );
        if( mine == null ) return false;

        if( TypeCanBeMined( mine ) == false ) return false;

        if( G.Hero.Control.Floor == 0 ||
          ( G.Hero.Control.Floor == 1 &&
          ( G.Hero.Control.StairStepDir <= 3 && G.Hero.Control.StairStepDir > 0 ) ) )
            if( TypeCanBeMined( mine ) == false ) return false;

        if( mine.Mine.SpikedMine )                                                                         // spike bock
            if( mine.Body.UPMineType == EMineType.NONE )
                if( G.Hero.Control.Floor > 0 ) return false;

        if( msg )
        if( mine.Body.UPMineType != EMineType.NONE )                                                       // up mine block
        if( mine.Mine.SpikedMine == false )
        if( G.Hero.Control.Floor <= 1 )
        if( G.Hero.Control.Floor == 0 ||
          ( G.Hero.Control.StairStepDir <= 3 && G.Hero.Control.StairStepDir > 0 ) )
        {
            if( TypeCanBeMined( mine, true ) )
                Message.RedMessage( "Destroy Mine in the Top First!" );
            Mine.ProceedTrip = true;
            return false;
        }

        if( G.Hero.Control.Floor == 0 )
            for( int j = 0; j < 8; j++ )
            {
                Vector2 tto = to + Manager.I.U.DirCord[ j ];                                                   // Dont mine the mine that is supporting a ladder
                Unit ttom = Map.GFU( ETileType.MINE, tto );
                if( ttom )
                {
                    if( ttom.Body.MineType == EMineType.LADDER &&
                        ttom.Dir == Util.GetInvDir( ( EDirection ) j ) )
                        return false;
                    if( ttom.Body.MineType == EMineType.BRIDGE &&                                              // same to the bridge
                      ( ttom.Dir == ( EDirection ) j ||
                        ttom.Dir == Util.GetInvDir( ( EDirection ) j ) ) )
                        return false;
                }
            }

        int tofl = Controller.GetUnitFloor( G.Hero.Pos, to, G.Hero );                                                   // No cross floor mining
        if( G.Hero.Control.Floor == 0 && tofl == 4 ) return false;
        if( G.Hero.Control.Floor == 1 && tofl == 4 ) return false;
        if( G.Hero.Control.Floor == 2 && TypeCanBeMined( mine, true ) == false ) return false;
        if( G.Hero.Control.Floor == 4 ) return false;
        return true;
    }
    public static bool TypeCanBeMined( Unit un, bool up = false )
    {
        EMineType type = un.Body.MineType;
        if( up ) type = un.Body.UPMineType;
        if( type == EMineType.INDESTRUCTIBLE ||                                              // These cannot be mined
            type == EMineType.SPIKE_BALL ||
            type == EMineType.HOOK ||
            type == EMineType.SHACKLE ||
            type == EMineType.LADDER ||
            type == EMineType.SUSPENDED ||
            type == EMineType.BRIDGE ||
            type == EMineType.VAULT ||
            type == EMineType.TUNNEL ||
            type == EMineType.SQUARE_BASE ) return false;
        if( type == EMineType.WEDGE_LEFT ||                                         // Wedge is Exception, Costs Zero to be destroyed, but requires at least one pickaxe
            type == EMineType.WEDGE_RIGHT )
        {
            if( Item.GetNum( ItemType.Res_Mining_Points ) < 1 ) return false;
            //if( un.Body.MinePushSteps > 0 ) return false;
        }
        return true;
    }
    #endregion
    private static bool UpdateMagnetMine( Unit mine, Vector2 tg, Vector2 from )
    {
        if( mine.Mine.MagnetMine == false ) return false;
        Vector2 mtg2 = new Vector2(-1,-1);
        Unit tgm = null;
        for( int i = 1; i < Sector.TSX; i++ )
        {
            Vector2 mtg = tg + ( Manager.I.U.DirCord[ ( int ) mine.Mine.MineBonusDir ] * i );                                // Find target mine
            mtg2 = tg + ( Manager.I.U.DirCord[ ( int ) mine.Mine.MineBonusDir ] * ( i - 1 ) ); 
            if( Map.IsWall( mtg ) ) break;
            if( mtg2 == G.Hero.Pos ) break;
            tgm = Map.GFU( ETileType.MINE, mtg );
            if( mtg2 != G.Hero.Pos )
            if( tgm && tgm.CanFlyFromTo( false, tgm.Pos, mtg2 ) ) break;
        }

        if( tgm )
        {
            if( tgm.Body.MineType == EMineType.VAULT ) return true;
            tgm.CanFlyFromTo( true, tgm.Pos, mtg2 );
            Mine.UpdateChiselText = true;
            tgm.Mine.AnimateIconTimer = 2.4f;
            Controller.CreateMagicEffect( tgm.Pos );
            MasterAudio.PlaySound3DAtVector3( "Eletric Shock", tg );                                                        // Shock Sound FX
            Map.I.LineEffect( mine.Pos, tgm.Pos, 3.5f, .5f, Color.red, Color.blue, .3f );                                   // Travel Line FX 
            MasterAudio.PlaySound3DAtVector3( "Boulder Rolling", mine.Pos );                                                // sound FX
            iTween.ShakePosition( mine.Graphic.gameObject, new Vector3( .09f, .09f, 0 ), .5f );
            mine.Mine.MineBonusUses++;
            if( mine.Mine.MineBonusUses >= 3 )                                                                              // no more uses, destroy magnet
            {
                mine.Mine.MagnetMine = false;                                                                             
                MasterAudio.PlaySound3DAtVector3( "Wood Break", mine.Pos );
                Map.I.CreateExplosionFX( mine.Pos );                                                                        // Explosion FX
            }
        }
        return true;
    }
    public static bool UpdateHoleMine( ref Vector2 tg, ref Unit mine, bool sort )
    {
        if( mine.Mine.HoleMine )                                                                               // Hole mine
        {
            Vector2 holetg = tg + Manager.I.U.DirCord[ ( int ) G.Hero.Dir ];
            Unit destmine = Map.GFU( ETileType.MINE, holetg );
            if( destmine == null ) return true;
            if( CanBeMined( holetg ) == false ) return true;
            if( sort )
            {
                if( destmine.Body.MineType == EMineType.VAULT ) return true;
                DestroyMine( ref mine );
                Controller.CurentMoveTrial = new Vector2( -1, -1 );
                bool rs = destmine.CanMoveFromTo( true, destmine.Pos, tg, G.Hero );                            // moves the target mine to the hole
                destmine.Mine.HoleMine = true;
                destmine.Mine.AnimateIconTimer = 1;
                Controller.CreateMagicEffect( tg, "Mine Debris" );                                             // Mine Debris FX
                MasterAudio.PlaySound3DAtVector3( "Mining 2", tg );
                Map.I.CreateExplosionFX( mine.Pos );                                                           // Explosion FX
                Mine.UpdateChiselText = true;
                return true;
            }
            else                                                                                               // hole mine failed attept
            {
                if( destmine == null )                                                                         // return if no rope connected
                    return true;
            }
        }
        return false;
    }
    public static bool UpdateWheelMine( Vector2 from, Vector2 to )
    {
        if( Map.I.CheckArrowBlockFromTo( from, to, G.Hero ) == true ) return false;
        Unit mine = Map.GFU( ETileType.MINE, to );
        if( mine )
        if( mine.Mine.WheelMine )                                                                                // Wheel mine bump
        {
            if( mine.Mine.MineBonusUses > 0 )
            {
                return false;
            }
            Unit mm = Map.GFU( ETileType.MINE, to + Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] );                // gets target mine
            if( mm )
            if( TypeCanBeMined( mm ) )
                {
                    mine.Mine.MineBonusDir = G.Hero.Dir;                                                        // Set rope connection direction by bump
                    MasterAudio.PlaySound3DAtVector3( "Raft Merge", mm.Pos );
                    Controller.CreateMagicEffect( mm.Pos );
                    return true;
                }
            return false;
        }

        if( from == to ) return false;
        if( mine ) return false;
        for( int i = 0; i < 8; i++ )
        {
            Unit mm = Map.GFU( ETileType.MINE, to + Manager.I.U.DirCord[ i ] );
            if( mm )
            if( Util.IsNeighbor( mm.Pos, from ) )
            if( mm.Mine.WheelMine )
            if( mm.Mine.MineBonusDir != EDirection.NONE )
            {
                Vector2 desttg = mm.Pos + Manager.I.U.DirCord[ ( int ) mm.Mine.MineBonusDir ];                 // Destination mine to rotate
                Unit destmine = Map.GFU( ETileType.MINE, desttg );
                if( destmine )
                {
                    int drfr = ( int ) Util.GetRotationToTarget( from, mm.Pos );
                    int drto = ( int ) Util.GetRotationToTarget( to, mm.Pos );
                    int sig = drto - drfr;
                    Vector2 dif = to - from;                    
                    if( from.x != to.x )
                        if( from.y != to.y )
                        {
                            CheckDiag( mm, from, dif, +1, -1, 0, -1, +1, ref sig );                           // Diagonal hero movement works differently
                            CheckDiag( mm, from, dif, -1, +1, -1, 0, -1, ref sig );
                            CheckDiag( mm, from, dif, +1, -1, +1, 0, -1, ref sig );
                            CheckDiag( mm, from, dif, -1, +1, 0, +1, +1, ref sig );
                            CheckDiag( mm, from, dif, -1, -1, -1, 0, +1, ref sig );
                            CheckDiag( mm, from, dif, +1, +1, 0, +1, -1, ref sig );
                            CheckDiag( mm, from, dif, +1, +1, +1, 0, +1, ref sig );
                            CheckDiag( mm, from, dif, -1, -1, 0, -1, -1, ref sig );
                        }
                    
                    if( sig == -7 ) sig = +1;
                    if( sig == +7 ) sig = -1;

                    List<Unit> ul = new List<Unit>();
                    RotateWheel( mm, destmine, sig, ref ul );                                                  // Recursive function to rotate all links
                    MasterAudio.PlaySound3DAtVector3( "Chain Rattling", mm.Pos, .5f );                         // Sound FX
                    MasterAudio.PlaySound3DAtVector3( "Boulder Rolling", mm.Pos );                             // sound FX
                    mm.Mine.MineBonusUses++;
                    Mine.UpdateChiselText = true;
                }
            }
        }
        return false;
    }
    private static void RotateWheel( Unit mm, Unit destmine, int sig, ref List<Unit> ul )
    {
        if( ul.Contains( mm ) ) return;
        ul.Add( mm );
        int dr = ( int ) destmine.Dir;
        dr = ( int ) Util.RotateDir( ( int ) dr, sig );
        destmine.RotateTo( ( EDirection ) dr );                                                                   // Rotate mine
        destmine.Mine.AnimateIconTimer = 1f;
        destmine.Body.UPMineDir = Util.RotateDir( ( int ) destmine.Body.UPMineDir, sig );
        mm.Mine.CogRotationDir = -sig;
        mm.Mine.MineRotationSpeed = 200;
        if( destmine.Mine.WheelMine == false )
        {
            destmine.Mine.MineBonusDir = Util.RotateDir( ( int ) destmine.Mine.MineBonusDir, sig );
            return;
        }
        Vector2 desttg = destmine.Pos + Manager.I.U.DirCord[ ( int ) destmine.Mine.MineBonusDir ];                // Destination mine to rotate
        mm = destmine;
        destmine = Map.GFU( ETileType.MINE, desttg );
        if( destmine )
        {
            RotateWheel( mm, destmine, sig, ref ul );                                                             // recursive function
        }
        else return;
    }
    public static bool CheckDiag( Unit mm, Vector2 from, Vector2 dif, int difx, int dify, int mpx, int mpy, int val, ref int sig )
    {
        if( mm.Pos == from + new Vector2( mpx, mpy ) )
        if( dif == new Vector2( difx, dify ) )
        {
            sig = val;
            return true;
        }
        return false;
    }
    public static bool UpdateSwapperMine( Vector2 from, Vector2 tg, bool test = false )
    {
        Unit mine = Map.GFU( ETileType.MINE, tg );                                                             // No mine in the target
        if( mine == null ) return false;
        if( mine.Mine.SwapperMine == false ) return false;
        EDirection destdr = Util.GetTargetUnitDir( from, tg );
        Vector2 basetg = tg + Manager.I.U.DirCord[ ( int ) destdr ];                                           // find Hammer mine position
        Unit basemine = Map.GFU( ETileType.MINE, basetg );
        if( basemine == null )
        {
            Vector2 jumptg = tg + ( Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] * 2 );                            // Swapper jumps 2 tiles if no base found towards hero dir
            Unit jumpm = Map.GFU( ETileType.MINE, jumptg );
            if( jumpm == null ) return true;
            if( jumpm.Body.MineType == EMineType.VAULT ) return true;
            if( jumpm.Body.MineType == EMineType.TUNNEL ) return true;
            if( test ) return false;                                                                            // just testing to set correct sprite color
            Mine.CopyMineBonus( mine, Map.I.AuxMine );                                                          // copy mine bonus
            Mine.CopyMineBonus( jumpm, mine );
            Mine.CopyMineBonus( Map.I.AuxMine, jumpm );
            Vector3 aux = mine.Body.Sprite3.transform.position;
            mine.Body.Sprite3.transform.position = jumpm.Body.Sprite3.transform.position;                       // animate bonus spr
            jumpm.Body.Sprite3.transform.position = aux;
            mine.Mine.AnimateIconTimer = 1;
            jumpm.Mine.AnimateIconTimer = 1;
            MasterAudio.PlaySound3DAtVector3( "Mine Switch", tg );                                              // Sound FX
            Mine.UpdateChiselText = true;  
            return true; 
        }
        if( test ) return true;
        Vector2 desttg = basetg + Manager.I.U.DirCord[ ( int ) G.Hero.Dir ];
        Unit hammermine = Map.GFU( ETileType.MINE, desttg );                                                                              
        if( mine.Body.AutoRotatingMineType == EAutoRotateMineType.RANDOM_ANY )
        if( hammermine )
        {
            if( hammermine.Body.MineType == EMineType.VAULT ) return false;
            MineSwitching = true;
            mine.Control.ApplyMove( mine.Pos, desttg );
            mine.Control.OldPos = tg;
            mine.Control.AnimationOrigin = tg;
            mine.Control.SmoothMovementFactor = 0;
            hammermine.Control.ApplyMove( hammermine.Pos, tg );
            hammermine.Control.OldPos = desttg;
            hammermine.Control.AnimationOrigin = desttg;
            hammermine.Control.SmoothMovementFactor = 0;
            mine.Mine.AnimateIconTimer = 1;
            hammermine.Mine.AnimateIconTimer = 1;
            MasterAudio.PlaySound3DAtVector3( "Mine Switch", tg );
            MineSwitching = false;
            mine.Mine.SwapperMine = false;
            MasterAudio.PlaySound3DAtVector3( "Boulder Rolling", tg );                                           // sound FX
            //hammermine.RotateTo( ( EDirection ) Util.GetRandomDir( hammermine.Dir ) );                         // rotate target mine        
            //MasterAudio.PlaySound3DAtVector3( "Save Game 2", mine.Pos );                                       // Sound FX
            //Controller.CreateMagicEffect( hammermine.Pos );                                                    // Magic FX
            Mine.UpdateChiselText = true;
            return true;
        }
        return true;
    }

    public static float GetHammerMinePrice( float price, Vector2 tg, Unit mine, ref bool up, Vector2 from )
    {
        if( mine.Mine.HammerMine )
        {
            EDirection destdr = Util.GetTargetUnitDir( from, tg );
            if( destdr == EDirection.NONE ) return -1;
            Vector2 desttg = tg + Manager.I.U.DirCord[ ( int ) destdr ];                                    // Hammer mine destroy mine behind
            Unit destmine = Map.GFU( ETileType.MINE, desttg +
            Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] );

            if( destmine && destmine.Body.MineLeverDir != EDirection.NONE )
            {
                mine = destmine;
                int hamprice = GetMiningPrice( tg, mine, ref up, desttg );
                //Debug.Log( "pr " + price + " hm " + hamprice +"  " + desttg );
                if( hamprice < price ) return hamprice;
            }
        }
        return price;
    }
    public static int GetNeighborChisels( Vector2 tg )
    {
        int chisel = 0;
        for( int i = 0; i < 8; i++ )
        {
            Unit cm = Map.GFU( ETileType.MINE, tg + Manager.I.U.DirCord[ i ] );
            if( cm && cm.Mine.ChiselMine ) chisel++;
        }
        return chisel;
    }
    public static bool UpdateCannonMine( Unit mine, Vector2 tg, Vector2 from, bool test = false )
    {
        if( mine.Mine.CannonMine == false ) return false;
        EDirection mov = Util.GetTargetUnitDir( from, tg );
        if( mov == Util.GetInvDir( G.Hero.Dir ) ) return false;                                                // no back casting
        Unit tgm = null;
        Unit lastm = null;
        Unit tgm2 = null;
        int jumpcount = 0;
        bool border = false;
        Vector2 stop = new Vector2( -1, -1 );

        for( int phase = 1; phase <= 2; phase++ )
        {
            int ax1 = ( int ) mov;
            int ax2 = ( int ) G.Hero.Dir;
            EDirection dr = Util.GetRotationToTarget( G.Hero.Pos, tg );
            mine.Mine.MineBonusDir = dr;
            if( phase == 2 )
            {
                ax1 = ( int ) G.Hero.Dir;
                ax2 = ( int ) mov;
                if( tgm ) break;
            }
            for( int i = 1; i < Sector.TSX; i++ )
            {
                Vector2 mtg = tg + ( Manager.I.U.DirCord[ ax1 ] * i );                                // Find target bonus mine
                if( Map.IsWall( mtg ) ) { tgm = null; break; }
                tgm = Map.GFU( ETileType.MINE, mtg );
                Vector2 lasttg = tg + ( Manager.I.U.DirCord[ ax1 ] * ( i - 1 ) );                     // Find target bonus mine
                lastm = Map.GFU( ETileType.MINE, lasttg );
                if( tgm && Mine.GetMineBonusCount( mtg ) > 0 ) break;
                if( tgm == null && lastm != null )
                    jumpcount++;
                if( jumpcount > 1 ) break;
            }

            if( tgm ) stop = tgm.Pos;
            border = false;
            jumpcount = 0;
            if( tgm )
            for( int i = 1; i < Sector.TSX; i++ )
            {
                Vector2 mtg = tgm.Pos + ( Manager.I.U.DirCord[ ax2 ] * i );                           // Find sliding destination
                if( Map.IsWall( mtg ) ) break;
                tgm2 = Map.GFU( ETileType.MINE, mtg );
                if( tgm2 )
                {
                    if( Mine.GetMineBonusCount( mtg ) > 0 )
                    {
                        stop = tgm2.Pos;
                        mine.Mine.MineBonusDir = G.Hero.Dir;
                        break;
                    }
                    if( border ) { mine.Mine.MineBonusDir = G.Hero.Dir; stop = tgm2.Pos; break; }
                }
                else
                {
                    if( border == false )
                    if( ++jumpcount == 2 ) break;
                    if( i == 1 ) border = true;
                    if( border == false )
                    if( i > 1 && tgm2 == null ) break;
                }
                if( tgm2 )
                {
                    stop = tgm2.Pos;
                    mine.Mine.MineBonusDir = G.Hero.Dir;
                }
            }
        }

        if( test ) return false;
        if( tgm == null ) return false;
        if( stop != null && stop != tgm.Pos )
        {
            Unit dest = Map.GFU( ETileType.MINE, stop );
            if( dest.Body.MineType == EMineType.VAULT ) return false;
            if( dest.Body.MineType == EMineType.TUNNEL ) return false;            
            Vector3 aux = tgm.Body.Sprite3.transform.position;
            Mine.CopyMineBonus( dest, Map.I.AuxMine );                                                          // copy mine bonus
            Mine.CopyMineBonus( tgm, dest );        
            Mine.CopyMineBonus( Map.I.AuxMine, tgm );  
            tgm.Body.Sprite3.transform.position = dest.Body.Sprite3.transform.position;
            tgm.Mine.AnimateIconTimer = 2.4f;  
            dest.Body.Sprite3.transform.position = aux;
            dest.Mine.AnimateIconTimer = 2.4f;                                                                                      // animation
            Map.I.LineEffect( mine.Pos, tgm.Pos, 3.5f, .5f, Color.red, Color.blue, .3f );                                           // Travel Line FX 1
            Map.I.LineEffect( tgm.Pos, dest.Pos, 3.5f, .5f, Color.red, Color.blue, .3f );                                           // Travel Line FX 2
            MasterAudio.PlaySound3DAtVector3( "Eletric Shock", tg );                                                                // Shock Sound FX
            iTween.ShakePosition( mine.Graphic.gameObject, new Vector3( .09f, .09f, 0 ), .5f );
            Mine.UpdateChiselText = true;
        }
        return true;
    }
    public static bool UpdateChiselMine( Unit mine, Vector2 tg, Vector2 from )
    {
        if( mine.Mine.ChiselMine == false ) return false;
        EDirection destdr = Util.GetTargetUnitDir( from, tg );
        if( destdr == EDirection.NONE ) return false;
        Vector2 desttg = tg + Manager.I.U.DirCord[ ( int ) destdr ];                                              // find Hammer mine position
        Unit hammermine = Map.GFU( ETileType.MINE, desttg +
        Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] );
        if( hammermine == null ) return false;
        Unit destmine = Map.GFU( ETileType.MINE, desttg );
        Unit srcmine = Map.GFU( ETileType.MINE, desttg );
        if( srcmine == null ) return false;
        Unit tgm = null;
        Unit exch = null;
        EDirection mov = Util.GetTargetUnitDir( from, tg );
        if( mov == Util.GetInvDir( G.Hero.Dir ) ) return false;                                                   // no back casting

        Unit lastmn = null;
        for( int i = 0; i < Sector.TSX; i++ )
        {
            Vector2 mtg = hammermine.Pos + ( Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] * i );                     // Find sliding destination
            tgm = Map.GTM( mtg );
            if( i < 1 && tgm == null ) return false;
            if( i >= 1 && tgm == null )
                break;

            if( Mine.GetMineBonusCount( mtg ) > 0 )
            {
                if( Mine.GetMineBonusCount( desttg ) == 0 )                                                       // is source mine bonus free?
                    exch = tgm;                                                                                   // mining containing bonus for exchange found
                break;
            }
            lastmn = tgm;
        }
        if( exch )
        {
            Mine.CopyMineBonus( exch, srcmine );                                                                                        // copy mine bonus
            exch.Body.MiningPrize = ItemType.NONE;
            exch.Body.MiningBonusAmount = 0;

            for( int i = 0; i <= Mine.TotBonus; i++ )
                Mine.SetMineBonus( ref exch, i, false );                                                                                // clears source mine bonuses

            Vector3 aux = exch.Body.Sprite3.transform.position;
            exch.Body.Sprite3.transform.position = destmine.Body.Sprite3.transform.position;
            exch.Mine.ChiselMine = true;
            srcmine.Body.ChiselSlideCount = 0;
            MasterAudio.PlaySound3DAtVector3( "Mechanical Sound", tg );
            Controller.CreateMagicEffect( srcmine.Pos );
            srcmine.Body.Sprite3.transform.position = aux;                                                                               // Slide animation FX
            srcmine.Body.Sprite4.transform.position = aux;                                                                               // Slide animation FX
            srcmine.Body.EffectList[ 3 ].transform.position = aux;
            UpdateNeighborMines( srcmine.Pos );
            UpdateNeighborMines( exch.Pos );
            srcmine.Mine.AnimateIconTimer = 2.4f;
            exch.Mine.AnimateIconTimer = 2.4f;
            Map.I.LineEffect( srcmine.Pos, tgm.Pos, 3.5f, .5f, Color.red, Color.blue, .3f );                                             // Travel Line FX
        }
        else
        {
            if( lastmn )
            {
                Map.I.LineEffect( lastmn.Pos, desttg, 3.5f, .5f, Color.red, Color.blue, .3f );                                           // Travel Line FX
                lastmn.Body.Sprite3.transform.position = destmine.Body.Sprite3.transform.position;
                UpdateNeighborMines( lastmn.Pos );
                lastmn.Mine.AnimateIconTimer = 2.4f;
                lastmn.Mine.ChiselMine = true;
            }
        }
        mine.Mine.ChiselMine = false;
        MasterAudio.PlaySound3DAtVector3( "Mine Switch", tg );                                                                           // Sound FX
        return true;
    }
    private static void UpdateNeighborMines( Vector2 pos )
    {
        Unit cm = Map.GFU( ETileType.MINE, pos );
        if( cm ) cm.Mine.AnimateIconTimer = .00001f;
        for( int i = 0; i < 8; i++ )
        {
            cm = Map.GFU( ETileType.MINE, pos + Manager.I.U.DirCord[ i ] );
            if( cm ) cm.Mine.AnimateIconTimer = .00001f;
        }
    }
    public static bool UpdateArrowMine( Unit mine, Vector2 tg, Vector2 from )
    {
        if( mine.Mine.ArrowMine == false ) return false;
        AttemptMining = false;                        
        Vector2 desttg = tg + Manager.I.U.DirCord[ ( int ) G.Hero.Dir ];
        Unit destmine = Map.GFU( ETileType.MINE, desttg );
        if( destmine != null )                                                                                      // mine bonus switch successful
        {
            if( Mine.GetMineBonusCount( desttg ) == 0 )                                                             // no target for switching
            {
                desttg = tg + Manager.I.U.DirCord[ ( int ) G.Hero.InvDr() ];
                Vector2 desttg2 = G.Hero.Pos + Manager.I.U.DirCord[ ( int ) G.Hero.InvDr() ];
                Vector2 destbl = desttg + Manager.I.U.DirCord[ ( int ) G.Hero.InvDr() ];
                Unit blockmine = Map.GFU( ETileType.MINE, desttg );
                if( blockmine )
                {
                    if( blockmine.Body.MineType == EMineType.VAULT ) return false;
                    if( blockmine.Body.MineType == EMineType.TUNNEL ) return false;
                    if( blockmine.CanMoveFromTo( true, desttg, destbl, G.Hero ) == false ) return true;            // move mine behind
                    blockmine.Mine.AnimateIconTimer = 1;
                }
                if( G.Hero.Pos == desttg )
                if( G.Hero.CanMoveFromTo( true, G.Hero.Pos, desttg2, G.Hero ) == false ) return true;              // move hero
                if( mine.CanMoveFromTo( false, mine.Pos, desttg, G.Hero ) == false ) return true;
                Controller.MoveFlyingUnitTo( ref mine, mine.Pos, desttg );
                mine.Mine.AnimateIconTimer = 1;
                Controller.CreateMagicEffect( tg );
                MasterAudio.PlaySound3DAtVector3( "Boulder Rolling", tg );                                          // sound FX
                Mine.UpdateChiselText = true;
                return true;
            }
            if( destmine.Body.MineType == EMineType.VAULT ) return true;
            if( destmine.Body.MineType == EMineType.TUNNEL ) return true;
            Mine.CopyMineBonus( mine, Map.I.AuxMine );                                                              // copy mine bonus
            Mine.CopyMineBonus( destmine, mine );
            Mine.CopyMineBonus( Map.I.AuxMine, destmine );
            Vector3 aux = mine.Body.Sprite3.transform.position;
            mine.Body.Sprite3.transform.position = destmine.Body.Sprite3.transform.position;                       // animate bonus spr
            destmine.Body.Sprite3.transform.position = aux;
            mine.Mine.AnimateIconTimer = 1;
            destmine.Mine.AnimateIconTimer = 1;
            MasterAudio.PlaySound3DAtVector3( "Mine Switch", tg );                                                 // Sound FX
            Mine.UpdateChiselText = true;
            return true;
        }
        MasterAudio.PlaySound3DAtVector3( "Error 2", tg );                                                         // Error Sound FX
        Controller.CreateMagicEffect( tg );
        return true;
    }
    public static bool UpdateGloveMine( Unit mine, Vector2 from, Vector2 tg )
    {
        if( G.HS.GloveTarget.x != -1 )
        if( tg != G.HS.GloveTarget )
        {
            Unit destmine = Map.GFU( ETileType.MINE, G.HS.GloveTarget );    
            Mine.CopyMineBonus( mine, Map.I.AuxMine );                                                              // copy mine bonus
            Mine.CopyMineBonus( destmine, mine );
            Mine.CopyMineBonus( Map.I.AuxMine, destmine );
            Vector3 aux = mine.Body.Sprite3.transform.position;
            mine.Body.Sprite3.transform.position = destmine.Body.Sprite3.transform.position;                       // animate bonus spr
            destmine.Body.Sprite3.transform.position = aux;
            mine.Mine.AnimateIconTimer = 1;
            destmine.Mine.AnimateIconTimer = 1;
            MasterAudio.PlaySound3DAtVector3( "Mine Switch", tg );                                                 // Sound FX
            MasterAudio.PlaySound3DAtVector3( "Save Game 2", tg );                                                 // Sound FX  
            Mine.UpdateChiselText = true;
            G.HS.GloveTarget = new Vector2( -1, -1 );
            Map.I.LineEffect( mine.Pos, destmine.Pos, 3.5f, .5f, Color.red, Color.blue, .3f );                     // Travel Line FX 
            return true;
        }

        if( mine.Mine.GloveMine == false ) return false; 
        Unit tgm = null;
        Unit lastm = null;
        int jumpcount = 0;
        for( int i = 1; i < Sector.TSX; i++ )
        {
            Vector2 mtg = tg + ( Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] * i );                                // Find target bonus mine
            tgm = Map.GFU( ETileType.MINE, mtg );
            Vector2 lasttg = tg + ( Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] * ( i - 1 ) );                     // Find target bonus mine
            lastm = Map.GFU( ETileType.MINE, lasttg );
            if( Map.I.CheckArrowBlockFromTo( lasttg, mtg, G.Hero ) == true ) return true;
            if( G.Hero.Pos == mtg ) return true;
            if( tgm && Mine.GetMineBonusCount( mtg ) > 0 ) break;
            if( i > 1 && tgm == null && lastm != null ) break;
            if( jumpcount > 0 && tgm ) { lastm = tgm; break; }
            if( tgm == null && lastm != null )
                jumpcount++;
            if( jumpcount > 1 ) return false;
            if( Map.IsWall( mtg ) ) return true;
        }

        if( tgm ) 
        if( Mine.GetMineBonusCount( tgm.Pos ) > 0 )
        {
            if( tgm.Body.MineType == EMineType.VAULT ) return false;
            if( tgm.Body.MineType == EMineType.TUNNEL ) return false; 
            G.HS.GloveTarget = tgm.Pos;
            mine.Mine.GloveMine = false;
            Transform tr = PoolManager.Pools[ "Pool" ].Spawn( "Archer Arrow" );                         
            ArcherArrowAnimation ar = tr.gameObject.GetComponent<ArcherArrowAnimation>();
            ArcherArrowAnimation.FixedRotation = mine.Body.Sprite3.transform.eulerAngles.z;
            ar.AuxUnit = null;         
            ar.Create( mine.Pos, null, tr, tgm.Pos, EBoltType.Glove );
            MasterAudio.PlaySound3DAtVector3( "Mine Switch", tg );                                                 // Sound FX
            MasterAudio.PlaySound3DAtVector3( "Raft Merge", tg );                                                 // Sound FX  
            Map.I.LineEffect( mine.Pos, tgm.Pos, 3.5f, .5f, Color.red, Color.blue, .3f );                          // Travel Line FX 
            return true; 
        }

        if( lastm )
        {
            mine.Mine.GloveMine = false;
            lastm.Mine.GloveMine = true;
            lastm.Mine.MineBonusDir = mine.Mine.MineBonusDir;
            lastm.Body.Sprite3.transform.position = mine.Body.Sprite3.transform.position;
            lastm.Mine.AnimateIconTimer = 2.4f;
            MasterAudio.PlaySound3DAtVector3( "Mine Switch", tg );                                                 // Sound FX
            Map.I.LineEffect( mine.Pos, lastm.Pos, 3.5f, .5f, Color.red, Color.blue, .3f );                        // Travel Line FX 
        }
        return true;
    }
    public static bool UpdateDynamiteMine( Unit mine, Vector2 tg, Vector2 from )
    {
        if( mine.Mine.DynamiteMine == false ) return false;
        EDirection mov = Util.GetTargetUnitDir( from, tg );
        Vector2 desttg = tg + Manager.I.U.DirCord[ ( int ) mov ];
        Unit destmine = Map.GFU( ETileType.MINE, desttg );
        bool explode = false;
        bool crack = false;
        if( destmine != null )                                                                                 // Theres a mine behind. slide it                                                  
        {
            for( int i = 1; i < Sector.TSX; i++ )
            {
                Vector2 mtg = tg + ( Manager.I.U.DirCord[ ( int ) mov ] * i );                                 // Find sliding destination
                Unit tgm = Map.GFU( ETileType.MINE, mtg );
                if( tgm )
                if( Mine.GetMineBonusCount( mtg ) == 0 )                                                       // is source mine bonus free?
                if( tgm.Body.MineType != EMineType.VAULT )
                    {
                        Mine.SetMineBonus( ref tgm, 8, true );
                        Mine.SetMineBonus( ref mine, 8, false );
                        tgm.Body.Sprite3.transform.position = mine.Body.Sprite3.transform.position;            // Slide animation FX
                        tgm.Mine.AnimateIconTimer = 2.4f;
                        MasterAudio.PlaySound3DAtVector3( "Mine Switch", tg );                                 // Sound FX
                        break;
                    }
                if( tgm == null )
                {
                    explode = true;                                                                            // no mine in the end of the row
                    break;
                }
            }
        }
        else                                                                                                   // no mine behind. Explode it!      
        {
            explode = true;
        }

        if( explode )
        {
            List<Unit> Bombl = new List<Unit>();
            List<Vector2> sidel = new List<Vector2>();
            Map.I.ObjectID = new int[ Map.I.Tilemap.width, Map.I.Tilemap.height ];
            CalculateDynamiteExplosion( from, tg, ref Bombl, ref sidel );                                        // Recursive calculation of Dynamite explosion
            Map.I.ObjectID = new int[ Map.I.Tilemap.width, Map.I.Tilemap.height ];
            for( int i = Bombl.Count - 1; i >= 0; i-- )
            {
                if( Bombl[ i ] .Mine.DynamiteMine == false )                                                    // decrement vault power
                if( Bombl[ i ].Mine.HitCount > 0 )
                    crack = true;
                Map.I.CreateExplosionFX( Bombl[ i ].Pos );                                                      // Main Dynamite Explosions
                Unit mn = Bombl[ i ];
                DestroyMine( ref mn, true );
            }

            for( int i = sidel.Count - 1; i >= 0; i-- )
            {
                Map.I.CreateExplosionFX( sidel[ i ] );                                                          // Side Explosions
                Unit tgm = Map.GFU( ETileType.MINE, sidel[ i ] );
                if( tgm ) DestroyMine( ref tgm, true );
            }
        }
        Mine.UpdateChiselText = true;
        if( crack ) G.HS.DynamiteExplodeCracked--;                                                              // decrement only once
        return true;
    }
    public static bool CalculateDynamiteExplosion( Vector2 from, Vector2 to, ref List<Unit> Bombl, ref List<Vector2> sidel )
    {
        if( Map.PtOnMap( Map.I.Tilemap, to ) == false ) return false;
        if( Map.I.ObjectID[ ( int ) to.x, ( int ) to.y ] > 0 )
            return false;
        Unit bomb = Map.GFU( ETileType.MINE, to );
        if( Bombl.Contains( bomb ) ) return false;
        if( bomb && bomb.Mine.DynamiteMine )
            Bombl.Add( bomb );                                                            // add bomb to list
        for( int i = 0; i < 8; i++ )
        {
            Vector2 tg = to + Manager.I.U.DirCord[ i ];
            Unit tgm = Map.GFU( ETileType.MINE, tg );
            Unit crack = null;
            if( G.HS.DynamiteExplodeCracked >= 1 )
            if( tgm && tgm.Mine.HitCount > 0 )                                            // Cracked mine vault power
            {
                if( Bombl.Contains( tgm ) == false ) 
                    Bombl.Add( tgm ); 
                crack = tgm;
            }

            if( tgm ) 
            if( tgm.Mine.DynamiteMine || crack )
            {
                Map.I.ObjectID[ ( int ) to.x, ( int ) to.y ] += 1;                        // fill map obj id matrix
                Vector2 sidetg = tg + Manager.I.U.DirCord[ i ];
                Unit sidemine = Map.GFU( ETileType.MINE, sidetg );

                if( sidemine == null || 
                  ( sidemine.Mine.DynamiteMine == false  &&
                    TypeCanBeMined( sidemine ) ) )
                if( sidel.Contains( sidetg ) == false )
                    sidel.Add( sidetg );                                                  // Add side target

                CalculateDynamiteExplosion( to, tg, ref Bombl, ref sidel );               // Recursive calculation
            }
        }
        return false;
    }
    public static bool UpdateCogMine( Unit mine, Vector2 tg, Vector2 from )
    {
        if( mine.Mine.CogMine == false ) return false;
        if( Util.IsDiagonal( G.Hero.Dir ) )
        {
            mine.Mine.CogRotationDir *= -1;                                                // Inverts cog rotation direction
            Controller.CreateMagicEffect( tg );                                            // Magic FX
            MasterAudio.PlaySound3DAtVector3( "Chain Rattling", tg );                      // Sound FX
            return true;
        }
        Vector2 mid = Vector2.zero;
        List<Vector2> pl = mine.Control.GetCogMineTgList( tg, ref mid );
        List<Unit> movl = new List<Unit>();
        bool block = false;
        bool ok = false;
        for( int phase = 1; phase <= 2; phase++ )
        for( int i = 0; i < 4; i++ )
        {
            Vector2 frtg = pl[ i ];
            Unit mn = Map.GFU( ETileType.MINE, frtg );
            if( movl.Contains( mn ) ) mn = null;
            if( mn == null && G.Hero.Pos == frtg ) 
                mn = G.Hero;

            if( mn && movl.Contains( mn ) == false )                                         // check this to avoid multiple mine move
            {
                if( mn.Body.MineType == EMineType.VAULT ) return true;
                Vector2 tgg = pl[ 0 ];                                                       // calculates target
                if( i + 1 < 4 ) tgg = pl[ i + 1 ];
                if( phase == 1 )                                                             // phase 1 test for lever block
                if( mn.CheckLeverBlock( false, mn.Pos, tgg, false ) )
                    block = true;
                List<ETileType> ex = new List<ETileType>();
                ex.Add( ETileType.ARROW );
                if( Util.CheckBlock( mn.Pos, tgg, mn, true, true, 
                false, false, false, false, ex ) ) return true;
                if( phase == 2 )                                                             // phase 2: do the move
                {
                    if( block ) return true;                                                 // Lever block
                    ok = true;
                    int floor = mn.Control.Floor;
                    mn.Control.ApplyMove( mn.Pos, tgg );
                    mn.Control.Floor = floor;
                    MasterAudio.PlaySound3DAtVector3( "Chain Rattling", tg );                // Sound FX
                    MasterAudio.PlaySound3DAtVector3( "Boulder Rolling", tg );               // sound FX
                    if( mn.Mine ) mn.Mine.AnimateIconTimer = 1;
                    movl.Add( mn );
                }
            }
        }
        if( ok )
        {
            mine.Mine.MineBonusUses++;                                                      // uses increment
            if( mine.Mine.MineBonusUses >= 3 )
                mine.Mine.CogMine = false;                                                  // max uses reached, destroy cog
        }
        Mine.UpdateChiselText = true;
        return true;
    }
    public List<Vector2> GetCogMineTgList( Vector2 tg, ref Vector2 mid )
    {
        if( Util.IsDiagonal( G.Hero.Dir ) ) return null;
        List<Vector2> pl = new List<Vector2>();
        pl.Add( tg );
        pl.Add( tg + Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] );
        int sig = -2;
        if( Unit.Mine.CogRotationDir == -1 ) sig = 2;
        Vector2 aux = pl[ 1 ] + Manager.I.U.DirCord[ ( int ) Util.GetRelativeDirection( sig, G.Hero.Dir ) ];
        pl.Add( aux );
        aux = pl[ 2 ] + Manager.I.U.DirCord[ ( int ) Util.GetInvDir( G.Hero.Dir ) ];
        pl.Add( aux );
        mid = ( pl[ 0 ] + pl[ 1 ] + pl[ 2 ] + pl[ 3 ] ) / 4;
        return pl;
    }
    public static bool UpdateRopeMine( Unit mine, Vector2 tg, Vector2 from )
    {
        EDirection destdr = Util.GetTargetUnitDir( from, tg );
        if( destdr == EDirection.NONE ) return false;
        Vector2 desttg = tg + Manager.I.U.DirCord[ ( int ) destdr ];                           // Rope Connected Mine bump
        Unit destmine = Map.GFU( ETileType.MINE, desttg );
        Vector2 levpos = new Vector2( -1, -1 );
        if( destmine && destmine.Body.MineHasLever() )
        if( destmine.Body.MineLeverDir != EDirection.NONE )
            levpos = mine.Pos + Manager.I.U.DirCord[ ( int ) destmine.Body.MineLeverDir ];

        if( mine.Mine.RopeMine )
        if( destmine != null )
        if( !destmine.CheckLeverBlock( false, destmine.Pos, tg, false ) )
        if( levpos.x == -1 || levpos != desttg )
        {
            if( destmine.Body.MineType == EMineType.VAULT ) return false;
            MineSwitching = true;
            mine.Control.ApplyMove( mine.Pos, desttg );
            mine.Control.OldPos = tg;
            mine.Control.AnimationOrigin = tg;
            mine.Control.SmoothMovementFactor = 0;
            destmine.Control.ApplyMove( destmine.Pos, tg );
            destmine.Control.OldPos = desttg;
            destmine.Control.AnimationOrigin = desttg;
            destmine.Control.SmoothMovementFactor = 0;
            mine.Mine.AnimateIconTimer = 1;
            destmine.Mine.AnimateIconTimer = 1;
            //MasterAudio.PlaySound3DAtVector3( "Mine Switch", tg );
            MasterAudio.PlaySound3DAtVector3( "Boulder Rolling", tg );                                 // sound FX
            MineSwitching = false;
            if( mine.Mine.StickyMine )
                mine.Mine.RopeMine = false;
            Mine.UpdateChiselText = true;
            return true;
        }
        return false;
    }
    public static bool UpdateWedgeMine( Vector2 tg, Vector2 from )
    {
        Unit wl = Map.GMine( EMineType.WEDGE_LEFT, tg );
        Unit wr = Map.GMine( EMineType.WEDGE_RIGHT, tg );
        if( Map.IsWall( tg ) )
        {
            for( int i = 0; i < 8; i++ )
            {
                Vector2 ntg = from + Manager.I.U.DirCord[ i ];
                Unit nn = Map.GFU( ETileType.MINE, ntg );
                if( nn ) InvertWedge( nn );                                              // Invert all wedges by direct wall bump
            }
            return false;
        }

        if( wl == null )
        if( wr == null ) return false;                                                   // no wedge as target
        int ori = -2;
        if( wr ) ori = +2;
        Unit wd = Map.GFU( ETileType.MINE, tg );
        bool charge = true;
        Unit mud = Map.I.GetMud( tg );
        if( mud ) charge = false;
        EDirection mov = Util.GetTargetUnitDir( from, tg );                              // initialize method vars
        if( Util.IsDiagonal( mov ) ) ori /= 2;                                           // diagonal specific target

        EDirection dir = Util.RotateDir( ( int ) mov, ori );
        Vector2 fr = tg + Manager.I.U.DirCord[ ( int ) mov ];
        List<Unit> ul = new List<Unit>();

        if( Map.IsWall( fr ) )                                                          // invert wedge type by bumping against wall
        {
            InvertWedge( wd );
            return true;
        }

        for( int i = 0; i < Sector.TSX; i++ )                                           // make a list of all aligned mines to be pushed
        {
            Vector2 pt = fr + ( Manager.I.U.DirCord[ ( int ) dir ] * i );
            Unit mine = Map.GFU( ETileType.MINE, pt );
            if( mine && mine.Body.MineType != EMineType.VAULT )
            {
                ul.Add( mine );                                                         // add mine to list
                if( mine.Body.MineType == EMineType.WEDGE_LEFT ||
                    mine.Body.MineType == EMineType.WEDGE_RIGHT )
                    charge = false;
            }
            else
            {
                if( i == 0 )
                {
                    List<Unit> neigh = GetNeighborSimilarMine( tg, wd );
                    List<Unit> neightg = GetNeighborSimilarMine( fr, wd );              // Gives the free push bonus for 2 different wedges
                    int same = 0;
                    for( int j = 0; j < neigh.Count; j++ )
                        if( neightg.Contains( neigh[ j ] ) )
                            same++;

                    if( neigh.Count >= 1 )
                    if( wd.Body.MinePushSteps >= 1 || neightg.Count == 0 )
                        {
                            wd.CanMoveFromTo( true, wd.Pos, fr, G.Hero );               // moves the wedge to the push target
                            Mine.UpdateChiselText = true;
                            wd.Mine.AnimateIconTimer = 1;
                            MasterAudio.PlaySound3DAtVector3( "Mud Push", tg );         // Push sound FX
                            if( neightg.Count == 0 || same < 1 )
                                charge = false;
                            if( charge ) wd.Body.MinePushSteps--;                       // Steps decrement
                        }
                    return false;                                                       // no mine in front of the wedge: return
                }
                break;                                                                  // no more mines aligned
            }
        }

        if( wd.Body.MinePushSteps < 1 ) return false;                                   // no push steps, so quit

        for( int i = ul.Count - 1; i >= 0; i-- )
        {
            Vector2 to2 = ul[ i ].Pos + Manager.I.U.DirCord[ ( int ) dir ];
            bool res = ul[ i ].CanMoveFromTo( true, ul[ i ].Pos, to2, G.Hero );        // move mine to the side
            if( res == false )
            if( i == ul.Count - 1 )
                {
                    Map.I.CreateExplosionFX( to2 );                                    // Explosion FX
                    Unit mn = ul[ i ];
                    DestroyMine( ref mn );                                             // kill crushed mine
                }
            ul[ i ].Mine.AnimateIconTimer = 1;
            Mine.UpdateChiselText = true;
        }

        wd.CanMoveFromTo( true, wd.Pos, fr, G.Hero );                                  // moves the wedge to the push target
        wd.Mine.AnimateIconTimer = 1;
        MasterAudio.PlaySound3DAtVector3( "Mud Push", tg );                            // Push sound FX  
        if( charge ) wd.Body.MinePushSteps--;                                          // Steps decrement
        return true;
    }
    private static void InvertWedge( Unit un )
    {
        bool ok = false;
        if( un.Body.MineType == EMineType.WEDGE_RIGHT )
        { un.Body.MineType = EMineType.WEDGE_LEFT; ok = true; }
        else
            if( un.Body.MineType == EMineType.WEDGE_LEFT )
            { un.Body.MineType = EMineType.WEDGE_RIGHT; ok = true; }
        if( ok )
        {
            MasterAudio.PlaySound3DAtVector3( "Bump", un.Pos );                         // Sound FX     
            Controller.CreateMagicEffect( un.Pos );                                     // Magic FX
        }
    }
    public static List<Unit> GetNeighborSimilarMine( Vector2 tg, Unit un )
    {
        List<Unit> neigh = new List<Unit>();
        for( int i = 0; i < 8; i++ )
        {
            Vector2 ntg = tg + Manager.I.U.DirCord[ i ];
            Unit nn = Map.GFU( ETileType.MINE, ntg );
            if( nn )
                if( ( un.Body.MineType == EMineType.WEDGE_RIGHT &&
                      nn.Body.MineType == EMineType.WEDGE_LEFT ) ||
                    ( un.Body.MineType == EMineType.WEDGE_LEFT &&
                      nn.Body.MineType == EMineType.WEDGE_RIGHT ) )
                    neigh.Add( nn );
        }
        return neigh;
    }
    #region Bridge
    public bool UpdateSuspendedBridgeConstruction( Vector2 from, Vector2 to )
    {
        if( from == to ) return false;
        if( MudPushing ) return false;
        if( Map.I.CheckArrowBlockFromTo( from, to, G.Hero ) == true ) return false;
        Unit tob = Map.GMine( EMineType.BRIDGE, to );
        Unit frb = Map.GMine( EMineType.BRIDGE, from );
        Unit frm = Map.GFU( ETileType.MINE, from );
        Unit tom = Map.GFU( ETileType.MINE, to );
        EDirection mov = Util.GetTargetUnitDir( from, to );
        if( Item.GetNum( ItemType.Res_Bridge_Wood ) < 1 ) return false;                                           // out of wood, quit
        if( Floor < 2 ) return false;
        if( Floor == 3 ) return false;
        if( frm == null )
            if( Map.I.GetUnit( ETileType.CLOSEDDOOR, from ) == null ) return false;
        if( Map.I.GetUnit( ETileType.FOREST, to ) != null ) return false;
        if( Map.I.GetUnit( ETileType.CLOSEDDOOR, to ) != null ) return false;
        if( tom )
        {
            if( tom.Body.UPMineType != EMineType.NONE ) return false;
            if( tom.Body.MineType == EMineType.BRIDGE ) return false;
            if( tom.Body.MineType == EMineType.LADDER ) return false;
        }
        if( frb )                                                                                                 // this is to avoid the bridge to support overlap problem
            if( tom ) return false;

        Vector2 tg = to + Manager.I.U.DirCord[ ( int ) mov ];
        Unit tgm = Map.GFU( ETileType.MINE, tg );
        if( Floor == 4 )
        if( tgm && tgm.Body.MineType == EMineType.LADDER )                                                       // dont block ladder
        if( tgm.Dir == Util.GetInvDir( mov ) )
        {
            UI.I.SetTurnInfoText( "Cannot Block Ladder!", 3, Color.red );
            MasterAudio.PlaySound3DAtVector3( "Error 2", G.Hero.Pos );
            Controller.CreateMagicEffect( tg );
            return false;
        }

        if( Floor == 4 )
        if( tom != null )
        if( tgm && tgm.Body.MineType == EMineType.BRIDGE )                                                       // dont block bridge support
        if( tgm.Dir == Util.GetInvDir( mov ) || tgm.Dir == mov )
        {
            UI.I.SetTurnInfoText( "Cannot Block Support!", 3, Color.red );
            MasterAudio.PlaySound3DAtVector3( "Error 2", G.Hero.Pos );
            Controller.CreateMagicEffect( tg );
            return false;
        }

        bool up = false;
        Unit b = null;
        string aux = "Bridge Built!";
        Color col = Color.green;
        if( tom == null )                                                                                        // creates a new bridge mine object
        {
            b = Map.I.SpawnFlyingUnit( to, ELayerType.MONSTER, ETileType.MINE, null );
            b.transform.position = new Vector3( b.transform.position.x, b.transform.position.y, 0 );
            b.RotateTo( mov );
            b.Body.MineType = EMineType.BRIDGE;
        }
        else                                                                                                    // creates an upmine bridge
        {
            if( frb == null )
                if( Floor != 4 ) return false;
            tom.Body.UPMineType = EMineType.BRIDGE;
            tom.Body.UPMineDir = mov;
            up = true;
            b = tom;
        }

        AuxBridgePos = to;
        AuxBridgeDir = Util.GetInvDir( mov );
        List<int> pointsID = new List<int>();
        Map.I.ObjectID = new int[ Map.I.Tilemap.width, Map.I.Tilemap.height ];
        int search = 1;
        BridgeSupportPoints = new List<Vector2>();
        Vector2 closest = new Vector2( -1, -1 );

        CalculateBridgeSupportPoints( from, to, ref BridgeSupportPoints, ref search, ref pointsID, 0, up );     // Perform recursive calculation of support points

        int bestdist = GetBestBridgeRoute( b, ref closest );                                                    // Gets the closest route to the next support

        if( bestdist > BridgeSupportPoints.Count )
        {
            int need = bestdist - BridgeSupportPoints.Count;
            Message.RedMessage( "Bridge Too Long!\n Support Points: " + BridgeSupportPoints.Count +             // error message
            "\nBridge Length: " + bestdist + "\nNeed " + need + " more Support Points." );
            if( up )                                                                                            // destroy created bridge or upbridge
                b.Body.UPMineType = EMineType.NONE;
            else b.Kill();
            aux = "Building Failed!";
            col = Color.red;
            MasterAudio.PlaySound3DAtVector3( "Error 2", G.Hero.Pos );                                          // sound FX
        }
        else
        {
            Controller.CreateMagicEffect( to );                                                                 // bridge creation successful
            Item.AddItem( ItemType.Res_Bridge_Wood, -1 );
            MasterAudio.PlaySound3DAtVector3( "Build Complete", to );                                           // Sound FX
            b.Mine.AnimateIconTimer = .3f;
        }
        string txt = aux + "\nSupport Points: " +
        BridgeSupportPoints.Count + "\nBridge Lenght: " + bestdist;
        UI.I.SetTurnInfoText( txt, 3, col );                                                                    // text message
        List<Vector2> done = new List<Vector2>();
        for( int i = 0; i < BridgeSupportPoints.Count; i++ )
        if( done.Contains( BridgeSupportPoints[ i ] ) == false )
        {
            Vector2 pt = BridgeSupportPoints[ i ];
            int amt = 1;
            for( int j = 0; j < BridgeSupportPoints.Count; j++ )
                if( i != j && pt == BridgeSupportPoints[ j ] ) amt++;
            col = Color.red;
            string msg = "+" + amt;
            if( closest == BridgeSupportPoints[ i ] )
            { msg += "\nbase"; col = Color.green; }
            Message.CreateMessage( ETileType.NONE, ItemType.NONE, msg, pt +
            new Vector2( -.2f, -.1f ), col, false, false, 6f, 0, -1, 100 );                                     // shows support points static message            
            done.Add( pt );
        }
        BridgeSupportPoints = new List<Vector2>();
        Mine.UpdateSupportStatus( b );                                                                          // updates Support status
        return true;
    }
    public int GetBestBridgeRoute( Unit mine, ref Vector2 closest )                                   // este method usa o mesmo algoritmo do WQ para encontrar a melhor rota da ponte ate o suporte mais proximo. lembre-se do bolo que cresce 1 tile de cada vez
    {
        Map.I.ObjectID = new int[ Map.I.Tilemap.width, Map.I.Tilemap.height ];
        List<Unit> ml = new List<Unit>();
        for( int yy = ( int ) G.HS.Area.yMin - 1; yy < G.HS.Area.yMax + 1; yy++ )
        for( int xx = ( int ) G.HS.Area.xMin - 1; xx < G.HS.Area.xMax + 1; xx++ )
        if( Map.PtOnMap( Map.I.Tilemap, new Vector2( xx, yy ) ) )
        {
            Unit auxm = Map.GFU( ETileType.MINE, new Vector2( xx, yy ) );
            if( auxm && GetBridgeDir( auxm.Pos ) != -1 ) { ml.Add( auxm ); }                           // make a list of all mines for efficiency
        }

        Map.I.ObjectID[ ( int ) mine.Pos.x, ( int ) mine.Pos.y ] = 1;
        int best = Sector.TSX + Sector.TSY;
        for( int i = 1; i < 900; i++ )
        for( int m = 0; m < ml.Count; m++ )
            {
                Unit min = ml[ m ];
                int id = Map.I.ObjectID[ ( int ) min.Pos.x, ( int ) min.Pos.y ];

                if( i == id )                                                                               // if the bridge id is equal to the current i adds i + 1 to connected bridges
                {
                    int dr = GetBridgeDir( min.Pos );
                    for( int j = 0; j < 8; j++ )
                    {
                        Vector2 ntg = min.Pos + Manager.I.U.DirCord[ j ];
                        Unit tom = Map.GFU( ETileType.MINE, ntg );
                        bool res = BridgeConnection( min.Pos, ntg );                                        // are 2 positions connected

                        if( res )
                            if( BridgeSupportPoints.Contains( ntg ) )
                            {
                                if( id < best )                                                             // a better route was found
                                {
                                    best = id;
                                    closest = ntg;
                                }
                            }

                        Unit m2 = Map.GFU( ETileType.MINE, ntg );
                        if( res )
                            if( Map.I.ObjectID[ ( int ) ntg.x, ( int ) ntg.y ] == 0 )
                            {
                                Map.I.ObjectID[ ( int ) ntg.x, ( int ) ntg.y ] = i + 1;                    // adds i + 1
                            }
                    }
                }
            }
        Map.I.ObjectID = new int[ Map.I.Tilemap.width, Map.I.Tilemap.height ];
        return best;
    }
    public int GetBridgeDir( Vector2 tg )
    {
        Unit br = Map.GFU( ETileType.MINE, tg );
        if( br )
        {
            if( br.Body.MineType == EMineType.BRIDGE ) return ( int ) br.Dir;
            if( br.Body.UPMineType == EMineType.BRIDGE ) return ( int ) br.Body.UPMineDir;
        }
        return -1;
    }
    public bool BridgeConnection( Vector2 from, Vector2 to )
    {
        bool res = false;
        EDirection mov = Util.GetTargetUnitDir( from, to );
        Unit frm = Map.GFU( ETileType.MINE, from );
        Unit tgm = Map.GFU( ETileType.MINE, to );
        int drfr = GetBridgeDir( from );
        if( drfr != -1 )
        {
            if( to == from + Manager.I.U.DirCord[ drfr ] )                                                              // checks from bridge dir
                res = true;
            if( to == from + Manager.I.U.DirCord[ ( int ) Util.GetInvDir( ( EDirection ) drfr ) ] )                     // checks from bridge inv dir
                res = true;
        }
        drfr = GetBridgeDir( to );
        if( drfr != -1 )
        {
            if( from == to + Manager.I.U.DirCord[ drfr ] )                                                              // checks to bridge dir                                                                 
                res = true;
            if( from == to + Manager.I.U.DirCord[ ( int ) Util.GetInvDir( ( EDirection ) drfr ) ] )                     // checks to bridge inv dir
                res = true;
        }

        if( BridgeBlock( from, to ) == true )                                                                           // checks forced support block
            res = false;

        if( to == AuxBridgePos + Manager.I.U.DirCord[ ( int ) mov ] )                                                   // do not connect the newly created bridge to the front tile
        if( mov != AuxBridgeDir )
            res = false;

        if( frm && frm.Body.UPMineType == EMineType.BRIDGE )                                                            // do not connect upbridge to low mine
        if( tgm && tgm.Body.UPMineType == EMineType.NONE && tgm.Body.MineType != EMineType.BRIDGE )
            res = false;
        return res;
    }
    public static bool BridgeBlock( Vector2 from, Vector2 to )  // this method checks if the bridge is connected to another if one is support and another is upbridge. they are neighbors but may not be connected due to one being support
    {
        Unit frm = Map.GFU( ETileType.MINE, from );
        Unit tom = Map.GFU( ETileType.MINE, to );
        Unit frb = Map.GMine( EMineType.BRIDGE, from );
        Unit tob = Map.GMine( EMineType.BRIDGE, to );
        EDirection mov = Util.GetTargetUnitDir( from, to );
        if( frm && frm.Body.UPMineType == EMineType.BRIDGE && tob )
        {
            if( tob.Mine.FrontSupport == 1 && tom.InvDr() == mov ) return true;
            if( tob.Mine.BackSupport == 1 && tom.Dir == mov ) return true;
        }
        if( tom && tom.Body.UPMineType == EMineType.BRIDGE && frb )
        {
            if( frb && tom.Mine.FrontSupport == 1 && tom.InvDr() == mov ) return true;
            if( frb && tom.Mine.BackSupport == 1 && tom.Dir == mov ) return true;
        }
        return false;
    }
    public bool CalculateBridgeSupportPoints( Vector2 from, Vector2 to, ref List<Vector2> suplist, ref int search, ref List<int> pointsid, int dist, bool up )
    {
        if( Map.PtOnMap( Map.I.Tilemap, to ) == false ) return false;
        Unit frm = Map.GFU( ETileType.MINE, from );
        Unit tom = Map.GFU( ETileType.MINE, to );
        Unit frb = Map.GMine( EMineType.BRIDGE, from );
        Unit tob = Map.GMine( EMineType.BRIDGE, to );
        Unit tol = Map.GMine( EMineType.LADDER, to );
        EDirection mov = Util.GetTargetUnitDir( from, to );
        if( search > 1 )
            if( tom == null ) return false;
        if( Input.GetKey( KeyCode.F1 ) )
            Debug.Log( "from: " + from + " to: " + to );
        if( isSupport( from, to ) )
        {                                                                                    // Support point found!
            if( search > 1 )
                suplist.Add( to );
            pointsid.Add( dist );

            if( Input.GetKey( KeyCode.F1 ) )
                Debug.Log( "Support Found! from: " + from + " to: " + to );
            return false;
        }
        if( Map.I.ObjectID[ ( int ) to.x, ( int ) to.y ] > 0 )
            return false;
        dist++;
        search++;
        Map.I.ObjectID[ ( int ) to.x, ( int ) to.y ] = dist;                                 // fill map obj id matrix

        for( int i = 0; i < 8; i++ )
        {
            Vector2 tg = to + Manager.I.U.DirCord[ i ];
            Unit tgm = Map.GFU( ETileType.MINE, tg );
            Unit tgb = Map.GMine( EMineType.BRIDGE, tg );
            bool connected = false;
            //if( tgm && suplist.Contains( tgm.Pos ) == false ) this was changed to avoid the missing second support bug
            if( tgm )
            {
                connected = BridgeConnection( to, tg );                                                                 // are two positions bridge connected?                
            }

            if( connected )
            {
                //Debug.Log( "search: " + tg );
                CalculateBridgeSupportPoints( to, tg, ref suplist, ref search, ref pointsid, dist, up );                // Recursive calculation
            }
        }
        return false;
    }
    public bool isSupport( Vector2 from, Vector2 to )
    {
        Unit frm = Map.GFU( ETileType.MINE, from );
        Unit tom = Map.GFU( ETileType.MINE, to );
        Unit frb = Map.GMine( EMineType.BRIDGE, from );
        Unit tob = Map.GMine( EMineType.BRIDGE, to );
        Unit tol = Map.GMine( EMineType.LADDER, to );
        bool connected = BridgeConnection( from, to );
        if( connected == false ) return false;
        if( tom )
        if( tom.Body.MineType == EMineType.TUNNEL ) return false;
        if( frm && frm.Body.UPMineType == EMineType.BRIDGE )
            if( tom )
            {
                if( tom.Body.UPMineType == EMineType.NONE ) return false;
                if( tom.Body.UPMineType == EMineType.LADDER ) return false;
            }

        if( frb || ( frm && frm.Body.UPMineType == EMineType.BRIDGE ) )
            if( tom && tob == null && tol == null &&
              ( tom.Body.UPMineType == EMineType.NONE ||
              ( tom.Body.UPMineType != EMineType.BRIDGE &&
                tom.Body.UPMineType != EMineType.LADDER ) ) )
                return true;
        return false;
    }
    public bool IsBridgeSupport( Vector2 from, Vector2 to )
    {
        for( int i = 0; i < 8; i++ )
        {
            Vector2 tg = to + Manager.I.U.DirCord[ i ];
            Unit tgm = Map.GFU( ETileType.MINE, tg );
            if( tgm && tg != from )
            {
                if( tgm.Body.MineType == EMineType.BRIDGE )                                                             // bridge
                    if( ( int ) tgm.Dir == i || tgm.Dir == Util.GetInvDir( ( EDirection ) i ) )
                        return true;
                if( tgm.Body.UPMineType == EMineType.BRIDGE )                                                           // up bridge
                    if( ( int ) tgm.Body.UPMineDir == i || tgm.Body.UPMineDir == Util.GetInvDir( ( EDirection ) i ) )
                        return true;
            }
        }
        return false;
    }
    #endregion
    #region Hook
    public bool KickTheHook( Vector2 from, Vector2 to, ref Unit destmine )
    {
        if( Map.I.CheckArrowBlockFromTo( from, to, G.Hero ) == true ) return false;
        Unit mudfr = Map.I.GetMud( from );
        if( mudfr == null )
            if( Unit.Body.MinePushSteps < 1 ) return false;
        Unit mudto = Map.I.GetMud( to );
        if( mudto != null ) return false;

        for( int i = 1; i < Map.I.Tilemap.width; i++ )
        {
            Vector2 tg = to + ( Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] * i );
            Vector2 lpos = to + ( Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] * ( i - 1 ) );
            Unit dm = Map.GFU( ETileType.MINE, tg );
            if( dm )
                if( dm.Body.MineType != EMineType.HOOK )
                    if( Unit.Body.RopeConnectFather != null && Unit.Body.RopeConnectFather.Count > 0 )
                    {
                        destmine = dm;
                        Transform tr = PoolManager.Pools[ "Pool" ].Spawn( "Archer Arrow" );                          // Creates the Kick hook animation since no kick is performed, only a hook connection
                        ArcherArrowAnimation ar = tr.gameObject.GetComponent<ArcherArrowAnimation>();
                        ArcherArrowAnimation.FixedRotation = G.Hero.Spr.transform.eulerAngles.z;
                        ar.AuxUnit = null;
                        for( int j = 0; j < Unit.Body.RopeConnectFather.Count; j++ )
                            if( Unit.Body.RopeConnectFather[ j ].Body.RopeConnectSon == Unit )
                                ar.AuxUnit = Unit.Body.RopeConnectFather[ j ];
                        ar.Create( Unit.Pos, null, tr, tg, EBoltType.Hook );
                        MasterAudio.PlaySound3DAtVector3( "Herb Move", G.Hero.Pos );                                 // fx
                        MasterAudio.PlaySound3DAtVector3( "Chain Rattling", G.Hero.Pos );                            // Sound FX
                        Unit.Body.MinePushSteps--;
                        if( ar.AuxUnit != null )                                                                     // in this case, a chained hook is kicked over another chained hook
                        {
                            if( destmine.Body.MineType == EMineType.HOOK )
                                destmine.Body.MinePushSteps += Unit.Body.MinePushSteps + 1;
                            ConnectHook( Unit, destmine, false, false );                                             // Connect hook
                            MasterAudio.PlaySound3DAtVector3( "Chain Rattling", G.Hero.Pos );                        // Sound FX
                            return true;
                        }
                        break;
                    }
            List<ETileType> ex = new List<ETileType>();
            ex.Add( ETileType.ARROW ); ex.Add( ETileType.WATER ); ex.Add( ETileType.PIT );
            if( Util.CheckBlock( Unit.Pos, to, G.Hero, true, true, false, false, false, true, ex ) ) break;
            bool block = Util.CheckBlock( to, tg, G.Hero, true, true, false, false, false, true, ex );
            if( Unit.CheckLeverBlock( false, to, tg, false ) ) block = true;
            if( Sector.GetPosSectorType( tg ) == Sector.ESectorType.GATES ) block = true;
            if( Map.GFU( ETileType.MINE, tg ) ) block = true;
            if( Map.LowTerrainBlock( tg ) ) block = true;

            if( block )
            {
                Controller.MoveFlyingUnitTo( ref Unit, Unit.Pos, lpos );                                   // real kick performed
                MasterAudio.PlaySound3DAtVector3( "Herb Move", G.Hero.Pos );
                if( Unit.Body.RopeConnectFather.Count > 0 )
                    MasterAudio.PlaySound3DAtVector3( "Chain Rattling", G.Hero.Pos );                      // Sound FX
                Unit.Mine.AnimateIconTimer = 2.4f;
                if( mudfr == null )
                    Unit.Body.MinePushSteps--;
                return true;
            }
        }
        return false;
    }
    public bool UpdateHookMinePush( bool apply, Vector2 from, Vector2 to )
    {
        if( Unit.Body.MineType != EMineType.HOOK ) return true;
        Unit destmine = Map.GFU( ETileType.MINE, to );
        EDirection dr = Util.GetTargetUnitDir( from, to );
        bool fx = true;
        if( destmine == null )
            if( dr == G.Hero.Dir )                                                                          // Same push direction as hero: try to kick
            {
                bool res = KickTheHook( from, to, ref destmine );                                           // Kick
                if( res ) return false;
                fx = false;
            }

        if( destmine )
        {
            if( destmine.Body.MineType == EMineType.HOOK )                                                 // solo hook merge
            {
                Unit.Body.MinePushSteps += destmine.Body.MinePushSteps + 1;                                // hook steps sum
                ConnectHook( destmine, Unit );
                Controller.CreateMagicEffect( destmine.Pos );
            }
            else
            {
                if( Unit.Body.RopeConnectFather == null ||
                    Unit.Body.RopeConnectFather.Count < 1 ) return false;                                  // solo hook does not connect
                ConnectHook( Unit, destmine, fx );                                                         // hook conect
                return false;
            }
            return true;
        }
        else
            if( Unit.Body.MinePushSteps <= 0 )                                                             // Not enough push steps
            if( Map.I.GetMud( from ) == null )
                return false;

        MasterAudio.PlaySound3DAtVector3( "Chain Rattling", G.Hero.Pos );                                  // Sound FX
        return true;
    }
    //public void UpdateLadderDirection()
    //{
    //    if( Unit.TileID != ETileType.MINE ) return;
    //    if( Unit.Body.MineType != EMineType.LADDER ) return;
    //    Vector2 tgg = Unit.Pos + Manager.I.U.DirCord[ ( int ) Unit.Dir ];
    //    if( Map.IsClimbable( tgg, false ) ) return;

    //    int id = -1;
    //    for( int i = 0; i < 8; i++ )
    //    {
    //        Vector2 tg = Unit.Pos + Manager.I.U.DirCord[ i ];
    //        Unit un = Map.GFU( ETileType.MINE, tg );
    //        if( Map.IsClimbable( tg, false ) &&
    //          ( un && un.Body.MineType != EMineType.LADDER ) )
    //        {
    //            id = ( int ) Unit.Dir + i;
    //            if( id >= 8 ) id -= 8;                
    //            break;
    //        }
    //    }
    //    if( id != -1 )
    //    {
    //        Unit.RotateTo( ( EDirection ) id );
    //    }
    //}
    //public void UpdateBridgeDestruction()
    //{
    //    if( Unit.TileID != ETileType.MINE ) return;
    //    if( Unit.Body.MineType != EMineType.BRIDGE ) return;
    //    int count = 0;
    //    List<int> drl = new List<int>();
    //    for( int i = 0; i < 8; i++ )
    //    if( i == ( int ) Unit.Dir || i == ( int ) Util.GetInvDir( Unit.Dir ) )
    //    {
    //        Vector2 tg = Unit.Pos + Manager.I.U.DirCord[ i ];
    //        Unit un = Map.GFU( ETileType.MINE, tg );
    //        Vector2 btg = Unit.Pos + Manager.I.U.DirCord[ ( int ) Util.GetInvDir( ( EDirection ) i ) ];
    //        Unit bk = Map.GFU( ETileType.MINE, btg );
    //        if( un )
    //        {
    //            count++;
    //            drl.Add( i );
    //        }
    //    }

    //    if( count < 1 )
    //    {
    //        Map.I.KillList.Add( Unit );
    //        return;
    //    }
    //}
    public static bool ConnectHook( Unit hookmine, Unit destmine, bool updchain = true, bool fx = true )
    {
        if( destmine.Body.RopeConnectSon == hookmine ) return false;
        for( int i = 0; i < hookmine.Body.RopeConnectFather.Count; i++ )
            if( destmine.Body.RopeConnectFather.Contains( hookmine.Body.RopeConnectFather[ i ] ) == false )
                destmine.Body.RopeConnectFather.Add( hookmine.Body.RopeConnectFather[ i ] );

        hookmine.Kill();
        for( int i = 0; i < destmine.Body.RopeConnectFather.Count; i++ )
        {
            destmine.Body.RopeConnectFather[ i ].Body.RopeConnectSon = destmine;
            destmine.Body.RopeConnectFather[ i ].Body.Rope.gameObject.SetActive( true );
            if( updchain )
                destmine.Body.RopeConnectFather[ i ].UpdateChainSizes( destmine.Pos );
        }
        if( fx ) MasterAudio.PlaySound3DAtVector3( "Raft Merge", G.Hero.Pos );
        return true;
    }
    public static void ConnectShackle( Unit shackle, Unit _unit )
    {
        _unit.Body.RopeConnectFather.Add( shackle.Body.RopeConnectSon );
        Vector2 pos = shackle.Pos;
        Unit spike = shackle.Body.RopeConnectSon;
        spike.Body.RopeConnectFather.Remove( shackle );
        shackle.Body.RopeConnectFather = new List<Unit>();
        shackle.Body.RopeConnectSon = null;
        shackle.Kill();

        for( int i = 0; i < _unit.Body.RopeConnectFather.Count; i++ )
        {
            _unit.Body.RopeConnectFather[ i ].Body.RopeConnectSon = _unit;
            _unit.Body.RopeConnectFather[ i ].UpdateChainSizes( _unit.Pos );
            _unit.Body.RopeConnectFather[ i ].Body.Rope.gameObject.SetActive( true );
            _unit.Body.RopeConnectFather[ i ].Body.ShackleDistance =
            Util.Manhattan( pos, _unit.Body.RopeConnectFather[ i ].Pos ) - 1;
            _unit.Body.OriginalHookType = EMineType.SHACKLE;
        }
        if( _unit.UnitType == EUnitType.HERO )
            MasterAudio.PlaySound3DAtVector3( "Hero Damage", G.Hero.Pos );
        else
            MasterAudio.PlaySound3DAtVector3( "Monster Falling", G.Hero.Pos );
        Body.CreateDeathFXAt( shackle.Pos );
    }
    public static void AddMineCrackEffect( Unit mine, Vector2 tg )
    {
        mine.Mine.AnimateIconTimer = .01f;
        int side = ( int ) Util.GetTargetUnitDir( mine.Pos, G.Hero.Pos );                                // Dark mine
        mine.Body.MineSideHitCount[ side ]++;
    }
    public static bool UpdateLeverMine( Unit un, bool apply, Vector2 from, Vector2 tg )
    {
        if( IgnoreLeverPush ) return false;
        EDirection destdr = Util.GetTargetUnitDir( from, tg );
        Unit destmine = Map.GFU( ETileType.MINE, tg );
        if( destmine )
        {
            return false;
        }

        for( int i = 0; i < 8; i++ )
        {
            bool clock = false;
            EDirection res = EDirection.NONE;
            Vector2 desttg = tg + Manager.I.U.DirCord[ i ];
            Unit mine = Map.GFU( ETileType.MINE, desttg );
            if( mine != null )
            if( mine.Body.MineHasLever() )
            if( IgnoreLevelPushList.Contains( mine ) == false )                                                  // is this mine being pushed by raft at this moment
            {
                EDirection ld = mine.Body.MineLeverDir;
                Vector2 lpos = mine.Pos + Manager.I.U.DirCord[ ( int ) ld ];
                int dif = 0;
                if( lpos == tg )
                {
                    if( destdr == ( EDirection ) Manager.I.U.InvDir[ ( int ) ld ] )                             // Invert Lever Direction frontal impact
                    {
                        Vector2 levdest = mine.Pos + Manager.I.U.DirCord[ ( int ) destdr ];
                        List<ETileType> ex = new List<ETileType>();
                        ex.Add( ETileType.WATER ); ex.Add( ETileType.PIT );
                        bool ok = !Util.CheckBlock( new Vector2( -1, -1 ), levdest, G.Hero, true, true, false, true, false, true, ex );
                        if( Map.GFU( ETileType.MINE, levdest ) ) ok = false;

                        if( ok && apply )
                        {
                            mine.Body.MineLeverDir = ( EDirection ) Manager.I.U.InvDir[ ( int ) ld ];
                            mine.Mine.AnimateIconTimer = .01f;
                            MasterAudio.PlaySound3DAtVector3( "Mechanical Sound", un.Pos );
                        }
                        if( ok == false ) return true;
                    }
                    else
                    {
                        res = EDirection.NONE;                                                                 // Check Lever directions
                        if( ld == EDirection.N )
                        {
                            if( from.x < tg.x ) res = EDirection.NE;
                            if( from.x > tg.x ) res = EDirection.NW;
                        }

                        if( ld == EDirection.S )
                        {
                            if( from.x < tg.x ) res = EDirection.SE;
                            if( from.x > tg.x ) res = EDirection.SW;
                        }

                        if( ld == EDirection.E )
                        {
                            if( from.y < tg.y ) res = EDirection.NE;
                            if( from.y > tg.y ) res = EDirection.SE;
                        }

                        if( ld == EDirection.W )
                        {
                            if( from.y < tg.y ) res = EDirection.NW;
                            if( from.y > tg.y ) res = EDirection.SW;
                        }

                        if( ld == EDirection.NE )                                                          // Diagonals check
                        {
                            if( from.x < tg.x || from.y > tg.y ) res = EDirection.E;
                            if( from.x > tg.x || from.y < tg.y ) res = EDirection.N;
                        }

                        if( ld == EDirection.SE )
                        {
                            if( from.x < tg.x || from.y < tg.y ) res = EDirection.E;
                            if( from.x > tg.x || from.y > tg.y ) res = EDirection.S;
                        }

                        if( ld == EDirection.SW )
                        {
                            if( from.x < tg.x || from.y > tg.y ) res = EDirection.S;
                            if( from.x > tg.x || from.y < tg.y ) res = EDirection.W;
                        }

                        if( ld == EDirection.NW )
                        {
                            if( from.x < tg.x || from.y < tg.y ) res = EDirection.N;
                            if( from.x > tg.x || from.y > tg.y ) res = EDirection.W;
                        }

                        dif = res - ld;
                        if( dif == 7 ) dif = -1;
                        if( dif == -7 ) dif = 1;
                        dif *= -1;

                        if( apply == false )
                        {
                            Vector2 levdest = mine.Pos + Manager.I.U.DirCord[ ( int ) res ];               // only test if move is possible
                            List<ETileType> ex = new List<ETileType>();
                            ex.Add( ETileType.WATER ); ex.Add( ETileType.PIT );
                            bool ok = !Util.CheckBlock( new Vector2( -1, -1 ), levdest, G.Hero, true, true, false, true, false, true, ex );
                            if( Map.GFU( ETileType.MINE, levdest ) ) ok = false;
                            if( ok == false ) return true;
                            return false;
                        }

                        if( res != EDirection.NONE )
                        {
                            int range = 1;
                            for( int y = ( int ) mine.Pos.y - range; y <= mine.Pos.y + range; y++ )
                            for( int x = ( int ) mine.Pos.x - range; x <= mine.Pos.x + range; x++ )
                            {
                                Unit tgmine = Map.GFU( ETileType.MINE, new Vector2( x, y ) );                   // change mine directions
                                if( tgmine != null )
                                {
                                    int df = dif;
                                    if( mine.Pos == new Vector2( x, y ) )
                                    {
                                        tgmine.Rotate( dif * -1 );
                                        tgmine.Body.MineLeverDir =
                                        Map.I.RotateDir( tgmine.Body.MineLeverDir, dif );
                                        if( dif == -1 ) clock = true;
                                    }
                                    else
                                        if( mine.Body.LeverMine )
                                        {
                                            tgmine.Rotate( dif );
                                            tgmine.Body.MineLeverDir =
                                            Map.I.RotateDir( tgmine.Body.MineLeverDir, dif * -1 );
                                        }
                                    tgmine.Mine.AnimateIconTimer = .01f;
                                }
                            }
                            MasterAudio.PlaySound3DAtVector3( "Mechanical Sound", un.Pos );
                        }
                    }
                }
            }
            if( res != EDirection.NONE ) PullChain( mine, clock );                                                // Pulls the chain
        }
        return false;
    }
    public static bool UpdateLeverMineBump( Vector2 from, Vector2 tg )
    {
        EDirection destdr = Util.GetTargetUnitDir( from, tg );
        Unit destmine = Map.GFU( ETileType.MINE, tg );
        if( destmine && destmine.Body.RopeConnectSon && destmine.Body.MineHasLever() )
        {
            Unit next = null;
            GetAllMineSons( destmine, ref next );
            return true;
        }
        return false;
    }
    public static void PullChain( Unit levermine, bool clock )
    {
        if( levermine == null ) return;
        if( levermine.Body.RopeConnectSon == null ) return;
        if( clock && !levermine.Body.ClockwiseChainPush ) return;
        if( !clock && levermine.Body.ClockwiseChainPush ) return;
        levermine = levermine.Body.MineLeverActivePuller;
        Unit tgu = levermine.Body.RopeConnectSon;
        Vector2 bestpos = tgu.Control.GetBestStandardMove( levermine.Pos, false );
        Unit destmine = Map.GFU( ETileType.MINE, bestpos );
        if( destmine && destmine != tgu )
        {
            if( Map.I.CheckArrowBlockFromTo( G.Hero.Pos, bestpos, G.Hero ) == false )
            {
                if( destmine == levermine )
                    if( levermine.Body.RopeConnectSon == tgu ) return;

                if( destmine.Body.MineType != EMineType.HOOK )
                {
                    if( tgu.Body.MineType == EMineType.HOOK )
                        ConnectHook( tgu, destmine );                                    // hook over mine or
                    return;
                }
                else
                {
                    if( destmine.Body.MineType == EMineType.HOOK )                       // hook pushed over hook: add steps
                        if( tgu.Body.MineType == EMineType.HOOK )
                            tgu.Body.MinePushSteps += destmine.Body.MinePushSteps;

                    if( ConnectHook( destmine, tgu ) == false )                          // mine over hook?
                        return;
                }
            }
        }

        tgu.Control.RealtimeMoveTried = true;
        bool res = tgu.CanMoveFromTo( true, tgu.Pos, bestpos, G.Hero );
        if( res == false ) return;

        if( tgu.Mine )
            tgu.Mine.AnimateIconTimer = 1;                                               
        for( int i = 0; i < tgu.Body.RopeConnectFather.Count; i++ )
            tgu.Body.RopeConnectFather[ i ].UpdateChainSizes( bestpos );
        if( tgu.Body.RopeConnectSon != null )
            tgu.UpdateChainSizes( tgu.Body.RopeConnectSon.transform.position );
        MasterAudio.PlaySound3DAtVector3( "Chain Rattling", G.Hero.Pos );
    }
    #endregion
    public static List<Unit> GetAllMineSons( Unit destmine, ref Unit next )
    {
        for( int y = ( int ) G.HS.Area.yMin - 1; y < G.HS.Area.yMax + 1; y++ )
            for( int x = ( int ) G.HS.Area.xMin - 1; x < G.HS.Area.xMax + 1; x++ )
            {
                Unit tgm = Map.GFU( ETileType.MINE, new Vector2( x, y ) );
                if( tgm ) tgm.Body.EffectList[ 2 ].gameObject.SetActive( false );
            }
        Unit father = destmine;
        List<Unit> sl = new List<Unit>();
        List<Unit> done = new List<Unit>();
        for( int i = 0; i < 999; i++ )
        {
            if( destmine.Body.RopeConnectSon )
            {
                if( i > 0 && father == destmine ) break;
                sl.Add( destmine );
                destmine = destmine.Body.RopeConnectSon;
            }
            else break;
        }

        if( sl != null && sl.Count >= 2 )
            for( int i = 0; i < sl.Count; i++ )
            {
                if( sl[ i ] == father.Body.MineLeverActivePuller )
                {
                    if( i + 1 < sl.Count )
                        next = sl[ i + 1 ];
                    else
                        next = father;
                    break;
                }
            }

        if( sl.Count <= 1 ) next = father;
        father.Body.MineLeverActivePuller = next;
        father.Body.MineLeverActivePuller.Body.EffectList[ 2 ].gameObject.SetActive( true );
        return sl;
    }
    public static bool UpdateAlcovePush( Vector2 tg, Vector2 from )
    {
        if( G.HS.AlcovePush < 1 ) return false;
        Unit mine = Map.GFU( ETileType.MINE, tg );                                                // No mine in the target
        if( mine == null ) return false;
        if( mine.Body.MineType == EMineType.VAULT ) return false;

        Vector2 dest = new Vector2( -1, -1 );
        int count = 0;
        for( int i = 0; i < 8; i++ )
        {
            Vector2 tgg = tg + Manager.I.U.DirCord[ i ];                                          // Checks possible targets
            if( mine.CanFlyFromTo( false, tg, tgg ) )
            if( tgg != G.Hero.Pos )
                {
                    count++;
                    dest = tgg;
                }
        }
        if( count == 1 )                                                                          // Perform Alcove Push
        {
            if( Mine.SafeMining )
            {
                Message.GreenMessage( "Alcove Push Detected!\nTry Again!" );                      // Safe Mining Msg
                Mine.SafeMining = false;                                                          // Safe Mining
                return true;
            }
            G.HS.AlcovePush--;
            mine.CanFlyFromTo( true, tg, dest );
            mine.Mine.AnimateIconTimer = 1;
            Mine.UpdateChiselText = true;
            MasterAudio.PlaySound3DAtVector3( "Mechanical Sound", G.Hero.Pos );
            return true;
        }
        return false;
    }
    public static bool UpdateVaultBump( Unit mine, Vector2 tg, Vector2 from )
    {
        if( mine.Body.MineType != EMineType.VAULT ) return false;
        if( G.HS.VaultReuse > 0 )
        {
            mine.Mine.MineBonusGiven = false;
            mine.Mine.Activated = false;                                                                             //
            mine.Mine.UpdateText = true;
            G.HS.VaultReuse--;
            Message.GreenMessage( "Vault Reused! " );
        }
        return true;
    }
    public static bool UpdateJumperMine( Unit mine, Vector2 from, Vector2 tg )
    {
        if( mine.Mine.JumperMine == false ) return false;
        if( Map.I.CheckArrowBlockFromTo( from, tg, G.Hero ) == true ) return false;
        Vector2 dtg = new Vector2( -1, -1 );
        int dist = 1;
        EDirection mov = Util.GetTargetUnitDir( from, tg );
        for( int i = 1; i < Sector.TSX; i++ )                                                                      // calculate jump target
        {
            Vector2 fr = dtg;
            if( fr.x == -1 ) fr = from;
            dtg = G.Hero.Pos + ( Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] * i );
            if( Map.I.CheckArrowBlockFromTo( fr, dtg, G.Hero ) == true ) return false;
            Unit tgm = Map.GFU( ETileType.MINE, dtg );
            if( i == 1 )            
            if( tgm == null || tgm == mine )
            {
                for( int y = 1; y < Sector.TSX; y++ )
                {
                    Vector2 mtg = tg + ( Manager.I.U.DirCord[ ( int ) mov ] * y );                                 // Find sliding destination
                    Unit tgm2 = Map.GFU( ETileType.MINE, mtg );
                    Unit mn = Map.I.GetUnit( mtg, ELayerType.MONSTER );
                    if( tgm2 )
                    if( Mine.GetMineBonusCount( mtg ) == 0 )                                                       // is source mine bonus free?
                    if( tgm2.Body.MineType != EMineType.VAULT )
                    if( mn == null )
                    {
                        Mine.SetMineBonus( ref tgm2, 15, true );
                        Mine.SetMineBonus( ref mine, 15, false );
                        tgm2.Body.Sprite3.transform.position = mine.Body.Sprite3.transform.position;              // Slide animation FX
                        tgm2.Mine.AnimateIconTimer = 2.4f;
                        tgm2.Mine.MineBonusDir = mov;
                        MasterAudio.PlaySound3DAtVector3( "Mine Switch", mtg );                                   // Sound FX
                        break;
                    }
                    if( tgm2 == null )
                    {
                        return false;                                                                             // no mine in the end of the row                      
                    }
                }
                return false;                                                                                     // no mine in front of hero or jumper in front
            }   

            if( tgm == null ) break;
            dist++;                                                                                   // dist increment
        }
        if( G.Hero.CanMoveFromTo( false, G.Hero.Pos, dtg, G.Hero ) )                                  // test jump
        {
            MasterAudio.PlaySound3DAtVector3( "Hero Jump", mine.Pos );                                // sound FX
            G.Hero.CanMoveFromTo( true, G.Hero.Pos, dtg, G.Hero );                                    // perform jump       
            Map.I.LineEffect( from, dtg, 3.5f, .5f, Color.blue, Color.blue, .3f );                    // Travel Line FX 
            mine.Mine.JumperMine = false;
            Controller.CreateMagicEffect( mine.Pos );                                                 // Magic effect
            Map.I.KickTimer = .35f;
            return true;
        }
        return false;
    }

    public static bool UpdateMineJump( Unit mine, Vector2 from, Vector2 tg )
    {
        if( Map.I.CheckArrowBlockFromTo( from, tg, G.Hero ) == true ) return false;
        if( Mine.GetMineBonusCount( tg, true, false ) > 0 ) return false;                             // needs cleared front mine
        if( G.HS.HeroJumpOverMinesItemID < 0 ) return false; 
        EDirection mov = Util.GetTargetUnitDir( from, tg );
        if( mov != G.Hero.Dir ) return false;
        Vector2 dtg = new Vector2( -1, -1 );
        int dist = 1;
        for( int i = 1; i < Sector.TSX; i++ )                                                         // calculate jump target
        {
            Vector2 fr = dtg;
            if( fr.x == -1 ) fr = from;
            dtg = mine.Pos + ( Manager.I.U.DirCord[ ( int ) mov ] * i );
            if( Map.I.CheckArrowBlockFromTo( fr, dtg, G.Hero ) == true ) return false;
            Unit tgm = Map.GFU( ETileType.MINE, dtg );
            Unit mn = Map.I.GetUnit( dtg, ELayerType.MONSTER );
            if( mn == null )
            if( tgm == null ) break;
            dist++;                                                                                   // dist increment
        }
        float cost = dist * G.HS.HeroJumpOverMinesCost;
        string msg = "x" + cost + " Jump Done!";
        float amt = Item.GetNum( ( ItemType ) G.HS.HeroJumpOverMinesItemID );
        if( G.Hero.CanMoveFromTo( false, G.Hero.Pos, dtg, G.Hero ) )                                  // test jump
        {
            if( amt < cost )
            {
                msg = "Can't Jump!";
                goto Message;
            }
            msg = "x" + cost + " Jump Done!";
            if( Mine.SafeMining )
            {
                msg = "x" + cost + " Jump Detected!\nProceed?";
                Mine.SafeMining = false;                                                              // Safe Mining
                goto Message;
            }
            Item.AddItem( ( ItemType ) G.HS.HeroJumpOverMinesItemID, -cost );                         // Charges item
            MasterAudio.PlaySound3DAtVector3( "Hero Jump", mine.Pos );                                // sound FX
            G.Hero.CanMoveFromTo( true, G.Hero.Pos, dtg, G.Hero );                                    // perform jump
            Message:
            Message.CreateMessage( ETileType.NONE, ( ItemType )
            G.HS.HeroJumpOverMinesItemID, msg, G.Hero.Pos, Color.white );                             // Safe Mining Msg
            Map.I.LineEffect( from, dtg, 3.5f, .5f, Color.blue, Color.blue, .3f );                    // Travel Line FX 
            Map.I.KickTimer = .35f;
            return true;
        }
        return false;
    }
}