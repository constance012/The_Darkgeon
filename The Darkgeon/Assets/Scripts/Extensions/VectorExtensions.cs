using UnityEngine;

public static class VectorExtensions
{
	public static Vector3 FlipByScale(this Vector3 scale, char axis)
	{
		Vector3 temp = scale;
		axis = char.ToLower(axis);

		switch (axis)
		{
			case 'x':
				temp.x *= -1;
				break;

			case 'y':
				temp.y *= -1;
				break;

			case 'z':
				temp.z *= -1;
				break;

			default:
				return scale;
		}

		return temp;
	}

	/// <summary>
	/// Adds or subtracts a value to a specified component of this vector.
	/// <para />
	/// Positive value will add, negative value will subtract.
	/// </summary>
	/// <param name="original"></param>
	/// <param name="axis"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public static Vector3 AddOrSubstractComponent(this Vector3 original, char axis, float value)
	{
		axis = char.ToLower(axis);

		switch (axis)
		{
			case 'x':
				return new Vector3(original.x + value, original.y, original.z);
			case 'y':
				return new Vector3(original.x, original.y + value, original.z);
			case 'z':
				return new Vector3(original.x, original.y, original.z + value);
			default: 
				return original;
		}
	}
}
