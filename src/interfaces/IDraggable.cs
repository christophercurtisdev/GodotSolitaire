namespace CardGame;

using System.Collections.Generic;
using Godot;

interface IDraggable {
  public void TryDragTo(Dictionary<ulong, Area2D> playerCollisions);

}
