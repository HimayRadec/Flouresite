using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    private Camera cam;
    [SerializeField]
    private float distance = 3f;
    [SerializeField]
    private LayerMask mask;
    private PlayerUI playerUI;

    // Start is called before the first frame update
    void Start()
    {

        // need this explained to me
        playerUI = GetComponent<PlayerUI>();

    }

    // Update is called once per frame
    void Update()
    {
        // Removes the interactable text 
        playerUI.UpdateText(string.Empty);

        // creates a ray at the center of the camera location and shoots it forward,
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * distance);

        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, distance, mask))
        {
            if (hitInfo.collider.GetComponent<Interactable>() != null)
            {
                // By doing this we can access the interactble component using the variable
                Interactable interactable = hitInfo.collider.GetComponent<Interactable>();
                playerUI.UpdateText(interactable.promptMessage);

                // Why is his OnFoot and mine onFoot
                /*
                if (inputManager.onFoot.Interact.triggered)
                {
                    interactable.BaseInteract();
                }
                */
            }
        }


    }
}
