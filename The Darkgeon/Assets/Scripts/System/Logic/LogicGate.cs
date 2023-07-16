using UnityEngine;

public class LogicGate : LogicEvaluator
{
	[Header("Boolean Operator")]
	[Space]
	public BooleanOperator _operator;

	[SerializeField] private Mechanisms triggerA;
	[SerializeField] private Mechanisms triggerB;

	public override void Evaluate()
	{
		if (triggerA == null || triggerB == null)
		{
			Debug.LogWarning("Please asign both levers before trying to evaluate the logic gate.", this);
			return;
		}

		bool result = false;

		switch (_operator)
		{
			case BooleanOperator.AND:
				result = triggerA.Status && triggerB.Status;
				break;

			case BooleanOperator.OR:
				result = triggerA.Status || triggerB.Status;
				break;

			case BooleanOperator.NOR:
				result = !(triggerA.Status || triggerB.Status);
				break;

			case BooleanOperator.NAND:
				result = !(triggerA.Status && triggerB.Status);
				break;

			case BooleanOperator.XOR:
				result = triggerA.Status != triggerB.Status;
				break;

			case BooleanOperator.XNOR:
				result = triggerA.Status == triggerB.Status;
				break;
		}

		controlTarget.ExecuteRemoteLogic(result);
	}
}