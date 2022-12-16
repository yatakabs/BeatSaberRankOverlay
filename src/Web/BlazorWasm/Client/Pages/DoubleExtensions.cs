namespace RankOverlay.Web.BlazorWasm.Client.Pages;

public static class DoubleExtensions
{
    public static string ToScoreSaberString(
        this double value)
    {
        return value.ToString("#,0.00");
    }

    public static string ToScoreSaberDiffString(
        this double value)
    {
        return value.ToString("+#,0.00;-#,0.00");
    }

    public static string ToScoreSaberString(
        this long value)
    {
        return value.ToString("#,#");
    }

    public static string ToScoreSaberString(
        this int value)
    {
        return value.ToString("#,#");
    }

    public static string ToPercent(
        this double value)
    {
        return value.ToString("#,#.00%");
    }
}
