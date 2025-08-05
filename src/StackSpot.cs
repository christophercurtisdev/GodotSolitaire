using System;
using System.Collections.Generic;
using Godot;

public partial class StackSpot : Node2D {
  private List<Card> _cards = [];
  public bool Empty = true;

  public override void _Ready() {
    ZIndex = 1;
  }

  public void AppendToStack(Card newCardHead) {
    var childCard = newCardHead.GetChildCard();
    if (childCard is not null) {
      AppendToStack(childCard);
    }
    else {
      _cards.Add(newCardHead);
      newCardHead.TweenPosition(Position + new Vector2(0, (_cards.Count - 1) * 20));
      Empty = false;
    }
  }
}
