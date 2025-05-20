using UnityEngine;

public class Indicator : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeSelf && Camera.main != null)
        {
            Vector3 direction = transform.position - Camera.main.transform.position;
            direction.y = 0f; 

            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}
