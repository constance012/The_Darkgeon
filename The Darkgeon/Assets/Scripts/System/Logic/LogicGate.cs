using UnityEngine;

public class LogicGate : LogicEvaluator
{
	[Header("Boolean Operator")]
	[Space]
	public BooleanOperator _operator;

	[SerializeField] private OnOffLever leverA;
	[SerializeField] private OnOffLever leverB;

	public override void Evaluate()
	{
		if (leverA == null || leverB == null)
		{
			Debug.LogWarning("Please asign both levers before trying to evaluate the logic gate.", this);
			return;
		}

		bool result = false;

		switch (_operator)
		{
			case BooleanOperator.AND:
				result = leverA.Status && leverB.Status;
				break;

			case BooleanOperator.OR:
				result = leverA.Status || leverB.Status;
				break;

			case BooleanOperator.NOR:
				result = !(leverA.Status || leverB.Status);
				break;

			case BooleanOperator.NAND:
				result = !(leverA.Status && leverB.Status);
				break;

			case BooleanOperator.XOR:
				result = leverA.Status != leverB.Status;
				break;

			case BooleanOperator.XNOR:
				result = leverA.Status == leverB.Status;
				break;
		}

		controlTarget.ExecuteRemoteLogic(result);
	}
}