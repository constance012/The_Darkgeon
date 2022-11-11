using UnityEngine;
using TMPro;

/// <summary>
/// A class to generates an UI popup text.
/// </summary>
public class DamageText : MonoBehaviour
{
	// References.
	[Header("References.")]
	[Space]

	[SerializeField] private TextMeshProUGUI textMesh;
	
	// Fields.
	[Header("Fields.")]
	[Space]

	public float disappearTime = 0f;
	public float disappearSpeed = 3f;
	public float yVelocity = .5f;
	public float desiredFontSize = .15f;
	public const float aliveTime = 1f;
	public Color currentTextColor;

	float smoothVel;

	private void Awake()
	{
		textMesh = GetComponent<TextMeshProUGUI>();
	}

	private void Update()
	{
		transform.position += new Vector3(0f, yVelocity * Time.deltaTime);
		textMesh.fontSize = Mathf.SmoothDamp(textMesh.fontSize, desiredFontSize, ref smoothVel, .05f);

		if (Time.time > disappearTime)
		{
			currentTextColor.a -= disappearSpeed * Time.deltaTime;
			textMesh.color = currentTextColor;

			if (currentTextColor.a < 0f)
				Destroy(gameObject);
		}

	}

	#region Generate Method Overloads
	public static DamageText Generate(GameObject prefab, Vector3 pos, string textContent)
	{
		Transform canvas = GameObject.Find("World Canvas").transform;

		GameObject dmgTextObj = Instantiate(prefab, pos, Quaternion.identity);
		dmgTextObj.transform.SetParent(canvas, true);

		DamageText dmgText = dmgTextObj.GetComponent<DamageText>();

		dmgText.Setup(Color.red, textContent);
		return dmgText;
	}

	public static DamageText Generate(GameObject prefab, Vector3 pos, Color txtColor, string textContent)
	{
		Transform canvas = GameObject.Find("World Canvas").transform;

		GameObject dmgTextObj = Instantiate(prefab, pos, Quaternion.identity);
		dmgTextObj.transform.SetParent(canvas, true);

		DamageText dmgText = dmgTextObj.GetComponent<DamageText>();

		dmgText.Setup(txtColor, textContent);
		return dmgText;
	}

	public static DamageText Generate(GameObject prefab, Transform canvas, Vector3 pos, Color txtColor, string textContent)
	{
		GameObject dmgTextObj = Instantiate(prefab, pos, Quaternion.identity);
		dmgTextObj.transform.SetParent(canvas, true);

		DamageText dmgText = dmgTextObj.GetComponent<DamageText>();
		
		dmgText.Setup(txtColor, textContent);
		return dmgText;
	}
	#endregion

	private void Setup(Color txtColor, string textContent)
	{
		textMesh.text = "" + textContent;
		currentTextColor = txtColor;
		textMesh.color = currentTextColor;
		textMesh.fontSize = 0f;

		if (disappearTime == 0f)
			disappearTime = Time.time + aliveTime;
	}
}
