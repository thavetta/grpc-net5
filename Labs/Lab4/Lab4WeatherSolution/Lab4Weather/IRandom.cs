namespace Lab4Weather;

public interface IRandomInt
{
    int Next(int max);
    int Next(int min, int max);
}