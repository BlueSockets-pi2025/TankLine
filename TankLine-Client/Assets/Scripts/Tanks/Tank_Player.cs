using Unity.Mathematics;
using UnityEngine;

public class Tank_Player : Tank
{

    public const int LEFT_CLICK = 0;
    public const int LAYER_MAP_OBJECT = 3;
    public const float MIN_ROTATION_BEFORE_MOVEMENT = math.PI/2;

    protected override void Start()
    {
        // set thisTank to the GameObject this script is attached to
        thisTank = gameObject.transform;
        // get the "tankGun" child
        thisGun = thisTank.transform.Find("tankGun");


    }

    /// <summary>
    /// Automatically called by unity every frame before the physic engine
    /// </summary>
    protected override void FixedUpdate() {
        float y = Input.GetAxis("Vertical");
        float x = Input.GetAxis("Horizontal");

        // process mouse aiming
        this.GunTrackPlayerMouse();
        this.ApplyRotation();

        // process movement input
        float movementToMake = this.FaceDirection(x,y);
        this.GoForward(movementToMake);
        this.ApplyRotation();

        // process mouse click to fire a bullet
        if (Input.GetMouseButtonDown(LEFT_CLICK)) {
            Vector3 origin = new Vector3(thisTank.position.x, 0.5f, thisTank.position.z);
            Vector3 direction = new Vector3(math.cos(gunRotation - math.PI / 2), 0, -math.sin(gunRotation - math.PI / 2));

            // use a raycast to prevent selfshooting if a wall is too close
            if (Physics.Raycast(origin, direction, 1.3f)) {
                Debug.Log("Prevent self-shooting");
            } else {
                Debug.Log("Fire !");
            }
            Debug.DrawRay(origin, direction*1.3f, Color.red, 1);
        }
    }

    /// <summary>
    /// Automatically called by unity when this object rigidbody is in a collision with another object
    /// </summary>
    protected void OnCollisionStay(Collision collision) {

        // check the collision type
        Vector3 collisionPoint = new Vector3(0,0,0);
        foreach (ContactPoint contact in collision.contacts) {
            if (math.abs(contact.point.y) >= 0.001) { // do not process floor collisions
                collisionPoint = contact.point-thisTank.position;
            }
        }

        // if a collision with a wall happened, correct rotation to make it stick to the wall
        if (collisionPoint != new Vector3(0,0,0)) {
            // Up correction
            if (tankRotation < math.PI / 4 || tankRotation > (7*math.PI / 4)) {
                // if rotation is close enough to target, set it to target
                if (math.abs(tankRotation - math.PI) > (math.PI - rotationSpeed*0.3)) {
                    this.SetRotationTank(0);
                } 

                // else, slightly correct it to make the closest side stick to the wall
                else if ((tankRotation - math.PI) > 0){
                    this.RotateTank(0.2f);
                } else {
                    this.RotateTank(-0.2f);
                }
            } 
            
            // Right correction
            else if (tankRotation < (3*math.PI / 4)) {
                // if rotation is close enough to target, set it to target
                if (math.abs(tankRotation - (math.PI)/2) < rotationSpeed*0.3) {
                    this.SetRotationTank(math.PI/2);
                } 

                // else, slightly correct it to make the closest side stick to the wall
                else if ((tankRotation - (math.PI)/2) < 0){
                    this.RotateTank(0.2f);
                } else {
                    this.RotateTank(-0.2f);
                }
            }

            // Down correction
            else if (tankRotation < (5*math.PI / 4)) {
                // if rotation is close enough to target, set it to target
                if (math.abs(tankRotation - math.PI) < rotationSpeed*0.3) {
                    this.SetRotationTank(math.PI);
                } 

                // else, slightly correct it to make the closest side stick to the wall
                else if ((tankRotation - math.PI) < 0){
                    this.RotateTank(0.2f);
                } else {
                    this.RotateTank(-0.2f);
                }
            }

            // Left correction
            else {
                // if rotation is close enough to target, set it to target
                if (math.abs((tankRotation - (3*math.PI)/2)) < rotationSpeed*0.3) {
                    this.SetRotationTank((3 * math.PI) / 2);
                } 

                // else, slightly correct it to make the closest side stick to the wall
                else if ((tankRotation - (3*math.PI)/2) < 0){
                    this.RotateTank(0.2f);
                } else {
                    this.RotateTank(-0.2f);
                }
            }

        }
    }

    /// <summary>
    /// Make the tank gun "look" in the direction of the player mouse
    /// </summary>
    protected void GunTrackPlayerMouse() {
        // get the current scene camera
        Camera cam = Camera.main;

        // the tank position on the screen
        Vector3 tankPosOnScreen = cam.WorldToScreenPoint(thisTank.position);

        Vector3 difference = new Vector3(Input.mousePosition.x - tankPosOnScreen.x, Input.mousePosition.y - tankPosOnScreen.y, 0);
        difference.Normalize();

        // change euclidian coordonates to polar
        float gunRotation = math.atan2(-difference.y, difference.x);

        // rotate this tank gun to face the mouse
        this.SetRotationGun(gunRotation + math.PI/2);
    }

    /// <summary>
    /// Make the player face the direction (x,y)
    /// </summary>
    /// <param name="x">The x axis of the direction</param>
    /// <param name="y">The y axis of the direction</param>
    /// <returns> The force of the movement the tank can execute after the rotation (between 0 and 1) </returns>
    protected float FaceDirection(float x, float y) {
        // avoid division by 0 and do not move if the player is touching nothing
        if (x==0 && y==0) {
            return 0;
        }

        float targetDirection;

        // the direction we want to face in [0, 2*pi]
        float targetDirection1 = (math.atan2(x, y) + math.PI2) % math.PI2;

        // if already in this direction, return
        if (targetDirection1 == tankRotation) {
            return 1;
        }

        // check for more than pi/2 and less than 0 to avoid useless 360°
        float targetDirection2 = (math.atan2(x, y) + math.PI2) % math.PI2 + math.PI2;
        float targetDirection3 = (math.atan2(x, y) + math.PI2) % math.PI2 - math.PI2;

        // find closest rotation
        if (math.abs(tankRotation - targetDirection1) < math.abs(tankRotation - targetDirection2)) {
            if (math.abs(tankRotation - targetDirection1) < math.abs(tankRotation - targetDirection3)) {
                targetDirection = targetDirection1;
            } else {
                targetDirection = targetDirection3;
            }
        } else if (math.abs(tankRotation - targetDirection2) < math.abs(tankRotation - targetDirection3)) {
            targetDirection = targetDirection2;
        } else {
            targetDirection = targetDirection3;
        }

        // apply rotation
        float forceRotation = (targetDirection - tankRotation) / rotationSpeed;
        if (math.abs(forceRotation)>1) {
            if (forceRotation>0) 
                this.RotateTank(1);
            else
                this.RotateTank(-1); 
        }else {
            this.SetRotationTank(targetDirection);
        }
    
        // if too far from target position, do not move
        if (math.abs(targetDirection - tankRotation) > MIN_ROTATION_BEFORE_MOVEMENT) {
            return 0;
        } else {
            // else, set the movement force proportionally inverse to the difference between the target angle and the current angle
            if (math.abs(targetDirection - tankRotation) == 0) { // avoid division by 0
                return 1;
            } else {
                return 1 - (math.abs(targetDirection - tankRotation) / MIN_ROTATION_BEFORE_MOVEMENT);
            }
        }
    }
}
