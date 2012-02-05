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
using System.Collections.Generic; //ADD TO PROJECTS!!!

namespace iFixit7
{
    public class StepTemp
    {
        private List<jsonImage> images = new List<jsonImage>();
        private List<jsonLines> lines = new List<jsonLines>();
        private int stepNumber;
        private string title;


        StepTemp(List<jsonImage> Images = null, List<jsonLines> Lines = null, int StepNumer = 0, string Title = null) //Default Values may not work!! Double check
        {
            images = Images;
            lines = Lines;
            stepNumber = StepNumer;
            title = Title;
        }

        public List<jsonImage> getImageList()
        {
            return images;
        }

        public jsonImage getImage(int imageNum)
        {
            return images[imageNum];
        }

        public List<jsonLines> getLinesList()
        {
            return lines;
        }

        public jsonLines getLines(int lineNum)
        {
            return lines[lineNum];
        }

        public int getStepNum()
        {
            return stepNumber;
        }

        public string getTitle()
        {
            return title;
        }

        public void setImageList(List<jsonImage> imageList)
        {
            images = imageList;
        }

        public void setImage(int imageNum, jsonImage image)
        {
            images[imageNum] = image;
        }

        public void setLinesList(List<jsonLines> lineList)
        {
            lines = lineList;
        }

        public void setLines(int lineNum, jsonLines line)
        {
            lines[lineNum] = line;
        }

        public void setStepNum(int stepNum)
        {
            stepNumber = stepNum;
        }

        public void setTitle(string Title)
        {
            title = Title;
        }

        public void addLine(jsonLines line)
        {
            lines.Add(line);
        }

        public void addImage(jsonImage image)
        {
            images.Add(image);
        }
    }
}
