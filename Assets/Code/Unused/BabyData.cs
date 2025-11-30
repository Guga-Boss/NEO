using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;

public enum EAlgaeBabyType
{
    NONE = -1, NETTLE, ROCK, EGG, FLOWER, ARROW, MOVABLE,
    EXPLODING, PROTECTOR, POISON
}

public class BabyData : MonoBehaviour
{
    #region Variables
    public Unit Algae;
    public int ID = 0;
    public EAlgaeBabyType BabyType = EAlgaeBabyType.NETTLE;
    public tk2dSprite Sprite;
    public CircleCollider2D Collider;
    public Rigidbody2D RigidBody;
    public Vector2 Cord;
    public Vector3 OldPos;
    public Vector2 OriginalPos; // this is the prefab original position, do not reset it anywhere but the prefab
    public float YPos;
    public float BabyTimeCounter;
    public int TimesDestroyed = 0;
    public float DeathCountDownTime = 0;
    public float BabyDisabledTimeCount = 0;
    public float Lifetime = 0;
    public float Hp = 100;
    public int HitCount = 0;
    public float HitTimeCount = 0;
    public EDirection ArrowDir;
    public static EDirection OverArrowDir;
    public static bool CheckWaterArrow = false;
    public float shift = 0;
    public int ConsecutiveFlowerHitCount = 0;
    public static float def = .5f;
    #endregion
    public static void CreateAlgae( Unit un )
    {
        un.transform.position = new Vector3( un.transform.position.x, un.transform.position.y, -2 );
        un.Control.NotAnimatedPosition = un.Graphic.transform.position;
        for( int i = 0; i < un.Body.BabySprite.Count; i++ )
        {
            BabyData bd = un.Body.BabyDataList[ i ];            
            BabyData.CreateAlgaeBrick( bd, true );
            bd.Algae = un;
        }
    }
    public static void CreateAlgaeBrick( BabyData bd, bool init )
    {
        bd.YPos = bd.transform.position.y;
        bd.BabyTimeCounter = 0;
        bd.Collider.enabled = true;
        bd.OldPos = Vector3.zero;
        bd.transform.localPosition = new Vector3( bd.OriginalPos.x, bd.OriginalPos.y, 1.5f );
        bd.gameObject.SetActive( true );
        bd.transform.localScale = new Vector3( 1, 1, 1 );
        bd.Sprite.transform.eulerAngles = new Vector3( 0, 0, Random.Range( 0f, 360f ) );
        bd.Lifetime = 0;
        bd.BabyDisabledTimeCount = 100;
        bd.Hp = 100;
        bd.shift = Random.Range( 0f, 1f );
        bd.HitCount = 0;
        bd.ArrowDir = ( EDirection ) Util.GetRandomDir();
        bd.HitTimeCount = 100;
        bd.DeathCountDownTime = 0;
        bd.RigidBody.isKinematic = true;
        bd.RigidBody.freezeRotation = false;
        bd.RigidBody.mass = 30;
        bd.ConsecutiveFlowerHitCount = 0;
        bd.RigidBody.velocity = Vector2.zero;
        if( init )
            bd.TimesDestroyed = 0;
    }
    public static void Save( Unit un )
    {
        for( int i = 0; i < un.Body.BabySprite.Count; i++ )
        {
            BabyData bd = un.Body.BabyDataList[ i ];
            GS.W.Write( bd.gameObject.activeSelf );
            if( bd.gameObject.activeSelf )
            {
                GS.W.Write( ( int ) bd.BabyType );
                GS.W.Write( bd.Hp );
                GS.W.Write( bd.BabyTimeCounter );
                GS.W.Write( bd.BabyDisabledTimeCount );
                GS.W.Write( bd.Lifetime );
                GS.W.Write( bd.HitCount );
                GS.W.Write( bd.HitTimeCount );
                GS.W.Write( bd.DeathCountDownTime );
                GS.W.Write( bd.ConsecutiveFlowerHitCount );
                GS.W.Write( bd.TimesDestroyed );
                GS.W.Write( bd.RigidBody.isKinematic );
                GS.W.Write( bd.Collider.enabled );
                GS.W.Write( bd.Collider.isTrigger);
                GS.SVector3( bd.OldPos );
                GS.SVector3( bd.transform.localPosition );
                GS.SVector3( bd.transform.localScale );
                GS.SVector3( bd.Sprite.transform.eulerAngles );
                GS.SVector2( bd.RigidBody.velocity );
            }
        }
    }
    public static void Load( Unit un )
    {
        for( int i = 0; i < un.Body.BabySprite.Count; i++ )
        {
            BabyData bd = un.Body.BabyDataList[ i ];
            bd.gameObject.SetActive( GS.R.ReadBoolean() );
            if( bd.gameObject.activeSelf)
            {
                bd.BabyType = ( EAlgaeBabyType ) GS.R.ReadInt32();
                bd.Hp = GS.R.ReadSingle();
                bd.BabyTimeCounter = GS.R.ReadSingle();
                bd.BabyDisabledTimeCount = GS.R.ReadSingle();
                bd.Lifetime = GS.R.ReadSingle();
                bd.HitCount = GS.R.ReadInt32();
                bd.HitTimeCount = GS.R.ReadSingle();
                bd.DeathCountDownTime = GS.R.ReadSingle();
                bd.ConsecutiveFlowerHitCount = GS.R.ReadInt32();
                bd.TimesDestroyed = GS.R.ReadInt32();
                bd.RigidBody.isKinematic = GS.R.ReadBoolean();
                bd.Collider.enabled = GS.R.ReadBoolean();
                bd.Collider.isTrigger = GS.R.ReadBoolean();
                bd.OldPos = GS.LVector3();
                bd.transform.localPosition = GS.LVector3();
                bd.transform.localScale = GS.LVector3();
                bd.Sprite.transform.eulerAngles = GS.LVector3();
                bd.RigidBody.velocity = GS.LVector2();
            }
        }
    }
    public static void UpdateAlgae( Unit un )
    {        
        int cont = 0, melt = 0;                
        un.transform.position = new Vector3( un.transform.position.x, un.transform.position.y, -2 );
        un.Control.NotAnimatedPosition = un.Graphic.transform.position;
        CheckWaterArrow = false;

        bool land = true;
        if( Map.I.GetUnit( ETileType.WATER, un.Pos ) ) land = false;

        bool pond = false;
        if( Map.I.ContinuousPondID != null ) 
        if( Map.I.ContinuousPondID[ ( int ) un.Pos.x, ( int ) un.Pos.y ] ==                     // Algae in the same pond
            Map.I.CurrentContinuousPondId ) pond = true;

        if( land == false )
        for( int i = 0; i < un.Body.BabyDataList.Count; i++ )                                   // Buoying animation
            un.Body.BabyDataList[ i ].UpdateAnimation();

        if( Map.I.RM.HeroSector.CubeFrameCount <= 10 ) pond = true;                             // to initialize sprites
        if( GS.LoadFrameCount <= 1 ) pond = true;                                               // after loading too

        if( land )                                                                              // while sliding
        if( Controller.SnowSliding ||
            Controller.SandSliding ) pond = true;

        if( pond )
        for( int i = 0; i < un.Body.BabyDataList.Count; i++ )
        {
            BabyData dt = un.Body.BabyDataList[ i ];
            if( dt.gameObject.activeSelf ) cont++;
            if( dt.BabyType == EAlgaeBabyType.EGG ) melt++;

            dt.Collider.isTrigger = false;                                                      // Land algae type
            float scale = -1;
            float sprscale = def;
            float perc = 100;

            if( Map.I.FishingMode == EFishingPhase.FISHING )                                    // Baby disable timer
            if( un.Md && un.Md.BabyDisableTotTime > 0 )
            {
                dt.BabyDisabledTimeCount += Time.deltaTime;
                if( dt.BabyDisabledTimeCount > un.Md.BabyDisableTotTime )
                {
                    dt.BabyDisabledTimeCount = 0;
                    dt.Collider.enabled = true;
                    if( Util.Chance( un.Md.BabyDisableChance ) )
                    {
                        Debug.Log( un.Md.BabyDisableChance );
                        dt.Collider.enabled = false;
                    }
                }
            }

            if( Map.I.FishingMode == EFishingPhase.NO_FISHING )                                  // Disabled sprite update
                dt.Collider.enabled = true;
            if( dt.Collider.enabled ) dt.Sprite.color = new Color( 1, 1, 1, 1 );
            else dt.Sprite.color = new Color( 1, 1, 1, .4f );

            switch( dt.BabyType )
            {
                case EAlgaeBabyType.FLOWER:                                                      // water flower
                if( Map.I.FishingMode == EFishingPhase.FISHING )
                {
                    dt.RigidBody.freezeRotation = true;

                    float flowerbn = Item.GetNum( ItemType.Res_Water_Flower ) * 
                    un.Md.WaterFlowerBonusPerUnit;
                    dt.RigidBody.isKinematic = false;

                    float consbn = dt.ConsecutiveFlowerHitCount * 
                    un.Md.WaterFlowerConsecutiveHitBonus;                                       // Consecutive Flower hit bonus
                    float totbn = flowerbn + consbn;                                            // total bonus

                    float damage = dt.HitCount * Time.deltaTime * un.Md.WaterFlowerBaseAttack;
                    damage += Util.Percent( totbn, damage );
                    dt.Hp -= damage;                                                            // hp damage

                    float rot = dt.Sprite.transform.eulerAngles.z;
                    float rotspd = Util.Percent( 100 + totbn, 15 );                             // rotation speed

                    rot -= dt.HitCount * Time.deltaTime * rotspd;
                    if( rot < 0 ) rot += 360;
                    dt.Sprite.transform.eulerAngles = new Vector3( 0, 0, rot );                 // flower rotation animation
                    dt.Sprite.color = new Color( 1, 1, 1 );
                    dt.HitTimeCount += Time.deltaTime;
                    if( dt.Hp <= 0 )                                                            // flower dies
                    if( dt.gameObject.activeSelf )
                    {
                        if( dt.DeathCountDownTime <= 0 )
                            Item.AddItem( ItemType.Res_Water_Flower, 1 );
                        dt.DeathCountDownTime = .01f;
                    }
                }
                dt.Sprite.spriteId = 468;
                perc = Util.GetPercent( dt.Hp, 100 );
                scale = 20 + ( Util.Percent( perc, 80 ) );
                if( perc < 100 ) dt.Sprite.spriteId = 469;
                if( dt.HitTimeCount < un.Md.WaterFlowerRefreshTime )
                {
                    dt.Sprite.spriteId = 470;                                                  // hitting able or not
                    dt.Sprite.color = new Color( 1, .7f, .7f );
                }
                else
                {
                    dt.RigidBody.velocity = Vector2.zero;
                    dt.ConsecutiveFlowerHitCount = 0;
                }
                break;
                case EAlgaeBabyType.NETTLE:
                dt.Sprite.spriteId = 466;
                dt.Sprite.color = Color.white;
                break;
                case EAlgaeBabyType.ARROW:
                dt.Sprite.spriteId = 471;
                dt.Sprite.transform.eulerAngles = Util.GetRotationAngleVector( Util.GetInvDir( dt.ArrowDir ) );
                dt.Collider.isTrigger = true;
                dt.Sprite.color = Color.white;
                break;
                case EAlgaeBabyType.EXPLODING:
                dt.Sprite.spriteId = 474;
                dt.Collider.isTrigger = false;
                dt.Sprite.color = Color.white;
                dt.Sprite.transform.eulerAngles = new Vector3( 0, 0, 0 );
                dt.RigidBody.isKinematic = false;
                break;
                case EAlgaeBabyType.PROTECTOR:
                dt.Sprite.spriteId = 475;
                dt.Collider.isTrigger = false;
                dt.Sprite.color = Color.white;
                dt.RigidBody.mass = 15;
                if( Map.I.RM.HeroSector.CubeFrameCount <= 10 )
                    dt.RigidBody.isKinematic = true;
                dt.Sprite.transform.eulerAngles = new Vector3( 0, 0, 0 );
                if( Map.I.IsHeroMeleeAvailable() )
                if( Vector2.Distance( Map.I.HeroSwordCollider.offset, dt.Collider.offset ) <= 1.5f &&
                    Util.IsBoxOverlappingCircle( Map.I.HeroSwordCollider, dt.Collider ) )
                    {
                        G.Hero.Body.InvulnerabilityFactor = .3f;                                                                    // activate protection shield
                        MasterAudio.PlaySound3DAtTransform( "Electric Orb", G.Hero.transform, .3f );
                        sprscale = Random.Range( 0.55f, 0.7f );
                        dt.RigidBody.velocity = Vector2.zero;
                    }
     
                break;
                case EAlgaeBabyType.POISON:
                dt.Sprite.spriteId = 476;
                dt.Collider.isTrigger = false;
                dt.Sprite.color = Color.white;
                dt.Sprite.transform.eulerAngles = new Vector3( 0, 0, 0 );
                dt.RigidBody.isKinematic = false;
                sprscale = def;
                float dist = Vector2.Distance( Map.I.HeroSwordCollider.transform.position, dt.transform.position );
                float fact = 3.5f - dist;
                if( dist < 3.5 )
                    sprscale = Random.Range( def, def + ( .08f * fact ) );
                break;
                case EAlgaeBabyType.MOVABLE:
                dt.RigidBody.isKinematic = false;
                if( land )
                {
                    int posx = 0, posy = 0;
                    Map.I.Tilemap.GetTileAtPosition( dt.transform.position, out posx, out posy );
                    Unit snow = Map.I.GetUnit( ETileType.SNOW, new Vector2( posx, posy ) );
                    if( snow ) 
                        dt.RigidBody.mass = 5;
                    else
                        dt.RigidBody.mass = 15;
                }
                dt.Sprite.spriteId = 472;
                dt.Sprite.color = Color.white;
                dist = Vector3.Distance( dt.transform.position, dt.OldPos );
                if( dt.OldPos != Vector3.zero )
                {
                    if( Map.I.CurrentFishingPole && Map.I.CurrentFishingPole.Water.PoleBonusCnType == 
                        EPoleBonusCnType.PULL_ALGAE_X_METERS )
                    {
                        Water.TempAlgaePullDistance += dist;
                        Water.CheckCn( EPoleBonusCnType.PULL_ALGAE_X_METERS, null );                                           // pull algae pole condition check
                    }
                }
                dt.OldPos = dt.transform.position;
                break;
                case EAlgaeBabyType.ROCK:
                float a = Random.RandomRange( .5f, .99f );
                dt.Sprite.color = new Color( 1, 1, 1, a );
                dt.Sprite.transform.eulerAngles = new Vector3( 0, 0, 0 );
                dt.Sprite.spriteId = 473;
                break;
            }

            float sperc = 100;
            if( dt.DeathCountDownTime > 0 )
            {
                sperc = 10 + Util.GetPercent( dt.DeathCountDownTime, 3 );                  // Death shrink Fx
                dt.DeathCountDownTime -= Time.deltaTime;
                dt.Collider.enabled = false;
                float f = .5f;
                dt.Sprite.color = new Color( 1, f, f, sperc / 100 );
                if( dt.DeathCountDownTime <= 0 )                                           // algae dies
                {
                    dt.DeathCountDownTime = 0;
                    dt.gameObject.SetActive( false );
                    Map.I.CreateExplosionFX( new Vector2(
                    dt.transform.position.x, dt.transform.position.y ) );
                    if( dt.BabyType == EAlgaeBabyType.EXPLODING )
                        Item.AddItem( ItemType.Res_Exploding_Plant, +1 );
                }
            }

            if( scale != -1 ) sperc = scale;
            dt.transform.localScale = new Vector3(
            Util.Percent( sperc, 1 ), Util.Percent( sperc, 1 ), 1 );                             // changes scale and colider size
            dt.Sprite.scale = new Vector3( sprscale, sprscale );                                 // changes only sprite scale

            int x, y;
            Map.I.Tilemap.GetTileAtPosition( dt.transform.position, out x, out y );              // Algae out of water is disabled
            Unit water = Map.I.GetUnit( ETileType.WATER, new Vector2( x, y ) );
            if( water == null && Map.IsWall( new Vector2( x, y ) ) == false )
            if( dt.DeathCountDownTime <= 0 && dt.gameObject.activeSelf )
            if( land == false )                                                                  // originally from water
                dt.DeathCountDownTime = .1f;
        }

        if( pond )
        if( cont < 1 )                                                                           // all algae destroyed
        {
            if( melt >= 1 )
                TKUtil.ClearLayer( un.Pos, ELayerType.GAIA );
            if( G.Hero.Pos == un.Pos )
                G.Hero.CanMoveFromTo( true, G.Hero.Control.OldPos, un.Pos, G.Hero );
            un.Kill();
        }
    }

