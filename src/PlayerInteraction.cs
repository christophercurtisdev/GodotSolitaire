using System;
using Godot;

public partial class PlayerInteraction : AnimatedSprite2D {
  private void OnAnimationFinished() {
    QueueFree();
  }
}
