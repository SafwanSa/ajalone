using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
public class Tile : MonoBehaviour
{
    [SerializeField] SpriteRenderer _renderer;
    public GameObject highlight;
    public GameObject selected;
    private GridManager grid;
    [SerializeField] Material black;
    [SerializeField] Material white;
    [SerializeField] Material gray;
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
        else if (y >= 8 || (y == 7 && x > 4 && x < 8))
            value = 2;
        else
            value = 0;
        this.value = value;
        this.UpdateColor();
    }

    public void UpdateColor()
    {

        if (this.value == 0)
            this.highlight.SetActive(false);
        // this._renderer.color = Color.clear;
        else if (this.value == 1)
        {
            this.highlight.SetActive(true);
            this.highlight.GetComponent<MeshRenderer>().material = this.white;
        }
        // this._renderer.color = Color.white;
        else
        {
            this.highlight.SetActive(true);
            this.highlight.GetComponent<MeshRenderer>().material = this.black;
        }
        // this._renderer.color = Color.black;

    }

    // void OnMouseEnter()
    // {
    //     if (this.grid.AllowedPos(this))
    //         this.highlight.SetActive(true);
    // }
    // void OnMouseExit()
    // {
    //     if (this.grid.AllowedPos(this))
    //         this.highlight.SetActive(false);
    // }

    void OnMouseDown()
    {
        if (this.grid.IsSelected(this))
        {
            this.grid.UnSelectTile();
        }
        else
        {
            this.grid.SelectTile(this);
        }
    }
}