    public void UpdateAnimation()
    {
        if( BabyType == EAlgaeBabyType.ROCK ) return;
        float amplitude = 0.02f;                                                            // buoying animation
        float frequency = 1.2f;
        Sprite.transform.localPosition = new Vector3( 0, Mathf.Sin( ( Time.fixedTime + shift )
        * Mathf.PI * frequency ) * amplitude, Sprite.transform.localPosition.z );
    }

    public void ProcessLandCollision( int posx, int posy )
    {     
        BabyTimeCounter = 0;
    }
    
     public void OnCollisionEnter2D( Collision2D col )
    {
        int posx = 0, posy = 0;
        Map.I.Tilemap.GetTileAtPosition( col.transform.position, out posx, out posy );
        BabyData bd = gameObject.GetComponent<BabyData>();
        if( bd )
        {
            bd.ProcessWaterCollision( posx, posy, col );
            Unit fire = Map.I.GetUnit( ETileType.FIRE, new Vector2( posx, posy ) );
            if( fire && fire.Body.FireIsOn )
            {
                bd.DeathCountDownTime = .1f;
                MasterAudio.PlaySound3DAtVector3( "Fire Ignite", Map.I.Hero.Pos );                                               // fx
            }

            //if( col.gameObject.name == "Hero" )
            {
                if( bd.BabyType == EAlgaeBabyType.PROTECTOR )
                {
                    bd.RigidBody.isKinematic = true;
                    bd.RigidBody.velocity = Vector2.zero;
                }
            }
        }
    }

