using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using PathologicalGames;

public enum EHerbColor
{
    NONE = -1, RED, GREEN, BLUE, YELLOW, BLACK, WHITE, GRAY = 20
}

public enum EHerbType
{
    NONE = -1, NORMAL, DIAGONAL, STATIC, SHUFFLE_BONUS, GRAY_HERB
}

public partial class Map : MonoBehaviour
{    
    public void UpdateHerbs()
    {
        if( Map.I.HerbMatchList != null )
        for( int i = Map.I.HerbMatchList.Count - 1; i >= 0; i-- )
        {
            Unit hb = HerbMatchList[ i ];

            if( hb.Body.HerbKillTimer != -1 )                                                  // Animation timer increase
                hb.Body.HerbKillTimer += Time.deltaTime;

            if( hb.Body.HerbKillTimer > .7f )                                                  // Deactivate herbs after move and shake animation
            {
                iTween.Stop( hb.Spr.gameObject );
                if( hb.Body.HerbType == EHerbType.GRAY_HERB )                                  // Gray herb remains
                {
                    Map.I.InitHerbGraphics( hb );                    
                }
                else                                                                           // Other herbs are deactivated
                {
                    hb.Activate( false );
                }
                hb.Body.HerbKillTimer = -1;
                Map.I.HerbMatchList.RemoveAt( i );
            }
        }
    }

    public void CreateHerb( Unit un )
    {
        un.Body.HerbColor = ( EHerbColor ) Random.Range( ( int ) EHerbColor.RED, ( int ) EHerbColor.BLUE + 1 );
        un.Spr.transform.localPosition = new Vector3( 0, 0, un.Spr.transform.localPosition.z );
        un.Activate( true );      
        InitHerbGraphics( un );
    }

