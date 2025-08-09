using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using CardGame;
using Godot;

public partial class Card : Node2D, IDraggable {

  private bool _hovered;
  public bool DrawnFromDeck;
  private AnimatedSprite2D _cardFace = new();
  private Card? _childCard;
  private StackSpot? _stackSpot;
  private Vector2? _previousPosition;

  // Maybe move these to global vars or something
  private static readonly string _pipMeta = "pip";
  private static readonly string _suitMeta = "suit";

  // Object type meta is here so functionality isn't coupled with specific C# classes
  private static readonly string _objectTypeMeta = "objectType";
  private static readonly int _cardBackFrame = 52;

  private static readonly string _draggableCollider = "CardCollider";

  public override void _Ready() {
    base._Ready();
    SetMeta("objectType", "Card");
    // Set collider draggable values
    GetNode("CardCollider").SetMeta(Player.DraggableMeta, GetNode(_draggableCollider).GetIndex().ToString());
    GetNode("HighPriorityCardCollider").SetMeta(Player.DraggableMeta, GetNode(_draggableCollider).GetIndex().ToString());
    _cardFace = (AnimatedSprite2D)GetNode("CardFace");
    HideFace();
    SetHighPriorityColliderHeight(StackSpot.StackItemGap);
    ZIndex = 2;
  }

  public string GetCardName() {
    var pipName = "";
    pipName = (int)GetMeta(_pipMeta) switch {
      (int)Pip.Ace => "Ace",
      (int)Pip.Jack => "Jack",
      (int)Pip.Queen => "Queen",
      (int)Pip.King => "King",
      _ => (string)GetMeta(_pipMeta),
    };
    return pipName + " of " + GetMeta(_suitMeta);
  }

  public void SetSuit(int suit) {
    switch (suit) {
      case (int)Suit.Spades:
        SetMeta(_suitMeta, "Spades");
        break;
      case (int)Suit.Clubs:
        SetMeta(_suitMeta, "Clubs");
        break;
      case (int)Suit.Hearts:
        SetMeta(_suitMeta, "Hearts");
        break;
      case (int)Suit.Diamonds:
        SetMeta(_suitMeta, "Diamonds");
        break;
      default:
        SetMeta(_suitMeta, "");
        break;
    }
  }

  public void SetPip(int pip) => SetMeta(_pipMeta, pip);

  public int GetPip() => (int)GetMeta(_pipMeta);
  public string GetSuit() => (string)GetMeta(_suitMeta);

  public bool TryDragTo(Dictionary<ulong, Area2D> playerCollisions) {
    bool success = false;
    foreach (var entry in playerCollisions) {
      Node2D other = entry.Value;
      var otherParent = (Node2D)other.GetParent();
      switch ((string)otherParent.GetMeta(_objectTypeMeta)) {
        case "StackSpot":
          return OnStackSpotCollisionEnter((StackSpot)otherParent);
        case "Card":
          success = OnCardCollisionEnter((Card)otherParent);
          if (success) {
            return success;
          }
          break;
        default:
          break;
      }
    }
    return success;
  }

  public void TryHold(Node2D destinationNode, double? delta = null) {
    var heldX = (int)((Position.X - destinationNode.Position.X) * (delta ?? 0.1) * 10);
    var heldY = (int)((Position.Y - destinationNode.Position.Y - 50) * (delta ?? 0.1) * 10);
    Position -= new Vector2(heldX, heldY);
    _childCard?.TryHold(this, delta);
  }

  public void PingBack() {
    TweenPosition(_previousPosition ?? new Vector2(0, 0));
    _previousPosition = null;
    _childCard?.PingBack();
  }

  public void SetPreviousPosition(Vector2 previousPosition) {
    _previousPosition = previousPosition;
    _childCard?.SetPreviousPosition(_childCard.Position);
  }

  public List<Card> GetChildrenCards(bool includeSelf = false) {
    List<Card> children = [];
    children = children.Concat(_childCard?.GetChildrenCards(true) ?? []).ToList();
    if (includeSelf) {
      children.Add(this);
    }
    return children;
  }

  private List<ulong> GetColliderIDs() {
    List<ulong> ids = [];
    ids.Add(GetNode("CardCollider").GetInstanceId());
    ids.Add(GetNode("HighPriorityCardCollider").GetInstanceId());
    return ids;
  }

  public Card? GetChildCard() => _childCard;

  public void SetChildCard(Card? childCard = null) {
    _childCard = childCard;
    if (childCard is not null) {
      SetLowPriority();
    }
  }

  public void SetDraggable(string draggable) {
    GetNode("CardCollider").SetMeta(Player.DraggableMeta, draggable);
  }

  public string GetDraggable() {
    return (string)GetNode("CardCollider").GetMeta(Player.DraggableMeta);
  }

  public int debug() {
    return _cardFace.Frame;
  }

  public void FlipCard() {
    if (_cardFace.Frame == _cardBackFrame) {
      ShowFace();
    }
    else {
      HideFace();
    }
  }

  public void TweenPosition(Vector2 newPosition) {
    var tween = GetTree().CreateTween();
    tween.TweenProperty(this, "position", newPosition, 0.2f);
  }

  public void SetHighPriority() {
    Area2D cardCollider = (Area2D)GetNode("CardCollider");
    cardCollider.SetMeta("collisionPriority", 1);
  }

  public void SetLowPriority() {
    Area2D cardCollider = (Area2D)GetNode("CardCollider");
    cardCollider.SetMeta("collisionPriority", 0);
  }

  private void ShowFace() {
    var suitNumber = (Suit)Enum.Parse(typeof(Suit), (string)GetMeta(_suitMeta));
    _cardFace.Frame = (int)GetMeta(_pipMeta) + ((int)suitNumber * 13) - 1;
  }

  private void HideFace() {
    _cardFace.Frame = 52;
  }

  private bool OnStackSpotCollisionEnter(StackSpot otherParent) {
    // Check stack spot can accept the given card
    if (otherParent.Empty && otherParent != _stackSpot) {
      otherParent.AppendToStack(this);
      return true;
    }
    return false;
  }

  public void SetStackSpot(StackSpot newStackSpot) {
    _stackSpot?.RemoveFromStack(this);
    _stackSpot = newStackSpot;
  }

  public StackSpot? GetStackSpot() {
    return _stackSpot;
  }

  public void SetHighPriorityColliderHeight(int height) {
    CollisionShape2D hpColliderShape = (CollisionShape2D)GetNode("HighPriorityCardCollider/HighPriorityCardColliderShape");
    RectangleShape2D hpColliderShapeRectangle = (RectangleShape2D)hpColliderShape.Shape;
    hpColliderShape.Position = new Vector2(0, (height / 2) - 62);
    hpColliderShapeRectangle.Size = new Vector2(hpColliderShapeRectangle.Size.X, height);
  }

  public bool OnCardCollisionEnter(Card otherParent) {
    StackSpot? newStackSpot = otherParent.GetStackSpot();
    if (newStackSpot is not null && newStackSpot != _stackSpot) {
      return OnStackSpotCollisionEnter(newStackSpot);
    }
    return false;
  }

  private void OnOtherCollisionEnter(Area2D other) { }

  private void OnOtherCollisionExit(Area2D other) { }

  private bool OnPlayerCollisionEnter() {
    return false;
  }

  private void OnPlayerCollisionExit() { }

  private void OnCardCollisionExit() { }

  private void OnStackSpotCollisionExit(Node2D otherParent) { }
}
