// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Launcher.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in "PUN Basic tutorial" to handle typical game management requirements
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine.UI;

namespace ExitGames.Demos.DemoAnimator
{
    /// <summary>
    /// Game manager.
    /// Connects and watch Photon Status, Instantiate Player
    /// Deals with quiting the room and the game
    /// Deals with level loading (outside the in room synchronization)
    /// </summary>

    public class GameManager : MonoBehaviourPun
    {
        #region Public Variables

        static public GameManager Instance;

       // [Tooltip("玩家的PlayerManager")]
        public GameObject playerManagerPrefab;

        #endregion

        #region Private Variables

        private GameObject instance;

        #endregion

        private void Start()
        {
            Instance = this;

            if (PlayerManager.LocalPlayerInstance == null)
            {
                // Debug.Log("動態生成玩家角色" + SceneManagerHelper.ActiveSceneName);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                GameObject playerManager = PhotonNetwork.Instantiate(this.playerManagerPrefab.name, new Vector3(0f, 0f, 0f), Quaternion.identity, 0);


            }
            else
            {

                // Debug.Log("無視場景Loaging " + SceneManagerHelper.ActiveSceneName);
            }

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.IsMasterClient); // called before OnPhotonPlayerDisconnected
                //LoadArena();

            }
        }

            void Update()
        {
            // "back" button of phone equals "Escape". quit app if that's pressed
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                //QuitApplication();
            }
        }


    }

}

