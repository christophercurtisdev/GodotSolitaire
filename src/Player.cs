using System;
using System.Collections.Generic;
using System.Linq;
using CardGame;
using Godot;

public partial class Player : Node2D {

  private bool _clicking;
  private Node2D? _heldItem;
  private int _previouslyHeldItemZIndex = 1;
  private PackedScene _playerInteractionClass = GD.Load<PackedScene>("res://src/PlayerInteraction.tscn");


  private static readonly Dictionary<ulong, Area2D> _draggableCollisions = [];
  private static readonly Dictionary<ulong, Area2D> _undraggableCollisions = [];
  private static readonly Dictionary<ulong, Area2D> _totalCollisions = [];
  private static readonly float _hoverScaleMultiplier = 1.2f;
  public static readonly string DraggableMeta = "draggable";

  public override void _Process(double delta) {
    HandleInput();
    ScaleDraggables();
    if (_clicking && _heldItem != null) {
      var heldItem = _heldItem as IDraggable;
      heldItem?.Hold(this, delta);
    }
  }

  private void ScaleDraggables() {
    foreach (var draggable in _draggableCollisions) {
      Node2D parent = (Node2D)draggable.Value.GetParent();
      Node2D draggableCollider = (Node2D)parent.GetChild((int)draggable.Value.GetMeta(DraggableMeta, 0));
      var draggableParent = parent as IDraggable;
      if (draggableCollider.GetInstanceId() == draggable.Value.GetInstanceId() && (draggableParent?.CanHold(this) ?? false)) {
        float mappedScaleMultiplier = MapRange(parent.Position.DistanceTo(Position), _hoverScaleMultiplier, 1, 0, 60);
        parent.Scale = new Vector2(mappedScaleMultiplier, mappedScaleMultiplier);
      }
    }
  }

  private float MapRange(float value, float nMin, float nMax, float oMin, float oMax) {
    float nUpperBound = nMax - nMin;
    float oUpperBound = oMax - oMin;
    float nValue = ((nUpperBound / oUpperBound) * value) + nMin;
    return nValue >= 1 ? nValue : 1f;
  }

  private void OnOtherCollisionEnter(Area2D other) {
    if ((int)other.GetMeta(DraggableMeta, 0) > 0) {
      _draggableCollisions.Add(other.GetInstanceId(), other);
    }
    else {
      _undraggableCollisions.Add(other.GetInstanceId(), other);
    }
    _totalCollisions.Add(other.GetInstanceId(), other);
  }

  private void OnOtherCollisionExit(Area2D other) {
    if ((int)other.GetMeta(DraggableMeta, 0) > 0) {
      _draggableCollisions.Remove(other.GetInstanceId());
      Node2D parent = (Node2D)other.GetParent();
      Node2D draggableCollider = (Node2D)parent.GetChild((int)other.GetMeta(DraggableMeta, 0));
      if (draggableCollider.GetInstanceId() == other.GetInstanceId()) {
        var tween = GetTree().CreateTween();
        tween.TweenProperty(other.GetParent(), "scale", new Vector2(1, 1), 0.2f);
      }
    }
    else {
      _undraggableCollisions.Remove(other.GetInstanceId());
    }
    _totalCollisions.Remove(other.GetInstanceId());
  }

  private void HandleInput() {
    Position = GetViewport().GetMousePosition();
    if (Input.IsActionJustPressed("LClick")) {
      _clicking = true;
      List<Area2D> collisionPriorityOrderedList = _draggableCollisions.Values.OrderBy(value => (int)value.GetMeta("collisionPriority", 0)).ToList();
      Node2D? newItem = collisionPriorityOrderedList.Count > 0 ? (Node2D)collisionPriorityOrderedList.Last().GetParent() : null;
      if (newItem is not null) {
        var heldItem = newItem as IDraggable;
        if (heldItem?.CanHold(this) ?? false) {
          _heldItem = newItem;
          _previouslyHeldItemZIndex = _heldItem.ZIndex;
          _heldItem.ZIndex = 100;
          heldItem?.SetPreviousPosition(_heldItem.Position);
        }
      }
    }
    else if (Input.IsActionJustReleased("LClick")) {
      if (_heldItem is not null) {
        var playerInteraction = (AnimatedSprite2D)_playerInteractionClass.Instantiate();
        playerInteraction.Position = Position;
        Game.GetTable().AddChild(playerInteraction);
        playerInteraction.Play();
        _heldItem.ZIndex = _previouslyHeldItemZIndex;
        var heldDraggableItem = _heldItem as IDraggable;
        _totalCollisions.Remove(_heldItem.GetInstanceId());
        var successfullyMoved = heldDraggableItem?.TryDragTo(_totalCollisions) ?? true;
        if (!successfullyMoved) {
          var heldItem = _heldItem as IDraggable;
          heldItem?.PingBack();
        }
      }
      _clicking = false;
      _heldItem = null;
    }
  }
}
