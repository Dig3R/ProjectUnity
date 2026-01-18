using UnityEngine;
using CesiumForUnity;
using Unity.Mathematics;
using System.Collections;

public class StartInParis : MonoBehaviour
{
    [Header("Références")]
    public CesiumGeoreference georeference;
    public GameObject droneParent;

    [Tooltip("Déplace ce Transform au lancement (XR Origin / XR Rig de préférence, sinon Main Camera).")]
    public Transform playerRoot;

    [Header("Positions (lon, lat, height)")]
    public double3 parisOrigin = new double3(2.3522, 48.8566, 0.0);
    public double3 droneLLH = new double3(2.2945, 48.8584, 120.0);
    public double cameraHeight = 250.0;

    void Start()
    {
        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        if (georeference == null)
            georeference = FindFirstObjectByType<CesiumGeoreference>();

        if (georeference == null)
        {
            Debug.LogError("StartInParis: aucun CesiumGeoreference trouvé.");
            yield break;
        }

        // 1) Mettre l'origine du monde sur Paris
        georeference.SetOriginLongitudeLatitudeHeight(parisOrigin.x, parisOrigin.y, parisOrigin.z);

        // Laisse 1 frame à Unity/Cesium pour s'initialiser proprement
        yield return null;

        // 2) Placer la caméra / XR Origin au-dessus de Paris
        if (playerRoot == null && Camera.main != null)
            playerRoot = Camera.main.transform;

        if (playerRoot != null)
        {
            // Convertit lon/lat/height -> ECEF -> Unity
            double3 ecef = georeference.ellipsoid.LongitudeLatitudeHeightToCenteredFixed(
                new double3(parisOrigin.x, parisOrigin.y, parisOrigin.z + cameraHeight)
            );
            double3 unityPos = georeference.TransformEarthCenteredEarthFixedPositionToUnity(ecef);

            playerRoot.position = new Vector3((float)unityPos.x, (float)unityPos.y, (float)unityPos.z);

            playerRoot.rotation = Quaternion.Euler(20f, 0f, 0f);
        }

        // 3) Placer le drone
        if (droneParent == null) yield break;

        // IMPORTANT : le GlobeAnchor résout son Georeference via le parent.
        // Donc on force DroneParent à être enfant de CesiumGeoreference si ce n'est pas déjà le cas.
        if (!droneParent.transform.IsChildOf(georeference.transform))
            droneParent.transform.SetParent(georeference.transform, true);

        var anchor = droneParent.GetComponent<CesiumGlobeAnchor>();
        if (anchor == null) anchor = droneParent.AddComponent<CesiumGlobeAnchor>();

        // Place le drone (méthode recommandée : longitudeLatitudeHeight) :contentReference[oaicite:1]{index=1}
        anchor.longitudeLatitudeHeight = droneLLH;

        // Optionnel : force une synchro immédiate
        anchor.Sync();
    }
}
