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


    protected void GunTrackPlayerMouse() {
        // get the current scene camera
        Camera cam = Camera.main;

        // the tank position on the screen
        Vector3 tankPosOnScreen = cam.WorldToScreenPoint(thisTank.position);

        Vector3 difference = new Vector3(Input.mousePosition.x - tankPosOnScreen.x, Input.mousePosition.y - tankPosOnScreen.y, 0);
        difference.Normalize();

        float gunRotation = math.atan2(-difference.y, difference.x);
        this.SetRotationGun(gunRotation + math.PI/2);
    }
}
