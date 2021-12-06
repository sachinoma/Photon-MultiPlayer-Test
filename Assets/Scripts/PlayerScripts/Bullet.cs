using ExitGames.Demos.DemoAnimator;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

namespace ExitGames.Demos.DemoAnimator
{
    public class Bullet : MonoBehaviourPun
    {       
        public float speed = 10f;
        public float destroyTime = 2f;
        public bool shootLeft = false;


        public float damageNum = 25f;

        IEnumerator destroyBullet()
        {
            yield return new WaitForSeconds(destroyTime);
            photonView.RPC("RPC_Destroy", RpcTarget.AllBuffered);
        }

        private void Start()
        {
            StartCoroutine(destroyBullet());
        }


        private void Update()
        {
            if(!shootLeft)
            {
                transform.Translate(Vector3.right * Time.deltaTime * speed);
            }
            else
            {
                transform.Translate(Vector3.left * Time.deltaTime * speed);
            }
            
        }


        [PunRPC]
        public void RPC_Destroy()
        {
            Destroy(this.gameObject);
        }

        [PunRPC]
        public void RPC_ChangeDirection()
        {
            shootLeft = true;
        }


        
        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Enemy")
            {
                other.gameObject.GetComponent<EnemyController>().TakeDamage(damageNum);
                Destroy(gameObject);
            }
        }
        
    }
}
