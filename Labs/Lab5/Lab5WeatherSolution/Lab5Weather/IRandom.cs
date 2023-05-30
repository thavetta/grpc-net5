namespace Lab5Weather;

public interface IRandomInt
{
    int Next(int max);
    int Next(int min, int max);
}