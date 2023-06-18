using System.Collections.Generic;
using UnityEngine;

public static class ListExtensions
{
	public static T GetRandomItem<T>(this IList<T> list)
	{
		return list[Random.Range(0, list.Count)];
	}

	public static void Shuffle<T>(this IList<T> list)
	{
		for (int i = list.Count - 1; i > 1; i--)
		{
			int j = Random.Range(0, i + 1);  // Because of exclusive.
			var value = list[j];
			list[j] = list[i];
			list[i] = value;
		}
	}
}
