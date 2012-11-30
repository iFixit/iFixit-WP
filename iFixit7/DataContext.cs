using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace iFixit7
{
    public class iFixitDataContext : DataContext
    {
        // Pass the connection string to the base class.
        public iFixitDataContext(string connectionString)
            : base(connectionString)
        { }

        // Specify all tables?
        //public Table<Category> CategoriesTable;
        public Table<Topic> TopicsTable;
        public Table<Guide> GuidesTable;
        public Table<Step> StepsTable;
        //public Table<Images> ImagesTable;
        public Table<Lines> LinesTable;
    }

    public class DBHelpers
    {
        /*
         * FIXME add methods simillar to above for Topic, Guide, Step!
         */
        public static Topic GetCompleteTopic(string name, iFixitDataContext dc)
        {
            Topic t = null;

            //get the node
            t = dc.TopicsTable.FirstOrDefault(c => c.Name == name);

            if (t == null)
                return null;

            //get its child guides
            t.Guides = dc.GuidesTable.Where(g => g.parentName == name).Distinct().ToList();

            return t;
        }

        public static Guide GetCompleteGuide(string guideID, iFixitDataContext dc)
        {
            Guide g = null;

            //get the node
            g = dc.GuidesTable.FirstOrDefault(c => c.GuideID == guideID);

            if (g == null)
                return null;

            //get its child steps
            g.Steps = dc.StepsTable.Where(s => s.parentName == g.GuideID).Distinct().ToList();

            List<Step> completeSteps = new List<Step>();
            for (int i = 0; i < g.Steps.Count; ++i)
            {
                string parentGuideId = g.Steps.ElementAt(i).parentName;
                string stepIndex = g.Steps.ElementAt(i).StepIndex;
                completeSteps.Add(GetCompleteStep(parentGuideId, stepIndex, dc));
            }

            g.Steps = completeSteps;

            return g;
        }

        public static Step GetCompleteStep(string parentGuideId, string stepNumber, iFixitDataContext dc)
        {
            Step s = null;

            //get the node
            s = dc.StepsTable.FirstOrDefault(c => c.parentName == parentGuideId && c.StepIndex == stepNumber);

            if (s == null)
                return null;

            //get its child lines
            s.Lines = dc.LinesTable.Where(l => l.parentName == s.parentName + stepNumber).Distinct().ToList();

            return s;
        }
    }
}