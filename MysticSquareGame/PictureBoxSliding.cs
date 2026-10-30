using System.Windows.Forms;

namespace MysticSquare;

public class PictureBoxSliding : PictureBox
{
    // Истинные (целевые) координаты плитки в сетке (0..3)
    public int trueX;
    public int trueY;

    // Текущие координаты плитки в сетке (0..3)
    public int curX;
    public int curY;
}
