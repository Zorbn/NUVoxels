using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class DebugInfo : MonoBehaviour
{
    private const float UpdateTime = 0.1f;
    
    private TMP_Text textMesh;
    private float updateTimer;
    
    private void Start()
    {
        textMesh = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        updateTimer += deltaTime;

        if (updateTimer >= UpdateTime)
        {
            updateTimer = 0;
            textMesh.text = $"{(int)(1.0f / deltaTime)} fps";
        }
    }
}
