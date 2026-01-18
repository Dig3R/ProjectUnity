using UnityEngine;

public class CameraFollowArrowsSnap : MonoBehaviour
{
    public Transform cible;

    public float distance = 6f;
    public float hauteur = 2.4f;

    public float smoothPos = 0.08f;
    public float smoothRot = 12f;

    public float vitesseRotation = 140f;

    public float snapAngle = 90f;
    public float snapVitesse = 260f;

    float yaw;
    Vector3 vel;

    public float Yaw => yaw;

    public void SetYaw(float v)
    {
        yaw = Mathf.Repeat(v, 360f);
    }

    void Start()
    {
        if (cible == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) cible = p.transform;
        }

        if (cible != null) yaw = cible.eulerAngles.y;
    }

    void Update()
    {
        float v = 0f;
        if (Input.GetKey(KeyCode.UpArrow)) v += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) v -= 1f;

        float h = 0f;
        if (Input.GetKey(KeyCode.RightArrow)) h += 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) h -= 1f;

        bool diagonale = (Mathf.Abs(v) > 0.1f) && (Mathf.Abs(h) > 0.1f);

        if (diagonale)
        {
            yaw += h * vitesseRotation * Time.deltaTime;
        }
        else if (Mathf.Abs(v) < 0.1f && Mathf.Abs(h) < 0.1f)
        {
            float cibleSnap = Mathf.Round(yaw / snapAngle) * snapAngle;
            yaw = Mathf.MoveTowardsAngle(yaw, cibleSnap, snapVitesse * Time.deltaTime);
        }

        yaw = Mathf.Repeat(yaw, 360f);
    }

    void LateUpdate()
    {
        if (cible == null) return;

        Quaternion rot = Quaternion.Euler(0f, yaw, 0f);
        Vector3 posVoulue = cible.position + rot * new Vector3(0f, hauteur, -distance);

        transform.position = Vector3.SmoothDamp(transform.position, posVoulue, ref vel, smoothPos);

        Vector3 look = cible.position + Vector3.up * 1.2f;
        Quaternion rotVoulue = Quaternion.LookRotation(look - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotVoulue, smoothRot * Time.deltaTime);
    }
}
