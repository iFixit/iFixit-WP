using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Collections.Generic;

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
        public Table<Images> ImagesTable;
    }
}