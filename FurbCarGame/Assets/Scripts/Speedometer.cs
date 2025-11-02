using UnityEngine;
using UnityEngine.UI;

public class Speedometer : MonoBehaviour
{
    public float maxSpeed = 280f;
    public float speed = 0.0f;

    public float minSpeedArrowAngle;
    public float maxSpeedArrowAngle;

    public Rigidbody rb;

    [Header("UI")]
    public Text speedLabel;
    public RectTransform arrow;

    private void Update()
    {
        speed = rb.linearVelocity.magnitude * 3.6f;
        speedLabel.text = ((int)speed) + " km/h";
        arrow.localEulerAngles = new Vector3(0, 0, Mathf.Lerp(minSpeedArrowAngle, maxSpeedArrowAngle, speed / maxSpeed));
    }
}
