using UnityEngine;
using TMPro;

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

	public static DamageText Generate(GameObject prefab, Transform canvas, Vector3 pos, Color txtColor, string textContent)
	{
		GameObject dmgTextObj = Instantiate(prefab, pos, Quaternion.identity);
		dmgTextObj.transform.SetParent(canvas, true);

		DamageText dmgText = dmgTextObj.GetComponent<DamageText>();
		
		dmgText.Setup(txtColor, textContent);
		return dmgText;
	}

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
