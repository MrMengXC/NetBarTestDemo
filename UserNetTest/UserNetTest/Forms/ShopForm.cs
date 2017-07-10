using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UserNetTest.Tools;

namespace UserNetTest.Forms
{
    public partial class ShopForm : Form
    {
        private enum ColumnType
        {
            C_Text = 0,
            C_Button,       //按钮
            C_Check,        //复选框
            C_Custom
        }
        private enum TitleList
        {
            None = 0,
            Name,
            Operation,

        }
        private DataTable mainTable;
        private SimpleModel model;
        private IList<StructGoods> goods;
        public ShopForm(SimpleModel tem)
        {
            InitializeComponent();
            mainTable = new DataTable();
            model = tem;
            InitUI();
        }
        //初始化UI
        private void InitUI()
        {

            //初始化GridView
           
            SetColumn(ColumnType.C_Text, TitleList.Name.ToString(), "商品",null);
            string[] btns = { "购买"};
            SetColumn(ColumnType.C_Button, TitleList.Operation.ToString(), "操作", btns);
            this.gridControl1.DataSource = this.mainTable;

            GoShop();

        }

        //进入商店获取商品列表
        private void GoShop()
        {
            ClientNetOperation.GoShop(model.manage, GoShopResult,1002,"");
        }

        //进入商店结果回调
        private void GoShopResult(ResultModel result)
        {
            if(result.pack.Cmd != Cmd.CMD_GOODS_FIND)
            {
                return;
            }

            System.Console.WriteLine("GoShopResult:"+result.pack);
            model.manage.RemoveResultBlock(GoShopResult);

            if(result.pack.Content.MessageType == 1)
            {
                this.Invoke(new UIHandleBlock(delegate () {
                    this.goods = result.pack.Content.ScGoodsFind.GoodsList;

                    RefreshGridControl();

                }));
            }


        }

        //刷新GridControl
        private void RefreshGridControl()
        {
            this.mainTable.Rows.Clear();
            foreach(StructGoods goods in this.goods)
            {
                AddNewRow(goods);
            }


        }
        //添加新行
        private void AddNewRow(StructGoods goods)
        {
            DataRow row = this.mainTable.NewRow();
            this.mainTable.Rows.Add(row);
            row[TitleList.Name.ToString()] = goods.GoodsName;
        }
        #region 设置GridView
        private void SetColumn(ColumnType type,string fieldname,string columnname,string[] buttonNames)
        {


            int i = 0;
           
            GridColumn column = gridView1.Columns.AddVisible(fieldname, columnname);
            column.AppearanceHeader.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            column.AppearanceCell.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            column.OptionsColumn.AllowEdit = false;

            switch (type)
            {





                    #region 添加复选框
                case ColumnType.C_Check:        //添加复选框
                {
                        RepositoryItemCheckEdit check = new RepositoryItemCheckEdit();
                        check.NullStyle = DevExpress.XtraEditors.Controls.StyleIndeterminate.Unchecked;
                        column.ColumnEdit = check;
                        column.OptionsColumn.AllowEdit = true;
                        column.Width = 40;
                        DataColumn dataColumn = new DataColumn(fieldname);
                        mainTable.Columns.Add(dataColumn);
                        dataColumn.DataType = typeof(bool);
                }
                    break;


                #endregion

                #region 添加按钮
                case ColumnType.C_Button:        //添加按钮
                {
                        RepositoryItemButtonEdit buttonEdit = new RepositoryItemButtonEdit();
                        buttonEdit.Buttons.Clear();
                        buttonEdit.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.HideTextEditor;
                        buttonEdit.ButtonClick += ButtonColumn_ButtonClick;

                        #region 添加按钮
                        int num = 0;
                        int width = 8;
                        foreach (string name in buttonNames)
                        {


                            EditorButton button = new EditorButton();
                            button.Kind = ButtonPredefines.Glyph;
                            button.Appearance.ForeColor = Color.Blue;

                            //按钮显示
                            button.Visible = true;
                            button.Tag = fieldname + "_" + num;
                            button.Caption = name;
                            width += 50;

                            //button.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
                            button.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                            buttonEdit.Buttons.Add(button);
                            num++;
                        }
                        #endregion
                        column.ColumnEdit = buttonEdit;
                        column.Width = width;
                        //column.MinWidth = width;
                        column.OptionsColumn.AllowEdit = true;
                        column.UnboundType = DevExpress.Data.UnboundColumnType.String;
                        DataColumn dataColumn = new DataColumn(fieldname);
                        mainTable.Columns.Add(dataColumn);
                    }
                break;
            #endregion
                default:
                        {
                column.SortMode = ColumnSortMode.Custom;

                DataColumn dataColumn = new DataColumn(fieldname);
                mainTable.Columns.Add(dataColumn);
                //dataColumn.DataType = typeof(Int32);

            }

            break;
        }
        i++;

            gridView1.OptionsSelection.MultiSelect = true;
            gridView1.OptionsSelection.MultiSelectMode = GridMultiSelectMode.RowSelect;
            gridView1.RowHeight = 40;

            //关闭最左侧
            gridView1.OptionsView.ShowIndicator = false;
            //关闭表头右键快捷键
            gridView1.OptionsMenu.EnableColumnMenu = false;
          


        }

        #endregion

        //按钮列点击事件
        private void ButtonColumn_ButtonClick(object sender, ButtonPressedEventArgs args)
        {
            int row = this.gridView1.FocusedRowHandle;
            StructGoods goods = this.goods[row];

            ClientNetOperation.PreBuyProduct(model.manage, PreBuyProductResult, model.card, goods.GoodsId, 5);


       }
        //预购买结果
        private void PreBuyProductResult(ResultModel result)
        {
            if(result.pack.Cmd != Cmd.CMD_PREBUY)
            {
                return;
            }
            System.Console.WriteLine("PreBuyProductResult:"+result.pack);
            model.manage.RemoveResultBlock(PreBuyProductResult);
            if(result.pack.Content.MessageType == 1)
            {
                MessageBox.Show("购买成功");
            }

        }
    }

    
}
