namespace CardGame;

using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using Godot;

interface IDraggable {
  public bool TryDragTo(Dictionary<ulong, Area2D> playerCollisions);

  public void TryHold(Node2D destinationNode, double? delta = null);
  public void PingBack();
  public void SetPreviousPosition(Vector2 previousPosition);
}
