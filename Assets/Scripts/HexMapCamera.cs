using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMapCamera : MonoBehaviour
{
    public float stickMinZoom = -250, stickMaxZoom = -45;
    public float swivelMinZoom = 90, swivelMaxZoom = 45;
    public float moveSpeedMinZoom = 400, moveSpeedMaxZoom = 100;
    public float rotationSpeed = 180, rotationalAngle;
    public HexGrid grid;
    private float _zoom = 1f;
    private Transform _swivel, _stick;

    private void Awake()
    {
        _swivel = transform.GetChild(0);
        _stick = _swivel.GetChild(0);
    }

    private void Update()
    {
        float zoomDela = Input.GetAxis("Mouse ScrollWheel");

        if (zoomDela != 0f)
        {
            AdjustZoom(zoomDela);
        }

        float rotationDelta = Input.GetAxis("Rotation");
        if (rotationDelta != 0f)
        {
            AdjustRotation(rotationDelta);
        }

        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");

        if (xDelta != 0f || zDelta != 0f)
        {
            AdjustPosition(xDelta, zDelta);
        } 
    }

    private void AdjustRotation(float delta)
    {
        rotationalAngle += delta * rotationSpeed * Time.deltaTime;

        if (rotationalAngle < 0f)
        {
            rotationalAngle += 360f;
        }
        else if (rotationalAngle >= 360f)
        {
            rotationalAngle -= 360f;
        }
        transform.localRotation = Quaternion.Euler(0f, rotationalAngle, 0f);
    }

    private void AdjustPosition(float xDelta, float zDelta)
    {
        Vector3 direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float distance = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, _zoom) * damping * Time.deltaTime;
        
        Vector3 position = transform.localPosition;
        position += direction * distance;
        /*transform.localPosition = position;*/
        transform.localPosition = ClampPosition(position);
    }

    private void AdjustZoom(float delta)
    {
        _zoom = Mathf.Clamp01(_zoom + delta);

        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, _zoom);
        _stick.localPosition = new Vector3(0f,0f,distance);

        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, _zoom);
        _swivel.localRotation = Quaternion.Euler(angle, 0f,0f);
    }

    private Vector3 ClampPosition(Vector3 position)
    {
        float xMax = (grid.chunkCountX * HexMetrics.CHUNK_SIZE_X  - .5f) * (2f * HexMetrics.INNER_RADIUS);
        position.x = Mathf.Clamp(position.x, 0f, xMax);
        
        float zMax = (grid.chunkCountZ * HexMetrics.CHUNK_SIZE_Z - 1) * (1.5f * HexMetrics.OUTER_RADIUS);
        position.z = Mathf.Clamp(position.z, 0f, zMax);
        
        return position;
    }
}
