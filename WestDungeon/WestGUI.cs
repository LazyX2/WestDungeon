using System;

namespace WestDungeon
{
    public partial class WestGUI
    {
        public static string Show(string title, string subtext, string[] buttons) {
            Form form = new Form();
            Size s = new Size(250, 250);
            Button[] btns = new Button[buttons.Length];
            string option = "§";
            for (int i = 0; i < buttons.Length; i++) {
                int index = i;
                s += new Size(buttons[index].Length * 5, 0);
                btns[index] = new Button()
                {
                    Text = buttons[index],
                    Location = new Point(index * 100, 10)
                };
                btns[index].Click += (o, s) =>
                {
                    option = btns[index].Text;
                    form.DialogResult = DialogResult.OK;
                    form.Close();
                };
                form.Controls.Add(btns[index]);
            }

            form.Size = s;
            form.ShowDialog();

            return option;
        }
    }
}
