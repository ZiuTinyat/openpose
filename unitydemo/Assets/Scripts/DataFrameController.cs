using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataFrameController : MonoBehaviour {

    // Singleton
    private static DataFrameController instance = null;

    private AnimDataSet dataSet;

    private void Awake(){ instance = this; }

    // Interface
    public static void Init(string fileName) { instance.InitData(fileName); }
    public static AnimData GetCurrentFrame() { return new AnimData(""); } // TODO

    private void InitData(string fileName)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            dataSet = new AnimDataSet(dataAsJson);
            dataSet.isValid = true;
        }
        else
        {
            Debug.Log("File not exists");
        }
    }
}
