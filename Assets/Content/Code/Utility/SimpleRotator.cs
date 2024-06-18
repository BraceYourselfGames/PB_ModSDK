using UnityEngine;

public class SimpleRotator : MonoBehaviour
{
    public float minRotation = -179;
    public float maxRotation = 179;

    public float currentRotation;
    public float targetRotation;
    public float rotationSpeed;

    public Vector3 rotationAxis;

    private Quaternion originalRotation;
    private Quaternion offsetRotation;

    //Zero to one rotation for an object, based on the min and max rotations
    public void Rotate(float rotation)
    {
        Rotate(rotation, minRotation, maxRotation);
    }

    public void Rotate(float rotation, float min, float max)
    {
        targetRotation = Mathf.Lerp(minRotation, maxRotation, rotation);
        offsetRotation = Quaternion.Euler(targetRotation * rotationAxis);
    }

    public void Awake()
    {
        originalRotation = this.transform.rotation;
    }

    public void Update()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, originalRotation * offsetRotation, rotationSpeed);
    }
}
