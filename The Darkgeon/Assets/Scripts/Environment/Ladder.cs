using UnityEngine;

public class Ladder : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			Debug.Log("Enter.");
			PlayerMovement.useLadder = true;
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			Debug.Log("Exit.");
			PlayerMovement.useLadder = false;
		}
	}
}
