//Ways of firing and syncing health:
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FireControl : NetworkBehaviour {

    public GameObject bulletPrefab;
    public GameObject bulletSpawn;

	void Start () {
		
	}
	
	void Update () {

        if (!isLocalPlayer)
        {
            return; //if it's not the character itself, don't run any code
        }

        if (Input.GetKeyDown("space"))
        {
            CmdShoot();
        }
	}
    void CreateBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
        bullet.GetComponent<Rigidbody>().velocity = bulletSpawn.transform.forward * 50;
        Destroy(bullet, 6.0f);
    }
    [ClientRpc]
    void RpcCreateBullet()
    {
        if (!isServer)
        {
            CreateBullet();
        }
    }
    [Command]//special function that runs only on the server - must start with Cmd, must notify clients if nescessary through network manager registration
    void CmdShoot()
    {
        CreateBullet(); //server will create bullet, now we tell cleints to create and handle their own version of the bullet
        RpcCreateBullet(); //pushes command out to all clients, they create a bullet for this particular tank
        /*
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
        //bullet.GetComponent<Rigidbody>().AddForce(bulletSpawn.transform.forward * 2000);
        bullet.GetComponent<Rigidbody>().velocity = bulletSpawn.transform.forward * 50;
        NetworkServer.Spawn(bullet); //we spawn instantiated prefab after instantiating on the server on all clients, we need the force to go with it so we add velocity before network spawn
        Destroy(bullet, 6.0f);
        */
    }
}

public class Health : NetworkBehaviour {

    public const int maxHealth = 100;

    [SyncVar(hook = "OnChangeHealth")]
    public int currentHealth = maxHealth;

    public RectTransform healthBar;

    public void TakeDamage(int amount)
    {
        if (!isServer)
            return;
        
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Debug.Log("Dead!");
        }
    }

    void OnChangeHealth (int health)
    {
        healthBar.sizeDelta = new Vector2(health, healthBar.sizeDelta.y);
    }
}

public class LocalAnimationControl : NetworkBehaviour {

    public Animator anim;
    public GameObject[] animationBodyParts;
    public Material invisible;

    [SyncVar(hook = "OnAnimationsStateChange")]
    public string animState = "idle";

    void OnAnimationsStateChange(string aString)
    {
        if (isLocalPlayer) return;
        UpdateAnimation(aString);
    }

    void UpdateAnimation(string aString)
    {
        if (animState == aString) return;
        animState = aString;

        if(animState == "idle")
        {
            anim.SetBool("Idle", true);
        }
        if(animState == "run")
        {
            anim.SetBool("Idle", false);
        }
    }

    [Command]
    public void CmdUpdateAnim(string aString)
    {
        UpdateAnimation(aString);
    }
	void Start () {
        anim = GetComponentInChildren<Animator>();
        anim.SetBool("Idle", true);
        if (!isLocalPlayer)
        {
            GetComponent<OVRPlayerController>().enabled = false;
        }

        if (isLocalPlayer)
        { 
            foreach(GameObject g in animationBodyParts)
            {
                SkinnedMeshRenderer[] m = g.GetComponentsInChildren<SkinnedMeshRenderer>();
                Renderer[] r = g.GetComponentsInChildren<Renderer>();
                foreach(SkinnedMeshRenderer matRend in m)
                {
                    matRend.material = invisible;
                }
                foreach(Renderer rend in r)
                {
                    rend.material = invisible;
                }
            }
        }
	}
}
public class LocalPlayer : NetworkBehaviour {

    public GameObject OvrCam;
    public Camera camL;
    public Camera camR;
    public Vector3 pos;
    public Transform rightHandAnchor;
    public Transform leftHandAnchor;
    public float speed = 5;
    public Animator anim;
    public Transform spawnPos;
    public GameObject prefabInstance;
    public GameObject projectilePrefab;
    public Inventory inventory;

    void Start () {
        if (isLocalPlayer)
        {
            inventory = GetComponentInChildren<Inventory>();
        }
        anim = GetComponentInChildren<Animator>();
        pos = transform.position;
    }

