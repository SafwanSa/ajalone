using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject highlight;
    [SerializeField] private GameObject selected;
    private GridManager grid;
    public int x, y;
    public int value = 0;

    public void Init(int x, int y, GridManager grid)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;

        int value = 0;
        if (y < 3 || (y == 3 && x > 2 && x < 6))
            value = 1;
        else if (y >= 8 || (y == 7 && x > 2 && x < 6))
            value = 2;
        else
            value = 0;
        this.value = value;
        this.UpdateColor();
    }

    public void UpdateColor()
    {
        if (this.value == 0)
            this._renderer.color = Color.grey;
        else if (this.value == 1)
            this._renderer.color = Color.white;
        else
            this._renderer.color = Color.black;
    }

    void OnMouseEnter()
    {
        if (this.grid.AllowedPos(this))
            this.highlight.SetActive(true);
    }
    void OnMouseExit()
    {
        if (this.grid.AllowedPos(this))
            this.highlight.SetActive(false);
    }

    void OnMouseDown()
    {
        if (this.grid.IsSelected(this))
        {
            this.selected.SetActive(!this.grid.UnSelectTile(this));
        }
        else
        {
            this.selected.SetActive(this.grid.SelectTile(this));
        }
    }
}
