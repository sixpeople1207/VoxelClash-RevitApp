using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myApp.C5D
{
    public abstract class ConnectionBase
    {
        public class DoNotApplyDBForBatchInternal : IDisposable
        {
            private ConnectionBase _conn;

            internal DoNotApplyDBForBatchInternal(ConnectionBase connection)
            {
                _conn = connection;
                _conn._doNotApplyDB = true;
            }

            public void Dispose()
            {
                _conn._doNotApplyDB = false;
            }
        }

        public static bool POC_AABB_REMOVE = false;

        public static int SDK_SCHEMA_VERSION = 2010;

        public static int UD_SDK_SCHEMA_VERSION = 2005;

        public static int ELEC_SCHEMA_VERSION = 2013;

        public static int ADDED_VERSION = -9999;

        public static int CHANGED_VERSION = -8888;

        public static int DELETED_VERSION = -7777;

        public static string LOCALADDED_VERSIONSEQUENCE = "-9999";

        public static string LOCALCHANGED_VERSIONSEQUENCE = "-9999";

        public static string LOCALDELETED_VERSIONSEQUENCE = "-9999";

        public static string ADDED_VERSIONSEQUENCE = "-9999";

        public static string CHANGED_VERSIONSEQUENCE = "-8888";

        public static string DELETED_VERSIONSEQUENCE = "-7777";

        protected bool _connected;

        protected string _connectionString = "";

        protected string _name = "";

        protected bool _autoCommit;

        protected bool _begin;

        protected int _transactionCount;

        protected int _dbSchemaVersion = 1;

        protected HashSet<string> _commitTables = new HashSet<string>();

        private bool _doNotApplyDB;

        protected virtual bool DoNotApplyDB => _doNotApplyDB;
    }
}
