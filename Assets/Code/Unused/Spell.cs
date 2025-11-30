using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using PathologicalGames;

public class Spell : MonoBehaviour
{
    public Attack Attack;
    public ESpiderBabyType Type = ESpiderBabyType.NONE;
    public tk2dSprite SpellSprite;
    public Transform sptr;  
    public EBoltType BoltType = EBoltType.NONE;
    public ItemType ItemType = ItemType.NONE;
    public int BambooSize = 0;
    public Unit HookedUnit = null;
    public bool Kill = false;
    public Vector2 Delta = Vector2.zero;
    public static Vector2 AddSpellOrigin = new Vector2( -1, -1 );
    public static EDirection AddSpellHeroDir = EDirection.NONE;
    public static ItemType WeaponType = ItemType.NONE;
    public static int SpellSlot = 1;
    public void Copy( Spell s )
    {
        Type = s.Type;
        BoltType = s.BoltType;
        ItemType = s.ItemType;
        BambooSize = s.BambooSize;
        HookedUnit = s.HookedUnit;
        Delta = s.Delta;
        Kill = s.Kill;
    }
    public void Reset()
    {
        Type = ESpiderBabyType.NONE;
        BambooSize = 0;
        BoltType = EBoltType.NONE;
        HookedUnit = null;
        ItemType = ItemType.NONE;
        if( SpellSprite ) SpellSprite.gameObject.SetActive( false );
        Delta = Vector2.zero;
        Kill = false;
    }
    public void Save( EUnitType type )
    {
        GS.W.Write( ( int ) Type );
        GS.W.Write( ( int ) BoltType );
        GS.W.Write( ( int ) ItemType );
        GS.W.Write( Kill );
        GS.SVector3( transform.localPosition );
        if( type == EUnitType.HERO )
        {
            GS.SVector2( Delta );
            GS.W.Write( BambooSize );
        }
        //GS.W.Write( HookedUnit );
    }

