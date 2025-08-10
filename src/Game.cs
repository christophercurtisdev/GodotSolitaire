namespace CardGame;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Game : Control {


  private static Node _table;
  private static Deck _deck;
  private static Timer _gameTimer;

  private int _stackSpotsCount = 7;
  private List<StackSpot> _stackSpots = [];
  public static readonly Random Seeder = new();
  private static PackedScene _stackSpotClass = GD.Load<PackedScene>("res://src/StackSpot.tscn");

  public override void _Ready() {
    _table = GetNode("%Table");
    _deck = (Deck)GetNode("%Deck");
    _gameTimer = (Timer)GetNode("%GameTimer");
  }

  public override void _Process(double delta) {
    var fpLabel = (Label)GetNode("FPSLabel");
    fpLabel.Text = "FPS: " + Engine.GetFramesPerSecond();
    var gameTimerLabel = (Label)GetNode("GameTimerLabel");
    gameTimerLabel.Text = "Time Left: " + (int)_gameTimer.TimeLeft;
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
        spot.value.AppendToStack(nextCard);
      }
    }
  }

  private void StartGameTimer() {
    _gameTimer.Start();
  }

  private void OnSetupGameClick() {
    SetupGameBoard();
    StartGameTimer();
  }

  private void OnGameTimerTimeout() {
    EndGame();
  }

  private void EndGame() {
    foreach (var card in _deck.GetCards()) {
      card.SetDraggable(false);
      card.PingBack();
    }
  }
}