    [Command]
    public void CmdFireProjectile()
    {
        if (NetworkServer.active)
        {
            prefabInstance = Instantiate(projectilePrefab, spawnPos.position, spawnPos.transform.rotation);
            prefabInstance.GetComponent<Rigidbody>().velocity = spawnPos.transform.forward * 2;
            NetworkServer.Spawn(prefabInstance);
        }
    }
    public void Fire(float fireRate)
    {
        
        CmdFireProjectile();
    }
    void Update () {
        if (!isLocalPlayer)
        {
            if(OvrCam != null) { 
            Destroy(OvrCam);
            }
        }
        else
        {
            if (camL != null)
            {
                if (camL.tag != "MainCamera")
                {
                    camL.tag = "MainCamera";
                    camL.enabled = true;
                }
                if (camR.tag != "MainCamera")
                {
                    camR.tag = "MainCamera";
                    camR.enabled = true;
                }
            }

            if (OVRInput.Get(OVRInput.Button.One))
            {
                Fire(1);
            }
            if (OVRInput.Get(OVRInput.Button.Two))
            {
                if (!inventory.inventoryShown)
                {
                    Debug.Log("Showing inventory");
                    inventory.inventoryShown = true;
                    inventory.showInventory();
                }
            }
            if (OVRInput.Get(OVRInput.Button.Three))
            {
                Fire(1);
            }
            if (OVRInput.Get(OVRInput.Button.Four))
            {
                if (inventory.inventoryShown)
                {
                    Debug.Log("Hiding inventory");
                    inventory.inventoryShown = false;
                    inventory.hideInventory();
                }
            }

            //handle animations
            if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x != 0 || OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).y != 0)
            {
                
                
                anim.SetBool("Idle", false);
                GetComponent<LocalAnimationControl>().CmdUpdateAnim("run");
            }
            else
            {
                anim.SetBool("Idle", true);
                GetComponent<LocalAnimationControl>().CmdUpdateAnim("idle");

            }

            //"hands" need to put in hand models and animations, but this is how to get the position:
            leftHandAnchor.localRotation = InputTracking.GetLocalRotation(Node.LeftHand);
            rightHandAnchor.localRotation = InputTracking.GetLocalRotation(Node.RightHand);
            leftHandAnchor.localPosition = InputTracking.GetLocalPosition(Node.LeftHand);
            rightHandAnchor.localPosition = InputTracking.GetLocalPosition(Node.RightHand);

            //If we want to pull from OVRinput
            Vector2 primaryAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

            if (primaryAxis.y > 0.0f)
                pos += (primaryAxis.y * transform.forward * Time.deltaTime);

            if (primaryAxis.y < 0.0f)
                pos += (Mathf.Abs(primaryAxis.y) * -transform.forward * Time.deltaTime); 

            if (primaryAxis.x < 0.0f)
                pos += (Mathf.Abs(primaryAxis.x) * -transform.right * Time.deltaTime);

            if (primaryAxis.x > 0.0f)
                pos += (primaryAxis.x * transform.right * Time.deltaTime);
            
            transform.position = pos;

            Vector3 euler = transform.rotation.eulerAngles;
            Vector2 secondaryAxis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
            euler.y += secondaryAxis.x;

            transform.rotation = Quaternion.Euler(euler);

            transform.localRotation = Quaternion.Euler(euler);

        }
    }
}

public class CustomNetworkMenu : MonoBehaviour {

    public GameObject[] playerPrefabs;

    public void SelectPlayer(int i)
    {
        if(playerPrefabs != null && playerPrefabs[i] != null)
        {
            NetworkManager.singleton.playerPrefab = playerPrefabs[i];
        }
    }
    public void SetupDisconnetMenu()
    {
        GameObject.Find("ButtonDisconnect").GetComponent<Button>().onClick.RemoveAllListeners();
        GameObject.Find("ButtonDisconnect").GetComponent<Button>().onClick.AddListener(StopHostAndClient);

    }
    public void StopHostAndClient()
    {
        NetworkManager.singleton.StopClient();
        NetworkManager.singleton.StopHost();
    }
    public void SetupMenu()
    {
        GameObject.Find("ButtonStartHost").GetComponent<Button>().onClick.RemoveAllListeners();
        GameObject.Find("ButtonStartHost").GetComponent<Button>().onClick.AddListener(StartServer);
        GameObject.Find("DropdownPlayerSelect").GetComponent<Dropdown>().onValueChanged.RemoveAllListeners();
        GameObject.Find("DropdownPlayerSelect").GetComponent<Dropdown>().onValueChanged.AddListener(SelectPlayer);
        GameObject.Find("ButtonJoinClient").GetComponent<Button>().onClick.RemoveAllListeners();
        GameObject.Find("ButtonJoinClient").GetComponent<Button>().onClick.AddListener(JoinClient);

    }
    public void StartServer()
    {
        NetworkManager.singleton.StartHost();
    }
    public void SceneChange(){
        bool someCondition = false;
        if (someCondition){
            
            //NetworkManager.singleton.SceneChange("newSceneName");
        }
    }
    public void JoinClient()
    {
        string ip = GameObject.Find("TextIP").GetComponent<Text>().text;
        string port = GameObject.Find("TextPort").GetComponent<Text>().text;
        if(ip.Length > 0)
        {
            NetworkManager.singleton.networkAddress = ip;
        }
        if(port.Length > 0)
        {
            int x;
            int.TryParse(port, out x);
            NetworkManager.singleton.networkPort = x;
        }
        Debug.Log("index of player prefabs = " + GameObject.Find("DropdownPlayerSelect").GetComponent<Dropdown>().value);
        NetworkManager.singleton.playerPrefab = playerPrefabs[GameObject.Find("DropdownPlayerSelect").GetComponent<Dropdown>().value];
        NetworkManager.singleton.StartClient();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "networkLobby")
        {
            SetupMenu();
        }
        else
        {
            SetupDisconnetMenu();
        }
    }
}
