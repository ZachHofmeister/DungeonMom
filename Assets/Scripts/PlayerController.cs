using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public float moveSpeed = 5f;
	public float healRate = 1f; //Baby health regen per second when baby is held
	public GameObject babyPrefab;
	public int weaponType = 1;

    [System.NonSerialized]
	public GameObject baby;
	[System.NonSerialized]
	public bool holdingBaby = true;

	private GameManager gameManager;
	private Rigidbody2D rb;
	private Animator anim;
	private GameObject weapon;
	private bool babyInRange;
	private float lastHeal = float.NegativeInfinity;

	// Start is called before the first frame update
	private void Start() {
		gameManager = FindObjectOfType<GameManager>();
		rb = GetComponent<Rigidbody2D>();
		anim = GetComponent<Animator>();

		anim.SetBool("HoldingBaby", holdingBaby); //Start out holding the baby
		baby = null;
		babyInRange = false;

		weapon = null; //Start out not holding weapon
	}

    // Update is called once per frame
    private void Update() {
		//Interaction
		if (Input.GetKeyDown(KeyCode.E)) {
			if (holdingBaby) { //Put baby down
				//Stop holding baby
				holdingBaby = false;
				anim.SetBool("HoldingBaby", false);
				//Put baby on the floor *gently*
				Vector2 babyPosition = new Vector2(Mathf.Floor(transform.position.x) + 0.5f, Mathf.Floor(transform.position.y) + 0.5f);
				Vector3 babyRotation = new Vector3(0, 0, Mathf.Round(transform.rotation.eulerAngles.z / 90) * 90);
				baby = Instantiate(babyPrefab, babyPosition, Quaternion.Euler(babyRotation)); //Places baby in center of standing tile, at the nearest 1/4 angle
				//Draw weapon, if you have one
				if (weaponType != 0) {
					anim.SetInteger("WeaponType", weaponType);
					weapon = Instantiate(Resources.Load<GameObject>("Weapons/" + weaponType), transform);
				}
			} else if (baby != null && babyInRange) { //Pick baby up
				//Hold baby
				holdingBaby = true;
				anim.SetBool("HoldingBaby", true);
				//The baby isn't on the floor anymore!
				Destroy(baby); //oh my!
				baby = null;
				//Put weapon away
				StartCoroutine(weapon.GetComponent<Weapon>().PutAway());
				weapon = null;
			} else { } //If, in the future, you want to do other stuff with the E key like opening doors, it goes here
		}
		if (weapon != null && (Input.GetMouseButtonDown(0) || (Input.GetMouseButton(0) && weapon.GetComponent<Weapon>().fullAuto))) { //Fire weapon
			StartCoroutine(weapon.GetComponent<Weapon>().Fire(true));
		}
	}

	private void FixedUpdate() {
		//Movement
		Vector2 movement;
		movement.x = Input.GetAxisRaw("Horizontal");
		movement.y = Input.GetAxisRaw("Vertical");
		movement = movement.normalized; //Normalize the movement, this will prevent diagonal motion from being faster
		rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);

		//Look/Aim
		Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		Vector2 lookDirection = mousePos - rb.position;
		float lookAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - 90f;
		//Debug.Log("mousePos: " + mousePos + " lookDirection: " + lookDirection + " lookAngle: " + lookAngle);
		rb.rotation = lookAngle;

		if (holdingBaby && Time.time >= lastHeal + 1f && gameManager.babyHealth < gameManager.babyMaxHealth) {
			lastHeal = Time.time;
			gameManager.babyHealth += healRate;
		}
	}

	private void OnTriggerEnter2D(Collider2D collider) {
		if(collider.tag == "Baby") babyInRange = true;
	}

	private void OnTriggerExit2D(Collider2D collider) {
		if(collider.tag == "Baby") babyInRange = false;
	}
}
