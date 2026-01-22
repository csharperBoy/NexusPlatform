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

        //public static ElementAccessPath TodayOrdersBox = new ElementAccessPath("کادر سفارشات روز", "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[2]" ,"این المنت شامل تمام سفارشات روز داخل خودش است");
        //public static ElementAccessPath TodayOrder = new ElementAccessPath("سفارش داخل کادر سفارشات روز", "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[2]/div[1]/div/div", "این المنت شامل تمام سفارشات روز داخل خودش است" , WebScrapper.Domain.Enums.ElementPathEnum.localXpath , "xpath=./div[contains(@class,'row-list__item-content')]", "/div[" , "]/div/div");

        public static TableElementAccessPath TodayOrdersTable = new TableElementAccessPath(
            "کادر سفارشات روز",
            _FullXpath: "/html/body/app-root/main-layout/main/div[3]/d-order-list/div/div[2]",
            _Description:  "این المنت شامل تمام سفارشات روز داخل خودش است",
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
    }
}
