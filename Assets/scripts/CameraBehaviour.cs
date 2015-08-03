using UnityEngine;
using System.Collections;



[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(MatrixBlender))]
public class CameraBehaviour : MonoBehaviour {

    //MatrixBlending properties
    private Matrix4x4 ortho,
                    perspective;
    public float fov = 30f,
                        near = .1f,
                        far = 1000f;
    private float aspect;
    private MatrixBlender blender;
    private bool orthoOn;

    public Transform target;
    public Transform bounds;
    public float boundsDistance;

    //Camera orbit properties
    private float distance = 5.0f;
    public float xSpeed = 10;
    public float ySpeed = 100;
    public float orbitSpeed = 0.1f;

    public float xMoveSpeed = 2;
    public float yMoveSpeed = 2;
    public float autoMoveSpeed = 0.5f;

    public float zoomSpeed = 5f;
    public float perspectiveZoomSpeed = 20f;

    private float yMinLimit = 20f;
    private float yMaxLimit = 80f;

    public float distanceMin = 1f;
    public float distanceMax = 20f;

    private float x = 0.0f;
    private float y = 0.0f;

    private float sphereRadius = 40f;
    private float sphereRadiusMin = 40f;
    private float sphereRadiusMax = 100f;
    private float orthographicSphereRadius = 200f;

    private bool alreadyClicked = false;
    private bool noRightClickMove = true;
    private bool noLeftClickMove = true;
    private bool noMiddleClickMove = true;
    private bool orbit = false;
    private bool cameraMoving = false;
    private Vector3 cameraMoveTarget;

	// Use this for initialization
    public void Start ()
	{
        transform.LookAt(target);
        Vector3 angles = target.transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        aspect = (float)Screen.width / (float)Screen.height;
        ortho = Matrix4x4.Ortho(-distance * aspect, distance * aspect, -distance, distance, near, far);
        perspective = Matrix4x4.Perspective(fov, aspect, near, far);
        GetComponent<Camera>().projectionMatrix = ortho;
        orthoOn = true;
        blender = (MatrixBlender)GetComponent(typeof(MatrixBlender));
	}
	
	// Update is called once per frame
	void Update () {

	}

    void LateUpdate()
    {
        if (Input.GetKeyDown("o"))
            orbit = !orbit;
        if (Input.GetKeyDown("p"))
        {
            orthoOn = !orthoOn;
            if (orthoOn)
            {
                ortho = Matrix4x4.Ortho(-distance * aspect, distance * aspect, -distance, distance, near, far);
                blender.BlendToMatrix(ortho, 1f);
                sphereRadius = orthographicSphereRadius;
            }
            else
            {
                blender.BlendToMatrix(perspective, 1f);
                sphereRadius = distance * 17;
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -sphereRadius);
            }
        }
        if ((Input.GetMouseButton(0)) || (Input.GetMouseButton(1)))
        {
            orbit = false;
            cameraMoving = false;
        }

        if ((!orbit) && (!cameraMoving))
        {
            // Ideally you'd want some kind of stack, so you're always doing whatever for the most recent button down
            if ((Input.GetMouseButton(0)) && !(Input.GetMouseButton(1)) && !(Input.GetMouseButton(2)))
            {
                float xMove = Input.GetAxis("MoveCameraX");
                float yMove = Input.GetAxis("MoveCameraY");

                if (Mathf.Abs(xMove) + Mathf.Abs(yMove) > 0)
                    noLeftClickMove = false;

                
                if (!noLeftClickMove)
                { // If we're dragging the mouse

                }
            }
            else if ((Input.GetMouseButton(1)) && !(Input.GetMouseButton(0)) && !(Input.GetMouseButton(2)))
            {
                float xMove = Input.GetAxis("MoveCameraX") * xSpeed * 0.02f;
                float yMove = Input.GetAxis("MoveCameraY") * ySpeed * 0.02f;
                x += xMove;
                y -= yMove;
                y = ClampAngle(y, yMinLimit, yMaxLimit);

                Quaternion rotation = Quaternion.Euler(y, x, 0);

                target.transform.rotation = rotation;

                if (Mathf.Abs(xMove) + Mathf.Abs(yMove) > 0)
                    noRightClickMove = false;
            }
            else if ((Input.GetMouseButton(2)) && !(Input.GetMouseButton(0)) && !(Input.GetMouseButton(1)))
            {
                float xMove = Input.GetAxis("MoveCameraX") * -xMoveSpeed * distance * 0.02f;
                float yMove = Input.GetAxis("MoveCameraY") * -yMoveSpeed * distance * 0.02f;

                if (Mathf.Abs(xMove) + Mathf.Abs(yMove) > 0)
                    noMiddleClickMove = false;

                //TODO: Need a bit more wiggle room to prevent being stuck at the edge
                float squareDistanceFromCentre = Mathf.Pow(target.transform.position.x + xMove - bounds.transform.position.x, 2) + Mathf.Pow(target.transform.position.z + yMove - bounds.transform.position.z, 2);
                if (squareDistanceFromCentre < Mathf.Pow(boundsDistance, 2))
                    target.transform.Translate(new Vector3(xMove, yMove, 0));
            }
            else if (!(Input.GetMouseButton(2)) && !(Input.GetMouseButton(0)) && !(Input.GetMouseButton(1)))
            {
                // If we're just hovering
                RaycastHit hit;
                Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    Clickable clickable = hit.transform.gameObject.GetComponent<Clickable>();
                    if (clickable != null)
                        clickable.OnMouseOverFromCamera(hit.point);
                }
            }

