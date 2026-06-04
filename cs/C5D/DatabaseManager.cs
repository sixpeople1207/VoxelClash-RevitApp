using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SQLitePCL;

namespace DDWorks_Shop_Designer.Database
{
    public class DatabaseManager
    {
        private string _path;
        private SqliteConnection conn;

        public DatabaseManager(string path)
        {
            try
            {
                _path = path;

                // SQLitePCL 초기화 (중요)

                // DB 파일 없으면 생성
                if (!File.Exists(path))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    File.Create(path).Dispose();
                }

                string connStr = $"Data Source={path}";

                conn = new SqliteConnection(connStr);
                conn.Open();
            }
            catch (Exception ex)
            {
                throw new Exception("Database init failed: " + ex.Message, ex);
            }
        }

        public DataTable ReadToDataTable(string tableName)
        {
            DataTable dt = new DataTable();
            // 1. SqliteCommand 생성 (큰따옴표 처리를 통해 테이블명 내 특수문자 대응)
            using (var cmd = new SqliteCommand($"SELECT * FROM \"{tableName}\"", conn))
            {
                // 2. DataReader를 열어 데이터 스트림 준비
                using (var reader = cmd.ExecuteReader())
                {
                    // 3. DataTable의 Load 메서드가 알아서 행(Row) 구조를 파악하고 데이터를 채워줍니다.
                    dt.Load(reader);
                }
            }

            return dt;
        }
    }

}