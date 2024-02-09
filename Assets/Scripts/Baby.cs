using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Seeker))]
[RequireComponent(typeof(Rigidbody2D))]
public class Baby : MonoBehaviour {

    [Header("General")]
    public float speed = 1f;
	public float crawlTime = 5f; //Time before baby will start crawling towards mother
    public float cryTime = 10f; //Time before baby will start crying
	public float neglectRate = 3; //Amount of health that the baby will lose per second it is left crying
    [Header("Pathfinding")]
    public float nextWaypointDistance = 1f;
    public float pathUpdateFrequency = 0.5f;

    private GameManager gameManager;
    private Transform player;
    private PlayerController playerController;
    private Animator anim;
	private AudioSource audio;

    private Path path;
    private Seeker seeker;
    private Rigidbody2D rb;
    private int currentWaypoint;
    private bool reachedPathEnd = false;
    private float lastPathUpdate = float.NegativeInfinity;
    private Vector2 targetPosition;

	private bool crawling = false, crying = false;
	private float lastCry = float.NegativeInfinity;

    // Start is called before the first frame update
    private void Start() {
		gameManager = FindObjectOfType<GameManager>();
		seeker = GetComponent<Seeker>();
		rb = GetComponent<Rigidbody2D>();
		playerController = FindObjectOfType<PlayerController>();
		player = playerController.transform;
		anim = GetComponent<Animator>();
		audio = GetComponent<AudioSource>();
		StartCoroutine(CrawlTimer());
		StartCoroutine(CryTimer());
	}

    private void FixedUpdate() {
        if (crawling && Vector2.Distance(transform.position, player.position) > 1f) {
			if (Time.time >= lastPathUpdate + pathUpdateFrequency) {
				targetPosition = player.position;
				UpdatePath();
				lastPathUpdate = Time.time;
			}
			anim.enabled = true;
			anim.SetBool("Crawling", true);
			MoveToTarget();
		} else {
			anim.enabled = false;
		}

        if (crying) {
			if (!audio.isPlaying) audio.Play();
            if (Time.time >= lastCry + 1f) {
				lastCry = Time.time;
				gameManager.babyHealth -= neglectRate;
			}
		}
	}

    private IEnumerator CrawlTimer() {
		yield return new WaitForSeconds(crawlTime);
		crawling = true;
	}

	private IEnumerator CryTimer() {
		yield return new WaitForSeconds(cryTime);
		crying = true;
	}

	//Pathfinding Function - Updates the path
	private void UpdatePath() {
		if (seeker.IsDone()) seeker.StartPath(rb.position, targetPosition, OnPathComplete);
	}

	//Function called when path is generated.
	private void OnPathComplete(Path p) {
		if (!p.error) {
			path = p;
			currentWaypoint = 0;
		}
	}

	private void MoveToTarget() {
		if (path == null) return;
		if (currentWaypoint >= path.vectorPath.Count) {
			reachedPathEnd = true;
			return;
		}
		else reachedPathEnd = false;

		//Movement
		if (currentWaypoint != 0) { //Fixes a glitch where ai will twitch as they rotate when the path updates
			Vector2 lookTarget = (Vector2)path.vectorPath[currentWaypoint];
			Vector2 lookDir = (lookTarget - rb.position).normalized;
			float lookAngle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 180f;
			float rotateVelocity = 0.0f;
			rb.rotation = Mathf.SmoothDampAngle(rb.rotation, lookAngle, ref rotateVelocity, 3f * Time.fixedDeltaTime);

			Vector2 moveDir = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
			Vector2 moveForce = moveDir * speed * Time.fixedDeltaTime;
			rb.MovePosition(rb.position + moveForce);
		}

		//More pathing
		float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
		if (distance < nextWaypointDistance) {
			++currentWaypoint;
		}
	}
}