     public void OnTriggerEnter2D( Collider2D col )
     {
         int posx = 0, posy = 0;
         Map.I.Tilemap.GetTileAtPosition( col.transform.position, out posx, out posy );
         BabyData data = gameObject.GetComponent<BabyData>();
         if( data == null ) return;
         if( BabyType == EAlgaeBabyType.EXPLODING )
         if( col.gameObject.name == "Hero Sword Collider" )
         if( Map.I.IsHeroMeleeAvailable() )
         {
             Explode( transform.position, 1.3f, 1.3f );                                                               // radial explosion
             Map.I.CreateExplosionFX( transform.position );
             DeathCountDownTime = .01f;
         }
         if( BabyType == EAlgaeBabyType.POISON )
         if( col.gameObject.name == "Hero Sword Collider" )
         if( Map.I.IsHeroMeleeAvailable() )
         if(  Map.I.SetHeroDeathTimer( .2f ) )
         {
             Explode( transform.position, 6, 100 );                                                                   // radial explosion
             Map.I.CreateExplosionFX( transform.position );
             Map.I.CreateLightiningEffect( null, null, "keys", data.transform.position.x, 
             data.transform.position.y, G.Hero.transform.position.x, G.Hero.transform.position.y );                  // Lighting FX  
         }
     }

