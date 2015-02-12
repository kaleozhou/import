using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Common;

namespace Import
{
    public partial class Frm_Main : Form
    {
        BLL.IBLL DB = new BLL.IBLL();
        public Frm_Main()
        {
            InitializeComponent();
            ConnectData.DataType = "MYSQL";
            ConnectData.StaticServerIP = "127.0.0.1";// "127.0.0.1";//的三分到手
            ConnectData.StaticDataName = "new";
            ConnectData.StaticUserName = "root";
            ConnectData.StaticPwd = "qjkj@2014";            
        }

        /// <summary>
        /// 供应商导入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            ConnectData mydata = GetConnect.GetConn();
            DataTable dt = mydata.Fill("select * from hdsy_suppliers order by id asc").Tables[0];
            
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow Rs=dt.Rows[i];
                MODEL.Supplier model = new MODEL.Supplier();//供应商导入
                model.SupplierNumber = Rs["suppliers_nb"].ToString();//供应商编号
                model.AccountBalance = Rs["account"].ToString().Todbl();//账户金
                model.SupplierName = Rs["suppliers_name"].ToString();//名称
                model.Company = Rs["suppliers_name"].ToString();//名称                               

                model.InArea = Rs["province"].ToString();//所在省份
                model.Area1 = Rs["province"].ToString();//省
                model.Area2 = Rs["city"].ToString();//市
                model.Area3 = Rs["city"].ToString();//县
                model.Address = Rs["address"].ToString();//地址

                model.MainLineArea = Rs["travel_city"].ToString();//主要线路区域

                model.PayType = Rs["clear_method"].ToString();//结算方式？？varchar16
                model.AddDate = Convert.ToDateTime(Rs["addtime"].ToString());//添加日期

                model.SettlementCommission = Convert.ToDecimal(Rs["fyprice"].ToString());//结算返佣

