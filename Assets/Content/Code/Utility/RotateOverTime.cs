using UnityEngine;

public class RotateOverTime : MonoBehaviour
{
    public float rotationSpeed;

    public Vector3 rotationAxis;

    public bool useDeltaTime = true;

    private Quaternion offsetRotation;

    void Awake()
    {
        // originalRotation = this.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        offsetRotation = Quaternion.Euler(rotationAxis * 45);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, transform.rotation * offsetRotation, useDeltaTime ? (rotationSpeed * Time.deltaTime) : rotationSpeed);
    }
}
