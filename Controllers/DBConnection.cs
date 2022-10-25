using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace NitroConnector.Controllers
{
    public class DBConnection
    {

        public SqlConnectionStringBuilder connString()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "avensia-im-common-sqlserver.database.windows.net";
            builder.UserID = "imadminsqlserver";
            builder.Password = "dfeusrg&W7rhriw3u1!";
            builder.InitialCatalog = "avensia-im-common-sqlserver-dev";

            return builder;
        }
    }
}
