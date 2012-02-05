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
    public class jsonImage
    {
        private int imageid;
        private int orderby;
        private string text;

        jsonImage(int imageId, int orderBy, string Text)
        {
            imageid = imageId;
            orderby = orderBy;
            text = Text;
        }

        public void setImageId(int imageId)
        {
            imageid = imageId;
        }

        public void setOrderBy(int orderBy)
        {
            orderby = orderBy;
        }

        public void setText(string Text)
        {
            text = Text;
        }

        public int getImageId()
        {
            return imageid;
        }

        public int getOrderBy()
        {
            return orderby;
        }

        public string getText()
        {
            return text;
        }
    }
}
