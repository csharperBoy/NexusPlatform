using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;
using WebScrapper.Application.DTOs;
using WebScrapper.Domain.Enums;

namespace BrokerageOperations.Domain.Property
{
    public static class EasyTraderProperties
    {
        /// <summary>
        /// آدرس صفحه اصلی
        /// </summary>
        public static string BaseUrl = "https://d.easytrader.ir/";

        #region صفحه ورود
        /// <summary>
        /// آدرس صفحه لاگین
        /// </summary>
        public static string LoginUrl = "https://login.emofid.com/";
        /// <summary>
        /// کادر نام کاربری
        /// </summary>
        public static ElementAccessPath UserNameInput = new ElementAccessPath("فیلد ورود نام کاربری", "/html/body/main/div/div[1]/form/div[1]/div[1]/input");
        /// <summary>
        /// فیلد ورود رمزعبور
        /// </summary>
        public static ElementAccessPath PasswordInput = new ElementAccessPath("فیلد ورود رمزعبور", "/html/body/main/div/div[1]/form/div[2]/div[1]/input");
        /// <summary>
        /// دکمه ورود
        /// </summary>
        public static ElementAccessPath LoginButton = new ElementAccessPath("دکمه ورود", "/html/body/main/div/div[1]/form/div[3]/button");
        #endregion

        #region جستجو و انتخاب سهم
        /// <summary>
        /// دکمه جستجو صفحه اصلی
        /// </summary>
        public static ElementAccessPath SearchButton = new ElementAccessPath("دکمه جستجو صفحه اصلی", "/html/body/app-root/main-layout/main/div[2]/div[1]/ul[1]/li[2]/span/svg-icon/svg");
        /// <summary>
        /// کادر جستجوی نماد
        /// </summary>
        public static ElementAccessPath SearchInput = new ElementAccessPath("کادر جستجوی نماد", "/html/body/app-root/main-layout/main/d-search-management/lib-search-panel/div/div[1]/lib-search-symbol-input/input");
        /// <summary>
        /// جدول نتیجه نماد های جستجو شده
        /// </summary>
        public static TableElementAccessPath SearchResultTable = new TableElementAccessPath(
            "جدول نماد های جستجو شده",
           _FullXpath: "/html/body/app-root/main-layout/main/d-search-management/lib-search-panel/div/div[2]/lib-search-flat-result-list/cdk-virtual-scroll-viewport/div[1]",
            _Code: "table",
            _ElementType: ElementTypeEnum.Table,
            _rowAccessPath: new TableRowElementAccessPath("نماد جستجو شده",
                    _Code: "rows",
                    _ElementType: ElementTypeEnum.TableRow,
                    _Description: "این المنت شامل تمام نماد های داخل جدول است",
                   _DefaultAccessPath: ElementPathEnum.Xpath,
                   _xpath: "xpath=./div[contains(@class,'list-group-item')]",
                   _localXpathPart1: "/lib-search-panel-item[", _localXpathPart2: "]",
                   _columnsAccessPath: new List<ElementAccessPath>()
                   {
                       new ElementAccessPath("عنوان سهم",
                           _Code: "StockTitle",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div/a/div[1]/div/div/div/b"
                           ),
                       new ElementAccessPath("انتخاب سهم",
                           _Code: "StockSelect",
                           _ElementType: ElementTypeEnum.Button,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div/a"
                           )
                   }
            ));
        #endregion

