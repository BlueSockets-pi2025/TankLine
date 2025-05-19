using System;
using FishNet.Object;
using FishNet.Connection;
using Unity.Mathematics;
using UnityEngine;
using Scripts.Tutoriel;
using UnityEngine.InputSystem;

public class Tank_Player : Tank
{
    /// <summary> The bullet prefab </summary>
    public GameObject bulletPrefab;


    /// <summary> The number of bullet already shot </summary>
    [HideInInspector]
    public int nbBulletShot = 0;

    /// <summary> The maximum number of bullet shot at the same time and still on the map </summary>
    [Range(1, 50)]
    public int MaxBulletShot = 5;

    /// <summary>The death alembic prefab</summary>
    public GameObject deathVfxPrefab;

    const int LEFT_CLICK = 0;
    const float MIN_ROTATION_BEFORE_MOVEMENT = math.PI / 2;

    protected float movementToMake = 0;

    //<summary> For mobile controls </summary>
    public InputActionReference move;
    Vector3 MoveDir;

    private MoveJoystick joystick;
    private ShootJoystick shootJoystick;
    private InGameUiManager uiManager;
    private GameObject indicator;
    private PlayerInput playerInput;

    protected override void Start()
    {
        // set thisTank to the GameObject this script is attached to
        thisTank = gameObject.transform;
        // get the "tankGun" child
        thisGun = thisTank.transform.Find("tankGun");

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

#if UNITY_STANDALONE
        controls.SetActive(false);
#endif
#if UNITY_ANDROID
        controls.SetActive(true);
        shootJoystick.player = gameObject;
#endif

        playerInput = GetComponent<PlayerInput>();
        activeInput();

        // get the canvas
        uiManager = new(GameObject.Find("Canvas"), true);
        uiManager.SetBulletUI(nbBulletShot, MaxBulletShot);
        // get the indicator
        indicator = thisTank.Find("indicator").gameObject;
        activeIndicator();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        playerInput = GetComponent<PlayerInput>();
        activeInput();
    }

    public void activeIndicator()
    {
        if (base.IsOwner)
        {
            indicator.SetActive(true);
        }
    }
    public void activeInput()
    {
        if (!base.IsOwner)
        {
            Debug.Log("TEST - Mobile input - X " + transform.Find("base").GetComponent<Renderer>().materials[4]);
            playerInput.enabled = false;
        }
        else
        {
            Debug.Log("TEST - Mobile input - active " + transform.Find("base").GetComponent<Renderer>().materials[4]);
            playerInput.enabled = true;
            playerInput.ActivateInput();
        }
    }

