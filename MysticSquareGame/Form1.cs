using System;
using System.Windows.Forms;

namespace MysticSquare
{
    public partial class Form1 : Form
    {
        public string playerName;

        public Form1()
        {
            InitializeComponent();
        }

        private void Start()
        {
            GameForm gameForm = new GameForm(this);
            gameForm.Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Чеклист п.4: Требуется ввести минимум 6 символов в поле с именем
            if (textBox1.Text.Length >= 6)
            {
                playerName = textBox1.Text;
                // Чеклист п.2: Нажатие на кнопку "Начать" открывает игровую форму и скрывает стартовую
                Start();
                Hide();
            }
            else
            {
                MessageBox.Show("Имя должно содержать хотя бы 6 символов");
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Чеклист п.3: Если поле с именем пустое, кнопка "Начать" не активна
            button1.Enabled = textBox1.Text.Length > 0;
        }
    }
}
