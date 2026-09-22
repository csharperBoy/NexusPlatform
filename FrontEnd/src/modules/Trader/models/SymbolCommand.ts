export interface CreateSymbolCommand {
  symbolName: string;
  symbolIsin: string;
  price: number;
  quantity: number;
  /** 0 = خرید، 1 = فروش */
  side: number;
  validityType: number;
  commission: number;
  orderModelType: number;
  orderFrom: number;
}

export interface UpdateSymbolCommand extends CreateSymbolCommand {
  id: string;
}

export interface GetSymbolMarketInfoQuery {
  symbolIsin: string;
}