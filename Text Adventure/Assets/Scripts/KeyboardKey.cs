using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardKey : MonoBehaviour
{
	private GridManager gm;
	//private SpellChecker sc;
    public char key;

	private void Start()
	{
		gm = FindObjectOfType<GridManager>();
		//sc = FindObjectOfType<SpellChecker>();
	}
	public void onKeyClicked()
	{
		gm.receiveKey(key);
		//sc.receiveKey(key);
	}
}
