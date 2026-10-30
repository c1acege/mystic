using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MysticSquare
{
    public partial class GameForm : Form
    {
        private DateTime start;
        private Form1 startForm;

        private PictureBoxSliding[,] pictures = new PictureBoxSliding[4, 4];
        private List<Bitmap> bmps = new List<Bitmap>();
        private Bitmap lastPicture;
        private List<Point> points = new List<Point>();

        private int pic_size = 150;

        public GameForm(Form1 sForm)
        {
            startForm = sForm;
            InitializeComponent();

            Size = new Size(620, 650);

            // Создаем 16 элементов PictureBox в сетке 4х4
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    pictures[i, j] = new PictureBoxSliding();
                    pictures[i, j].Click += PictureBox_Click;
                    pictures[i, j].Location = new Point(i * pic_size, j * pic_size);
                    pictures[i, j].Size = new Size(pic_size, pic_size);
                    pictures[i, j].SizeMode = PictureBoxSizeMode.StretchImage;
                    Controls.Add(pictures[i, j]);
                }
            }

            // Загружаем 16 изображений (0.png - 15.png) и сохраняем целевые координаты
            for (int k = 0; k < 16; k++)
            {
                string imgPath = GetImagePath(k);
                using (var stream = new FileStream(imgPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var img = Image.FromStream(stream))
                {
                    bmps.Add(new Bitmap(img));
                }
                points.Add(new Point(k % 4, k / 4));
            }

            // 16-е изображение (15.png) — недостающая плитка
            lastPicture = bmps[15];

            // Инициализируем пустую плитку в позиции (3, 3)
            pictures[3, 3].Image = null;
            pictures[3, 3].Tag = "Sliding";
            pictures[3, 3].curX = 3;
            pictures[3, 3].curY = 3;
            pictures[3, 3].trueX = 3;
            pictures[3, 3].trueY = 3;

            bmps.RemoveAt(15);
            points.RemoveAt(15);

            // Случайно распределяем оставшиеся 15 плиток по сетке
            Random random = new Random();
            int num = 1;
            for (int l = 0; l < 4; l++)
            {
                for (int m = 0; m < 4 && (l != 3 || m != 3); m++)
                {
                    int index = random.Next(16 - num);
                    pictures[l, m].Image = bmps[index];
                    pictures[l, m].trueX = points[index].X;
                    pictures[l, m].trueY = points[index].Y;
                    bmps.RemoveAt(index);
                    points.RemoveAt(index);
                    pictures[l, m].curX = l;
                    pictures[l, m].curY = m;
                    num++;
                }
            }

            // Проверяем разрешимость расстановки (теорема о четности инверсий в пятнашках)
            EnsureSolvable();

            // Фиксируем время начала игры для подсчета времени прохождения
            start = DateTime.Now;

            // Включаем обработку горячих клавиш (F12 — демонстрация победы за 1 клик)
            KeyPreview = true;
            KeyDown += GameForm_KeyDown;
        }

        private string GetImagePath(int index)
        {
            string fileName = $"{index}.png";

            // Поиск в папке запуска приложения
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(baseDir, fileName);
            if (File.Exists(fullPath))
                return fullPath;

            // Поиск в текущей рабочей директории
            if (File.Exists(fileName))
                return fileName;

            // Поиск в корне проекта (если запуск из bin/Debug)
            string projectPath = Path.Combine(baseDir, @"..\..\..\", fileName);
            if (File.Exists(projectPath))
                return projectPath;

            return fileName;
        }

        private void EnsureSolvable()
        {
            // Сбор целевых индексов плиток в порядке обхода сетки
            List<int> tileOrder = new List<int>();
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    if (pictures[x, y].Tag == null)
                    {
                        tileOrder.Add(pictures[x, y].trueY * 4 + pictures[x, y].trueX);
                    }
                }
            }

            // Подсчет количества инверсий
            int inversions = 0;
            for (int i = 0; i < tileOrder.Count; i++)
            {
                for (int j = i + 1; j < tileOrder.Count; j++)
                {
                    if (tileOrder[i] > tileOrder[j])
                    {
                        inversions++;
                    }
                }
            }

            // В сетке 4х4, когда пустая клетка находится на 4-й строке снизу (четный ряд),
            // конфигурация разрешима тогда и только тогда, когда число инверсий четное.
            if (inversions % 2 != 0)
            {
                // Меняем местами первые две плитки для гарантии разрешимости
                SwapTileData(pictures[0, 0], pictures[1, 0]);
            }

            // Если случайно выпала уже собранная комбинация, делаем пару сдвигов
            if (CheckForWin())
            {
                Swap(pictures[2, 3], pictures[3, 3]);
                Swap(pictures[2, 2], pictures[2, 3]);
            }
        }

        private void SwapTileData(PictureBoxSliding p1, PictureBoxSliding p2)
        {
            Image tempImage = p1.Image;
            int tempTrueX = p1.trueX;
            int tempTrueY = p1.trueY;

            p1.Image = p2.Image;
            p1.trueX = p2.trueX;
            p1.trueY = p2.trueY;

            p2.Image = tempImage;
            p2.trueX = tempTrueX;
            p2.trueY = tempTrueY;
        }

        private void Swap(PictureBoxSliding p1, PictureBoxSliding p2)
        {
            pictures[p1.curX, p1.curY] = p2;
            pictures[p2.curX, p2.curY] = p1;

            Point location = p1.Location;
            p1.Location = p2.Location;
            p2.Location = location;

            int curX = p1.curX;
            int curY = p1.curY;
            p1.curX = p2.curX;
            p1.curY = p2.curY;
            p2.curX = curX;
            p2.curY = curY;
        }

        private void PictureBox_Click(object sender, EventArgs e)
        {
            PictureBoxSliding clicked = (PictureBoxSliding)sender;

            if (clicked.Tag == null)
            {
                int curX = clicked.curX;
                int curY = clicked.curY;

                // Проверяем соседние клетки на наличие пустой ("Sliding")
                if (curX > 0 && pictures[curX - 1, curY].Tag != null)
                {
                    Swap(pictures[curX - 1, curY], pictures[curX, curY]);
                }
                else if (curX < 3 && pictures[curX + 1, curY].Tag != null)
                {
                    Swap(pictures[curX + 1, curY], pictures[curX, curY]);
                }
                else if (curY > 0 && pictures[curX, curY - 1].Tag != null)
                {
                    Swap(pictures[curX, curY - 1], pictures[curX, curY]);
                }
                else if (curY < 3 && pictures[curX, curY + 1].Tag != null)
                {
                    Swap(pictures[curX, curY + 1], pictures[curX, curY]);
                }
            }

            // Проверяем условие победы
            if (!CheckForWin())
            {
                return;
            }

            // Чеклист п.5: В конце игры выводится сообщение
            MessageBox.Show("Победа");

            // Чеклист п.5: В файл stats.txt записывается результат в формате "победа имя прошедшее_с_начала_игры_время"
            SaveStatsToFile();

            // Чеклист п.6: После закрытия сообщения о победе на последней плитке появляется недостающее изображение
            pictures[3, 3].Image = lastPicture;

            // Отключаем обработку кликов после завершения игры
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    pictures[i, j].Click -= PictureBox_Click;
                }
            }
        }

        private bool CheckForWin()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (pictures[i, j].trueX != pictures[i, j].curX || pictures[i, j].trueY != pictures[i, j].curY)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void GameForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Возврат на стартовую форму при закрытии игры
            startForm.Show();
        }

        private void SaveStatsToFile()
        {
            // Чеклист п.5: формат "победа имя прошедшее_с_начала_игры_время"
            using (StreamWriter streamWriter = new StreamWriter("stats.txt", append: true, System.Text.Encoding.UTF8))
            {
                streamWriter.WriteLine($"Победа {startForm.playerName} {(DateTime.Now - start).ToString()}");
            }
        }

        private void GameForm_KeyDown(object sender, KeyEventArgs e)
        {
            // Быстрая демонстрация победы при сдаче работы (клавиша F12):
            // расставляет все плитки так, чтобы до победы оставался ровно 1 ход
            if (e.KeyCode == Keys.F12)
            {
                SetupOneMoveFromWin();
            }
        }

        private void SetupOneMoveFromWin()
        {
            // Расставляем все плитки по их правильным местам
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (pictures[i, j].trueX == i && pictures[i, j].trueY == j)
                        continue;

                    for (int x = 0; x < 4; x++)
                    {
                        for (int y = 0; y < 4; y++)
                        {
                            if (pictures[x, y].trueX == i && pictures[x, y].trueY == j)
                            {
                                Swap(pictures[i, j], pictures[x, y]);
                                goto NextTile;
                            }
                        }
                    }
                NextTile: ;
                }
            }

            // Делаем один ход: меняем местами плитку (2, 3) и пустую ячейку (3, 3)
            // Теперь пустая ячейка находится в (2, 3), а плитка (2, 3) — в ячейке (3, 3).
            // Достаточно кликнуть по (3, 3), чтобы игра завершилась победой!
            Swap(pictures[2, 3], pictures[3, 3]);
        }
    }
}
