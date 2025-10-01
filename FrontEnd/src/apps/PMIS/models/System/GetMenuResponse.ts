// src/models/GetMenuResponse.ts

export interface MenuItemDto {
  /** آیا این آیتم لینک است یا اکشن دیگری */
  isLink: boolean;
  /** آدرس برای هدایت (URL) */
  url: string;
  /** کلید آیکون به صورت اسم (مانند "HiOutlineHome") */
  icon: string;
  /** متن برچسب آیتم */
  label: string;
}

export interface MenuCatalogDto {
  /** نام گروه (کاتالوگ) */
  catalog: string;
  /** لیست آیتم‌های زیرمجموعه */
  listItems: MenuItemDto[];
}

export interface GetMenuResponse {
  /** لیست گروه‌های منو */
  menuList: MenuCatalogDto[];
}
