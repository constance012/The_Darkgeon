namespace CSTGames.CommonEnums
{
	/// <summary>
	/// Represents types of treasure and the their maximum quantity when generated in a chest.
	/// </summary>
	public enum ItemCategory
	{
		Null,
		Equipment,
		Weapon,
		Food,
		Potion,
		Material,
		Mineral,
		MonsterPart,
		Coin,
		Special
	}

	/// <summary>
	/// An enum represents the player's kill sources, displays as an message when the player died.
	/// </summary>
	public enum KillSources
	{
		Unknown,
		Environment,
		Bat,
		Crab,
		Golem,
		ReinforcedGolem,
		Rat,
		Skull,
		SpikedSlime
	}

	/// <summary>
	/// Represents different actions in the game associated with different control keys.
	/// </summary>
	public enum KeybindingActions
	{
		None,
		PrimaryAttack,
		SecondaryAttack,
		MoveLeft,
		MoveRight,
		ClimbUp,
		ClimbDown,
		Jump,
		Crouch,
		Dash,
		Inventory,
		Interact,
		Pause
	}
}