using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using WebScrapper.Application.DTOs;
using WebScrapper.Domain.Enums;

namespace BrokerageOperations.Domain.Property
{
    public class EasyTraderProperties
    {
        /// <summary>
        /// آدرس صفحه اصلی
        /// </summary>
        public string BaseUrl { get; set; } = "https://d.easytrader.ir/";

        #region صفحه ورود
        /// <summary>
        /// آدرس صفحه لاگین
        /// </summary>
        public string LoginUrl { get; set; } = "https://login.emofid.com/";
        /// <summary>
        /// کادر نام کاربری
        /// </summary>
        public ElementAccessPath UserNameInput { get; set; } = new ElementAccessPath("فیلد ورود نام کاربری", "/html/body/main/div/div[1]/form/div[1]/div[1]/input");
        /// <summary>
        /// فیلد ورود رمزعبور
        /// </summary>
        public ElementAccessPath PasswordInput { get; set; } = new ElementAccessPath("فیلد ورود رمزعبور", "/html/body/main/div/div[1]/form/div[2]/div[1]/input");
        /// <summary>
        /// دکمه ورود
        /// </summary>
        public ElementAccessPath LoginButton { get; set; } = new ElementAccessPath("دکمه ورود", "/html/body/main/div/div[1]/form/div[3]/button");
        #endregion

        #region جستجو و انتخاب سهم
        /// <summary>
        /// دکمه جستجو صفحه اصلی
        /// </summary>
        public ElementAccessPath SearchButton { get; set; } = new ElementAccessPath("دکمه جستجو صفحه اصلی", "/html/body/app-root/main-layout/main/div[2]/div[1]/ul[1]/li[2]/span/svg-icon/svg");
        /// <summary>
        /// کادر جستجوی نماد
        /// </summary>
        public ElementAccessPath SearchInput { get; set; } = new ElementAccessPath("کادر جستجوی نماد", "/html/body/app-root/main-layout/main/d-search-management/lib-search-panel/div/div[1]/lib-search-symbol-input/input");
        /// <summary>
        /// جدول نتیجه نماد های جستجو شده
        /// </summary>
        public TableElementAccessPath SearchResultTable { get; set; } = new TableElementAccessPath(
            "جدول نماد های جستجو شده",
           _FullXpath: "/html/body/app-root/main-layout/main/d-search-management/lib-search-panel/div/div[2]/lib-search-flat-result-list/cdk-virtual-scroll-viewport/div[1]",
            _Code: "table",
            _ElementType: ElementTypeEnum.Table,
            _rowAccessPath: new TableRowElementAccessPath("نماد جستجو شده",
                    _Code: "rows",
                    _ElementType: ElementTypeEnum.TableRow,
                    _Description: "این المنت شامل تمام نماد های داخل جدول است",
                   _DefaultAccessPath: ElementPathEnum.Xpath,
                   _xpath: "xpath{ get; set; } =./div[contains(@class,'list-group-item')]",
                   _localXpathPart1: "/lib-search-panel-item[", _localXpathPart2: "]",
                   _columnsAccessPath: new List<ElementAccessPath>()
                   {
                       new ElementAccessPath("عنوان سهم",
                           _Code: "StockTitle",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div/a/div[1]/div/div/div/b"
                           ),
                       new ElementAccessPath("انتخاب سهم",
                           _Code: "StockSelect",
                           _ElementType: ElementTypeEnum.Button,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div/a"
                           )
                   }
            ));
        #endregion

