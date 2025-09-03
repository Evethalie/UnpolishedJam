using UnityEngine;

public class RisingPlatform : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Vector2 startPosition;
    [SerializeField] private PlayerRespawn playerRespawn;
    public bool shouldReset = true;

    private void Awake()
    {
        startPosition = transform.position;
        playerRespawn = FindAnyObjectByType<PlayerRespawn>();
    }
    private void OnEnable()
    {
        playerRespawn.onManualRespawn += Reset;
    }

    private void OnDisable()
    {
        playerRespawn.onManualRespawn -= Reset;
    }
   

    // Update is called once per frame
    void Update()
    {
        transform.Translate(0, speed * Time.deltaTime, 0);
    }

    void Reset()
    {
        if (!shouldReset) return;
        transform.position = startPosition;
    }
}
