using System.Collections.Generic;
using UnityEngine;

public class JsonHelper
{
	public static T[] FromJsonArray<T>(string json)
	{
		string newJson = "{ \"array\": " + json + "}";
		Wrapper<T> wrapper = UnityEngine.JsonUtility.FromJson<Wrapper<T>>(newJson);
		return wrapper.array;
	}

   

    [System.Serializable]
	private class Wrapper<T>
	{
		public T[] array;
        
    }
}