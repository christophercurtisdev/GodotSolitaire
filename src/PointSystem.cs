namespace CardGame;

public abstract class PointSystem {
  public static decimal Mult { get; set; } = 10.999M;
  public static int Base { get; set; } = 100;
  public static void AddToBase(int value) => Base += value;
  public static void AddToMult(decimal value) => Mult += value;
  public static int CalculateScore() => (int)(Mult * Base);
}
