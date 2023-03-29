using System.Linq;
using System;

/// <summary>
/// Provides some useful methods to manipulate String.
/// </summary>
public class StringUtility
{
	public static string AddWhitespaceBeforeCapital(string str)
	{
		return String.Concat(str.Select(x => Char.IsUpper(x) ? " " + x : x.ToString()))
								.TrimStart(' ');
	}

	public static string AddHyphenBeforeNumber(string str)
	{
		return String.Concat(str.Select(x => Char.IsDigit(x) ? "-" + x : x.ToString()))
								.TrimStart('-');
	}

	public static string ClearWhitespaces(string str)
	{
		return new string(str.ToCharArray()
			.Where(c => !Char.IsWhiteSpace(c))
			.ToArray());
	}
}