using FishNet.Object;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : NetworkBehaviour {

    /// <summary> Number of rebound left to make </summary>
    [Range(0,100)]
    public int nbRebounds = 1;

    [HideInInspector]
    /// <summary> This bullet direction </summary>
    public Vector3 direction;

    /// <summary> This bullet speed movement (READONLY) </summary>
    [Range(1,10)]
    public float bulletSpeed = 6;

    /// <summary> The tank that shot this bullet </summary>
    [HideInInspector]
    public GameObject tankOwner = null;


    const string BREAKABLE_TAG = "BreakableWall";

    const int WALL_LAYER = 3;
    const int SHELL_LAYER = 9;
    const int PLAYER_LAYER = 10;


    /// <summary> This bullet GameObject </summary>
    protected GameObject thisBullet;
    /// <summary> This bullet mesh renderer </summary>
    protected Transform meshTransform;

    void Start() {
        thisBullet = gameObject;
        meshTransform = thisBullet.transform.Find("shell");
        meshTransform.rotation = Quaternion.Euler(0, (math.atan2(-direction.z, direction.x) + math.PI / 2) * Mathf.Rad2Deg, 9.648f);
    }

    /// <summary>
    /// Automatically called by unity every frame before the physic engine
    /// </summary>
    void FixedUpdate() {
        thisBullet.transform.Translate(bulletSpeed * Time.deltaTime * direction);
    }

    /// <summary>
    /// Automatically called by unity when this object rigidbody is in a collision with another object
    /// </summary>

    void OnCollisionEnter(Collision collision) {
        if (!base.IsController) return;

        /* 
            ############################## WALL COLLISIONS ############################## 
        */
        if (collision.gameObject.layer == WALL_LAYER) {

            // if breakable wall
            if (collision.gameObject.CompareTag(BREAKABLE_TAG)) {
                BreakableObject wall = collision.gameObject.GetComponent<BreakableObject>();

                wall.TakeDamage();
                if (base.IsServerInitialized)
                    Despawn(thisBullet);
            } 
            
            // if static wall
            else {

                // if all bounces have been made, delete
                if (nbRebounds <= 0) {
                    if (base.IsServerInitialized)
                        Despawn(thisBullet);
                    return;
                }

                // else, decrease the number of rebound that are left to make
                nbRebounds--;

                Vector3 relativeCollision = collision.GetContact(0).point - thisBullet.transform.position;

                // get the contact point direction
                // right-left direction
                if (math.abs(relativeCollision.x) > 0.0001f) {
                    direction.x = -direction.x;
                }

                // up-down direction
                else if (math.abs(relativeCollision.z) > 0.0001f) {
                    direction.z = -direction.z;
                }

                else {
                    Debug.LogError("Collision detected but not direction.");
                }
            }
        }

        /* 
            ############################## PLAYER COLLISIONS ############################## 
        */
        else if (collision.gameObject.layer == PLAYER_LAYER) {
            Tank hitTank = collision.gameObject.GetComponent<Tank>();
            
            if (hitTank != null) {
                hitTank.LoseSingleLife(); // the tank hit by the bullet lose a life
            }

            if (base.IsServerInitialized)
                Despawn(thisBullet);

            return;
        }

        /* 
            ############################## SHELL COLLISIONS ############################## 
        */
        else if (collision.gameObject.layer == SHELL_LAYER) {
            if (base.IsServerInitialized) {
                Despawn(collision.gameObject);
                Despawn(thisBullet);
            }
            return;
        }

        // update the bullet rotation
        meshTransform.rotation = Quaternion.Euler(0, (math.atan2(-direction.z, direction.x) + math.PI / 2) * Mathf.Rad2Deg, 9.648f);
    }

    /// <summary>
    /// Function called by unity before destroying the object. <br/>
    /// Used to decrease the number of shot fired by the tank owner when a bullet dies.
    /// </summary>
    public void OnDestroy() {
        // decrease the nb of bullet shot from the tank that shot this bullet
        if (tankOwner != null) {
            Tank_Player playerOwner = tankOwner.GetComponent<Tank_Player>();
            playerOwner.DecreaseNbBulletShot();
        }
    }
}
