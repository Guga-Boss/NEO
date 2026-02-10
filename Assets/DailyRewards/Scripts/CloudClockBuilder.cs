using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

// atencao: estes arquivos foram removidos para limpar lixo. se precisar procure em sources antigos pre migracao unity 2022
//DailyRewards.cs( Lógica de calendário / dias ).
//TimedRewards.cs( Lógica de baús temporais ).
//IntegrationDailyRewards.cs( Exemplo de código ).
//IntegrationTimedRewards.cs( Exemplo de código ).
//Reward.cs( Definição de prêmios específicos ).

namespace NiobiumStudios
{
    public static class CloudClockBuilder
    {
        public static string errorMessage = String.Empty;
        public static DateTime cloudClockDate;
        public static State currentState;

        public enum State { NotInitialized, Initializing, Initialized, FailedToInitialize };

        public static IEnumerator InitializeCloudClock( List<CloudClock> cloudClockList, int maxRetries )
        {
            currentState = State.Initializing;

            // Tentativa com os servidores da lista
            if( cloudClockList != null )
            {
                foreach( var cloudClock in cloudClockList )
                {
                    using( UnityWebRequest request = UnityWebRequest.Get( cloudClock.url ) )
                    {
                        request.timeout = 3; // Não deixa o jogo preso se a net estiver ruim
                        yield return request.SendWebRequest();

                        if( request.result == UnityWebRequest.Result.Success )
                        {
                            if( ProcessarResposta( request.downloadHandler.text ) )
                            {
                                currentState = State.Initialized;
                                yield break;
                            }
                        }
                    }
                }
            }

            // Se chegou aqui, deu erro de rede ou timeout, mas NÃO TRAVAMOS.
            UsarHoraLocal();
        }

        private static bool ProcessarResposta( string json )
        {
            try
            {
                // Procuramos a chave "datetime" manualmente na string para evitar erro de Parser
                // O formato da WorldTimeAPI é: "datetime":"2023-10-27T..."
                string chave = "\"datetime\":\"";
                int inicio = json.IndexOf(chave);

                if( inicio != -1 )
                {
                    inicio += chave.Length;
                    int fim = json.IndexOf("\"", inicio);
                    string dataExtraida = json.Substring(inicio, fim - inicio);

                    // Tenta converter a string extraída
                    if( DateTime.TryParse( dataExtraida, null, System.Globalization.DateTimeStyles.RoundtripKind, out cloudClockDate ) )
                    {
                        // Adicionamos os segundos locais para precisão
                        cloudClockDate = cloudClockDate.AddSeconds( DateTime.Now.Second );
                        Debug.Log( "<color=green>CloudClock: Sincronizado com sucesso!</color>" );
                        return true;
                    }
                }
                return false;
            }
            catch( Exception e )
            {
                Debug.LogWarning( "Erro ao processar JSON manualmente: " + e.Message );
                return false;
            }
        }

        private static void UsarHoraLocal()
        {
            cloudClockDate = DateTime.Now;
            currentState = State.Initialized; // Marcamos como Initialized para o DailyRewardsCore liberar o jogo
            Debug.Log( "<color=yellow>CloudClock: Usando hora local (Offline ou Timeout).</color>" );
        }
    }
}