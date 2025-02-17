using System;
using Unity.Mathematics;
using UnityEngine;

public class Tank : MonoBehaviour
{

    /// <summary>This object movement speed (base speed : 4)</summary>
    public float movementSpeed = 4.0f;

    protected Transform thisTank;
    protected Transform thisGun;


    /// <summary> The whole tank current angle in radians </summary>
    protected float rotationAngle = 0;

    /// <summary> The gun current angle in radians </summary>
    protected float gunRotationAngle = 0;

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
        float degreeRotation = rotationAngle * Mathf.Rad2Deg; // transform radians to degrees
        thisTank.transform.rotation = Quaternion.Euler(0, degreeRotation, 0); // aply rotation

        // Tank gun rotation
        degreeRotation = (gunRotationAngle) * Mathf.Rad2Deg; // transform radians to degrees
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
    /// <param name="angle">The rotation angle in radians</param>
    public void RotateTank(float angle) {
        rotationAngle = (rotationAngle + angle) % (math.PI * 2);
    }

    /// <summary>
    /// Set the tank rotation.
    /// </summary>
    /// <param name="angle">The rotation angle in radians</param>
    public void SetRotationTank(float angle) {
        rotationAngle = (float)(angle % (Math.PI*2));
    }

    /// <summary>
    /// Rotate the tank gun relatively to its previous rotation. <br/>
    /// </summary>
    /// <param name="angle">The rotation angle in radians</param>
    public void RotateGun (float angle) {
        gunRotationAngle = (gunRotationAngle + angle) % (math.PI * 2);
    }

    /// <summary>
    /// Set the tank gun rotation.
    /// </summary>
    /// <param name="angle">The rotation angle in radians</param>
    public void SetRotationGun(float angle)
    {
        rotationAngle = (float)(angle % (Math.PI * 2));
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
    public void GoForward(float force) {
        thisTank.Translate(Vector3.forward * movementSpeed * Time.deltaTime * force);
    }
}
