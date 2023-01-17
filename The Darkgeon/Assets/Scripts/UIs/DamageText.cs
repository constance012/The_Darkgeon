using UnityEngine;
using TMPro;
using System.Runtime.CompilerServices;

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

	public float yVelocity = .5f;

	public float normalFontSize = .15f;
	public float critFontSize = .2f;
	
	public float disappearSpeed = 3f;
	public float aliveTime = 1f;
	
	private Color currentTextColor;
	private float smoothVel;
	private bool isCrit;

	private void Awake()
	{
		textMesh = GetComponent<TextMeshProUGUI>();
	}

	private void Update()
	{
		aliveTime -= Time.deltaTime;
		
		float selectedFontSize = normalFontSize;
		float yVel = yVelocity;

		// Gradually increase font size.
		if (isCrit)
		{
			selectedFontSize = critFontSize;
			yVel = .8f;
		}

		// Gradually move up.
		transform.position += yVel * Time.deltaTime * Vector3.up;
			
		textMesh.fontSize = Mathf.SmoothDamp(textMesh.fontSize, selectedFontSize, ref smoothVel, .05f);

		if (aliveTime <= 0f)
		{
			currentTextColor.a -= disappearSpeed * Time.deltaTime;
			textMesh.color = currentTextColor;

			if (currentTextColor.a < 0f)
				Destroy(gameObject);
		}
	}

	#region Generate Method Overloads
	// Default color is red, and parent is world canvas.
	public static DamageText Generate(GameObject prefab, Vector3 pos, string textContent)
	{
		Transform canvas = GameObject.Find("World Canvas").transform;
		GameObject dmgTextObj = Instantiate(prefab, pos, Quaternion.identity);
		dmgTextObj.transform.SetParent(canvas, true);

		DamageText dmgText = dmgTextObj.GetComponent<DamageText>();

		dmgText.Setup(Color.red, textContent);
		return dmgText;
	}

	// Default parent is world canvas.
	public static DamageText Generate(GameObject prefab, Vector3 pos, Color txtColor, bool isCrit, string textContent)
	{
		Transform canvas = GameObject.Find("World Canvas").transform;

		GameObject dmgTextObj = Instantiate(prefab, pos, Quaternion.identity);
		dmgTextObj.transform.SetParent(canvas, true);

		DamageText dmgText = dmgTextObj.GetComponent<DamageText>();

		dmgText.Setup(txtColor, textContent, isCrit);
		return dmgText;
	}

	public static DamageText Generate(GameObject prefab, Transform canvas, Vector3 pos, Color txtColor, bool isCrit, string textContent)
	{
		GameObject dmgTextObj = Instantiate(prefab, pos, Quaternion.identity);
		dmgTextObj.transform.SetParent(canvas, true);

		DamageText dmgText = dmgTextObj.GetComponent<DamageText>();
		
		dmgText.Setup(txtColor, textContent, isCrit);
		return dmgText;
	}
	#endregion

	private void Setup(Color txtColor, string textContent, bool isCritHit = false)
	{
		textMesh.text = textContent.ToUpper();
		currentTextColor = txtColor;
		textMesh.color = currentTextColor;
		textMesh.fontSize = 0f;

		isCrit = isCritHit;
	}
}
