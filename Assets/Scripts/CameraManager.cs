using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [Header("Cinemachine Ayarları")]
    // Cinemachine Virtual Camera referansı (Inspector üzerinden atayın)
    public CinemachineVirtualCamera virtualCamera;

    // Ana karakterin transform referansı (takip edilecek hedef)
    public Transform mainCharacter;

    // Kameranın sola dönüş hızı (derece/saniye)
    public float rotationSpeed = 30f;

    [Header("Trigger Ayarı")]
    // Bu trigger aktif olduğunda kamera sola dönecek
    public bool rotateLeftTrigger = false;

    // Kameranın Orbital Transposer bileşeni (Body kısmında bulunur)
    private CinemachineOrbitalTransposer orbitalTransposer;

    // Trigger false olduğunda geri dönecek varsayılan X-axis açısı
    private float defaultXAxis;

    void Start()
    {
        // Virtual Camera ve ana karakter referansları kontrol ediliyor
        if (virtualCamera == null)
            return;


        if (mainCharacter != null)
        {
            // Ana karakteri takip etmesi için Follow hedefi atanıyor
            virtualCamera.Follow = mainCharacter;
        }
        else
        {
            Debug.LogError("Main Character referansı atanmamış!");
        }

        // Virtual Camera'nın Body bölümündeki Orbital Transposer bileşenini alıyoruz
        orbitalTransposer = virtualCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
        if (orbitalTransposer == null)
        {
            Debug.LogError("Virtual Camera'da CinemachineOrbitalTransposer bileşeni bulunamadı!");
        }
        else
        {
            // Varsayılan X-axis değerini saklıyoruz (trigger false olduğunda bu açıya geri dönülecek)
            defaultXAxis = orbitalTransposer.m_XAxis.Value;
        }
    }

    void Update()
    {
        // Eğer Orbital Transposer bulunamadıysa, Update çalışmasın.
        if (orbitalTransposer == null)
            return;

        if (rotateLeftTrigger)
        {
            // Trigger aktifse, kameranın X-axis açısını azaltarak sola dönmesini sağlıyoruz.
            // (CinemachineOrbitalTransposer.m_XAxis.Value, hedef etrafındaki yatay açıyı temsil eder.)
            orbitalTransposer.m_XAxis.Value -= rotationSpeed * Time.deltaTime;
        }
        else
        {
            // Trigger false ise, kameranın X-axis açısını varsayılan değere doğru yumuşakça geri getiriyoruz.
            orbitalTransposer.m_XAxis.Value = Mathf.Lerp(orbitalTransposer.m_XAxis.Value, defaultXAxis, Time.deltaTime * rotationSpeed);
        }
    }
}
