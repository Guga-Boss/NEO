/***************************************************************************\
Project:      Daily Rewards
Copyright (c) Niobium Studios.
Author:       Guilherme Nunes Barbosa (gnunesb@gmail.com)
\***************************************************************************/
using UnityEngine;
using UnityEngine.UI;

/* 
 * Timed Reward Object UI representation
 */
namespace NiobiumStudios
{
    /** 
     * The UI Representation of a Timed Reward.
     **/
    public class TimedRewardUI : MonoBehaviour
    {
        [Header("UI Elements")]
        public Text textReward;             // The Text containing the Reward amount
        public Text textUnit;               // The Text containing the Reward unit
        public Image imageReward;           // The Reward Image
        public Button button;               // The claim Button

        [Header("Internal")]
        public int index;

        [HideInInspector]
        public Reward reward;

        public void Initialize()
        {
            textUnit.text = reward.Item.ToString();
            if (reward.RewardAmt > 0)
            {
                textReward.text = reward.RewardAmt.ToString();
            }

            //imageReward.sprite = reward.sprite;
        }
    }
}