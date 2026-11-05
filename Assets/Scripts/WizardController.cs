using UnityEngine;
using UnityEngine.AI;

public class WizardController : MonoBehaviour
{

    [SerializeField] Transform player;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] float chaseSpeed = 4.5f;
    [SerializeField] float attackRange = 20.6f;     // perto assim: ataca
    [SerializeField] float attackCooldown = 3.2f;  // tempo entre um golpe e outro
    [SerializeField] float attackDuration = 2.8f;  // quanto dura a animação do golpe (menor que o cooldown)


    public bool IsDead { get; private set; }

    NavMeshAgent agent;
    Animator animator;

    State current;        
    ChaseState chase;
    AttackState attack;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Cria os estados uma vez só e reaproveita
        chase = new ChaseState(this);
        attack = new AttackState(this);
    }

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        ChangeState(chase); 
    }

    // Update is called once per frame
    void Update()
    {
        current.Execute();
    }

    void ChangeState(State next)
    {
        current = next;
        current.Enter();        // prepara o novo estado (velocidade, animação...)
    }

    float DistanceToPlayer()
    {
        return Vector3.Distance(transform.position, player.position);
    }
    public void ThrowProjectile()
    {
        GameObject newProjectile = Instantiate(projectilePrefab);
        newProjectile.transform.position = new Vector3(
            transform.position.x,
            transform.position.y + 1.5f,
            transform.position.z
        );
    }


    abstract class State
    {
        protected WizardController ai;
        protected State(WizardController ai) { this.ai = ai; }

        public abstract void Enter();
        public abstract void Execute();
    }

    class ChaseState : State
    {
        public ChaseState(WizardController ai) : base(ai) { }

        public override void Enter()
        {
            ai.agent.isStopped = false;
            ai.agent.speed = ai.chaseSpeed;
            ai.agent.updateRotation = true;
        }

        public override void Execute()
        {
            ai.agent.SetDestination(ai.player.position);   // atualiza o destino todo frame

            float dist = ai.DistanceToPlayer();

            if (dist <= ai.attackRange)
                ai.ChangeState(ai.attack);  
        }
    }

    class AttackState : State
    {
        float timer;    // conta o tempo desde o último golpe

        public AttackState(WizardController ai) : base(ai) { }

        public override void Enter()
        {
            //ThrowProjectile();
            ai.agent.isStopped = true;          // para de andar
            ai.agent.updateRotation = false;
            ai.agent.velocity = Vector3.zero;
            timer = ai.attackCooldown;          // já começa pronto: bate logo ao chegar
        }

        public override void Execute()
        {
            timer += Time.deltaTime;

            if (timer < ai.attackDuration)
                return;

            if (ai.DistanceToPlayer() > ai.attackRange * 1.2f)
            {
                ai.ChangeState(ai.chase);
                return;
            }

            // Olha pro player, sem inclinar pra cima/baixo
            Vector3 lookPos = ai.player.position;
            lookPos.y = ai.transform.position.y;
            ai.transform.LookAt(lookPos);

            // Passou o cooldown? Bate e zera o contador.
            if (timer >= ai.attackCooldown)
            {
                ai.animator.ResetTrigger("Attack");
                ai.animator.SetTrigger("Attack");
                timer = 0f;
            }
        }
    }

    public void Die()
    {
        if (IsDead)
            return;
        IsDead = true;

        agent.enabled = false;      // para de andar

        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = false;      // não bloqueia nem recebe mais golpes

        animator.SetTrigger("Death");
        animator.SetBool("IsDead", true);

        enabled = false;            // desliga o Update: a IA para
        Destroy(gameObject, 5f);    // some da cena depois de 5 segundos
    }
}
