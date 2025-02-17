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


        this.RotateTank(- math.PI / 4);
    }


    protected override void FixedUpdate()
    {
        float acceleration = Input.GetAxis("Vertical");
        float rotation = Input.GetAxis("Horizontal");

        this.GoForward(acceleration);
        this.ApplyRotation();
    }
}
