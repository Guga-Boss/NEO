/***************************************************************************\
Project:      Daily Rewards
Copyright (c) Niobium Studios
Author:       Guilherme Nunes Barbosa (gnunesb@gmail.com)
\***************************************************************************/
using System;
using System.Globalization;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NiobiumStudios
{
    /**
     * Daily Rewards common methods
     **/
    public abstract class DailyRewardsCore<T>: MonoBehaviour where T : DailyRewardsCore<T>
    {
        public int instanceId;
        public bool isSingleton = true;

        public List<CloudClock> cloudClockList;

        public bool useCloudClock = true;

        public string errorMessage;
        public bool isErrorConnect;
        public DateTime now;

        public int maxRetries = 3;

        public delegate void OnInitialize( bool error = false, string errorMessage = "" );
        public OnInitialize onInitialize;

        protected bool isInitialized = false;

        public IEnumerator InitializeDate()
        {
            if( useCloudClock )
            {
                // Conecta com o CloudClockBuilder (que corrigimos para UnityWebRequest)
                if( CloudClockBuilder.currentState == CloudClockBuilder.State.NotInitialized )
                {
                    yield return StartCoroutine( CloudClockBuilder.InitializeCloudClock( cloudClockList, maxRetries ) );
                }
                else if( CloudClockBuilder.currentState == CloudClockBuilder.State.Initializing )
                {
                    while( CloudClockBuilder.currentState == CloudClockBuilder.State.Initializing )
                        yield return null;
                }

                if( CloudClockBuilder.currentState == CloudClockBuilder.State.Initialized )
                {
                    now = CloudClockBuilder.cloudClockDate;
                    isInitialized = true;
                    if( onInitialize != null ) onInitialize();
                }
                else
                {
                    // Se falhar a rede, usamos a hora do PC como fallback para o Idle não zerar
                    isErrorConnect = true;
                    errorMessage = CloudClockBuilder.errorMessage;
                    now = DateTime.Now;
                    isInitialized = true;
                    if( onInitialize != null ) onInitialize( true, errorMessage );
                }
            }
            else
            {
                now = DateTime.Now;
                isInitialized = true;
                if( onInitialize != null ) onInitialize();
            }
        }

        public void RefreshTime()
        {
            if( useCloudClock && CloudClockBuilder.currentState == CloudClockBuilder.State.Initialized )
                now = CloudClockBuilder.cloudClockDate;
            else
                now = DateTime.Now;
        }

        public static T GetInstance( int id = 0 )
        {
            var instances = FindObjectsOfType<T>();
            for( int i = 0; i < instances.Length; i++ )
            {
                if( ( instances[ i ] ).instanceId == id )
                    return instances[ i ];
            }
            return null;
        }

        // Essencial para o Idle Production: mantém o "now" atualizado enquanto o jogo roda
        public virtual void TickTime()
        {
            if( !isInitialized )
                return;

            now = now.AddSeconds( Time.unscaledDeltaTime );

            if( useCloudClock && CloudClockBuilder.currentState == CloudClockBuilder.State.Initialized )
                CloudClockBuilder.cloudClockDate = now;
        }

        public string GetFormattedTime( TimeSpan span )
        {
            return string.Format( "{0:D2}:{1:D2}:{2:D2}", span.Hours, span.Minutes, span.Seconds );
        }

        protected virtual void Awake()
        {
            if( isSingleton )
            {
                var instanceCount = GetInstanceCount();
                if( instanceCount > 1 )
                {
                    Destroy( gameObject );
                }
                else
                {
                    DontDestroyOnLoad( this.gameObject );
                }
            }
        }

        private int GetInstanceCount()
        {
            var instances = FindObjectsOfType<T>();
            var count = 0;
            for( int i = 0; i < instances.Length; i++ )
            {
                if( ( instances[ i ] ).instanceId == instanceId )
                    count++;
            }
            return count;
        }

        protected virtual void OnApplicationPause( bool pauseStatus )
        {
            if( !pauseStatus )
                RefreshTime();
        }
    }
}