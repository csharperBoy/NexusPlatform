import type { Symbol, OrderPayload } from "../models";
import { formatEasyTraderDateTime } from "./time";

export function buildOrderPayload(
  sym: Symbol,
  price: number,
  quantity: number,
): OrderPayload {
  const totalValue = Math.round(price * quantity * (1 + sym.commission));
  return {
    order: {
      price,
      quantity,
      side: sym.side,
      validityType: sym.validityType,
      createDateTime: formatEasyTraderDateTime(new Date()),
      commission: sym.commission,
      symbolIsin: sym.symbolIsin,
      symbolName: sym.symbolName,
      orderModelType: sym.orderModelType,
      totalValue,
      orderFrom: sym.orderFrom,
    },
  };
}