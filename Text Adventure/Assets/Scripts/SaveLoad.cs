using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;

public class SaveLoad : MonoBehaviour
{
	public static SaveLoad Instance;
	public SaveState state;
	public bool resetSaveOnStart = false;

	private void Awake()
	{
		setupSingleton();
		if (resetSaveOnStart)
		{
			PlayerPrefs.DeleteAll();
		}
		Load();
	}

	public void Save()
	{
		state.initialized = true;
		PlayerPrefs.SetString("save", SerializeState(state));
	}

	public void Load()
	{
		if (PlayerPrefs.HasKey("save"))
		{
			state = DeserializeState(PlayerPrefs.GetString("save"));
		}
		else 
		{
			Debug.Log("No save found. Creating a new save");
			state = new SaveState();
		}
	}

	private string SerializeState(SaveState toSerialize)
	{
		XmlSerializer xml = new XmlSerializer(typeof(SaveState));
		StringWriter writer = new StringWriter();
		xml.Serialize(writer, toSerialize);
		return writer.ToString();
	}

	private SaveState DeserializeState(string toDeserialize)
	{
		XmlSerializer xml = new XmlSerializer(typeof(SaveState));
		StringReader reader = new StringReader(toDeserialize);
		return (SaveState)xml.Deserialize(reader);
	}

	private void setupSingleton()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}
}
