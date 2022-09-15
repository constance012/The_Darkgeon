using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
	// References.
	[Header("References.")]
	[Space]

	[SerializeField] private TextMeshPro textMesh;
	
	// Fields.
	[Header("Fields.")]
	[Space]

	public float disappearTime = 0f;
	public float disappearSpeed = 3f;
	public const float aliveTime = 1f;
	public float yVelocity = .5f;
	public Color currentTextColor;

	private void Awake()
	{
		textMesh = GetComponent<TextMeshPro>();
	}

	private void Update()
	{
		transform.position += new Vector3(0f, yVelocity * Time.deltaTime);

		if (Time.time > disappearTime)
		{
			currentTextColor.a -= disappearSpeed * Time.deltaTime;
			textMesh.color = currentTextColor;

			if (currentTextColor.a < 0f)
				Destroy(gameObject);
		}

	}

	public static DamageText Generate(GameObject prefab, Vector3 pos, Color txtColor, int damageAmount)
	{
		GameObject dmgTextTransform = Instantiate(prefab, pos, Quaternion.identity);
		DamageText dmgText = dmgTextTransform.GetComponent<DamageText>();
		
		dmgText.Setup(txtColor, damageAmount);
		return dmgText;
	}

	private void Setup(Color txtColor, int damageAmount)
	{
		textMesh.text = "" + damageAmount;
		currentTextColor = txtColor;
		textMesh.color = currentTextColor;

		if (disappearTime == 0f)
			disappearTime = Time.time + aliveTime;
	}
}
