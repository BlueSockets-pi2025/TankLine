using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Scripts.Tutoriel;

public class Tank_Offline : MonoBehaviour
{
    /// <summary>This object movement speed (base speed : 3.0)</summary>
    [Range(1f, 10f)]
    public float movementSpeed = 3.0f;

    /// <summary>This object rotation speed (base speed : pi/48)</summary>
    [Range(math.PI / 128, math.PI / 16)]
    public float rotationSpeed = math.PI / 48;

    /// <summary>The number of life left for this tank</summary>
    [Range(1, 10)]
    public int nbLifeLeft = 3;

    protected Transform thisTank;
    protected Transform thisGun;

    /// <summary> The bullet prefab </summary>
    public GameObject bulletPrefab;

    /// <summary> The number of bullet already shot </summary>
    [HideInInspector]
    public int nbBulletShot = 0;

    /// <summary> The maximum number of bullet shot at the same time and still on the map </summary>
    [Range(1, 50)]
    public int MaxBulletShot = 5;

    const int LEFT_CLICK = 0;
    const float MIN_ROTATION_BEFORE_MOVEMENT = math.PI / 2;

    protected float movementToMake = 0;
    public GameObject deathVfxPrefab;

    /// <summary> The whole tank current angle in radians </summary>
    protected float tankRotation = 0;

    /// <summary> The gun current angle in radians </summary>
    protected float gunRotation = 0;

    // Input system variables
    public MoveJoystick joystick; // For mobile movement
    public ShootJoystick shootJoystick; // For mobile aiming
    public InputActionReference move; // For new input system
    Vector3 MoveDir; // Stores movement input

    private InGameUiManager uiManager;

    protected virtual void Start()
    {
        // set thisTank to the GameObject this script is attached to
        thisTank = gameObject.transform;
        // get the "tankGun" child
        thisGun = thisTank.transform.Find("tankGun");
#if UNITY_ANDROID
//Mobile controls
        GameObject canvas = GameObject.Find("Canvas");
        GameObject controls = canvas.transform.Find("Controls").gameObject;
        joystick = controls.transform.Find("ImgMove")?.GetComponent<MoveJoystick>();
        if (joystick == null)
        {
            Debug.LogError("[Tank_Player] joystick is null in Start.");
        }

        shootJoystick = controls.transform.Find("ButtonShot")?.GetComponent<ShootJoystick>();
        if (shootJoystick == null)
        {
            Debug.LogError("[Tank_Player] shootJoystick is null in Start.");
        }
        shootJoystick.player = gameObject;
#endif

        if (GetCurrentSceneName() != "Tuto" && GetCurrentSceneName() != "TutoPC")
            uiManager = new(GameObject.Find("Canvas"), true);
    }

    /// <summary>
    /// Automatically called by unity every frame after the physic engine
    /// </summary>
    protected void Update()
    {
        // Handle aiming based on platform
#if UNITY_STANDALONE || UNITY_WEBGL
        GunTrackPlayerMouse();
#elif UNITY_ANDROID || UNITY_IOS
        GunTrackJoystick(shootJoystick.GetInput());
#endif

        ApplyRotation();

        if (GetCurrentSceneName() == "Tuto" || GetCurrentSceneName() == "TutoPC")
        {
            var tutorial = FindObjectOfType<TankTutorial>();
            if (!tutorial.IsInShootingStep) return;
        }

        // Handle shooting input
#if UNITY_STANDALONE || UNITY_WEBGL
        if (Input.GetMouseButtonDown(LEFT_CLICK))
        {
            // TryShoot();
        }
#endif

        // stick the tank to the ground
        if (transform.position.y > 0.07)
            transform.position = new(transform.position.x, 0, transform.position.z);
    }

    /// <summary>
    /// Automatically called by unity every frame before the physic engine
    /// </summary>
    protected void FixedUpdate()
    {
        float x, y;

        // Get input based on platform
#if UNITY_STANDALONE || UNITY_WEBGL
        y = MoveDir.y;
        x = MoveDir.x;
#elif UNITY_ANDROID || UNITY_IOS
        x = joystick.GetHorizontal();
        y = joystick.GetVertical();
#endif

        movementToMake = FaceDirection(x, y);
        GoForward(movementToMake);
    }

    public void onMove(InputAction.CallbackContext ctxt)
    {
        Vector3 NewMoveDir = ctxt.ReadValue<Vector2>();
        MoveDir.x = NewMoveDir.x;
        MoveDir.y = NewMoveDir.y;
    }

    public void onShoot(InputAction.CallbackContext ctxt)
    {
        if (ctxt.performed)
        {
            if (GetCurrentSceneName() == "Tuto" || GetCurrentSceneName() == "TutoPC")
            {
                var tutorial = FindObjectOfType<TankTutorial>();
                tutorial.OnShootPerformed();
                if (!tutorial.IsInShootingStep) return;
            }
            TryShoot();
        }
    }

