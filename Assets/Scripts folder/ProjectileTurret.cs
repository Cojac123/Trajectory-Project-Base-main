using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileTurret : MonoBehaviour
{
    // -----------------------------
    // ?? Inspector Variables (Editable in Unity)
    // -----------------------------
    [SerializeField] float projectileSpeed = 1f;
    [SerializeField] Vector3 gravity = new Vector3(0, -9.8f, 0);
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3f;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;
    [SerializeField] bool useLowAngle = true;

    // Stores each position point of the arc
    List<Vector3> points = new List<Vector3>();

    // -----------------------------
    // ?? Unity Update Loop
    // -----------------------------
    void Update()
    {
        TrackMouse();     // Aim toward where the mouse hits
        TurnBase();       // Rotate base to face that direction
        RotateGun();      // Aim the gun barrel up/down
        DrawTrajectory(); // ?? Draw the curved path preview

        // Fire projectile on left-click
        if (Input.GetButtonDown("Fire1"))
            Fire();
    }

    // -----------------------------
    // ?? Fire projectile forward
    // -----------------------------
    void Fire()
    {
        GameObject projectile = Instantiate(projectilePrefab, barrelEnd.position, gun.transform.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.linearVelocity = projectileSpeed * barrelEnd.forward; // ? FIXED (was "linearVelocity")
    }

    // -----------------------------
    // ??? Aim at where the mouse hits
    // -----------------------------
    void TrackMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(cameraRay, out RaycastHit hit, 1000f, targetLayer))
        {
            crosshair.transform.forward = hit.normal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
        }
    }

    // -----------------------------
    // ?? Rotate base horizontally
    // -----------------------------
    void TurnBase()
    {
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));
        turretBase.transform.rotation = Quaternion.Slerp(turretBase.transform.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
    }

    // -----------------------------
    // ?? Rotate gun vertically
    // -----------------------------
    void RotateGun()
    {
        float? angle = CalculateTrajectory(crosshair.transform.position, useLowAngle);
        if (angle != null)
            gun.transform.localEulerAngles = new Vector3(360f - (float)angle, 0, 0);
    }

    // -----------------------------
    // ?? Draw projectile path line
    // -----------------------------
    void DrawTrajectory()
    {
        points.Clear();

        Vector3 startPos = barrelEnd.position;
        Vector3 startVelocity = projectileSpeed * barrelEnd.forward;
        float timeStep = 0.1f;  // Smaller = smoother curve
        float maxTime = 5f;

        for (float t = 0; t < maxTime; t += timeStep)
        {
            // Calculate next point using kinematic equation
            Vector3 newPoint = startPos + (startVelocity * t) + (0.5f * gravity * t * t);

            // Stop drawing if it hits something
            if (Physics.Raycast(startPos, newPoint - startPos, out RaycastHit hit, (newPoint - startPos).magnitude, targetLayer))
            {
                points.Add(hit.point);
                break;
            }

            points.Add(newPoint);
            startPos = newPoint;
        }

        // Update LineRenderer
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }

    // -----------------------------
    // ?? Calculate launch angle for turret
    // -----------------------------
    float? CalculateTrajectory(Vector3 target, bool useLow)
    {
        Vector3 targetDir = target - barrelEnd.position;

        float y = targetDir.y;
        targetDir.y = 0;
        float x = targetDir.magnitude;

        float v = projectileSpeed;
        float v2 = Mathf.Pow(v, 2);
        float v4 = Mathf.Pow(v, 4);
        float g = gravity.y;
        float x2 = Mathf.Pow(x, 2);

        float underRoot = v4 - g * ((g * x2) + (2 * y * v2));

        if (underRoot >= 0)
        {
            float root = Mathf.Sqrt(underRoot);
            float highAngle = v2 + root;
            float lowAngle = v2 - root;

            if (useLow)
                return Mathf.Atan2(lowAngle, g * x) * Mathf.Rad2Deg;
            else
                return Mathf.Atan2(highAngle, g * x) * Mathf.Rad2Deg;
        }
        else
            return null;
    }
}
