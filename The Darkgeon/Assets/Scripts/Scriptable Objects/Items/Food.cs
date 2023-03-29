using UnityEngine;

[CreateAssetMenu(fileName = "New Food", menuName = "Inventory/New Food")]
public class Food : Item
{
	[Header("Healing")]
	[Space]

	public int health;

	public override void Use()
	{
		base.Use();

		GameObject.FindWithTag("Player").GetComponent<PlayerStats>().Heal(health);

		Inventory.instance.UpdateQuantity(id, -1);
	}

	public override string ToString()
	{
		return base.ToString() +
				$"\n+{health} HP.\n\n" +
				$"Right Click to consume.";
	}
}
