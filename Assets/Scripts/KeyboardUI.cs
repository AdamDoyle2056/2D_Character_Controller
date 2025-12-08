using UnityEngine;

public class KeyboardSpriteRenderer : MonoBehaviour
{
    [Header("Key Sprites")]
    public Sprite aUp;
    public Sprite aDown;
    public Sprite dUp;
    public Sprite dDown;
    public Sprite spaceUp;
    public Sprite spaceDown;

    [Header("Key Layout (relative to parent)")]
    public Vector3 aPosition = new Vector3(-2, 0, 0);
    public Vector3 dPosition = new Vector3(0, 0, 0);
    public Vector3 spacePosition = new Vector3(2, 0, 0);

    private SpriteRenderer aRenderer;
    private SpriteRenderer dRenderer;
    private SpriteRenderer spaceRenderer;

    void Awake()
    {
        // Create keys dynamically as children
        aRenderer = CreateKey("Key_A", aPosition, aUp);
        dRenderer = CreateKey("Key_D", dPosition, dUp);
        spaceRenderer = CreateKey("Key_Space", spacePosition, spaceUp);
    }

    void Update()
    {
        // Swap sprites based on input
        aRenderer.sprite = Input.GetKey(KeyCode.A) ? aDown : aUp;
        dRenderer.sprite = Input.GetKey(KeyCode.D) ? dDown : dUp;
        spaceRenderer.sprite = Input.GetKey(KeyCode.Space) ? spaceDown : spaceUp;
    }

    // Helper method to create a key GameObject with a SpriteRenderer
    private SpriteRenderer CreateKey(string name, Vector3 localPosition, Sprite initialSprite)
    {
        GameObject keyObj = new GameObject(name);
        keyObj.transform.parent = transform;
        keyObj.transform.localPosition = localPosition;

        SpriteRenderer sr = keyObj.AddComponent<SpriteRenderer>();
        sr.sprite = initialSprite;
        sr.sortingOrder = 10; // optional, makes sure keys render above background
        return sr;
    }
}
