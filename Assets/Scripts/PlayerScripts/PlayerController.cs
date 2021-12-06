// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerManager.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in DemoAnimator to deal with the networked player instance
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.IO;

namespace ExitGames.Demos.DemoAnimator
{
    /// <summary>
    /// Player manager.
    /// Handles fire Input and Beams.
    /// </summary>
    /// 
    public class PlayerController : MonoBehaviourPun, IPunObservable
    {
        //跳躍本身功能
        public float speed;
        public float jumpForce;
        private float moveInput;

        private Rigidbody rb;

        private bool facingRight = true;
        
        //方塊外型部分
        public GameObject Mesh;

        //找到PlayerManager
        public PlayerManager playerManager;

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
        public Text nameText;

        //camera
        public bool IsFiring;

        public void Awake()
        {
            if(photonView.IsMine)
            {
                nameText.text = PhotonNetwork.NickName;
            }
            else
            {
                nameText.text = photonView.Owner.NickName;
            }
        }

        public void Start()
        {
            CameraWork _cameraWork = GetComponent<CameraWork>();
            currentHealth = maxHealth;

            if (_cameraWork != null && photonView.IsMine)
            {
                _cameraWork.OnStartFollowing();
            }
            else
            {
                //Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on PlayerController.", this);
            }


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


                moveInput = Input.GetAxis("Horizontal");
                rb.velocity = new Vector3(moveInput * speed, rb.velocity.y);

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
            }
        }

        private void Update()
        {
            LifeBar.fillAmount = currentHealth / maxHealth;

            if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
            {
                return;
            }

            if (view.IsMine)
            {
                if(groundGate == true)
                {
                    if (isGrounded == true)
                    {
                        extraJumps = extraJumpValue;
                    }
                }


                if (Input.GetButtonDown("Jump") && extraJumps > 0)
                {
                    if (isGrounded == true)
                    {
                        groundGate = false;
                        Invoke("GroundGateOn", 0.2f);
                    }
                    rb.velocity = Vector3.up * jumpForce;
                    extraJumps--;
                }

                /*
                else if (Input.GetKeyDown(KeyCode.UpArrow) && extraJumps == 0 && isGrounded == true)
                {
                    rb.velocity = Vector3.up * jumpForce;
                }
                */

                if(Input.GetButtonDown("Fire1"))
                {
                    Shoot();
                }
    

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

        /*
        public void Shoot()
        {
            photonView.RPC("RPC_Shoot",RpcTarget.All);
        }
        */

        
        public void Shoot()
        {
            GameObject bullet = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "bulletPrefabs"), shootPos.position, Quaternion.identity);
            if(isFlip == true)
            {
                //bullet.GetComponent<PhotonView>().RPC("RPC_BulletStart", RpcTarget.AllBuffered);
                bullet.GetComponent<PhotonView>().RPC("RPC_ChangeDirection", RpcTarget.AllBuffered);

            }
            //bullet.GetComponent<Rigidbody>().AddForce(shootPos.right * 16f, ForceMode.Impulse);
        }





        public void OnCollisionEnter(Collision collision)
        {
            if(collision.gameObject.tag == "DeadZone")
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

        /*
        [PunRPC]
        public void RPC_TakeDamage(float damage)
        {
            
            if(!photonView.IsMine)
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
        */

        void Die()
        {
            playerManager.Die();
        }





        #region IPunObservable的实现
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {   //我们把我们的数据IsFiring发送给其他人
                stream.SendNext(IsFiring);
                stream.SendNext(currentHealth);
            }
            else
            {   //接收数据
                this.IsFiring = (bool)stream.ReceiveNext();
                this.currentHealth = (float)stream.ReceiveNext();
            }
        }
        #endregion
    }

}


