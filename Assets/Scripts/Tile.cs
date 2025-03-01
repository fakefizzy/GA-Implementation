using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileType { Path, Wall, BorderWall, Start, End }
    public TileType type;
    private SpriteRenderer sr;
    public Sprite startSprite;
    public Sprite endSprite;
    public Sprite defaultSprite;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        UpdateColor();
    }

    public void ChangeTileType(TileType newType)
    {
        type = newType;
        UpdateColor();
    }

    public void SetAsBorderWall()
    {
        type = TileType.BorderWall;
        UpdateColor();
    }

    void UpdateColor()
    {
        if (sr == null) return;

        switch (type)
        {
            case TileType.Path:
                sr.sprite = defaultSprite;
                sr.color = Color.green;
                break;
            case TileType.Wall:
                sr.sprite = defaultSprite;
                sr.color = Color.black;
                break;
            case TileType.BorderWall:
                sr.sprite = defaultSprite;
                sr.color = Color.black;
                break;
            case TileType.Start:
                sr.color = Color.white;
                sr.sprite = startSprite;
                break;
            case TileType.End:
                sr.color = Color.white;
                sr.sprite = endSprite;
                break;
        }
    }
}
