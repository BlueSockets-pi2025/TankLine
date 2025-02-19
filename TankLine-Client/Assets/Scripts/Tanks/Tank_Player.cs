using System;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Tank_Player : Tank
{

    public const int LEFT_CLICK = 0;
    public const int LAYER_MAP_OBJECT = 3;

    protected override void Start()
    {
        // set thisTank to the GameObject this script is attached to
        thisTank = gameObject.transform;
        // get the "tankGun" child
        thisGun = thisTank.transform.Find("tankGun");


    }


    protected override void FixedUpdate() {
        float acceleration = Input.GetAxis("Vertical");
        float rotation = Input.GetAxis("Horizontal");

        // process mouse aiming
        this.GunTrackPlayerMouse();

        // process movement input
        this.GoForward(acceleration);
        this.RotateTank(rotation);
        this.ApplyRotation();

        // process mouse click to fire a bullet
        if (Input.GetMouseButtonDown(LEFT_CLICK)) {
            Vector3 origin = new Vector3(thisTank.position.x, 0.5f, thisTank.position.z);
            Vector3 direction = new Vector3(math.cos(gunRotationAngle - math.PI / 2), 0, -math.sin(gunRotationAngle - math.PI / 2));

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
            if (rotationAngle < math.PI / 4 || rotationAngle > (7*math.PI / 4)) {
                // if rotation is close enough to target, set it to target
                if (math.abs(rotationAngle - math.PI) > (math.PI - rotationSpeed*0.3)) {
                    this.SetRotationTank(0);
                } 

                // else, slightly correct it to make the closest side stick to the wall
                else if ((rotationAngle - math.PI) > 0){
                    this.RotateTank(0.2f);
                } else {
                    this.RotateTank(-0.2f);
                }
            } 
            
            // Right correction
            else if (rotationAngle < (3*math.PI / 4)) {
                // if rotation is close enough to target, set it to target
                if (math.abs(rotationAngle - (math.PI)/2) < rotationSpeed*0.3) {
                    this.SetRotationTank(math.PI/2);
                } 

                // else, slightly correct it to make the closest side stick to the wall
                else if ((rotationAngle - (math.PI)/2) < 0){
                    this.RotateTank(0.2f);
                } else {
                    this.RotateTank(-0.2f);
                }
            }

            // Down correction
            else if (rotationAngle < (5*math.PI / 4)) {
                // if rotation is close enough to target, set it to target
                if (math.abs(rotationAngle - math.PI) < rotationSpeed*0.3) {
                    this.SetRotationTank(math.PI);
                } 

                // else, slightly correct it to make the closest side stick to the wall
                else if ((rotationAngle - math.PI) < 0){
                    this.RotateTank(0.2f);
                } else {
                    this.RotateTank(-0.2f);
                }
            }

            // Left correction
            else {
                // if rotation is close enough to target, set it to target
                if (math.abs((rotationAngle - (3*math.PI)/2)) < rotationSpeed*0.3) {
                    this.SetRotationTank((3 * math.PI) / 2);
                } 

                // else, slightly correct it to make the closest side stick to the wall
                else if ((rotationAngle - (3*math.PI)/2) < 0){
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
}
