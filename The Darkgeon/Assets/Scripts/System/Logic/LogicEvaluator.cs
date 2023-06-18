using UnityEngine;

[RequireComponent(typeof(Interactable))]
public abstract class LogicEvaluator : MonoBehaviour
{
	public enum BooleanOperator { AND, OR, NOR, NAND, XOR, XNOR }

	[Header("Target to Control")]
	[Space]
	[SerializeField] protected Interactable controlTarget;

	protected virtual void Awake()
	{
		controlTarget = GetComponent<Interactable>();
	}

	protected void Start()
	{
		Evaluate();
	}

	public abstract void Evaluate();
}
