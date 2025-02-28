using Unity.Mathematics;
using UnityEngine;

public class Bullet : MonoBehaviour {

    /// <summary> Number of rebound left to make </summary>
    public int nbRebounds = 1;
    /// <summary> This bullet direction </summary>
    public Vector3 direction;
    /// <summary> This bullet speed movement (READONLY) </summary>
    public float bulletSpeed = 6;
    /// <summary> The tank that shot this bullet </summary>
    public GameObject tankOwner;


    /// <summary> The layer number for unbreakable walls </summary>
    public const int UNBREAKABLE_LAYER = 3;


    /// <summary> This bullet GameObject </summary>
    protected GameObject thisBullet;

    void Start() {
        thisBullet = gameObject;
    }

    /// <summary>
    /// Automatically called by unity every frame before the physic engine
    /// </summary>
    void FixedUpdate() {
        direction.Normalize();
        thisBullet.transform.Translate(bulletSpeed * Time.deltaTime * direction);
    }

    /// <summary>
    /// Automatically called by unity when this object rigidbody is in a collision with another object
    /// </summary>

    void OnCollisionEnter(Collision collision) {
        
        // if collision with unbreakable wall, bounce or destroy
        if (collision.gameObject.layer == UNBREAKABLE_LAYER) {

            // if all bounces have been made, delete
            if (nbRebounds <= 0) {
                Destroy(thisBullet);
                return;
            }
            nbRebounds--;

            Vector3 relativeCollision = collision.GetContact(0).point - thisBullet.transform.position;

            // get the contact point direction
            // right-left direction
            if (math.abs(relativeCollision.x) > 0.0001f) {
                direction.x = -direction.x;
            } else 

            // up-down direction
            if (math.abs(relativeCollision.z) > 0.0001f) {
                direction.z = -direction.z;
            }
        }
    }
}
