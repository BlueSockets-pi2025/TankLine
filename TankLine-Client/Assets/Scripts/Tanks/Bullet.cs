using FishNet.Object;
using Unity.Mathematics;
using FishNet.Object.Synchronizing;
using UnityEngine;
using Unity.VisualScripting;
using System;

public class Bullet : NetworkBehaviour {

    const string BREAKABLE_TAG = "BreakableWall";
    const int WALL_LAYER = 3;
    const int SHELL_LAYER = 9;
    const int PLAYER_LAYER = 10;
    const float THRESHOLD_CORRECTION_DISTANCE = 2;


    /// <summary> Local variable to check if every client bullet position is correct with the server one </summary>
    private readonly SyncVar<Vector3> serverPosition = new(new SyncTypeSettings(.1f));

    /// <summary> This bullet direction </summary>
    public readonly SyncVar<Vector3> direction = new(new SyncTypeSettings(.1f));


    /// <summary> Number of rebound left to make </summary>
    [Range(0,100)]
    public int nbRebounds = 1;

    /// <summary> This bullet speed movement (READONLY) </summary>
    [Range(1,10)]
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
    protected Vector3 correctionWithServer;
    /// <summary> The server bullet position simulated from the last informations received </summary>
    protected Vector3 virtualServerPosition;

    void Awake() {
        // instanciate variables
        direction.Value = new Vector3(0,0,0);
        thisBullet = gameObject;
        meshTransform = thisBullet.transform.Find("shell");

        // functions called when the var is changed by another game instance
        direction.OnChange += OnDirectionChange;
        serverPosition.OnChange += OnServerPositionChange;
    }

    /// <summary>
    /// Automatically called by unity every frame before the physic engine
    /// </summary>
    void FixedUpdate() {
        // every 20 frames (1/3 seconds) send the server bullet position for correction
        if (base.IsServerInitialized && Time.frameCount % 20 == 0) {
            serverPosition.Value = thisBullet.transform.position;
        } 
        // else, correct this bullet instance position
        else {
            // if the difference is smaller than the correction, apply the difference
            if ((thisBullet.transform.position - virtualServerPosition).magnitude <= correctionWithServer.magnitude) {
                thisBullet.transform.position = virtualServerPosition;
            } else {
                thisBullet.transform.Translate(correctionWithServer);
            }
        }

        thisBullet.transform.Translate(bulletSpeed * Time.deltaTime * direction.Value);
        virtualServerPosition += bulletSpeed * Time.deltaTime * direction.Value;
    }

    /// <summary>
    /// Automatically called by unity when this object rigidbody is in a collision with another object
    /// </summary>

    void OnCollisionEnter(Collision collision) {
        if (Environment.GetEnvironmentVariable("IS_DEDICATED_SERVER") != "true") return;

        /* 
            ############################## WALL COLLISIONS ############################## 
        */
        if (collision.gameObject.layer == WALL_LAYER) {

            // if breakable wall
            if (collision.gameObject.CompareTag(BREAKABLE_TAG)) {
                BreakableObject wall = collision.gameObject.GetComponent<BreakableObject>();

                wall.TakeDamage();
                Despawn(thisBullet);
            } 
            
            // if static wall
            else {

                // if all bounces have been made, delete
                if (nbRebounds <= 0) {
                    Despawn(thisBullet);
                    return;
                }

                // else, decrease the number of rebound that are left to make
                nbRebounds--;

                Vector3 relativeCollision = collision.GetContact(0).point - thisBullet.transform.position;

                // get the contact point direction
                // right-left direction
                if (math.abs(relativeCollision.x) > 0.0001f) {
                    direction.Value = new Vector3(-direction.Value.x, 0, direction.Value.z);
                }

                // up-down direction
                else if (math.abs(relativeCollision.z) > 0.0001f) {
                    direction.Value = new Vector3(direction.Value.x, 0, -direction.Value.z);
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
            
            Tank_Player hitTank = collision.gameObject.GetComponent<Tank_Player>();

            FindAnyObjectByType<LobbyManager>().OnPlayerHit(hitTank.Owner, tankOwner.GetComponent<Tank_Player>().Owner);

            Despawn(thisBullet);

            return;
        }

        /* 
            ############################## SHELL COLLISIONS ############################## 
        */
        else if (collision.gameObject.layer == SHELL_LAYER) {
            Despawn(collision.gameObject);
            Despawn(thisBullet);
            return;
        }

        else {
            Debug.Log($"[WARNING] Bullet collided with non identified object at {transform.position}");
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
            Tank_Player playerOwner = tankOwner.GetComponent<Tank_Player>();
            playerOwner.DecreaseNbBulletShot();
        }

        if (Environment.GetEnvironmentVariable("IS_DEDICATED_SERVER") == "true") return; // only exec on client

        // play death vfx
        Instantiate(bulletExplosionVfxPrefab, transform.position, Quaternion.identity);
    }

    public void OnDirectionChange(Vector3 oldDir, Vector3 newDir, bool asServer) {
        // update the bullet rotation
        meshTransform.rotation = Quaternion.Euler(0, (math.atan2(-direction.Value.z, direction.Value.x) + math.PI / 2) * Mathf.Rad2Deg, 9.648f);
    }

    public void OnServerPositionChange(Vector3 oldPos, Vector3 newPos, bool asServer) {
        if (asServer) return;

        virtualServerPosition = newPos;

        // if distances is too high, teleport 
        if (Vector3.Distance(newPos, thisBullet.transform.position) > THRESHOLD_CORRECTION_DISTANCE) {
            thisBullet.transform.position = newPos;
            correctionWithServer = new Vector3(0,0,0);
        } else {
            correctionWithServer = newPos - thisBullet.transform.position;
            correctionWithServer *= 1f - correctionSmoothness;
        }
    }
}