    public void ProcessWaterCollision( int posx, int posy, Collision2D col  )
     {
         if( BabyType != EAlgaeBabyType.FLOWER )                                                                    // Hook bonus algae destroy
             if( G.HS.AlgaeDestroy || Water.TempAlgaeDestroy )
         {
             DeathCountDownTime = .01f;             
             if( Map.I.CurrentFishingPole.Water.PoleBonusCnType == EPoleBonusCnType.DESTROY_X_ALGAE )
             {
                 Water.CheckCn( EPoleBonusCnType.DESTROY_X_ALGAE, null );                                           // hook bonus destroy x algae
             }
         }

        if( BabyType == EAlgaeBabyType.FLOWER )
        if( HitTimeCount > Algae.Md.WaterFlowerRefreshTime )
        if( Map.I.FishingMode == EFishingPhase.FISHING )
        {
            ConsecutiveFlowerHitCount = GetConsecutiveFlowerHitCount();
            BabyTimeCounter = 0;
            HitCount++;
            HitTimeCount = 0;
            RigidBody.velocity = Vector2.zero;
            MasterAudio.PlaySound3DAtVector3( "Click 2", Map.I.MainHook.transform.position );
        }
    }
    
    public static int GetNumActiveAlgaeinTile( Vector2 pt, EAlgaeBabyType tp = EAlgaeBabyType.NONE )
    {
        int cont = 0;
        List<Unit> algae = Map.I.GF( pt, ETileType.ALGAE );
        Unit water = Map.I.GetUnit( ETileType.WATER, pt );                                        // Algae baby present
        if( water == null && algae != null && algae.Count > 0 )
        {
            for( int a = 0; a < algae.Count; a++ )
            for( int b = 0; b < algae[ a ].Body.BabyDataList.Count; b++ )
            if ( algae[ a ].Body.BabyDataList[ b ].gameObject.activeSelf )
            {
                if( tp == EAlgaeBabyType.NONE )
                    cont++;
                else
                if( algae[ a ].Body.BabyDataList[ b ].BabyType == tp )
                    cont++;
            }
        }
        return cont;
    }

