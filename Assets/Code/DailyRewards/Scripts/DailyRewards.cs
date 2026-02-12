using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NiobiumStudios
{
    /**
    * Daily Rewards simplificado apenas para controle de tempo (Idle Production) se quiser reativar rewards procure codigo completo em projetos antigos
    **/
    public class DailyRewards: DailyRewardsCore<DailyRewards>
    {
        // Removemos todo o lixo de sprites e textos

        public delegate void OnTimerInitialized();
        public OnTimerInitialized onTimerInitialized;

        // Chamado pelo seu Manager ou Start do jogo
        public void StartIt()
        {
            StartCoroutine( InitializeTimer() );
        }

        private IEnumerator InitializeTimer()
        {
            // O 'now' é herdado do DailyRewardsCore e inicializado aqui
            yield return StartCoroutine( base.InitializeDate() );

            if( !base.isErrorConnect )
            {
                Debug.Log( "<color=green>Timer de Idle Inicializado. Hora atual: " + now + "</color>" );

                if( onInitialize != null )
                    onInitialize();

                if( onTimerInitialized != null )
                    onTimerInitialized();
            }
            else
            {
                // Fallback automático já acontece no Core, mas avisamos aqui
                Debug.LogWarning( "Timer inicializado com hora local (sem internet)." );
            }
        }

        // Função que você vai usar para calcular o ganho Idle
        // Ela retorna o 'now' que está sempre sendo atualizado pelo TickTime() do Core
        public DateTime GetNow()
        {
            return now;
        }

        // Se você precisar saber quantos segundos se passaram desde o último save
        public double GetSecondsSince( DateTime lastSaveTime )
        {
            return ( now - lastSaveTime ).TotalSeconds;
        }

        protected override void Awake()
        {
            base.Awake(); // Mantém o DontDestroyOnLoad e Singleton
        }

        // Removemos CheckRewards, UpdateDailyReward e CollectRewardButtonCallBack
        // pois eram parte do sistema de calendário que você não quer.
    }
}