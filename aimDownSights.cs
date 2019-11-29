using UnityEngine;
using System.Collections;
//using UnityEngine.Networking;

public class aimDownSights : MonoBehaviour
{
    public Vector3 aimPos;
    public Vector3 hipPos;
    public Vector3 normalPos;
    public Vector3 recoilPos;
    public Vector3 hipFireRecoilPos;
    public GameObject _gun;
    //public Player player;

    public bool isAimed = false;
    //private bool isLocal = false;
    //public int maxAmmo = 2;
    // public int currentAmmo;
    //public float reloadTime = 2f;
    //private bool isReloading = false;
    //public bool isFiring = false;
    //public Animator animator;

    
    
    //public float aimSpeed = 30f;
    public float damage = 10f;
    public float range = 100f;
   // public Camera fpsCam;
    //public Camera zoomCam;
    public float impactForce = 30f;
    //public GameObject Bullet;
    public GameObject bulletEmitter;
    public float Bullet_Forward_Force;
    //public GameObject muzzleFlash;
    //public GameObject bulletImpact;
    
    //public GameObject redDot;
    //public int gunDamage = 1;
    public float fireRate = 0.25f;
    private float nextFire;
    //public GameObject bulletHole;
    
    public int zoom = 30;
    public int normal = 60;
    public float smooth = 5;
    private bool zoomed= false;

    
   
    void Start()
    {
        //if(currentAmmo == -1)
        //    currentAmmo = maxAmmo;
        //muzzleFlash.SetActive(false);
    }

    
    void Update()
    {

        
        normalPos = transform.localPosition;
        


        if  (Input.GetAxis("Fire2") > 0)
        {//AIMDOWNSIGHTS LERP
            //Gun.transform.localPosition = Vector3.Lerp(Gun.transform.localPosition, aimPos, Time.deltaTime * smooth);
            //zoomCam.fieldOfView = Mathf.Lerp(zoomCam.fieldOfView, zoom, Time.deltaTime * smooth);
            isAimed = true;
           

            AimDownSights();
        }
        else
        {//LERP BACK TO HIPFIRE POS
            //zoomCam.fieldOfView = Mathf.Lerp(zoomCam.fieldOfView, normal, Time.deltaTime * smooth);
           transform.localPosition = Vector3.Lerp(transform.localPosition, hipPos, Time.deltaTime * smooth);
            isAimed = false;
            
        }

        
        if (Input.GetAxis("Fire1") > 0 && Time.time > nextFire && isAimed == true)
        {//AIMDOWNSIGHTS RECOIL LERP
            nextFire = Time.time + fireRate;
            //transform.localPosition = Vector3.Lerp(transform.localPosition, recoilPos, Time.deltaTime * smooth);
            Shoot();
           // Recoil();
            //GetComponent<AudioSource>().Play();
            
        }
        
        if (Input.GetAxis("Fire1") > 0 && Time.time > nextFire && isAimed == false)
        {//HIPFIRERECOIL LERP
            nextFire = Time.time + fireRate;
            //transform.localPosition = Vector3.Lerp(transform.localPosition, hipFireRecoilPos, Time.deltaTime * smooth);
            Shoot();
            //HipFireRecoil();
            //GetComponent<AudioSource>().Play();
            
        }

        else 
        {
            //muzzleFlash.SetActive(false);

        }
      

    }
    
    //void Recoil()
    //{
    //    transform.localPosition = Vector3.Lerp(transform.localPosition, recoilPos, Time.deltaTime * smooth);
    //}
    
    //void HipFireRecoil()
    //{
    //    transform.localPosition = Vector3.Lerp(transform.localPosition, hipFireRecoilPos, Time.deltaTime * smooth);
    //}

    
    void AimDownSights()
    {
        //if (!isLocalPlayer)
        //{
        //    return;
        //}

        transform.localPosition = Vector3.Lerp(transform.localPosition, aimPos, Time.deltaTime * smooth);
    }
   
    void Shoot()
    {

        //if(!isLocalPlayer)
        //{
        //    return;
        //}

        //muzzleFlash.SetActive(true);
        RaycastHit hit;

        // //trace bulletEmitter raycast
        var forward = bulletEmitter.transform.TransformDirection(Vector3.forward);
        

        // //working 12/28
        // //RAYCAST METHOD
        if (Physics.Raycast(bulletEmitter.transform.position, forward * 30, hitInfo: out hit, maxDistance: range))
        {
            //BULLET HOLE
            //GameObject bulletHoleHandler;
            //GameObject impactHandler;
            //var player = hit.transform.GetComponentInChildren<Player>();
            
            //if (!isLocalPlayer)
            //{
            //    return;
            //}

            Debug.DrawRay(bulletEmitter.transform.position, forward * 30, Color.red);
            var hitRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            
            //impactHandler = Instantiate(bulletImpact, hit.point, hitRotation);
            
            //bulletHoleHandler = Instantiate(bulletHole, hit.point, hitRotation);
            
            //if (hit.transform.name == "Enemy")
            //{

            //    Destroy(bulletHoleHandler);
            //    Debug.Log("enemy hit");
            //}

            
            if (hit.transform.CompareTag("Player"))
            {
                Player enemy = hit.transform.GetComponentInChildren<Player>();
                if(enemy != null)
                {
                    //Debug.Log("enemy hit");
                    //enemy.Die();
                    
                }


                //player.Die();
               

                


                
                //Debug.Log("HIT");
            }




            // Destroy(impactHandler, 5f);
            //Destroy(bulletHoleHandler, 5f);



            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * impactForce);
            }       




            ////PROJECTILE METHOD - not for use on server
            //GameObject tempBulletHandler;

            //tempBulletHandler = Instantiate(Bullet, bulletEmitter.transform.position, bulletEmitter.transform.rotation) as GameObject;

            //tempBulletHandler.transform.Rotate(Vector3.forward);

            //Rigidbody tempRigidBody;
            //tempRigidBody = tempBulletHandler.GetComponent<Rigidbody>();

            //tempRigidBody.AddForce(forward * Bullet_Forward_Force);

            //Destroy(tempBulletHandler, 2f);
            //Debug.Log(tempBulletHandler.transform.name);
        }









        //private static RaycastHit bulletHit(RaycastHit hit)
        //{

        //    Debug.Log(hit.transform.name);
        //    return hit;
        //}
    }
}