    public void Load( EUnitType type )
    {
        Type = ( ESpiderBabyType ) GS.R.ReadInt32();
        BoltType = ( EBoltType ) GS.R.ReadInt32();
        ItemType = ( ItemType ) GS.R.ReadInt32();
        Kill = GS.R.ReadBoolean();
        transform.localPosition = GS.LVector3();
        if( type == EUnitType.HERO )
        {
            Delta = GS.LVector2();
            BambooSize = GS.R.ReadInt32();
        }
        //HookedUnit = null;                                                                // this may cause a bug!!!! not loading
    }
    public static void UpdateWeaponInstalation()
    {
        if( AddSpellOrigin == new Vector2( -1, -1 ) ) return;
        if( AddSpellOrigin == G.Hero.Pos ) return;
        Spell fsp = null;
        string snd1 = "", snd2 = "";

        if( SpellSlot == 2 )                                                                 // Handles FIXED spells here
        {        
            EDirection mov = Util.GetTargetUnitDir( G.Hero.Control.OldPos, G.Hero.Pos );
            Vector3 tg = new Vector3( G.Hero.Pos.x, G.Hero.Pos.y, -3.4f );
            Vector2 add = Manager.I.U.DirCord[ ( int ) mov ];
            add += Manager.I.U.DirCord[ ( int ) G.Hero.Dir ];
            tg.x += add.x;
            tg.y += add.y;
            if( Util.Manhattan( AddSpellOrigin, tg ) <= 1 )
            {
                add = Manager.I.U.DirCord[ ( int ) G.Hero.Dir ];                            // too close to hero, ignore hero dir
                tg.x -= add.x;
                tg.y -= add.y;
                Message.RedMessage( "Error: Target Adjacent!" );
            }
            
            for( int i = 8; i < G.Hero.Body.Sp.Count; i++ )
            {
                Spell spl = G.Hero.Body.Sp[ i ];
                if( spl.GetPos( i ) == new Vector2( tg.x, tg.y ) + 
                    Manager.I.U.DirCord[ ( int ) mov ] )
                {
                    Message.RedMessage( "Error: Slot Occupied!" );                          // slot occupied
                    snd1 = "Error 2";
                    goto end;
                }
            }
            Transform tr = PoolManager.Pools[ "Pool" ].Spawn( "Spell" );                    // spawn obj
            fsp = tr.GetComponent<Spell>();
            tr.position = tg;
            tr.parent = Map.I.HeroSpells.transform;
            G.Hero.Body.Sp.Add( fsp );     
            fsp.Attack.Unit = G.Hero;
            G.Hero.Attacks.Add( fsp.Attack );
            fsp.Attack.ID = G.Hero.Attacks.Count - 1;
            fsp.Delta = new Vector2( tg.x, tg.y ) - AddSpellOrigin;
        }

        Sector hs = Map.I.RM.HeroSector;
        EDirection dr = Util.GetTargetUnitDir( AddSpellOrigin, G.Hero.Pos );
        int id = G.Hero.Body.GetShieldID( dr, AddSpellHeroDir );                             // Get Shield ID
        Spell sp = G.Hero.Body.Sp[ id ];
        if( fsp != null ) sp = fsp;

        if( Map.I.RM.HeroSector.Type == Sector.ESectorType.NORMAL )
        {
            if( WeaponType == ItemType.Res_Hero_Shield )
                if( id < 8 )
                {
                    Map.I.RM.HeroSector.ActiveHeroShield[ id ] = true;
                    sp.ItemType = ItemType.Res_Hero_Shield;
                    snd1 = "Electric Orb";
                    snd2 = "Eletric Shock";
                }
            if( WeaponType == ItemType.Res_Throwing_Axe )
            {
                sp.Type = ESpiderBabyType.THROWING_AXE;
                sp.BoltType = EBoltType.Throwing_Axe;
                sp.ItemType = ItemType.Res_Throwing_Axe;
                snd1 = "Axe Throw";
                snd2 = "Item Collect";
            }
            if( WeaponType == ItemType.Res_Spear )
            {
                sp.Type = ESpiderBabyType.SPEAR;
                sp.BoltType = EBoltType.Spear;
                sp.ItemType = ItemType.Res_Spear;
                snd1 = "Axe Throw";
                snd2 = "Item Collect";
            }
            if( WeaponType == ItemType.Res_Bone )
            {
                sp.Type = ESpiderBabyType.BONE;
                snd1 = "Item Collect";
                sp.ItemType = ItemType.Res_Bone;
            }
            if( WeaponType == ItemType.Res_Mushroom_Potion )
            {
                sp.Type = ESpiderBabyType.MUSHROOM_POTION;
                sp.ItemType = ItemType.Res_Mushroom_Potion;
                snd1 = "Item Collect";
            }
            if( WeaponType == ItemType.Res_Torch )
            {
                sp.Type = ESpiderBabyType.TORCH;
                sp.ItemType = ItemType.Res_Torch;
                snd1 = "";
                LightSource2D.Create( CubeData.I.TorchBase, id );
                Vector2 direction = Manager.I.U.DirCord[ ( int ) dr ].normalized;
                Vector2 tg = G.Hero.Pos + direction;
                Map.I.CreateExplosionFX( tg, "Fire Explosion", "Fire Ignite" );                                  // Smoke Cloud FX
            }
            if( WeaponType == ItemType.Res_Bamboo )
                if( id < 8 )
                {
                    int sz = sp.BambooSize;
                    sp.ItemType = ItemType.Res_Bamboo;
                    Vector2 bfrom = G.Hero.Pos + ( Manager.I.U.DirCord[ ( int ) dr ] * sz );
                    Vector2 bto = G.Hero.Pos + ( Manager.I.U.DirCord[ ( int ) dr ] * ( sz + 1 ) );
                    if( Unit.BlocksBamboo( bfrom, bto, null ) == false )                                            // space for Bamboo creation?                  
                    {
                        sp.Type = ESpiderBabyType.BAMBOO;
                        snd2 = "Item Collect";
                        sp.BambooSize++;
                        Unit item = Map.I.GetUnit( ETileType.ITEM, G.Hero.Control.OldPos );
                        if( item ) item.Kill();
                        Item.AddItem( ItemType.Res_Bamboo, +1 );
                    }
                    else Message.RedMessage( "Bamboo\nTarget\nBlocked!" );
                }
            if( WeaponType == ItemType.Res_Hook_CW )
            if( id < 8 )
            {
                sp.Type = ESpiderBabyType.HOOK_CW;
                sp.ItemType = ItemType.Res_Hook_CW;
            }
            if( WeaponType == ItemType.Res_Hook_CCW )
            if( id < 8 )
            {
                sp.Type = ESpiderBabyType.HOOK_CCW;
                sp.ItemType = ItemType.Res_Hook_CCW;
            }
            if( WeaponType == ItemType.Res_Double_Attack )
            {
                sp.Type = ESpiderBabyType.DOUBLE_ATTACK;
                sp.ItemType = ItemType.Res_Double_Attack;
            }
            if( WeaponType == ItemType.Res_WebTrap )
            {
                sp.Type = ESpiderBabyType.WEBTRAP;
                sp.ItemType = ItemType.Res_WebTrap;
            }
            if( WeaponType == ItemType.Res_Attack_Bonus )
            {
                sp.Type = ESpiderBabyType.ATTACK_BONUS;
                sp.ItemType = ItemType.Res_Attack_Bonus;
            }
        }
        sp.Kill = false;
        end:
        MasterAudio.PlaySound3DAtTransform( snd1, G.Hero.transform );
        if( snd2 != "" )
            MasterAudio.PlaySound3DAtVector3( snd2, G.Hero.transform.position );                      // Sound fx
        AddSpellOrigin = new Vector2( -1, -1 );
        AddSpellHeroDir = EDirection.NONE;
        WeaponType = ItemType.NONE;
        SpellSlot = 1;
    }
    public static void AddSpell( ItemType type, Unit res )                                                   // function called when you step over resource
    {
        SpellSlot = res.Body.ResourceSlot;
        if( IsEquipable( type ) ||
            res.Dir != EDirection.NONE )
        {
            Spell.AddSpellOrigin = res.Pos;
            Spell.AddSpellHeroDir = G.Hero.Dir;
            WeaponType = type;
            string msg = Item.GetName( WeaponType ) + " obtained!\nyour next move places it in the first slot adjacent to the hero.";
            if( res.Body.ResourceSlot == 2 )
                msg = Item.GetName( WeaponType ) + " obtained!\nYour next move places it two tiles away in the slot indicated by the hero’s direction.";
            UI.I.SetBigMessage( msg, Color.green, 5, 0, 122.8f, 55, 3, 1 );             
        }
    }
    public static bool IsEquipable( ItemType type )
    {
        if( type == ItemType.Res_Throwing_Axe    ||
            type == ItemType.Res_Spear           ||
            type == ItemType.Res_Hero_Shield     ||
            type == ItemType.Res_Bone            ||
            type == ItemType.Res_Bamboo          ||
            type == ItemType.Res_Hook_CW         ||
            type == ItemType.Res_Hook_CCW        ||
            type == ItemType.Res_Mushroom_Potion ||
            type == ItemType.Res_Torch           ||
            type == ItemType.Res_Double_Attack   ||
            type == ItemType.Res_WebTrap         ||
            type == ItemType.Res_Attack_Bonus    ) return true;
        return false;
    }

