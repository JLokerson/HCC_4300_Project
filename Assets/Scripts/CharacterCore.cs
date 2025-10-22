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
    public float fireRate = 0.5f;
    public float reloadTime = 2f;


    private float nextFireTime;
    private InputAction shootAction;
    private InputAction reloadAction;
    private Renderer reticleRenderer=null;
    private bool isReloading = false;

    private void Start()
    {
        moveAction=InputSystem.actions.FindAction("Move"); //binds the "Move" actions from the Input Actions Asset
        controller = GetComponent<CharacterController>();

        shootAction = InputSystem.actions.FindAction("Attack"); //binds the "Attack" actions from the Input Actions Asset
        reticleRenderer = reticle.GetComponent<Renderer>();

        reloadAction = InputSystem.actions.FindAction("Reload");
    }

    private void Update()
    {
        //moveAction is a Vector2, which looks like (x,y) with x and y being -1, 0, or 1 depending on the input
        //(movement along an axis. 0 is none, -1 is one direction, and 1 is the other)
        Vector2 moveValue= moveAction.ReadValue<Vector2>(); 
        Vector3 move = new Vector3(moveValue.x, 0, moveValue.y); //now we convert it to a Vector3 for 3D movement. the 0 is since we don't want to move up or down
        controller.Move(move * Time.deltaTime * MovementSpeed); //this is the actual movement

        //shooting
        if(shootAction.triggered && Time.time >= nextFireTime && !isReloading) //if shoot action triggered and the time is greater than what is calculated as the next time a shot can be fired and not reloading
        {
            Shoot();            
            nextFireTime = Time.time + fireRate;
        }            
        //reload
        else if(reloadAction.triggered && !isReloading)
        {            
            Reload();
            nextFireTime = Time.time + reloadTime;
        }
        //set reticle back to normal
        else if (reticleRenderer.material!=normalReticle && Time.time >= nextFireTime) //set the reticle back to normal if enough time has passed since last shot
        {
            reticleRenderer.material = normalReticle;
        }    
    }

    private void Shoot()
    {
        reticleRenderer.material = shotReticle;

        if (bulletPrefab != null)
        {
            GameObject bulletObj=Instantiate(bulletPrefab, transform.position, transform.rotation); //create bullet at player
            Projectile bullet=bulletObj.GetComponent<Projectile>();
            bullet.SetTarget(reticle.transform.position);
        }
        else
        {
            Debug.LogWarning("Bullet Prefab is not assigned.");
        }
    }
    private void Reload()
    {
        isReloading = true;
        reticleRenderer.material = reloadReticle;
        WaitForSeconds wait = new WaitForSeconds(reloadTime);
        isReloading = false;
    }

}
