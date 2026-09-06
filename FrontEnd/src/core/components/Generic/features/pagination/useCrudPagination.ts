

export function useCrudPagination() {
  // استفاده از سلکتور برای جلوگیری از رندرهای اضافی
  const page = useCrudStore((state) => state.page);
  const pageSize = useCrudStore((state) => state.pageSize);
  const totalCount = useCrudStore((state) => state.totalCount);
  const setPagination = useCrudStore((state) => state.setPagination);

  const totalPages = Math.ceil(totalCount / pageSize);

  const nextPage = () => {
    if (page < totalPages) setPagination(page + 1, pageSize);
  };

  const prevPage = () => {
    if (page > 1) setPagination(page - 1, pageSize);
  };

  const changePageSize = (newSize: number) => {
    setPagination(1, newSize); // با تغییر سایز، برمی‌گردیم صفحه اول
  };

  return { page, pageSize, totalCount, totalPages, nextPage, prevPage, changePageSize };
}