using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTurret : MonoBehaviour
{
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3f;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;

    List<Vector3> laserPoints = new List<Vector3>();

    void Update()
    {
        TrackMouse();
        TurnBase();
        DrawLaser();
    }

    void TrackMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(cameraRay, out RaycastHit hit, 1000f, targetLayer))
        {
            crosshair.transform.forward = hit.normal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
        }
    }

    void TurnBase()
    {
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
        turretBase.transform.rotation = Quaternion.Slerp(
            turretBase.transform.rotation,
            lookRotation,
            Time.deltaTime * baseTurnSpeed
        );
    }

    void DrawLaser()
    {
        laserPoints.Clear();

        Vector3 startPos = barrelEnd.position;
        Vector3 direction = barrelEnd.forward;
        float maxDistance = 100f;
        int maxBounces = 3; // Change this for more or fewer reflections

        laserPoints.Add(startPos);

        for (int i = 0; i < maxBounces; i++)
        {
            if (Physics.Raycast(startPos, direction, out RaycastHit hit, maxDistance, targetLayer))
            {
                // Add the hit point
                laserPoints.Add(hit.point);

                // Reflect manually using the dot product
                direction = direction - 2 * Vector3.Dot(direction, hit.normal) * hit.normal;

                // Move to the new start position
                startPos = hit.point;
            }
            else
            {
                // If no hit, just draw the remaining laser
                laserPoints.Add(startPos + direction * maxDistance);
                break;
            }
        }

        // Update the LineRenderer
        line.positionCount = laserPoints.Count;
        line.SetPositions(laserPoints.ToArray());
    }
}
