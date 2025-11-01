using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class CharacterCore : MonoBehaviour
{
    //upgrade system integration
    [Header("Stat System")]
    private StatManager statManager;
    private UpgradeManager upgradeManager;

    private InputAction moveAction;
    private CharacterController controller;

    //shooting
    [Tooltip("The bullet prefab to shoot")]
    public GameObject bulletPrefab;
    private Projectile bulletProperties=null;
    [Tooltip("The reticle object to aim with")]
    public GameObject reticle;
    public Material normalReticle = null;
    public Material shotReticle = null;
    public Material reloadReticle = null;

    private InputAction shootAction;
    private InputAction reloadAction;
    private Renderer reticleRenderer=null;
    private bool isReloading = false;
    private bool isShooting = false;
    private int CurrentBulletCount;

    private TextMeshPro ammoCounter=null;
    private PlayerDirectionController directionController=null;

    private void Start()
    {
        // Get StatManager and set up base stats
        statManager = GetComponent<StatManager>();
        upgradeManager = GetComponent<UpgradeManager>();

        if (statManager == null)
        {
            Debug.LogError("StatManager component not found on " + gameObject.name);
        }
        
        moveAction = InputSystem.actions.FindAction("Move"); //binds the "Move" actions from the Input Actions Asset
        controller = GetComponent<CharacterController>();

        shootAction = InputSystem.actions.FindAction("Attack"); //binds the "Attack" actions from the Input Actions Asset
        reticleRenderer = reticle.GetComponent<Renderer>();

        reloadAction = InputSystem.actions.FindAction("Reload"); //binds the "Reload" actions from the Input Actions Asset
        
        // Get max bullet count from StatManager
        CurrentBulletCount = Mathf.RoundToInt(statManager != null ? statManager.GetStatValue(StatType.MagazineSize) : 5);


        try
        {
            ammoCounter = GameObject.Find("AmmoCounter").GetComponent<TextMeshPro>();
            int maxBullets = Mathf.RoundToInt(statManager != null ? statManager.GetStatValue(StatType.MagazineSize) : 5);
            ammoCounter.text = CurrentBulletCount.ToString() + " / " + maxBullets.ToString();
        }
        catch
        {
            Debug.LogWarning("No AmmoCounter TextMeshPro object found.");
        }

        try
        {
            bulletProperties = bulletPrefab.GetComponent<Projectile>();
        }
        catch
        {
            Debug.LogWarning("No Projectile component found on the bullet prefab.");
        }

        // Get the direction controller component
        directionController = GetComponent<PlayerDirectionController>();
        if (directionController == null)
        {
            Debug.LogWarning("No PlayerDirectionController found on player.");
        }
    }

    private void Update()
    {
        //moveAction is a Vector2, which looks like (x,y) with x and y being -1, 0, or 1 depending on the input
        //(movement along an axis. 0 is none, -1 is one direction, and 1 is the other)
        Vector2 moveValue= moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(moveValue.x, 0, moveValue.y); //now we convert it to a Vector3 for 3D movement. the 0 is since we don't want to move up or down

        //Read the current speed from the stat manager
        float currentSpeed = statManager != null ? statManager.GetStatValue(StatType.MoveSpeed) : 5f;

        //Use currentSpeed to account for upgrades/modifiers
        controller.Move(move * Time.deltaTime * currentSpeed); //this is the actual movement

        //shooting
        if (shootAction.IsPressed() && !isReloading && !isShooting && CurrentBulletCount > 0) //if shoot action is pressed (held works too this way) and the time is greater than what is calculated as the next time a shot can be fired and not reloading and has bullets
        {
            StartCoroutine(Shoot()); //start corutine lets us wait for some time without blocking the main thread
        }
        //reload
        else if ((reloadAction.triggered || CurrentBulletCount <= 0) && !isReloading) //if reload action triggered manually or out of bullets, reload if not currently
        {
            StartCoroutine(Reload()); //start corutine lets us wait for some time without blocking the main thread

        }
        
        // Press U to get upgrade for testing 
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (upgradeManager != null)
            {
                upgradeManager.AcquireUpgrade(upgradeManager.GetRandomAvailableUpgrades(1)[0]);
                Debug.Log("Upgrade acquired!");
            }
        }
        
    }

    private System.Collections.IEnumerator Shoot() //system.collections.ienumerator lets us use yield return to wait
    {
        reticleRenderer.material = shotReticle;
        CurrentBulletCount--;
        isShooting = true;
        
        // Notify direction controller about shooting state
        if (directionController != null)
        {
            directionController.SetShooting(true);
        }

        if (bulletPrefab != null)
        {
            GameObject bulletObj = Instantiate(bulletPrefab, transform.position, transform.rotation); //create bullet at player
            Projectile bullet = bulletObj.GetComponent<Projectile>();
            
            // Get bullet spread from StatManager
            float currentBulletSpread = statManager != null ? statManager.GetStatValue(StatType.BulletSpread) : 0.2f;
            bullet.SetTarget(reticle.transform.position, currentBulletSpread);

            if (ammoCounter != null)//update ammo counter display if it exists
            {
                int maxBullets = Mathf.RoundToInt(statManager != null ? statManager.GetStatValue(StatType.MagazineSize) : 5);
                ammoCounter.text = CurrentBulletCount.ToString() + " / " + maxBullets.ToString();
            }
        }
        else
        {
            Debug.LogWarning("Bullet Prefab is not assigned.");
        }

        //read the current attack speed from the stat manager if it exists, otherwise use the default fireRate
        float currentAttackSpeed = statManager != null ? statManager.GetStatValue(StatType.AttackSpeed) : 5f;
        float timeBetweenShots = 1f / currentAttackSpeed; // Convert attacks-per-second back to seconds-between-attacks
        
        yield return new WaitForSeconds(timeBetweenShots); //this is what actually waits for fire rate time
        isShooting = false;
        
        // Notify direction controller about shooting state
        if (directionController != null)
        {
            directionController.SetShooting(false);
        }
        
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
        //read the current reload speed from the stat manager
        float currentReloadSpeed = statManager != null ? statManager.GetStatValue(StatType.ReloadSpeed) : 2f;
        yield return new WaitForSeconds(currentReloadSpeed); //this is what actually waits for reload time

        // Get max bullet count from StatManager
        CurrentBulletCount = Mathf.RoundToInt(statManager != null ? statManager.GetStatValue(StatType.MagazineSize) : 5);
        reticleRenderer.material = normalReticle;
        if (ammoCounter != null)//update ammo counter display if it exists
        {
            int maxBullets = Mathf.RoundToInt(statManager != null ? statManager.GetStatValue(StatType.MagazineSize) : 5);
            ammoCounter.text = CurrentBulletCount.ToString() + " / " + maxBullets.ToString();
        }
        isReloading = false;
    }

}
