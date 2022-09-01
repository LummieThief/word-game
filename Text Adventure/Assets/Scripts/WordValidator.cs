using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class WordValidator : MonoBehaviour
{
    public static WordValidator Instance;
    public TextAsset[] dictionaries;

	private void Awake()
	{
        setupSingleton();
	}


	public bool hasWord(string word)
	{
        if (word[0] == ' ')
            return false;
        word = word.ToUpper();
        TextAsset dict = dictionaries[word[0] - 'A'];
        string text = dict.text;
        string pattern = "^" + word + "\r\n";
        return Regex.IsMatch(text, pattern, RegexOptions.Multiline);
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