            // Reset the CameraTarget position to the top of the object from the canera's perspective
            if ((Input.GetMouseButtonUp(1)) || (Input.GetMouseButton(2)))
            {
                Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                RaycastHit h = new RaycastHit();
                if (Physics.Raycast(ray, out h, 1000f))
                {
                    Vector3 camPos = transform.position;
                    target.transform.position = h.point;
                    transform.position = camPos;
                }
            }

            if (((Input.GetMouseButton(0)) || (Input.GetMouseButton(1))) && !alreadyClicked)
            {   // If we've just clicked the mouse
                alreadyClicked = true;
                RaycastHit hit;
                Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    Clickable clickable = hit.transform.gameObject.GetComponent<Clickable>();
                    if (clickable != null)
                        if (Input.GetMouseButton(0))
                            clickable.OnClickFromCamera(hit.point);
                        else
                            clickable.OnRightClickFromCamera(hit.point);
                }
            }
            else if (Input.GetMouseButton(2) && !alreadyClicked)
                alreadyClicked = true;
            else if (((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))) && (alreadyClicked))
            {   // If we've just released the mouse
                alreadyClicked = false;
                if ((noRightClickMove) || (noLeftClickMove) || (noMiddleClickMove))
                {
                    RaycastHit hit;
                    Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out hit))
                    {
                        Clickable clickable = hit.transform.gameObject.GetComponent<Clickable>();
                        if (clickable != null)
                        {
                            if ((Input.GetMouseButtonUp(0)) && (noLeftClickMove))
                                clickable.OnClickUpFromCamera(hit.point);
                            else if ((Input.GetMouseButtonUp(1)) && (noRightClickMove))
                                clickable.OnRightClickUpFromCamera(hit.point);
                        }
                        if ((Input.GetMouseButtonUp(2)) && (noMiddleClickMove))
                            moveCameraToPoint(hit.point);
                    }

                }
                noRightClickMove = true;
                noLeftClickMove = true;
                noMiddleClickMove = true;
            }
        }
        else if (orbit)
        {   // If Orbiting
            float xMove = orbitSpeed*xSpeed * 0.02f;
            float yMove = 0;// ySpeed * 0.02f;
            x += xMove;
            y -= yMove;
            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);

            target.transform.rotation = rotation;

            if (Mathf.Abs(xMove) + Mathf.Abs(yMove) > 0)
                noRightClickMove = false;

            // Let us auto move the camera during orbit
            if (Input.GetMouseButton(2) && !alreadyClicked)
                alreadyClicked = true;
            if (Input.GetMouseButtonUp(2) && alreadyClicked && noMiddleClickMove)
            {
                RaycastHit hit;
                Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                    moveCameraToPoint(hit.point);
            }
        }

        if (cameraMoving)
        {
            if (cameraMoveTarget != null)
            {
                target.transform.position = Vector3.MoveTowards(target.transform.position, cameraMoveTarget, autoMoveSpeed);
                if (target.transform.position == cameraMoveTarget)
                    cameraMoving = false;
            }
        }

        float scrollInput = Input.GetAxis("ZoomCamera");
        if (scrollInput != 0)
            if (orthoOn)
            {
                distance = Mathf.Clamp(distance - scrollInput* zoomSpeed, distanceMin, distanceMax);
                ortho = Matrix4x4.Ortho(-distance * aspect, distance * aspect, -distance, distance, near, far);
                blender.BlendToMatrix(ortho, 0.15f);
            }
            else
            {
                sphereRadius = Mathf.Clamp(sphereRadius - scrollInput * perspectiveZoomSpeed, sphereRadiusMin, sphereRadiusMax);
                transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -sphereRadius);
            }
    }

    public void moveCameraToPoint(Vector3 point)
    {
        cameraMoving = true;
        cameraMoveTarget = point;
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