    public static bool SpellUsage( Attack att, Unit enemy )
    {
        if( att == null ) return false;
        if( att.TargettingType != ETargettingType.SPELL ) return false;
        ArcherArrowAnimation.SpellPosition = -1;
        Sector hs = Map.I.RM.HeroSector;
        Spell sp = G.Hero.Body.Sp[ att.ID ];
 
        ItemType it = ItemType.NONE;
        switch( sp.Type )
        {
            case ESpiderBabyType.THROWING_AXE:
                it = ItemType.Res_Throwing_Axe;
                ArcherArrowAnimation.BoltType = EBoltType.Spell;
                ArcherArrowAnimation.SpriteID = ( int ) ItemType.Res_Throwing_Axe;
                Attack.AxeAttackList.Add(att.ID);
                return false;
            break;
            case ESpiderBabyType.SPEAR:
                it = ItemType.Res_Spear;
                return false;
            break;
            case ESpiderBabyType.BONE: 
                ArcherArrowAnimation.Spell = new Spell();
                ArcherArrowAnimation.Spell.Copy( sp );
                it = ItemType.Res_Bone;
            break;
            case ESpiderBabyType.MUSHROOM_POTION: 
                ArcherArrowAnimation.Spell = new Spell();
                ArcherArrowAnimation.Spell.Copy( sp );
                it = ItemType.Res_Mushroom_Potion;
            break;
            case ESpiderBabyType.TORCH:
            return false;
            break;
            case ESpiderBabyType.HOOK_CW: 
                ArcherArrowAnimation.BoltType = EBoltType.Spell;
                ArcherArrowAnimation.SpriteID = ( int ) ItemType.Res_Hook_CW;
                //ArcherArrowAnimation.FixedRotation =
                //G.Hero.Body.Sp[ att.ID ].SpellSprite.transform.eulerAngles.z;
                //ArcherArrowAnimation.FixedPosition =
                //G.Hero.Body.Sp[ att.ID ].SpellSprite.transform.position;
                it = ItemType.Res_Hook_CW;
            break;
            case ESpiderBabyType.HOOK_CCW: 
                ArcherArrowAnimation.BoltType = EBoltType.Spell;
                ArcherArrowAnimation.SpriteID = ( int ) ItemType.Res_Hook_CCW;
                //ArcherArrowAnimation.FixedRotation = 
                //G.Hero.Body.Sp[ att.ID ].SpellSprite.transform.eulerAngles.z;
                //ArcherArrowAnimation.FixedPosition = 
                //G.Hero.Body.Sp[ att.ID ].SpellSprite.transform.position;
                it = ItemType.Res_Hook_CCW;   
            break; 
        }

        int slot = GetSpellSlot( enemy );
        
        if( it == ItemType.Res_Mushroom_Potion )                                                  // No attack, just spell
        {
            if( slot != -1 && enemy.Body.Sp[ slot ].Type == ESpiderBabyType.NONE )
            {
                enemy.Control.ForcedFrontalMovementFactor = 4;
                //EDirection dr = Util.GetTargetUnitDir( enemy.Control.OldPos, enemy.Pos );
                EDirection dr = enemy.Dir;
                enemy.Control.ForcedFrontalMovementDir = dr;                                                                  // el torero direction is always relative from old position
                ChargeItem( att, it );                                                             // Charges item
                ArcherArrowAnimation.SpriteID = ( int ) ItemType.Res_Mushroom_Potion;              // Spell FX
                ArcherArrowAnimation.SpellPosition = slot;
                G.Hero.RangedAttack.CreateArrowAnimation( enemy, Vector2.zero, EBoltType.Spell );
            }
            return true;
        }
        else
        if( it == ItemType.Res_Bone )                                                               // No attack, just spell
        {
            if( slot != -1 && enemy.Body.Sp[ slot ].Type == ESpiderBabyType.NONE )
            {                
                enemy.Body.Crippled = true;
                ChargeItem( att, it );                                                             // Charges item
                ArcherArrowAnimation.SpriteID = ( int ) ItemType.Res_Bone;                         // Spell FX
                ArcherArrowAnimation.SpellPosition = slot; 
                G.Hero.RangedAttack.CreateArrowAnimation( enemy, Vector2.zero, EBoltType.Spell );                
            }
            return true;
        }
        else
        if( it == ItemType.Res_Spear        ||
            it == ItemType.Res_Hook_CW      ||
            it == ItemType.Res_Hook_CCW     )  
        {
            if( enemy.TileID == ETileType.SPIDER ) return true;                            // these ones cant be attacked
            if( enemy.TileID == ETileType.HUGGER ) return true;
        }

        if( IsHook( sp.Type ) ) return false;                                              // Hook can be pulled, do not charge it

        ChargeItem( att, it );                                                             // Charges item and removes spell icons

        return false;
    }
    public static int GetSpellSlot( Unit enemy )
    {
        EDirection dr = enemy.Body.GetPosRelativeDir( enemy.Dir, Attack.AttackOrigin );
        if( enemy.TileID == ETileType.SCORPION )                                                // Scorpion fixed baby pos                            
            dr = Util.GetInvDir( Attack.AttackOriginalDirection );
        return ( int ) dr;
    }
    public static void AttachSpell( Unit un, int id )
    {
        if( un == null ) return;
        if( ArcherArrowAnimation.SpellPosition == -1 ) return;
        un.Body.Sp[ id ].Type = ArcherArrowAnimation.Spell.Type;                            // Attach spell to enemy
        MasterAudio.PlaySound3DAtTransform( "Raft Merge", un.transform );
    }

