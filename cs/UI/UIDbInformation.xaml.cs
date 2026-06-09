using DDWorks_Shop_Designer.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TemplateRevitCs;
using myApp.Drawing;
using Autodesk.Revit.DB;
using myApp.Database;
using System.Numerics;
using Autodesk.Revit.UI;

namespace myApp.UI
{
    /// <summary>
    /// UIDbInformation.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UIDbInformation : Window
    {
        public string dbPath = "";
        public string modelName = "";
        public string tableName = "";
        public DataTable dataTable = null;
        public DatabaseManager dbManager = null;
        public List<Vector3> modelVertex = null;
        UIDocument uidoc = null;
        Document doc = null;

        public UIDbInformation(UIDocument uiDoc)
        {
            uidoc = uiDoc;

            doc = uidoc.Document;
            InitializeComponent();
        }

        private void dataGridFindResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
     
        }

        private async void inIt()
        {
            dbPath = this.textBoxDBPath.Text;

            if (dbPath != "")
            {
                dbManager = new DatabaseManager(dbPath);
                var tbNames = dbManager.GetTableNames();

                await Task.Run(() =>
                {
                    //var tbList = new List<TableNames>();
                    //foreach (string dbName in tbNames)
                    //{
                    //    tbList.Add(new Database.TableNames(1, dbName, "C:\\test"));
                    //}

                    Dispatcher.Invoke(() =>
                    {
                        comboBoxTbList.ItemsSource = tbNames;
                    });
                });
              
            }
            else
            {
                System.Windows.MessageBox.Show("DB 경로와 모델 이름을 입력해주세요.");
            }

        }

        private void btnModeling_Click(object sender, RoutedEventArgs e)
        {
            var dataRow = dataGridFindResult.SelectedItem as DataRowView;
            textBoxLog.Text = "";
            if (dataRow != null)
            {
                modelName = dataRow["Name"].ToString();
                var modelData = dataRow["Model"];
                C5DModel cmodel = null;

                if (dataTable != null && modelData != null)
                {
                    cmodel = new C5DModel((byte[])modelData, Convert.ToInt32(textBoxShift.Text));
                    modelVertex = cmodel.vertexData;
                    Modeling modeling = new Modeling();
                    modeling.DrawMeshWithVectorFacesTo3D(doc, modelVertex);
                        
                    textBoxLog.AppendText($"Model Type :{cmodel.modelType}\n");
                    foreach(var v in modelVertex)
                    {
                        textBoxLog.AppendText($"vx:{v.X} vy:{v.Y} vz:{v.Z}\n");

                        break;
                    }
                   // textBoxLog.AppendText($"모델 {modelName}이(가) 성공적으로 생성되었습니다.\n");
                }
                else
                {
                    System.Windows.MessageBox.Show("먼저 DB와 테이블이름을 선택 해주세요.");
                }
            }
            
        }

        private void comboBoxTbList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tableNames = comboBoxTbList.SelectedItem?.ToString();
            if (tableNames != null)
            {
                DataTable dt = dbManager.ReadToDataTable(tableNames);
                dataGridFindResult.ItemsSource = dt.DefaultView;
                dataTable = dt;
            }
        }

        private void btnDBConnect_Click(object sender, RoutedEventArgs e)
        {
            inIt();
        }
    }
}
