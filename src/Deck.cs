namespace CardGame;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Deck : Node2D {

  private static PackedScene _cardClass = GD.Load<PackedScene>("res://src/Card.tscn");
  private List<Card> _cards = [];
  private bool _hovered = false;
  public override void _Ready() {
    GenerateStandardDeck();
  }

  public override void _Process(double delta) {
    if (Input.IsActionJustPressed("LClick") && _hovered) {
      DrawThreeCards();
    }
  }

  public List<Card> GetCards() => _cards;

  private void OnMouseCollisionEnter() {
    _hovered = true;
  }

  private void OnMouseCollisionExit() {
    _hovered = false;
  }

  private void GenerateStandardDeck() {
    for (var suit = 0; suit < 4; suit++) {
      for (var pip = 1; pip < 14; pip++) {
        var card = (Card)_cardClass.Instantiate();
        card.SetSuit(suit);
        card.SetPip(pip);
        card.Position = Position;
        card.SetDraggable(false);
        _cards.Add(card);
      }
    }
    var shuffled = _cards.OrderBy(_ => Game.Seeder.Next()).ToList();
    _cards = shuffled;
  }

  private List<Card> DrawThreeCards() {
    List<Card> drawnCards = [];
    for (var i = 0; i < 3; i++) {
      Card card = DrawNextCard(true);
      drawnCards.Add(card);
      Vector2 destination = card.Position + new Vector2((i * 20) + 100, 0);
      card.TweenPosition(destination);
    }
    return drawnCards;
  }

  public Card DrawNextCard(bool flip = false) {
    foreach (Card card in _cards) {
      if (!card.DrawnFromDeck) {
        card.DrawnFromDeck = true;
        GD.Print(card.debug());
        if (flip) {
          card.FlipCard();
        }
        GD.Print(card.debug());
        return card;
      }
    }
    return new Card();
  }
}
