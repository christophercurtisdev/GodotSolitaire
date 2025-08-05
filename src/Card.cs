using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using CardGame;
using Godot;

public partial class Card : Node2D, IDraggable {

  private bool _hovered;
  public bool DrawnFromDeck;
  private AnimatedSprite2D _cardFace = new();
  private Card? _childCard;

  // Maybe move these to global vars or something
  private static readonly string _pipMeta = "pip";
  private static readonly string _suitMeta = "suit";

  // Object type meta is here so functionality isn't coupled with specific C# classes
  private static readonly string _objectTypeMeta = "objectType";
  private static readonly int _cardBackFrame = 52;

  public override void _Ready() {
    base._Ready();
    SetMeta("objectType", "Card");
    _cardFace = (AnimatedSprite2D)GetNode("CardFace");
    HideFace();
    ZIndex = 2;
  }

  public override void _Process(double delta) {
    base._Process(delta);
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

  public void TryDragTo(Dictionary<ulong, Area2D> playerCollisions) {
    foreach (var entry in playerCollisions) {
      Node2D other = entry.Value;
      var otherParent = (Node2D)other.GetParent();
      switch ((string)otherParent.GetMeta(_objectTypeMeta)) {
        case "Player":
          OnPlayerCollisionEnter();
          break;
        case "Card":
          OnCardCollisionEnter();
          break;
        case "StackSpot":
          OnStackSpotCollisionEnter((StackSpot)otherParent);
          break;
        default:
          break;
      }
    }
  }

  public Card? GetChildCard() => _childCard;

  public void SetDraggable(bool draggable) {
    GetNode("CardCollider").SetMeta(Player.DraggableMeta, draggable);
  }

  public bool GetDraggable() {
    return (bool)GetNode("CardCollider").GetMeta(Player.DraggableMeta);
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

  private void ShowFace() {
    var suitNumber = (Suit)Enum.Parse(typeof(Suit), (string)GetMeta(_suitMeta));
    _cardFace.Frame = (int)GetMeta(_pipMeta) + ((int)suitNumber * 13) - 1;
  }

  private void HideFace() {
    _cardFace.Frame = 52;
  }

  private void OnPlayerCollisionEnter() {
    _hovered = true;
  }

  private void OnStackSpotCollisionEnter(StackSpot otherParent) {
    // Check stack spot can accept the given card
    if (otherParent.Empty) {
      TweenPosition(otherParent.Position);
      otherParent.Empty = false;
    }
  }

  private void OnOtherCollisionEnter(Area2D other) { }

  private void OnOtherCollisionExit(Area2D other) { }

  private void OnPlayerCollisionExit() => _hovered = false;

  private void OnCardCollisionExit() { }

  private void OnStackSpotCollisionExit(Node2D otherParent) { }

  private void OnCardCollisionEnter() { }
}
