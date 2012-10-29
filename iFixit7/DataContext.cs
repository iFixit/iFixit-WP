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
            dc.CategoriesTable.InsertOnSubmit(root);
            dc.TopicsTable.InsertAllOnSubmit(root.Topics);

            //Debug.WriteLine("\t\tinsert: cat = " + root.Name);

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
            /*
            var q = from c in dc.CategoriesTable
                    where c.Name == catName
                    select c;
            n = q.FirstOrDefault();
             * */
            n = dc.CategoriesTable.SingleOrDefault(c => c.Name == catName);

            if (n == null)
                return null;

            //get its child categories
            /*
            var q = from cats in dc.CategoriesTable
                join c in dc.TopicsTable on cats.parentName equals n.Name
                select cats;
            n.Categories = q.Distinct().ToList();
             */
            n.Categories = dc.CategoriesTable.Where(c => c.parentName == catName).Distinct().ToList();

            //get its child topics
            /*
            var q2 = from tops in dc.TopicsTable
                    join t in dc.TopicsTable on tops.parentName equals n.Name
                    select tops;
            n.Topics = q2.Distinct().ToList();
            */
            n.Topics = dc.TopicsTable.Where(t => t.parentName == catName).Distinct().ToList();

            return n;
        }

        /*
         * FIXME add methods simillar to above for Topic, Guide, Step!
         */
    }
}