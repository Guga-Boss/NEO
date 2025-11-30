/***************************************************************************\
Project:      Daily Rewards
Copyright (c) Niobium Studios.
Author:       Guilherme Nunes Barbosa (gnunesb@gmail.com)
\***************************************************************************/
using UnityEngine;
using System;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;

namespace NiobiumStudios
{
    /**
    * Daily Rewards keeps track of the player daily rewards based on the time he last selected a reward
    **/
    public class DailyRewards : DailyRewardsCore<DailyRewards>
    {
        public List<Reward> rewards;        // Rewards list 
        public DateTime LastRewardTime;     // The last time the user clicked in a reward
        public int RewardID;                // the last reward the player claimed
        public bool keepOpen = true;        // Keep open even when there are no Rewards available?
        public bool resetPrize = true;      // Reset the prize on next day?
        public bool readyToClaim = false;                  // Update flag
        public List<GameObject> ButtonObjList;
        public GameObject RewardsPanel;
        public tk2dSprite[] ButtonSpriteList;
        public tk2dSprite[] RewardIcon;
        public tk2dSlicedSprite[] RewardBack;
        public tk2dTextMesh[] RewardText;
        public tk2dTextMesh RewardButtonTextMesh;
        public tk2dSprite NextRewardIndicator;
        // Delegates
        public delegate void OnClaimPrize(int day);                 // When the player claims the prize
        public OnClaimPrize onClaimPrize;
        public const string FMT = "O";

        public TimeSpan DebugTime;         // For debug purposes only

        public void StartIt()
        {
            // Initializes the timer with the current time
            StartCoroutine(InitializeTimer());
        }

        private IEnumerator InitializeTimer()
        {
            yield return StartCoroutine(base.InitializeDate());

            if (base.isErrorConnect) 
			{
                if (onInitialize != null)
                    onInitialize(true, base.errorMessage);
			}
            else 
			{
                Load( true, false, false );
                CheckRewards();

                if(onInitialize!=null)
                    onInitialize();
			}
        }

        protected override void OnApplicationPause(bool pauseStatus)
        {
            base.OnApplicationPause(pauseStatus);
            CheckRewards();
        }

        public TimeSpan GetTimeDifference()
        {
            TimeSpan difference = (LastRewardTime - now);
            difference = difference.Subtract(DebugTime);
            return difference.Add(new TimeSpan(0, 24, 0, 0));
        }

        // Check if the player have unclaimed prizes
        public string lastClaimedTimeStr = "";
        public void CheckRewards()
        {
            lastClaimedTimeStr = "";

            Load( false, true, true );
            // It is not the first time the user claimed.
            // We need to know if he can claim another reward or not
            if (!string.IsNullOrEmpty(lastClaimedTimeStr))
            {
                LastRewardTime = DateTime.ParseExact(lastClaimedTimeStr, FMT, CultureInfo.InvariantCulture);

                // if Debug time was added, we use it to check the difference
                DateTime advancedTime = now.AddHours(DebugTime.TotalHours);

                TimeSpan diff = advancedTime - LastRewardTime;

                int days = (int)(Math.Abs(diff.TotalHours) / 24);
                if (days == 0)
                {
                    // No claim for you. Try tomorrow
                    return;
                }

                // The player can only claim if he logs between the following day and the next.
                if (days >= 1 && days < 2)
                {
                    // If reached the last reward, resets to the first restarting the cicle
                    if (RewardID == rewards.Count)
                    {
                        RewardID = 0;
                        return;
                    }
                    return;
                }

                if (days >= 2 ) // && resetPrize)    changed to avoid not reseting bug
                {
                    // The player loses the following day reward and resets the prize
                    RewardID = 0;
                }
            }
        }

        // Checks if the player claim the prize and claims it by calling the delegate. Avoids duplicate call
        public void ClaimPrize()
        {
            CheckRewards();
        }

        // Returns the daily Reward of the day
        public Reward GetReward(int day)
        {
            return rewards[day - 1];
        }

        // Resets the Daily Reward for testing purposes
        public void Reset()
        {
            LastRewardTime = now;
            RewardID = 0;
            DebugTime = new TimeSpan();
        }