    public void InitHerbGraphics( Unit un )
    {
        if( un.TileID != ETileType.HERB ) return;
        un.Graphic.gameObject.SetActive( true );
        un.gameObject.SetActive( true );
        un.Body.Sprite2.gameObject.SetActive( false );
        un.Spr.transform.localScale = new Vector3( 1, 1, 1 );
        un.RightText.gameObject.SetActive( false );

        switch( un.Body.HerbColor )                                                                     // Herb Color specific choices
        {
            case EHerbColor.RED:
            un.Spr.spriteId = 544;
            break;
            case EHerbColor.GREEN:
            un.Spr.spriteId = 545;
            break;
            case EHerbColor.BLUE:
            un.Spr.spriteId = 546;
            break;
            case EHerbColor.YELLOW:
            un.Spr.spriteId = 547;
            break;
            case EHerbColor.BLACK:
            un.Spr.spriteId = 548;
            break;
            case EHerbColor.WHITE:
            un.Spr.spriteId = 549;
            break;
            case EHerbColor.GRAY:
            un.Spr.spriteId = 580;
            break;
        }

        switch( un.Body.HerbType )                                                                     // Herb type specific choices
        {
            case EHerbType.STATIC:
            un.Body.Sprite2.spriteId = 576;
            un.Body.Sprite2.gameObject.SetActive( true );
            break;
            case EHerbType.DIAGONAL:
            un.Body.Sprite2.spriteId = 577;
            un.Body.Sprite2.gameObject.SetActive( true );
            break;
            case EHerbType.SHUFFLE_BONUS:
            un.Spr.spriteId = 578;
            break;
            case EHerbType.GRAY_HERB:
            un.Spr.spriteId = 579;
            break;
        }
    }
    public void UpdateHerbBump( Vector2 from, Vector2 to )
    {
        if( CheckHerbBonusStep( from, to ) ) return;                                                    // Herb Bonus

        Unit herb = GetHerb( to );
        EDirection dr = Util.GetTargetUnitDir( from, to );                                              // Frontal or bumped herb choice
        if( dr == EDirection.NONE ) return;
        if( IsWall( to ) == true ) return;
        if( CheckArrowBlockFromTo( from, to, G.Hero ) == true ) return;

        if( herb == null || herb.Body.HerbType == EHerbType.GRAY_HERB )
        {
            to = to + Manager.I.U.DirCord[ ( int ) dr ];
            herb = GetHerb( to );
            if( herb == null ) return;                                                                  // No herb, return            
        }
        else
        {
            if( herb.Body.HerbType == EHerbType.DIAGONAL )                                              // Moves Herb ability to another herb
            {
                Vector2 tgg = to + Manager.I.U.DirCord[ ( int ) dr ];
                Unit herbtgg = GetHerb( tgg );
                if( herbtgg != null && herbtgg.Body.HerbType == EHerbType.NORMAL )
                {                                                                                       // Destination herb found
                    herbtgg.Body.HerbType = herb.Body.HerbType;
                    herb.Body.HerbType = EHerbType.NORMAL;
                    InitHerbGraphics( herbtgg );
                    InitHerbGraphics( herb );
                    return;
                }
            }
        }
        if( IsHerbBonus( herb.Body.HerbType ) ) return;
        if( herb.Body.HerbType == EHerbType.STATIC ) return;                                            // Static Herb
        if( herb.Body.HerbType == EHerbType.GRAY_HERB ) return;                                         // Gray Herb

        if( herb.Body.HerbType != EHerbType.DIAGONAL )                                                  // Not Diagonal
        if( Util.IsDiagonal( dr ) ) return;
        if( herb.Body.HerbKillTimer != -1 ) return;

        Vector2 tg = new Vector2( -1, -1 );
        for( int d = 1; d < Sector.TSX; d++ )                                                           // Find kicked her target
        {
            Vector2 aux = to + ( Manager.I.U.DirCord[ ( int ) dr ] * d );
            Vector2 fr  = to + ( Manager.I.U.DirCord[ ( int ) dr ] * ( d - 1 ) );
            if( Map.PtOnMap( Map.I.Tilemap, aux ) )
            {
                if( CanHerbMoveTo( fr, aux, d ) == false ) break;
                tg = aux;

                MoveHerbTo( ref herb, fr, tg );
                bool res = CheckHerbMatchAtPos( tg );                                                   // Check Match

                if( res ) break;
                Unit mud = GetMud( aux );
                Unit raft = Controller.GetRaft( aux );

                if( mud == null && raft == null )                                                      // Herb kicked over mud tile
                {
                    if( HerbMatchList == null ) HerbMatchList = new List<Unit>();
                    HerbMatchList.Add( herb );
                    herb.Body.HerbType = EHerbType.GRAY_HERB;
                    break;
                }                    
            }
        }
        if( tg.x == -1 ) return;
        herb.Spr.transform.position = new Vector3( to.x, to.y, herb.Spr.transform.position.z );        // Keep sprite at kick origin for animation purposes                                              
    }

    public void MoveHerbTo( ref Unit herb, Vector2 from, Vector2 to )
    {
        return;
        herb.Pos = to;                                                                                  // Move herb to position
        herb.transform.position = to;        
        herb.Control.FlyingTarget = to;
        MasterAudio.PlaySound3DAtVector3( "Herb Move", G.Hero.Pos );                                    // Sound Fx
        if( Map.I.FUnit[ ( int ) to.x, ( int ) to.y ] == null )
            Map.I.FUnit[ ( int ) to.x, ( int ) to.y ] = new List<Unit>();
        Map.I.FUnit[ ( int ) to.x, ( int ) to.y ].Add( herb );
        Map.I.FUnit[ ( int ) from.x, ( int ) from.y ].Remove( herb );
    }

