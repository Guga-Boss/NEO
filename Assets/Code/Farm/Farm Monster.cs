using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using DarkTonic.MasterAudio;

#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class Farm : MonoBehaviour
{
    public static List<Unit> ProcessedChicken = new List<Unit>();
    public bool UpdateBlockingMonsters( Item it )
    {
        if( Item.IsPlagueMonster( ( int ) it.Type ) == true )                                   // Updates all the farm monster types                                   
        {
            Map.I.Farm.UpdateBlockingMonsterShuffle( it );
            return true;
        }
        return false;
    }

    public void UpdateBlockingMonsterShuffle( Item it )
    {
        if( Manager.I.GameType != EGameType.FARM ) return;
        if( Manager.I.Status == EGameStatus.LEVELINTRO ) return;

        float mcount = GetMonsterNumber( it );                                                   // Calculates number of monsters based on quest factor
        if( Item.IsPlagueMonster( ( int ) it.Type, false ) )
            Item.SetAmt( it.Type, ( int ) mcount );
        it.ProductionCount = 0;
        List<Vector2> poslist = new List<Vector2>();
        KillAllMonsters( it.Type );
        if( mcount < 1 ) return;
        for( int i = 0; i < mcount; i++ )                                                        // Reposition all the monsters 
            AddMonster( it );
    }
    public void KillAllMonsters( ItemType it )
    {
        for( int y = 0; y < Map.I.Tilemap.height; y++ )                                          // Clears all the plague monsters
        for( int x = 0; x < Map.I.Tilemap.width; x++ )
        {
            if( Map.I.Unit[ x, y ] )
            if( Map.I.Unit[ x, y ].TileID == ETileType.PLAGUE_MONSTER )
            if( Map.I.Unit[ x, y ].Variation == ( int ) it )
                Map.I.Unit[ x, y ].Kill();
        }
    }
    public int GetMonsterNumber( Item it )
    {
        if( it.Type == ItemType.HoneyComb )                                                                     // honeycomb 
            return ( int ) it.Count / 2;
        if( it.Type == ItemType.Feather )                                                                       // feather 
            return FeathersPlaced;

        float mcount = 0;
        float tot = 0;
        for( int i = 0; i < Map.I.RM.RMList.Count; i++ )
        {
            float percn = Item.GetNum( Inventory.IType.Inventory,                                                // Completition Percentage
            ItemType.Adventure_Completion, i );
            if( percn > 100 ) percn = 100;                                                                       // no genius trophy

            if( percn > 0 )
            {
                float num = Map.I.RM.RMList[ i ].ChooserMonsterGenerationFactor;
                if( it.Type == ItemType.Plague_Monster_Spawner )
                    num = Map.I.RM.RMList[ i ].SpawnerPlagueMonsterGenerationFactor;
                if( it.Type == ItemType.Plague_Monster_Slayer )
                    num = Map.I.RM.RMList[ i ].KillerPlagueMonsterGenerationFactor;
                if( it.Type == ItemType.Plague_Monster_Kickable )
                    num = Map.I.RM.RMList[ i ].KickablePlagueMonsterGenerationFactor;
                if( it.Type == ItemType.Plague_Monster_Swap )
                    num = Map.I.RM.RMList[ i ].SwapPlagueMonsterGenerationFactor;
                if( it.Type == ItemType.Plague_Monster_Blocker )
                    num = Map.I.RM.RMList[ i ].BlockerPlagueMonsterGenerationFactor;
                if( it.Type == ItemType.Plague_Monster_Grabber )
                    num = Map.I.RM.RMList[ i ].GrabPlagueMonsterGenerationFactor;

                tot += num;
                mcount += Util.Percent( percn, num );
            }
        }
        int amt = G.Farm.FarmSize - 54;                                                 // be careful with the 54 number if default farm shape changes
        for( int i = 0; i < amt; i++ )                                                   
        if( i < AddMonsterList.Count )
        {
            ItemType itt = AddMonsterList[ i ];                                         // adds monsters from AddMonsterList for every cleared forest tile 
            if( itt == it.Type ) mcount++;   
        }

        return Mathf.RoundToInt( mcount );
    }

    public void AddMonster( Item it )
    {
        int minHeroDist = -1;
        if( it.Type == ItemType.Chicken )                                                        // Do not create chicken too close to hero
            minHeroDist = 3;

        List<Vector2> poslist = GetPossibleTargets( minHeroDist );                               // Get possible targets for monster

        if( poslist.Count < 1 ) return;                                                          // Creates the Monster
        int id = Random.Range( 0, poslist.Count );
        Vector2 pt = poslist[ id ];
        CreateMonster( pt, it );
    }

    private List<Vector2> GetPossibleTargets( int minHeroDist = -1 )
    {
        List<Vector2> poslist = new List<Vector2>();
        for( int tid = 0; tid < G.Farm.Tl.Count; tid++ )                                         // Loop Through all pos to add possible targets
        {
            int x = G.Farm.Tl[ tid ].x;
            int y = G.Farm.Tl[ tid ].y;
            Vector2 tg = new Vector2( x, y );

            int dist = Util.Manhattan( tg, G.Hero.Pos );
            bool ok = true;
            if( minHeroDist != -1 )
                if( dist < minHeroDist )                                                         // check min hero distance
                    ok = false;

            if( ok && CanCreatePlagueMonster( new Vector2( -1, -1 ), tg, true, true ) )          // Creates a list of possible new targets
                poslist.Add( tg );
        }
        return poslist;
    }

    public Unit CreateMonster( Vector2 pt, Item it )
    {
        if( it.Type == ItemType.Feather || it.Type == ItemType.HoneyComb )                       // Creates Feathers and hcombs only after Scorpion quest got Bronze
        if( Map.I.RM.DungeonDialog.GetTrophyCount(
            Map.I.RM.RMList[ 11 ].QuestID, ETrophyType.BRONZE ) < 1 ) return null;               // BE CAREFUL not to change Quests ID!
        if( pt == ScrollPosition ) return null;                                                  // No creation over scroll pos
        if( pt == Map.I.LevelEntrancePosition ) return null;
        Unit prefabUnit = Map.I.GetUnitPrefab( ETileType.PLAGUE_MONSTER );
        GameObject go = Map.I.CreateUnit( prefabUnit, pt );                                      // Creates the monster object
        Unit res = go.GetComponent<Unit>();
        res.Variation = ( int ) it.Type;
        res.Body.Sprite2.gameObject.SetActive( true );
        res.Body.Sprite2.color = new Color( 1, 1, 1, 0 );
        res.Body.StackAmount = 1;
        res.Control.SmoothMovementFactor = 0;
        res.Control.TurnTime = 0;
        res.Control.AnimationOrigin = pt;
        res.Graphic.transform.position = pt;
        if( Item.IsPlagueMonster( res.Variation, false ) )
        {
            res.InitPlagueMonster();                                                             // plague monster
        }
        else
        {
            res.Body.Animator = res.Spr.GetComponent<tk2dSpriteAnimator>();
            res.Body.Animator.Stop();
            res.Body.Sprite2.gameObject.SetActive( false );
            res.Spr.Collection = Map.I.SpriteCollectionList[ ( int ) ESpriteCol.ITEM ];          // others like feather, chicken, etc
            res.Spr.spriteId = G.GIT( res.Variation ).TKSprite.spriteId;
            if( res.Variation == ( int ) ItemType.Chicken )       
                res.Body.StackAmount = 12;                                                       // Chicken initial timer
        }
        return res;
    }
    public bool CanCreatePlagueMonster( Vector2 from, Vector2 to, bool heropos = true, bool init = false )
    {
        if( Map.PtOnMap( Map.I.Tilemap, to ) == false ) return false;
        if( to == Map.I.LevelEntrancePosition ) return false;
        if( init )
        {
            if( to == Map.I.LevelEntrancePosition + new Vector2( +1, 0 ) ) return false;
            if( to == Map.I.LevelEntrancePosition + new Vector2( -1, 0 ) ) return false;
            if( to == Map.I.LevelEntrancePosition + new Vector2( 0, +1 ) ) return false;
            if( to == Map.I.LevelEntrancePosition + new Vector2( 0, -1 ) ) return false;
        }

        if( Manager.I.GameType == EGameType.CUBES ) 
        if( Sector.GetPosSectorType( to ) == Sector.ESectorType.GATES ) return false;
        if( heropos )
        if( to == G.Hero.Pos ) return false;
        Unit ga = Map.I.GetUnit( to, ELayerType.GAIA );
        if( ga && ga.TileID == ETileType.WATER && Controller.GetRaft( to ) ) { } else
        if( ga && ga.BlockMovement ) return false; 
        if( Map.I.GetUnit( to, ELayerType.MONSTER ) ) return false;
        if( from.x != -1 )
        if( Map.I.CheckArrowBlockFromTo( from, to, G.Hero) == true ) return false;
        if( Map.I.GetUnit( ETileType.FIRE, to ) == false )
        if( Map.I.GetUnit( ETileType.ARROW, to ) == false )
        if( Map.I.GetUnit( to, ELayerType.GAIA2 ) ) return false;
        if( Map.I.GetUnit( ETileType.SCROLL, to ) != null ) return false;
        if( Map.CheckLeverCrossingBlock( from, to ) ) return false;
        if( Map.DoesLeverBlockMe( to, null ) ) return false;       
        return true;
    }
    public void InitMonstersAfterLoading()
    {
        InitMonster( ItemType.Plague_Monster_Cloner, ChooserMonsterList );                                 // inits plague monsters
        InitMonster( ItemType.Plague_Monster_Spawner, PushMonsterList );
        InitMonster( ItemType.Plague_Monster_Slayer, KillerMonsterList );
        InitMonster( ItemType.Plague_Monster_Kickable, KickableMonsterList );
        InitMonster( ItemType.Plague_Monster_Swap, SwapMonsterList );
        InitMonster( ItemType.Plague_Monster_Blocker, BlockerMonsterList );
        InitMonster( ItemType.Plague_Monster_Grabber, GrabMonsterList );
        InitMonster( ItemType.HoneyComb, HoneyCombList );

        InitMonsterAdding( ItemType.Plague_Monster_Cloner, ChooserMonsterList );                          // adds monsters
        InitMonsterAdding( ItemType.Plague_Monster_Spawner, PushMonsterList );
        InitMonsterAdding( ItemType.Plague_Monster_Slayer, KillerMonsterList );
        InitMonsterAdding( ItemType.Plague_Monster_Kickable, KickableMonsterList );
        InitMonsterAdding( ItemType.Plague_Monster_Swap, SwapMonsterList );
        InitMonsterAdding( ItemType.Plague_Monster_Blocker, BlockerMonsterList );
        InitMonsterAdding( ItemType.Plague_Monster_Grabber, GrabMonsterList );
        InitMonsterAdding( ItemType.HoneyComb, HoneyCombList );

        if( Item.GetNum( ItemType.Chicken ) > 0 )
        {
            AddMonster( G.GIT( ItemType.Chicken ) );                                                      // adds chicken
            Item.AddItem( ItemType.Chicken, -1 );
            MasterAudio.PlaySound3DAtVector3( "Chicken", G.Hero.Pos );                                    // FX to warn player
        }
    }

    public void InitMonster( ItemType type, List<Vector2> list )
    {
        Item it = G.GIT( type );
        for( int i = 0; i < list.Count; i++ )                                                             // Recreates monster over the saved pos
            CreateMonster( list[ i ], it );
    }

    public void InitMonsterAdding( ItemType type, List<Vector2> list )
    {
        Item it = G.GIT( type );
        int bl = GetMonsterNumber( it );                               // Check if theres a new monster and adds it case positive
        int cur = ( int ) it.Count;
        int dif = bl - cur;

        if( type == ItemType.HoneyComb )                               // honeycomb 
        {
            dif = ( int ) it.Count / 2;
            dif -= HoneyCombList.Count;
        }
        else                                                           // plague monsters
        {
            if( dif > 0 ) Item.SetAmt( it.Type, bl );
        }
        for( int i = 0; i < dif; i++ )
            AddMonster( it );
    }

    public bool UpdateFlockingMonsterPushing( Vector2 from, Vector2 to )
    {
        Unit un = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, to );
        if( un == null ) return false;
        if( Item.IsPlagueMonster( un.Variation, false ) == false ) return false;
        EDirection dr = Util.GetTargetUnitDir( from, to );
        Vector2 tg = to + Util.GetTargetUnitVector( from, to );
        if( Manager.I.GameType == EGameType.CUBES ) 
        if( Sector.GetPosSectorType( tg ) == Sector.ESectorType.GATES ) return false;
        if( CanCreatePlagueMonster( to, tg ) == false ) return false;
        int neigh = GetNeighborCount( un );
        if( neigh <= 0 ) return false;
        Item it = G.GIT( un.Variation );
        Unit mn = CreateMonster( tg, it );                                                                           // Move the monster
        mn.Body.Sprite2.color = un.Body.Sprite2.color;
        mn.Control.AnimationOrigin = tg;
        mn.Graphic.transform.position = tg;
        mn.CheckFireDamage();
        if( NextPosition == un.Pos )
        if( NextMonsterType != ItemType.NONE )
            ActivateIndicator( true, ref NextPlagueIndicator, ref NextPosition, mn );
        if( GrabPosition == un.Pos )
            ActivateIndicator( GrabActivated, ref GrabPlagueIndicator, ref GrabPosition, mn );
        un.Kill();
        Controller.ForceDiagonalMovement = true;
        MasterAudio.PlaySound3DAtVector3( "Mine Switch", G.Hero.Pos );
        return false;
    }
    public bool UpdateMonsterSpawning( Vector2 from, Vector2 to )
    {
        Unit un = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, to );
        if( un == null ) return false;
        if( un.Variation != ( int ) ItemType.Plague_Monster_Spawner ) return false;
        Vector2 tg = to + Manager.I.U.DirCord[ ( int ) G.Hero.Dir ];
        if( Manager.I.GameType == EGameType.CUBES ) 
        if( Sector.GetPosSectorType( tg ) == Sector.ESectorType.GATES ) return true;
        if( CanCreatePlagueMonster( to, tg ) == false ) return true;
        if( Map.I.GetUnit( ETileType.SCROLL, from ) != null ) return false;
        if( from == Map.I.LevelEntrancePosition ) return false;
        if( Map.I.CheckArrowBlockFromTo( from, to, G.Hero ) == true ) return false;
        Item it = G.GIT( un.Variation );
        Unit bl = CreateMonster( from, it );                                                            // Move the monster
        bl.Body.Sprite2.color = un.Body.Sprite2.color;
        int type = GetRandomMonsterType();
        it = G.GIT( type );
        un.Kill();
        Unit mn = CreateMonster( tg, it );
        mn.Control.AnimationOrigin = to;
        mn.Graphic.transform.position = to;
        mn.CheckFireDamage();
        Controller.ForceDiagonalMovement = true;
        MasterAudio.PlaySound3DAtVector3( "Monster Hit", G.Hero.Pos );
        return false;
    }

    public bool UpdateMonsterSwapping( Vector2 from, Vector2 to )
    {
        Unit un = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, to );
        if( un == null ) return false;
        if( un.Variation != ( int ) ItemType.Plague_Monster_Swap ) return false;
        Vector2 tg = to + Manager.I.U.DirCord[ ( int ) G.Hero.Dir ];
        Item it = G.GIT( un.Variation );
        Unit un2 = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, tg );
        if( un2 == null ) return true;
        if( CheckAngle( un, 2, 2 ) == false ) return false;
        if( Map.I.CheckArrowBlockFromTo( from, to, G.Hero ) == true ) return false;
        ItemType temp = ( ItemType ) un2.Variation;                                                   // Swaps Monsters
        un2.Kill();
        Unit cm = CreateMonster( tg, it );
        cm.Control.AnimationOrigin = un.Pos;
        cm.Graphic.transform.position = un.Pos;
        un.Kill();
        it = G.GIT( temp );
        cm = CreateMonster( to, it );
        cm.Control.AnimationOrigin = tg;
        cm.Graphic.transform.position = tg;
        MasterAudio.PlaySound3DAtVector3( "Mine Switch", G.Hero.Pos );
        return true;
    }

    public bool CheckAngle( Unit un, int lev, int neighlev )
    {
        EMoveType type = Util.GetMoveType( G.Hero, un.Pos );                            // Some monsters can have activation angle increased if there are Big Neighboors to the hero
        int neigh = GetAnyNeighborCount();
        if( neigh > 0 ) lev += neighlev;                                      
        if( lev <= ( int ) type ) return false;  
        return true;
    }

    public int GetAnyNeighborCount( Unit un = null, Unit excludeMe = null )
    {
        if( un == null ) un = G.Hero;
        int neigh = 0;
        for( int i = 0; i < 8; i++ )
        {
            Vector2 tg = un.Pos + ( Manager.I.U.DirCord[ i ] );
            Unit unn = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, tg );
            if( unn && Item.IsPlagueMonster( unn.Variation, false ) )
            if( GetNeighborCount( unn ) > 0 )
            if( excludeMe == null || Util.IsNeighbor( excludeMe.Pos, tg ) == false )
                neigh++;
        }
        return neigh;
    }

    public bool UpdateMonsterKilling( Vector2 from, Vector2 to )
    {
        Unit un = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, to );
        if( un == null ) return false;
        if( un.Variation != ( int ) ItemType.Plague_Monster_Slayer ) return false;
        if( Map.I.CheckArrowBlockFromTo( from, to, G.Hero ) == true ) return false;
        EDirection dr = Util.GetTargetUnitDir( from, to );
        if( CheckAngle( un, 1, 1 ) == false ) return false;
        bool res = false;
        int type = -1;
        int count = 0;
        for( int i = 0; i < Sector.TSX; i++ )
        {
            Vector2 tg = to + ( Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] * ( i + 1 ) );
            Vector2 fr = to + ( Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] * ( i ) );
            if( tg == Map.I.LevelEntrancePosition ) break;
            if( Map.I.GetUnit( ETileType.SCROLL, tg ) != null ) break;
            if( Map.I.CheckArrowBlockFromTo( fr, tg, G.Hero ) == true ) break;
            Unit un2 = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, tg );
            if( un2 == null || Item.IsPlagueMonster( un2.Variation, false ) == false )
            if( i == 0 ) return true; else break;
             if( i == 0 ) type = un2.Variation;
             if( i > 0 && un2.Variation != type ) break;
            Body.CreateDeathFXAt( un2.Pos, "Monster Hit" );
            un2.Kill();
            count++;
            int bonus = 1 + ( i / 2 );           
            Item.AddItem( ItemType.Res_Plague_Monster, bonus );
        }
        if( count >= 2 ) Message.GreenMessage("Line Kill! x" + count );
        Body.CreateDeathFXAt( un.Pos, "Monster Hit" );
        un.Kill();

        if( Manager.I.GameType == EGameType.FARM ) 
        if( ConsecutiveFeatherCollected - 1 > 0 )
            Item.AddItem( ItemType.Feather, ConsecutiveFeatherCollected - 1 );
        Controller.ForceDiagonalMovement = true;
        return res;
    }
    public bool UpdateMonsterBlocking( Vector2 from, Vector2 to )
    {
        Unit un = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, to );
        if( un == null ) return false;
        if( un.Variation != ( int ) ItemType.Plague_Monster_Blocker ) return false;
        if( Map.I.CheckArrowBlockFromTo( from, to, G.Hero ) == true ) return false;
        int price = GetUnblockPrice( un );
        float amt = Item.GetNum( ItemType.Res_Plague_Monster );
        if( amt < price ) 
        {
            Message.CreateMessage( ETileType.NONE, ItemType.Res_Plague_Monster,
                    "Not Enough!\nCost: " + price + "\nHave: " + amt, G.Hero.Pos, Color.red, true, true, 10, .1f );
            return false; 
        }

        Item.AddItem( ItemType.Res_Plague_Monster, -price );
        Body.CreateDeathFXAt( un.Pos, "Monster Hit" );        
        un.Kill();
        Controller.ForceDiagonalMovement = true;
        return false;
    }

    public bool UpdateRaftHammer( Vector2 from, Vector2 to )
    {
        Unit un = Map.I.GetUnit( ETileType.ITEM, to );
        if( un == null ) return false;
        if( un.Variation != ( int ) ItemType.Res_Raft_Hammer ) return false;
        EDirection dr = Util.GetTargetUnitDir( from, to );
        Vector2 tg = to + Manager.I.U.DirCord[ ( int ) dr ];
        Item it = G.GIT( un.Variation );
        if( Map.I.CheckArrowBlockFromTo( from, to, G.Hero ) == true ) return true;
        if( Map.I.CheckArrowBlockFromTo( to, tg, G.Hero ) == true ) return true;
        Unit water = Map.I.GetUnit( ETileType.WATER, tg );
        if( water == null ) water = Map.I.GetUnit( ETileType.PIT, tg );
        Unit raft = Controller.GetRaft( tg );
        Unit fromraft = Controller.GetRaft( to );

        if( water == null || raft != null )
        {
            MoveRaftHammer( to, tg, ref un );
        }
        else
        {
            if( Controller.IsRock( tg ) )
            {
                MoveRaftHammer( to, tg, ref un );
                return true;
            }
            if( raft == null )
            {
                Unit r = Map.I.SpawnFlyingUnit( tg, ELayerType.MONSTER, ETileType.RAFT, null );

                if( fromraft )
                {
                    r.Copy( fromraft, false, false, false );
                    r.transform.position = new Vector3( r.transform.position.x, r.transform.position.y, fromraft.transform.position.z );
                    if( r.Control.BrokenRaft )
                        r.Spr.spriteId = 79;
                    MoveRaftHammer( to, tg, ref un );
                }
                else
                {
                    r.Control.RaftGroupID = ++Map.I.RM.HeroSector.MaxRaftID;
                    r.transform.position = new Vector3( r.transform.position.x, r.transform.position.y, 0 );
                    MoveRaftHammer( to, tg, ref un );
                }
                MasterAudio.PlaySound3DAtVector3( "Build Complete", G.Hero.Pos );
                MasterAudio.PlaySound3DAtVector3( "Water Splash", G.Hero.Pos );
                Map.I.CreateExplosionFX( tg, "Fire Explosion", "" );
                return false;
            }
        }
        MasterAudio.PlaySound3DAtVector3( "Mine Switch", G.Hero.Pos );
        return false;
    }
    public bool MoveRaftHammer( Vector2 from, Vector2 to, ref Unit un )
    {
        if( un.Body.StackAmount <= 1 )
        {
            un.Kill();
            return false;
        }
        bool res = un.CanMoveFromTo( true, from, to, G.Hero );
        un.Body.StackAmount--;
        if( res == false ) return false;
        return true;
    }


    public int GetUnblockPrice( Unit un )            
    {
        int neigh = GetAnyNeighborCount( un, G.Hero );
        int price = Map.I.RM.RMD.BlockerPlagueMonsterBaseCost - neigh;                // Price in tokens to destroy Blocker

        int glow = GetAnyNeighborCount();                                             // Glow
        if( glow > 0 ) price -= glow;
        if( GetNeighborCount( un ) > 0 ) price += 1;                                  // Do not count target monster if its grouped

        if( price < 1 ) price = 1;
        return price;
    }
    public bool UpdateMonsterKicking( Vector2 from, Vector2 to )
    {
        Unit un = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, to );
        if( un == null ) return false;
        if( un.Variation == ( int ) ItemType.Plague_Monster_Cloner ) return true;                     // To prevent hero move over it
        if( un.Variation != ( int ) ItemType.Plague_Monster_Kickable ) return false;
        if( Map.I.CheckArrowBlockFromTo( from, to, G.Hero ) == true ) return false;
        Vector2 tgg = un.Pos;
        Vector2 tg = tgg;
        Vector2 tg2 = tgg;
        bool neigh = false;
        if( GetAnyNeighborCount() > 0 )
            neigh = true;
        int i = 1;
        for( i = 1; i < Map.I.Tilemap.width; i++ )
        {
            tg2 = tg;
            tg = tgg;
            tgg = un.Pos + ( Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] * i );
            if( CanCreatePlagueMonster( tg, tgg ) == false )
            if( i == 1 ) return true; else break;
        }
        if( neigh == false )                                                                         // creates the second monster
        {
            if( i <= 2 ) return true;
            if( CanCreatePlagueMonster( new Vector2( -1, -1 ), tg2 ) == false )
                return true;
            int type = GetRandomMonsterType();
            Item it = G.GIT( type );
            Unit mn = CreateMonster( tg2, it );
            mn.Control.AnimationOrigin = un.Pos;
            mn.Graphic.transform.position = un.Pos;
            mn.CheckFireDamage();
        }
        int typee = GetRandomMonsterType();
        Item itt = G.GIT( typee );
        Unit mnn = CreateMonster( tg, itt );                                                          // Moves the monster
        mnn.Control.AnimationOrigin = un.Pos;
        mnn.Graphic.transform.position = un.Pos;
        Body.CreateDeathFXAt( mnn.Pos );
        un.Kill();
        mnn.CheckFireDamage();
        Controller.ForceDiagonalMovement = true;
        MasterAudio.PlaySound3DAtVector3( "Herb Move", G.Hero.Pos );
        return false;
    }
    public void UpdateHoneyComb( Vector2 from, Vector2 to )
    {
        if( Manager.I.GameType != EGameType.FARM ) return;
        Unit un = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, to );
        if( un == null ) return;
        if( un.Variation != ( int ) ItemType.HoneyComb ) return;
        if( Map.I.CheckArrowBlockFromTo( from, to, G.Hero ) == true ) return;
        EDirection dr = Util.GetTargetUnitDir( from, to );
        if( Util.IsDiagonal( dr ) ) return;
        ConsecutiveHoneyCombsCollected++;
        Item.IgnoreMessage = false;
        int pz = ConsecutiveHoneyCombsCollected;
        Building.AddItem( true, ItemType.Honey, HeroData.I.HoneycombPrizes[ pz ] );
        Message.CreateMessage( ETileType.NONE, ItemType.Honey, "+" +
        HeroData.I.HoneycombPrizes[ pz ], G.Hero.Pos, Color.green, true, true, 4, 0, 0, 70 );
        Item.AddItem( ItemType.HoneyComb, -2 );
        un.Kill();
        MasterAudio.PlaySound3DAtVector3( "Click 1", G.Hero.Pos );
    }

    public void UpdatePlagueMonsterMovingAway()
    {
        bool neigh = false;
        for( int i = 0; i < 8; i++ )
        {
            Vector2 tg = G.Hero.Pos + ( Manager.I.U.DirCord[ i ] );
            Unit un = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, tg );
            if( un && un.Variation == ( int ) ItemType.Plague_Monster_Cloner ) neigh = true;
        }

        Unit nun = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, NextPosition );
        if( nun == null && NextPlagueIndicator.gameObject.activeSelf ) neigh = false;

        if( neigh == false )
        {
            if( NextMonsterType != ItemType.NONE )
                MasterAudio.PlaySound3DAtVector3( "Error 2", G.Hero.Pos, .7f );
            ActivateIndicator( false, ref NextPlagueIndicator, ref NextPosition, nun );
            NextMonsterType = ItemType.NONE;
        }

        neigh = false;
        for( int i = 0; i < 8; i++ )
        {
            Vector2 tg = G.Hero.Pos + ( Manager.I.U.DirCord[ i ] );
            Unit un = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, tg );
            if( un && un.Variation == ( int ) ItemType.Plague_Monster_Grabber )
            if( GrabTarget != tg )     
                neigh = true;
        }
        GrabTarget = new Vector2( -1, -1 );
        nun = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, GrabPosition );
        if( nun == null && GrabPlagueIndicator.gameObject.activeSelf ) neigh = false;

        if( neigh == false )
        {
            if( GrabActivated )
                MasterAudio.PlaySound3DAtVector3( "Error 2", G.Hero.Pos, .7f );
            ActivateIndicator( false, ref GrabPlagueIndicator, ref GrabPosition, nun );
            GrabActivated = false;
            return;
        }
    }

    public bool UpdateMonsterChoosing( Vector2 from, Vector2 to )
    {
        Unit un = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, to );
        if( un == null ) return false;
        if( un.Variation != ( int ) ItemType.Plague_Monster_Cloner ) return false;
        Vector2 front = G.Hero.Pos + ( Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] );
        Unit fun = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, front );
        if( fun == null ) return false;
        if( Item.IsPlagueMonster( fun.Variation, false ) == false ) return false;
        if( Map.I.CheckArrowBlockFromTo( from, to, G.Hero ) == true ) return false;
        if( un == fun ) return false;
        bool active = false;
        if( NextMonsterType == ItemType.NONE ) active = true;
        if( NextPosition != fun.Pos ) 
            active = true;

        if( active == false )                       
        {                                                                                       // disable indicator
            ActivateIndicator( false, ref NextPlagueIndicator, ref NextPosition, fun );
            NextMonsterType = ItemType.NONE;
        }
        else
        {
            ActivateIndicator( true, ref NextPlagueIndicator, ref NextPosition , fun );
            NextMonsterType = ( ItemType ) fun.Variation;                                       // enable indicator            
        }
        MasterAudio.PlaySound3DAtVector3( "Click 1", G.Hero.Pos );
        return true;
    }

    public bool UpdateMonsterGrabbing( Vector2 from, Vector2 to )
    {
        GrabTarget = new Vector2( -1, -1 );
        if( GrabActivated == false ) return false;
        if( from == to ) return false;
        EMoveType mv = Util.GetMoveType( G.Hero, to );
        if( mv == EMoveType.FRONT ) return false;
        if( mv == EMoveType.FRONTSIDE ) return false;

        bool neigh = false;
        if( GetAnyNeighborCount() > 0 )
            neigh = true;      
        if( neigh == false )
        if( mv == EMoveType.SIDE ) return false;                                   // Side Grabbing
        Vector2 frontOld = G.Hero.Pos + ( Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] );
        Unit fun = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, frontOld );
        if( fun == null ) return false;
        if( fun.Pos == GrabPosition ) return false;
        if( Item.IsPlagueMonster( fun.Variation, false ) == false ) return false;
        Vector2 frontNew = to + ( Manager.I.U.DirCord[ ( int ) G.Hero.Dir ] );
        if( CanCreatePlagueMonster( frontOld, frontNew, false ) == false ) return false;
        if( G.Hero.CanMoveFromTo( false, from, to, G.Hero ) == false ) return false;
        Controller.ForceDiagonalMovement = true;
        Item it = G.GIT( fun.Variation );
        Unit mn = CreateMonster( frontNew, it );                                                                     // Move the monster
        mn.Control.AnimationOrigin = frontOld;
        mn.Graphic.transform.position = frontOld;
        mn.Body.Sprite2.color = fun.Body.Sprite2.color;
        mn.CheckFireDamage();
        GrabTarget = mn.Pos;
        fun.Kill();
        MasterAudio.PlaySound3DAtVector3( "Herb Move", G.Hero.Pos );
        return false;
    }

    public bool UpdateMonsterGrabbingSwitch( Vector2 from, Vector2 to )
    {
        Unit un = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, to );
        if( un == null ) return false;
        if( un.Variation != ( int ) ItemType.Plague_Monster_Grabber ) return false;
        if( Map.I.CheckArrowBlockFromTo( from, to, G.Hero ) == true ) return false;
        GrabActivated = !GrabActivated;
        ActivateIndicator( GrabActivated, ref GrabPlagueIndicator, ref GrabPosition, un );
        return true;
    }

    public void ActivateIndicator( bool active, ref tk2dSprite spr, ref Vector2 ind, Unit un, bool dead = false )
    {
        if( dead && spr.gameObject.activeSelf )
        if( un == null || Util.EqualPos( ind, un.Pos ) == false ) return;
        spr.gameObject.SetActive( active );
        if( active )
        {
            spr.transform.parent = un.Graphic.transform;
            spr.transform.localPosition = new Vector3( 0, 0, -3 );
            ind = un.Pos;
            MasterAudio.PlaySound3DAtVector3( "Click 1", G.Hero.Pos );
        }
        else
        {
            spr.transform.parent = PlagueIndicatorsFolder.transform;
            ind = new Vector2( -1, -1 );
        }
    }

    public int GetRandomMonsterType()
    {
        int type = ( int ) ItemType.Plague_Monster_Cloner;
        int rnd = Random.Range( 0, 7 );
        if( rnd == 1 ) type = ( int ) ItemType.Plague_Monster_Spawner;
        if( rnd == 2 ) type = ( int ) ItemType.Plague_Monster_Slayer;
        if( rnd == 3 ) type = ( int ) ItemType.Plague_Monster_Kickable;
        if( rnd == 4 ) type = ( int ) ItemType.Plague_Monster_Swap;
        if( rnd == 5 ) type = ( int ) ItemType.Plague_Monster_Blocker;
        if( rnd == 6 ) type = ( int ) ItemType.Plague_Monster_Grabber;
        if( NextMonsterType != ItemType.NONE ) type = ( int ) NextMonsterType;
        return type;
    }

    public void UpdateFeatherCreation()
    {
        if( Manager.I.GameType != EGameType.FARM ) return;
        Item it = G.GIT( ItemType.Feather );
        for( int i = 0; i < FeathersPlaced; i++ )
        {
            AddMonster( it );
        }
        it.ProductionCount = 0;
    }

    public void UpdateFeatherPicking( Vector2 from, Vector2 to )
    {
        if( Manager.I.GameType != EGameType.FARM ) return;
        Unit un = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, to );
        if( un == null ) return;
        if( un.Variation != ( int ) ItemType.Feather ) return;
        if( Map.I.CheckArrowBlockFromTo( from, to, G.Hero ) == true ) return;
        EDirection dr = Util.GetTargetUnitDir( from, to );
        if( Util.IsDiagonal( dr ) ) return;
        Item it = G.GIT( ItemType.Feather );
        ConsecutiveFeatherCollected++;
        Item.IgnoreMessage = false;
        Item.AddItem( ItemType.Feather, +1 );
        un.Kill();
        it.ProductionCount += 600;                                                              // Next feather session time decrease
        MasterAudio.PlaySound3DAtVector3( "Click 1", G.Hero.Pos );
    }
    public void UpdateFarmObjects()
    {
        ProcessedChicken = new List<Unit>();
        for( int tid = 0; tid < G.Farm.Tl.Count; tid++ )                                        // Loop Through all pos to add possible targets
        {
            int x = G.Farm.Tl[ tid ].x;
            int y = G.Farm.Tl[ tid ].y;
            Unit un = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, new Vector2( x, y ) );
            if( un )
            {
                UpdatePlagueMonster( un );
            }

            if( Map.I.AdvanceTurn )
            {
                un = Map.I.GetUnit( ETileType.ITEM, new Vector2( x, y ) );
                if( un )
                    UpdateEggs( un );                                                           // Update Eggs Counter decrease
            }
        }
    }
    private void UpdateEggs( Unit un )
    {            
        if( un.Variation == ( int ) ItemType.Egg )
        {
            if( un.Pos != G.Hero.GetFront() )
            {
                un.Body.StackAmount--;
                un.UpdateText();
                if( un.Body.StackAmount <= 0 )                                              // counter decrease
                    Map.Kill( un, true );
            }
        }
    }

    public void UpdatePlagueMonster( Unit un )
    {
        UpdateChicken( un );                                                               // Update Chicken

        un.Body.Sprite2.gameObject.SetActive( true );
        int neigh = GetNeighborCount( un );
        if( neigh == 0 )
        {
            un.Spr.transform.localScale = new Vector3( .65f, .65f, .65f );                 // No neighbor for this monster
            float a = un.Body.Sprite2.color.a - Time.deltaTime;
            if( a < 0 ) a = 0;
            un.Body.Sprite2.color = new Color( 1, 1, 1, a );
        }
        else
        {
            un.Spr.transform.localScale = new Vector3( .65f, .65f, .65f );                // Grouped monster
            float a = un.Body.Sprite2.color.a + Time.deltaTime;
            if( a > 1 ) a = 1;
            un.Body.Sprite2.color = new Color( 1, 1, 1, a );
            if( Map.I.GetMud( un.Pos ) )
                un.Body.Sprite2.color = new Color( 1, .6f, .6f, a );
        }
    }

    private void UpdateChicken( Unit un )
    {
        if( un.Variation != ( int ) ItemType.Chicken ) return;

        if( un.Body.StackAmount > 0 )                                                        
            un.RightText.text = "+" + ( un.Body.StackAmount );
        else un.RightText.text = "Gone!";
        un.RightText.gameObject.SetActive( true );                                                   // Chicken Timer Text mesh

        if( Map.I.AdvanceTurn == false ) return;
        if( Map.I.TimeKillList.Contains( un ) == true ) return;

        if( Util.Manhattan( un.Pos, G.Hero.Pos ) <= 1 )
        {
            List<Vector2> poslist = GetPossibleTargets();                                            // Get possible targets for egg
            if( poslist.Count < 1 ) return;                                                          // Creates the Monster
            int id = Random.Range( 0, poslist.Count );
            Vector2 pt = poslist[ id ];
            Map.I.Farm.PlaceItem( pt, 12, false, false, ItemType.Egg );                              // Place eggs on the ground
            Controller.CreateMagicEffect( pt );                                                      // Magic FX  
            Controller.CreateMagicEffect( un.Pos );                                                  // Magic FX  
            MasterAudio.PlaySound3DAtVector3( "Item Collect", un.Pos );                              // FX
            Map.TimeKill( un, 5 );
            return;
        }
                    
        if( ProcessedChicken.Contains( un ) == false )
        if( Util.Manhattan( G.Hero.Pos, un.Pos ) <= 3 )
        {
            //List<Vector2> dl = new List<Vector2> { new VI( 0, 1 ), new VI( 0, -1 ), 
            //new VI( 1, 0 ), new VI( -1, 0 ), new VI( 0, 0 ) };                                     // ortho
            List<Vector2> dl = new List<Vector2>
            {  new VI( 0, 1 ), new VI( 0, -1 ), new VI( 1, 0 ), new VI(-1, 0  ),    
               new VI( 1, 1 ), new VI( 1, -1 ), new VI(-1, 1 ), new VI(-1, -1 ), new VI( 0, 0 ) };   // diag

            int bestid = -1;
            float bestscore = -1;
            for( int i = 0; i < dl.Count; i++ )
            {
                Vector2 tg = un.Pos + dl[ i ];
                if( Map.PtOnMap( Map.I.Tilemap, tg ) )
                if( Map.I.IsTileOnlyGrass( tg, true ) || tg == un.Pos )                               // compare best distances
                if( tg != G.Hero.Pos )
                {
                    float dist = Vector2.Distance( tg, G.Hero.Pos );
                    if( dist >= bestscore )
                    {
                        bestscore = dist;                                                              // best found
                        bestid = i;
                    }
                }
            }

            if( bestid == -1 ) return;
            un.Control.ApplyMove( un.Pos, un.Pos + dl[ bestid ] );                                     // move chicken

            if( --un.Body.StackAmount <= 0 ) 
                Map.TimeKill( un, 1 );                                                                 // not enough movements, kill chicken

            MasterAudio.PlaySound3DAtVector3( "Chicken", un.Pos );                                     // FX
            ProcessedChicken.Add( un );
            return;
        }
    }
    public int GetNeighborCount( Unit un )
    {
        int count = 0;
        for( int i = 0; i < 8; i++ )
        {
            Vector2 tgg = un.Pos + ( Manager.I.U.DirCord[ i ] );                                       // returns the number of neighbors of the same type
            Unit unn = Map.I.GetUnit( ETileType.PLAGUE_MONSTER, tgg );
            if( unn && unn.Variation == ( int ) un.Variation )
            if( Item.IsPlagueMonster( un.Variation, false ) ) count++;
        }
        return count;
    }
}