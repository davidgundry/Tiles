using UnityEngine;
using System.Collections;

public interface Clickable {

    void OnClickFromCamera(Vector3 point);

    void OnRightClickFromCamera(Vector3 point);

    void OnClickUpFromCamera(Vector3 point);

    void OnRightClickUpFromCamera(Vector3 point);

    void OnMouseOverFromCamera(Vector3 point);
}
