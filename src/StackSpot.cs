using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class StackSpot : Node2D {

  public static readonly int StackItemGap = 20;
  private List<Card> _cards = [];
  public bool Empty = true;

  public override void _Ready() {
    ZIndex = 1;
  }

  public void AppendToStack(Card newTailCard) {
    var childCard = newTailCard.GetChildCard();
    if (_cards.Count > 0) {
      var lastTail = _cards.Last();
      lastTail.SetChildCard(newTailCard);
    }
    newTailCard.SetHighPriority();
    _cards.Add(newTailCard);
    newTailCard.TweenPosition(Position + new Vector2(0, (_cards.Count - 1) * StackItemGap));
    Empty = false;
    if (childCard is not null) {
      AppendToStack(childCard);
    }
  }
}