        #region خرید و فروش سهم
        /// <summary>
        /// دکمه خرید
        /// </summary>
        public static ElementAccessPath BuyButton = new ElementAccessPath("دکمه خرید", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[1]/div/button[1]");
        /// <summary>
        /// دکمه فروش
        /// </summary>
        public static ElementAccessPath SellButton = new ElementAccessPath("دکمه فروش", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[1]/div/button[2]");
        /// <summary>
        /// دکمه ارسال خرید
        /// </summary>
        public static ElementAccessPath SendBuyOrderButton = new ElementAccessPath("دکمه ارسال خرید", "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[2]/order-form-tabular-container/div/order-form-actions/div/button[1]");
        /// <summary>
        /// دکمه ارسال فروش
        /// </summary>
        public static ElementAccessPath SendSellOrderButton = new ElementAccessPath("دکمه ارسال فروش", "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[2]/order-form-tabular-container/div/order-form-actions/div/button[1]");
        /// <summary>
        /// فیلد ورود حجم سفارش
        /// </summary>
        public static ElementAccessPath VolumeInput = new ElementAccessPath("فیلد ورود حجم سفارش", "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[2]/order-form-tabular-container/div/div/order-form-inputs/div/form/div/div[1]/order-form-quantity-input/div/div/input-widget/div/div[1]/div/input");
        /// <summary>
        /// فیلد ورود قیمت سفارش
        /// </summary>
        public static ElementAccessPath PriceInput = new ElementAccessPath("فیلد ورود قیمت سفارش", "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[2]/order-form-tabular-container/div/div/order-form-inputs/div/form/div/div[2]/order-form-price-input/div/input-widget/div/div[1]/div/input");
        #endregion

        #region ناوبر سمت چپ صفحه شامل سفارشات روز
        /// <summary>
        /// دکمه فعال و غیرفعال شدن فیلتر بر روی سفارشات روز
        /// </summary>
        public static ElementAccessPath FilterActiveButton = new ElementAccessPath("دکمه فعال شدن فیلتر بر روی سفارشات روز",
            "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[1]/div/div[2]/svg-icon[2]/svg");
        /// <summary>
        /// فیلتر عنوان سهم در سفارشات روز
        /// </summary>
        public static ElementAccessPath StockTitleFilter = new ElementAccessPath("فیلتر عنوان سهم در سفارشات روز",
            "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[1]/div[2]/div[1]/input");
        /// <summary>
        /// فیلتر سفارشات انجام شده
        /// </summary>
        public static ElementAccessPath DoneOrderFilter = new ElementAccessPath("فیلتر سفارشات انجام شده",
            "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[1]/div[2]/div[2]/div[1]/svg-icon/svg");
        /// <summary>
        /// فیلتر سفارشات فروش
        /// </summary>
        public static ElementAccessPath SellOrderFilter = new ElementAccessPath("فیلتر سفارشات فروش",
            "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[1]/div[2]/div[2]/div[2]/svg-icon/svg");
        /// <summary>
        /// فیلتر سفارشات خرید
        /// </summary>
        public static ElementAccessPath BuyOrderFilter = new ElementAccessPath("فیلتر سفارشات خرید",
            "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[1]/div[2]/div[2]/div[3]/svg-icon/svg");
        /// <summary>
        /// "فیلتر سفارشات پیش نویس
        /// </summary>
        public static ElementAccessPath DraftOrderFilter = new ElementAccessPath("فیلتر سفارشات پیش نویس",
            "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[1]/div[2]/div[2]/div[4]/svg-icon/svg");
        /// <summary>
        /// دکمه باز و بسته کردن باکس سفارشات روز
        /// </summary>
        public static ElementAccessPath OpenSidebarFilter = new ElementAccessPath("دکمه باز و بسته کردن باکس سفارشات روز",
            "/html/body/app-root/main-layout/main/div[3]/span/span/ui-chevron/svg-icon");
        /// <summary>
        /// جدول سفارشات روز که در سمت چپ ایزی تریدر هست به همراه ردیف ها و فیلد های هر ردیف
        /// </summary>
        public static TableElementAccessPath TodayOrdersTable = new TableElementAccessPath(
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
                   _xpath: "xpath=./div[contains(@class,'row-list__item-content')]",
                   _localXpathPart1: "/div[", _localXpathPart2: "]/div/div",
                   _columnsAccessPath: new List<ElementAccessPath>()
                   {
                       new ElementAccessPath("عنوان سهم",
                           _Code: "StockTitle",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[1]/div[1]/h6"
                           ),
                       new ElementAccessPath("حجم سفارش",
                           _Code: "VolumOrder",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[2]/span[1]"
                           ),
                       new ElementAccessPath("حجم انجام شده",
                           _Code: "VolumeDone",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[2]/span[2]"
                           ),
                       new ElementAccessPath("قیمت سفارش",
                           _Code: "Price",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[4]/span[1]"
                           ),
                        new ElementAccessPath("انتخاب ",
                           _Code: "Select",
                           _ElementType: ElementTypeEnum.Button,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[1]/div[1]/h6"
                           ),
                       new ElementAccessPath("دکمه حذف سفارش",
                           _Code: "delete",
                           _ElementType: ElementTypeEnum.Button,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[1]/div[2]/div/svg-icon[3]"
                           ),
                        new ElementAccessPath("دکمه ویرایش سفارش",
                           _Code: "edit",
                           _ElementType: ElementTypeEnum.Button,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[1]/div[2]/div/svg-icon[2]"
                           ),
                   }
                   )
            );


        #endregion

        #region تاریخچه سفارشات
        /// <summary>
        /// دکمه رفتن به صفحه تاریخچه سفارشات
        /// </summary>
        public static ElementAccessPath OrderHistoryNavigateButton = new ElementAccessPath("دکمه رفتن به صفحه تاریخچه سفارشات", "/html/body/app-root/main-layout/main/div[2]/div[2]/div/div/ul[1]/li[2]");
        /// <summary>
        /// کادر مربوط به فیلتر جستجوی نماد رد صفحه تاریخچه سفارشات
        /// </summary>
        public static ElementAccessPath OrderHistoryStockTitleInput = new ElementAccessPath("کادر مربوط به فیلتر جستجوی نماد", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/d-order-history-filters/div/div[2]/form/div[1]/lib-search-symbol/lib-search-symbol-input/input");

        /// <summary>
        /// کادر مربوط به فیلتر از تاریخ
        /// </summary>
        public static ElementAccessPath OrderHistoryFromDateInput = new ElementAccessPath("کادر مربوط به فیلتر از تاریخ", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/d-order-history-filters/div/div[2]/form/div[2]/custom-date-picker/div/div[1]/input");
        /// <summary>
        /// دکمه انتخاب امروز برای فیلتر از تاریخ
        /// </summary>
        public static ElementAccessPath OrderHistoryFromDateSelectTodayButton = new ElementAccessPath("دکمه انتخاب امروز برای فیلتر از تاریخ", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/d-order-history-filters/div/div[2]/form/div[2]/custom-date-picker/div/div[2]/div/persian-datepicker/div/div/div[3]/div/span");
        /// <summary>
        /// کادر مربوط به فیلتر تا تاریخ
        /// </summary>
        public static ElementAccessPath OrderHistoryToDateInput = new ElementAccessPath("کادر مربوط به فیلتر تا تاریخ", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/d-order-history-filters/div/div[2]/form/div[3]/custom-date-picker/div/div[1]/input");
        /// <summary>
        /// دکمه انتخاب امروز برای فیلتر تا تاریخ
        /// </summary>
        public static ElementAccessPath OrderHistoryToDateSelectTodayButton = new ElementAccessPath("دکمه انتخاب امروز برای فیلتر تا تاریخ", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/d-order-history-filters/div/div[2]/form/div[3]/custom-date-picker/div/div[2]/div/persian-datepicker/div/div/div[3]/div/span");
        /// <summary>
        /// دکمه اعمال فیلتر
        /// </summary>
        public static ElementAccessPath OrderHistoryFilterExecuteButton = new ElementAccessPath("دکمه اعمال فیلتر", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/d-order-history-filters/div/div[2]/form/button");
        /// <summary>
        /// جدول تاریخچه سفارشات
        /// </summary>
        public static TableElementAccessPath OrderHistoryTable = new TableElementAccessPath(
           "جدول تاریخچه سفارشات",
          _FullXpath: "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/div/div[1]/ag-grid-angular/div[3]/div[1]/div[2]/div[3]/div[1]/div[2]/div",
          _Code: "table",
           _ElementType: ElementTypeEnum.Table,
           _rowAccessPath: new TableRowElementAccessPath("ردیف سفارش",
                   _Code: "rows",
                   _ElementType: ElementTypeEnum.TableRow,
                   _Description: "این المنت شامل تمام سفارش های داخل جدول است",
                  _DefaultAccessPath: ElementPathEnum.Xpath,
                  _xpath: "xpath=./div[contains(@role=,'row')]",
                  _localXpathPart1: "/div[", _localXpathPart2: "]",
                  _columnsAccessPath: new List<ElementAccessPath>()
                  {
                       new ElementAccessPath("تاریخ",
                           _Code: "Date",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[1]/span"
                           ),
                       new ElementAccessPath("ساعت",
                           _Code: "Time",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[2]"
                           ),
                       new ElementAccessPath("سمت سفارش",
                           _Code: "Side",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[3]/app-ag-order-side-render/div/div"
                           ),
                       new ElementAccessPath("عنوان نماد",
                           _Code: "StockTitle",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[4]/span/span"
                           ),
                       new ElementAccessPath("حجم کل",
                           _Code: "OrderVolum",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[5]"
                           ),
                       new ElementAccessPath("قیمت",
                           _Code: "Price",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[6]"
                           ),
                       new ElementAccessPath("حجم انجام شده",
                           _Code: "DoneVolume",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[7]"
                           )
                  }
           ));
        /// <summary>
        /// دکمه نمایش 100 رکورد در هر صفحه
        /// </summary>
        public static ElementAccessPath RowPerPage100Button = new ElementAccessPath("دکمه 100 رکورد در هر صفحه", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/div/div[2]/div[1]/button[4]");
        /// <summary>
        /// دکمه نمایش 50 رکورد در هر صفحه
        /// </summary>
        public static ElementAccessPath RowPerPage50Button = new ElementAccessPath("دکمه 50 رکورد در هر صفحه", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/div/div[2]/div[1]/button[3]");
        /// <summary>
        /// دکمه نمایش 20 رکورد در هر صفحه
        /// </summary>
        public static ElementAccessPath RowPerPage20Button = new ElementAccessPath("دکمه 20 رکورد در هر صفحه", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/div/div[2]/div[1]/button[2]");
        /// <summary>
        /// دکمه نمایش 10 رکورد در هر صفحه
        /// </summary>
        public static ElementAccessPath RowPerPage10Button = new ElementAccessPath("دکمه 10 رکورد در هر صفحه", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/div/div[2]/div[1]/button[1]");
        /// <summary>
        /// رفتن به صفحه قبل
        /// </summary>
        public static ElementAccessPath PrevPageButton = new ElementAccessPath("رفتن به صفحه قبل", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/div/div[2]/div[2]/div/app-pagination/div/button[1]/ui-chevron/svg-icon/svg");
        /// <summary>
        /// رفتن به صفحه بعد
        /// </summary>
        public static ElementAccessPath NextPageButton = new ElementAccessPath("رفتن به صفحه بعد", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/orders-history-wrapper/div/ng-component/div/div/div[2]/div[2]/div/app-pagination/div/button[2]/ui-chevron/svg-icon/svg");
        #endregion

        #region خواندن اطلاعات سهم و آپشن
        /// <summary>
        /// عنوان سهم
        /// </summary>
        public static ElementAccessPath StockTitle = new ElementAccessPath("عنوان سهم", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[1]/lib-symbol-header/div[1]/div/div[2]/app-symbol/a/div/div/div");
        /// <summary>
        /// قیمت آخرین معامله
        /// </summary>
        public static ElementAccessPath LastPrice = new ElementAccessPath("قیمت آخرین معامله", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[1]/lib-symbol-header/div[1]/div/div[2]/div/span");
        /// <summary>
        /// قیمت پایانی
        /// </summary>
        public static ElementAccessPath ClosePrice = new ElementAccessPath("قیمت پایانی", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[1]/lib-symbol-header/div[2]/div/symbol-header-price/div/div[1]/span[3]");
        /// <summary>
        /// حجم کل معاملات
        /// </summary>
        public static ElementAccessPath TotalVolum = new ElementAccessPath("حجم کل معاملات", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[1]/lib-symbol-header/div[2]/div/symbol-header-price/div/div[2]/span[2]");
        /// <summary>
        /// قیمت بازگشایی
        /// </summary>
        public static ElementAccessPath ZeroPrice = new ElementAccessPath("قیمت بازگشایی", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[1]/div[1]/lib-market-depth/div/symbol-detail-candle/div/div/div[1]/div[2]/text()");
        /// <summary>
        /// nav ابطال
        /// </summary>
        public static ElementAccessPath NavPrice = new ElementAccessPath("nav ابطال", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[1]/lib-symbol-header/div[2]/symbol-header-nav/div/div[1]/span[2]");
        /// <summary>
        /// جدول پنج مظنه برتر
        /// </summary>
        public static TableElementAccessPath TopFiveOrderTable = new TableElementAccessPath(
             "جدول پنج مظنه برتر",
            _FullXpath: "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[1]/div[1]/lib-market-depth/div/market-depth-best-limit/div/div/table/tbody",

            _Code: "table",
             _ElementType: ElementTypeEnum.Table,
             _rowAccessPath: new TableRowElementAccessPath("ردیف سفارشات",
                     _Code: "rows",
                     _ElementType: ElementTypeEnum.TableRow,
                     _Description: "این المنت شامل تمام سفارش های داخل جدول است",
                    _DefaultAccessPath: ElementPathEnum.Xpath,
                    _xpath: "xpath=./tr",
                    _localXpathPart1: "/tr[", _localXpathPart2: "]",
                    _columnsAccessPath: new List<ElementAccessPath>()
                    {
                       new ElementAccessPath("تعداد - خرید",
                           _Code: "BuyCount",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./td[1]"
                           ),
                       new ElementAccessPath("تعداد - فروش",
                           _Code: "SellCount",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./td[4]"
                           ),
                       new ElementAccessPath("حجم - خرید",
                           _Code: "BuyVolume",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./td[2]/div/div[2]/span[2]"
                           ),
                       new ElementAccessPath("حجم - فروش",
                           _Code: "SellVolume",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./td[3]/div/div[2]/span[2]"
                           ),
                       new ElementAccessPath("قیمت - خرید",
                           _Code: "BuyPrice",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./td[2]/div/div[2]/span[1]"
                           ),
                       new ElementAccessPath("قیمت - فروش",
                           _Code: "SellPrice",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./td[3]/div/div[2]/span[1]"
                           ),

                    }
             ));
        /// <summary>
        /// مجموع حجم سفارشات خرید
        /// </summary>
        public static ElementAccessPath TotalBuyOrderVolume = new ElementAccessPath("مجموع حجم سفارشات خرید", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[1]/div[1]/lib-market-depth/div/market-depth-best-limit/div/div/lib-market-depth-aggregates/div[2]/div[2]");
        /// <summary>
        /// مجموع تعداد سفارشات خرید
        /// </summary>
        public static ElementAccessPath TotalBuyOrderCount = new ElementAccessPath("مجموع تعداد سفارشات خرید", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[1]/div[1]/lib-market-depth/div/market-depth-best-limit/div/div/lib-market-depth-aggregates/div[2]/div[1]");
        /// <summary>
        /// مجموع حجم سفارشات فروش
        /// </summary>
        public static ElementAccessPath TotalSellOrderVolume = new ElementAccessPath("مجموع حجم سفارشات فروش", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[1]/div[1]/lib-market-depth/div/market-depth-best-limit/div/div/lib-market-depth-aggregates/div[2]/div[3]");
        /// <summary>
        /// مجموع تعداد سفارشات فروش
        /// </summary>
        public static ElementAccessPath TotalSellOrderCount = new ElementAccessPath("مجموع تعداد سفارشات فروش", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[1]/div[1]/lib-market-depth/div/market-depth-best-limit/div/div/lib-market-depth-aggregates/div[2]/div[4]");
        
        /// <summary>
        /// تعداد خرید انجام شده حقیقی
        /// </summary>
        public static ElementAccessPath TotalBuyTruePersonalityCount = new ElementAccessPath("تعداد خرید انجام شده حقیقی", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[3]/lib-ind-inst-chart/div[2]/div[1]/div[1]/div/span[1]");
        /// <summary>
        /// تعداد فروش انجام شده حقیقی
        /// </summary>
        public static ElementAccessPath TotalSellTruePersonalityCount = new ElementAccessPath("تعداد فروش انجام شده حقیقی", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[3]/lib-ind-inst-chart/div[2]/div[1]/div[3]/div/span[1]");
        /// <summary>
        /// حجم خرید انجام شده حقیقی
        /// </summary>
        public static ElementAccessPath TotalBuyTruePersonalityVolume = new ElementAccessPath("حجم خرید انجام شده حقیقی", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[3]/lib-ind-inst-chart/div[2]/div[1]/div[1]/div/span[2]/text()");
        /// <summary>
        /// حجم فروش انجام شده حقیقی
        /// </summary>
        public static ElementAccessPath TotalSellTruePersonalityVolume = new ElementAccessPath("حجم فروش انجام شده حقیقی", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[3]/lib-ind-inst-chart/div[2]/div[1]/div[3]/div/span[2]/text()");
        /// <summary>
        /// تعداد خرید انجام شده حقوقی
        /// </summary>
        public static ElementAccessPath TotalBuyLegalPersonalityCount = new ElementAccessPath("تعداد خرید انجام شده حقوقی", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[3]/lib-ind-inst-chart/div[2]/div[2]/div[1]/div[1]/span[1]");
        /// <summary>
        /// تعداد فروش انجام شده حقوقی
        /// </summary>
        public static ElementAccessPath TotalSellLegalPersonalityCount = new ElementAccessPath("تعداد فروش انجام شده حقوقی", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[3]/lib-ind-inst-chart/div[2]/div[2]/div[3]/div[1]/span[1]");
        /// <summary>
        /// حجم خرید انجام شده حقوقی
        /// </summary>
        public static ElementAccessPath TotalBuyLegalPersonalityVolume = new ElementAccessPath("حجم خرید انجام شده حقوقی", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[3]/lib-ind-inst-chart/div[2]/div[2]/div[1]/div[1]/span[2]/text()");
        /// <summary>
        /// حجم فروش انجام شده حقوقی
        /// </summary>
        public static ElementAccessPath TotalSellLegalPersonalityVolume = new ElementAccessPath("حجم فروش انجام شده حقوقی", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[3]/lib-ind-inst-chart/div[2]/div[2]/div[3]/div[1]/span[2]/text()");
        
        


        #region اطلاعات قرارداد - برای آپشن ها
        /// <summary>
        /// ارزش معاملات
        /// </summary>
        public static ElementAccessPath ContractTradesValue = new ElementAccessPath("ارزش معاملات", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[4]/symbol-contract-info/div/div/div/div[3]/div[2]/span[2]");
        /// <summary>
        /// موقعیت های باز
        /// </summary>
        public static ElementAccessPath ContractOpenPosition = new ElementAccessPath("موقعیت های باز", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[4]/symbol-contract-info/div/div/div/div[4]/div[1]/span[2]");
        /// <summary>
        /// موقعیت های باز هم گروه
        /// </summary>
        public static ElementAccessPath ContractOpenPositionGroup = new ElementAccessPath("موقعیت های باز هم گروه", "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[2]/div[2]/div[2]/lib-symbol-information-container/div/div/div[4]/symbol-contract-info/div/div/div/div[6]/div[1]/span[2]");

        #endregion
        #endregion

        #region خواندن دیده بان
        /// <summary>
        /// لیست دیده بان ها
        /// </summary>
        public static TableElementAccessPath MarketWatchHeaderTable = new TableElementAccessPath(
           "لیست دیده بان ها",
          _FullXpath: "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[1]/div/as-split/as-split-area/d-portfolio-watch-container/d-market-watch-route-gateway/d-market-watch-new-container/div/market-watch-header/div/div[1]/div/div",

          _Code: "table",
           _ElementType: ElementTypeEnum.Table,
           _rowAccessPath: new TableRowElementAccessPath("ردیف سفارش",
                   _Code: "rows",
                   _ElementType: ElementTypeEnum.TableRow,
                   _Description: "این المنت شامل تمام سفارش های داخل جدول است",
                  _DefaultAccessPath: ElementPathEnum.Xpath,
                  _xpath: "xpath=./div[contains(@class=,'tab')]",
                  _localXpathPart1: "/div[", _localXpathPart2: "]",
                  _columnsAccessPath: new List<ElementAccessPath>()
                  {
                       new ElementAccessPath("عنوان",
                           _Code: "Title",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./span"
                           ),
                      new ElementAccessPath("انتخاب دیده بان",
                           _Code: "SelectWatch",
                           _ElementType: ElementTypeEnum.Button,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./span"
                           )
                  }
           ));
        /// <summary>
        /// جدول نمادهای داخل دیده بان - قسمت سمت راست که فقط شامل عنوان است
        /// </summary>
        public static TableElementAccessPath MarketWatchTableRightPart = new TableElementAccessPath(
           "لیست نماد های داخل دیده بان",
          _FullXpath: "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[1]/div/as-split/as-split-area/d-portfolio-watch-container/d-market-watch-route-gateway/d-market-watch-new-container/div/div/market-watch-symbols/div/ag-grid-angular/div[3]/div[1]/div[2]/div[3]/div[1]/div[3]",
                     _Code: "table",
           _ElementType: ElementTypeEnum.Table,
           _rowAccessPath: new TableRowElementAccessPath("ردیف نمادها",
                   _Code: "rows",
                   _ElementType: ElementTypeEnum.TableRow,
                   _Description: "این المنت شامل تمام عناوین نماد های داخل جدول است",
                  _DefaultAccessPath: ElementPathEnum.Xpath,
                  _xpath: "xpath=./div[contains(@role=,'row')]",
                  _localXpathPart1: "/div[", _localXpathPart2: "]",
                  _columnsAccessPath: new List<ElementAccessPath>()
                  {
                       new ElementAccessPath("عنوان",
                           _Code: "Title",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div/div/span/symbol-name-renderer/symbol-state-plus-alarm-and-note/app-symbol/a/div/div/div"
                           ),
                      new ElementAccessPath("انتخاب",
                           _Code: "Select",
                           _ElementType: ElementTypeEnum.Button,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div/div/span/symbol-name-renderer/symbol-state-plus-alarm-and-note/app-symbol/a/div/div/div"
                           )
                  }
           ));
        /// <summary>
        /// جدول نمادهای داخل دیده بان - قسمت سمت چپ که  شامل مقادیر است
        /// </summary>
        public static TableElementAccessPath MarketWatchTableLeftPart = new TableElementAccessPath(
           "لیست نماد های داخل دیده بان",
           _FullXpath: "/html/body/app-root/main-layout/main/div[3]/div/div/as-split/as-split-area/app-layout-selector/app-layout2/as-split/as-split-area[1]/div/as-split/as-split-area/d-portfolio-watch-container/d-market-watch-route-gateway/d-market-watch-new-container/div/div/market-watch-symbols/div/ag-grid-angular/div[3]/div[1]/div[2]/div[3]/div[1]/div[2]",
           _Code: "table",
           _ElementType: ElementTypeEnum.Table,
           _rowAccessPath: new TableRowElementAccessPath("ردیف نمادها",
                   _Code: "rows",
                   _ElementType: ElementTypeEnum.TableRow,
                   _Description: "این المنت شامل تمام مقادیر نماد های داخل جدول است",
                  _DefaultAccessPath: ElementPathEnum.Xpath,
                  _xpath: "xpath=./div[contains(@role=,'row')]",
                  _localXpathPart1: "/div/div[", _localXpathPart2: "]",
                  _columnsAccessPath: new List<ElementAccessPath>()
                  {
                       new ElementAccessPath("حجم معاملات",
                           _Code: "TotalTradeVolume",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[1]/bold-blink-formatted-value-renderer/div/div"
                           ),
                      new ElementAccessPath("ارزش معاملات",
                           _Code: "TotalTradeValue",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[2]/bold-blink-formatted-value-renderer/div/div"
                           ),
                      new ElementAccessPath("آخرین قیمت",
                           _Code: "LastPrice",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[3]/value-percent-renderer/div/div/div[2]"
                           ),
                      new ElementAccessPath("قیمت پایانی",
                           _Code: "ClosePrice",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[4]/value-percent-renderer/div/div/div[2]"
                           ),
                      new ElementAccessPath("حجم تقاضا",
                           _Code: "TopBuyOrderVolume",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[6]/bold-blink-formatted-value-renderer/div/div"
                           ),
                      new ElementAccessPath("قیمت تقاضا",
                           _Code: "TopBuyOrderPrice",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[7]/bold-blink-formatted-value-renderer/div/div"
                           ),
                      new ElementAccessPath("حجم عرضه",
                           _Code: "TopSellOrderVolume",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[8]/bold-blink-formatted-value-renderer/div/div"
                           ),
                      new ElementAccessPath("قیمت عرضه",
                           _Code: "TopSellOrderPrice",
                           _ElementType: ElementTypeEnum.TableColumn,
                           _DefaultAccessPath: ElementPathEnum.Xpath ,
                           _xpath: "xpath=./div[9]/bold-blink-formatted-value-renderer/div/div"
                           )
                  }
           ));

        #endregion
    }
}
