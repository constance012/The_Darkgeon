using UnityEngine;

public class MenuChest : MonoBehaviour
{
	[Header("References")]
	[Space]
	[SerializeField] private Animator animator;
	[SerializeField] private Material mat;

	private bool isOpened;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		mat = GetComponentInChildren<SpriteRenderer>().sharedMaterial;
	}

	private void OnMouseDown()
	{
		if (!isOpened)
		{
			animator.SetTrigger("Open");
			isOpened = true;
		}
		else
		{
			animator.SetTrigger("Close");
			isOpened = false;
		}
	}

	private void OnMouseEnter()
	{
		mat.SetFloat("_Thickness", .002f);
	}

	private void OnMouseExit()
	{
		mat.SetFloat("_Thickness", .0f);
	}
}