                model.SupplierStatus = true;
                if (DB.I_Supplier.GetSingleModelBy(s=>s.SupplierNumber==model.SupplierNumber)==null)
	            {	 
                    DataRow bankRow = mydata.GetRs("select * from hdsy_bank where con_nb='" + Rs["suppliers_nb"] + "'");
                    if (bankRow!=null)
                    {
                        model.BankName = bankRow["bank_name"].ToString() + "-"+bankRow["bank_branch"].ToString();//银行名称
                        model.BankAccount = bankRow["bank_card"].ToString().Replace(" ","").Replace("　","");//卡号
                        model.AccountName = bankRow["bank_user"].ToString();//户主
                    }                   
                    DB.I_Supplier.Add(model);


                    model.UserName = model.SupplierNumber.ToString();//供应商编号为用户名
                    model.Pwd = Tool.Md5("1", model.UserName);//以后注意改密
                    //model.Pwd = Rs["password"].ToString();//密码

                    DB.I_Supplier.ExecNonSql("update set Supplier set UserName='"+model.UserName+"',Pwd='"+model.Pwd+"' where SupplierId="+model.SupplierId);
                }
            }
            MessageBox.Show("Supplier导入成功！"); 

        }

        /// <summary>
        /// 线路表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            ConnectData mydata = GetConnect.GetConn();
            DataTable dt = mydata.Fill("select * from hdsy_plan order by id asc").Tables[0];//线路导入
            List<string> trafficType = new List<string>();
            trafficType.Add("飞去飞回");
            trafficType.Add("火车往返");
            trafficType.Add("卧去卧回");
            trafficType.Add("汽车往返");
            trafficType.Add("动去动回");
            trafficType.Add("飞去卧回");
            trafficType.Add("卧去飞回");
            trafficType.Add("卧去动回");
            trafficType.Add("动去卧回");
            trafficType.Add("火车去汽车回");
            trafficType.Add("汽车去火车回");
            trafficType.Add("船去船回");
            trafficType.Add("其他");
            List<string> travel_grade=new List<string>();
            travel_grade.Add("A豪华");
            travel_grade.Add("B标准");
            travel_grade.Add("C经济");
            List<string> hotel_grade = new List<string>();
            hotel_grade.Add("二星级");
            hotel_grade.Add("三星级");
            hotel_grade.Add("四星级");
            hotel_grade.Add("五星级");
            hotel_grade.Add("其他");
            List<string> price_tier = new List<string>();
            price_tier.Add("0     ~ 1000元");
            price_tier.Add("1001 ~ 3000元");
            price_tier.Add("3001 ~ 5000元");
            price_tier.Add("5001 ~ 10000元");
            price_tier.Add("10000元以上");
            List<string> travel_type = new List<string>();
            travel_type.Add("广告线路");
            travel_type.Add("特价线路");
            travel_type.Add("国庆特惠");
            travel_type.Add("中秋特惠");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow Rs=dt.Rows[i];
                MODEL.TravelProduct model = new MODEL.TravelProduct();
                string sNum = Rs["suppliers_nb"].ToString();
                MODEL.Supplier sModel = DB.I_Supplier.GetSingleModelBy(s => s.SupplierNumber ==sNum);
                if (sModel!=null)
                {
                    model.SupplierId = sModel.SupplierId;//关联供应商号
                    model.LineName = Rs["travel_name"].ToString();
                    model.TravelInformation = Rs["travel_rim"].ToString();//行程信息
                    model.TravelDays = Rs["days"].ToString().ToInt();//行程天数
                    model.AdvanceDays = Rs["advance_day"].ToString().ToInt();//提前报名天数
                    model.LastGroupDate = Rs["end_date"].ToString().ToDate();//结束日期
                    model.AddDate = Rs["addtime"].ToString().ToDate();
                   
                    model.LineNumber=Rs["plan_nb"].ToString();//计划号
                    if (DB.I_TravelProduct.GetSingleModelBy(s=>s.LineNumber==model.LineNumber)==null)
                    {
                        //model.AddPeople = sModel.SupplierName;
                        model.LineFeatures = Rs["feature"].ToString();//特色
                        model.TrafficType = trafficType[Rs["traffic_type"].ToString().ToInt()];//交通
                        model.LineLevel = travel_grade[Rs["travel_grade"].ToString().ToInt()];//线路等 级
                        model.HotelRating = hotel_grade[Rs["hotel_grade"].ToString().ToInt()];//星级
                        model.PriceRange = price_tier[Rs["price_tier"].ToString().ToInt()];
                        model.LineType = travel_type[Rs["travel_type"].ToString().ToInt()];//类型
                        model.SuitablepPeople = Rs["crowd"].ToString();//适宜人群

                        model.MealStandards1 = Rs["eat_type"].ToString();
                        model.MealStandards2 = Rs["eat_type_zao"].ToString();

                        model.ChildrenFare = Rs["child_markup"].ToString().Todbl();//儿童加价
                        model.AdultFare = Rs["adult_markup"].ToString().Todbl();//成人加价
                        model.TouristSpots = Rs["tourist_attraction"].ToString();//景点
                        model.TravelProductStatus = Rs["tourist_attraction"].ToString() == "2" ? true : false;//已发布
                        model.Direction = "门市";// Rs["send_for"].ToString();//发布方向
                        model.DepartureCity = "北京";//Rs["start_city"].ToString();//
                        model.ObjectiveCity = Rs["province"].ToString();//目标城市
                        
                        DB.I_TravelProduct.Add(model);
                    }
                   
                }
            }
            MessageBox.Show("TravelProduct导入成功！"); 
        }

        private void Frm_Main_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// 门市
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            ConnectData mydata = GetConnect.GetConn();
            DataTable dt = mydata.Fill("select * from hdsy_retail order by id asc").Tables[0];//导入门市
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow Rs = dt.Rows[i];
                MODEL.RetailSales model = new MODEL.RetailSales();
                model.AmountAccounts = Rs["account"].ToString().Todbl();//门市账户金
                model.AccountAmount = Rs["invoice_account"].ToString().Todbl();//发示账户
                model.CostExpense = Rs["cost_account"].ToString().Todbl();//成本账户
                model.StoreNumber = Rs["ms_nb"].ToString();//门市编号
                if (DB.I_RetailSales.GetSingleModelBy(s=>s.StoreNumber==model.StoreNumber)==null)
                {
                    model.StoreName = Rs["ms_name"].ToString();//门市名
                    model.FullName = Rs["ms_name"].ToString();//门市名
                    model.Province = Rs["province"].ToString();//省
                    model.City = Rs["city"].ToString();//市
                    model.County = Rs["city"].ToString();//县
                    model.Address = Rs["address"].ToString();//地址                    
                   
                    model.State = Rs["status"].ToString()=="1"? "启用":"停用";//状态
             
                    DataRow contactRs = mydata.GetRs("select * from hdsy_contact where con_nb='" + model.StoreNumber + "' order by id asc");//查联系人
                    if (contactRs!=null)
                    {
                        model.Contacts = contactRs["contact"].ToString();//联系人
                        model.MobilePhone = contactRs["mobile"].ToString();//手机                        
                    }
                    
                    DataRow bankRow = mydata.GetRs("select * from hdsy_bank where con_nb='" + model.StoreNumber + "'");
                    if (bankRow != null)
                    {
                        model.BankName =bankRow["bank_name"].ToString()+ bankRow["bank_branch"].ToString();//开户行
                        //model.FullName = bankRow["bank_name"].ToString();//银行
                        model.BankAccount = bankRow["bank_card"].ToString().Replace(" ", "").Replace("　", "");//卡号
                    }
                    model.AddUser = Rs["addname"].ToString();//添加人
                    model.RetailSalesStatus = true;
                    if(DB.I_RetailSales.Add(model))
                    {
                        DataTable contactTable = mydata.Fill("select * from hdsy_contact where con_nb='" + model.StoreNumber + "' order by id asc").Tables[0];
                        for (int j = 0; j < contactTable.Rows.Count; j++)
                        {
                            DataRow subRs = contactTable.Rows[j];
                            if (j == 0)
                            {
                                model.Contacts = subRs["contact"].ToString();//联系人
                                model.MobilePhone = subRs["mobile"].ToString();//手机      
                            }
                            MODEL.BusinessMan manModel = new MODEL.BusinessMan();
                            manModel.FullName = subRs["contact"].ToString();//业务员
                            
                           
                            manModel.Gender=subRs["sex"].ToString();
                            manModel.Role = subRs["zhiwu"].ToString();//职务
                            manModel.MobilePhone = subRs["mobile"].ToString();
                            manModel.StoreNumber = model.StoreNumber;
                            manModel.Email = subRs["email"].ToString();
                            manModel.Telphone = subRs["phone"].ToString();
                            manModel.QQ = subRs["qq"].ToString();
                            manModel.UseState = subRs["main_statu"].ToString() == "1" ? "启用" : "停用";//状态 model.State;//启用？
                            manModel.BusinessManStatus = true;

                            manModel.Email = manModel.Email.Length == 0 ? manModel.QQ + "@qq.com" : manModel.Email;

                            manModel.UserName = model.StoreNumber +"-"+ j;//门市编号+　Ｊ
                            manModel.Pwd = Common.Tool.Md5("lsjr"+manModel.UserName, manModel.UserName); //Tool.Md5(model.StoreNumber + manModel.FullName, model.StoreNumber.ToString());//密码
                            manModel.PayPwd = Tool.Md5(model.StoreNumber, manModel.UserName.ToString());//支付密码


                            DB.I_BusinessMan.Add(manModel);
                        }
                    }
                }
               
            }
            MessageBox.Show("RetailSales导入完毕！"); 
        }

        /// <summary>
        /// 团期导入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
              ConnectData mydata = GetConnect.GetConn();
              DataTable dt = mydata.Fill("select * from hdsy_travel order by id asc").Tables[0];
            for (int i = 23703; i < dt.Rows.Count; i++)
            {
                DB = new BLL.IBLL();
                DataRow Rs = dt.Rows[i];
                MODEL.GroupStage model = new MODEL.GroupStage();//团期
                model.GroupNumber = Rs["travel_nb"].ToString();//团期编号
                if (DB.I_GroupStage.GetSingleModelBy(s=>s.GroupNumber==model.GroupNumber)==null)
                {
                    string PlanNum=Rs["plan_nb"].ToString();//线路号
                    MODEL.TravelProduct LineModel = DB.I_TravelProduct.GetSingleModelBy(s => s.LineNumber == PlanNum);
                    if (LineModel!=null)
                    {
                        model.LineId = LineModel.TravelProductId;
                    }
                    model.OutDate = Rs["travel_date"].ToString().ToDate();//发团日期
                    model.ComeDate = Rs["return_date"].ToString().ToDate();//回团
                    model.EndRegistration = Rs["close_date"].ToString().ToDate();//最后注册
                    model.ChildrenPrice = Rs["trade_child_price"].ToString().Todbl();//儿童结算价
                    model.ChildrenTakeoutPrice = Rs["child_price"].ToString().Todbl();//儿外卖价
                    model.AdultPrice = Rs["trade_adult_price"].ToString().Todbl();
                    model.AdultTakeoutPrice = Rs["adult_price"].ToString().Todbl();//成外卖价
                    model.PlanBit = Rs["plan_num"].ToString().ToInt();//计划数
                    model.ApplyBit = Rs["ask_num"].ToString().ToInt();//申请数
                    model.ConfirmationBit = Rs["true_num"].ToString().ToInt();//确认数
                    model.PublishState = Rs["status"].ToString()=="2"?"收客中":"截止";// "收客中";

                    model.GroupStageStatus = true;
                    DB.I_GroupStage.Add(model);
                    //DB = null;
                }
            }
            MessageBox.Show("GroupStage导入完毕！"); 
        }

        /// <summary>
        /// 订单表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
              ConnectData mydata = GetConnect.GetConn();
              DataTable dt = mydata.Fill("select * from hdsy_order order by id asc").Tables[0];
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow Rs = dt.Rows[i];
                MODEL.Orders model = new MODEL.Orders();//订单
                string gnum=Rs["travel_nb"].ToString();
                MODEL.GroupStage Gmodel=DB.I_GroupStage.GetSingleModelBy(s=>s.GroupNumber==gnum);
                if (Gmodel!=null)
	            {
                    model.GroupStageId = Gmodel.GroupStageId;//团ID
                    model.LineId = Gmodel.LineId;//线路ID
                    if (Gmodel.TravelProduct!=null)
                    {
                        model.SupplierId = Gmodel.TravelProduct.SupplierId; //供应商ID                       
                    }
                    #region 添加用游客
                    string order_nb = Rs["order_nb"].ToString();
                    DataTable vDt = mydata.Fill("select * from hdsy_order_client where order_nb='" + order_nb + "' order by id asc").Tables[0];
                    for (int j = 0; j < vDt.Rows.Count; j++)
                    {
                        if (j == 0)
                        {
                            MODEL.Visitor Vmodel = new MODEL.Visitor();//游客
                            Vmodel.FullName = vDt.Rows[j]["contact"].ToString();
                            Vmodel.Sex = vDt.Rows[j]["sex"].ToString();
                            Vmodel.IdType = "身份证";
                            Vmodel.IdNumber = vDt.Rows[j]["id_card"].ToString();
                            Vmodel.MobilePhone = vDt.Rows[j]["mobile"].ToString();
                            Vmodel.Passport = vDt.Rows[j]["client_rim"].ToString();
                            Vmodel.Telphone = vDt.Rows[j]["phone"].ToString();
                            DB.I_Visitor.Add(Vmodel);
                            model.VisitorId = Vmodel.VisitorId;
                        }
                        MODEL.CommonContact Cmodel = new MODEL.CommonContact();
                        Cmodel.VisitorId = model.VisitorId;
                        Cmodel.FullName = vDt.Rows[j]["contact"].ToString();
                        Cmodel.Sex = vDt.Rows[j]["sex"].ToString();
                        Cmodel.IdType = "身份证";
                        Cmodel.IdNumber = vDt.Rows[j]["id_card"].ToString();
                        Cmodel.MobilePhone = vDt.Rows[j]["mobile"].ToString();
                        Cmodel.Passport = vDt.Rows[j]["client_rim"].ToString();
                        if (vDt.Rows[j]["adult_child"].ToString() == "1")
                        {
                            Cmodel.IsChildren = true;
                        }
                        else
                        {
                            Cmodel.IsChildren = false;
                        }
                        DB.I_CommonContact.Add(Cmodel);
                    }
                } 
                    #endregion
                model.AdultNum = Rs["adult_num"].ToString().ToInt();//成人数
                model.ChildrenNum = Rs["child_num"].ToString().ToInt();//儿童数
                model.PayAmount = Rs["price"].ToString().Todbl();//交易金额
                model.TradePrice = Rs["trade_price"].ToString().Todbl();//成交金额
                model.PriceUpdate = Rs["price_update"].ToString().Todbl();//更新价
                model.OrderDate = Rs["addtime"].ToString().ToDate();//下单日期
                model.OrderNumber = Rs["order_nb"].ToString();//订单号
                model.OrderType = Rs["order_type"].ToString();//类型

                model.OrderState = Convert.ToInt16(Rs["status"].ToString());//状态
                model.OrderRemark = Rs["order_rim"].ToString();//备注


                string storeN=Rs["client_nb"].ToString();
                MODEL.RetailSales Rmodel=DB.I_RetailSales.GetSingleModelBy(s=>s.StoreNumber==storeN);
                if (Rmodel!=null)
	            {
                    model.StoreNumber = storeN;//门市号
	            }
                if (DB.I_Orders.Add(model))
                {                   
                    var cResult = DB.I_CommonContact.GetListBy(s => s.VisitorId == model.VisitorId).ToList();
                    foreach (var item in cResult)
                    {
                        MODEL.OrderToCommonContact Omodel = new MODEL.OrderToCommonContact();
                        Omodel.OrdersId = model.OrdersId;
                        Omodel.CommonContactId = 1;
                        DB.I_OrderToCommonContact.Add(Omodel);
                    }                    
                }
            }
            MessageBox.Show("Orders导入完毕！"); 
        }

        /// <summary>
        /// payment导入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            
            ConnectData mydata = GetConnect.GetConn();
            DataTable table = mydata.Fill("select * from hdsy_pay order by id asc").Tables[0];//支付
            foreach (DataRow row in table.Rows)
            {
                MODEL.Payment item=new MODEL.Payment ();
                //item.PaymentId= Convert.ToInt32(row["Id"]);
                item.PayType = row["pay_type"].ToString();//支付类型号
                item.PayObject = row["client"].ToString();//客户
                item.LinkMan = row["linkman"].ToString();//联系人
                item.Mobile = row["mobile"].ToString();//电话

                item.PayAmount = Convert.ToDecimal(row["amt"]);//总额
                item.PaymentRemark = row["rim"].ToString();//备注
                item.PayInfo = row["info"].ToString();//支付信息
                item.TabUser = row["addname"].ToString();//制单人
                item.PayTime = Convert.ToDateTime(row["addtime"]);//添加时间

                item.PayDept = row["section"].ToString();//门市号+名
                item.PayCategory = row["pay_method"].ToString();//支付方式

                item.Status = row["status"].ToString();//状态
                item.StoreNumber = row["client_nb"].ToString();//门市编号/供应商号
                item.PayNumber = row["pay_nb"].ToString();//?支付流水号
                item.PayDate = Convert.ToDateTime(row["pay_date"]);//支付日期
                item.VoucherNo = row["voucher_nb"].ToString();//?收据

                item.AmountCapital = row["amt_cn"].ToString();//大写
                item.PassCheck = row["passname"].ToString();//通过人（审核）
                //item.PayObject = row["pay_name"].ToString();//支付名->对象,上面有

                item.CommissionIncomeAmt =Convert.ToDecimal(row["fyamt"].ToString());//返佣费
                item.OrderAmt = Convert.ToDecimal(row["orderamt"].ToString());//订单总额

                item.Currency = row["pay_currency"].ToString();//币种

                DB.I_Payment.Add(item);
               
            }
            MessageBox.Show("payment导入完毕！");

        }

       

        private void button7_Click_1(object sender, EventArgs e)
        {
            ConnectData mydata = GetConnect.GetConn();
            DataTable table = mydata.Fill("select * from hdsy_contact order by id asc").Tables[0];//XXX
            for (int i = 1; i < table.Rows.Count; i++)
            {
                DataRow row=table.Rows[i];
                MODEL.Contract contract = new MODEL.Contract();
                contract.ReturnNumber = 1;
                contract.ApplyNumber = 1;

                string flowNo = (100000001 + i).ToString();
                flowNo = flowNo.Substring(1, flowNo.Length - 1);

                contract.ReturnContractNo = "CN" + flowNo;
                contract.ApplyDate = Convert.ToDateTime(row["addtime"]);
                contract.YesNo = Convert.ToInt16(row["main_statu"].ToString()) == 1 ? true : false;
                contract.StoreNumber = row["con_nb"].ToString();
                contract.Remark = row["rim"].ToString();

                DB.I_Contract.Add(contract);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ConnectData mydata = GetConnect.GetConn();
            DataTable table = mydata.Fill("select b.Id,b.days,b.advance_day,a.close_date,a.return_date,"+
                "a.trade_adult_price,a.adult_price,a.`status`,b.plan_nb from hdsy_travel a left JOIN hdsy_plan"+
                " b on a.plan_nb=b.plan_nb where b.Id is not NULL").Tables[0];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                MODEL.Travel model = new MODEL.Travel();

                //model.LineId = Convert.ToInt32(row["id"]);//猜测
                model.NumberOfDays = Convert.ToInt32(row["advance_day"]);
                model.Total = Convert.ToInt32(row["days"]);
                model.Remainder = null;
                model.CollegeDate = Convert.ToDateTime(row["close_date"]);
                model.TradePrice = Convert.ToDecimal(row["trade_adult_price"]);
                model.Remainder = Convert.ToInt32(row["adult_price"]);
                model.LineNumber = row["plan_nb"].ToString();//猜测

                string flowNo = (100000001 + i).ToString();
                flowNo = flowNo.Substring(1, flowNo.Length - 1);
                model.TravelNo = flowNo;

                DB.I_Travel.Add(model);
            }
        }

        /// <summary>
        /// 员工
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button9_Click(object sender, EventArgs e)
        {
            ConnectData mydata = GetConnect.GetConn();
            DataTable table = mydata.Fill("select * from hdsy_sysuser").Tables[0];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row=table.Rows[i];

                MODEL.Employee model = new MODEL.Employee();
                model.StaffBirthday = null;

                string flowNo = (100000001 + i).ToString();
                flowNo = "Em"+flowNo.Substring(1, flowNo.Length - 1);
                model.StaffNumber = flowNo;

                model.FullName = row["username"].ToString();
                model.Sex = null;
                model.IdNumber = null;
                model.Photo = null;
                model.Nation = null;
                model.MaritalStatus = null;
                model.Mark = null;
                model.MobilePhone = row["mobile"].ToString();
                model.Telphone = row["phone"].ToString();
                model.Country = null;
                model.County = null;
                model.Province = null;
                model.City = null;
                model.Address = null;
                model.ZipCode = null;
                model.Email = null;
                model.CulturalDegree = null;
                model.AccountProp = null;
                model.WorkExperience = null;
                model.EducationExperience = null;
                model.SocialSecurity = null;

                DB.I_Employee.Add(model);
            }

            MessageBox.Show("Employee导入完毕！");
        }

        /// <summary>
        /// 收款
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button10_Click(object sender, EventArgs e)
        {
            ConnectData mydata = GetConnect.GetConn();
            DataTable table = mydata.Fill("select * from hdsy_retail_downline_pay order by id asc").Tables[0];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row=table.Rows[i];
                MODEL.Receivables model = new MODEL.Receivables();

                model.BorrowedDate = Convert.ToDateTime(row["addtime"]);//入款时间
                model.BorrowedAmount = Convert.ToDecimal(row["amt"]);//总额
                model.StoreNumber = row["ms_nb"].ToString();//门市号
                model.AmountCapital = row["amt_cn"].ToString();//大写

                model.PaymentInformation = row["suppliers"].ToString();//付款单位信息
                //model.ReceiptsRemark = row["travel_name"].ToString();//


                //string flowNo = (100000001 + i).ToString();
                //flowNo = "RB" + flowNo.Substring(1, flowNo.Length - 1);
                model.ReceiptsNumber = row["pay_no"].ToString();//流水号

                model.Company = row["ms_name"].ToString();
                model.ReceiptType = row["pay_method"].ToString();
                model.Currency = row["pay_currency"].ToString();
                
                model.SettlementAudit = null;
                model.State = row["status"].ToString();
                model.DeptManager = null;
                model.FinancialAudit = null;
                model.FinancialManager = null;
                model.MoneyType = row["pay_type"].ToString();
                model.DocumentNumber = null;
                model.PayeeInformation = row["bank_user"].ToString();
                model.GeneralManager = null;
                model.ReceivablesStatus = null;

                DB.I_Receivables.Add(model);
            }
            MessageBox.Show("Receivables导入完毕！");
        }

        private void button11_Click(object sender, EventArgs e)
        {
            
            ConnectData mydata = GetConnect.GetConn();
            DataTable table = mydata.Fill("select * from hdsy_retail_cost").Tables[0];//业务成本

            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row=table.Rows[i];
                MODEL.OtherPayment model = new MODEL.OtherPayment();//其它付款
                model.AddDate = Convert.ToDateTime(row["addtime"]);

                 string flowNo = (100000001 + i).ToString();
                flowNo = "OP" + flowNo.Substring(1, flowNo.Length - 1);

                model.SerialNumber = flowNo;
                model.SpendingDate = Convert.ToDateTime(row["pay_date"]);
                model.ApplyObject = row["rcost_name"].ToString();
                model.DepartmentName = row["section"].ToString();
                model.PayType = row["rcost_type"].ToString();
                model.SpendingAmount = Convert.ToDecimal(row["amt"]);
                model.AmountCapital = row["amt_cn"].ToString();
                model.PayInfo = row["pay_info"].ToString();
                model.State = row["status"].ToString();

                DB.I_OtherPayment.Add(model);
            }
            MessageBox.Show("OtherPayment导入完毕！");
        }
        //以下不知道导入哪个表，就把他的值取出来，放在这里啦。
        private void button12_Click(object sender, EventArgs e)
        {
            //ConnectData mydata = GetConnect.GetConn();
            //DataTable table = mydata.Fill("select * from hdsy_account order by id").Tables[0];
            
            //for (int i = 0; i < table.Rows.Count; i++)
            //{
            //    //流水账
            //    MODEL.TransactionFlow model = new MODEL.TransactionFlow();
                
            //    DataRow row = table.Rows[i];
            //    //model.TransactionFlowId = Convert.ToInt32(row["Id"]);
            //    model.SerialNumber = row["acc_nb"].ToString();//流水号
            //    model.CreateDate = Convert.ToDateTime(row["addtime"]);
            //    model.PayInfo = row["info"].ToString();//支付信息

            //    decimal amt = Convert.ToDecimal(row["amt"]);//收支
            //    if (amt < 0)
            //        model.Spending = amt;
            //    else
            //        model.Revenue = amt;
            //    model.AccountBalance = Convert.ToDecimal(row["now_amt"]);//余额
            //    model.Department = row["acc_name"].ToString();//部门编号+名称形式
            //    model.AccountType = row["acc_type"].ToString();//账户类型-号
            //    model.ClientType = row["client_type"].ToString();//客户类型-号 M,S
            //    model.Attn = row["addname"].ToString();//经办人
            //    model.Object = row["obj_name"].ToString();//对象
            //    model.StoreNumber = row["con_nb"].ToString();//门市编号

            //    DB.I_TransactionFlow.Add(model);
            //}
            //MessageBox.Show("流水账导入完毕！");
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button13_Click(object sender, EventArgs e)
        {
            ConnectData mydata = GetConnect.GetConn();
            DataTable table = mydata.Fill("select * from hdsy_bank").Tables[0];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                int id = Convert.ToInt32(row["Id"]);
                string bank_name = row["bank_name"].ToString();
                string bank_branch = row["bank_branch"].ToString();
                string bank_user = row["bank_user"].ToString();
                string bank_card = row["bank_card"].ToString();
                string con_nb = row["con_nb"].ToString();
                string addname = row["addname"].ToString();
                DateTime addtime = Convert.ToDateTime(row["addtime"]);

            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            ConnectData mydata = GetConnect.GetConn();
            DataTable table = mydata.Fill("select * from hdsy_city").Tables[0];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                int city_id = Convert.ToInt32(row["city_id"]);
                string city = row["city"].ToString();
                int pro_id = Convert.ToInt32(row["pro_id"]);
                int orders = Convert.ToInt32(row["orders"]);
                int state = Convert.ToInt32(row["state"]);
                int quhao = Convert.ToInt32(row["quhao"]);

            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            ConnectData mydata = GetConnect.GetConn();
            DataTable table = mydata.Fill("select * from hdsy_client_type").Tables[0];

            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                int Id = Convert.ToInt32(row["Id"]);
                string type_name = row["type_name"].ToString();
                string type_rim = row["type_rim"].ToString();
                int parent_id = Convert.ToInt32(row["parent_id"]);

            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            ConnectData mydata = GetConnect.GetConn();
            DataTable table = mydata.Fill("select * from hdsy_do_message").Tables[0];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                int Id = Convert.ToInt32(row["Id"]);
                string title = row["title"].ToString();
                string do_key = row["do_key"].ToString();
                string username = row["username"].ToString();
                DateTime do_date = Convert.ToDateTime(row["do_date"]);
                string do_no = row["do_no"].ToString();
                string to_time = row["do_time"].ToString();

            }
        }

        /// <summary>
        /// 发票导入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button17_Click(object sender, EventArgs e)
        {
            ConnectData mydata = GetConnect.GetConn();
            DataTable table = mydata.Fill("select * from hdsy_fapiao order by id asc").Tables[0];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                MODEL.Invoice model = new MODEL.Invoice();//发票
                DataRow row = table.Rows[i];
                //int Id = Convert.ToInt32(row["Id"]);
                model.TravelNo=  row["travel_nb"].ToString();//线路号
                
               model.AddDate= Convert.ToDateTime(row["add_date"]);//添加时间
                model.ApplyPerson = row["addname"].ToString();//申请人
               model.ApplyDepartment = row["section"].ToString();//开票单位
                model.StoreNumber = row["usercode"].ToString();//门市号
                model.InvoiceProperty = row["xingzhi"].ToString();//开票性质
                model.InvoiceText = row["neirong"].ToString();//开票内容
                model.TotalAmount = Convert.ToDecimal(row["price"]);//开票价格
                model.InvoiceTitle = row["taitou"].ToString();//抬头
                model.LineName = row["travel_name"].ToString();//线路名称
                model.CheckPerson = row["pass"].ToString();//审核人1
                model.CheckPerson +=","+ row["pass2"].ToString();//审核人2
                model.CheckPerson += "," + row["pass3"].ToString();//审核人3
                model.CheckPerson += "," + row["pass4"].ToString();//审核人4
                model.CheckPerson = model.CheckPerson.Replace(",,", ",").Replace(",,", ",").Replace(",,", ",");
                model.SettlementRemark = row["beizhu"].ToString();//备注
                model.Address = row["r_address"].ToString();//快 递 地 址
                model.Recipient = row["r_name"].ToString();//收件人姓名
                model.MobilePhone = row["r_phone"].ToString();//收件人手机
                model.Progress = Convert.ToInt32(row["statu"].ToString());//状态
                model.AmountCapital= row["price_c"].ToString();//大写
                model.FinancialRemark = row["fenpi"].ToString();//分批cwbeizhu
                model.People = row["renshu"].ToString();//人数
                model.ContractNo = row["hetong"].ToString();//合同号
                model.TravelAgencyName = row["dijieshe"].ToString();//地接社名

                model.InvoiceType = row["type"].ToString();//类型leixing
                model.InvoiceDate = Convert.ToDateTime(row["kp_date"]);//开票时间

                DB.I_Invoice.Add(model);
            }
            MessageBox.Show("发票导入完毕！");
        }

        private void button18_Click(object sender, EventArgs e)
        {
            ConnectData mydata = GetConnect.GetConn();
            DataTable table = mydata.Fill("select * from hdsy_order_clear").Tables[0];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                int Id = Convert.ToInt32(row["id"]);
                int price = Convert.ToInt32(row["price"]);
                int trade_price = Convert.ToInt32(row["trade_price"]);
                int trade_price_update = Convert.ToInt32(row["trade_price_update"]);
                int price_update = Convert.ToInt32(row["price_update"]);
                string order_nb = row["order_nb"].ToString();
                string travel_nb = row["travel_nb"].ToString();
                DateTime addtime = Convert.ToDateTime(row["addtime"]);
                int received = Convert.ToInt32(row["received"]);
                int status = Convert.ToInt32(row["status"]);
                int deposit = Convert.ToInt32(row["deposit"]);
                decimal fyprice = Convert.ToDecimal(row["fyprice"]);
                int return_status = Convert.ToInt32(row["return_status"]);
                string pay_nb = row["pay_nb"].ToString();
            }
        }

        private void button19_Click(object sender, EventArgs e)
        {
            ConnectData mydata = GetConnect.GetConn();
            DataTable table = mydata.Fill("select * from hdsy_order_client").Tables[0];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                int Id = Convert.ToInt32(row["Id"]);
                string phone = row["phone"].ToString();
                string mobile = row["mobile"].ToString();
                string sex = row["sex"].ToString();
                string contact = row["contact"].ToString();
                string order_nb = row["order_nb"].ToString();
                int main_statu = Convert.ToInt32(row["main_statu"]);
                string id_card = row["id_card"].ToString();
                string travel_nb = row["travel_nb"].ToString();
                int adult_child = Convert.ToInt32(row["adult_child"]);
                string client_rim = row["client_rim"].ToString();

            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            ConnectData mydata = GetConnect.GetConn();
            DataTable table = mydata.Fill("select * from hdsy_order_price_update").Tables[0];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                int Id = Convert.ToInt32(row["Id"]);
                string pup_nb = row["pup_nb"].ToString();
                string travel_nb = row["travel_nb"].ToString();
                string order_nb = row["order_nb"].ToString();
                int price_update = Convert.ToInt32(row["price_update"]);
                DateTime addtime = Convert.ToDateTime(row["addtime"]);
                string addname = row["addname"].ToString();
                int status = Convert.ToInt32(row["status"]);
                int pup_type = Convert.ToInt32(row["pup_type"]);
                string pup_rim = row["pup_rim"].ToString();
                int now_price = Convert.ToInt32(row["now_price"]);
                string section = row["section"].ToString();
                string suppilers_nb = row["suppiers_nb"].ToString();
                string ms_nb = row["ms_nb"].ToString();

            }
        }

        private void button21_Click(object sender, EventArgs e)
        {
            ConnectData mydata = GetConnect.GetConn();
            DataTable table = mydata.Fill("select * from hdsy_province").Tables[0];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                int pro_id = Convert.ToInt32(row["pro_id"]);
                string province = row["province"].ToString();
                int state = Convert.ToInt32(row["state"]);
                int orders = Convert.ToInt32(row["orders"]);
                int child_city = Convert.ToInt32(row["child_city"]);
                string quhoa = row["quhao"].ToString();
                int pro_statu = Convert.ToInt32(row["pro_statu"]);

            }
        }

        /// <summary>
        /// 入款
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button22_Click(object sender, EventArgs e)
        {
            ConnectData mydata = GetConnect.GetConn();
            DataTable table = mydata.Fill("select * from hdsy_receipt").Tables[0];

            for (int i = 0; i < table.Rows.Count; i++)
            {
                MODEL.Receivables model = new MODEL.Receivables();//收款

                DataRow row = table.Rows[i];
                model.Currency = row["money_type"].ToString();//类型
                model.PaymentInformation = row["client"].ToString();//入款单位（客户）
                model.LinkMan = row["linkman"].ToString();//联系人
                model.Moblie = row["mobile"].ToString();//手机
                model.BorrowedAmount = Convert.ToInt32(row["amt"]);//总金 额
                model.ReceiptsRemark = row["rim"].ToString();//入款信息备注
                model.PayInfo = row["info"].ToString();//入款信息

                model.TabUser = row["addname"].ToString();//入款人
                model.ApplyDate = Convert.ToDateTime(row["addtime"]);//申请时间

                model.FromDept = row["section"].ToString();//入款门市、部门

                model.ReceiptType = row["receipt_method"].ToString();//方式
                model.State = row["status"].ToString();//状态
                model.StoreNumber = row["client_nb"].ToString();//门市编号

                model.PayFor = row["payforco"].ToString();//接收
                model.KeyNo1 = row["key_nb"].ToString();
                model.KeyNo2 = row["key_nb2"].ToString();

                model.BorrowedDate = Convert.ToDateTime(row["receipt_date"]);//收款时间
                model.VoucherNo = row["voucher_nb"].ToString();//收据号
                model.AmountCapital = row["amt_cn"].ToString();//大写
                model.SettlementAudit = row["passname"].ToString();//审核 人
                model.SupplierNo = row["suppliers_nb"].ToString();//供应商号
                model.Remark = row["beizhu"].ToString();//备注
                model.Company = row["payforco"].ToString();//付款给
                model.FinancialAudit = row["cwpassname"].ToString();//财务审核人
                model.PrintTimes = Convert.ToInt32(row["print"]);//打印次数

                DB.I_Receivables.Add(model);
            }
            MessageBox.Show("Receivables导入完毕！"); 
        }

        private void button23_Click(object sender, EventArgs e)
        {
            
            ConnectData mydata = GetConnect.GetConn();
            DataTable table = mydata.Fill("select * from hdsy_resource").Tables[0];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                int Id = Convert.ToInt32(row["Id"]);
                string rs_nb = row["rs_nb"].ToString();
                string rs_name = row["rs_name"].ToString();
                int country = Convert.ToInt32(row["country"]);
                string province = row["province"].ToString();
                string city = row["city"].ToString();
                string addname = row["addname"].ToString();
                DateTime adddate = Convert.ToDateTime(row["addtime"]);
                string section = row["section"].ToString();
                int sign_statu = Convert.ToInt32(row["sign_statu"]);
                string address = row["address"].ToString();
                int status = Convert.ToInt32(row["status"]);
                string rs_type = row["rs_type"].ToString();
                string rs_rim = row["rs_rim"].ToString();

            }
        }

        private void button24_Click(object sender, EventArgs e)
        {
            ConnectData mydata = GetConnect.GetConn();
            DataTable table = mydata.Fill("select * from hdsy_section").Tables[0];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                int Id = Convert.ToInt32(row["Id"]);
                string section = row["section"].ToString();
                int account = Convert.ToInt32(row["account"]);
                string section_nb = row["section_nb"].ToString();
                int staff_num = Convert.ToInt32(row["staff_num"]);

            }
        }
        /// <summary>
        /// hdsy_suppliers_back表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button25_Click(object sender, EventArgs e)
        {
            ConnectData mydata = GetConnect.GetConn();
            DataTable table = mydata.Fill("select * from hdsy_suppliers_back").Tables[0];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                int Id = Convert.ToInt32(row["Id"]);
                string suppliers_nb = row["suppliers_nb"].ToString();
                string back_nb = row["back_nb"].ToString();
                string ms_nb = row["ms_nb"].ToString();
                int back_money = Convert.ToInt32(row["back_money"]);
                string order_nb = row["order_nb"].ToString();
                string travel_nb = row["travel_nb"].ToString();
                string back_money_cn = row["back_money_cn"].ToString();
                string remark = row["remark"].ToString();
                DateTime addtime = Convert.ToDateTime(row["addtime"]);
                int status = Convert.ToInt32(row["status"]);

            }
        }

        /// <summary>
        /// 导入hdsy_suppliers_pay//供应商流水
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button26_Click(object sender, EventArgs e)
        {
            //ConnectData mydata = GetConnect.GetConn();
            //DataTable table = mydata.Fill("select * from hdsy_suppliers_pay order by id").Tables[0];//供应商付款
            //foreach (DataRow row in table.Rows)
            //{
            //    MODEL.Payment item = new MODEL.Payment();

            //    item.PayType = row["pay_type"].ToString();//支付类型号
            //    //item.PayObject = row["client"].ToString();//客户
            //   // item.LinkMan = row["linkman"].ToString();//联系人X
            //    //item.Mobile = row["mobile"].ToString();//电话X
            //    item.PayAmount = Convert.ToDecimal(row["amt"]);//总额
            //    item.PaymentRemark = row["rim"].ToString();//备注
            //    item.PayInfo = row["info"].ToString();//支付信息
            //    item.TabUser = row["addname"].ToString();//制单人
            //    item.PayTime = Convert.ToDateTime(row["addtime"]);//添加时间
            //    item.PayDept = row["section"].ToString();//门市号+名
            //    item.PayCategory = row["pay_method"].ToString();//支付方式
            //    item.Status = row["status"].ToString();//状态
            //    item.SuppliersNo = row["suppliers_nb"].ToString();//门市编号/供应商号
            //    item.PayNumber = row["pay_nb"].ToString();//?支付流水号
            //    try
            //    {
            //        item.PayDate = Convert.ToDateTime(row["pay_date"]);//支付日期
            //    }
            //    catch { }
            //    //item.VoucherNo = row["voucher_nb"].ToString();//?收据
            //    //item.AmountCapital = row["amt_cn"].ToString();//大写
            //    item.PassCheck = row["passname"].ToString();//通过人（审核）
            //    item.PayObject = row["pay_name"].ToString();//支付名->对象,上面有
            //    item.CommissionIncomeAmt = Convert.ToDecimal(row["fy_amt"].ToString());//返佣费
            //    item.OrderAmt = Convert.ToDecimal(row["order_amt"].ToString());//订单总额
            //    //item.Currency = row["pay_currency"].ToString();//币种

               
            //    item.OrderNos = row["order_nbs"].ToString();//

            //    DB.I_Payment.Add(item);

            //}
            
            ////for (int i = 0; i < table.Rows.Count; i++)
            ////{
            ////    int pay_type = Convert.ToInt32(row["pay_type"]);
            ////    int child_num = Convert.ToInt32(row["child_num"]);
            ////    int adult_num = Convert.ToInt32(row["adult_num"]);
            ////    int order_num = Convert.ToInt32(row["order_num"]);
            ////    int amt = Convert.ToInt32(row["amt"]);
            ////    string rim = row["rim"].ToString();
            ////    string info = row["info"].ToString();
            ////    string addname = row["addname"].ToString();
            ////    DateTime addtime = Convert.ToDateTime(row["addtime"]);
            ////    int section = Convert.ToInt32(row["section"]);
            ////    string pay_method = row["pay_method"].ToString();
            ////    int status = Convert.ToInt32(row["status"]);
            ////    string pay_nb = row["pay_nb"].ToString();
            ////    DateTime pay_date = Convert.ToDateTime(row["pay_date"]);
            ////    string passname = row["passname"].ToString();
            ////    string suppliers_nb = row["suppliers_nb"].ToString();
            ////    int order_amt = Convert.ToInt32(row["order_amt"]);
            ////    int fy_amt = Convert.ToInt32(row["fy_amt"]);
            ////    string travel_nbs = row["travel_nbs"].ToString();
            ////    string order_nbs = row["order_nbs"].ToString();
            ////    int ids = Convert.ToInt32(row["ids"]);
            ////    DateTime passtime = Convert.ToDateTime(row["passtime"]);
            ////    int fyamt = Convert.ToInt32(row["fyamt"]);
            ////}
            //for (int i = 0; i < table.Rows.Count; i++)
            //{
            //    //流水账
            //    MODEL.TransactionFlow model = new MODEL.TransactionFlow();

            //    DataRow row = table.Rows[i];

            //    model.SerialNumber = row["pay_nb"].ToString();//流水号
            //    model.CreateDate = Convert.ToDateTime(row["addtime"]);
            //    model.PayInfo = row["info"].ToString();//支付信息

            //    decimal amt = Convert.ToDecimal(row["amt"]);//收支
            //    if (amt < 0)
            //        model.Spending = amt;
            //    else
            //        model.Revenue = amt;
            //    model.AccountBalance = Convert.ToDecimal(row["amt"]);//余额
            //    model.Department = row["section"].ToString();//部门编号+名称形式
            //    model.AccountType = row["pay_type"].ToString();//账户类型-号
            //    model.ClientType = "S";// row["client_type"].ToString();//客户类型-号 M,S
            //    model.Attn = row["addname"].ToString();//经办人
            //    model.Object = row["pay_name"].ToString();//对象
            //    model.StoreNumber = row["suppliers_nb"].ToString();//门市编号
            //}
           
            //MessageBox.Show("Pay导入完毕！");
        }

        private void button27_Click(object sender, EventArgs e)
        {
            ConnectData mydata = GetConnect.GetConn();
            DataTable table = mydata.Fill("select * from hdsy_contact").Tables[0];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                //MODEL.Contract model = new MODEL.Contract();//联系人??
                DataRow row = table.Rows[i];



                int Id = Convert.ToInt32(row["Id"]);
                string usercode = row["usercode"].ToString();
                int menu_id =Convert.ToInt32(row["menu_id"].ToString());
                string new_name = row["new_name"].ToString();
                DateTime adddate = Convert.ToDateTime(row["adddate"]);
                string addname = row["addname"].ToString();

            }
        }

        private void button28_Click(object sender, EventArgs e)
        {
            ConnectData mydata = GetConnect.GetConn();
            DataTable table = mydata.Fill("select * from hdsy_travel_clear").Tables[0];
            for (int i = 0; i < table.Rows.Count; i++)
            {
                DataRow row = table.Rows[i];
                int Id = Convert.ToInt32(row["Id"]);
                string travel_nb = row["travel_nb"].ToString();
                int adult_rum = Convert.ToInt32(row["adult_num"]);
                int child_num = Convert.ToInt32(row["child_num"]);
                int price = Convert.ToInt32(row["price"]);
                int trade_price = Convert.ToInt32(row["trade_price"]);
                int order_num = Convert.ToInt32(row["order_num"]);
                int trade_price_update = Convert.ToInt32(row["trade_price_update"]);
                int price_update = Convert.ToInt32(row["price_update"]);
                int received = Convert.ToInt32(row["received"]);
                int pay_price = Convert.ToInt32(row["pay_price"]);
                int haspay_price = Convert.ToInt32(row["haspay_price"]);
                int pay_price_update = Convert.ToInt32(row["pay_price_update"]);
                int deposit = Convert.ToInt32(row["deposit"]);
                int cost_num = Convert.ToInt32(row["cost_num"]);
                int cost_affirm = Convert.ToInt32(row["cost_affirm"]);
                int passcost_num = Convert.ToInt32(row["passcost_num"]);
                int status = Convert.ToInt32(row["status"]);

            }
        }

        //重置密码
        private void button29_Click(object sender, EventArgs e)
        {
            var list=DB.I_SupplierContact.GetAllList().ToList();
            for (int i = 0; i < list.Count;i++ )
            {
                MODEL.SupplierContact model = list[i];
                //if (model.FullName.Length > 0)
                //    model.Password = Tool.Md5(NPinyin.Pinyin.GetPinyin(model.FullName).ToLower().Replace(" ", ""), model.LoginName);//密码加密生成, lsjr+供应商号
                //else
                {                    
                    model.Password = Tool.Md5("kaleozhou", model.LoginName);
                }

                //model.Password = Tool.Md5("lsjr" + model.LoginName, model.LoginName);//密码加密生成, lsjr+供应商号
                DB.I_SupplierContact.ExtSql("update SupplierContact set Password='" + model.Password + "' where SupplierContactId=" + model.SupplierContactId);
            }
            MessageBox.Show(list.Count+"条数据更新成功！");
        }

        private void button30_Click(object sender, EventArgs e)
        {
            //manModel.UserName = model.StoreNumber + "-" + j;//门市编号+　Ｊ
            //manModel.Pwd = Common.Tool.Md5("lsjr" + manModel.UserName, manModel.UserName);
            //manModel.PayPwd = Tool.Md5(model.StoreNumber, manModel.UserName.ToString());//支付密码

            var list2 = DB.I_BusinessMan.GetAllList().ToList();
            for (int i = 0; i < list2.Count; i++)
            {
                MODEL.BusinessMan model = list2[i];
                var RetailSalesId=0;
                if (model.RetailSalesId != null)
                    RetailSalesId = model.RetailSalesId.Value;
                else
                  RetailSalesId = DB.I_RetailSales.GetSingleModelBy(s => s.StoreNumber == model.StoreNumber).RetailSalesId;

                if (model==null || model.StoreNumber == null || model.UserName == null)
                    continue;
                model.UserName = model.StoreNumber;// +NPinyin.Pinyin.GetInitials(model.FullName).Replace(" ", "");//门市编号+业务员姓名首字母大写
                //model.UserName = model.UserName.ToLower();
                //model.Pwd = Common.Tool.Md5(NPinyin.Pinyin.GetPinyin(model.FullName).ToLower().Replace(" ",""), model.UserName);//业务员姓名拼音小写
                model.Pwd = Common.Tool.Md5("kaleozhou", model.UserName);
                model.PayPwd = Tool.Md5("kaleozhou", model.UserName.ToString());//门市编号为支付密码
                DB.I_BusinessMan.ExtSql("update BusinessMan set UserName='" + model.UserName + "',Pwd='" + model.Pwd + "',PayPwd='" + model.PayPwd + "',RetailSalesId=" + RetailSalesId + " where BusinessManId=" + model.BusinessManId);
            }
            MessageBox.Show(list2.Count+"条数据更新成功！");
        }
    }
}
