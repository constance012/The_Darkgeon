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

        FindObjectOfType<PlayerStats>().Heal(health);
    }
}
