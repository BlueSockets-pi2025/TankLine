using Unity.Mathematics;
using UnityEngine;

public class Tank_Player : Tank
{
    protected override void Start()
    {
        // set thisTank to the GameObject this script is attached to
        thisTank = gameObject.transform;
        // get the "tankGun" child
        thisGun = thisTank.transform.Find("tankGun");


    }


    protected override void FixedUpdate()
    {
        float acceleration = Input.GetAxis("Vertical");
        float rotation = Input.GetAxis("Horizontal");

        this.GoForward(acceleration);
        this.RotateTank(rotation);
        this.GunTrackPlayerMouse();

        this.ApplyRotation();
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
