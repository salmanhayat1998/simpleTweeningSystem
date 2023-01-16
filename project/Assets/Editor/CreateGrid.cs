using UnityEditor;
using UnityEngine;

public class CreateGrid : ScriptableWizard
{
    public int totalCount = 100;
    public float spacing = 1;
    public GameObject prefab;

    [MenuItem("My Tools/Create Grid of Objects")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<CreateGrid>("Create Grid", "Create");
    }

    void OnWizardCreate()
    {
        GameObject parent = new GameObject("Grid");

        int columns = (int)Mathf.Sqrt(totalCount);
        int rows = (int)Mathf.Ceil(totalCount / (float)columns);

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                int index = (y * columns) + x;
                if (index >= totalCount)
                {
                    break;
                }
                GameObject instance = Instantiate(prefab);
                instance.transform.parent = parent.transform;
                instance.transform.position = new Vector3(x * spacing, 0, y * spacing);
            }
        }
    }
}
