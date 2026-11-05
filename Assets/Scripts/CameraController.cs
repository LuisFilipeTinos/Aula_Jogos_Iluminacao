using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 targetOffset = new Vector3(0f, 1.5f, 0f); // ponto que a câmera mira
    public float distance = 10f;
    public float sensitivity = 0.1f;

    //Limites da inclinação vertical, em graus.
    //Impede a câmera de ir pra baixo do chão ou passar por cima da cabeça do player e
    //virar de cabeça pra baixo.
    public float minPitch = 10f;
    public float maxPitch = 70f;

    PlayerControls controls;

    //Rotação horizontal (girar em volta do player, esquerda/direita), em graus
    float yaw;

    //Rotação vertical (olhar mais de cima ou mais de lado). Começa em 40°, uma vista levemente de cima.
    float pitch = 40f;

    void Awake()
    {
        controls = new PlayerControls();
    }
    void OnEnable() { 
        controls.Enable(); 
    }
    
    void OnDisable() { 
        controls.Disable(); 
    }
    void Start()
    {
        //yaw = transform.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        Vector2 look = controls.Movement.CameraLook.ReadValue<Vector2>();

        yaw += look.x * sensitivity;

        //Aqui é menos porque, na Unity, pitch positivo inclina pra baixo. Mouse pra cima (y positivo) diminui o pitch, e a câmera passa a olhar mais pra cima. Se trocasse pra +=, o controle ficaria invertido (estilo avião).
        pitch -= look.y * sensitivity;
        //Trava o pitch entre 10° e 70°. O yaw não tem limite porque dá pra girar em volta à vontade.
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);

        //Calcula o ponto em volta do qual a câmera orbita: a posição do player mais o deslocamento de 1,5 pra cima.
        Vector3 pivot = target.position + targetOffset;

        transform.rotation = rot;
        transform.position = pivot - rot * Vector3.forward * distance;
    }
}