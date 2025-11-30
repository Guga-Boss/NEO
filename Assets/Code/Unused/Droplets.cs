using UnityEngine;
using System.Collections.Generic;

public class Droplets : MonoBehaviour
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

    private void Start()
    {
        InitializeDroplets();
    }

    private void InitializeDroplets()
    {
        dropletPositions = new Vector4[ maxDroplets ];
        dropletStartTimes = new float[ maxDroplets ];

        for( int i = 0; i < maxDroplets; i++ )
        {
            dropletPositions[ i ] = Vector4.zero;
            dropletStartTimes[ i ] = -1f;

            WaterMaterial.SetVector( "_DropletPositions_" + i, Vector4.zero );       // zera posição
            WaterMaterial.SetFloat( "_DropletTimes_" + i, -1f );                     // tempo inativo
        }

        WaterMaterial.SetInt( "_DropletCount", 0 );
    }

    private void Update()
    {
        UpdateFollowDroplets();
    }

    private void UpdateFollowDroplets()
    {
        int activeCount = 0;
        if( followTargets == null ) return;

        int count = Mathf.Min( followTargets.Count, maxDroplets );
        for( int i = 0; i < count; i++ )
        {
            if( followTargets[ i ] == null ) continue;

            bool isNew = dropletStartTimes[ i ] < 0;
            Vector3 pos = followTargets[ i ].position;

            dropletPositions[ i ] = new Vector4( pos.x, pos.y, isNew ? Time.time : dropletPositions[ i ].z, 1 );
            if( isNew )
                dropletStartTimes[ i ] = Time.time;

            UpdateShaderSlot( i );
            activeCount++;
        }

        // Limpa slots excedentes
        for( int i = count; i < maxDroplets; i++ )
        {
            if( dropletStartTimes[ i ] >= 0 )
            {
                dropletPositions[ i ] = Vector4.zero;
                dropletStartTimes[ i ] = -1f;
                WaterMaterial.SetVector( "_DropletPositions_" + i, Vector4.zero );
                WaterMaterial.SetFloat( "_DropletTimes_" + i, -1f );
            }
        }

        WaterMaterial.SetInt( "_DropletCount", activeCount );
    }

    private void UpdateShaderSlot( int index )
    {
        WaterMaterial.SetVector( "_DropletPositions_" + index, dropletPositions[ index ] );
        WaterMaterial.SetFloat( "_DropletTimes_" + index,
            dropletStartTimes[ index ] > 0 ? Time.time - dropletStartTimes[ index ] : -1f );
    }
}
