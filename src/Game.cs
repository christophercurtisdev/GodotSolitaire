namespace CardGame;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Game : Control {


  private static Node _table;
  private static Deck _deck;

  private int _stackSpotsCount = 7;
  private List<StackSpot> _stackSpots = [];
  public static readonly Random Seeder = new();
  private static PackedScene _stackSpotClass = GD.Load<PackedScene>("res://src/StackSpot.tscn");

  public override void _Ready() {
    _table = GetNode("%Table");
    _deck = (Deck)GetNode("%Deck");
  }

  public static Node GetTable() => _table;

  private void PutCardsOnTable() {
    foreach (var card in _deck.GetCards().Select((value, index) => new { value, index })) {
      _table.AddChild(card.value);
    }
  }

  private void SetupGameBoard() {
    PutCardsOnTable();
    for (var i = 0; i < _stackSpotsCount; i++) {
      var spot = (StackSpot)_stackSpotClass.Instantiate();
      spot.Position = new Vector2((i * 130) + 150, 200);
      _stackSpots.Add(spot);
      _table.AddChild(spot);
    }
    foreach (var spot in _stackSpots.Select((value, index) => new { value, index })) {
      for (var i = 0; i < spot.index + 1; i++) {
        Card nextCard = _deck.DrawNextCard(i == spot.index);
        nextCard.SetDraggable(true);
        spot.value.AppendToStack(nextCard);
      }
    }
  }

  private void OnSetupGameClick() {
    SetupGameBoard();
  }
}
