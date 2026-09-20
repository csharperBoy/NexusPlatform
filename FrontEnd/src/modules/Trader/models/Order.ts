/** سفارش داخل لیست UI (قبل از ارسال) */
export interface DraftOrder {
  id: string;
  isin: string;
  price: string;
  quantity: string;
  time: string;
}

/** payload نهایی که به EasyTrader فرستاده میشه */
export interface OrderPayload {
  order: {
    price: number;
    quantity: number;
    side: 0 | 1;
    validityType: number;
    createDateTime: string;
    commission: number;
    symbolIsin: string;
    symbolName: string;
    orderModelType: number;
    totalValue: number;
    orderFrom: number;
  };
}

/** پاسخ EasyTrader */
export interface OrderResponse {
  isSuccessful: boolean;
  id?: string;
  message?: string;
  omsError?: Array<{
    name: string;
    error: string;
    code: number;
  }> | null;
}