        #region خرید و فروش سهم
        /// <summary>
        /// دکمه خرید
        /// </summary>
        public ElementAccessPath BuyButton { get; set; } = new ElementAccessPath("دکمه خرید", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[1]/div/button[1]");
        /// <summary>
        /// دکمه فروش
        /// </summary>
        public ElementAccessPath SellButton { get; set; } = new ElementAccessPath("دکمه فروش", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[1]/div/button[2]");
        /// <summary>
        /// دکمه ارسال خرید
        /// </summary>
        public ElementAccessPath SendBuyOrderButton { get; set; } = new ElementAccessPath("دکمه ارسال خرید", "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[2]/order-form-tabular-container/div/order-form-actions/div/button[1]");
        /// <summary>
        /// دکمه ارسال فروش
        /// </summary>
        public ElementAccessPath SendSellOrderButton { get; set; } = new ElementAccessPath("دکمه ارسال فروش", "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[2]/order-form-tabular-container/div/order-form-actions/div/button[1]");
        /// <summary>
        /// فیلد ورود حجم سفارش
        /// </summary>
        public ElementAccessPath VolumeInput { get; set; } = new ElementAccessPath("فیلد ورود حجم سفارش", "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[2]/order-form-tabular-container/div/div/order-form-inputs/div/form/div/div[1]/order-form-quantity-input/div/div/input-widget/div/div[1]/div/input");
        /// <summary>
        /// فیلد ورود قیمت سفارش
        /// </summary>
        public ElementAccessPath PriceInput { get; set; } = new ElementAccessPath("فیلد ورود قیمت سفارش", "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[2]/order-form-tabular-container/div/div/order-form-inputs/div/form/div/div[2]/order-form-price-input/div/input-widget/div/div[1]/div/input");
        #endregion

        #region ناوبر سمت چپ صفحه شامل سفارشات روز
        /// <summary>
        /// دکمه فعال و غیرفعال شدن فیلتر بر روی سفارشات روز
        /// </summary>
        public ElementAccessPath FilterActiveButton { get; set; } = new ElementAccessPath("دکمه فعال شدن فیلتر بر روی سفارشات روز",
            "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[1]/div/div[2]/svg-icon[2]/svg");
        /// <summary>
        /// فیلتر عنوان سهم در سفارشات روز
        /// </summary>
        public ElementAccessPath StockTitleFilter { get; set; } = new ElementAccessPath("فیلتر عنوان سهم در سفارشات روز",
            "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[1]/div[2]/div[1]/input");
        /// <summary>
        /// فیلتر سفارشات انجام شده
        /// </summary>
        public ElementAccessPath DoneOrderFilter { get; set; } = new ElementAccessPath("فیلتر سفارشات انجام شده",
            "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[1]/div[2]/div[2]/div[1]/svg-icon/svg");
        /// <summary>
        /// فیلتر سفارشات فروش
        /// </summary>
        public ElementAccessPath SellOrderFilter { get; set; } = new ElementAccessPath("فیلتر سفارشات فروش",
            "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[1]/div[2]/div[2]/div[2]/svg-icon/svg");
        /// <summary>
        /// فیلتر سفارشات خرید
        /// </summary>
        public ElementAccessPath BuyOrderFilter { get; set; } = new ElementAccessPath("فیلتر سفارشات خرید",
            "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[1]/div[2]/div[2]/div[3]/svg-icon/svg");
        /// <summary>
        /// "فیلتر سفارشات پیش نویس
        /// </summary>
        public ElementAccessPath DraftOrderFilter { get; set; } = new ElementAccessPath("فیلتر سفارشات پیش نویس",
            "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[1]/div[2]/div[2]/div[4]/svg-icon/svg");
        /// <summary>
        /// دکمه باز و بسته کردن باکس سفارشات روز
        /// </summary>
        public ElementAccessPath OpenSidebarFilter { get; set; } = new ElementAccessPath("دکمه باز و بسته کردن باکس سفارشات روز",
            "/html/body/app-root/main-layout/main/div[3]/span/span/ui-chevron/svg-icon");
        /// <summary>
        /// جدول سفارشات روز که در سمت چپ ایزی تریدر هست به همراه ردیف ها و فیلد های هر ردیف
        /// </summary>
        public TableElementAccessPath TodayOrdersTable { get; set; } = new TableElementAccessPath(
            "کادر سفارشات روز",
            _FullXpath: "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[2]",
            _Description: "این المنت شامل تمام سفارشات روز داخل خودش است",
            _Code: "table",
            _ElementType: ElementTypeEnum.Table,
            _rowAccessPath: new TableRowElementAccessPath("سفارش داخل کادر سفارشات روز",
                    _Code: "rows",
                    _ElementType: ElementTypeEnum.TableRow,
                    _Description: "این المنت شامل تمام سفارشات روز داخل خودش است",
                   _DefaultAccessPath: ElementPathEnum.Xpath,
                   _xpath: "xpath{ get; set; } =./div[contains(@class,'row-list__item-content')]",
                   _localXpathPart1: "/div[", _localXpathPart2: "]/div/div",
                   _columnsAccessPath: new List<ElementAccessPath>()
                   {
                       new ElementAccessPath("عنوان سهم",
                           _Code: "StockTitle",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[1]/div[1]/h6"
                           ),
                       new ElementAccessPath("حجم سفارش",
                           _Code: "VolumOrder",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[2]/span[1]"
                           ),
                       new ElementAccessPath("حجم انجام شده",
                           _Code: "VolumeDone",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[2]/span[2]"
                           ),
                       new ElementAccessPath("قیمت سفارش",
                           _Code: "Price",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[4]/span[1]"
                           ),
                        new ElementAccessPath("انتخاب ",
                           _Code: "Select",
                           _ElementType: ElementTypeEnum.Button,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[1]/div[1]/h6"
                           ),
                       new ElementAccessPath("دکمه حذف سفارش",
                           _Code: "delete",
                           _ElementType: ElementTypeEnum.Button,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[1]/div[2]/div/svg-icon[3]"
                           ),
                        new ElementAccessPath("دکمه ویرایش سفارش",
                           _Code: "edit",
                           _ElementType: ElementTypeEnum.Button,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[1]/div[2]/div/svg-icon[2]"
                           ),
                   }
                   )
            );


        #endregion

        #region تاریخچه سفارشات

        public string OrderHistoryUrl { get; set; } = "https://d.easytrader.ir/orders-history";
        /// <summary>
        /// دکمه رفتن به صفحه تاریخچه سفارشات
        /// </summary>
        public ElementAccessPath OrderHistoryNavigateButton { get; set; } = new ElementAccessPath("دکمه رفتن به صفحه تاریخچه سفارشات", "/html/body/app-root/main-layout/main/div[2]/div[2]/div/div/ul[1]/li[2]");
        /// <summary>
        /// کادر مربوط به فیلتر جستجوی نماد رد صفحه تاریخچه سفارشات
        /// </summary>
        public ElementAccessPath OrderHistoryStockTitleInput { get; set; } = new ElementAccessPath("کادر مربوط به فیلتر جستجوی نماد", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/d-order-history-filters/div/div[2]/form/div[1]/lib-search-symbol/lib-search-symbol-input/input");

        /// <summary>
        /// کادر مربوط به فیلتر از تاریخ
        /// </summary>
        public ElementAccessPath OrderHistoryFromDateInput { get; set; } = new ElementAccessPath("کادر مربوط به فیلتر از تاریخ", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/d-order-history-filters/div/div[2]/form/div[2]/custom-date-picker/div/div[1]/input");
        /// <summary>
        /// دکمه انتخاب امروز برای فیلتر از تاریخ
        /// </summary>
        public ElementAccessPath OrderHistoryFromDateSelectTodayButton { get; set; } = new ElementAccessPath("دکمه انتخاب امروز برای فیلتر از تاریخ", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/d-order-history-filters/div/div[2]/form/div[2]/custom-date-picker/div/div[2]/div/persian-datepicker/div/div/div[3]/div/span");
        /// <summary>
        /// کادر مربوط به فیلتر تا تاریخ
        /// </summary>
        public ElementAccessPath OrderHistoryToDateInput { get; set; } = new ElementAccessPath("کادر مربوط به فیلتر تا تاریخ", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/d-order-history-filters/div/div[2]/form/div[3]/custom-date-picker/div/div[1]/input");
        /// <summary>
        /// دکمه انتخاب امروز برای فیلتر تا تاریخ
        /// </summary>
        public ElementAccessPath OrderHistoryToDateSelectTodayButton { get; set; } = new ElementAccessPath("دکمه انتخاب امروز برای فیلتر تا تاریخ", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/d-order-history-filters/div/div[2]/form/div[3]/custom-date-picker/div/div[2]/div/persian-datepicker/div/div/div[3]/div/span");
        /// <summary>
        /// دکمه اعمال فیلتر
        /// </summary>
        public ElementAccessPath OrderHistoryFilterExecuteButton { get; set; } = new ElementAccessPath("دکمه اعمال فیلتر", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/d-order-history-filters/div/div[2]/form/button");
        /// <summary>
        /// جدول تاریخچه سفارشات
        /// </summary>

        public TableElementAccessPath OrderHistoryTable { get; set; } = new TableElementAccessPath(
          "جدول تاریخچه سفارشات",
         _FullXpath: "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/div/div[1]/ag-grid-angular/div[3]/div[1]/div[2]/div[3]/div[1]/div[2]/div",
         _Code: "table",
          _ElementType: ElementTypeEnum.Table,
          _rowAccessPath: new TableRowElementAccessPath("ردیف سفارش",
                  _Code: "rows",
                  _ElementType: ElementTypeEnum.TableRow,
                  _Description: "این المنت شامل تمام سفارش های داخل جدول است",
                 _DefaultAccessPath: ElementPathEnum.Xpath,
                  _xpath: "./div[contains(@role,'row') and contains(@class,'ag-row-level-0')]",
                    _localXpathPart1: "/div[", _localXpathPart2: "]",
                 _columnsAccessPath: new List<ElementAccessPath>()
                 {
                     new ElementAccessPath("باز کردن ردیف های سفارش",
                          _Code: "OpenSubOrders",
                          _ElementType: ElementTypeEnum.Button,
                          _DefaultAccessPath: ElementPathEnum.Xpath ,
                          _xpath: "xpath{ get; set; } =./div[1]/span/span[2]/span"
                          ),
                     new ElementAccessPath("تاریخ سفارش",
                           _Code: "Date",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "./div[1]//span[@class='ag-group-value']"
                         ),
                       new ElementAccessPath("ساعت",
                           _Code: "Time",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[2]"
                           ),
                       new ElementAccessPath("سمت سفارش",
                           _Code: "Side",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[3]/app-ag-order-side-render/div/div"
                           ),
                       new ElementAccessPath("عنوان نماد",
                           _Code: "StockTitle",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[4]/span/span"
                           ),
                       new ElementAccessPath("حجم کل",
                           _Code: "OrderVolum",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[5]"
                           ),
                       new ElementAccessPath("قیمت",
                           _Code: "Price",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[6]"
                           ),
                       new ElementAccessPath("حجم انجام شده",
                           _Code: "DoneVolume",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[7]"
                           )
                 }
          ));

        public TableElementAccessPath OrderHistoryDetailsTable { get; set; } = new TableElementAccessPath(
    "جدول جزئیات معاملات",
    _Code: "orderDetails",
    _ElementType: ElementTypeEnum.Table,
    // این XPath از ریشه ردیف جزئیات شروع میشه
    _DefaultAccessPath: ElementPathEnum.Xpath,
    _xpath: ".//div[contains(@class,'ag-details-grid')]//div[@role='grid']",
    _rowAccessPath: new TableRowElementAccessPath(
        "ردیف جزئیات معامله",
        _Code: "detailRows",
        _ElementType: ElementTypeEnum.TableRow,
        _DefaultAccessPath: ElementPathEnum.Xpath,
        _xpath: ".//div[@role='row' and contains(@class,'ag-row-level-0')]",
        _columnsAccessPath: new List<ElementAccessPath>()
        {
            new ElementAccessPath("تاریخ معامله",
                _Code: "TradeDate",
                _ElementType: ElementTypeEnum.TableColumn,
                _DefaultAccessPath: ElementPathEnum.Xpath,
                _xpath: "./div[1]"
            ),
            new ElementAccessPath("ساعت معامله",
                _Code: "TradeTime",
                _ElementType: ElementTypeEnum.TableColumn,
                _DefaultAccessPath: ElementPathEnum.Xpath,
                _xpath: "./div[2]"
            ),
            new ElementAccessPath("تعداد",
                _Code: "Quantity",
                _ElementType: ElementTypeEnum.TableColumn,
                _DefaultAccessPath: ElementPathEnum.Xpath,
                _xpath: "./div[3]"
            ),
            new ElementAccessPath("نرخ",
                _Code: "Price",
                _ElementType: ElementTypeEnum.TableColumn,
                _DefaultAccessPath: ElementPathEnum.Xpath,
                _xpath: "./div[4]"
            )
        }
    )
);
        /*public TableElementAccessPath OrderHistoryTable { get; set; } = new TableElementAccessPath(
           "جدول تاریخچه سفارشات",
          _FullXpath: "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/div/div[1]/ag-grid-angular/div[3]/div[1]/div[2]/div[3]/div[1]/div[2]/div",
          _Code: "table",
           _ElementType: ElementTypeEnum.Table,
           _rowAccessPath: new TableRowElementAccessPath("ردیف سفارش",
                   _Code: "rows",
                   _ElementType: ElementTypeEnum.TableRow,
                   _Description: "این المنت شامل تمام سفارش های داخل جدول است",
                  _DefaultAccessPath: ElementPathEnum.Xpath,
                  _xpath: "xpath{ get; set; } =./div[contains(@role{ get; set; } =,'row')]",
                  _localXpathPart1: "/div[", _localXpathPart2: "]",
                  _columnsAccessPath: new List<ElementAccessPath>()
                  {
                       new ElementAccessPath("تاریخ",
                           _Code: "Date",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[1]/span"
                           ),
                       new ElementAccessPath("ساعت",
                           _Code: "Time",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[2]"
                           ),
                       new ElementAccessPath("سمت سفارش",
                           _Code: "Side",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[3]/app-ag-order-side-render/div/div"
                           ),
                       new ElementAccessPath("عنوان نماد",
                           _Code: "StockTitle",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[4]/span/span"
                           ),
                       new ElementAccessPath("حجم کل",
                           _Code: "OrderVolum",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[5]"
                           ),
                       new ElementAccessPath("قیمت",
                           _Code: "Price",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[6]"
                           ),
                       new ElementAccessPath("حجم انجام شده",
                           _Code: "DoneVolume",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[7]"
                           )
                  }
           ));
       */
        /// <summary>
        /// دکمه نمایش 100 رکورد در هر صفحه
        /// </summary>
        public ElementAccessPath RowPerPage100Button { get; set; } = new ElementAccessPath("دکمه 100 رکورد در هر صفحه", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/div/div[2]/div[1]/button[4]");
        /// <summary>
        /// دکمه نمایش 50 رکورد در هر صفحه
        /// </summary>
        public ElementAccessPath RowPerPage50Button { get; set; } = new ElementAccessPath("دکمه 50 رکورد در هر صفحه", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/div/div[2]/div[1]/button[3]");
        /// <summary>
        /// دکمه نمایش 20 رکورد در هر صفحه
        /// </summary>
        public ElementAccessPath RowPerPage20Button { get; set; } = new ElementAccessPath("دکمه 20 رکورد در هر صفحه", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/div/div[2]/div[1]/button[2]");
        /// <summary>
        /// دکمه نمایش 10 رکورد در هر صفحه
        /// </summary>
        public ElementAccessPath RowPerPage10Button { get; set; } = new ElementAccessPath("دکمه 10 رکورد در هر صفحه", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/div/div[2]/div[1]/button[1]");
        /// <summary>
        /// رفتن به صفحه قبل
        /// </summary>
        public ElementAccessPath PrevPageButton { get; set; } = new ElementAccessPath("رفتن به صفحه قبل", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/div/div[2]/div[2]/div/app-pagination/div/button[1]/ui-chevron/svg-icon/svg");
        /// <summary>
        /// رفتن به صفحه بعد
        /// </summary>
        public ElementAccessPath NextPageButton { get; set; } = new ElementAccessPath("رفتن به صفحه بعد", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/div/div[2]/div[2]/div/app-pagination/div/button[2]/ui-chevron/svg-icon/svg");
        #endregion

        #region خواندن اطلاعات سهم و آپشن
        /// <summary>
        /// عنوان سهم
        /// </summary>
        public ElementAccessPath StockTitle { get; set; } = new ElementAccessPath("عنوان سهم", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[1]/lib-symbol-header/div[1]/div/div[2]/app-symbol/a/div/div/div");
        /// <summary>
        /// قیمت آخرین معامله
        /// </summary>
        public ElementAccessPath LastPrice { get; set; } = new ElementAccessPath("قیمت آخرین معامله", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[1]/lib-symbol-header/div[1]/div/div[2]/div/span");
        /// <summary>
        /// قیمت پایانی
        /// </summary>
        public ElementAccessPath ClosePrice { get; set; } = new ElementAccessPath("قیمت پایانی", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[1]/lib-symbol-header/div[2]/div/symbol-header-price/div/div[1]/span[3]");
        /// <summary>
        /// حجم کل معاملات
        /// </summary>
        public ElementAccessPath TotalVolum { get; set; } = new ElementAccessPath("حجم کل معاملات", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[1]/lib-symbol-header/div[2]/div/symbol-header-price/div/div[2]/span[2]");
        /// <summary>
        /// قیمت بازگشایی
        /// </summary>
        public ElementAccessPath ZeroPrice { get; set; } = new ElementAccessPath("قیمت بازگشایی", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[1]/div[1]/lib-market-depth/div/symbol-detail-candle/div/div/div[1]/div[2]/text()");
        /// <summary>
        /// nav ابطال
        /// </summary>
        public ElementAccessPath NavPrice { get; set; } = new ElementAccessPath("nav ابطال", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[1]/lib-symbol-header/div[2]/symbol-header-nav/div/div[1]/span[2]");
        /// <summary>
        /// جدول پنج مظنه برتر
        /// </summary>
        public TableElementAccessPath TopFiveOrderTable { get; set; } = new TableElementAccessPath(
             "جدول پنج مظنه برتر",
            _FullXpath: "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[1]/div[1]/lib-market-depth/div/market-depth-best-limit/div/div/table/tbody",

            _Code: "table",
             _ElementType: ElementTypeEnum.Table,
             _rowAccessPath: new TableRowElementAccessPath("ردیف سفارشات",
                     _Code: "rows",
                     _ElementType: ElementTypeEnum.TableRow,
                     _Description: "این المنت شامل تمام سفارش های داخل جدول است",
                    _DefaultAccessPath: ElementPathEnum.Xpath,
                    _xpath: "xpath{ get; set; } =./tr",
                    _localXpathPart1: "/tr[", _localXpathPart2: "]",
                    _columnsAccessPath: new List<ElementAccessPath>()
                    {
                       new ElementAccessPath("تعداد - خرید",
                           _Code: "BuyCount",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./td[1]"
                           ),
                       new ElementAccessPath("تعداد - فروش",
                           _Code: "SellCount",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./td[4]"
                           ),
                       new ElementAccessPath("حجم - خرید",
                           _Code: "BuyVolume",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./td[2]/div/div[2]/span[2]"
                           ),
                       new ElementAccessPath("حجم - فروش",
                           _Code: "SellVolume",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./td[3]/div/div[2]/span[2]"
                           ),
                       new ElementAccessPath("قیمت - خرید",
                           _Code: "BuyPrice",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./td[2]/div/div[2]/span[1]"
                           ),
                       new ElementAccessPath("قیمت - فروش",
                           _Code: "SellPrice",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./td[3]/div/div[2]/span[1]"
                           ),

                    }
             ));
        /// <summary>
        /// مجموع حجم سفارشات خرید
        /// </summary>
        public ElementAccessPath TotalBuyOrderVolume { get; set; } = new ElementAccessPath("مجموع حجم سفارشات خرید", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[1]/div[1]/lib-market-depth/div/market-depth-best-limit/div/div/lib-market-depth-aggregates/div[2]/div[2]");
        /// <summary>
        /// مجموع تعداد سفارشات خرید
        /// </summary>
        public ElementAccessPath TotalBuyOrderCount { get; set; } = new ElementAccessPath("مجموع تعداد سفارشات خرید", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[1]/div[1]/lib-market-depth/div/market-depth-best-limit/div/div/lib-market-depth-aggregates/div[2]/div[1]");
        /// <summary>
        /// مجموع حجم سفارشات فروش
        /// </summary>
        public ElementAccessPath TotalSellOrderVolume { get; set; } = new ElementAccessPath("مجموع حجم سفارشات فروش", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[1]/div[1]/lib-market-depth/div/market-depth-best-limit/div/div/lib-market-depth-aggregates/div[2]/div[3]");
        /// <summary>
        /// مجموع تعداد سفارشات فروش
        /// </summary>
        public ElementAccessPath TotalSellOrderCount { get; set; } = new ElementAccessPath("مجموع تعداد سفارشات فروش", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[1]/div[1]/lib-market-depth/div/market-depth-best-limit/div/div/lib-market-depth-aggregates/div[2]/div[4]");

        /// <summary>
        /// تعداد خرید انجام شده حقیقی
        /// </summary>
        public ElementAccessPath TotalBuyTruePersonalityCount { get; set; } = new ElementAccessPath("تعداد خرید انجام شده حقیقی", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[3]/lib-ind-inst-chart/div[2]/div[1]/div[1]/div/span[1]");
        /// <summary>
        /// تعداد فروش انجام شده حقیقی
        /// </summary>
        public ElementAccessPath TotalSellTruePersonalityCount { get; set; } = new ElementAccessPath("تعداد فروش انجام شده حقیقی", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[3]/lib-ind-inst-chart/div[2]/div[1]/div[3]/div/span[1]");
        /// <summary>
        /// حجم خرید انجام شده حقیقی
        /// </summary>
        public ElementAccessPath TotalBuyTruePersonalityVolume { get; set; } = new ElementAccessPath("حجم خرید انجام شده حقیقی", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[3]/lib-ind-inst-chart/div[2]/div[1]/div[1]/div/span[2]/text()");
        /// <summary>
        /// حجم فروش انجام شده حقیقی
        /// </summary>
        public ElementAccessPath TotalSellTruePersonalityVolume { get; set; } = new ElementAccessPath("حجم فروش انجام شده حقیقی", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[3]/lib-ind-inst-chart/div[2]/div[1]/div[3]/div/span[2]/text()");
        /// <summary>
        /// تعداد خرید انجام شده حقوقی
        /// </summary>
        public ElementAccessPath TotalBuyLegalPersonalityCount { get; set; } = new ElementAccessPath("تعداد خرید انجام شده حقوقی", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[3]/lib-ind-inst-chart/div[2]/div[2]/div[1]/div[1]/span[1]");
        /// <summary>
        /// تعداد فروش انجام شده حقوقی
        /// </summary>
        public ElementAccessPath TotalSellLegalPersonalityCount { get; set; } = new ElementAccessPath("تعداد فروش انجام شده حقوقی", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[3]/lib-ind-inst-chart/div[2]/div[2]/div[3]/div[1]/span[1]");
        /// <summary>
        /// حجم خرید انجام شده حقوقی
        /// </summary>
        public ElementAccessPath TotalBuyLegalPersonalityVolume { get; set; } = new ElementAccessPath("حجم خرید انجام شده حقوقی", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[3]/lib-ind-inst-chart/div[2]/div[2]/div[1]/div[1]/span[2]/text()");
        /// <summary>
        /// حجم فروش انجام شده حقوقی
        /// </summary>
        public ElementAccessPath TotalSellLegalPersonalityVolume { get; set; } = new ElementAccessPath("حجم فروش انجام شده حقوقی", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[3]/lib-ind-inst-chart/div[2]/div[2]/div[3]/div[1]/span[2]/text()");




        #region اطلاعات قرارداد - برای آپشن ها
        /// <summary>
        /// ارزش معاملات
        /// </summary>
        public ElementAccessPath ContractTradesValue { get; set; } = new ElementAccessPath("ارزش معاملات", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[4]/symbol-contract-info/div/div/div/div[3]/div[2]/span[2]");
        /// <summary>
        /// موقعیت های باز
        /// </summary>
        public ElementAccessPath ContractOpenPosition { get; set; } = new ElementAccessPath("موقعیت های باز", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[4]/symbol-contract-info/div/div/div/div[4]/div[1]/span[2]");
        /// <summary>
        /// موقعیت های باز هم گروه
        /// </summary>
        public ElementAccessPath ContractOpenPositionGroup { get; set; } = new ElementAccessPath("موقعیت های باز هم گروه", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[4]/symbol-contract-info/div/div/div/div[6]/div[1]/span[2]");

        #endregion
        #endregion

        #region خواندن دیده بان
        /// <summary>
        /// لیست دیده بان ها
        /// </summary>
        public TableElementAccessPath MarketWatchHeaderTable { get; set; } = new TableElementAccessPath(
           "لیست دیده بان ها",
          _FullXpath: "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[1]/div/as-split/as-split-area/d-portfolio-watch-container/d-market-watch-route-gateway/d-market-watch-new-container/div/market-watch-header/div/div[1]/div/div",

          _Code: "table",
           _ElementType: ElementTypeEnum.Table,
           _rowAccessPath: new TableRowElementAccessPath("ردیف سفارش",
                   _Code: "rows",
                   _ElementType: ElementTypeEnum.TableRow,
                   _Description: "این المنت شامل تمام سفارش های داخل جدول است",
                  _DefaultAccessPath: ElementPathEnum.Xpath,
                  _xpath: "xpath{ get; set; } =./div[contains(@class{ get; set; } =,'tab')]",
                  _localXpathPart1: "/div[", _localXpathPart2: "]",
                  _columnsAccessPath: new List<ElementAccessPath>()
                  {
                       new ElementAccessPath("عنوان",
                           _Code: "Title",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./span"
                           ),
                      new ElementAccessPath("انتخاب دیده بان",
                           _Code: "SelectWatch",
                           _ElementType: ElementTypeEnum.Button,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./span"
                           )
                  }
           ));
        /// <summary>
        /// جدول نمادهای داخل دیده بان - قسمت سمت راست که فقط شامل عنوان است
        /// </summary>
        public TableElementAccessPath MarketWatchTableRightPart { get; set; } = new TableElementAccessPath(
           "لیست نماد های داخل دیده بان",
          _FullXpath: "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[1]/div/as-split/as-split-area/d-portfolio-watch-container/d-market-watch-route-gateway/d-market-watch-new-container/div/div/market-watch-symbols/div/ag-grid-angular/div[3]/div[1]/div[2]/div[3]/div[1]/div[3]",
                     _Code: "table",
           _ElementType: ElementTypeEnum.Table,
           _rowAccessPath: new TableRowElementAccessPath("ردیف نمادها",
                   _Code: "rows",
                   _ElementType: ElementTypeEnum.TableRow,
                   _Description: "این المنت شامل تمام عناوین نماد های داخل جدول است",
                  _DefaultAccessPath: ElementPathEnum.Xpath,
                  _xpath: "xpath{ get; set; } =./div[contains(@role{ get; set; } =,'row')]",
                  _localXpathPart1: "/div[", _localXpathPart2: "]",
                  _columnsAccessPath: new List<ElementAccessPath>()
                  {
                       new ElementAccessPath("عنوان",
                           _Code: "Title",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div/div/span/symbol-name-renderer/symbol-state-plus-alarm-and-note/app-symbol/a/div/div/div"
                           ),
                      new ElementAccessPath("انتخاب",
                           _Code: "Select",
                           _ElementType: ElementTypeEnum.Button,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div/div/span/symbol-name-renderer/symbol-state-plus-alarm-and-note/app-symbol/a/div/div/div"
                           )
                  }
           ));
        /// <summary>
        /// جدول نمادهای داخل دیده بان - قسمت سمت چپ که  شامل مقادیر است
        /// </summary>
        public TableElementAccessPath MarketWatchTableLeftPart { get; set; } = new TableElementAccessPath(
           "لیست نماد های داخل دیده بان",
           _FullXpath: "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[1]/div/as-split/as-split-area/d-portfolio-watch-container/d-market-watch-route-gateway/d-market-watch-new-container/div/div/market-watch-symbols/div/ag-grid-angular/div[3]/div[1]/div[2]/div[3]/div[1]/div[2]",
           _Code: "table",
           _ElementType: ElementTypeEnum.Table,
           _rowAccessPath: new TableRowElementAccessPath("ردیف نمادها",
                   _Code: "rows",
                   _ElementType: ElementTypeEnum.TableRow,
                   _Description: "این المنت شامل تمام مقادیر نماد های داخل جدول است",
                  _DefaultAccessPath: ElementPathEnum.Xpath,
                  _xpath: "xpath{ get; set; } =./div[contains(@role{ get; set; } =,'row')]",
                  _localXpathPart1: "/div/div[", _localXpathPart2: "]",
                  _columnsAccessPath: new List<ElementAccessPath>()
                  {
                       new ElementAccessPath("حجم معاملات",
                           _Code: "TotalTradeVolume",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[1]/bold-blink-formatted-value-renderer/div/div"
                           ),
                      new ElementAccessPath("ارزش معاملات",
                           _Code: "TotalTradeValue",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[2]/bold-blink-formatted-value-renderer/div/div"
                           ),
                      new ElementAccessPath("آخرین قیمت",
                           _Code: "LastPrice",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[3]/value-percent-renderer/div/div/div[2]"
                           ),
                      new ElementAccessPath("قیمت پایانی",
                           _Code: "ClosePrice",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[4]/value-percent-renderer/div/div/div[2]"
                           ),
                      new ElementAccessPath("حجم تقاضا",
                           _Code: "TopBuyOrderVolume",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[6]/bold-blink-formatted-value-renderer/div/div"
                           ),
                      new ElementAccessPath("قیمت تقاضا",
                           _Code: "TopBuyOrderPrice",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[7]/bold-blink-formatted-value-renderer/div/div"
                           ),
                      new ElementAccessPath("حجم عرضه",
                           _Code: "TopSellOrderVolume",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[8]/bold-blink-formatted-value-renderer/div/div"
                           ),
                      new ElementAccessPath("قیمت عرضه",
                           _Code: "TopSellOrderPrice",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath{ get; set; } =./div[9]/bold-blink-formatted-value-renderer/div/div"
                           )
                  }
           ));

        #endregion
    }
}