    public static bool UpdateWaterArrowBlock( Vector3 pos, ref bool block, EDirection movedir, int x, int y )
    {
        int posx = 0; int posy = 0;
        Map.I.Tilemap.GetTileAtPosition( pos, out posx, out posy );
        List<Unit> un = Map.I.GF( new Vector2( posx, posy ), ETileType.ALGAE );
        if( un == null || un.Count < 1 ) return false;
        //Debug.Log( un[ 0 ] + "  " + un[ 0 ].Body.BabyDataList + "  " + un[ 0 ].Body.BabyDataList.Count );

        for( int i = 0; i < un[ 0 ].Body.BabyDataList.Count; i++ )
        {
            BabyData dt = un[ 0 ].Body.BabyDataList[ i ];
            if( dt.BabyType == EAlgaeBabyType.ARROW )
            if( dt.Collider.OverlapPoint( Map.I.MainHook.transform.position ) )   // arrow dir for blocking
                OverArrowDir = dt.ArrowDir;
        }

        if( BabyData.OverArrowDir != EDirection.NONE )
            if( movedir != EDirection.NONE )
            {
                BabyData.CheckWaterArrow = true;
                Vector2 to = new Vector2( x, y ) + Manager.I.U.DirCord[ ( int ) Util.GetInvDir( movedir) ];
                if( Map.I.CheckArrowBlockFromTo( new Vector2( x, y ), to,
                    G.Hero, true, 5, ( int ) G.Hero.Control.ArrowOutLevel ) == true )
                    block = true;
                BabyData.CheckWaterArrow = false;
                if( block )
                    return true;
            }
        return false;
    }

