using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ПР5
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        StreamReader readAlph;
        StreamReader readProb;
        string pathAlph = @"C:\Alphabet.txt";
        string pathProb = @"C:\Probability.txt";

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists(pathAlph)) { readAlph = new StreamReader(pathAlph); }
            else
            {
                openFileDialog1.FileName = "Файл_с_алфавитом";
                openFileDialog1.Filter = "Alphabet (.txt)|*.txt";
                if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                { MessageBox.Show("Файл не найден. Пожалуйста, перейдите в пункт меню 'Подключить'"); return; }
                pathAlph = @openFileDialog1.FileName;
                readAlph = new StreamReader(pathAlph);
            }

            if (File.Exists(pathProb)) { readProb = new StreamReader(pathProb); }
            else
            {
                openFileDialog1.FileName = "Файл_с_вероятностями";
                openFileDialog1.Filter = "Probability (.txt)|*.txt";
                if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                { MessageBox.Show("Файл не найден. Пожалуйста, перейдите в пункт меню 'Подключить'"); return; }
                pathProb = @openFileDialog1.FileName;
                readProb = new StreamReader(pathProb);
            }

            dataGridView1.RowHeadersVisible = false;

            for(int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dataGridView1.Columns[i].ReadOnly = true;
            }

            if (readAlph != null && readProb != null) FillTable();
        }

        List<Alphabet> alphabet = new List<Alphabet>();
        bool filled;

        private void button1_Click(object sender, EventArgs e)
        {
            if (filled)
            {
                string encod = null;
                string cod = null;
                if (radioButton1.Checked)
                {                   
                    encod = richTextBox1.Text.ToUpperInvariant();
                    foreach (char c in encod)
                    {
                        for (int i = 0; i < dataGridView1.Rows.Count; i++)
                        {
                            if (c == Convert.ToChar(dataGridView1[0, i].Value)) cod += dataGridView1[2, i].Value.ToString() + " ";
                        }
                    }
                    if (cod != null) richTextBox1.Text =  cod;
                }
                else
                {
                    if (string.IsNullOrEmpty(richTextBox1.Text) && string.IsNullOrWhiteSpace(richTextBox1.Text)) return;

                    cod = richTextBox1.Text;
                    foreach (char a in cod) if (a != '0' && a != '1' && a != ' ') { MessageBox.Show("шифр-текст должен состоять только из 0 и 1"); return; }

                    cod = cod.Replace(" ", "");


                    for (int i = 0; i < cod.Length; i++) 
                    {
                        for (int j = 0; j < dataGridView1.Rows.Count; j++)
                        {
                            if (cod.Length - i >= dataGridView1[2, j].Value.ToString().Length)
                            {
                                string q = cod.Substring(i, dataGridView1[2, j].Value.ToString().Length);
                                if (q == dataGridView1[2, j].Value.ToString())
                                {
                                    encod += dataGridView1[0, j].Value.ToString();
                                    i += dataGridView1[2, j].Value.ToString().Length - 1;
                                    break;
                                }
                            }
                        }
                    }
                    if (encod != null) richTextBox1.Text = encod;
                }
            }
            else { MessageBox.Show("Не подключён либо файл с алфавитом, либо с вероятностями"); return; }

        }
       
        void FillTable()
        {
            dataGridView1.Rows.Clear();

            Reader();

            alphabet = Sort(alphabet);

            CrossVertex vert = Haffman(alphabet);

            foreach (Alphabet a in alphabet) dataGridView1.Rows.Add(a.word, a.probability, vert.Search(a.word));

            Parameters();

            filled = true;
        }

        CrossVertex Haffman(List<Alphabet> alph)
        {
            List<Alphabet> alphabet = new List<Alphabet>();
            CrossVertex vert = new CrossVertex(-1, alph[0], alph[1]);

            List<CrossVertex> v = new List<CrossVertex>();
            int size = alph.Count;

            for (int i = 0; size > 1; i++)
            {
                alphabet.Clear();

                if(alph[0].word == '▲' && alph[1].word != '▲')
                {
                    alphabet.Add(new Alphabet(i, '▲', alph[0].probability + alph[1].probability));
                    for (int j = 0; j < v.Count; j++)
                    {
                        if (v[j].Id == alph[0].id) vert = v[j];
                    }
                    v.Add(new CrossVertex(i, vert, alph[1]));
                }

                if (alph[0].word != '▲' && alph[1].word == '▲')
                {
                    alphabet.Add(new Alphabet(i, '▲', alph[0].probability + alph[1].probability));
                    for (int j = 0; j < v.Count; j++)
                    {
                        if (v[j].Id == alph[1].id) vert = v[j];
                    }
                    v.Add(new CrossVertex(i, alph[0], vert));
                }

                if(alph[0].word != '▲' && alph[1].word != '▲')
                {
                    alphabet.Add(new Alphabet(i, '▲', alph[0].probability + alph[1].probability));
                    v.Add(new CrossVertex(i, alph[0], alph[1]));
                }

                if(alph[0].word == '▲' && alph[1].word == '▲') 
                {
                    CrossVertex[] a = new CrossVertex[2];
                    alphabet.Add(new Alphabet(i, '▲', alph[0].probability + alph[1].probability));
                    for (int j = 0; j < v.Count; j++) 
                    {
                        if(v[j].Id == alph[0].id) a[0] = v[j];
                        if (v[j].Id == alph[1].id) a[1] = v[j];
                    }
                    v.Add(new CrossVertex(i, a[0], a[1]));
                }
                for (int j = 1; j < size - 1; j++) alphabet.Add(alph[j + 1]);
                alph = Sort(alphabet);
                size = alph.Count;
            }

            return v[v.Count - 1];
        }

        List<Alphabet> Sort(List<Alphabet> alph) //сортировка списка по возрастанию
        {
            List<Alphabet> alphabet = new List<Alphabet>();
            bool[] sorted = new bool[alph.Count];
            int choosed = 0;
            double best = 2;
            for (int i = 0; i < sorted.Length; i++) sorted[i] = false;

            for (int i = 0; i < alph.Count; i++)
            {
                for (int j = 0; j < alph.Count; j++)
                {
                    if (!sorted[j])
                        if (alph[j].probability < best)
                        {
                            choosed = j;
                            best = alph[j].probability;
                        }
                }
                best = 2;
                alphabet.Add(alph[choosed]);
                sorted[choosed] = true;
            }

            return alphabet;
        }

        private void Reader()
        {
            if(readAlph != null && readProb != null)
            {
                alphabet.Clear();

                readAlph.Close();
                readAlph = new StreamReader(pathAlph);

                readProb.Close();
                readProb = new StreamReader(pathProb);

                for(int i = 0; ; i++)
                {
                    string a = readAlph.ReadLine();
                    string b = readProb.ReadLine();
                    double k = 0;
                    char c;

                    if (a != null && b != null)
                    {
                        a.Trim(' ');
                        b.Trim(' ');
                    }
                    else
                    {
                        if (a == null && b == null) return;
                        else { MessageBox.Show("Документы и строки в них не должны быть пустыми"); return; }
                    }


                    try
                    {
                        k = Convert.ToDouble(b);
                        if (k > 1 || k < 0) { MessageBox.Show("Вероятность не может быть меньше 0 или больше 1"); return; }
                    }
                    catch (FormatException) { MessageBox.Show("Убедитесь, что вероятности вляются числами и дробная часть отделена запятой"); return; }

                    if (a.Length != 1) { MessageBox.Show("На строке должен быть только один символ алфавита"); return; }

                    c = Convert.ToChar(a);
                    alphabet.Add(new Alphabet(c, k));
                } 
            }
        }

        void Parameters()
        {
            label2.Text = "Средняя длина кодового слова\n Lср = ";
            label3.Text = "Избыточность Г = ";
            label4.Text = "Неравенство крафта К = ";

            double L = 0;
            double H = 0;
            double G = 0;
            double k = 0;

            double p;

            foreach(DataGridViewRow a in dataGridView1.Rows)
            {
                if(Convert.ToDouble(a.Cells[1].Value) != 0)
                {
                    p = Convert.ToDouble(a.Cells[1].Value);
                    L += a.Cells[2].Value.ToString().Length * p;
                    H += -p * Math.Log(p, 2);
                    k += Math.Pow(2, -a.Cells[2].Value.ToString().Length);
                }
            }
            L = Math.Round(L, 4);
            G = Math.Round(L - H, 4);
            k = Math.Round(k, 4);

            label2.Text += L.ToString();
            label3.Text += G.ToString();
            if (k <= 1) label4.Text += k + " <= 1\nНеравенство выполняется";
            else label4.Text += k + " <= 1\nНеравенство не выполняется";
        }

        private void алфавитToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "Файл_с_алфавитом";
            openFileDialog1.Filter = "Alphabet (.txt)|*.txt";
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
            { MessageBox.Show("Файл не найден. Пожалуйста, повторите попытку"); return; }
            pathAlph = @openFileDialog1.FileName;
            readAlph = new StreamReader(pathAlph);

            if (readAlph != null && readProb != null) FillTable();
        }

        private void вероятностиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "Файл_с_вероятностями";
            openFileDialog1.Filter = "Probability (.txt)|*.txt";
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
            { MessageBox.Show("Файл не найден. Пожалуйста, повторите попытку"); return; }
            pathProb = @openFileDialog1.FileName;
            readProb = new StreamReader(pathProb);

            if (readAlph != null && readProb != null) FillTable();
        }

        private void импортироватьСодержимоеФайлаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "Выберете_файл";
            openFileDialog1.Filter = "(.txt)|*.txt";
            if(openFileDialog1.ShowDialog() == DialogResult.Cancel)
            { MessageBox.Show("Файл не найден. Пожалуйста, повторите попытку"); return; }
            StreamReader a = new StreamReader(@openFileDialog1.FileName);
            richTextBox1.Text = a.ReadToEnd();
            a.Close();
        }

        private void экспортироватьФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "Выберете_файл";
            openFileDialog1.Filter = "(.txt)|*.txt";

            if (openFileDialog1.ShowDialog() != DialogResult.Cancel)
            {
                StreamWriter a = new StreamWriter(@openFileDialog1.FileName);
                a.WriteLine(richTextBox1.Text);
                a.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = null;
        }
    }

    public struct Alphabet
    {
        public char word { get; set; }
        public double probability { get; set; }
        public int id { get; set; }

        public Alphabet(char a, double x)
        {
            word = a;
            probability = x;
            id = -1;
        }

        public Alphabet(int i, char a, double x)
        {
            id = i;
            word = a;
            probability = x;
        }
    }

    public class CrossVertex
    {
        public Alphabet NameLeft { get; set; }
        public CrossVertex LeftSide { get; set; }
        public Alphabet NameRight { get; set; }
        public CrossVertex RightSide { get; set; }
        public int Id { get; set; }

        public double Probability
        {
            get 
            { 
                if(LeftSide == null && RightSide == null)
                {
                    return NameLeft.probability + NameRight.probability;
                }
                if(LeftSide != null && RightSide == null)
                {
                    return LeftSide.Probability + NameRight.probability;
                }
                if (LeftSide == null && RightSide != null)
                {
                    return RightSide.Probability + NameLeft.probability;
                }
                if (LeftSide != null && RightSide != null)
                {
                    return LeftSide.Probability + RightSide.Probability;
                }
                return 0;
            }
        }

        public CrossVertex(int i, Alphabet Left, Alphabet Right)
        {
            NameLeft = Left;
            NameRight = Right;
            Id = i;
        }

        public CrossVertex(int i, CrossVertex Left, CrossVertex Right) 
        {
            LeftSide = Left;
            RightSide = Right;
            Id = i;
        }

        public CrossVertex(int i, CrossVertex Left, Alphabet Right)
        {
            LeftSide = Left;
            NameRight = Right;
            Id = i;
        }

        public CrossVertex(int i, Alphabet Left, CrossVertex Right)
        {
            NameLeft = Left;
            RightSide = Right;
            Id = i;
        }

        public string Search(char w)
        {
            string a = null;

            if (NameLeft.word == w) return "0" + a;

            if (NameRight.word == w) return "1" + a;

            if (LeftSide != null) 
            { 
                a = LeftSide.Search(w);
                if (a != null) return "0" + a;
            }
            if (RightSide != null)
            {
                a = RightSide.Search(w);
                if (a != null) return "1" + a;
            }
            return null;
        }
    }

}
