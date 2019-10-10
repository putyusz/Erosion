using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreePool : MonoBehaviour {

    private static readonly int numberOfTrees = 1000;
    public GameObject tree;
    private static GameObject[] forest;

	void Start () {
        forest = new GameObject[numberOfTrees];
        for (int i = 0; i < numberOfTrees; i++)
        {
            forest[i] = Instantiate(tree, Vector3.zero, Quaternion.identity);
            forest[i].SetActive(false);
        }
	}
	
    public static GameObject GetTree()
    {
        for (int i = 0; i < numberOfTrees; i++)
        {
            if (!forest[i].activeSelf)
            {
                return forest[i];
            }
        }
        return null;
    }


	// Update is called once per frame
	void Update () {
		
	}
}
