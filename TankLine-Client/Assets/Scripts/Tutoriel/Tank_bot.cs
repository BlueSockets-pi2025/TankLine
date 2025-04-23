using UnityEngine;
using Unity.Mathematics;
using FishNet.Object;

public class Tank_Bot : Tank
{
    public Transform target;
    public GameObject bulletPrefab;

    private float fireCooldown = 2f;
    private float lastShotTime = -10f;

    private const float MIN_ROTATION_BEFORE_MOVEMENT = math.PI / 2;
    private int nbBulletShot = 0;
    public int MaxBulletShot = 5;

    protected override void Start()
    {
        base.Start();
        thisTank = transform;
        thisGun = thisTank.Find("tankGun");
    }

    void Update()
    {
        if (!IsServer || target == null) return;
        AimAtTarget();
        TryShoot();
    }
    void TryShoot()
{
    if (Time.time - lastShotTime >= fireCooldown && CanShoot())
    {
        Shoot();
        lastShotTime = Time.time;
    }
}


    void FixedUpdate()
    {
        if (!IsServer || target == null) return;
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
        Vector3 dir = (target.position - thisTank.position).normalized;
        float x = dir.x;
        float y = dir.z;

        float moveForce = FaceDirection(x, y);
        GoForward(moveForce);
    }

    float FaceDirection(float x, float y)
    {
        if (x == 0 && y == 0) return 0;

        float targetDirection;
        float angle = Mathf.Repeat(math.atan2(x, y), math.PI2);
        int isBackward = 1;

        float angle2 = angle + math.PI2;
        float angle3 = angle - math.PI2;

        if (math.abs(tankRotation - angle) < math.abs(tankRotation - angle2))
        {
            if (math.abs(tankRotation - angle) < math.abs(tankRotation - angle3))
                targetDirection = angle;
            else
                targetDirection = angle3;
        }
        else if (math.abs(tankRotation - angle2) < math.abs(tankRotation - angle3))
            targetDirection = angle2;
        else
            targetDirection = angle3;

        float angle1 = Mathf.Repeat(angle + math.PI, math.PI2);
        angle2 = angle1 + math.PI2;
        angle3 = angle1 - math.PI2;

        if (math.abs(tankRotation - angle1) < math.abs(tankRotation - angle2))
        {
            if (math.abs(tankRotation - angle1) > math.abs(tankRotation - angle3))
                angle1 = angle3;
        }
        else if (math.abs(tankRotation - angle2) < math.abs(tankRotation - angle3))
            angle1 = angle2;
        else
            angle1 = angle3;

        if (math.abs(tankRotation - targetDirection) > math.abs(tankRotation - angle1))
        {
            isBackward = -1;
            targetDirection = angle1;
        }

        if (math.abs(targetDirection - tankRotation) <= 0.0001f)
            return math.max(math.abs(x), math.abs(y)) * isBackward;

        float forceRotation = (targetDirection - tankRotation) / rotationSpeed;
        if (math.abs(forceRotation) > 1)
        {
            RotateTank(forceRotation > 0 ? 1 : -1);
        }
        else
        {
            SetRotationTank(targetDirection);
        }

        if (math.abs(targetDirection - tankRotation) > MIN_ROTATION_BEFORE_MOVEMENT)
        {
            return 0;
        }
        else
        {
            if (math.abs(targetDirection - tankRotation) < 0.0001f)
                return math.max(math.abs(x), math.abs(y)) * isBackward;
            else
                return math.clamp(1 - (math.abs(targetDirection - tankRotation) / MIN_ROTATION_BEFORE_MOVEMENT), 0, 1) * math.max(math.abs(x), math.abs(y)) * isBackward;
        }
    }

    protected bool CanShoot()
    {
        Vector3 origin = new Vector3(thisTank.position.x, 0.5f, thisTank.position.z);
        float gunRotation = thisGun.eulerAngles.y * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(math.cos(gunRotation - math.PI / 2), 0, -math.sin(gunRotation - math.PI / 2));
        return !Physics.Raycast(origin, direction, 1.3f);
    }

    [ServerRpc(RequireOwnership = false)]
    protected void Shoot()
    {
        if (nbBulletShot >= MaxBulletShot) return;

        Vector3 pos = new Vector3(thisGun.position.x, 0.5f, thisGun.position.z);
        float rotation = thisGun.rotation.eulerAngles.y * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(math.cos(rotation - math.PI / 2), 0, -math.sin(rotation - math.PI / 2));

        GameObject newBulletObject = Instantiate(bulletPrefab, pos + 0.9f * dir, Quaternion.identity);

        Bullet newBullet = newBulletObject.GetComponent<Bullet>();
        newBullet.direction.Value = dir;
        newBullet.tankOwner = gameObject;
        nbBulletShot++;

        Spawn(newBulletObject);
    }

    public void DecreaseNbBulletShot()
    {
        nbBulletShot--;
    }
}
