using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates a tiny playable placeholder scene when no authored scene exists.
/// Replace placeholder visuals with original pixel-art sprites as production progresses.
/// </summary>
public class PIXELFALLBootstrap : MonoBehaviour
{
    public static PIXELFALLBootstrap Instance;
    private Transform player;
    private float velocityY;
    private bool grounded;
    private int hp = 100;
    private Text hud;
    private float enemyTimer;
    private int kills;

    void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        Camera.main.backgroundColor = new Color(0.055f, 0.045f, 0.10f);
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 5.2f;
        BuildWorld();
        BuildHUD();
    }

    void Update()
    {
        if (player == null) return;
        float move = Input.GetAxisRaw("Horizontal");
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            move = t.position.x < Screen.width * .42f ? -1 : (t.position.x < Screen.width * .7f ? 1 : 0);
        }
        player.position += Vector3.right * move * 4.2f * Time.deltaTime;
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)) && grounded)
        {
            velocityY = 8.2f;
            grounded = false;
        }
        velocityY -= 20f * Time.deltaTime;
        player.position += Vector3.up * velocityY * Time.deltaTime;
        if (player.position.y <= -2.0f) { player.position = new Vector3(player.position.x, -2f, 0); velocityY = 0; grounded = true; }
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, new Vector3(player.position.x, 1.2f, -10), 4f * Time.deltaTime);
        enemyTimer += Time.deltaTime;
        if (enemyTimer > 2.4f) { enemyTimer = 0; SpawnEnemy(); }
        if (hud != null) hud.text = "PIXELFALL  |  HP " + hp + "  |  SOULS " + kills + "\nMOVE: LEFT / RIGHT     JUMP: SPACE";
    }

    void BuildWorld()
    {
        MakeBlock("Moonlit floor", new Vector3(0,-2.45f,0), new Vector3(40,.8f,1), new Color(.12f,.13f,.24f));
        MakeBlock("Floating platform A", new Vector3(4,-.15f,0), new Vector3(3,.35f,1), new Color(.20f,.18f,.33f));
        MakeBlock("Floating platform B", new Vector3(-4,1.25f,0), new Vector3(2.6f,.35f,1), new Color(.20f,.18f,.33f));
        for (int i=0;i<18;i++)
        {
            float x = -15 + i*1.8f;
            MakeBlock("Distant ruin", new Vector3(x, 2.7f + Mathf.Sin(i*1.7f)*.6f, 5), new Vector3(.8f, 4.2f, .5f), new Color(.10f,.09f,.17f));
        }
        player = MakeBlock("Hero placeholder", new Vector3(0,-1.1f,0), new Vector3(.62f,.9f,.6f), new Color(.20f,.85f,.95f)).transform;
        MakeBlock("Glow crystal", new Vector3(3,-1.6f,0), new Vector3(.3f,.45f,.4f), new Color(.7f,.25f,1f));
    }

    GameObject MakeBlock(string label, Vector3 pos, Vector3 scale, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = label; go.transform.position = pos; go.transform.localScale = scale;
        Renderer r = go.GetComponent<Renderer>();
        r.material = new Material(Shader.Find("Sprites/Default"));
        r.material.color = color;
        if (label == "Hero placeholder") Destroy(go.GetComponent<Collider>());
        return go;
    }

    void SpawnEnemy()
    {
        Vector3 pos = player.position + new Vector3(Random.value > .5f ? 7 : -7, .15f, 0);
        GameObject e = MakeBlock("Cursed slime", pos, new Vector3(.7f,.65f,.6f), new Color(.75f,.18f,.35f));
        e.AddComponent<PIXELFALLSlime>();
    }

    void BuildHUD()
    {
        GameObject canvasGO = new GameObject("HUD");
        Canvas canvas = canvasGO.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();
        GameObject txt = new GameObject("Status"); txt.transform.SetParent(canvasGO.transform,false);
        hud = txt.AddComponent<Text>(); hud.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        hud.fontSize = 22; hud.color = Color.white; hud.alignment = TextAnchor.UpperLeft;
        RectTransform rt = hud.rectTransform; rt.anchorMin = new Vector2(0,1); rt.anchorMax = new Vector2(1,1); rt.pivot = new Vector2(0,1); rt.anchoredPosition = new Vector2(20,-18); rt.sizeDelta = new Vector2(-40,90);
    }

    public void EnemyKilled() { kills++; }
}

public class PIXELFALLSlime : MonoBehaviour
{
    private float age;
    private Transform target;
    void Start() { target = GameObject.Find("Hero placeholder")?.transform; }
    void Update()
    {
        age += Time.deltaTime;
        if (target == null) return;
        Vector3 p = transform.position;
        p.x = Mathf.MoveTowards(p.x, target.position.x, 1.1f * Time.deltaTime);
        p.y = -1.9f + Mathf.Abs(Mathf.Sin(age*4f))*.25f;
        transform.position = p;
        if (Mathf.Abs(target.position.x-p.x)<.65f && Mathf.Abs(target.position.y-p.y)<.9f)
        {
            // Placeholder enemy contact; full combat and touch controls are next milestone.
        }
    }
}
