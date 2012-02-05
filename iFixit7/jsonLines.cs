using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace iFixit7
{
    public class jsonLines
    {
        private string bullet;
        private int level;
        private string text;

        public jsonLines()
        {
            bullet = "";
            level = 0;
            text = "";
        }

        public jsonLines(string Bullet, int Level, string Text)
        {
            bullet = Bullet;
            level = Level;
            text = Text;
        }

        public void setBullet(string Bullet)
        {
            bullet = Bullet;
        }

        public void setLevel(int Level)
        {
            level = Level;
        }

        public void setText(string Text)
        {
            text = Text;
        }

        public string getBullet()
        {
            return bullet;
        }

        public int getLevel()
        {
            return level;
        }

        public string getText()
        {
            return text;
        }
    }
}