    public bool CheckHerbBonusStep( Vector2 from, Vector2 to )
    {
        Unit herb = GetHerb( to );
        if( herb == null ) return false;
        if( IsHerbBonus( herb.Body.HerbType ) == false ) 
            return false;
        if( herb.Body.HerbType == EHerbType.GRAY_HERB ) return false;

        EDirection dr = Util.GetTargetUnitDir( from, to );
        if( dr == EDirection.NONE ) return false;

        Vector2 target = new Vector2( -1, -1 );
        for( int d = 1; d < Sector.TSX; d++ )                                                                
        {
            Vector2 tg = G.Hero.Pos + Manager.I.U.DirCord[ ( int ) dr ] * d;
            Vector2 fr = G.Hero.Pos + Manager.I.U.DirCord[ ( int ) dr ] * ( d - 1 );
            if( IsWall( tg ) == true ) return false;
            if( d < Sector.TSX - 1 )
            if( CheckArrowBlockFromTo( fr, tg, G.Hero ) == true ) return false;                                     // Arrow Block
            if( Map.I.RM.HeroSector.Type == Sector.ESectorType.GATES ) return false;
            if( GetMud( tg ) )
            {
                target = tg;
                break;
            }
        }

        bool res = RandomizeHerbsAtMudPos( target, RM.HeroSector, false );                                          // Randomize herbs
        if( res )
        {
            herb.Body.HerbBonusAmount--;
            if( herb.Body.HerbBonusAmount <= 0 )
                herb.Activate( false );     
            UI.I.SetBigMessage( "Watering...", Color.yellow, 2f );
            MasterAudio.PlaySound3DAtVector3( "Watering", G.Hero.transform.position );
            return true;
        }
        else
        {
            UI.I.SetBigMessage( "No Herbs for repositioning...", Color.yellow, 2f );
        }
        return false;
    }

    public bool CanHerbMoveTo( Vector2 from, Vector2 to, int depth )
    {
        if( Sector.GetPosSectorType( to ) != Sector.ESectorType.NORMAL ) return false;
        Unit herb = GetHerb( to );
        if( herb != null && herb.Activated ) return false;
        if( IsWall( to ) == true ) return false;
        Unit raft = Controller.GetRaft( to );
        Unit water = GetUnit( ETileType.WATER, to );
        if( water != null && raft == null ) return false;

        if( CheckArrowBlockFromTo( from, to, G.Hero ) == true ) return false;

        if( depth > 1 )
        {
            Unit E1 = GetHerb( from + new Vector2( +1, 0 ) );
            Unit E2 = GetHerb( from + new Vector2( +2, 0 ) );
            Unit W1 = GetHerb( from + new Vector2( -1, 0 ) );
            Unit W2 = GetHerb( from + new Vector2( -2, 0 ) );
            Unit N1 = GetHerb( from + new Vector2( 0, +1 ) );
            Unit N2 = GetHerb( from + new Vector2( 0, +2 ) );
            Unit S1 = GetHerb( from + new Vector2( 0, -1 ) );
            Unit S2 = GetHerb( from + new Vector2( 0, -2 ) );

            if( E1 && W1 ) return false;              // stops Glued to neighbos N and South Herbs
            if( N1 && S1 ) return false;
            if( E1 && E2 ) return false;
            if( W1 && W2 ) return false;
            if( N1 && N2 ) return false;
            if( S1 && S2 ) return false;
        }
        return true;
    }

    public bool IsHerbBonus( EHerbType type )
    {
        if( type == EHerbType.SHUFFLE_BONUS) return true;                                                  // Is herb a bonus type?
        if( type == EHerbType.GRAY_HERB ) return true;        
        return false;
    }

