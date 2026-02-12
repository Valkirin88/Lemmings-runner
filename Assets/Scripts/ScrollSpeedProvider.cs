/// <summary>
/// Статический провайдер скорости прокрутки. LemmingPlaceView обновляет, Obstacles и idle-лемминги читают.
/// </summary>
public static class ScrollSpeedProvider
{
    public static float CurrentSpeed { get; set; }
}
