using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Networking;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
public class ARPlaceAndInteract : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject m_PlacedPrefab;

    [SerializeField]
     // Add this line at the top of your class
    private OpenAIController openAIController;

    // Add this variable to your class to reference the glow material
    [SerializeField]
    private Material glowMaterial;

    private bool isLoading = false;
    ARRaycastManager m_RaycastManager;
    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();
    GameObject spawnedObject;
    bool m_Pressed;

    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
    }

    // Add this method to your class to start the glow animation
// Coroutine to animate the albedo opacity for a loading effect


    void Update()
{
    if(Input.touchCount > 0)
    {
        Touch touch = Input.GetTouch(0);

        if(touch.phase == UnityEngine.TouchPhase.Began)
        {
            var touchPosition = touch.position;

            // Create a ray from the camera to the touch position
            Ray ray = Camera.main.ScreenPointToRay(touchPosition);
            RaycastHit hit;

            // Perform the raycast
            if (Physics.Raycast(ray, out hit))
            {
            ARPlane arPlane;
                if(hit.collider.gameObject.TryGetComponent<ARPlane>(out arPlane))
                {
                    
                   
                        // If the raycast hit an ARPlane, move or spawn the object
                        if (spawnedObject == null)
                        {
                            spawnedObject = Instantiate(m_PlacedPrefab, hit.point, Quaternion.identity);
                        }
                        else
                        {
                            spawnedObject.transform.position = hit.point;
                        }

                }
                else if(hit.collider.gameObject == spawnedObject)
                {

                    Debug.Log("OpenAI Request sent");
                     // Simulate sending a request to OpenAI
                    openAIController.GetShortResponseForObject(() => 
                    {
                        // This is a callback that should be called when the OpenAI text is ready
                        isLoading = false; // This will stop the animation
                    });

                        // Start the glow animation
                    StartAlbedoOpacityAnimation();
                    
                    return;
                }
                else{
                    Debug.Log(hit.collider.gameObject.ToString());
                }
            }
        }
    }
}

private IEnumerator AlbedoOpacityAnimation()
{
    // Check if spawnedObject is not null
    if (spawnedObject == null)
    {
        Debug.LogError("spawnedObject is null.");
        yield break;
    }

    // Safely get the Renderer component
    Renderer renderer = spawnedObject.GetComponent<GlowComponent>().glowGameObject.GetComponent<Renderer>();
    if (renderer == null)
    {
        Debug.LogError("Renderer component not found on spawnedObject.");
        yield break;
    }

    // Check if the material is not null
    if (renderer.material == null)
    {
        Debug.LogError("Material on renderer is null.");
        yield break;
    }

    Material instanceMaterial = new Material(renderer.material);
    renderer.material = instanceMaterial;

    Color originalColor = instanceMaterial.color;
    float direction = 1f;

    isLoading = true;
    while (isLoading)
    {
        // Safely modify the material's color
        if (instanceMaterial != null)
        {
            float alpha = instanceMaterial.color.a + (Time.deltaTime * direction);
            if (alpha > 0.3f) { alpha = 0.3f; direction = -1f; }
            if (alpha < 0f) { alpha = 0f; direction = 1f; }

            Color newColor = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            instanceMaterial.color = newColor;
        }
        else
        {
            Debug.LogError("Instance material has been destroyed or is null.");
            yield break;
        }
        yield return null;
    }

    // Restore the original material
    if (renderer != null && originalColor != null)
    {
        renderer.material.color = originalColor;
    }
    else
    {
        Debug.LogError("Renderer or original color is null, cannot restore material.");
    }
}

// Call this method to start the opacity animation
private void StartAlbedoOpacityAnimation()
{
    StartCoroutine(AlbedoOpacityAnimation());
}

}