    /// <summary>
    /// Automatically called by unity every frame after the physic engine
    /// </summary>
    protected void Update()
    {
        if (!base.IsOwner) return;

#if UNITY_STANDALONE
        // process mouse aiming
        this.GunTrackPlayerMouse();
        this.ApplyRotation();
#endif
#if UNITY_ANDROID
        if (shootJoystick != null)
        {
            this.GunTrackJoystick(shootJoystick.GetInput());
            this.ApplyRotation();
        }
        else
        {
            Debug.LogError("[Tank_Player] shootJoystick is null in Update.");
        }
#endif
        if (GetCurrentSceneName() == "Tuto" || GetCurrentSceneName() == "TutoPC")
        {
            var tutorial = FindObjectOfType<TankTutorial>();
            if (!tutorial.IsInShootingStep) return;
        }

        // stick the tank to the ground
        if (transform.position.y > 0.07)
            transform.position = new(transform.position.x, 0, transform.position.z);
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
            if (this.CanShoot())
            {
                if (nbBulletShot < MaxBulletShot)
                {
                    nbBulletShot++;
                    uiManager.SetBulletUI(nbBulletShot, MaxBulletShot);
                }

                this.Shoot(gunRotation);
            }
            else
            {
                Debug.Log("Prevent self-shoot. TODO : animation");
            }
        }
    }

    public void OnShootButtonClick()
    {
        if (this.CanShoot())
        {
            if (nbBulletShot < MaxBulletShot)
            {
                nbBulletShot++;
                uiManager.SetBulletUI(nbBulletShot, MaxBulletShot);
            }

            this.Shoot(gunRotation);
        }
        else
        {
            // Debug.Log("Prevent self-shoot. TODO : animation");
        }
    }

    /// <summary>
    /// Automatically called by unity every frame before the physic engine
    /// </summary>
    protected override void FixedUpdate()
    {
        if (!base.IsOwner) return;

#if UNITY_STANDALONE
        // move.action.ReadValue<Vector2>();

        // process rotation input
        // float y = Input.GetAxis("Vertical");
        // float x = Input.GetAxis("Horizontal");
        float y = MoveDir.y;
        float x = MoveDir.x;
        movementToMake = this.FaceDirection(x, y);
#endif
#if UNITY_ANDROID

        if (joystick != null)
        {
            float x = joystick.GetHorizontal();
            float y = joystick.GetVertical();
            movementToMake = this.FaceDirection(x, y);
        }
        else
        {
            Debug.LogError("[Tank_Player] joystick is null in FixedUpdate.");
        }

#endif

        // process movement input
        this.GoForward(movementToMake);

        /* ------------------------------------------------------
            Old rotation system, might re-use later as an option
           ------------------------------------------------------

            float acceleration = Input.GetAxis("Vertical");
            float rotation = Input.GetAxis("Horizontal");

            // process movement input
            this.GoForward(acceleration);
            this.RotateTank(rotation);
           ------------------------------------------------------ */
    }

    /// <summary>
    /// Automatically called by unity when this object rigidbody is in a collision with another object
    /// </summary>
    protected void OnCollisionStay(Collision collision)
    {

        // check the collision type
        Vector3 collisionPoint = new Vector3(0, 0, 0);
        foreach (ContactPoint contact in collision.contacts)
        {
            if (math.abs(contact.point.y) >= 0.001)
            { // do not process floor collisions
                collisionPoint = contact.point - thisTank.position;
            }
        }

        // if a collision with a wall happened, correct rotation to make it stick to the wall
        if (collisionPoint != new Vector3(0, 0, 0))
        {
            // Up correction
            if (tankRotation < math.PI / 4 || tankRotation > (7 * math.PI / 4))
            {
                // if rotation is close enough to target, set it to target
                if (math.abs(tankRotation - math.PI) > (math.PI - rotationSpeed * 0.3))
                {
                    this.SetRotationTank(0);
                }

                // else, slightly correct it to make the closest side stick to the wall
                else if ((tankRotation - math.PI) > 0)
                {
                    this.RotateTank(0.2f);
                }
                else
                {
                    this.RotateTank(-0.2f);
                }
            }

            // Right correction
            else if (tankRotation < (3 * math.PI / 4))
            {
                // if rotation is close enough to target, set it to target
                if (math.abs(tankRotation - (math.PI) / 2) < rotationSpeed * 0.3)
                {
                    this.SetRotationTank(math.PI / 2);
                }

                // else, slightly correct it to make the closest side stick to the wall
                else if ((tankRotation - (math.PI) / 2) < 0)
                {
                    this.RotateTank(0.2f);
                }
                else
                {
                    this.RotateTank(-0.2f);
                }
            }

            // Down correction
            else if (tankRotation < (5 * math.PI / 4))
            {
                // if rotation is close enough to target, set it to target
                if (math.abs(tankRotation - math.PI) < rotationSpeed * 0.3)
                {
                    this.SetRotationTank(math.PI);
                }

                // else, slightly correct it to make the closest side stick to the wall
                else if ((tankRotation - math.PI) < 0)
                {
                    this.RotateTank(0.2f);
                }
                else
                {
                    this.RotateTank(-0.2f);
                }
            }

            // Left correction
            else
            {
                // if rotation is close enough to target, set it to target
                if (math.abs((tankRotation - (3 * math.PI) / 2)) < rotationSpeed * 0.3)
                {
                    this.SetRotationTank((3 * math.PI) / 2);
                }

                // else, slightly correct it to make the closest side stick to the wall
                else if ((tankRotation - (3 * math.PI) / 2) < 0)
                {
                    this.RotateTank(0.2f);
                }
                else
                {
                    this.RotateTank(-0.2f);
                }
            }

        }
    }

    /// <summary>
    /// Make the tank gun "look" in the direction of the player mouse
    /// </summary>
    protected void GunTrackPlayerMouse()
    {
        // get the current scene camera
        Camera cam = Camera.main;

        // the tank position on the screen
        Vector3 tankPosOnScreen = cam.WorldToScreenPoint(thisTank.position);

        Vector3 difference = new Vector3(Input.mousePosition.x - tankPosOnScreen.x, Input.mousePosition.y - tankPosOnScreen.y, 0);
        difference.Normalize();

        // change euclidian coordonates to polar
        float gunRotation = math.atan2(-difference.y, difference.x);

        // rotate this tank gun to face the mouse
        this.SetRotationGun(gunRotation + math.PI / 2);
    }

    //for android Joystick
    protected void GunTrackJoystick(Vector2 joystickInput)
    {
        if (joystickInput.magnitude < 0.2f)
            return; // Ignore les petits mouvements

        // Convertit l’entrée joystick en angle
        float gunRotation = Mathf.Atan2(-joystickInput.y, joystickInput.x);

        // Applique la rotation au canon
        this.SetRotationGun(gunRotation + Mathf.PI / 2);
    }

    /// <summary>
    /// Make the player face the direction (x,y)
    /// </summary>
    /// <param name="x">The x axis of the direction</param>
    /// <param name="y">The y axis of the direction</param>
    /// <returns> The force of the movement the tank can execute after the rotation (between 0 and 1) </returns>
    protected float FaceDirection(float x, float y)
    {
        // avoid division by 0 and do not move if the player is touching nothing
        if (x == 0 && y == 0)
        {
            return 0;
        }

        float targetDirection;
        float angle = Mathf.Repeat(math.atan2(x, y), math.PI2);
        int isBackward = 1;

        // check for more than pi/2 and less than 0 to avoid useless 360°
        float angle2 = angle + math.PI2;
        float angle3 = angle - math.PI2;

        // find closest rotation
        if (math.abs(tankRotation - angle) < math.abs(tankRotation - angle2))
        {
            if (math.abs(tankRotation - angle) < math.abs(tankRotation - angle3))
            {
                targetDirection = angle;
            }
            else
            {
                targetDirection = angle3;
            }
        }
        else if (math.abs(tankRotation - angle2) < math.abs(tankRotation - angle3))
        {
            targetDirection = angle2;
        }
        else
        {
            targetDirection = angle3;
        }

        // check if moving backward is faster than rotating then moving forward
        float angle1 = Mathf.Repeat(angle + math.PI, math.PI2);
        angle2 = Mathf.Repeat(angle + math.PI, math.PI2) + math.PI2;
        angle3 = Mathf.Repeat(angle + math.PI, math.PI2) - math.PI2;

        if (math.abs(tankRotation - angle1) < math.abs(tankRotation - angle2))
        {
            if (math.abs(tankRotation - angle1) > math.abs(tankRotation - angle3))
            {
                angle1 = angle3;
            }
        }
        else if (math.abs(tankRotation - angle2) < math.abs(tankRotation - angle3))
        {
            angle1 = angle2;
        }
        else
        {
            angle1 = angle3;
        }

        if (math.abs(tankRotation - targetDirection) > math.abs(tankRotation - angle1))
        {
            isBackward = -1;
            targetDirection = angle1;
        }

        // if already in this direction, return
        if (math.abs(targetDirection - tankRotation) <= 0.0001f)
        {
            return isBackward;
        }

        // apply rotation
        float forceRotation = (targetDirection - tankRotation) / rotationSpeed;
        if (math.abs(forceRotation) > 1)
        {
            if (forceRotation > 0)
                this.RotateTank(1);
            else
                this.RotateTank(-1);
        }
        else
        {
            this.SetRotationTank(targetDirection);
        }

        // if too far from target position, do not move
        if (math.abs(targetDirection - tankRotation) > MIN_ROTATION_BEFORE_MOVEMENT)
        {
            return 0;
        }
        else
        {
            // else, set the movement force proportionally inverse to the difference between the target angle and the current angle
            if (math.abs(targetDirection - tankRotation) < 0.0001f)
            { // avoid division by 0
                return isBackward;
            }
            else
            {
                return math.clamp(1 - (math.abs(targetDirection - tankRotation) / MIN_ROTATION_BEFORE_MOVEMENT), 0, 1) * isBackward;
            }
        }
    }

    /// <summary>
    /// Test if this player can shoot. <br/>
    /// Used to prevent bug and insta-self shooting when close to a wall
    /// </summary>
    /// <returns>True if there isn't a wall too close where the player is trying to shoot.</returns>
    public bool CanShoot()
    {
        Vector3 origin = new Vector3(thisTank.position.x, 0.5f, thisTank.position.z);
        Vector3 direction = new Vector3(math.cos(gunRotation - math.PI / 2), 0, -math.sin(gunRotation - math.PI / 2));
        Debug.DrawRay(origin - direction * 0.5f, direction * 1.8f, Color.red, 1); // DEBUG ONLY

        // use a raycast to prevent self-shooting if a wall is too close
        if (Physics.Raycast(origin, direction, 1.3f))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    // only call this function as server
    [ServerRpc(RequireOwnership = true)]
    public void Shoot(float clientGunRotation)
    {
        Debug.Log($"Shooting : {clientGunRotation}");
        if (nbBulletShot < MaxBulletShot)
        {
            // compute new bullet position
            Vector3 pos = new Vector3(thisGun.position.x, 0.5f, thisGun.position.z);

            // compute new bullet direction
            Vector3 dir = new Vector3(math.cos(clientGunRotation - math.PI / 2), 0, -math.sin(clientGunRotation - math.PI / 2));

            // spawn object
            GameObject newBulletObject = Instantiate(bulletPrefab, pos + 0.9f * dir, Quaternion.identity);

            // change direction and tankOwner (for bullet count)
            Bullet newBullet = newBulletObject.GetComponent<Bullet>();
            newBullet.direction.Value = dir;
            newBullet.tankOwner = gameObject;
            nbBulletShot++;

            Spawn(newBulletObject, null);
        }
    }

    public void DecreaseNbBulletShot()
    {
        nbBulletShot--;

        if (Environment.GetEnvironmentVariable("IS_DEDICATED_SERVER") != "true") return;

        DecreaseNbBulletShotUI(base.Owner, nbBulletShot);
    }

    [TargetRpc]
    public void DecreaseNbBulletShotUI(NetworkConnection connection, int current)
    {
        nbBulletShot = current;
        uiManager.SetBulletUI(current, MaxBulletShot);
    }


    public void OnDestroy()
    {
        if (Environment.GetEnvironmentVariable("IS_DEDICATED_SERVER") == "true") return; // only exec on client

        BushGroup[] bushes = FindObjectsByType<BushGroup>(FindObjectsSortMode.None);
        foreach (BushGroup bush in bushes)
        {
            bush.SetSolidForGroup();
        }

#if UNITY_STANDALONE
        // play death vfx
        Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
#endif
    }
    public string GetCurrentSceneName()
    {
        string haja = Application.loadedLevelName;
        return haja;
    }
}
