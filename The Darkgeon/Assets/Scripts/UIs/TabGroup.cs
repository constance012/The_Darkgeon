using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabGroup : MonoBehaviour
{
	public List<MenuTabButton> tabButtons;
	[SerializeField] private List<GameObject> pagesToSwap;

	private MenuTabButton currentTab;

	private Color idleColor = Color.white;
	private Color hoverColor = new Color(1f, .84f, .5f);
	private Color selectedColor = new Color(1f, .76f, 0f);

	public void Subscribe(MenuTabButton button)
	{
		if (tabButtons == null)
			tabButtons = new List<MenuTabButton> ();

		tabButtons.Add(button);
	}

	public void OnTabEnter(MenuTabButton target)
	{
		ResetTab();

		// Change the color to hover if the tab is not selected.
		if(currentTab == null || target != currentTab)
			target.text.color = hoverColor;
	}
	
	public void OnTabExit(MenuTabButton target)
	{
		ResetTab();
	}

	public void OnTabSelected(MenuTabButton target)
	{
		currentTab = target;
		ResetTab();
		target.text.color = selectedColor;

		int currentIndex = target.transform.GetSiblingIndex();

		for (int i = 0; i < pagesToSwap.Count; i++)
		{
			if (i == currentIndex)
				pagesToSwap[i].SetActive(true);
			else
				pagesToSwap[i].SetActive(false);
		}
	}

	private void ResetTab()
	{
		// Reset all other tabs except the selected one.
		foreach (MenuTabButton button in tabButtons)
		{
			if (currentTab != null && button == currentTab)
				continue;

			button.text.color = idleColor;
		}
	}
}