    public void OnShootButtonClick()
    {
        TryShoot();
    }

    private void TryShoot()
    {
        if (CanShoot())
        {
            if (nbBulletShot < MaxBulletShot)
            {
                if (GetCurrentSceneName() != "Tuto" && GetCurrentSceneName() != "TutoPC")
                    uiManager.SetBulletUI(nbBulletShot, MaxBulletShot);
            }

            Shoot(gunRotation);
        }
    }

    /// <summary>
    /// Apply this script stored rotation to the unity gameObject transform
    /// </summary>
    public void ApplyRotation()
    {
        // Tank rotation
        float degreeRotation = tankRotation * Mathf.Rad2Deg;
        thisTank.transform.rotation = Quaternion.Euler(0, degreeRotation, 0);

        // Tank gun rotation
        degreeRotation = gunRotation * Mathf.Rad2Deg;
        thisGun.rotation = Quaternion.Euler(PlatformSpecificGunRotation(), degreeRotation, 0);
    }

    private float PlatformSpecificGunRotation()
    {
        return -90;
    }

    /*
        -------------------- Rotations --------------------
    */

    /// <summary>
    /// Rotate the tank base relatively to its previous rotation.
    /// </summary>
    public void RotateTank(float force)
    {
        tankRotation = Mathf.Repeat(tankRotation + (force * rotationSpeed), math.PI * 2);
    }

    /// <summary>
    /// Set the tank rotation.
    /// </summary>
    public void SetRotationTank(float angle)
    {
        tankRotation = Mathf.Repeat(angle, Mathf.PI * 2);
    }

    /// <summary>
    /// Rotate the tank gun relatively to its previous rotation.
    /// </summary>
    public void RotateGun(float force)
    {
        gunRotation = Mathf.Repeat(gunRotation + (force * rotationSpeed), math.PI * 2);
    }

    /// <summary>
    /// Set the tank gun rotation.
    /// </summary>
    public void SetRotationGun(float angle)
    {
        gunRotation = Mathf.Repeat(angle, Mathf.PI * 2);
    }

    /*
        -------------------- Movement --------------------
    */

    /// <summary>
    /// Set this tank position
    /// </summary>
    public void SetPosition(Vector3 position)
    {
        thisTank.position = position;
    }

    /// <summary>
    /// Make the tank go forward (relative to rotation)
    /// </summary>
    public void GoForward(float force)
    {
        thisTank.Translate(force * movementSpeed * Time.deltaTime * thisTank.forward, Space.World);
    }

    /*
        -------------------- Combat --------------------
    */