    public bool CheckHerbMatchAtPos( Vector2 tg )
    {
        Sector s = RM.HeroSector;
        for( int yy = ( int ) s.Area.yMin - 1; yy < s.Area.yMax + 1; yy++ )                                // Set all cube herbs as not checked
        for( int xx = ( int ) s.Area.xMin - 1; xx < s.Area.xMax + 1; xx++ )
        if ( Map.PtOnMap( Tilemap, new Vector2( xx, yy ) ) )
        {
            Unit hr = GetHerb( new Vector2( xx, yy ) );
            if( hr != null ) hr.Body.HerbMatchChecked = 0;            
        }

        Unit herb = GetHerb( tg );
        MatchHerbType = herb.Body.HerbColor;

        List<Unit> hl = new List<Unit>();
        CheckHerbMatch( tg, 1, ref hl );                                                                   // Check match recursivelly

        if( hl.Count < 3 ) return false;                                                                   // Less than 3, quit

        if( HerbMatchList == null )
            HerbMatchList = new List<Unit>();

        HerbMatchList.AddRange( hl );
        ItemType it = ItemType.NONE;                                                                       // Chooses Resource Prize
        if( MatchHerbType == EHerbColor.RED    ) it = ItemType.Res_Red_Herb;
        if( MatchHerbType == EHerbColor.GREEN  ) it = ItemType.Res_Green_Herb;
        if( MatchHerbType == EHerbColor.BLUE   ) it = ItemType.Res_Blue_Herb;
        if( MatchHerbType == EHerbColor.YELLOW ) it = ItemType.Res_Yellow_Herb;
        if( MatchHerbType == EHerbColor.BLACK  ) it = ItemType.Res_Black_Herb;
        if( MatchHerbType == EHerbColor.WHITE  ) it = ItemType.Res_White_Herb;

        int amt = hl.Count - 2;
        Item.AddItem( it, amt );                                                                            // Gives Resource Prize     
        return true;
    }

    public bool CheckHerbMatch( Vector2 pos, int id, ref List<Unit> hl )
    {
        if( PtOnMap( Tilemap, pos ) == false ) return false;
        Unit herb = GetHerb( pos );
        if( herb == null ) return false;
        if( herb.Body.HerbMatchChecked > 0 ) return false;
        if( herb.Body.HerbColor != MatchHerbType ) return false;
        if( herb.Body.HerbType != EHerbType.NORMAL &&
            herb.Body.HerbType != EHerbType.DIAGONAL &&
            herb.Body.HerbType != EHerbType.STATIC )            
            return false;
        if( herb.Body.HerbColor == EHerbColor.GRAY ) return false;
        hl.Add( herb );
        herb.Body.HerbMatchChecked = id;
        CheckHerbMatch( new Vector2( pos.x - 1, pos.y ), ++id, ref hl );                                    // Check match in all ortho directions
        CheckHerbMatch( new Vector2( pos.x + 1, pos.y ), ++id, ref hl );
        CheckHerbMatch( new Vector2( pos.x, pos.y - 1 ), ++id, ref hl );
        CheckHerbMatch( new Vector2( pos.x, pos.y + 1 ), ++id, ref hl );
        return true;
    }

    public bool CheckMudPool( Vector2 pos, int id, ref List<Vector2> hl )
    {
        if( PtOnMap( Tilemap, pos ) == false ) return false;
        Unit mud = GetMud( pos );
        if( mud == null ) return false;
        if( MudPoolID[ ( int ) pos.x, ( int ) pos.y ] > 0 ) return false;                                  // Checks mud pool
        MudPoolID[ ( int ) pos.x, ( int ) pos.y ] = id;
        hl.Add( pos );
        CheckMudPool( new Vector2( pos.x - 1, pos.y ), id, ref hl );
        CheckMudPool( new Vector2( pos.x + 1, pos.y ), id, ref hl );
        CheckMudPool( new Vector2( pos.x, pos.y - 1 ), id, ref hl );
        CheckMudPool( new Vector2( pos.x, pos.y + 1 ), id, ref hl );
        return true;
    }

