using System;
using Unity.Mathematics;
using UnityEngine;

public class Bullet_Offline : MonoBehaviour
{

    const string BREAKABLE_TAG = "BreakableWall";
    const int WALL_LAYER = 3;
    const int SHELL_LAYER = 9;
    const int PLAYER_LAYER = 10;
    const float THRESHOLD_CORRECTION_DISTANCE = 2;

    /// <summary> This bullet direction </summary>
    public Vector3 direction;


    /// <summary> Number of rebound left to make </summary>
    [Range(0, 100)]
    public int nbRebounds = 1;

    /// <summary> This bullet speed movement (READONLY) </summary>
    [Range(1, 10)]
    public float bulletSpeed = 6;

    /// <summary> The tank that shot this bullet </summary>
    [HideInInspector]
    public GameObject tankOwner = null;

    /// <summary>The explosion alambic prefab</summary>
    public GameObject bulletExplosionVfxPrefab;

    /// <summary> The smoothness of the correction applied to the bullet position to match with the server 
    // (0 means no smoothness, almost teleport to right location, and 1 means high smoothness, almost no correction)</summary>
    [Range(0f, 1f)]
    public float correctionSmoothness = 0.8f;


    /// <summary> This bullet GameObject </summary>
    protected GameObject thisBullet;
    /// <summary> This bullet mesh renderer </summary>
    protected Transform meshTransform;
    /// <summary> The correction vector between this client bullet position and the server position </summary>

    void Awake()
    {
        thisBullet = gameObject;
        meshTransform = thisBullet.transform.Find("shell");
    }

    /// <summary>
    /// Automatically called by unity every frame before the physic engine
    /// </summary>
    void FixedUpdate()
    {
        thisBullet.transform.Translate(bulletSpeed * Time.deltaTime * direction);
        meshTransform.rotation = Quaternion.Euler(0, (math.atan2(-direction.z, direction.x) + math.PI / 2) * Mathf.Rad2Deg, 9.648f);
    }

    /// <summary>
    /// Automatically called by unity when this object rigidbody is in a collision with another object
    /// </summary>

    void OnCollisionEnter(Collision collision)
    {

        /* 
            ############################## WALL COLLISIONS ############################## 
        */
        if (collision.gameObject.layer == WALL_LAYER)
        {

            // if breakable wall
            if (collision.gameObject.CompareTag(BREAKABLE_TAG))
            {
                BreakableObject wall = collision.gameObject.GetComponent<BreakableObject>();

                wall.TakeDamage();
                Destroy(thisBullet);
            }

            // if static wall
            else
            {

                // if all bounces have been made, delete
                if (nbRebounds <= 0)
                {
                    Destroy(thisBullet);
                    return;
                }

                // else, decrease the number of rebound that are left to make
                nbRebounds--;

                Vector3 relativeCollision = collision.GetContact(0).point - thisBullet.transform.position;

                // get the contact point direction
                // right-left direction
                if (math.abs(relativeCollision.x) > 0.0001f)
                {
                    direction = new Vector3(-direction.x, 0, direction.z);
                }

                // up-down direction
                else if (math.abs(relativeCollision.z) > 0.0001f)
                {
                    direction = new Vector3(direction.x, 0, -direction.z);
                }

                else
                {
                    Debug.LogError("Collision detected but not direction.");
                }
            }
        }

        /* 
            ############################## PLAYER COLLISIONS ############################## 
        */
        else if (collision.gameObject.layer == PLAYER_LAYER)
        {
            //Tank_Offline hitTank = collision.gameObject.GetComponent<Tank_Offline>();
            Tank_Bot hitTank = collision.gameObject.GetComponent<Tank_Bot>();
            if (hitTank != null)
            {
                hitTank.LoseSingleLife();
            }

            Destroy(thisBullet);

            return;
        }

        /* 
            ############################## SHELL COLLISIONS ############################## 
        */
        else if (collision.gameObject.layer == SHELL_LAYER)
        {
            Destroy(collision.gameObject);
            Destroy(thisBullet);
            return;
        }
    }

    /// <summary>
    /// Function called by unity before destroying the object. <br/>
    /// Used to decrease the number of shot fired by the tank owner when a bullet dies.
    /// </summary>
    public void OnDestroy()
    {
        // decrease the nb of bullet shot from the tank that shot this bullet
        if (tankOwner != null)
        {
            Tank_Offline playerOwner = tankOwner.GetComponent<Tank_Offline>();
            if (playerOwner != null)
                playerOwner.DecreaseNbBulletShot();
        }

        if (Environment.GetEnvironmentVariable("IS_DEDICATED_SERVER") == "true") return; // only exec on client

#if UNITY_STANDALONE
        // play death vfx
        Instantiate(bulletExplosionVfxPrefab, transform.position, Quaternion.identity);
#endif
    }
}