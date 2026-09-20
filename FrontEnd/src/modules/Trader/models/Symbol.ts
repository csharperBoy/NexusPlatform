export interface Symbol {
  symbolName: string;
  symbolIsin: string;
  price: number;
  quantity: number;
  side: 0 | 1; // 0=خرید 1=فروش
  validityType: number;
  commission: number;
  orderModelType: number;
  orderFrom: number;
}
