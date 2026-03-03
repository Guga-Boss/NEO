using UnityEngine;
using System.Collections.Generic;

public class Droplets: MonoBehaviour
{
    [Header( "Water Material" )]
    public Material WaterMaterial;

    [Header( "Droplet Settings" )]
    public int maxDroplets = 50;  // número total de slots
    public float dropletLifetime = 2.0f;

    [Header( "Follow Targets" )]
    public List<Transform> followTargets;

    private Vector4[] dropletPositions;
    private float[] dropletStartTimes;

    // --- OPTIMIZATION: Shader Property IDs Cache ---
    private int[] _posPropertyIDs;
    private int[] _timePropertyIDs;
    private int _countPropertyID;

    private void Start()
    {
        InitializeDroplets();
    }

    private void InitializeDroplets()
    {
        dropletPositions = new Vector4[ maxDroplets ];
        dropletStartTimes = new float[ maxDroplets ];

        _posPropertyIDs = new int[ maxDroplets ];
        _timePropertyIDs = new int[ maxDroplets ];
        _countPropertyID = Shader.PropertyToID( "_DropletCount" );               // Cache main count ID;

        for( int i = 0; i < maxDroplets; i++ )
        {
            dropletPositions[ i ] = Vector4.zero;
            dropletStartTimes[ i ] = -1f;

            // Generate the string ONCE at startup and get the integer ID;
            _posPropertyIDs[ i ] = Shader.PropertyToID( "_DropletPositions_" + i );
            _timePropertyIDs[ i ] = Shader.PropertyToID( "_DropletTimes_" + i );

            WaterMaterial.SetVector( _posPropertyIDs[ i ], Vector4.zero );       // zera posição usando ID;
            WaterMaterial.SetFloat( _timePropertyIDs[ i ], -1f );                // tempo inativo usando ID;
        }

        WaterMaterial.SetInt( _countPropertyID, 0 );
    }

    private void Update()
    {
        UpdateFollowDroplets();
    }

    private void UpdateFollowDroplets()
    {
        if( followTargets == null ) return;

        int activeCount = 0;
        int count = Mathf.Min( followTargets.Count, maxDroplets );
        float currentTime = Time.time;                                           // Cache Time.time to save C++ interop calls;

        for( int i = 0; i < count; i++ )
        {
            var target = followTargets[ i ];                                     // Cache list access;
            if( target == null ) continue;

            bool isNew = dropletStartTimes[ i ] < 0;
            Vector3 pos = target.position;                                       // Single Transform call;

            dropletPositions[ i ] = new Vector4( pos.x, pos.y, isNew ? currentTime : dropletPositions[ i ].z, 1 );

            if( isNew )
                dropletStartTimes[ i ] = currentTime;

            UpdateShaderSlot( i, currentTime );                                  // Pass cached time;
            activeCount++;
        }

        // Limpa slots excedentes
        for( int i = count; i < maxDroplets; i++ )
        {
            if( dropletStartTimes[ i ] >= 0 )
            {
                dropletPositions[ i ] = Vector4.zero;
                dropletStartTimes[ i ] = -1f;

                WaterMaterial.SetVector( _posPropertyIDs[ i ], Vector4.zero );   // Use cached ID;
                WaterMaterial.SetFloat( _timePropertyIDs[ i ], -1f );            // Use cached ID;
            }
        }

        WaterMaterial.SetInt( _countPropertyID, activeCount );                   // Use cached ID;
    }

    private void UpdateShaderSlot( int index, float currentTime )
    {
        // Zero String Concatenation. Zero GC Alloc. Pure Speed.
        WaterMaterial.SetVector( _posPropertyIDs[ index ], dropletPositions[ index ] );
        WaterMaterial.SetFloat( _timePropertyIDs[ index ],
            dropletStartTimes[ index ] > 0 ? currentTime - dropletStartTimes[ index ] : -1f );
    }
}