using UnityEngine;
using Unity.Mathematics;

public class Tank_Bot : Tank
{
    public Transform target;
    public GameObject bulletPrefab;

    private float fireCooldown = 2f;
    private float lastShotTime = -12f;

    private const float MIN_ROTATION_BEFORE_MOVEMENT = math.PI / 4; 
    private int nbBulletShot = 0;
    public int MaxBulletShot = 5;

    public int maxLives = 3;
    private int currentLives;

    protected override void Start()
    {
        base.Start();
        thisTank = transform;
        thisGun = thisTank.Find("tankGun");
        currentLives = maxLives;
    }

    void Update()
    {
        if (target == null)
        {
            return;
        }
        AimAtTarget();
        TryShoot();
    }

    void FixedUpdate()
    {
        if (target == null) return;
        MoveTowardsTarget();
    }

    void AimAtTarget()
    {
        Vector3 dir = (target.position - thisTank.position).normalized;
        float angle = math.atan2(dir.x, dir.z);
        SetRotationGun(angle + math.PI / 2);
    }

    void MoveTowardsTarget()
    {
        if (target == null) 
        {
            Debug.Log("Can't move - missing target or not server");
            return;
        }
        
        Vector3 dir = (target.position - thisTank.position).normalized;
        float x = dir.x;
        float y = dir.z;

        float moveForce = FaceDirection(x, y);
        if (moveForce > 0)
        {
            GoForward(moveForce);
        }
    }

    void TryShoot()
    {
        if (Time.time - lastShotTime >= fireCooldown)
        {
            if (CanShoot())
            {
                Shoot();
                lastShotTime = Time.time;
            }
            else
            {
                Debug.Log("Can't shoot - obstacle in the way");
            }
        }
    }
    protected bool CanShoot()
    {
        Vector3 origin = new Vector3(thisTank.position.x, 0.5f, thisTank.position.z);
        float gunRotation = thisGun.eulerAngles.y * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(math.cos(gunRotation - math.PI / 2), 0, -math.sin(gunRotation - math.PI / 2));
        return !Physics.Raycast(origin, direction, 1.3f);
    }
    protected void Shoot()
    {
        if (nbBulletShot < MaxBulletShot && bulletPrefab != null)
        {
            Vector3 pos = new Vector3(thisGun.position.x, 0.5f, thisGun.position.z);
            float rotation = thisGun.rotation.eulerAngles.y * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(math.cos(rotation - math.PI / 2), 0, -math.sin(rotation - math.PI / 2));
            
            GameObject newBulletObject = Instantiate(bulletPrefab, pos + 0.9f * dir, Quaternion.identity);
            Bullet_Offline newBullet = newBulletObject.GetComponent<Bullet_Offline>();
            
            if (newBullet != null)
            {
                newBullet.direction = dir;
                newBullet.tankOwner = gameObject;
                nbBulletShot++;
            }
            else
            {
                Debug.LogError("Bullet prefab is missing Bullet_Offline component!");
            }
        }
        else if (bulletPrefab == null)
        {
            Debug.LogError("Bullet prefab is not assigned in Tank_Bot!");
        }
    }
    public void DecreaseNbBulletShot()
    {
        nbBulletShot--;
    }

    public void LoseSingleLife()
    {
        currentLives--;
        if (currentLives <= 0)
        {
            Die();
        }
    }
    private float FaceDirection(float x, float y)
    {
        Vector3 desiredDirection = new Vector3(x, 0, y).normalized;

        // Calculate the angle we want to face (in degrees)
        float targetAngle = Mathf.Atan2(desiredDirection.x, desiredDirection.z) * Mathf.Rad2Deg;

        // Smoothly rotate the tank toward the target angle
        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
        thisTank.rotation = Quaternion.RotateTowards(thisTank.rotation, targetRotation, 180f * Time.fixedDeltaTime); // Adjust rotation speed as needed

        // Calculate angle difference
        float angleDiff = Quaternion.Angle(thisTank.rotation, targetRotation);

        // Only move forward if mostly facing the target
        return angleDiff < MIN_ROTATION_BEFORE_MOVEMENT * Mathf.Rad2Deg ? 1f : 0f;
    }



    private void Die()
    {
        Destroy(gameObject); 
    }
}