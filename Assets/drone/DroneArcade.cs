using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroneArrowsAvecYawCamera : MonoBehaviour
{
    Rigidbody rb;
    public CameraFollowArrowsSnap cam;

    [Header("Vitesse horizontale")]
    public float vitesseMax = 50f;
    public float accel = 50f;
    public float frein = 100f;

    [Header("Vitesse verticale")]
    public float vitesseYMax = 50f;
    public float accelY = 20f;
    public float freinY = 100f;

    [Header("Rotation drone (Yaw)")]
    public float vitesseYaw = 160f;

    [Header("Tilt visuel")]
    public Transform modele;
    public float tiltAvant = 20f;
    public float tiltCote = 20f;
    public float tiltVitesse = 12f;

    public KeyCode toucheReset = KeyCode.R;

    float vIn, hIn, yawIn, yIn;
    Quaternion rotModeleBase;

    Vector3 startPos;
    Quaternion startRot;

    Vector3 dirDeplacementMonde;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        rb.angularDrag = 6f;
        rb.maxAngularVelocity = 4f;

        if (cam == null && Camera.main != null) cam = Camera.main.GetComponent<CameraFollowArrowsSnap>();

        if (modele == null && transform.childCount > 0) modele = transform.GetChild(0);
        if (modele != null) rotModeleBase = modele.localRotation;

        startPos = rb.position;
        startRot = rb.rotation;
    }

    void Update()
    {
        if (Input.GetKeyDown(toucheReset))
            ResetDrone();

        vIn = 0f;
        if (Input.GetKey(KeyCode.UpArrow)) vIn += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) vIn -= 1f;

        hIn = 0f;
        if (Input.GetKey(KeyCode.RightArrow)) hIn += 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) hIn -= 1f;

        yawIn = 0f;
        if (Input.GetKey(KeyCode.E)) yawIn += 1f; // droite
        if (Input.GetKey(KeyCode.Q)) yawIn -= 1f; // gauche

        yIn = 0f;
        if (Input.GetKey(KeyCode.Space)) yIn += 1f; // monter
        if (Input.GetKey(KeyCode.D)) yIn -= 1f;     // descendre
    }

    void FixedUpdate()
    {
        GererYawCameraEtDrone();
        DeplacementDansRepereCamera();
        DeplacementVertical();
        TiltVisuel();

        Vector3 av = rb.angularVelocity;
        rb.angularVelocity = new Vector3(0f, av.y, 0f);
    }

    void GererYawCameraEtDrone()
    {
        float yaw = (cam != null) ? cam.Yaw : rb.rotation.eulerAngles.y;

        if (yawIn != 0f)
        {
            yaw = Mathf.Repeat(yaw + yawIn * vitesseYaw * Time.fixedDeltaTime, 360f);
            if (cam != null) cam.SetYaw(yaw);
        }

        Quaternion cibleRot = Quaternion.Euler(0f, yaw, 0f);
        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, cibleRot, 720f * Time.fixedDeltaTime));
    }

    void DeplacementDansRepereCamera()
    {
        float yaw = (cam != null) ? cam.Yaw : rb.rotation.eulerAngles.y;
        Quaternion yawRot = Quaternion.Euler(0f, yaw, 0f);

        Vector3 dir = yawRot * new Vector3(hIn, 0f, vIn);
        Vector3 dirPlat = new Vector3(dir.x, 0f, dir.z);

        dirDeplacementMonde = (dirPlat.sqrMagnitude > 0.0001f) ? dirPlat.normalized : Vector3.zero;

        Vector3 vel = rb.velocity;
        Vector3 vPlat = new Vector3(vel.x, 0f, vel.z);

        Vector3 vCible = (dirDeplacementMonde != Vector3.zero) ? dirDeplacementMonde * vitesseMax : Vector3.zero;
        float a = (dirDeplacementMonde != Vector3.zero) ? accel : frein;

        Vector3 vPlatNew = Vector3.MoveTowards(vPlat, vCible, a * Time.fixedDeltaTime);
        rb.velocity = new Vector3(vPlatNew.x, vel.y, vPlatNew.z);
    }

    void DeplacementVertical()
    {
        float targetVy = (yIn != 0f) ? yIn * vitesseYMax : 0f;

        Vector3 vel = rb.velocity;
        float a = (yIn != 0f) ? accelY : freinY;

        float newVy = Mathf.MoveTowards(vel.y, targetVy, a * Time.fixedDeltaTime);
        rb.velocity = new Vector3(vel.x, newVy, vel.z);
    }

    void TiltVisuel()
    {
        if (modele == null) return;

        float pitch = 0f;
        float roll = 0f;

        if (dirDeplacementMonde != Vector3.zero)
        {
            Vector3 dirLocal = transform.InverseTransformDirection(dirDeplacementMonde);
            pitch = dirLocal.z * tiltAvant;
            roll = -dirLocal.x * tiltCote;
        }

        Quaternion cible = rotModeleBase * Quaternion.Euler(pitch, 0f, roll);
        modele.localRotation = Quaternion.Slerp(modele.localRotation, cible, tiltVitesse * Time.fixedDeltaTime);
    }

    void ResetDrone()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = startPos;
        rb.rotation = startRot;

        dirDeplacementMonde = Vector3.zero;

        if (cam != null) cam.SetYaw(startRot.eulerAngles.y);
        if (modele != null) modele.localRotation = rotModeleBase;
    }
}