    public static void ChargeItem( Attack att, ItemType it )
    {
        if( it == ItemType.Res_Throwing_Axe ) return;
        Sector hs = Map.I.RM.HeroSector;
        if( it != ItemType.NONE )
        {
            int count = 0;
            if ( hs && hs.ActiveHeroShield != null )                                         // Only disables sprite at spell position if theres no more item available
            for( int i = 0; i < G.Hero.Body.Sp.Count; i++ )
            {
                Spell sp = G.Hero.Body.Sp[ i ];
                bool force = false;
                ESpiderBabyType type = sp.Type;
                if( IsHook( type ) && IsHook( G.Hero.Body.Sp[ att.ID ].Type ) )             // Hook uses only CW for data storage
                    force = true;

                if( type == G.Hero.Body.Sp[ att.ID ].Type || force )
                    count++;
            }

            Item.AddItem( it, -1 );                                                          // Remove item from the inventory

            if( ( int ) Item.GetNum( it ) < count )                                          // Disables sprite
            {
                G.Hero.Body.Sp[ att.ID ].Reset();
                if( att.ID < 8 )
                    Map.I.HeroSpellSpriteList[ att.ID ].gameObject.SetActive( false );
                if( att.ID >= 8 )                                                            // removes fixed att and spell from lists
                {
                    G.Hero.Body.Sp[ att.ID ].Kill = true;
                }    
            }
        }
    }
    public static void ChargeOneTimeSpellUsage()
    {
        for( int listid = 1; listid <= 2; listid++ )
        {
            List<int> list = Attack.AxeAttackList;                                                        // first pass: axe
            ItemType type = ItemType.Res_Throwing_Axe;
            ESpiderBabyType sptype = ESpiderBabyType.THROWING_AXE;
            if( listid == 2 )
            {
                list = Attack.DuplicatorAttackList;                                                       // second pass: duplicator
                type = ItemType.Res_Double_Attack;
                sptype = ESpiderBabyType.DOUBLE_ATTACK;
            }

            if( list.Count > 0 )                                                                           // spell item has been used
            {
                Item.AddItem( type, -1 );                                                                  // Remove item from the inventory
                if ( Item.GetNum( type ) <= 0 )
                for( int i = G.Hero.Body.Sp.Count - 1; i >= 0; i-- )
                if ( G.Hero.Body.Sp[ i ].Type == sptype )
                {
                    if( i >= 8 )
                        G.Hero.Body.Sp[ i ].Kill = true;                                                   // kill and despawn fixed axe
                    else
                        G.Hero.Body.Sp[ i ].Reset();                                                       // or reset adjacent one
                }
            }
        }

        if( Attack.AxeAttackList.Count > 1 )
            Message.CreateMessage( ETileType.NONE, ItemType.Res_Throwing_Axe, "Combo x" +                  // axe combo msg
            Attack.AxeAttackList.Count, G.Hero.Pos, Color.green );
    }
    public static bool IsHook( ESpiderBabyType type )
    {
        if( type == ESpiderBabyType.HOOK_CW || type == ESpiderBabyType.HOOK_CCW ) return true;
        return false;
    }
    public static void UpdateSprites( Unit un )
    {
        Sector hs = Map.I.RM.HeroSector;
        un.Body.Crippled = false;
        bool updID = false;
        int torchCount = 0;
        List<Unit> firelist = new List<Unit>();
        if( hs && hs.ActiveHeroShield != null && hs.Type == Sector.ESectorType.NORMAL )
        for( int i = un.Body.Sp.Count - 1; i >= 0; i-- )
            {
                Spell s = un.Body.Sp[ i ];
                if( s.sptr == null ) s.sptr = s.SpellSprite.transform;                       // opt
                if( i < 8 )
                {
                    Map.I.HeroShieldSpriteList[ i ].gameObject.SetActive( false );           // Hero Shield sprite activation
                    if( hs.ActiveHeroShield[ i ] )
                        Map.I.HeroShieldSpriteList[ i ].gameObject.SetActive( true );

                    Map.I.HeroBambooSpriteList[ i ].gameObject.SetActive( false );           // Bamboo uses a tk2dtiled sprite
                    if( s.BambooSize >= 1 )
                        Map.I.HeroBambooSpriteList[ i ].gameObject.SetActive( true );
                }

                s.sptr.gameObject.SetActive( false );
                bool rot = false;
                float scale = .6f;
                float euler = -1;
                bool attach = false;
                bool point = false;
                bool face = false;
                int id = -1;
                ESpriteCol col = ESpriteCol.MONSTER_ANIM;

                switch( s.Type )
                {
                    case ESpiderBabyType.THROWING_AXE: id = 551; scale = .8f; rot = true;
                    break;
                    case ESpiderBabyType.SPEAR:
                    id = 553; point = true;
                    scale = .8f;
                    break;
                    case ESpiderBabyType.DOUBLE_ATTACK:  
                    col = ESpriteCol.ITEM; euler = 0;
                    id = 155; scale = .4f;
                    break;
                    case ESpiderBabyType.WEBTRAP:
                    col = ESpriteCol.ITEM; 
                    euler = 0;
                    id = 156;
                    scale = .8f;
                    Vector2 tg = s.GetPos( i );
                    Unit mn = Map.I.GetUnit( tg, ELayerType.MONSTER );
                    if( mn && mn.ValidMonster )
                    if( mn.Control.WaitTimeCounter <= 0 )
                    {
                        mn.Control.WaitTimeCounter = 5;
                        mn.Control.SpeedTimeCounter = 0;                                                                         // web slows down land monster
                        mn.MeleeAttack.SpeedTimeCounter = 0;
                        ArcherArrowAnimation.SpriteID = ( int ) ItemType.Res_WebTrap;
                        ArcherArrowAnimation an = ArcherArrowAnimation.Create( G.Hero.Pos, mn.Pos, EBoltType.Spell, 5 );
                        an.Sprite.Collection = Map.I.SpriteCollectionList[ ( int ) ESpriteCol.ITEM ];
                        an.Sprite.spriteId = G.GIT( ItemType.Res_WebTrap ).TKSprite.spriteId;
                        an.Sprite.transform.localScale = new Vector3( .8f, .8f, 1 );
                        an.RandomFactor = Vector3.zero;
                        ArcherArrowAnimation.PlayExplosion = 1;
                        an.AfterReachTime = 5;
                        ChargeItem( s.Attack, ItemType.Res_WebTrap );
                    }
                    break;
                    case ESpiderBabyType.ATTACK_BONUS:
                    col = ESpriteCol.ITEM; euler = 0;
                    id = 154;
                    break;
                    case ESpiderBabyType.BONE:
                    id = 552; rot = true;
                    un.Body.Crippled = true;
                    break;
                    case ESpiderBabyType.MUSHROOM_POTION:
                    id = 557; euler = 0;
                    break;
                    case ESpiderBabyType.TORCH:
                    id = 558 + Util.Animate( 0.1f, 3 );
                    euler = 0;
                    int id2 = ( int ) Util.RotateDir( ( int ) G.Hero.Dir, i );
                    Vector2 pt = G.Hero.Pos + Manager.I.U.DirCord[ id2 ];
                    Unit fire = Map.I.GetUnit( ETileType.FIRE, pt );
                    if( fire && fire.Body.FireIsOn == false )
                        firelist.Add( fire );
                    torchCount++;
                    break;
                    case ESpiderBabyType.EMPTY_TILE:
                    id = 4; euler = 0;
                    break;
                    case ESpiderBabyType.HOOK_CW: id = 555; scale = .8f;
                    attach = true;
                    break;
                    case ESpiderBabyType.HOOK_CCW: id = 556; scale = .8f;
                    attach = true;
                    break;
                    case ESpiderBabyType.COCOON: id = 2; break;
                    case ESpiderBabyType.BOTH: id = 3; break;
                    case ESpiderBabyType.SPIDER: id = 1; face = true; break;
                    case ESpiderBabyType.SCARAB: id = 63; face = true; break;
                    case ESpiderBabyType.SCORPION: id = 165; face = true; break;
                    case ESpiderBabyType.ITEM:
                    col = ESpriteCol.ITEM;

                    int itid = un.Body.BabyVariation[ i ];
                    id = G.GIT( itid ).TKSprite.spriteId;
                    s.SpellSprite.scale = new Vector3( .35f, .35f, 1f );
                    scale = -1;
                    euler = 0;
                    break;
                }

                float mult = 64;
                float basesz = 80;
                EDirection dr = Util.GetRelativeDirection( i, G.Hero.Dir );                             // Calculates bamboo size
                if( Util.IsDiagonal( dr ) )
                {
                    basesz = 80; // 120 for full diagonal tile coverage
                    mult = 90;
                }
                if( i < 8 )
                    Map.I.HeroBambooSpriteList[ i ].dimensions = new Vector2( 50, basesz + ( s.BambooSize - 1 ) * mult );

                if( scale > 0 )
                    s.SpellSprite.scale = new Vector3( scale, scale, 1f );

                if( id != -1 )
                {
                    s.SpellSprite.spriteId = id;                                                            // Spell Sprite set
                    s.SpellSprite.gameObject.SetActive( true );
                    s.SpellSprite.SetSprite( Map.I.SpriteCollectionList[ ( int ) col ], id );
                }

                if( rot )
                {
                    float ang = s.sptr.eulerAngles.z;                                                       // axe rotation animation
                    ang += Time.deltaTime * 300;
                    if( ang > 360 ) ang -= 360;
                    s.sptr.eulerAngles = new Vector3( 0, 0, ang );
                }

                if( attach )
                {                                                                                           // Direction depending on hero dir
                    dr = Util.RotateDir( ( int ) G.Hero.Dir, i );
                    euler = Util.GetRotationAngle( dr );
                }

                if( euler != -1 )
                    s.sptr.eulerAngles = new Vector3( 0, 0, euler );                                        // Fixed angle

                if( point )
                {
                    EDirection dir = Util.GetTargetUnitDir( G.Hero.Control.OldPos, G.Hero.Pos );
                    s.sptr.eulerAngles = Util.GetRotationAngleVector( dir );
                }

                if( face )                                                                                   // Face hero Directly
                {
                    Quaternion qn = Util.GetRotationToPoint( s.sptr.position, G.Hero.transform.position );
                    s.sptr.rotation = Quaternion.RotateTowards( s.sptr.rotation, qn, Time.deltaTime * 10000 );
                }

                if( s.Kill )
                {
                    s.transform.parent = PoolManager.Pools[ "Pool" ].gameObject.transform;
                    PoolManager.Pools[ "Pool" ].Despawn( s.transform );
                    G.Hero.Body.Sp.Remove( s );
                    G.Hero.Attacks.Remove( s.Attack );
                    updID = true; 
                }
            }

        if( updID )
        for( int i = 0; i < un.Attacks.Count; i++ )
        {
            if( un.Attacks[ i ] != null )
                un.Attacks[ i ].ID = i;
        }

        UpdateTorchFireLighting( torchCount,  firelist  );
    }
    public Vector2 GetPos( int id )
    { 
        if( id < 8 )
        {
            int id2 = ( int ) Util.RotateDir( ( int ) G.Hero.Dir, id );
            return G.Hero.Pos + Manager.I.U.DirCord[ id2 ];
        }
        return G.Hero.Pos + Delta;
    }

