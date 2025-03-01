using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TileType { Path, Wall, BorderWall, Start, End }
    public TileType type;
    private SpriteRenderer sr;
    public Sprite startSprite;
    public Sprite endSprite;

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
                sr.color = Color.green;
                break;
            case TileType.Wall:
                sr.color = Color.black;
                break;
            case TileType.BorderWall:
                sr.color = Color.grey;
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
