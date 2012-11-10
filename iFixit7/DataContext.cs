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
        public Table<Category> CategoriesTable;
        public Table<Topic> TopicsTable;
        public Table<Guide> GuidesTable;
        public Table<Step> StepsTable;
        //public Table<Images> ImagesTable;
        public Table<Lines> LinesTable;
    }

    public class DBHelpers
    {
        /*
         * Recursively inserts an entire fully-formed tree (with topics) into the DB
         */
        public static void InsertTree(Category root, iFixitDataContext dc)
        {
            Category dbCopy = GetCompleteCategory(root.Name, dc);

            if (dbCopy == null)
            {
                dc.CategoriesTable.InsertOnSubmit(root);
                dc.CategoriesTable.InsertAllOnSubmit(root.Categories);
                dc.TopicsTable.InsertAllOnSubmit(root.Topics);
            }
            else
            {
                var catsToRemove = dbCopy.Categories.Except(root.Categories);
                var catsToAdd = root.Categories.Except(dbCopy.Categories);
                dc.CategoriesTable.InsertAllOnSubmit(catsToAdd);
                dc.CategoriesTable.DeleteAllOnSubmit(catsToRemove);
                //dc.TopicsTable.InsertAllOnSubmit(root.Topics);

                //Debug.WriteLine("\t\tinsert: cat = " + root.Name);

                //foreach (Category c in root.Categories)
                //{
                //    InsertTree(c, dc);
                //}
                //else 
                //{
                var topicsToRemove = dbCopy.Topics.Except(root.Topics);
                var topicsToAdd = root.Topics.Except(dbCopy.Topics);
                dc.TopicsTable.InsertAllOnSubmit(topicsToAdd);
                dc.TopicsTable.DeleteAllOnSubmit(topicsToRemove);
            }

                //iterate through all children in all cases
                foreach (Category c in root.Categories)
                {
                    InsertTree(c, dc);
                }
        }

        /*
         * Get a node of the tree, and populate its collections
         */
        public static Category GetCompleteCategory(string catName, iFixitDataContext dc)
        {
            Category n = null;

            //get the node
            n = dc.CategoriesTable.FirstOrDefault(c => c.Name == catName);

            if (n == null)
                return null;

            //get its child categories
            n.Categories = dc.CategoriesTable.Where(c => c.parentName == catName).OrderBy(c => c.Name).ToList();
            /*
            var qc = from c in dc.CategoriesTable
                     where c.parentName == catName
                     orderby c.Name
                     select c;
            n.Categories = qc.Distinct().ToList();
            */

            //get its child topics
            n.Topics = dc.TopicsTable.Where(t => t.parentName == catName).OrderBy(t => t.Name).ToList();
            /*
            var qt = from t in dc.TopicsTable
                     where t.parentName == catName
                     orderby t.Name
                     select t;
            n.Topics = qt.Distinct().ToList();
            */
            return n;
        }

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

        public static Guide GetCompleteGuide(string title, iFixitDataContext dc)
        {
            Guide g = null;

            //get the node
            g = dc.GuidesTable.FirstOrDefault(c => c.Title == title);

            if (g == null)
                return null;

            //get its child guides
            g.Steps = dc.StepsTable.Where(s => s.parentName == title).Distinct().ToList();

            return g;
        }

        public static Step GetCompleteStep(string title, iFixitDataContext dc)
        {
            Step s = null;

            //get the node
            s = dc.StepsTable.FirstOrDefault(c => c.Title == title);

            if (s == null)
                return null;

            //get its child guides
            s.Lines = dc.LinesTable.Where(l => l.parentName == title).Distinct().ToList();

            return s;
        }
    }
}