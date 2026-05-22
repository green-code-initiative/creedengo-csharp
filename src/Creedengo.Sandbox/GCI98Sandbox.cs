namespace Creedengo.Sandbox;

internal class RCS1200Sandbox
{
    public static void Sort()
    {

        var items = new (int a, int b)[] { 
            (1, 2), 
            (3, 4), 
            (5, 6) 
        };

        var sorted = items.OrderBy(item => item.Item1)
             .OrderBy(item => item.Item2);

    }

}
