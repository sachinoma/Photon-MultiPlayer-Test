using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine.UI;
using System.IO;

namespace ExitGames.Demos.DemoAnimator
{
    public class EnemyManager : MonoBehaviourPun
    {


        public GameObject enemyPrefab;
        public GameObject controller;

        public Transform SpawnTransform;


        //倒數計時
        bool startTimer = false;
        double timerIncrementValue;
        double startTime;
        [SerializeField] double timer = 20;
        ExitGames.Client.Photon.Hashtable CustomeValue;


        private void Start()
        {
            SpawnTransform = GameObject.Find("EnemySpawnPos").transform;
            if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
            {
                return;
            }

            //CreateController();
            Timer();
        }

        public void Timer()
        {

            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                CustomeValue = new ExitGames.Client.Photon.Hashtable();
                startTime = PhotonNetwork.Time;
                startTimer = true;
                CustomeValue.Add("StartTime", startTime);
                //PhotonNetwork.room.SetCustomProperties(CustomeValue);
            }
            else
            {
                //startTime = double.Parse(PhotonNetwork.room.CustomProperties["StartTime"].ToString());
                startTimer = true;
            }
        }



        void Update()
        {
            if (!startTimer) return;
            timerIncrementValue = PhotonNetwork.Time - startTime;
            if (timerIncrementValue >= timer)
            {
                //Timer Completed
                //Do What Ever You What to Do Here
                CreateController();
                Timer();
            }
        }


        public void CreateController()
        {
            Debug.Log("生成PlayerController");
            Transform spawnPoint = SpawnTransform;
            controller = PhotonNetwork.Instantiate(Path.Combine("EnemyPrefabs", "Enemy2.5DController"), spawnPoint.position, spawnPoint.rotation);
            //controller.GetComponent<PlayerController>().playerManager = this;
        }
    }
}
