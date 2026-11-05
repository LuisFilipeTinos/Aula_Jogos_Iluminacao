using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private float velocity = 10f;
    [SerializeField] private float lifeTime = 5f;    

    private Vector3 direcao;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            direcao = (new Vector3(
                 player.transform.position.x - transform.position.x,
                 (player.transform.position.y - transform.position.y) + 1.5f,
                 player.transform.position.z - transform.position.z)).normalized;
        }
        else
        {
            direcao = new Vector3(transform.forward.x, transform.forward.y + 2f, transform.forward.z);
        }

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += direcao * velocity * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}