    private static void UpdateTorchFireLighting( int torchCount, List<Unit> firelist )
    {
        for( int i = 0; i < firelist.Count; i++ )
        {
            Map.I.CheckFireIgnition( ( int ) firelist[ i ].Pos.x, ( int ) firelist[ i ].Pos.y, torchCount );          // use torch to light firepits. Each Torch reduces Log needed by 1
        }
    }

    public static bool IsSpawnAble( ESpiderBabyType type )
    {
        if( type == ESpiderBabyType.BONE ) return false;
        if( type == ESpiderBabyType.MUSHROOM_POTION ) return false;
        return true;
    }

    public static bool CheckSpellMoveBlock( Unit un )
    {
        if( un.Body.Crippled )                                                                                           // Crippled monster (by bone)
        if( Map.I.IsInTheSameLine( un.Pos, G.Hero.Pos, true ) == false ||
            Map.I.HasLOS( un.Pos, Map.I.Hero.Pos, true, null, true, true ) == false )
            return true;
        return false; 
    }

    public static float GetSpellDamage( Attack attack )
    {
        float damage = -1;
        Spell sp = G.Hero.Body.Sp[ attack.ID ];

        switch( sp.Type )
        {
        case ESpiderBabyType.THROWING_AXE: damage = Map.I.RM.RMD.BaseThrowingAxeAttackDamage; break;                      // Returns spell damage
        case ESpiderBabyType.SPEAR: damage = Map.I.RM.RMD.BaseSpearAttackDamage; break;
        case ESpiderBabyType.HOOK_CW: damage = Map.I.RM.RMD.BaseHookAttackDamage; break;
        case ESpiderBabyType.HOOK_CCW: damage = Map.I.RM.RMD.BaseHookAttackDamage; break;
        case ESpiderBabyType.BAMBOO: damage = Map.I.RM.RMD.BaseBambooAttackDamage; break;
        }
        return damage;
    }

