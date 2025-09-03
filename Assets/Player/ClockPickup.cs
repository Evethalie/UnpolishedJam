using UnityEngine;

public class ClockPickup : MonoBehaviour
{
    private float timeOffset;
    [SerializeField] private float amplitude = 0.5f;    
    [SerializeField] private float frequency = 2f;
    [SerializeField] private float num;

    private void Start()
    {
        timeOffset = Random.Range(0f, 2f * Mathf.PI); 
    }

    private void Update()
    {
        SineFloat();
    }

    private void SineFloat()
    {
        // Calculate the vertical position using a sine wave
        float newY = amplitude * Mathf.Sin(Time.time * frequency + timeOffset);
  
        // Update the object's position
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y + newY * Time.deltaTime,
            transform.position.z
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            UIManager.instance.levelTimer += num;
            Destroy(gameObject);
        }
    }
}
