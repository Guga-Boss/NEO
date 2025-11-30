using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour
{
    public static int sizeX = 30;                                  // grid width
    public static int sizeY = 30;                                  // grid height
    public static int[ , ] dist;                                   // distance matrix to hero
    public static Unit[ , ] un, ga;                                // cache for units and terrain
    private static Vector2 tempOffset;                              // reusable offset
    private static Vector2 tempVec;                                 // reusable temp vector

    private static readonly VI[] dirs = new VI[]                   // directions including diagonals
    {
        new VI(0, 1),  // N
        new VI(0, -1), // S
        new VI(1, 0),  // E
        new VI(-1, 0), // W
        new VI(1, 1),  // NE
        new VI(-1, 1), // NW
        new VI(1, -1), // SE
        new VI(-1, -1),// SW
    };

    // Updates the FlowField from the hero's global position
    public static void UpdateBrainMove()
    {
        if( !Active() ) return;                                      // return if Brain not active
        if( Map.I.CubeDeath ) return;                                // return if hero is dead
        if( Map.I.RM.HeroSector == null ) return;                    // return if hero sector null
        if( Map.I.RM.HeroSector.Type != Sector.ESectorType.NORMAL )
            return;                                                  // only normal sectors
        un = Map.I.Unit;                                             // cache units
        ga = Map.I.Gaia;                                             // cache terrain
        VI heroLocal = GetCubeTile( G.Hero.Pos );                    // convert global hero pos to local
        BuildFlowField( heroLocal );                                 // build BFS distances
    }

    // Converts global position to local cube coordinates
    public static VI GetCubeTile( Vector2 globalPos )
    {
        if( Manager.I.GameType == EGameType.CUBES )
        {
            return new VI(
                ( int ) ( globalPos.x - Map.I.RM.HeroSector.Area.xMin ), // x relative to cube
                ( int ) ( globalPos.y - Map.I.RM.HeroSector.Area.yMin )  // y relative to cube
            );
        }
        return new VI( -1, -1 );                                         // invalid if not cubes
    }

    // Builds BFS FlowField inside 30x30 cube
    public static void BuildFlowField( VI heroLocal )
    {
        if( !InsideGrid( heroLocal.x, heroLocal.y ) ) return;            // check bounds

        dist = new int[ sizeX, sizeY ];                                  // initialize distance matrix
        for( int x = 0; x < sizeX; x++ )
        for( int y = 0; y < sizeY; y++ )
             dist[ x, y ] = int.MaxValue;                                // mark all unvisited

        Queue<VI> q = new Queue<VI>();                                   // BFS queue
        dist[ heroLocal.x, heroLocal.y ] = 0;                            // hero distance = 0
        q.Enqueue( heroLocal );                                          // enqueue hero

        tempOffset.x = Map.I.RM.HeroSector.Area.xMin;                    // reuse static offset
        tempOffset.y = Map.I.RM.HeroSector.Area.yMin;

        while( q.Count > 0 )
        {
            VI p = q.Dequeue();                                          // current tile

            foreach( VI dir in dirs )                                    // iterate directions
            {
                VI n = p + dir;                                          // neighbor
                if( !InsideGrid( n.x, n.y ) ) continue;                  // skip out of bounds
                if( dist[ n.x, n.y ] != int.MaxValue ) continue;         // skip visited

                tempVec.x = n.x + tempOffset.x;                          // compute global position
                tempVec.y = n.y + tempOffset.y;
                Vector2 globalFrom = p.ToVector2() + tempOffset;         // local->global
                Vector2 globalTo = tempVec;                              // local->global
                if( !CanMove( globalFrom, globalTo ) ) continue;         // check blockage

                dist[ n.x, n.y ] = dist[ p.x, p.y ] + 1;                 // set distance
                q.Enqueue( n );                                          // enqueue neighbor
            }
        }
    }

    // Checks if a unit/enemy can move from one tile to another
    public static bool CanMove( Vector2 from, Vector2 to, bool arrow = true )
    {
        int tx = ( int ) to.x;
        int ty = ( int ) to.y;
        if( ga[ tx, ty ] != null )
        {
            var tile = ga[ tx, ty ];
            if( ( tile.TileID == ETileType.WATER || tile.TileID == ETileType.PIT ) &&
                 Controller.GetRaft( to ) != null ) { }                                   // raft allows movement
            else if( tile.BlockMovement ) return false;                                   // blocked
        }

        if( Map.GFU( ETileType.MINE, to ) != null ) return false;                         // mine blocks movement

        if( arrow )
        if( Map.I.CheckArrowBlockFromTo( to, from, G.Hero ) ) return false;               // arrow allows movement
        return true;                                                                      // default allow
    }

    private static bool InsideGrid( int x, int y )
    {
        return x >= 0 && y >= 0 && x < sizeX && y < sizeY;                           // check bounds
    }

    // Returns the next step for an enemy from global position
    public static Vector2 GetNextStep( Vector2 enemyGlobal )
    {
        VI enemyLocal = GetCubeTile( enemyGlobal );                                 // convert to local
        if( !InsideGrid( enemyLocal.x, enemyLocal.y ) ) return enemyGlobal;         // invalid

        int bestDist = dist[ enemyLocal.x, enemyLocal.y ];                          // current distance
        VI bestLocal = enemyLocal;                                                  // best tile

        foreach( VI dir in dirs )                                                   // check neighbors
        {
            VI n = enemyLocal + dir;
            if( !InsideGrid( n.x, n.y ) ) continue;                                 // skip out of bounds
            if( dist[ n.x, n.y ] < bestDist )                                       // closer neighbor
            {
                bestDist = dist[ n.x, n.y ];                                        // update distance
                bestLocal = n;                                                      // update best tile
            }
        }

        return bestLocal.ToVector2() + tempOffset;                                  // convert back to global
    }

    // Reconstructs the full path from enemy to hero
    public static List<Vector2> GetPath( Vector2 enemyGlobal )
    {
        List<VI> path = new List<VI>();                                            // path list
        VI current = GetCubeTile( enemyGlobal );                                   // start local
        if( !InsideGrid( current.x, current.y ) ) return new List<Vector2>();      // invalid

        path.Add( current );                                                       // add start
        while( dist[ current.x, current.y ] > 0 )                                  // until hero
        {
            VI nextStep = current;                                                 // next tile
            int bestDist = dist[ current.x, current.y ];                           // current distance

            foreach( VI dir in dirs )                                              // check neighbors
            {
                VI n = current + dir;
                if( !InsideGrid( n.x, n.y ) ) continue;                            // skip out of bounds

                if( dist[ n.x, n.y ] < bestDist )                                  // closer neighbor
                {
                    bestDist = dist[ n.x, n.y ];                                   // update best
                    nextStep = n;                                                  // update next
                }
            }
            current = nextStep;                                                    // move
            path.Add( current );                                                   // add to path
        }

        List<Vector2> globalPath = new List<Vector2>();
        foreach( VI v in path )
            globalPath.Add( v.ToVector2() + tempOffset );                          // convert back to global
        return globalPath;                                                         // return path
    }
    public static Vector2 GetBestMove( Vector2 enemyGlobal )                                          // choose best move toward hero, avoid arrows, check occupancy
    {
        if( dist == null ) return enemyGlobal;
        VI enemyLocal = GetCubeTile( enemyGlobal );                                                   // local enemy
        if( !InsideGrid( enemyLocal.x, enemyLocal.y ) ) return enemyGlobal;                           // invalid

        float bestScore = float.PositiveInfinity;                                                     // starting score
        VI bestLocal = enemyLocal;                                                                    // best tile

        List<VI> validPositions = new List<VI>();                                                     // store valid positions
        List<float> validScores = new List<float>();                                                  // store corresponding scores

        Vector2 off = new Vector2( Map.I.RM.HeroSector.Area.xMin, Map.I.RM.HeroSector.Area.yMin );    // sector offset
        Vector2 fromGlobal = enemyLocal.ToVector2() + off;                                            // current global
        VI heroLocal = GetCubeTile( G.Hero.Pos );                                                     // hero local

        foreach( VI dir in dirs )                                                                     // check neighbors
        {
            VI n = enemyLocal + dir;
            if( !InsideGrid( n.x, n.y ) ) continue;                                                   // skip out of bounds
            if( dist[ n.x, n.y ] == int.MaxValue ) continue;                                          // skip unreachable
              
            Vector2 tg = n.ToVector2() + off;                                                         // candidate global
            if( !CanMove( fromGlobal, tg, false ) ) continue;                                         // dynamic block
            if( Map.I.CheckArrowBlockFromTo( fromGlobal, tg, G.Hero ) ) continue;                     // arrow blocks

            float score = dist[ n.x, n.y ] + 
            ( ( n.ToVector2() - heroLocal.ToVector2() ).sqrMagnitude * 0.01f );                       // distance + tie-break
            validPositions.Add( n ); validScores.Add( score );                                        // store valid moves
             
            if( score < bestScore )                                                                   // update best
            {
                bestScore = score;
                bestLocal = n;
            }
        }

        if( validPositions.Count == 0 ) return enemyGlobal;                                           // no valid moves

        Vector2 bestGlobal = bestLocal.ToVector2() + off;                                             // best move global
        Unit occupyingMonster = Map.I.GetUnit( bestGlobal, ELayerType.MONSTER );                      // check occupancy

        if( occupyingMonster != null && occupyingMonster.ValidMonster )                               // if occupied
        {
            VI alternativeLocal = enemyLocal; 
            float alternativeScore = float.PositiveInfinity; 
            bool foundAlternative = false;                                                            // init
            for( int i = 0; i < validPositions.Count; i++ )
            {
                VI movePos = validPositions[ i ]; Vector2 moveGlobal = movePos.ToVector2() + off;
                Unit monsterAtPos = Map.I.GetUnit( moveGlobal, ELayerType.MONSTER );

                if( monsterAtPos != null && monsterAtPos.ValidMonster ) continue;                     // skip occupied

                if( validScores[ i ] < alternativeScore )                                             // better alternative
                {
                    alternativeScore = validScores[ i ];
                    alternativeLocal = movePos;
                    foundAlternative = true;
                }
            }

            if( foundAlternative ) return alternativeLocal.ToVector2() + off;                         // use alternative
        }

        return bestGlobal;                                                                            // return best move
    }
    public static bool IsMonsterTrapped( Vector2 monsterGlobal )                                      // check if monster cannot reach hero
    {
        if( dist == null ) return false;
        VI monsterLocal = GetCubeTile( monsterGlobal );                                               // convert global to local
        if( !InsideGrid( monsterLocal.x, monsterLocal.y ) ) return true;                              // outside grid is considered trapped
        return dist[ monsterLocal.x, monsterLocal.y ] == int.MaxValue;                                // unreachable tiles have MaxValue
    }

    // Checks if Brain system is active
    public static bool Active()
    {
        return Map.I.NumBrains > 0;                                                                   // active if any Brain exists
    }
}