    public static void RemoveAllSpellsOfType( ESpiderBabyType type, Unit un )
    {
        for( int i = 0; i < un.Body.Sp.Count; i++ )
        if ( un.Body.Sp[ i ].Type == type )
             un.Body.Sp[ i ].Reset();
    }

    public static void UpdateHookDestruction( Vector2 tg )
    {
        for( int i = 0; i < 8; i++ )
        {
            Spell sp = G.Hero.Body.Sp[ i ];
            int id2 = ( int ) Util.RotateDir( ( int ) G.Hero.Dir, i );
            Vector2 pt = G.Hero.Pos + Manager.I.U.DirCord[ id2 ];
            if( tg == pt )
            if( IsHook(  sp.Type ) )
            {
                bool hooked = AnyMonsterHookStuck();                                          // Already hooked?
                float damage = Spell.GetSpellDamage( sp.Attack );           // Calculates damage
                Unit mn = Map.I.GetUnit( tg, ELayerType.MONSTER );                           // Land Units
                bool destroy = false;
                if( mn && mn.ValidMonster )
                if( mn.Body.HookIsStuck == false )
                {
                    destroy = true;  
                }                

                List<Unit> fl = Map.I.GetFUnit( tg );                                        // Flying units
                if ( fl != null )
                for( int f = 0; f < fl.Count; f++ )
                if ( fl[ f ].ValidMonster )
                if ( fl[ f ].Body.HookIsStuck == false )
                {
                    destroy = true;
                }            

                if( destroy )                                                                // Destroy hook
                {
                    MasterAudio.PlaySound3DAtTransform( "Crunch", G.Hero.transform );
                    Item.AddItem( ItemType.Res_Hook_CW, -1 );
                    sp.Reset();
                }
                return;
            }
        }
    }
    public static List<int> processed = new List<int>();                                                        // no 2x processing of same hook

