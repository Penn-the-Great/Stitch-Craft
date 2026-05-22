using UnityEngine;  
using UnityEngine.EventSystems;  
using UnityEngine.SceneManagement;
  
// Attach this C# script to your Windows Panel or its title bar  
public class HangerDragger : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{  

    public float returnSpeed = 10f;
    private bool shouldReturn = false;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
      
    }

       public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Pointer Down");
        // Optional: Add logic if needed when starting to drag
    }

  

      public void OnPointerUp(PointerEventData eventData)
    {
        // Start returning when mouse is released
        shouldReturn = true;
        Debug.Log("Return");
    }


    public void OnDrag(PointerEventData eventData)  
    {  
    // Get the current mouse position from the event data
    Vector3 mouseScreenPosition = eventData.position;
    // Convert screen position to world position
    Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
    // Keep the original Z position
    mouseWorldPosition.z = transform.position.z;
    // Set the object's position directly to follow the mouse precisely
    transform.position = mouseWorldPosition;
    }  

    void Update()
    {
        if(shouldReturn)
        {
             transform.position = Vector3.Lerp(transform.position, transform.position, Time.deltaTime * returnSpeed);
        }
      
        
    }

}
