using System.Collections.Generic;
using UnityEngine;

namespace GearDefenders
{
    public static class GearConnectionGraph
    {
        static readonly Vector2Int[] Ortho =
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        public static void Recompute(GearTileState[,] tiles, Vector2Int corePos)
        {
            int w = tiles.GetLength(0);
            int h = tiles.GetLength(1);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var tile = tiles[x, y];
                    tile.IsConnectedToCore = false;
                    tile.CurrentProductionRate = 0f;
                }
            }

            if (!InBounds(corePos, w, h)) return;

            var queue = new Queue<Vector2Int>();
            var seen = new bool[w, h];
            queue.Enqueue(corePos);
            seen[corePos.x, corePos.y] = true;
            tiles[corePos.x, corePos.y].IsConnectedToCore = true;

            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                for (int i = 0; i < Ortho.Length; i++)
                {
                    var n = p + Ortho[i];
                    if (!InBounds(n, w, h) || seen[n.x, n.y]) continue;
                    var neighbor = tiles[n.x, n.y];
                    if (neighbor.IsEmpty) continue;
                    seen[n.x, n.y] = true;
                    neighbor.IsConnectedToCore = true;
                    queue.Enqueue(n);
                }
            }

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var tile = tiles[x, y];
                    if (tile.TileType != GearTileType.Unit || tile.Unit == null || !tile.IsConnectedToCore)
                        continue;

                    float bonus = 0f;
                    for (int i = 0; i < Ortho.Length; i++)
                    {
                        var n = new Vector2Int(x, y) + Ortho[i];
                        if (!InBounds(n, w, h)) continue;
                        var neighbor = tiles[n.x, n.y];
                        if (neighbor.TileType == GearTileType.Boost && neighbor.IsConnectedToCore && neighbor.Boost != null)
                            bonus += neighbor.Boost.productionBonus;
                    }

                    tile.CurrentProductionRate = tile.Unit.baseProductionRate * (1f + bonus);
                }
            }
        }

        static bool InBounds(Vector2Int p, int w, int h) => p.x >= 0 && p.y >= 0 && p.x < w && p.y < h;
    }
}
