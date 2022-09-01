using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Persistant : MonoBehaviour
{
	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
	}
}
