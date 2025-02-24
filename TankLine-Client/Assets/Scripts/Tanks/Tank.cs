using System;
using Unity.Mathematics;
using UnityEngine;

public class Tank : MonoBehaviour
{

    /// <summary>This object movement speed (base speed : 2.0)</summary>
    public float movementSpeed = 3.0f;
    /// <summary>This object rotation speed (base speed : pi/64)</summary>
    public float rotationSpeed = math.PI / 48;

    protected Transform thisTank;
    protected Transform thisGun;


    /// <summary> The whole tank current angle in radians </summary>
    protected float tankRotation = 0;

    /// <summary> The gun current angle in radians </summary>
    protected float gunRotation = 0;

    protected virtual void Start() {
        thisTank = gameObject.transform;
        thisGun = thisTank.transform.Find("tankGun");
    }
    protected virtual void FixedUpdate() {
        this.ApplyRotation();
    }

    /// <summary>
    /// Apply this script stored rotation to the unity gameObject transform
    /// </summary>
    public void ApplyRotation() {
        // Tank rotation
        float degreeRotation = tankRotation * Mathf.Rad2Deg; // transform radians to degrees
        thisTank.transform.rotation = Quaternion.Euler(0, degreeRotation, 0); // aply rotation

        // Tank gun rotation
        degreeRotation = gunRotation * Mathf.Rad2Deg; // transform radians to degrees
        thisGun.rotation = Quaternion.Euler(0, degreeRotation, 0);
    }


    /*
        -------------------- Rotations --------------------
    */

    /// <summary>
    /// Rotate the tank base relatively to its previous rotation. <br/>
    /// Note that it will not rotate the tank gun.
    /// For this purpose you might want to use `RotateGun()`  
    /// </summary>
    /// <param name="force">The rotation force</param>
    public void RotateTank(float force) {
        tankRotation = (tankRotation + (force*rotationSpeed) + math.PI*2) % (math.PI * 2);
    }

    /// <summary>
    /// Set the tank rotation.
    /// </summary>
    /// <param name="angle">The rotation angle in radians</param>
    public void SetRotationTank(float angle) {
        tankRotation = (float)(((angle % (Math.PI*2))+math.PI*2)%(math.PI*2));
    }

    /// <summary>
    /// Rotate the tank gun relatively to its previous rotation. <br/>
    /// </summary>
    /// <param name="force">The rotation force</param>
    public void RotateGun (float force) {
        gunRotation = (gunRotation + (force*rotationSpeed) + math.PI*2) % (math.PI * 2);
    }

    /// <summary>
    /// Set the tank gun rotation.
    /// </summary>
    /// <param name="angle">The rotation angle in radians</param>
    public void SetRotationGun(float angle)
    {
        gunRotation = (float)(((angle % (Math.PI*2))+math.PI*2)%(math.PI*2));
    }


    /*
        -------------------- Position --------------------
    */

    /// <summary>
    /// Set this tank position
    /// </summary>
    /// <param name="position">The new position</param>
    public void SetPosition(Vector3 position) {
        thisTank.transform.Translate(position);
    }

    /// <summary>
    /// Make the tank go forward (relative to rotation)
    /// </summary>
    /// <param name="force">The movement force (between -1 and 1)</param>
    public void GoForward(float force) {
        thisTank.Translate(force * movementSpeed * Time.deltaTime * Vector3.forward);
    }
}
