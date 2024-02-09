using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
	public bool firedByPlayer = false;
	public int damage;

	private void OnCollisionEnter2D(Collision2D collision) {
		if (!firedByPlayer && collision.transform.tag == "Mom" && collision.transform.GetComponent<PlayerController>().baby == null) { //if hits Mom and she is holding baby
			//Do damage to baby
		} else if (firedByPlayer && collision.transform.tag == "Enemy") { //if hits enemy
			EnemyAI enemy = collision.transform.GetComponent<EnemyAI>();
			if ((enemy.health -= damage) <= 0) { //Do damage, check if dead
				enemy.Die();
			}
		}
		Destroy(gameObject);
	}

	private void OnTriggerEnter(Collider collider) { //Baby is a trigger, so need this too
		if (!firedByPlayer && collider.tag == "Baby") { //Can only hit baby if the player didn't fire it, no friendly fire
			//Do damage to baby
		}
	}
}
