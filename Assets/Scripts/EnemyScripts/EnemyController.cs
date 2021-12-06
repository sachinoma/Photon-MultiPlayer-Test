using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine.UI;
using System.IO;

namespace ExitGames.Demos.DemoAnimator
{
    public class EnemyController : MonoBehaviourPun, IPunObservable
    {
        public string currentState = "IdleState";
        private Transform target;
        //public GameObject playerTransform;
        public float chaseRange = 7;
        public float attackRange = 3;
        public float speed = 3;
        public GameObject meshObject;


        public Animator animator;

        //生命值
        public float currentHealth;
        public float maxHealth;
        public Image LifeBar;

        //攻擊
        bool alreadyAttacked;
        //有無翻面
        public bool isFlip = false;
        //射擊
        public Transform shootPos;
        //倒數計時
        bool startTimer = false;
        double timerIncrementValue;
        double startTime;
        [SerializeField] double timer = 1f;
        ExitGames.Client.Photon.Hashtable CustomeValue;


        //多人連線
        PhotonView view;

        public Transform ClosestPlayer;

        

        // Start is called before the first frame update
        void Start()
        {
            currentHealth = maxHealth;
            view = GetComponent<PhotonView>();
        }

        // Update is called once per frame
        void Update()
        {
            LifeBar.fillAmount = currentHealth / maxHealth;
            OnGetPlayer();
            target = ClosestPlayer.transform;

            float distance = Vector3.Distance(transform.position, target.position);
            if (currentState == "IdleState")
            {
                if (distance < chaseRange)
                {
                    currentState = "ChaseState";
                }
            }
            else if (currentState == "ChaseState")
            {
                /*
                //放置動畫
                animator.SetTrigger("chase");
                animator.SetBool("isAttacking", false);
                */


                if (distance < attackRange)
                {
                    currentState = "AttackState";
                }
                if (distance > chaseRange)
                {
                    currentState = "IdleState";
                }


                //移動去找Player
                if (target.position.x > transform.position.x)
                {
                    //往右
                    transform.Translate(-transform.right * speed * Time.deltaTime);
                    meshObject.transform.rotation = Quaternion.identity;
                    isFlip = false;
                }
                else
                {
                    //往左
                    transform.Translate(transform.right * speed * Time.deltaTime);
                    meshObject.transform.rotation = Quaternion.Euler(0, 180, 0);
                    isFlip = true;
                }
            }
            else if (currentState == "AttackState")
            {
                //animator.SetBool("isAttacking", true);
                AttackPlayer();

                if (distance > attackRange)
                {
                    currentState = "ChaseState";
                }

                //移動去找Player
                if (target.position.x > transform.position.x)
                {
                    //往右
                    //transform.Translate(-transform.right * speed * Time.deltaTime);
                    meshObject.transform.rotation = Quaternion.identity;
                    isFlip = false;
                }
                else
                {
                    //往左
                    //transform.Translate(transform.right * speed * Time.deltaTime);
                    meshObject.transform.rotation = Quaternion.Euler(0, 180, 0);
                    isFlip = true;
                }
            }

            if (view.IsMine)
            {
                if (!startTimer) return;
                timerIncrementValue = PhotonNetwork.Time - startTime;
                if (timerIncrementValue >= timer)
                {
                    //Timer Completed
                    //Do What Ever You What to Do Here
                    ResetAttack();
                }
            }

        }

        //找最近的玩家
        public void OnGetPlayer()
        {
            GameObject[] enemy = GameObject.FindGameObjectsWithTag("Player");
            float distance_min = 10000;
            float distance = 0;
            int id = 0;

            for (int i = 0; i < enemy.Length; i++)
            {
                if (enemy[i].activeSelf == true)
                {
                    distance = Vector3.Distance(transform.position, enemy[i].transform.position);
                    if (distance < distance_min)
                    {
                        distance_min = distance;
                        id = i;
                    }

                }
            }
            ClosestPlayer = enemy[id].transform;
        }


        //攻擊
        private void AttackPlayer()
        {
            Debug.Log("攻擊人");
            //transform.LookAt(castle);

            if (!alreadyAttacked)
            {
                //攻擊指令打這
                Shoot();

                //
                alreadyAttacked = true;
                AttackTimer();
            }
        }
        public void ResetAttack()
        {
            alreadyAttacked = false;
        }
        public void AttackTimer()
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
        public void Shoot()
        {
            GameObject bullet = PhotonNetwork.Instantiate(Path.Combine("EnemyPrefabs", "enemyBulletPrefabs"), shootPos.position, Quaternion.identity);
            if (isFlip == true)
            {
                //bullet.GetComponent<PhotonView>().RPC("RPC_BulletStart", RpcTarget.AllBuffered);
                bullet.GetComponent<PhotonView>().RPC("RPC_ChangeDirection", RpcTarget.AllBuffered);

            }
            //bullet.GetComponent<Rigidbody>().AddForce(shootPos.right * 16f, ForceMode.Impulse);
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "DeadZone")
            {
                TakeDamage(100f);
            }
        }

        public void TakeDamage(float damage)
        {
            //photonView.RPC("RPC_TakeDamage", RpcTarget.All, damage);
            Debug.Log("受傷 : " + damage);
            if (!photonView.IsMine)
            {
                return;
            }

            //被打的人繼續執行
            Debug.Log("我被打");
            currentHealth = currentHealth - damage;
            if (currentHealth <= 0f)
            {
                Die();
            }
        }

        void Die()
        {
            PhotonNetwork.Destroy(this.gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, chaseRange);
        }

        #region IPunObservable的实现
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {   //我们把我们的数据发送给其他人
                stream.SendNext(currentHealth);
            }
            else
            {   //接收数据
                this.currentHealth = (float)stream.ReceiveNext();
            }
        }
        #endregion
    }
}
