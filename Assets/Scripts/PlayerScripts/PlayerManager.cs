using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Pun;
using System.IO;

namespace ExitGames.Demos.DemoAnimator
{
    public class PlayerManager : MonoBehaviourPun
    {

        // [Tooltip("玩家的PlayerManager")]
        public string playerName;
        public GameObject playerPrefab;

        public GameObject controller;

        public Transform SpawnTransform;


        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        void Start()
        {
            SpawnTransform = GameObject.Find("SpawnPos").transform;

            if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
            {
                return;
            }

            /*
            if (PlayerManager.LocalPlayerInstance == null)
            {
                // Debug.Log("動態生成玩家角色" + SceneManagerHelper.ActiveSceneName);

                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(UnityEngine.Random.Range(-2f, 7f), 6f, 0f), Quaternion.identity, 0);
            }
            else
            {

                // Debug.Log("無視場景Loaging " + SceneManagerHelper.ActiveSceneName);
            }
            */

            CreateController();

            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.IsMasterClient); // called before OnPhotonPlayerDisconnected
                //LoadArena();

            }
        }


        void Update()
        {

        }

        public void CreateController()
        {
            Debug.Log("生成PlayerController");
            Transform spawnPoint = SpawnTransform;
            controller = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnPoint.position, spawnPoint.rotation);
            controller.GetComponent<PlayerController>().playerManager = this;
        }

        public void Die()
        {
            PhotonNetwork.Destroy(controller);
            CreateController();
        }



        /*
        public void RespawnPlayer()
        {
            if (PlayerManager.LocalPlayerInstance == null)
            {
                // Debug.Log("動態生成玩家角色" + SceneManagerHelper.ActiveSceneName);

                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(UnityEngine.Random.Range(-2f, 7f), 6f, 0f), Quaternion.identity, 0);
            }
        }
        */
    }
}

