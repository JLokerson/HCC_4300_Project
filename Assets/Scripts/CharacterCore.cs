using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(CharacterController))]
public class CharacterCore : MonoBehaviour
{
    [Tooltip("Used to modify the speed the player moves")]
    public float MovementSpeed = 5;
    private InputAction moveAction;
    private CharacterController controller;

    //shooting
    [Tooltip("The bullet prefab to shoot")]
    public GameObject bulletPrefab;
    [Tooltip("The reticle object to aim with")]
    public GameObject reticle;
    [Tooltip("The time between shots")]
    public Material normalReticle = null;
    public Material shotReticle = null;
    public Material reloadReticle = null;
    [Tooltip("The time between shots")]
    public float fireRate = 0.5f;
    public float reloadTime = 2f;
    public int MaxBulletCount = 5;

    [Tooltip("The spread of the bullets when fired, between 0 and 1. 0 means no spread")]
    [Range(0, 1)]
    public float bulletSpread = .2f;

    private InputAction shootAction;
    private InputAction reloadAction;
    private Renderer reticleRenderer=null;
    private bool isReloading = false;
    private bool isShooting = false;
    private int CurrentBulletCount;

    private TextMeshPro ammoCounter=null;

    private void Start()
    {
        moveAction=InputSystem.actions.FindAction("Move"); //binds the "Move" actions from the Input Actions Asset
        controller = GetComponent<CharacterController>();

        shootAction = InputSystem.actions.FindAction("Attack"); //binds the "Attack" actions from the Input Actions Asset
        reticleRenderer = reticle.GetComponent<Renderer>();

        reloadAction = InputSystem.actions.FindAction("Reload");
        CurrentBulletCount = MaxBulletCount;

        try
        {
            ammoCounter = GameObject.Find("AmmoCounter").GetComponent<TextMeshPro>();
            ammoCounter.text = CurrentBulletCount.ToString() + " / " + MaxBulletCount.ToString();
        }
        catch
        {
            Debug.LogWarning("No AmmoCounter TextMeshPro object found.");
        }

    }

    private void Update()
    {
        //moveAction is a Vector2, which looks like (x,y) with x and y being -1, 0, or 1 depending on the input
        //(movement along an axis. 0 is none, -1 is one direction, and 1 is the other)
        Vector2 moveValue= moveAction.ReadValue<Vector2>(); 
        Vector3 move = new Vector3(moveValue.x, 0, moveValue.y); //now we convert it to a Vector3 for 3D movement. the 0 is since we don't want to move up or down
        controller.Move(move * Time.deltaTime * MovementSpeed); //this is the actual movement

        //shooting
        if(shootAction.IsPressed() && !isReloading &&!isShooting && CurrentBulletCount>0) //if shoot action is pressed (held works too this way) and the time is greater than what is calculated as the next time a shot can be fired and not reloading and has bullets
        {
            StartCoroutine(Shoot()); //start corutine lets us wait for some time without blocking the main thread
        }            
        //reload
        else if((reloadAction.triggered || CurrentBulletCount<=0) && !isReloading) //if reload action triggered manually or out of bullets, reload if not currently
        {            
            StartCoroutine(Reload()); //start corutine lets us wait for some time without blocking the main thread
            
        }
        
    }

    private System.Collections.IEnumerator Shoot() //system.collections.ienumerator lets us use yield return to wait
    {
        reticleRenderer.material = shotReticle;
        CurrentBulletCount--;
        isShooting = true;

        if (bulletPrefab != null)
        {
            GameObject bulletObj=Instantiate(bulletPrefab, transform.position, transform.rotation); //create bullet at player
            Projectile bullet=bulletObj.GetComponent<Projectile>();
            bullet.SetTarget(reticle.transform.position, bulletSpread);

            if(ammoCounter!=null)//update ammo counter display if it exists
            {
                ammoCounter.text = CurrentBulletCount.ToString() + " / " + MaxBulletCount.ToString();
            }
        }
        else
        {
            Debug.LogWarning("Bullet Prefab is not assigned.");
        }
        yield return new WaitForSeconds(fireRate); //this is what actually waits for fire rate time
        isShooting = false;
        if (!isReloading)
        {
            reticleRenderer.material = normalReticle;
        }
    }
    private System.Collections.IEnumerator Reload() //system.collections.ienumerator lets us use yield return to wait
    {
        isReloading = true;
        reticleRenderer.material = reloadReticle;
        if(ammoCounter!=null)//update ammo counter display if it exists
        {
            ammoCounter.text = "Reloading...";
        }

        yield return new WaitForSeconds(reloadTime); //this is what actually waits for reload time

        CurrentBulletCount = MaxBulletCount;
        reticleRenderer.material = normalReticle;
        if (ammoCounter != null)//update ammo counter display if it exists
        {
            ammoCounter.text = CurrentBulletCount.ToString() + " / " + MaxBulletCount.ToString();
        }
        isReloading = false;
    }

}