    public static void StartFishingInit( Unit un )
    {
        for( int i = 0; i < un.Body.BabyDataList.Count; i++ )
        {
            BabyData dt = un.Body.BabyDataList[ i ];
            dt.BabyDisabledTimeCount = 100;
        }
    }

    public static int GetConsecutiveFlowerHitCount()
    {
        int res = 0;
        Sector s = Map.I.RM.HeroSector;
        if ( s && s.Type == Sector.ESectorType.NORMAL )
        for( int y = ( int ) s.Area.yMin - 1; y < s.Area.yMax + 1; y++ )
        for( int x = ( int ) s.Area.xMin - 1; x < s.Area.xMax + 1; x++ )
        {
            if( Map.I.FUnit[ x, y ] != null )
            if( Map.I.FUnit[ x, y ].Count > 0 )
            for( int i = 0; i < Map.I.FUnit[ x, y ].Count; i++ )
            if( Map.I.FUnit[ x, y ][ i ].TileID == ETileType.ALGAE )
            {
                Unit un = Map.I.FUnit[ x, y ][ i ];
                for( int b = 0; b < un.Body.BabyDataList.Count; b++ )
                {
                    if( un.Body.BabyDataList[ b ].HitTimeCount < un.Md.WaterFlowerRefreshTime ) res++;
                }       
            }
        }
        return res;
    }
    public static void Explode( Vector2 center, float radius, float force )
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll( center, radius );
        foreach( Collider2D col in colliders )
        {
            Rigidbody2D rb = col.attachedRigidbody;
            if( rb )
            {
                BabyData bd = rb.GetComponent<BabyData>();
                if( bd )
                if( bd.BabyType == EAlgaeBabyType.PROTECTOR )
                {
                    rb.isKinematic = false;
                    rb.mass = 15;              
                }
                Vector2 direction = ( rb.position - center );
                float distance = direction.magnitude;
                float distanceFactor = 1 - Mathf.Clamp01( distance / radius );           // mais perto => mais força
                distanceFactor = 1;                                                      // remove this line to apply distance factor
                Vector2 explosionForce = direction.normalized * force * distanceFactor;
                if( bd )
                    rb.AddForce( explosionForce, ForceMode2D.Impulse );
            }
        }
    }
}
