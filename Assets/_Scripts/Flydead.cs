using UnityEngine;

public class Flydead : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    float speed = 6;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.up * Time.deltaTime * speed;
        if (transform.position.y > 6) {
            Destroy(gameObject);
        }
    }
}
