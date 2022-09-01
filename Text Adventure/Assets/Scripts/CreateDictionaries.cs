using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

[ExecuteInEditMode]
public class CreateDictionaries : MonoBehaviour
{
    public bool run;
    // removes all words less then this length from sub dictionaries
    public int minWordLength = 3;
    // removes all words greater than this length from sub dictionaries
    public int maxWordLength = 8;
    // the path to the master dictionary file
    public string masterPath;
    // the path to the directory where the sub dictionaries will be stored
    public string subDirPath;
    // the name of the sub dictionaries that will be made, when appended with MIN-MAX_LETTER
    public string subPrefix = "dict";
    // Update is called once per frame
    void Update()
    {
        if (run)
		{
            createDictionaries();
            run = false;
		}
    }

    private void createDictionaries()
	{
        try
        {
            // Create an instance of StreamReader to read from a file.
            // The using statement also closes the StreamReader.
            using (StreamReader reader = new StreamReader(masterPath))
            {
                string line;
                char letter = 'A';
                StreamWriter writer = new StreamWriter(
                    subDirPath + "/" + subPrefix + minWordLength + "-" + maxWordLength + "_" + letter + ".txt");
                // Read and display lines from the file until the end of
                // the file is reached.
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length < 3 || line.Length > 8)
					{
                        continue;
					}

                    if (line[0] > letter)
					{
                        letter = line[0];
                        writer.Close();
                        writer = new StreamWriter(
                            subDirPath + "/" + subPrefix + minWordLength + "-" + maxWordLength + "_" + letter + ".txt");
                    }
                    writer.WriteLine(line);
                }
                writer.Close();
            }
        }
        catch (Exception e)
        {
            // Let the user know what went wrong.
            Debug.Log("The file could not be read:");
            Debug.Log(e.Message);
        }
    }
}
