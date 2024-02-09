using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAI : MonoBehaviour {

	[Header("General")]
	public float speed = 3f;
	public float health = 50f;
	public float fov = 90f;
	public float viewDistance = 20f, senseDistance = 10f;
	public LayerMask viewMask;
	[Header("Pathfinding")]
	public float nextWaypointDistance = 1f;
	public float pathUpdateFrequency = 0.5f;
	[Header("Patrol state")]
	public float patrolRange = 5.0f;
	public float patrolFrequency = 5.0f;
	[Header("Attack state")]
	public float attackDamage = 5.0f;
	public float attackDistance = 1.0f;
	public float attackTime = 1.0f;
	[Header("Search state")]
	public float searchTime = 3.0f;

	public enum State { Patrol, Attack, Search, Kidnap, Evade }
	public State currentState = State.Patrol;

	private GameManager gameManager;
	private Transform player, baby;
	private PlayerController playerController;
	private Animator anim;

	private Path path;
	private Seeker seeker;
	private Rigidbody2D rb;
	private int currentWaypoint;
	private bool reachedPathEnd = false;
	private float lastPathUpdate = float.NegativeInfinity;
	private Vector2 targetPosition;
	private float playerDistance, babyDistance;
	private bool playerVisible = false, babyVisible = false;

	private float lastPatrol = float.NegativeInfinity;
	private float lastAttack = float.NegativeInfinity;
	private float searchStart = float.NegativeInfinity;
	
	private void Start() {
		gameManager = FindObjectOfType<GameManager>();
		seeker = GetComponent<Seeker>();
		rb = GetComponent<Rigidbody2D>();
		playerController = FindObjectOfType<PlayerController>();
		player = playerController.transform;
		anim = GetComponent<Animator>();
	}

	private void FixedUpdate() {
		//Calculate if the player is visible
		playerDistance = Vector2.Distance(transform.position, player.position);
		RaycastHit2D playerHit = new RaycastHit2D();
		if (playerDistance <= viewDistance) playerHit = Physics2D.Raycast(transform.position, player.position - transform.position, viewDistance, viewMask);

		float playerAngle = Vector3.Angle(player.position - transform.position, transform.up);
		if (playerHit.transform == player && (playerAngle < fov / 2 || playerDistance < senseDistance))
            playerVisible = true;
		else playerVisible = false;

		//Calculate if baby is visible
		if (playerController.baby != null) {
			baby = playerController.baby.transform;

			babyDistance = Vector2.Distance(transform.position, baby.position);
			RaycastHit2D babyHit = new RaycastHit2D();
            if (babyDistance <= viewDistance) babyHit = Physics2D.Raycast(transform.position, baby.position - transform.position, viewDistance, viewMask);

			float babyAngle = Vector3.Angle(baby.position - transform.position, transform.up);
			if (babyHit.transform == baby && (babyAngle < fov / 2 || babyDistance < 5f))
				babyVisible = true;
			else babyVisible = false;
		} else {
			baby = null;
			babyVisible = false;
		}

		//Calculate if dead
		if(health <= 0) Die();

		//Do actions based on state
		switch(currentState) {
			case State.Patrol: //Move to random positions
				Patrol();
				break;
			case State.Attack: //Move near player, attack them
				Attack();
				break;
			case State.Search: //Move to last known position, look around
				Search();
				break;
			case State.Kidnap: //Move to baby, pick baby up
				Kidnap();
				break;
			case State.Evade: //Run away from the player
				Evade();
				break;
			default: break;
		}
	}

	private void Patrol() {
		//State updaters
		if (babyVisible) {
			currentState = State.Kidnap;
			return;
		}
		if(playerVisible) {
			currentState = State.Attack;
			return;
		}
		//Pick a random point to patrol to and update the path once
		if (Time.time >= lastPatrol + patrolFrequency) {
			Vector2 randomPosition = (Vector2)transform.position + Random.insideUnitCircle * patrolRange;
			targetPosition = AstarPath.active.GetNearest(randomPosition, NNConstraint.Default).position;
			//Debug.DrawRay(transform.position, targetPosition - (Vector2)transform.position, Color.red, 5f);
			lastPatrol = Time.time;
			UpdatePath();
		}
		//Move to that point
		MoveToTarget();
	}

	private void Attack() {
		//State updaters
		if (babyVisible) {
			currentState = State.Kidnap;
			return;
		}
		if(!playerVisible) {
			currentState = State.Search;
			searchStart = Time.time;
			return;
		}
		//Pursue or attack
		if (playerDistance > attackDistance) { //If player is too far away
			//Target the player and update the path, as frequently as possible
			if(Time.time >= lastPathUpdate + pathUpdateFrequency) {
				targetPosition = player.position;
				UpdatePath();
				lastPathUpdate = Time.time;
			}
			//Move to player
			MoveToTarget();
			anim.ResetTrigger("Attack");
		} else { //Player is close enough, attack
			if(Time.time >= lastAttack + attackTime) {
				if(baby == null) { //if player is holding baby, deal reduced damage to baby
					anim.SetTrigger("Attack");
					gameManager.babyHealth -= attackDamage / 2;
					lastAttack = Time.time;
				} else {
					anim.SetTrigger("Attack");
					//Do some sort of attack to the player
				}
			}
		}
	}

	private void Search() {
		//State updaters
		if(babyVisible) {
			currentState = State.Kidnap;
			return;
		}
		if (playerVisible) {
			currentState = State.Attack;
			return;
		}
		MoveToTarget();
		//Timer for ending the search
		if(Time.time >= searchStart + searchTime) {
			currentState = State.Patrol;
		}
	}

	private void Kidnap() {
		//State updaters
		if(!babyVisible) {
			currentState = State.Search;
			searchStart = Time.time;
			return;
		}
		//Pursue or attack
		if(babyDistance > attackDistance) { //If baby is too far away
			//Target the baby and update the path, as frequently as possible
			if(Time.time >= lastPathUpdate + pathUpdateFrequency) {
				targetPosition = baby.position;
				UpdatePath();
				lastPathUpdate = Time.time;
			}
			//Move to baby
			MoveToTarget();
			anim.ResetTrigger("Attack");
		} else { //Baby is close enough, attack
			if(Time.time >= lastAttack + attackTime) {
				anim.SetTrigger("Attack");
				gameManager.babyHealth -= attackDamage;
				lastAttack = Time.time;
			}
		}
	}

	private void Evade() {

	}

	public void Die() {
		Destroy(gameObject);
	}

	//Pathfinding Function - Updates the path
	private void UpdatePath() {
		if(seeker.IsDone()) {
			seeker.StartPath(rb.position, targetPosition, OnPathComplete);
		}
	}
	//Function called when path is generated.
	private void OnPathComplete(Path p) {
		if(!p.error) {
			path = p;
			currentWaypoint = 0;
		}
	}

	private void MoveToTarget() {
		if(path == null) return;
		if(currentWaypoint >= path.vectorPath.Count) {
			reachedPathEnd = true;
			return;
		} else reachedPathEnd = false;

		//Movement
		if (currentWaypoint != 0) { //Fixes a glitch where ai will twitch as they rotate when the path updates
			Vector2 lookTarget = (Vector2)path.vectorPath[currentWaypoint];
			Vector2 lookDir = (lookTarget - rb.position).normalized;
			float lookAngle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
			float rotateVelocity = 0.0f;
			rb.rotation = Mathf.SmoothDampAngle(rb.rotation, lookAngle, ref rotateVelocity, 3f * Time.fixedDeltaTime);

			Vector2 moveDir = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
			Vector2 moveForce = moveDir * speed * Time.fixedDeltaTime;
			rb.MovePosition(rb.position + moveForce);
		}

		//More pathing
		float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
		if(distance < nextWaypointDistance) {
			++currentWaypoint;
		}
	}
}