    public static bool UpdateHookJump( Vector2 from, Vector2 to )
    {
        processed = new List<int>();
        EDirection dr = Util.GetTargetUnitDir( from, to );
        for( int i = 0; i < 8; i++ )
        if( IsHook( G.Hero.Body.Sp[ i ].Type ) )
        {
            int id1 = ( int ) Util.RotateDir( ( int ) G.Hero.Dir, i );
            Vector2 newp = G.Hero.Pos + Manager.I.U.DirCord[ id1 ];                                   // Calculates target postion
            Unit mn = Map.I.GetUnit( newp, ELayerType.MONSTER );   
            if( mn != null && mn.ValidMonster )
            if( mn.Body.HookIsStuck )
            {
                if( id1 == ( int ) dr )
                {
                    Vector2 jump = to + Manager.I.U.DirCord[ id1 ];
                    if( Sector.GetPosSectorType( jump ) == Sector.ESectorType.NORMAL )
                    if( G.Hero.CanMoveFromTo( true, G.Hero.Pos, jump, G.Hero ) == true )
                    {
                        int j = ( int ) Util.GetInvDir( ( EDirection ) i );                          // Performs jump
                        G.Hero.Body.Sp[ j ].Copy( G.Hero.Body.Sp[ i ] );                             // Relocate spell position after jump
                        G.Hero.Body.Sp[ i ].Reset();
                        processed.Add( j );
                    }
                }
                else
                if( Util.IsNeighbor( to, mn.Pos ) == false )
                {
                    int newId = -1;
                    newp = GetHookDragTarget( dr, i, id1, ref newId, ref mn, to );                   // Blocks the hero if Hooked monster is pushed against water or pit   
                    Unit ga = Map.I.GetUnit( newp, ELayerType.GAIA );
                    if( ga )
                    if( ga.TileID == ETileType.WATER || ga.TileID == ETileType.PIT )
                    {
                        Map.I.AdvanceTurn = false;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public static void UpdateHookedMonsters()
    {
        EDirection dr = Util.GetTargetUnitDir( G.Hero.Control.OldPos, G.Hero.Pos );
        for( int i = 0; i < 8; i++ )
        if( processed.Contains( i ) == false )
        if( IsHook( G.Hero.Body.Sp[ i ].Type ) )
        {
            Spell sp = G.Hero.Body.Sp[ i ];
            int id1 = ( int ) Util.RotateDir( ( int ) G.Hero.Dir, i );
            int newId = -1;
            Vector2 newp = G.Hero.Control.OldPos + Manager.I.U.DirCord[ id1 ];                        // Calculates target postion
            Unit mn = Map.I.GetUnit( newp, ELayerType.MONSTER );                           
            if( mn != null && mn.ValidMonster )
            if( mn.Body.HookIsStuck )
            if( Util.IsNeighbor( G.Hero.Pos, mn.Pos ) == true )
                {
                    for( int j = 0; j < 8; j++ )
                    {
                        int id2 = ( int ) Util.RotateDir( ( int ) G.Hero.Dir, j );
                        Vector2 pt = G.Hero.Pos + Manager.I.U.DirCord[ id2 ];
                        Unit mn2 = Map.I.GetUnit( pt, ELayerType.MONSTER );
                        if( processed.Contains( i ) == false )
                            if( mn2 && mn && mn2 == mn )                                             // Relocates hook after monster side step
                            {
                                processed.Add( j );
                                G.Hero.Body.Sp[ j ].Copy( G.Hero.Body.Sp[ i ] );
                                G.Hero.Body.Sp[ i ].Reset();
                                mn.Control.SpeedTimeCounter = 0;
                            }
                    }
                }
                else
                {
                newp = GetHookDragTarget( dr, i, id1, ref newId, ref mn, G.Hero.Pos );                                          // Gets the position where the hooked monster should move to   

                bool res = mn.CanMoveFromTo( true, mn.Pos, newp, G.Hero );                                                      // Moves monster
                if( res )
                {
                    mn.Control.SpeedTimeCounter = 0;                                                                            // Move is OK
                    processed.Add( newId );
                    if( newId != -1 )
                    {
                        G.Hero.Body.Sp[ newId ].Copy( sp );                                                                     // Relocates diagonal special move hook
                        sp.Reset();
                    }
                    MasterAudio.PlaySound3DAtTransform( "Hook Drag", mn.transform, .7f );                                       // Sound FX
                    mn.Body.CreateBloodSpilling( mn.Pos );
                }
                else                                                                                                            // Monster blocked
                {    
                    Unit mn2 = Map.I.GetUnit( newp, ELayerType.MONSTER );
                    processed.Add( newId );
                    if( mn.Body.HookIsStuck )
                    {
                        StickHook( sp, mn );                                                                   // Source monsters is attacked since it cannot move
                        if( newId != -1 )
                        {
                            G.Hero.Body.Sp[ newId ].Copy( sp );                                                // Relocates diagonal special move hook
                            sp.Reset();
                        }
                    }

                    if( mn2  )
                    {
                        Item.AddItem( ItemType.Res_Hook_CW, 1 );                                               // Adds a free hook for translating hooked monsters
                        StickHook( sp, mn2 );                                                                  // Destination Monster is attacked
                        processed.Add( newId );
                        if( newId != -1 )
                        {
                            G.Hero.Body.Sp[ newId ].Copy( sp );                                                // Relocates diagonal special move hook
                            sp.Reset();
                        }
                    }
                    else
                    {
                        ChargeItem( sp.Attack, ItemType.Res_Hook_CW );                                         // Empty destination, charge hook
                    }
                }
            }
        }
    }

    private static Vector2 GetHookDragTarget( EDirection dr, int i, int id1, ref int newId, ref Unit mn, Vector2 tg )
    {
        Vector2 newp = mn.Pos + Manager.I.U.DirCord[ ( int ) dr ];

        if( Util.IsNeighbor( newp, tg ) )
        if( Util.IsDiagonal( dr ) )                                                                            // Diagonal special movement case
            {
                UpdHook( EActionType.MOVE_NE, id1, 3, 4, +1, ref mn, i, ref newId, ref newp, tg );
                UpdHook( EActionType.MOVE_NE, id1, 7, 6, -1, ref mn, i, ref newId, ref newp, tg );
                UpdHook( EActionType.MOVE_SE, id1, 5, 6, +1, ref mn, i, ref newId, ref newp, tg );
                UpdHook( EActionType.MOVE_SE, id1, 1, 0, -1, ref mn, i, ref newId, ref newp, tg );
                UpdHook( EActionType.MOVE_SW, id1, 7, 0, +1, ref mn, i, ref newId, ref newp, tg );
                UpdHook( EActionType.MOVE_SW, id1, 3, 2, -1, ref mn, i, ref newId, ref newp, tg);
                UpdHook( EActionType.MOVE_NW, id1, 1, 2, +1, ref mn, i, ref newId, ref newp, tg );
                UpdHook( EActionType.MOVE_NW, id1, 5, 4, -1, ref mn, i, ref newId, ref newp, tg );
            }
        return newp;
    }
    private static void UpdHook( EActionType ac, int id1, int p1, int p2, int rot, ref Unit mn, int i, ref int newID, ref Vector2 newp, Vector2 tg )
    {
        if( id1 != p1 ) return;
        if( Map.I.CurrentMoveTrial == ac )
        {
            int np = ( int ) Util.GetRelativeDirection( rot, ( EDirection ) p1 );
            newp = tg + Manager.I.U.DirCord[ np ];
            newID = ( int ) Util.GetRelativeDirection( rot, ( EDirection ) i );
        }
        return;
    }

    public static void StickHook( Spell sp, Unit mn, bool attack = true )
    {
        float damage = Spell.GetSpellDamage( sp.Attack );                                                      // Calculates damage
        mn.WakeMeUp();
        mn.Body.HookIsStuck = true;
        mn.Body.HookID = sp;
        sp.HookedUnit = mn;
        if( attack )
            mn.Body.ReceiveDamage( damage, EDamageType.BLEEDING, G.Hero, sp.Attack );
        MasterAudio.PlaySound3DAtTransform( "Monster Falling", mn.transform );
        mn.Body.CreateBloodSpilling( mn.Pos );
    }

    public static bool AnyMonsterHookStuck()
    {
        for( int i = 0; i < 8; i++ )
        if( IsHook( G.Hero.Body.Sp[ i ].Type ) )
        {
            int id1 = ( int ) Util.RotateDir( ( int ) G.Hero.Dir, i );
            Vector2 pos = G.Hero.Pos + Manager.I.U.DirCord[ id1 ];
            Unit mn = Map.I.GetUnit( pos, ELayerType.MONSTER );
            if( mn && mn.Body.HookIsStuck ) return true;
        }
        return false;
    }

    public static bool IsCompoundMonster( Unit un )
    {
        for( int i = 0; i < un.Body.Sp.Count; i++ )
        switch( un.Body.Sp[ i ].Type )
        {
            case ESpiderBabyType.SCARAB: return true;
            case ESpiderBabyType.SCORPION: return true;
            case ESpiderBabyType.SPIDER: return true;
        }
        return false;
    }
    public static void CheckDuplicator( Vector2 tg, int id )
    {
        Spell sp = G.Hero.Body.Sp[ id ];
        if( sp.GetPos( id ) == tg )
        if( sp.Type == ESpiderBabyType.DOUBLE_ATTACK )
        {
            Attack.DuplicatorCount++;
        }
    }

    internal static void UpdateAfterAttackItemCharge( Attack att )
    {
        if( att.TargettingType != ETargettingType.SPELL ) return;
        if( att.Unit.UnitType != EUnitType.HERO ) return;
        Spell sp = G.Hero.Body.Sp[ att.ID ];
        if( att.TargettingType == ETargettingType.SPELL )
        if( sp.Type == ESpiderBabyType.SPEAR )
            Spell.ChargeItem( att, sp.ItemType );                     // Charges these item only here to allow multiple targets
    }
    internal bool IsAttackingSpell()
    {
        if( Type == ESpiderBabyType.SPEAR || 
            Type == ESpiderBabyType.THROWING_AXE ) 
            return true;
        return false;
    }
}


  