        public void UpdateDailyReward()
        {
            if( G.Tutorial.CanSave() == false || Item.GetTotalGained( ItemType.Shell ) < 150 )
            {
                RewardsPanel.gameObject.SetActive( false );
            }
            else
                RewardsPanel.gameObject.SetActive( true );       

            RewardsPanel.gameObject.SetActive( false );   // Rewards removed by me
 

            for( int i = 0; i < RewardIcon.Length; i++ )
                RewardIcon[ i ].gameObject.transform.parent.gameObject.SetActive( false );

            for( int i = 0; i < Manager.I.Reward.rewards.Count; i++ )
            {
                int id = ( int ) Manager.I.Reward.rewards[ i ].Item;
                RewardIcon[ i ].spriteId = G.GIT( id ).TKSprite.spriteId;
                RewardText[ i ].text = "x" + Manager.I.Reward.rewards[ i ].RewardAmt;
                RewardIcon[ i ].gameObject.transform.parent.gameObject.SetActive( true );

                RewardBack[ i ].color = Color.white;
                if( i < Manager.I.Reward.RewardID )
                    RewardBack[ i ].color = new Color32( 127, 255, 0, 255 );
            }

            if( NiobiumStudios.CloudClockBuilder.currentState != CloudClockBuilder.State.Initialized )
            {
                RewardButtonTextMesh.text = "Connecting To The Internet...\n(Trial: " + Manager.I.ConnectionRetries + ")";
                return;
            }

            Manager.I.Reward.CheckRewards();

            // Updates the time due
            if( !readyToClaim )
            {
                TimeSpan difference = Manager.I.Reward.GetTimeDifference();

                // If the counter below 0 it means there is a new reward to claim
                if( difference.TotalSeconds <= 0 )
                {
                    readyToClaim = true;
                    RewardButtonTextMesh.text = "Collect Reward!!!";
                    return;
                }
                string formattedTs = Manager.I.Reward.GetFormattedTime( difference );
                RewardButtonTextMesh.text = string.Format( "Come back in {0} \nfor your next reward", formattedTs );
            }

            int rid = Manager.I.Reward.RewardID;
            if( rid < Manager.I.Reward.rewards.Count )
            {
                NextRewardIndicator.transform.position = new Vector3( NextRewardIndicator.transform.position.x,
                RewardBack[ rid ].transform.position.y - -1.5f, NextRewardIndicator.transform.position.z );
            }
        }
        public void CollectRewardButtonCallBack()
        {
            if( readyToClaim == false ) return;
            if( Manager.I.Reward.RewardID >= rewards.Count ) //new: added to prevent bug
                Manager.I.Reward.RewardID = 0;
            int rid = Manager.I.Reward.RewardID;
            ItemType it = Manager.I.Reward.rewards[ rid ].Item;
            int amt = Manager.I.Reward.rewards[ rid ].RewardAmt;
            G.GIT( it ).PreLoadBonus += amt;
            if( ++RewardID > rewards.Count )
                RewardID = 0;

            readyToClaim = false;
            Save();
            MasterAudio.PlaySound3DAtVector3( "Item Collect", G.Hero.Pos );
        }
        
        public void Save()
        {
            return;
            if( G.Tutorial.CanSave() == false ) return;

            string file = Manager.I.GetProfileFolder() + "Player.NEO";
            ES2.Save( RewardID, file + "?tag=Daily Reward " );
            string lastClaimedStr = now.AddHours( DebugTime.TotalHours ).ToString( FMT );
            ES2.Save( lastClaimedStr, file + "?tag=Last Reward Time Key " );
            ES2.Save( ( int ) DebugTime.TotalHours, file + "?tag=Debug Total Hours " );
        }
        public bool Load( bool debugTime, bool availRewards, bool lastrwtime )
        {
            return true;
            if( Manager.I == null ) return false;
            string file = Manager.I.GetProfileFolder() + "Player.NEO";
            if( ES2.Exists( file + "?tag=Daily Reward " ) )
            {
                if( availRewards )
                {
                    RewardID = ES2.Load<int>( file + "?tag=Daily Reward " );
                }
                if( debugTime )
                {
                    int debugHours = ES2.Load<int>( file + "?tag=Debug Total Hours " );
                    DebugTime = new TimeSpan( debugHours, 0, 0 );
                }
                if( lastrwtime )
                {
                    lastClaimedTimeStr = ES2.Load<string>( file + "?tag=Last Reward Time Key " );
                    // It is not the first time the user claimed.
                    // We need to know if he can claim another reward or not
                    if( !string.IsNullOrEmpty( lastClaimedTimeStr ) )
                        LastRewardTime = DateTime.ParseExact( lastClaimedTimeStr, FMT, CultureInfo.InvariantCulture );
                }

                return true;
            }
            else
            {
                Reset();
                Save();
                return false;
            }
        }
    }
}