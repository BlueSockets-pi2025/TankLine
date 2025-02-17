using UnityEngine;

public class Tank : MonoBehaviour
{
    protected GameObject thisTank;
    protected Transform thisGun;

    void Start()
    {
        thisTank = gameObject;
        thisGun = thisTank.transform.Find("tankGun");
        thisGun.Translate(0,0,4);
    }

    void Update()
    {
        
    }
}