    public void LoseSingleLife()
    {
        nbLifeLeft--;

        if (nbLifeLeft <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void LoseMultipleLife(int n)
    {
        nbLifeLeft -= n;

        if (nbLifeLeft <= 0)
        {
            Destroy(gameObject);
        }
    }

    /*
        -------------------- Collision --------------------
    */

    protected void OnCollisionStay(Collision collision)
    {
        Vector3 collisionPoint = Vector3.zero;
        foreach (ContactPoint contact in collision.contacts)
        {
            if (math.abs(contact.point.y) >= 0.001)
            {
                collisionPoint = contact.point - thisTank.position;
            }
        }

        if (collisionPoint != Vector3.zero)
        {
            HandleWallCollision(collisionPoint);
        }
    }

    private void HandleWallCollision(Vector3 collisionPoint)
    {
        // Up correction
        if (tankRotation < math.PI / 4 || tankRotation > (7 * math.PI / 4))
        {
            HandleCollisionCorrection(tankRotation, 0, math.PI);
        }
        // Right correction
        else if (tankRotation < (3 * math.PI / 4))
        {
            HandleCollisionCorrection(tankRotation, math.PI / 2, math.PI / 2);
        }
        // Down correction
        else if (tankRotation < (5 * math.PI / 4))
        {
            HandleCollisionCorrection(tankRotation, math.PI, math.PI);
        }
        // Left correction
        else
        {
            HandleCollisionCorrection(tankRotation, (3 * math.PI) / 2, (3 * math.PI) / 2);
        }
    }

    private void HandleCollisionCorrection(float currentRotation, float targetRotation, float comparisonRotation)
    {
        if (math.abs(currentRotation - comparisonRotation) > (math.PI - rotationSpeed * 0.3))
        {
            SetRotationTank(targetRotation);
        }
        else if ((currentRotation - comparisonRotation) > 0)
        {
            RotateTank(0.2f);
        }
        else
        {
            RotateTank(-0.2f);
        }
    }

    /*
        -------------------- Aiming --------------------
    */

    protected void GunTrackPlayerMouse()
    {
        Camera cam = Camera.main;
        Vector3 tankPosOnScreen = cam.WorldToScreenPoint(thisTank.position);
        Vector3 difference = new Vector3(Input.mousePosition.x - tankPosOnScreen.x,
                                       Input.mousePosition.y - tankPosOnScreen.y, 0);
        difference.Normalize();

        float gunAngle = math.atan2(-difference.y, difference.x);
        SetRotationGun(gunAngle + math.PI / 2);
    }

    protected void GunTrackJoystick(Vector2 joystickInput)
    {
        if (joystickInput.magnitude < 0.2f) return;

        float gunAngle = Mathf.Atan2(-joystickInput.y, joystickInput.x);
        SetRotationGun(gunAngle + Mathf.PI / 2);
    }

    /*
        -------------------- Movement Direction --------------------
    */

    protected float FaceDirection(float x, float y)
    {
        if (x == 0 && y == 0) return 0;

        float angle = Mathf.Repeat(math.atan2(x, y), math.PI2);
        int isBackward = 1;

        // Find closest rotation (including wrapped angles)
        float targetDirection = FindClosestAngle(angle);

        // Check if moving backward would be more efficient
        float backwardAngle = Mathf.Repeat(angle + math.PI, math.PI2);
        float closestBackward = FindClosestAngle(backwardAngle);

        if (math.abs(tankRotation - targetDirection) > math.abs(tankRotation - closestBackward))
        {
            isBackward = -1;
            targetDirection = closestBackward;
        }

        // If already facing the right direction
        if (math.abs(targetDirection - tankRotation) <= 0.0001f)
        {
#if UNITY_STANDALONE || UNITY_WEBGL || UNITY_EDITOR
            return math.max(math.abs(x), math.abs(y)) * isBackward;
#else
            return isBackward;
#endif
        }

        // Apply rotation
        float forceRotation = (targetDirection - tankRotation) / rotationSpeed;
        if (math.abs(forceRotation) > 1)
        {
            RotateTank(forceRotation > 0 ? 1 : -1);
        }
        else
        {
            SetRotationTank(targetDirection);
        }

        // If too far from target, don't move
        if (math.abs(targetDirection - tankRotation) > MIN_ROTATION_BEFORE_MOVEMENT)
        {
            return 0;
        }

        // Calculate movement force based on angle difference
        float movementForce = math.clamp(1 - (math.abs(targetDirection - tankRotation) / MIN_ROTATION_BEFORE_MOVEMENT), 0, 1);
#if UNITY_STANDALONE || UNITY_WEBGL || UNITY_EDITOR
        return movementForce * math.max(math.abs(x), math.abs(y)) * isBackward;
#else
        return movementForce * isBackward;
#endif
    }

    private float FindClosestAngle(float angle)
    {
        float angle2 = angle + math.PI2;
        float angle3 = angle - math.PI2;

        if (math.abs(tankRotation - angle) < math.abs(tankRotation - angle2))
        {
            return math.abs(tankRotation - angle) < math.abs(tankRotation - angle3) ? angle : angle3;
        }
        return math.abs(tankRotation - angle2) < math.abs(tankRotation - angle3) ? angle2 : angle3;
    }

    /*
        -------------------- Shooting --------------------
    */

    protected bool CanShoot()
    {
        Vector3 origin = new Vector3(thisTank.position.x, 0.5f, thisTank.position.z);
        Vector3 direction = new Vector3(math.cos(gunRotation - math.PI / 2), 0, -math.sin(gunRotation - math.PI / 2));
        Debug.DrawRay(origin - direction * 0.5f, direction * 1.8f, Color.red, 1);

        return !Physics.Raycast(origin, direction, 1.3f);
    }

    protected void Shoot(float rotation)
    {
        if (nbBulletShot >= MaxBulletShot) return;

        Vector3 pos = new Vector3(thisGun.position.x, 0.5f, thisGun.position.z);
        Vector3 dir = new Vector3(math.cos(rotation - math.PI / 2), 0, -math.sin(rotation - math.PI / 2));

        GameObject newBulletObject = Instantiate(bulletPrefab, pos + 1.0f * dir, Quaternion.identity);
        Bullet_Offline newBullet = newBulletObject.GetComponent<Bullet_Offline>();
        newBullet.direction = dir;
        newBullet.tankOwner = gameObject;
        nbBulletShot++;
    }

    /*
        -------------------- Utility --------------------
    */

    public void DecreaseNbBulletShot()
    {
        nbBulletShot--;
    }

    public void OnDestroy()
    {
        if (Environment.GetEnvironmentVariable("IS_DEDICATED_SERVER") == "true") return;

        BushGroup[] bushes = FindObjectsByType<BushGroup>(FindObjectsSortMode.None);
        foreach (BushGroup bush in bushes)
        {
            bush.SetSolidForGroup();
        }

#if UNITY_STANDALONE || UNITY_WEBGL || UNITY_EDITOR
        Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
#endif
    }

    public string GetCurrentSceneName()
    {
        return Application.loadedLevelName;
    }
}