    public bool RandomizeHerbsAtMudPos( Vector2 tg, Sector s, bool init )
    {
        Unit mud = GetMud( tg );
        if( mud == null ) return false;
        MudPoolID = new int[ Map.I.Tilemap.width, Map.I.Tilemap.height ];
        List<Vector2> mpl = new List<Vector2>();
        List<Unit> hbl = new List<Unit>();

        CheckMudPool( tg, 1, ref mpl );                                                                    // Delimitate mud pool

        for( int u = s.Fly.Count - 1; u >= 0; u-- )
        if ( s.Fly[ u ].TileID == ETileType.HERB )                                                 // loops through all herbs to see if it belongs to the mud pool
        {
            bool add = true;
            Unit herb = s.Fly[ u ];
            if( herb.Activated ) add = false;
            if( init == true ) add = true;
            if( IsHerbBonus( herb.Body.HerbType ) == true ) add = false;
            if( herb.Body.HerbType == EHerbType.GRAY_HERB ) add = true;
            if( mpl.Contains( herb.IniPos ) == false ) add = false;
            if( herb.Body.RandomizableHerb == false) add = false;

            if( add )
            {
                hbl.Add( herb );                                                                           // Add herb
                herb.Activate( false );
            }

            //if( herb.Activated )
            if( IsHerbBonus( herb.Body.HerbType ) )                                                        // dont move over static bonus
                mpl.Remove( herb.Pos );
        }

        if( mpl.Contains( G.Hero.Pos ) ) mpl.Remove( G.Hero.Pos );                                         // Remove hero and busy targets
        for( int i = mpl.Count - 1; i >= 0; i-- )
        {
            if( GetHerb( mpl[ i ] ) != null ) mpl.RemoveAt( i );
        }

        if( hbl.Count <= 0 ) return false;
        for( int i = hbl.Count - 1; i >= 0; i-- )
        {
            Unit herb = hbl[ i ];
            Map.I.FUnit[ ( int ) herb.Pos.x, ( int ) herb.Pos.y ].Remove( herb );
            int id = Random.Range( 0, mpl.Count );                                                         // Sorts new position
            Vector2 ntg = mpl[ id ];
            herb.Pos = ntg;
            herb.transform.position = ntg;                                                                 // move herb to position
            herb.Control.FlyingTarget = new Vector2( -1, -1 );
            herb.Spr.transform.localPosition = new Vector3( 0, 0, herb.Spr.transform.position.z );
            herb.Body.HerbType = herb.Body.OriginalHerbType;
            herb.Activate( true );
            if( Map.I.FUnit[ ( int ) ntg.x, ( int ) ntg.y ] == null )
                Map.I.FUnit[ ( int ) ntg.x, ( int ) ntg.y ] = new List<Unit>();                            // FUnit changing
            Map.I.FUnit[ ( int ) ntg.x, ( int ) ntg.y ].Add( herb );
            if( shuffleList != null ) shuffleList.Add( herb );
            mpl.RemoveAt( id );
        }
        MudPoolID = null;
        return true;
    }

    public void ShuffleAllHerbs( Sector s )
    {
        shuffleList = new List<Unit>();
        for( int yy = ( int ) s.Area.yMin - 1; yy < s.Area.yMax + 1; yy++ )                               // loops through all herbs to shuffle their pos
        for( int xx = ( int ) s.Area.xMin - 1; xx < s.Area.xMax + 1; xx++ )
        if ( Map.PtOnMap( Map.I.Tilemap, new Vector2( xx, yy ) ) )
        {
            Unit herb = GetHerb( new Vector2( xx, yy ) );
            if( herb != null )
            if( shuffleList.Contains( herb ) == false )
                {
                    RandomizeHerbsAtMudPos( new Vector2( xx, yy ), s, true );
                }
        }
        shuffleList = null;
    }
    public Unit GetHerb( Vector2 tg, bool checkActivated = true )
    {
        if( PtOnMap( Tilemap, tg ) == false ) return null;
        if ( FUnit[ ( int ) tg.x, ( int ) tg.y ] != null )
        for( int u = 0; u < FUnit[ ( int ) tg.x, ( int ) tg.y ].Count; u++ )            
        if ( FUnit[ ( int ) tg.x, ( int ) tg.y ][ u ].TileID == ETileType.HERB )
        if ( checkActivated == false || 
            FUnit[ ( int ) tg.x, ( int ) tg.y ][ u ].Activated )
            return FUnit[ ( int ) tg.x, ( int ) tg.y ][ u ];            
        return null;
    }        
}

