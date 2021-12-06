using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.IO;
using UnityEngine.AI;


namespace ExitGames.Demos.DemoAnimator
{
    public class EnemyAI : MonoBehaviourPun, IPunObservable
    {
        //有關AI的功能
        public NavMeshAgent agent;
        public Transform castle;
        public LayerMask whatIsCastle;
        //巡邏
        public Vector3 walkPoint;
        bool walkPointSet;
        public float walkPointRange;
        //攻擊
        bool alreadyAttacked;

        //State
        public float sightRange, attackRange;
        public bool castleInSightRange, castleInAttackRange;

        //倒數計時
        bool startTimer = false;
        double timerIncrementValue;
        double startTime;
        [SerializeField] double timer = 1f;
        ExitGames.Client.Photon.Hashtable CustomeValue;



        //跳躍本身功能
        public float speed;
        public float jumpForce;
        private float moveInput;

        private Rigidbody rb;

        private bool facingRight = true;

        //方塊外型部分
        public GameObject Mesh;

        //生命值
        public float currentHealth;
        public float maxHealth;
        public Image LifeBar;


        //確認玩家有無在地面
        public bool isGrounded;
        public Transform groundCheck;
        public float checkRadius;
        public LayerMask whatIsGround;
        public bool groundGate = true;

        //多餘跳躍次數
        public int extraJumps;
        public int extraJumpValue;
        //重力加乘
        public float Multiplier = 1f;

        //有無翻面
        public bool isFlip = false;
        //射擊
        public Transform shootPos;

        //多人連線
        PhotonView view;


        private void Awake()
        {
            castle = GameObject.Find("Castle").transform;
            agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {
            currentHealth = maxHealth;
            extraJumps = extraJumpValue;
            view = GetComponent<PhotonView>();
            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
            {
                return;
            }

            if (view.IsMine)
            {
                rb.AddForce((Multiplier - 1f) * Physics.gravity, ForceMode.Acceleration);


                Collider[] colliders = Physics.OverlapSphere(groundCheck.position, checkRadius, whatIsGround);
                if (colliders.Length <= 0)
                {
                    isGrounded = false;
                }
                else
                {
                    isGrounded = true;
                }


                //moveInput = Input.GetAxis("Horizontal");
                rb.velocity = new Vector3(moveInput * speed, rb.velocity.y);

                /*
                if (facingRight == false && moveInput > 0)
                {
                    Flip();
                    isFlip = false;
                }
                else if (facingRight == true && moveInput < 0)
                {
                    Flip();
                    isFlip = true;
                }
                */
            }
        }

        private void Update()
        {
            LifeBar.fillAmount = currentHealth / maxHealth;

            if(isGrounded == true)
            {
                agent.enabled = true;
            }
            else
            {
                agent.enabled = false;
            }

            if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
            {
                return;
            }

            if (view.IsMine)
            {
                castleInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsCastle);
                castleInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsCastle);
                Debug.Log("讓AI決定策略");
                if (!castleInSightRange && !castleInAttackRange) Patroling();
                if (castleInSightRange && !castleInAttackRange) ChasePlayer();
                if (castleInSightRange && castleInAttackRange) AttackPlayer();



                if (!startTimer) return;
                timerIncrementValue = PhotonNetwork.Time - startTime;
                if (timerIncrementValue >= timer)
                {
                    //Timer Completed
                    //Do What Ever You What to Do Here
                    ResetAttack();
                }

                /*
                if (groundGate == true)
                {
                    if (isGrounded == true)
                    {
                        extraJumps = extraJumpValue;
                    }
                }
                */
            }
        }

        //巡邏
        private void Patroling()
        {
            if (!walkPointSet) SearchWalkPoint();

            if (walkPointSet)
            {
                agent.SetDestination(walkPoint);
                Debug.Log("追蹤點");
            }
                

            Vector3 distanceToWalkPoint = transform.position - walkPoint;

            //到達walkpoint
            if (distanceToWalkPoint.magnitude < 1f)
                walkPointSet = false;
            
        }
        private void SearchWalkPoint()
        {
            float randomX = Random.Range(-walkPointRange, walkPointRange);
            float randomZ = Random.Range(-walkPointRange, walkPointRange);

            walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z);

            if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
                walkPointSet = true;
        }

        //追逐
        private void ChasePlayer()
        {
            Debug.Log("追蹤人");
            agent.SetDestination(castle.position);
        }
        //攻擊
        private void AttackPlayer()
        {
            Debug.Log("攻擊人");
            agent.SetDestination(transform.position);
            //transform.LookAt(castle);

            if(!alreadyAttacked)
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




        public void GroundGateOn()
        {
            groundGate = true;
        }

        void Flip()
        {
            facingRight = !facingRight;
            Vector3 Scaler = Mesh.transform.localScale;
            Scaler.x *= -1;
            Mesh.transform.localScale = Scaler;

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
            Gizmos.DrawWireSphere(transform.position, sightRange);
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
